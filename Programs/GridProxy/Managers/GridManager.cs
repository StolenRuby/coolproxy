using OpenMetaverse;
using OpenMetaverse.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static GridProxy.RegionManager;

namespace GridProxy
{
    public class GridManager
    {
        private ProxyFrame Frame;



        #region Delegates

        /// <summary>The event subscribers. null if no subcribers</summary>
        private EventHandler<CoarseLocationUpdateEventArgs> m_CoarseLocationUpdate;

        /// <summary>Raises the CoarseLocationUpdate event</summary>
        /// <param name="e">A CoarseLocationUpdateEventArgs object containing the
        /// data sent by simulator</param>
        protected virtual void OnCoarseLocationUpdate(CoarseLocationUpdateEventArgs e)
        {
            EventHandler<CoarseLocationUpdateEventArgs> handler = m_CoarseLocationUpdate;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>Thread sync lock object</summary>
        private readonly object m_CoarseLocationUpdateLock = new object();

        /// <summary>Raised when the simulator sends a <see cref="CoarseLocationUpdatePacket"/> 
        /// containing the location of agents in the simulator</summary>
        public event EventHandler<CoarseLocationUpdateEventArgs> CoarseLocationUpdate
        {
            add { lock (m_CoarseLocationUpdateLock) { m_CoarseLocationUpdate += value; } }
            remove { lock (m_CoarseLocationUpdateLock) { m_CoarseLocationUpdate -= value; } }
        }


        /// <summary>The event subscribers. null if no subcribers</summary>
        private EventHandler<GridRegionEventArgs> m_GridRegion;

        /// <summary>Raises the GridRegion event</summary>
        /// <param name="e">A GridRegionEventArgs object containing the
        /// data sent by simulator</param>
        protected virtual void OnGridRegion(GridRegionEventArgs e)
        {
            EventHandler<GridRegionEventArgs> handler = m_GridRegion;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>Thread sync lock object</summary>
        private readonly object m_GridRegionLock = new object();

        /// <summary>Raised when the simulator sends a Region Data in response to 
        /// a Map request</summary>
        public event EventHandler<GridRegionEventArgs> GridRegion
        {
            add { lock (m_GridRegionLock) { m_GridRegion += value; } }
            remove { lock (m_GridRegionLock) { m_GridRegion -= value; } }
        }

        #endregion

        /// <summary>A dictionary of all the regions, indexed by region name</summary>
        internal Dictionary<string, GridRegion> Regions = new Dictionary<string, GridRegion>(StringComparer.OrdinalIgnoreCase);
        /// <summary>A dictionary of all the regions, indexed by region handle</summary>
        internal Dictionary<ulong, GridRegion> RegionsByHandle = new Dictionary<ulong, GridRegion>();

        public GridManager(ProxyFrame frame)
        {
            Frame = frame;

            Frame.Network.AddDelegate(PacketType.CoarseLocationUpdate, Direction.Incoming, onCoarseLocationUpdate);
            Frame.Network.AddDelegate(PacketType.MapBlockReply, Direction.Incoming, MapBlockReplyHandler);
        }

        private Packet onCoarseLocationUpdate(Packet packet, RegionProxy sim)
        {
            CoarseLocationUpdatePacket coarse = (CoarseLocationUpdatePacket)packet;


            // populate a dictionary from the packet, for local use
            Dictionary<UUID, Vector3> coarseEntries = new Dictionary<UUID, Vector3>();
            for (int i = 0; i < coarse.AgentData.Length; i++)
            {
                if (coarse.Location.Length > 0)
                    coarseEntries[coarse.AgentData[i].AgentID] = new Vector3((int)coarse.Location[i].X, (int)coarse.Location[i].Y, (int)coarse.Location[i].Z * 4);

                // the friend we are tracking on radar
                if (i == coarse.Index.Prey)
                    sim.preyID = coarse.AgentData[i].AgentID;
            }

            // find stale entries (people who left the sim)
            List<UUID> removedEntries = sim.avatarPositions.FindAll(delegate (UUID findID) { return !coarseEntries.ContainsKey(findID); });

            // anyone who was not listed in the previous update
            List<UUID> newEntries = new List<UUID>();

            lock (sim.avatarPositions.Dictionary)
            {
                // remove stale entries
                foreach (UUID trackedID in removedEntries)
                    sim.avatarPositions.Dictionary.Remove(trackedID);

                // add or update tracked info, and record who is new
                foreach (KeyValuePair<UUID, Vector3> entry in coarseEntries)
                {
                    if (!sim.avatarPositions.Dictionary.ContainsKey(entry.Key))
                        newEntries.Add(entry.Key);

                    sim.avatarPositions[entry.Key] = entry.Value;
                }
            }

            if (m_CoarseLocationUpdate != null)
            {
                WorkPool.QueueUserWorkItem(delegate (object o)
                { OnCoarseLocationUpdate(new CoarseLocationUpdateEventArgs(sim, newEntries, removedEntries)); });
            }


            return packet;
        }


        /// <summary>Process an incoming packet and raise the appropriate events</summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">The EventArgs object containing the packet data</param>
        protected Packet MapBlockReplyHandler(Packet packet, RegionProxy sim)
        {
            MapBlockReplyPacket map = (MapBlockReplyPacket)packet;

            foreach (MapBlockReplyPacket.DataBlock block in map.Data)
            {
                if (block.X != 0 || block.Y != 0)
                {
                    GridRegion region;

                    region.X = block.X;
                    region.Y = block.Y;
                    region.Name = Utils.BytesToString(block.Name);
                    // RegionFlags seems to always be zero here?
                    region.RegionFlags = (RegionFlags)block.RegionFlags;
                    region.WaterHeight = block.WaterHeight;
                    region.Agents = block.Agents;
                    region.Access = (SimAccess)block.Access;
                    region.MapImageID = block.MapImageID;
                    region.RegionHandle = Utils.UIntsToLong((uint)(region.X * 256), (uint)(region.Y * 256));

                    lock (Regions)
                    {
                        Regions[region.Name] = region;
                        RegionsByHandle[region.RegionHandle] = region;
                    }

                    if (m_GridRegion != null)
                    {
                        OnGridRegion(new GridRegionEventArgs(region));
                    }
                }
            }

            return packet;
        }



        /// <summary>
        /// Get grid region information using the region name, this function
        /// will block until it can find the region or gives up
        /// </summary>
        /// <param name="name">Name of sim you're looking for</param>
        /// <param name="layer">Layer that you are requesting</param>
        /// <param name="region">Will contain a GridRegion for the sim you're
        /// looking for if successful, otherwise an empty structure</param>
        /// <returns>True if the GridRegion was successfully fetched, otherwise
        /// false</returns>
        public bool GetGridRegion(string name, GridLayerType layer, out GridRegion region)
        {
            if (String.IsNullOrEmpty(name))
            {
                Logger.Log("GetGridRegion called with a null or empty region name", Helpers.LogLevel.Error);
                region = new GridRegion();
                return false;
            }

            if (Regions.ContainsKey(name))
            {
                // We already have this GridRegion structure
                region = Regions[name];
                return true;
            }
            else
            {
                AutoResetEvent regionEvent = new AutoResetEvent(false);
                EventHandler<GridRegionEventArgs> callback =
                    delegate (object sender, GridRegionEventArgs e)
                    {
                        if (e.Region.Name.ToLower() == name.ToLower())
                            regionEvent.Set();
                    };
                GridRegion += callback;

                RequestMapRegion(name, layer);
                regionEvent.WaitOne(Frame.Config.MAP_REQUEST_TIMEOUT, false);

                GridRegion -= callback;

                if (Regions.ContainsKey(name))
                {
                    // The region was found after our request
                    region = Regions[name];
                    return true;
                }
                else
                {
                    Logger.Log("Couldn't find region " + name, Helpers.LogLevel.Warning);
                    region = new GridRegion();
                    return false;
                }
            }
        }

        /// <summary>
        /// Request a map layer
        /// </summary>
        /// <param name="regionName">The name of the region</param>
        /// <param name="layer">The type of layer</param>
        public void RequestMapRegion(string regionName, GridLayerType layer)
        {
            MapNameRequestPacket request = new MapNameRequestPacket();

            request.AgentData.AgentID = Frame.Agent.AgentID;
            request.AgentData.SessionID = Frame.Agent.SessionID;
            request.AgentData.Flags = (uint)layer;
            request.AgentData.EstateID = 0; // Filled in on the sim
            request.AgentData.Godlike = false; // Filled in on the sim
            request.NameData.Name = Utils.StringToBytes(regionName);

            Frame.Network.InjectPacket(request, Direction.Outgoing);
        }
    }


    #region EventArgs classes

    public class CoarseLocationUpdateEventArgs : EventArgs
    {
        private readonly RegionProxy m_Simulator;
        private readonly List<UUID> m_NewEntries;
        private readonly List<UUID> m_RemovedEntries;

        public RegionProxy Simulator { get { return m_Simulator; } }
        public List<UUID> NewEntries { get { return m_NewEntries; } }
        public List<UUID> RemovedEntries { get { return m_RemovedEntries; } }

        public CoarseLocationUpdateEventArgs(RegionProxy simulator, List<UUID> newEntries, List<UUID> removedEntries)
        {
            this.m_Simulator = simulator;
            this.m_NewEntries = newEntries;
            this.m_RemovedEntries = removedEntries;
        }
    }

    #endregion
}
