/*
 * Copyright (c) 2006-2016, openmetaverse.co
 * All rights reserved.
 *
 * - Redistribution and use in source and binary forms, with or without 
 *   modification, are permitted provided that the following conditions are met:
 *
 * - Redistributions of source code must retain the above copyright notice, this
 *   list of conditions and the following disclaimer.
 * - Neither the name of the openmetaverse.co nor the names 
 *   of its contributors may be used to endorse or promote products derived from
 *   this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" 
 * AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE 
 * IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE 
 * ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE 
 * LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR 
 * CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF 
 * SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS 
 * INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN 
 * CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) 
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE 
 * POSSIBILITY OF SUCH DAMAGE.
 */

using System;
using System.Threading;
using System.Reflection;
using System.Collections.Generic;
using OpenMetaverse.Http;
using OpenMetaverse.Packets;
using OpenMetaverse.Interfaces;
using OpenMetaverse.StructuredData;
using OpenMetaverse.Messages.Linden;
using OpenMetaverse;
using static GridProxy.RegionManager;

namespace GridProxy
{
    #region Parcel Class

    /// <summary>
    /// Parcel of land, a portion of virtual real estate in a simulator
    /// </summary>
    public class Parcel
    {
        /// <summary>The total number of contiguous 4x4 meter blocks your agent owns within this parcel</summary>        
        public int SelfCount;
        /// <summary>The total number of contiguous 4x4 meter blocks contained in this parcel owned by a group or agent other than your own</summary>
        public int OtherCount;
        /// <summary>Deprecated, Value appears to always be 0</summary>
        public int PublicCount;
        /// <summary>Simulator-local ID of this parcel</summary>
        public int LocalID;
        /// <summary>UUID of the owner of this parcel</summary>
        public UUID OwnerID;
        /// <summary>Whether the land is deeded to a group or not</summary>
        public bool IsGroupOwned;
        /// <summary></summary>
        public uint AuctionID;
        /// <summary>Date land was claimed</summary>
        public DateTime ClaimDate;
        /// <summary>Appears to always be zero</summary>
        public int ClaimPrice;
        /// <summary>This field is no longer used</summary>
        public int RentPrice;
        /// <summary>Minimum corner of the axis-aligned bounding box for this
        /// parcel</summary>
        public Vector3 AABBMin;
        /// <summary>Maximum corner of the axis-aligned bounding box for this
        /// parcel</summary>
        public Vector3 AABBMax;
        /// <summary>Bitmap describing land layout in 4x4m squares across the 
        /// entire region</summary>
        public byte[] Bitmap;
        /// <summary>Total parcel land area</summary>
        public int Area;
        /// <summary></summary>
        public ParcelStatus Status;
        /// <summary>Maximum primitives across the entire simulator owned by the same agent or group that owns this parcel that can be used</summary>
        public int SimWideMaxPrims;
        /// <summary>Total primitives across the entire simulator calculated by combining the allowed prim counts for each parcel
        /// owned by the agent or group that owns this parcel</summary>
        public int SimWideTotalPrims;
        /// <summary>Maximum number of primitives this parcel supports</summary>
        public int MaxPrims;
        /// <summary>Total number of primitives on this parcel</summary>
        public int TotalPrims;
        /// <summary>For group-owned parcels this indicates the total number of prims deeded to the group,
        /// for parcels owned by an individual this inicates the number of prims owned by the individual</summary>
        public int OwnerPrims;
        /// <summary>Total number of primitives owned by the parcel group on 
        /// this parcel, or for parcels owned by an individual with a group set the
        /// total number of prims set to that group.</summary>
        public int GroupPrims;
        /// <summary>Total number of prims owned by other avatars that are not set to group, or not the parcel owner</summary>
        public int OtherPrims;
        /// <summary>A bonus multiplier which allows parcel prim counts to go over times this amount, this does not affect
        /// the max prims per simulator. e.g: 117 prim parcel limit x 1.5 bonus = 175 allowed</summary>
        public float ParcelPrimBonus;
        /// <summary>Autoreturn value in minutes for others' objects</summary>
        public int OtherCleanTime;
        /// <summary></summary>
        public ParcelFlags Flags;
        /// <summary>Sale price of the parcel, only useful if ForSale is set</summary>
        /// <remarks>The SalePrice will remain the same after an ownership
        /// transfer (sale), so it can be used to see the purchase price after
        /// a sale if the new owner has not changed it</remarks>
        public int SalePrice;
        /// <summary>Parcel Name</summary>
        public string Name;
        /// <summary>Parcel Description</summary>
        public string Desc;
        /// <summary>URL For Music Stream</summary>
        public string MusicURL;
        /// <summary></summary>
        public UUID GroupID;
        /// <summary>Price for a temporary pass</summary>
        public int PassPrice;
        /// <summary>How long is pass valid for</summary>
        public float PassHours;
        /// <summary></summary>
        public ParcelCategory Category;
        /// <summary>Key of authorized buyer</summary>
        public UUID AuthBuyerID;
        /// <summary>Key of parcel snapshot</summary>
        public UUID SnapshotID;
        /// <summary>The landing point location</summary>
        public Vector3 UserLocation;
        /// <summary>The landing point LookAt</summary>
        public Vector3 UserLookAt;
        /// <summary>The type of landing enforced from the <see cref="LandingType"/> enum</summary>
        public LandingType Landing;
        /// <summary></summary>
        public float Dwell;
        /// <summary></summary>
        public bool RegionDenyAnonymous;
        /// <summary></summary>
        public bool RegionPushOverride;
        /// <summary>Access list of who is whitelisted on this
        /// parcel</summary>
        public List<ParcelManager.ParcelAccessEntry> AccessWhiteList;
        /// <summary>Access list of who is blacklisted on this
        /// parcel</summary>
        public List<ParcelManager.ParcelAccessEntry> AccessBlackList;
        /// <summary>TRUE of region denies access to age unverified users</summary>
        public bool RegionDenyAgeUnverified;
        /// <summary>true to obscure (hide) media url</summary>
        public bool ObscureMedia;
        /// <summary>true to obscure (hide) music url</summary>
        public bool ObscureMusic;
        /// <summary>A struct containing media details</summary>
        public ParcelMedia Media;

        /// <summary>
        /// Displays a parcel object in string format
        /// </summary>
        /// <returns>string containing key=value pairs of a parcel object</returns>
        public override string ToString()
        {
            string result = "";
            Type parcelType = this.GetType();
            FieldInfo[] fields = parcelType.GetFields();
            foreach (FieldInfo field in fields)
            {
                result += (field.Name + " = " + field.GetValue(this) + " ");
            }
            return result;
        }
        /// <summary>
        /// Defalt constructor
        /// </summary>
        /// <param name="localID">Local ID of this parcel</param>
        public Parcel(int localID)
        {
            LocalID = localID;
            ClaimDate = Utils.Epoch;
            Bitmap = Utils.EmptyBytes;
            Name = String.Empty;
            Desc = String.Empty;
            MusicURL = String.Empty;
            AccessWhiteList = new List<ParcelManager.ParcelAccessEntry>(0);
            AccessBlackList = new List<ParcelManager.ParcelAccessEntry>(0);
            Media = new ParcelMedia();
        }

        /// <summary>
        /// Update the simulator with any local changes to this Parcel object
        /// </summary>
        /// <param name="simulator">Simulator to send updates to</param>
        /// <param name="wantReply">Whether we want the simulator to confirm
        /// the update with a reply packet or not</param>
        public void Update(RegionProxy simulator, bool wantReply)
        {
            if (simulator.Caps.TryGetValue("ParcelPropertiesUpdate", out string cap))
            {
                Uri url = new Uri(cap);

                ParcelPropertiesUpdateMessage req = new ParcelPropertiesUpdateMessage();
                req.AuthBuyerID = this.AuthBuyerID;
                req.Category = this.Category;
                req.Desc = this.Desc;
                req.GroupID = this.GroupID;
                req.Landing = this.Landing;
                req.LocalID = this.LocalID;
                req.MediaAutoScale = this.Media.MediaAutoScale;
                req.MediaDesc = this.Media.MediaDesc;
                req.MediaHeight = this.Media.MediaHeight;
                req.MediaID = this.Media.MediaID;
                req.MediaLoop = this.Media.MediaLoop;
                req.MediaType = this.Media.MediaType;
                req.MediaURL = this.Media.MediaURL;
                req.MediaWidth = this.Media.MediaWidth;
                req.MusicURL = this.MusicURL;
                req.Name = this.Name;
                req.ObscureMedia = this.ObscureMedia;
                req.ObscureMusic = this.ObscureMusic;
                req.ParcelFlags = this.Flags;
                req.PassHours = this.PassHours;
                req.PassPrice = (uint)this.PassPrice;
                req.SalePrice = (uint)this.SalePrice;
                req.SnapshotID = this.SnapshotID;
                req.UserLocation = this.UserLocation;
                req.UserLookAt = this.UserLookAt;

                OSDMap body = req.Serialize();

                CapsClient capsPost = new CapsClient(url);
                capsPost.BeginGetResponse(body, OSDFormat.Xml, simulator.Client.Config.CAPS_TIMEOUT);
            }
            else
            {
                ParcelPropertiesUpdatePacket request = new ParcelPropertiesUpdatePacket();

                request.AgentData.AgentID = simulator.Client.Agent.AgentID;
                request.AgentData.SessionID = simulator.Client.Agent.SessionID;

                request.ParcelData.LocalID = this.LocalID;

                request.ParcelData.AuthBuyerID = this.AuthBuyerID;
                request.ParcelData.Category = (byte)this.Category;
                request.ParcelData.Desc = Utils.StringToBytes(this.Desc);
                request.ParcelData.GroupID = this.GroupID;
                request.ParcelData.LandingType = (byte)this.Landing;
                request.ParcelData.MediaAutoScale = (this.Media.MediaAutoScale) ? (byte)0x1 : (byte)0x0;
                request.ParcelData.MediaID = this.Media.MediaID;
                request.ParcelData.MediaURL = Utils.StringToBytes(this.Media.MediaURL.ToString());
                request.ParcelData.MusicURL = Utils.StringToBytes(this.MusicURL.ToString());
                request.ParcelData.Name = Utils.StringToBytes(this.Name);
                if (wantReply) request.ParcelData.Flags = 1;
                request.ParcelData.ParcelFlags = (uint)this.Flags;
                request.ParcelData.PassHours = this.PassHours;
                request.ParcelData.PassPrice = this.PassPrice;
                request.ParcelData.SalePrice = this.SalePrice;
                request.ParcelData.SnapshotID = this.SnapshotID;
                request.ParcelData.UserLocation = this.UserLocation;
                request.ParcelData.UserLookAt = this.UserLookAt;

                simulator.Inject(request, Direction.Outgoing);
            }

            UpdateOtherCleanTime(simulator);

        }

        /// <summary>
        /// Set Autoreturn time
        /// </summary>
        /// <param name="simulator">Simulator to send the update to</param>
        public void UpdateOtherCleanTime(RegionProxy simulator)
        {
            ParcelSetOtherCleanTimePacket request = new ParcelSetOtherCleanTimePacket();
            request.AgentData.AgentID = simulator.Client.Agent.AgentID;
            request.AgentData.SessionID = simulator.Client.Agent.SessionID;
            request.ParcelData.LocalID = this.LocalID;
            request.ParcelData.OtherCleanTime = this.OtherCleanTime;

            simulator.Inject(request, Direction.Outgoing);
        }
    }

    #endregion Parcel Class

    /// <summary>
    /// Parcel (subdivided simulator lots) subsystem
    /// </summary>
    public class ParcelManager
    {
        #region Structs

        /// <summary>
        /// Parcel Accesslist
        /// </summary>
        public struct ParcelAccessEntry
        {
            /// <summary>Agents <seealso cref="T:OpenMetaverse.UUID"/></summary>
            public UUID AgentID;
            /// <summary></summary>
            public DateTime Time;
            /// <summary>Flags for specific entry in white/black lists</summary>
            public AccessList Flags;
        }

        /// <summary>
        /// Owners of primitives on parcel
        /// </summary>
        public struct ParcelPrimOwners
        {
            /// <summary>Prim Owners <seealso cref="T:OpenMetaverse.UUID"/></summary>
            public UUID OwnerID;
            /// <summary>True of owner is group</summary>
            public bool IsGroupOwned;
            /// <summary>Total count of prims owned by OwnerID</summary>
            public int Count;
            /// <summary>true of OwnerID is currently online and is not a group</summary>
            public bool OnlineStatus;
            /// <summary>The date of the most recent prim left by OwnerID</summary>
            public DateTime NewestPrim;
        }

        #endregion Structs

        #region Delegates
        /// <summary>
        /// Called once parcel resource usage information has been collected
        /// </summary>
        /// <param name="success">Indicates if operation was successfull</param>
        /// <param name="info">Parcel resource usage information</param>
        public delegate void LandResourcesCallback(bool success, LandResourcesInfo info);

        /// <summary>The event subscribers. null if no subcribers</summary>
        private EventHandler<ParcelDwellReplyEventArgs> m_DwellReply;

        /// <summary>Raises the ParcelDwellReply event</summary>
        /// <param name="e">A ParcelDwellReplyEventArgs object containing the
        /// data returned from the simulator</param>
        protected virtual void OnParcelDwellReply(ParcelDwellReplyEventArgs e)
        {
            EventHandler<ParcelDwellReplyEventArgs> handler = m_DwellReply;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>Thread sync lock object</summary>
        private readonly object m_DwellReplyLock = new object();

        /// <summary>Raised when the simulator responds to a <see cref="RequestDwell"/> request</summary>
        public event EventHandler<ParcelDwellReplyEventArgs> ParcelDwellReply
        {
            add { lock (m_DwellReplyLock) { m_DwellReply += value; } }
            remove { lock (m_DwellReplyLock) { m_DwellReply -= value; } }
        }

        /// <summary>The event subscribers. null if no subcribers</summary>
        private EventHandler<ParcelInfoReplyEventArgs> m_ParcelInfo;

        /// <summary>Raises the ParcelInfoReply event</summary>
        /// <param name="e">A ParcelInfoReplyEventArgs object containing the
        /// data returned from the simulator</param>
        protected virtual void OnParcelInfoReply(ParcelInfoReplyEventArgs e)
        {
            EventHandler<ParcelInfoReplyEventArgs> handler = m_ParcelInfo;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>Thread sync lock object</summary>
        private readonly object m_ParcelInfoLock = new object();

        /// <summary>Raised when the simulator responds to a <see cref="RequestParcelInfo"/> request</summary>
        public event EventHandler<ParcelInfoReplyEventArgs> ParcelInfoReply
        {
            add { lock (m_ParcelInfoLock) { m_ParcelInfo += value; } }
            remove { lock (m_ParcelInfoLock) { m_ParcelInfo -= value; } }
        }

        /// <summary>The event subscribers. null if no subcribers</summary>
        private EventHandler<ParcelPropertiesEventArgs> m_ParcelProperties;

        /// <summary>Raises the ParcelProperties event</summary>
        /// <param name="e">A ParcelPropertiesEventArgs object containing the
        /// data returned from the simulator</param>
        protected virtual void OnParcelProperties(ParcelPropertiesEventArgs e)
        {
            EventHandler<ParcelPropertiesEventArgs> handler = m_ParcelProperties;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>Thread sync lock object</summary>
        private readonly object m_ParcelPropertiesLock = new object();

        /// <summary>Raised when the simulator responds to a <see cref="RequestParcelProperties"/> request</summary>
        public event EventHandler<ParcelPropertiesEventArgs> ParcelProperties
        {
            add { lock (m_ParcelPropertiesLock) { m_ParcelProperties += value; } }
            remove { lock (m_ParcelPropertiesLock) { m_ParcelProperties -= value; } }
        }

        /// <summary>The event subscribers. null if no subcribers</summary>
        private EventHandler<ParcelAccessListReplyEventArgs> m_ParcelACL;

        /// <summary>Raises the ParcelAccessListReply event</summary>
        /// <param name="e">A ParcelAccessListReplyEventArgs object containing the
        /// data returned from the simulator</param>
        protected virtual void OnParcelAccessListReply(ParcelAccessListReplyEventArgs e)
        {
            EventHandler<ParcelAccessListReplyEventArgs> handler = m_ParcelACL;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>Thread sync lock object</summary>
        private readonly object m_ParcelACLLock = new object();

        /// <summary>Raised when the simulator responds to a <see cref="RequestParcelAccessList"/> request</summary>
        public event EventHandler<ParcelAccessListReplyEventArgs> ParcelAccessListReply
        {
            add { lock (m_ParcelACLLock) { m_ParcelACL += value; } }
            remove { lock (m_ParcelACLLock) { m_ParcelACL -= value; } }
        }

        /// <summary>The event subscribers. null if no subcribers</summary>
        private EventHandler<ParcelObjectOwnersReplyEventArgs> m_ParcelObjectOwnersReply;

        /// <summary>Raises the ParcelObjectOwnersReply event</summary>
        /// <param name="e">A ParcelObjectOwnersReplyEventArgs object containing the
        /// data returned from the simulator</param>
        protected virtual void OnParcelObjectOwnersReply(ParcelObjectOwnersReplyEventArgs e)
        {
            EventHandler<ParcelObjectOwnersReplyEventArgs> handler = m_ParcelObjectOwnersReply;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>Thread sync lock object</summary>
        private readonly object m_ParcelObjectOwnersLock = new object();

        /// <summary>Raised when the simulator responds to a <see cref="RequestObjectOwners"/> request</summary>
        public event EventHandler<ParcelObjectOwnersReplyEventArgs> ParcelObjectOwnersReply
        {
            add { lock (m_ParcelObjectOwnersLock) { m_ParcelObjectOwnersReply += value; } }
            remove { lock (m_ParcelObjectOwnersLock) { m_ParcelObjectOwnersReply -= value; } }
        }

        /// <summary>The event subscribers. null if no subcribers</summary>
        private EventHandler<SimParcelsDownloadedEventArgs> m_SimParcelsDownloaded;

        /// <summary>Raises the SimParcelsDownloaded event</summary>
        /// <param name="e">A SimParcelsDownloadedEventArgs object containing the
        /// data returned from the simulator</param>
        protected virtual void OnSimParcelsDownloaded(SimParcelsDownloadedEventArgs e)
        {
            EventHandler<SimParcelsDownloadedEventArgs> handler = m_SimParcelsDownloaded;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>Thread sync lock object</summary>
        private readonly object m_SimParcelsDownloadedLock = new object();

        /// <summary>Raised when the simulator responds to a <see cref="RequestAllSimParcels"/> request</summary>
        public event EventHandler<SimParcelsDownloadedEventArgs> SimParcelsDownloaded
        {
            add { lock (m_SimParcelsDownloadedLock) { m_SimParcelsDownloaded += value; } }
            remove { lock (m_SimParcelsDownloadedLock) { m_SimParcelsDownloaded -= value; } }
        }

        /// <summary>The event subscribers. null if no subcribers</summary>
        private EventHandler<ForceSelectObjectsReplyEventArgs> m_ForceSelectObjects;

        /// <summary>Raises the ForceSelectObjectsReply event</summary>
        /// <param name="e">A ForceSelectObjectsReplyEventArgs object containing the
        /// data returned from the simulator</param>
        protected virtual void OnForceSelectObjectsReply(ForceSelectObjectsReplyEventArgs e)
        {
            EventHandler<ForceSelectObjectsReplyEventArgs> handler = m_ForceSelectObjects;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>Thread sync lock object</summary>
        private readonly object m_ForceSelectObjectsLock = new object();

        /// <summary>Raised when the simulator responds to a <see cref="RequestForceSelectObjects"/> request</summary>
        public event EventHandler<ForceSelectObjectsReplyEventArgs> ForceSelectObjectsReply
        {
            add { lock (m_ForceSelectObjectsLock) { m_ForceSelectObjects += value; } }
            remove { lock (m_ForceSelectObjectsLock) { m_ForceSelectObjects -= value; } }
        }

        /// <summary>The event subscribers. null if no subcribers</summary>
        private EventHandler<ParcelMediaUpdateReplyEventArgs> m_ParcelMediaUpdateReply;

        /// <summary>Raises the ParcelMediaUpdateReply event</summary>
        /// <param name="e">A ParcelMediaUpdateReplyEventArgs object containing the
        /// data returned from the simulator</param>
        protected virtual void OnParcelMediaUpdateReply(ParcelMediaUpdateReplyEventArgs e)
        {
            EventHandler<ParcelMediaUpdateReplyEventArgs> handler = m_ParcelMediaUpdateReply;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>Thread sync lock object</summary>
        private readonly object m_ParcelMediaUpdateReplyLock = new object();

        /// <summary>Raised when the simulator responds to a Parcel Update request</summary>
        public event EventHandler<ParcelMediaUpdateReplyEventArgs> ParcelMediaUpdateReply
        {
            add { lock (m_ParcelMediaUpdateReplyLock) { m_ParcelMediaUpdateReply += value; } }
            remove { lock (m_ParcelMediaUpdateReplyLock) { m_ParcelMediaUpdateReply -= value; } }
        }

        /// <summary>The event subscribers. null if no subcribers</summary>
        private EventHandler<ParcelMediaCommandEventArgs> m_ParcelMediaCommand;

        /// <summary>Raises the ParcelMediaCommand event</summary>
        /// <param name="e">A ParcelMediaCommandEventArgs object containing the
        /// data returned from the simulator</param>
        protected virtual void OnParcelMediaCommand(ParcelMediaCommandEventArgs e)
        {
            EventHandler<ParcelMediaCommandEventArgs> handler = m_ParcelMediaCommand;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>Thread sync lock object</summary>
        private readonly object m_ParcelMediaCommandLock = new object();

        /// <summary>Raised when the parcel your agent is located sends a ParcelMediaCommand</summary>
        public event EventHandler<ParcelMediaCommandEventArgs> ParcelMediaCommand
        {
            add { lock (m_ParcelMediaCommandLock) { m_ParcelMediaCommand += value; } }
            remove { lock (m_ParcelMediaCommandLock) { m_ParcelMediaCommand -= value; } }
        }
        #endregion Delegates

        private ProxyFrame Client;
        private AutoResetEvent WaitForSimParcel;

        #region Public Methods

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="client">A reference to the GridClient object</param>
        public ParcelManager(ProxyFrame client)
        {
            Client = client;

            // Setup the callbacks
            Client.Network.AddDelegate(PacketType.ParcelInfoReply, Direction.Incoming, ParcelInfoReplyHandler);
            Client.Network.AddEventDelegate("ParcelObjectOwnersReply", new EventQueueDelegate(ParcelObjectOwnersReplyHandler));
            // CAPS packet handler, to allow for Media Data not contained in the message template
            Client.Network.AddEventDelegate("ParcelProperties", new EventQueueDelegate(ParcelPropertiesReplyHandler));
            Client.Network.AddDelegate(PacketType.ParcelDwellReply, Direction.Incoming, ParcelDwellReplyHandler);
            Client.Network.AddDelegate(PacketType.ParcelAccessListReply, Direction.Incoming, ParcelAccessListReplyHandler);
            Client.Network.AddDelegate(PacketType.ForceObjectSelect, Direction.Incoming, SelectParcelObjectsReplyHandler);
            Client.Network.AddDelegate(PacketType.ParcelMediaUpdate, Direction.Incoming, ParcelMediaUpdateHandler);
            Client.Network.AddDelegate(PacketType.ParcelOverlay, Direction.Incoming, ParcelOverlayHandler);
            Client.Network.AddDelegate(PacketType.ParcelMediaCommandMessage, Direction.Incoming, ParcelMediaCommandMessagePacketHandler);
        }

        /// <summary>
        /// Request basic information for a single parcel
        /// </summary>
        /// <param name="parcelID">Simulator-local ID of the parcel</param>
        public void RequestParcelInfo(UUID parcelID)
        {
            ParcelInfoRequestPacket request = new ParcelInfoRequestPacket();
            request.AgentData.AgentID = Client.Agent.AgentID;
            request.AgentData.SessionID = Client.Agent.SessionID;
            request.Data.ParcelID = parcelID;

            Client.Network.InjectPacket(request, Direction.Outgoing);
        }

        /// <summary>
        /// Request properties of a single parcel
        /// </summary>
        /// <param name="simulator">Simulator containing the parcel</param>
        /// <param name="localID">Simulator-local ID of the parcel</param>
        /// <param name="sequenceID">An arbitrary integer that will be returned
        /// with the ParcelProperties reply, useful for distinguishing between
        /// multiple simultaneous requests</param>
        public void RequestParcelProperties(RegionProxy simulator, int localID, int sequenceID)
        {
            ParcelPropertiesRequestByIDPacket request = new ParcelPropertiesRequestByIDPacket();

            request.AgentData.AgentID = Client.Agent.AgentID;
            request.AgentData.SessionID = Client.Agent.SessionID;

            request.ParcelData.LocalID = localID;
            request.ParcelData.SequenceID = sequenceID;

            simulator.Inject(request, Direction.Outgoing);
        }

        /// <summary>
        /// Request the access list for a single parcel
        /// </summary>
        /// <param name="simulator">Simulator containing the parcel</param>
        /// <param name="localID">Simulator-local ID of the parcel</param>
        /// <param name="sequenceID">An arbitrary integer that will be returned
        /// with the ParcelAccessList reply, useful for distinguishing between
        /// multiple simultaneous requests</param>
        /// <param name="flags"></param>
        public void RequestParcelAccessList(RegionProxy simulator, int localID, AccessList flags, int sequenceID)
        {
            ParcelAccessListRequestPacket request = new ParcelAccessListRequestPacket();

            request.AgentData.AgentID = Client.Agent.AgentID;
            request.AgentData.SessionID = Client.Agent.SessionID;
            request.Data.LocalID = localID;
            request.Data.Flags = (uint)flags;
            request.Data.SequenceID = sequenceID;

            simulator.Inject(request, Direction.Outgoing);
        }

        /// <summary>
        /// Request properties of parcels using a bounding box selection
        /// </summary>
        /// <param name="simulator">Simulator containing the parcel</param>
        /// <param name="north">Northern boundary of the parcel selection</param>
        /// <param name="east">Eastern boundary of the parcel selection</param>
        /// <param name="south">Southern boundary of the parcel selection</param>
        /// <param name="west">Western boundary of the parcel selection</param>
        /// <param name="sequenceID">An arbitrary integer that will be returned
        /// with the ParcelProperties reply, useful for distinguishing between
        /// different types of parcel property requests</param>
        /// <param name="snapSelection">A boolean that is returned with the
        /// ParcelProperties reply, useful for snapping focus to a single
        /// parcel</param>
        public void RequestParcelProperties(RegionProxy simulator, float north, float east, float south, float west,
            int sequenceID, bool snapSelection)
        {
            ParcelPropertiesRequestPacket request = new ParcelPropertiesRequestPacket();

            request.AgentData.AgentID = Client.Agent.AgentID;
            request.AgentData.SessionID = Client.Agent.SessionID;
            request.ParcelData.North = north;
            request.ParcelData.East = east;
            request.ParcelData.South = south;
            request.ParcelData.West = west;
            request.ParcelData.SequenceID = sequenceID;
            request.ParcelData.SnapSelection = snapSelection;

            simulator.Inject(request, Direction.Outgoing);
        }

        /// <summary>
        /// Request all simulator parcel properties (used for populating the <code>Simulator.Parcels</code> 
        /// dictionary)
        /// </summary>
        /// <param name="simulator">Simulator to request parcels from (must be connected)</param>
        public void RequestAllSimParcels(RegionProxy simulator)
        {
            RequestAllSimParcels(simulator, false, 750);
        }

        /// <summary>
        /// Request all simulator parcel properties (used for populating the <code>Simulator.Parcels</code> 
        /// dictionary)
        /// </summary>
        /// <param name="simulator">Simulator to request parcels from (must be connected)</param>
        /// <param name="refresh">If TRUE, will force a full refresh</param>
        /// <param name="msDelay">Number of milliseconds to pause in between each request</param>
        public void RequestAllSimParcels(RegionProxy simulator, bool refresh, int msDelay)
        {
            if (simulator.DownloadingParcelMap)
            {
                Logger.Log("Already downloading parcels in " + simulator.Name, Helpers.LogLevel.Info);
                return;
            }
            else
            {
                simulator.DownloadingParcelMap = true;
                WaitForSimParcel = new AutoResetEvent(false);
            }

            if (refresh)
            {
                for (int y = 0; y < 64; y++)
                    for (int x = 0; x < 64; x++)
                        simulator.ParcelMap[y, x] = 0;
            }

            Thread th = new Thread(delegate ()
            {
                int count = 0, timeouts = 0, y, x;

                for (y = 0; y < 64; y++)
                {
                    for (x = 0; x < 64; x++)
                    {
                        if (!Client.LoggedIn)
                            return;

                        if (simulator.ParcelMap[y, x] == 0)
                        {
                            Client.Parcels.RequestParcelProperties(simulator,
                                                             (y + 1) * 4.0f, (x + 1) * 4.0f,
                                                             y * 4.0f, x * 4.0f, int.MaxValue, false);

                            // Wait the given amount of time for a reply before sending the next request
                            if (!WaitForSimParcel.WaitOne(msDelay, false))
                                ++timeouts;

                            ++count;
                        }
                    }
                }

                //Logger.Log(String.Format(
                //    "Full simulator parcel information retrieved. Sent {0} parcel requests. Current outgoing queue: {1}, Retry Count {2}",
                //    count, Client.Network.OutboxCount, timeouts), Helpers.LogLevel.Info, Client);

                simulator.DownloadingParcelMap = false;
            });

            th.Start();
        }

        /// <summary>
        /// Request the dwell value for a parcel
        /// </summary>
        /// <param name="simulator">Simulator containing the parcel</param>
        /// <param name="localID">Simulator-local ID of the parcel</param>
        public void RequestDwell(RegionProxy simulator, int localID)
        {
            ParcelDwellRequestPacket request = new ParcelDwellRequestPacket();
            request.AgentData.AgentID = Client.Agent.AgentID;
            request.AgentData.SessionID = Client.Agent.SessionID;
            request.Data.LocalID = localID;
            request.Data.ParcelID = UUID.Zero; // Not used by clients

            simulator.Inject(request, Direction.Outgoing);
        }

        /// <summary>
        /// Send a request to Purchase a parcel of land
        /// </summary>
        /// <param name="simulator">The Simulator the parcel is located in</param>
        /// <param name="localID">The parcels region specific local ID</param>
        /// <param name="forGroup">true if this parcel is being purchased by a group</param>
        /// <param name="groupID">The groups <seealso cref="T:OpenMetaverse.UUID"/></param>
        /// <param name="removeContribution">true to remove tier contribution if purchase is successful</param>
        /// <param name="parcelArea">The parcels size</param>
        /// <param name="parcelPrice">The purchase price of the parcel</param>
        /// <returns></returns>
        public void Buy(RegionProxy simulator, int localID, bool forGroup, UUID groupID,
            bool removeContribution, int parcelArea, int parcelPrice)
        {
            ParcelBuyPacket request = new ParcelBuyPacket();

            request.AgentData.AgentID = Client.Agent.AgentID;
            request.AgentData.SessionID = Client.Agent.SessionID;

            request.Data.Final = true;
            request.Data.GroupID = groupID;
            request.Data.LocalID = localID;
            request.Data.IsGroupOwned = forGroup;
            request.Data.RemoveContribution = removeContribution;

            request.ParcelData.Area = parcelArea;
            request.ParcelData.Price = parcelPrice;

            simulator.Inject(request, Direction.Outgoing);
        }

        /// <summary>
        /// Reclaim a parcel of land
        /// </summary>
        /// <param name="simulator">The simulator the parcel is in</param>
        /// <param name="localID">The parcels region specific local ID</param>
        public void Reclaim(RegionProxy simulator, int localID)
        {
            ParcelReclaimPacket request = new ParcelReclaimPacket();
            request.AgentData.AgentID = Client.Agent.AgentID;
            request.AgentData.SessionID = Client.Agent.SessionID;

            request.Data.LocalID = localID;

            simulator.Inject(request, Direction.Outgoing);
        }

        /// <summary>
        /// Deed a parcel to a group
        /// </summary>
        /// <param name="simulator">The simulator the parcel is in</param>
        /// <param name="localID">The parcels region specific local ID</param>
        /// <param name="groupID">The groups <seealso cref="T:OpenMetaverse.UUID"/></param>
        public void DeedToGroup(RegionProxy simulator, int localID, UUID groupID)
        {
            ParcelDeedToGroupPacket request = new ParcelDeedToGroupPacket();
            request.AgentData.AgentID = Client.Agent.AgentID;
            request.AgentData.SessionID = Client.Agent.SessionID;

            request.Data.LocalID = localID;
            request.Data.GroupID = groupID;

            simulator.Inject(request, Direction.Outgoing);
        }

        /// <summary>
        /// Request prim owners of a parcel of land.
        /// </summary>
        /// <param name="simulator">Simulator parcel is in</param>
        /// <param name="localID">The parcels region specific local ID</param>
        public void RequestObjectOwners(RegionProxy simulator, int localID)
        {
            ParcelObjectOwnersRequestPacket request = new ParcelObjectOwnersRequestPacket();

            request.AgentData.AgentID = Client.Agent.AgentID;
            request.AgentData.SessionID = Client.Agent.SessionID;

            request.ParcelData.LocalID = localID;
            simulator.Inject(request, Direction.Outgoing);
        }

        /// <summary>
        /// Return objects from a parcel
        /// </summary>
        /// <param name="simulator">Simulator parcel is in</param>
        /// <param name="localID">The parcels region specific local ID</param>
        /// <param name="type">the type of objects to return, <seealso cref="T:OpenMetaverse.ObjectReturnType"/></param>
        /// <param name="ownerIDs">A list containing object owners <seealso cref="OpenMetaverse.UUID"/>s to return</param>
        public void ReturnObjects(RegionProxy simulator, int localID, ObjectReturnType type, List<UUID> ownerIDs)
        {
            ParcelReturnObjectsPacket request = new ParcelReturnObjectsPacket();
            request.AgentData.AgentID = Client.Agent.AgentID;
            request.AgentData.SessionID = Client.Agent.SessionID;

            request.ParcelData.LocalID = localID;
            request.ParcelData.ReturnType = (uint)type;

            // A single null TaskID is (not) used for parcel object returns
            request.TaskIDs = new ParcelReturnObjectsPacket.TaskIDsBlock[1];
            request.TaskIDs[0] = new ParcelReturnObjectsPacket.TaskIDsBlock();
            request.TaskIDs[0].TaskID = UUID.Zero;

            // Convert the list of owner UUIDs to packet blocks if a list is given
            if (ownerIDs != null)
            {
                request.OwnerIDs = new ParcelReturnObjectsPacket.OwnerIDsBlock[ownerIDs.Count];

                for (int i = 0; i < ownerIDs.Count; i++)
                {
                    request.OwnerIDs[i] = new ParcelReturnObjectsPacket.OwnerIDsBlock();
                    request.OwnerIDs[i].OwnerID = ownerIDs[i];
                }
            }
            else
            {
                request.OwnerIDs = new ParcelReturnObjectsPacket.OwnerIDsBlock[0];
            }

            simulator.Inject(request, Direction.Outgoing);
        }

        /// <summary>
        /// Subdivide (split) a parcel
        /// </summary>
        /// <param name="simulator"></param>
        /// <param name="west"></param>
        /// <param name="south"></param>
        /// <param name="east"></param>
        /// <param name="north"></param>
        public void ParcelSubdivide(RegionProxy simulator, float west, float south, float east, float north)
        {
            ParcelDividePacket divide = new ParcelDividePacket();
            divide.AgentData.AgentID = Client.Agent.AgentID;
            divide.AgentData.SessionID = Client.Agent.SessionID;
            divide.ParcelData.East = east;
            divide.ParcelData.North = north;
            divide.ParcelData.South = south;
            divide.ParcelData.West = west;

            simulator.Inject(divide, Direction.Outgoing);
        }

        /// <summary>
        /// Join two parcels of land creating a single parcel
        /// </summary>
        /// <param name="simulator"></param>
        /// <param name="west"></param>
        /// <param name="south"></param>
        /// <param name="east"></param>
        /// <param name="north"></param>
        public void ParcelJoin(RegionProxy simulator, float west, float south, float east, float north)
        {
            ParcelJoinPacket join = new ParcelJoinPacket();
            join.AgentData.AgentID = Client.Agent.AgentID;
            join.AgentData.SessionID = Client.Agent.SessionID;
            join.ParcelData.East = east;
            join.ParcelData.North = north;
            join.ParcelData.South = south;
            join.ParcelData.West = west;

            simulator.Inject(join, Direction.Outgoing);
        }

        /// <summary>
        /// Get a parcels LocalID
        /// </summary>
        /// <param name="simulator">Simulator parcel is in</param>
        /// <param name="position">Vector3 position in simulator (Z not used)</param>
        /// <returns>0 on failure, or parcel LocalID on success.</returns>
        /// <remarks>A call to <code>Parcels.RequestAllSimParcels</code> is required to populate map and
        /// dictionary.</remarks>
        public int GetParcelLocalID(RegionProxy simulator, Vector3 position)
        {
            if (simulator.ParcelMap[(byte)position.Y / 4, (byte)position.X / 4] > 0)
            {
                return simulator.ParcelMap[(byte)position.Y / 4, (byte)position.X / 4];
            }
            else
            {
                Logger.Log(String.Format("ParcelMap returned an default/invalid value for location {0}/{1} Did you use RequestAllSimParcels() to populate the dictionaries?", (byte)position.Y / 4, (byte)position.X / 4), Helpers.LogLevel.Warning);
                return 0;
            }
        }

        /// <summary>
        /// Terraform (raise, lower, etc) an area or whole parcel of land
        /// </summary>
        /// <param name="simulator">Simulator land area is in.</param>
        /// <param name="localID">LocalID of parcel, or -1 if using bounding box</param>
        /// <param name="action">From Enum, Raise, Lower, Level, Smooth, Etc.</param>
        /// <param name="brushSize">Size of area to modify</param>
        /// <returns>true on successful request sent.</returns>
        /// <remarks>Settings.STORE_LAND_PATCHES must be true, 
        /// Parcel information must be downloaded using <code>RequestAllSimParcels()</code></remarks>
        public bool Terraform(RegionProxy simulator, int localID, TerraformAction action, TerraformBrushSize brushSize)
        {
            return Terraform(simulator, localID, 0f, 0f, 0f, 0f, action, brushSize, 1);
        }

        /// <summary>
        /// Terraform (raise, lower, etc) an area or whole parcel of land
        /// </summary>
        /// <param name="simulator">Simulator land area is in.</param>
        /// <param name="west">west border of area to modify</param>
        /// <param name="south">south border of area to modify</param>
        /// <param name="east">east border of area to modify</param>
        /// <param name="north">north border of area to modify</param>
        /// <param name="action">From Enum, Raise, Lower, Level, Smooth, Etc.</param>
        /// <param name="brushSize">Size of area to modify</param>
        /// <returns>true on successful request sent.</returns>
        /// <remarks>Settings.STORE_LAND_PATCHES must be true, 
        /// Parcel information must be downloaded using <code>RequestAllSimParcels()</code></remarks>
        public bool Terraform(RegionProxy simulator, float west, float south, float east, float north,
            TerraformAction action, TerraformBrushSize brushSize)
        {
            return Terraform(simulator, -1, west, south, east, north, action, brushSize, 1);
        }

        /// <summary>
        /// Terraform (raise, lower, etc) an area or whole parcel of land
        /// </summary>
        /// <param name="simulator">Simulator land area is in.</param>
        /// <param name="localID">LocalID of parcel, or -1 if using bounding box</param>
        /// <param name="west">west border of area to modify</param>
        /// <param name="south">south border of area to modify</param>
        /// <param name="east">east border of area to modify</param>
        /// <param name="north">north border of area to modify</param>
        /// <param name="action">From Enum, Raise, Lower, Level, Smooth, Etc.</param>
        /// <param name="brushSize">Size of area to modify</param>
        /// <param name="seconds">How many meters + or - to lower, 1 = 1 meter</param>
        /// <returns>true on successful request sent.</returns>
        /// <remarks>Settings.STORE_LAND_PATCHES must be true, 
        /// Parcel information must be downloaded using <code>RequestAllSimParcels()</code></remarks>
        public bool Terraform(RegionProxy simulator, int localID, float west, float south, float east, float north,
            TerraformAction action, TerraformBrushSize brushSize, int seconds)
        {
            float height = 0f;
            int x, y;
            if (localID == -1)
            {
                x = (int)east - (int)west / 2;
                y = (int)north - (int)south / 2;
            }
            else
            {
                Parcel p;
                if (!simulator.Parcels.TryGetValue(localID, out p))
                {
                    Logger.Log(String.Format("Can't find parcel {0} in simulator {1}", localID, simulator),
                        Helpers.LogLevel.Warning);
                    return false;
                }

                x = (int)p.AABBMax.X - (int)p.AABBMin.X / 2;
                y = (int)p.AABBMax.Y - (int)p.AABBMin.Y / 2;
            }

            if (!simulator.TerrainHeightAtPoint(x, y, out height))
            {
                Logger.Log("Land Patch not stored for location", Helpers.LogLevel.Warning);
                return false;
            }

            Terraform(simulator, localID, west, south, east, north, action, brushSize, seconds, height);
            return true;
        }

        /// <summary>
        /// Terraform (raise, lower, etc) an area or whole parcel of land
        /// </summary>
        /// <param name="simulator">Simulator land area is in.</param>
        /// <param name="localID">LocalID of parcel, or -1 if using bounding box</param>
        /// <param name="west">west border of area to modify</param>
        /// <param name="south">south border of area to modify</param>
        /// <param name="east">east border of area to modify</param>
        /// <param name="north">north border of area to modify</param>
        /// <param name="action">From Enum, Raise, Lower, Level, Smooth, Etc.</param>
        /// <param name="brushSize">Size of area to modify</param>
        /// <param name="seconds">How many meters + or - to lower, 1 = 1 meter</param>
        /// <param name="height">Height at which the terraform operation is acting at</param>
        public void Terraform(RegionProxy simulator, int localID, float west, float south, float east, float north,
            TerraformAction action, TerraformBrushSize brushSize, int seconds, float height)
        {
            ModifyLandPacket land = new ModifyLandPacket();
            land.AgentData.AgentID = Client.Agent.AgentID;
            land.AgentData.SessionID = Client.Agent.SessionID;

            land.ModifyBlock.Action = (byte)action;
            land.ModifyBlock.BrushSize = (byte)brushSize;
            land.ModifyBlock.Seconds = seconds;
            land.ModifyBlock.Height = height;

            land.ParcelData = new ModifyLandPacket.ParcelDataBlock[1];
            land.ParcelData[0] = new ModifyLandPacket.ParcelDataBlock();
            land.ParcelData[0].LocalID = localID;
            land.ParcelData[0].West = west;
            land.ParcelData[0].South = south;
            land.ParcelData[0].East = east;
            land.ParcelData[0].North = north;

            land.ModifyBlockExtended = new ModifyLandPacket.ModifyBlockExtendedBlock[1];
            land.ModifyBlockExtended[0] = new ModifyLandPacket.ModifyBlockExtendedBlock();
            land.ModifyBlockExtended[0].BrushSize = (float)brushSize;

            simulator.Inject(land, Direction.Outgoing);
        }

        /// <summary>
        /// Sends a request to the simulator to return a list of objects owned by specific owners
        /// </summary>
        /// <param name="localID">Simulator local ID of parcel</param>
        /// <param name="selectType">Owners, Others, Etc</param>
        /// <param name="ownerID">List containing keys of avatars objects to select; 
        /// if List is null will return Objects of type <c>selectType</c></param>
        /// <remarks>Response data is returned in the event <seealso cref="E:ForceSelectObjectsReply"/></remarks>
        public void RequestSelectObjects(int localID, ObjectReturnType selectType, UUID ownerID)
        {
            ParcelSelectObjectsPacket select = new ParcelSelectObjectsPacket();
            select.AgentData.AgentID = Client.Agent.AgentID;
            select.AgentData.SessionID = Client.Agent.SessionID;

            select.ParcelData.LocalID = localID;
            select.ParcelData.ReturnType = (uint)selectType;

            select.ReturnIDs = new ParcelSelectObjectsPacket.ReturnIDsBlock[1];
            select.ReturnIDs[0] = new ParcelSelectObjectsPacket.ReturnIDsBlock();
            select.ReturnIDs[0].ReturnID = ownerID;

            Client.Network.InjectPacket(select, Direction.Outgoing);
        }

        /// <summary>
        /// Eject and optionally ban a user from a parcel
        /// </summary>
        /// <param name="targetID">target key of avatar to eject</param>
        /// <param name="ban">true to also ban target</param>
        public void EjectUser(UUID targetID, bool ban)
        {
            EjectUserPacket eject = new EjectUserPacket();
            eject.AgentData.AgentID = Client.Agent.AgentID;
            eject.AgentData.SessionID = Client.Agent.SessionID;
            eject.Data.TargetID = targetID;
            if (ban) eject.Data.Flags = 1;
            else eject.Data.Flags = 0;

            Client.Network.InjectPacket(eject, Direction.Outgoing);
        }

        /// <summary>
        /// Freeze or unfreeze an avatar over your land
        /// </summary>
        /// <param name="targetID">target key to freeze</param>
        /// <param name="freeze">true to freeze, false to unfreeze</param>
        public void FreezeUser(UUID targetID, bool freeze)
        {
            FreezeUserPacket frz = new FreezeUserPacket();
            frz.AgentData.AgentID = Client.Agent.AgentID;
            frz.AgentData.SessionID = Client.Agent.SessionID;
            frz.Data.TargetID = targetID;
            if (freeze) frz.Data.Flags = 0;
            else frz.Data.Flags = 1;

            Client.Network.InjectPacket(frz, Direction.Outgoing);
        }

        /// <summary>
        /// Abandon a parcel of land
        /// </summary>
        /// <param name="simulator">Simulator parcel is in</param>
        /// <param name="localID">Simulator local ID of parcel</param>
        public void ReleaseParcel(RegionProxy simulator, int localID)
        {
            ParcelReleasePacket abandon = new ParcelReleasePacket();
            abandon.AgentData.AgentID = Client.Agent.AgentID;
            abandon.AgentData.SessionID = Client.Agent.SessionID;
            abandon.Data.LocalID = localID;

            simulator.Inject(abandon, Direction.Outgoing);
        }

        /// <summary>
        /// Requests the UUID of the parcel in a remote region at a specified location
        /// </summary>
        /// <param name="location">Location of the parcel in the remote region</param>
        /// <param name="regionHandle">Remote region handle</param>
        /// <param name="regionID">Remote region UUID</param>
        /// <returns>If successful UUID of the remote parcel, UUID.Zero otherwise</returns>
        public UUID RequestRemoteParcelID(Vector3 location, ulong regionHandle, UUID regionID)
        {
            if (Client.Network.CurrentSim == null || Client.Network.CurrentSim.Caps == null)
                return UUID.Zero;

            if (Client.Network.CurrentSim.Caps.TryGetValue("RemoteParcelRequest", out string cap))
            {
                Uri url = new Uri(cap);

                RemoteParcelRequestRequest msg = new RemoteParcelRequestRequest();
                msg.Location = location;
                msg.RegionHandle = regionHandle;
                msg.RegionID = regionID;

                try
                {
                    CapsClient request = new CapsClient(url);
                    OSD result = request.GetResponse(msg.Serialize(), OSDFormat.Xml, Client.Config.CAPS_TIMEOUT);
                    RemoteParcelRequestReply response = new RemoteParcelRequestReply();
                    response.Deserialize((OSDMap)result);
                    return response.ParcelID;
                }
                catch (Exception)
                {
                    Logger.Log("Failed to fetch remote parcel ID", Helpers.LogLevel.Debug);
                }
            }

            return UUID.Zero;

        }

        /// <summary>
        /// Retrieves information on resources used by the parcel
        /// </summary>
        /// <param name="parcelID">UUID of the parcel</param>
        /// <param name="getDetails">Should per object resource usage be requested</param>
        /// <param name="callback">Callback invoked when the request is complete</param>
        public void GetParcelResouces(UUID parcelID, bool getDetails, LandResourcesCallback callback)
        {
            try
            {
                Client.Network.CurrentSim.Caps.TryGetValue("LandResources", out string cap);

                Uri url = new Uri(cap);
                CapsClient request = new CapsClient(url);

                request.OnComplete += delegate (CapsClient client, OSD result, Exception error)
                {
                    try
                    {
                        if (result == null || error != null)
                        {
                            callback(false, null);
                        }
                        LandResourcesMessage response = new LandResourcesMessage();
                        response.Deserialize((OSDMap)result);

                        CapsClient summaryRequest = new CapsClient(response.ScriptResourceSummary);
                        OSD summaryResponse = summaryRequest.GetResponse(Client.Config.CAPS_TIMEOUT);

                        LandResourcesInfo res = new LandResourcesInfo();
                        res.Deserialize((OSDMap)summaryResponse);

                        if (response.ScriptResourceDetails != null && getDetails)
                        {
                            CapsClient detailRequest = new CapsClient(response.ScriptResourceDetails);
                            OSD detailResponse = detailRequest.GetResponse(Client.Config.CAPS_TIMEOUT);
                            res.Deserialize((OSDMap)detailResponse);
                        }
                        callback(true, res);
                    }
                    catch (Exception ex)
                    {
                        Logger.Log("Failed fetching land resources", Helpers.LogLevel.Error, ex);
                        callback(false, null);
                    }
                };

                LandResourcesRequest param = new LandResourcesRequest();
                param.ParcelID = parcelID;
                request.BeginGetResponse(param.Serialize(), OSDFormat.Xml, Client.Config.CAPS_TIMEOUT);

            }
            catch (Exception ex)
            {
                Logger.Log("Failed fetching land resources:", Helpers.LogLevel.Error, ex);
                callback(false, null);
            }
        }

        #endregion Public Methods

        #region Packet Handlers

        /// <summary>Process an incoming packet and raise the appropriate events</summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">The EventArgs object containing the packet data</param>
        /// <remarks>Raises the <see cref="ParcelDwellReply"/> event</remarks>
        protected Packet ParcelDwellReplyHandler(Packet packet, RegionProxy simulator)
        {
            //if (m_DwellReply != null || Client.Settings.ALWAYS_REQUEST_PARCEL_DWELL == true)
            if (m_DwellReply != null)
            {
                ParcelDwellReplyPacket dwell = (ParcelDwellReplyPacket)packet;

                lock (simulator.Parcels.Dictionary)
                {
                    if (simulator.Parcels.Dictionary.ContainsKey(dwell.Data.LocalID))
                    {
                        Parcel parcel = simulator.Parcels.Dictionary[dwell.Data.LocalID];
                        parcel.Dwell = dwell.Data.Dwell;
                        simulator.Parcels.Dictionary[dwell.Data.LocalID] = parcel;
                    }
                }

                if (m_DwellReply != null)
                {
                    OnParcelDwellReply(new ParcelDwellReplyEventArgs(dwell.Data.ParcelID, dwell.Data.LocalID, dwell.Data.Dwell));
                }
            }

            return packet;
        }

        /// <summary>Process an incoming packet and raise the appropriate events</summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">The EventArgs object containing the packet data</param>
        /// <remarks>Raises the <see cref="ParcelInfoReply"/> event</remarks>
        protected Packet ParcelInfoReplyHandler(Packet packet, RegionProxy sim)
        {
            if (m_ParcelInfo != null)
            {
                ParcelInfoReplyPacket info = (ParcelInfoReplyPacket)packet;

                ParcelInfo parcelInfo = new ParcelInfo();

                parcelInfo.ActualArea = info.Data.ActualArea;
                parcelInfo.AuctionID = info.Data.AuctionID;
                parcelInfo.BillableArea = info.Data.BillableArea;
                parcelInfo.Description = Utils.BytesToString(info.Data.Desc);
                parcelInfo.Dwell = info.Data.Dwell;
                parcelInfo.GlobalX = info.Data.GlobalX;
                parcelInfo.GlobalY = info.Data.GlobalY;
                parcelInfo.GlobalZ = info.Data.GlobalZ;
                parcelInfo.ID = info.Data.ParcelID;
                parcelInfo.Mature = ((info.Data.Flags & 1) != 0) ? true : false;
                parcelInfo.Name = Utils.BytesToString(info.Data.Name);
                parcelInfo.OwnerID = info.Data.OwnerID;
                parcelInfo.SalePrice = info.Data.SalePrice;
                parcelInfo.SimName = Utils.BytesToString(info.Data.SimName);
                parcelInfo.SnapshotID = info.Data.SnapshotID;

                OnParcelInfoReply(new ParcelInfoReplyEventArgs(parcelInfo));
            }

            return packet;
        }

        protected void ParcelPropertiesReplyHandler(string capsKey, IMessage message, RegionProxy simulator)
        {
            //if (m_ParcelProperties != null || Client.Settings.PARCEL_TRACKING == true)
            if (m_ParcelProperties != null || true)
            {
                ParcelPropertiesMessage msg = (ParcelPropertiesMessage)message;

                Parcel parcel = new Parcel(msg.LocalID);

                parcel.AABBMax = msg.AABBMax;
                parcel.AABBMin = msg.AABBMin;
                parcel.Area = msg.Area;
                parcel.AuctionID = msg.AuctionID;
                parcel.AuthBuyerID = msg.AuthBuyerID;
                parcel.Bitmap = msg.Bitmap;
                parcel.Category = msg.Category;
                parcel.ClaimDate = msg.ClaimDate;
                parcel.ClaimPrice = msg.ClaimPrice;
                parcel.Desc = msg.Desc;
                parcel.Flags = msg.ParcelFlags;
                parcel.GroupID = msg.GroupID;
                parcel.GroupPrims = msg.GroupPrims;
                parcel.IsGroupOwned = msg.IsGroupOwned;
                parcel.Landing = msg.LandingType;
                parcel.MaxPrims = msg.MaxPrims;
                parcel.Media.MediaAutoScale = msg.MediaAutoScale;
                parcel.Media.MediaID = msg.MediaID;
                parcel.Media.MediaURL = msg.MediaURL;
                parcel.MusicURL = msg.MusicURL;
                parcel.Name = msg.Name;
                parcel.OtherCleanTime = msg.OtherCleanTime;
                parcel.OtherCount = msg.OtherCount;
                parcel.OtherPrims = msg.OtherPrims;
                parcel.OwnerID = msg.OwnerID;
                parcel.OwnerPrims = msg.OwnerPrims;
                parcel.ParcelPrimBonus = msg.ParcelPrimBonus;
                parcel.PassHours = msg.PassHours;
                parcel.PassPrice = msg.PassPrice;
                parcel.PublicCount = msg.PublicCount;
                parcel.RegionDenyAgeUnverified = msg.RegionDenyAgeUnverified;
                parcel.RegionDenyAnonymous = msg.RegionDenyAnonymous;
                parcel.RegionPushOverride = msg.RegionPushOverride;
                parcel.RentPrice = msg.RentPrice;
                ParcelResult result = msg.RequestResult;
                parcel.SalePrice = msg.SalePrice;
                int selectedPrims = msg.SelectedPrims;
                parcel.SelfCount = msg.SelfCount;
                int sequenceID = msg.SequenceID;
                parcel.SimWideMaxPrims = msg.SimWideMaxPrims;
                parcel.SimWideTotalPrims = msg.SimWideTotalPrims;
                bool snapSelection = msg.SnapSelection;
                parcel.SnapshotID = msg.SnapshotID;
                parcel.Status = msg.Status;
                parcel.TotalPrims = msg.TotalPrims;
                parcel.UserLocation = msg.UserLocation;
                parcel.UserLookAt = msg.UserLookAt;
                parcel.Media.MediaDesc = msg.MediaDesc;
                parcel.Media.MediaHeight = msg.MediaHeight;
                parcel.Media.MediaWidth = msg.MediaWidth;
                parcel.Media.MediaLoop = msg.MediaLoop;
                parcel.Media.MediaType = msg.MediaType;
                parcel.ObscureMedia = msg.ObscureMedia;
                parcel.ObscureMusic = msg.ObscureMusic;

                //if (Client.Settings.PARCEL_TRACKING)
                {
                    lock (simulator.Parcels.Dictionary)
                        simulator.Parcels.Dictionary[parcel.LocalID] = parcel;

                    bool set = false;
                    int y, x, index, bit;
                    for (y = 0; y < 64; y++)
                    {
                        for (x = 0; x < 64; x++)
                        {
                            index = (y * 64) + x;
                            bit = index % 8;
                            index >>= 3;

                            if ((parcel.Bitmap[index] & (1 << bit)) != 0)
                            {
                                simulator.ParcelMap[y, x] = parcel.LocalID;
                                set = true;
                            }
                        }
                    }

                    if (!set)
                    {
                        Logger.Log("Received a parcel with a bitmap that did not map to any locations",
                            Helpers.LogLevel.Warning);
                    }
                }

                if (sequenceID.Equals(int.MaxValue) && WaitForSimParcel != null)
                    WaitForSimParcel.Set();

                //// auto request acl, will be stored in parcel tracking dictionary if enabled
                //if (Client.Settings.ALWAYS_REQUEST_PARCEL_ACL)
                //    Client.Parcels.RequestParcelAccessList(simulator, parcel.LocalID,
                //        AccessList.Both, sequenceID);

                //// auto request dwell, will be stored in parcel tracking dictionary if enables
                //if (Client.Settings.ALWAYS_REQUEST_PARCEL_DWELL)
                //    Client.Parcels.RequestDwell(simulator, parcel.LocalID);

                // Fire the callback for parcel properties being received
                if (m_ParcelProperties != null)
                {
                    OnParcelProperties(new ParcelPropertiesEventArgs(simulator, parcel, result, selectedPrims, sequenceID, snapSelection));
                }

                // Check if all of the simulator parcels have been retrieved, if so fire another callback
                if (simulator.IsParcelMapFull() && m_SimParcelsDownloaded != null)
                {
                    OnSimParcelsDownloaded(new SimParcelsDownloadedEventArgs(simulator, simulator.Parcels, simulator.ParcelMap));
                }
            }
        }

        /// <summary>Process an incoming packet and raise the appropriate events</summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">The EventArgs object containing the packet data</param>
        /// <remarks>Raises the <see cref="ParcelAccessListReply"/> event</remarks>
        protected Packet ParcelAccessListReplyHandler(Packet packet, RegionProxy simulator)
        {
            //if (m_ParcelACL != null || Client.Settings.ALWAYS_REQUEST_PARCEL_ACL == true)
            if (m_ParcelACL != null)
            {
                ParcelAccessListReplyPacket reply = (ParcelAccessListReplyPacket)packet;

                List<ParcelAccessEntry> accessList = new List<ParcelAccessEntry>(reply.List.Length);

                for (int i = 0; i < reply.List.Length; i++)
                {
                    ParcelAccessEntry pae = new ParcelAccessEntry();
                    pae.AgentID = reply.List[i].ID;
                    pae.Time = Utils.UnixTimeToDateTime((uint)reply.List[i].Time);
                    pae.Flags = (AccessList)reply.List[i].Flags;

                    accessList.Add(pae);
                }

                lock (simulator.Parcels.Dictionary)
                {
                    if (simulator.Parcels.Dictionary.ContainsKey(reply.Data.LocalID))
                    {
                        Parcel parcel = simulator.Parcels.Dictionary[reply.Data.LocalID];
                        if ((AccessList)reply.Data.Flags == AccessList.Ban)
                            parcel.AccessBlackList = accessList;
                        else
                            parcel.AccessWhiteList = accessList;

                        simulator.Parcels.Dictionary[reply.Data.LocalID] = parcel;
                    }
                }


                if (m_ParcelACL != null)
                {
                    OnParcelAccessListReply(new ParcelAccessListReplyEventArgs(simulator, reply.Data.SequenceID, reply.Data.LocalID,
                        reply.Data.Flags, accessList));
                }
            }

            return packet;
        }

        protected void ParcelObjectOwnersReplyHandler(string capsKey, IMessage message, RegionProxy simulator)
        {
            if (m_ParcelObjectOwnersReply != null)
            {
                List<ParcelPrimOwners> primOwners = new List<ParcelPrimOwners>();

                ParcelObjectOwnersReplyMessage msg = (ParcelObjectOwnersReplyMessage)message;

                for (int i = 0; i < msg.PrimOwnersBlock.Length; i++)
                {
                    ParcelPrimOwners primOwner = new ParcelPrimOwners();
                    primOwner.OwnerID = msg.PrimOwnersBlock[i].OwnerID;
                    primOwner.Count = msg.PrimOwnersBlock[i].Count;
                    primOwner.IsGroupOwned = msg.PrimOwnersBlock[i].IsGroupOwned;
                    primOwner.OnlineStatus = msg.PrimOwnersBlock[i].OnlineStatus;
                    primOwner.NewestPrim = msg.PrimOwnersBlock[i].TimeStamp;

                    primOwners.Add(primOwner);
                }

                OnParcelObjectOwnersReply(new ParcelObjectOwnersReplyEventArgs(simulator, primOwners));
            }
        }

        /// <summary>Process an incoming packet and raise the appropriate events</summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">The EventArgs object containing the packet data</param>
        /// <remarks>Raises the <see cref="ForceSelectObjectsReply"/> event</remarks>
        protected Packet SelectParcelObjectsReplyHandler(Packet packet, RegionProxy simulator)
        {
            if (m_ForceSelectObjects != null)
            {
                ForceObjectSelectPacket reply = (ForceObjectSelectPacket)packet;
                List<uint> objectIDs = new List<uint>(reply.Data.Length);

                for (int i = 0; i < reply.Data.Length; i++)
                {
                    objectIDs.Add(reply.Data[i].LocalID);
                }

                OnForceSelectObjectsReply(new ForceSelectObjectsReplyEventArgs(simulator, objectIDs, reply._Header.ResetList));
            }

            return packet;
        }

        /// <summary>Process an incoming packet and raise the appropriate events</summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">The EventArgs object containing the packet data</param>
        /// <remarks>Raises the <see cref="ParcelMediaUpdateReply"/> event</remarks>
        protected Packet ParcelMediaUpdateHandler(Packet packet, RegionProxy simulator)
        {
            if (m_ParcelMediaUpdateReply != null)
            {
                ParcelMediaUpdatePacket reply = (ParcelMediaUpdatePacket)packet;
                ParcelMedia media = new ParcelMedia();

                media.MediaAutoScale = (reply.DataBlock.MediaAutoScale == (byte)0x1) ? true : false;
                media.MediaID = reply.DataBlock.MediaID;
                media.MediaDesc = Utils.BytesToString(reply.DataBlockExtended.MediaDesc);
                media.MediaHeight = reply.DataBlockExtended.MediaHeight;
                media.MediaLoop = ((reply.DataBlockExtended.MediaLoop & 1) != 0) ? true : false;
                media.MediaType = Utils.BytesToString(reply.DataBlockExtended.MediaType);
                media.MediaWidth = reply.DataBlockExtended.MediaWidth;
                media.MediaURL = Utils.BytesToString(reply.DataBlock.MediaURL);

                OnParcelMediaUpdateReply(new ParcelMediaUpdateReplyEventArgs(simulator, media));
            }

            return packet;
        }

        /// <summary>Process an incoming packet and raise the appropriate events</summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">The EventArgs object containing the packet data</param>
        protected Packet ParcelOverlayHandler(Packet packet, RegionProxy simulator)
        {
            const int OVERLAY_COUNT = 4;

            ParcelOverlayPacket overlay = (ParcelOverlayPacket)packet;

            if (overlay.ParcelData.SequenceID >= 0 && overlay.ParcelData.SequenceID < OVERLAY_COUNT)
            {
                int length = overlay.ParcelData.Data.Length;

                Buffer.BlockCopy(overlay.ParcelData.Data, 0, simulator.ParcelOverlay,
                    overlay.ParcelData.SequenceID * length, length);
                simulator.ParcelOverlaysReceived++;

                if (simulator.ParcelOverlaysReceived >= OVERLAY_COUNT)
                {
                    // TODO: ParcelOverlaysReceived should become internal, and reset to zero every 
                    // time it hits four. Also need a callback here
                }
            }
            else
            {
                Logger.Log("Parcel overlay with sequence ID of " + overlay.ParcelData.SequenceID +
                    " received from " + simulator.ToString(), Helpers.LogLevel.Warning);
            }

            return packet;
        }

        /// <summary>Process an incoming packet and raise the appropriate events</summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">The EventArgs object containing the packet data</param>
        /// <remarks>Raises the <see cref="ParcelMediaCommand"/> event</remarks>
        protected Packet ParcelMediaCommandMessagePacketHandler(Packet packet, RegionProxy simulator)
        {
            if (m_ParcelMediaCommand != null)
            {
                ParcelMediaCommandMessagePacket pmc = (ParcelMediaCommandMessagePacket)packet;
                ParcelMediaCommandMessagePacket.CommandBlockBlock block = pmc.CommandBlock;

                OnParcelMediaCommand(new ParcelMediaCommandEventArgs(simulator, pmc.Header.Sequence, (ParcelFlags)block.Flags,
                    (ParcelMediaCommand)block.Command, block.Time));
            }

            return packet;
        }

        #endregion Packet Handlers
    }
    #region EventArgs classes

    /// <summary>Contains a parcels dwell data returned from the simulator in response to an <see cref="RequestParcelDwell"/></summary>
    public class ParcelDwellReplyEventArgs : EventArgs
    {
        private readonly UUID m_ParcelID;
        private readonly int m_LocalID;
        private readonly float m_Dwell;

        /// <summary>Get the global ID of the parcel</summary>
        public UUID ParcelID { get { return m_ParcelID; } }
        /// <summary>Get the simulator specific ID of the parcel</summary>
        public int LocalID { get { return m_LocalID; } }
        /// <summary>Get the calculated dwell</summary>
        public float Dwell { get { return m_Dwell; } }

        /// <summary>
        /// Construct a new instance of the ParcelDwellReplyEventArgs class
        /// </summary>
        /// <param name="parcelID">The global ID of the parcel</param>
        /// <param name="localID">The simulator specific ID of the parcel</param>
        /// <param name="dwell">The calculated dwell for the parcel</param>
        public ParcelDwellReplyEventArgs(UUID parcelID, int localID, float dwell)
        {
            this.m_ParcelID = parcelID;
            this.m_LocalID = localID;
            this.m_Dwell = dwell;
        }
    }

    /// <summary>Contains basic parcel information data returned from the 
    /// simulator in response to an <see cref="RequestParcelInfo"/> request</summary>
    public class ParcelInfoReplyEventArgs : EventArgs
    {
        private readonly ParcelInfo m_Parcel;

        /// <summary>Get the <see cref="ParcelInfo"/> object containing basic parcel info</summary>
        public ParcelInfo Parcel { get { return m_Parcel; } }

        /// <summary>
        /// Construct a new instance of the ParcelInfoReplyEventArgs class
        /// </summary>
        /// <param name="parcel">The <see cref="ParcelInfo"/> object containing basic parcel info</param>
        public ParcelInfoReplyEventArgs(ParcelInfo parcel)
        {
            this.m_Parcel = parcel;
        }
    }

    /// <summary>Contains basic parcel information data returned from the simulator in response to an <see cref="RequestParcelInfo"/> request</summary>
    public class ParcelPropertiesEventArgs : EventArgs
    {
        private readonly RegionProxy m_Simulator;
        private Parcel m_Parcel;
        private readonly ParcelResult m_Result;
        private readonly int m_SelectedPrims;
        private readonly int m_SequenceID;
        private readonly bool m_SnapSelection;

        /// <summary>Get the simulator the parcel is located in</summary>
        public RegionProxy Simulator { get { return m_Simulator; } }
        /// <summary>Get the <see cref="Parcel"/> object containing the details</summary>
        /// <remarks>If Result is NoData, this object will not contain valid data</remarks>
        public Parcel Parcel { get { return m_Parcel; } }
        /// <summary>Get the result of the request</summary>
        public ParcelResult Result { get { return m_Result; } }
        /// <summary>Get the number of primitieves your agent is 
        /// currently selecting and or sitting on in this parcel</summary>
        public int SelectedPrims { get { return m_SelectedPrims; } }
        /// <summary>Get the user assigned ID used to correlate a request with
        /// these results</summary>
        public int SequenceID { get { return m_SequenceID; } }
        /// <summary>TODO:</summary>
        public bool SnapSelection { get { return m_SnapSelection; } }

        /// <summary>
        /// Construct a new instance of the ParcelPropertiesEventArgs class
        /// </summary>
        /// <param name="simulator">The <see cref="Parcel"/> object containing the details</param>
        /// <param name="parcel">The <see cref="Parcel"/> object containing the details</param>
        /// <param name="result">The result of the request</param>
        /// <param name="selectedPrims">The number of primitieves your agent is 
        /// currently selecting and or sitting on in this parcel</param>
        /// <param name="sequenceID">The user assigned ID used to correlate a request with
        /// these results</param>
        /// <param name="snapSelection">TODO:</param>
        public ParcelPropertiesEventArgs(RegionProxy simulator, Parcel parcel, ParcelResult result, int selectedPrims,
            int sequenceID, bool snapSelection)
        {
            this.m_Simulator = simulator;
            this.m_Parcel = parcel;
            this.m_Result = result;
            this.m_SelectedPrims = selectedPrims;
            this.m_SequenceID = sequenceID;
            this.m_SnapSelection = snapSelection;
        }
    }

    /// <summary>Contains blacklist and whitelist data returned from the simulator in response to an <see cref="RequestParcelAccesslist"/> request</summary>
    public class ParcelAccessListReplyEventArgs : EventArgs
    {
        private readonly RegionProxy m_Simulator;
        private readonly int m_SequenceID;
        private readonly int m_LocalID;
        private readonly uint m_Flags;
        private readonly List<ParcelManager.ParcelAccessEntry> m_AccessList;

        /// <summary>Get the simulator the parcel is located in</summary>
        public RegionProxy Simulator { get { return m_Simulator; } }
        /// <summary>Get the user assigned ID used to correlate a request with
        /// these results</summary>
        public int SequenceID { get { return m_SequenceID; } }
        /// <summary>Get the simulator specific ID of the parcel</summary>
        public int LocalID { get { return m_LocalID; } }
        /// <summary>TODO:</summary>
        public uint Flags { get { return m_Flags; } }
        /// <summary>Get the list containing the white/blacklisted agents for the parcel</summary>
        public List<ParcelManager.ParcelAccessEntry> AccessList { get { return m_AccessList; } }

        /// <summary>
        /// Construct a new instance of the ParcelAccessListReplyEventArgs class
        /// </summary>
        /// <param name="simulator">The simulator the parcel is located in</param>
        /// <param name="sequenceID">The user assigned ID used to correlate a request with
        /// these results</param>
        /// <param name="localID">The simulator specific ID of the parcel</param>
        /// <param name="flags">TODO:</param>
        /// <param name="accessEntries">The list containing the white/blacklisted agents for the parcel</param>
        public ParcelAccessListReplyEventArgs(RegionProxy simulator, int sequenceID, int localID, uint flags, List<ParcelManager.ParcelAccessEntry> accessEntries)
        {
            this.m_Simulator = simulator;
            this.m_SequenceID = sequenceID;
            this.m_LocalID = localID;
            this.m_Flags = flags;
            this.m_AccessList = accessEntries;
        }
    }

    /// <summary>Contains blacklist and whitelist data returned from the 
    /// simulator in response to an <see cref="RequestParcelAccesslist"/> request</summary>
    public class ParcelObjectOwnersReplyEventArgs : EventArgs
    {
        private readonly RegionProxy m_Simulator;
        private readonly List<ParcelManager.ParcelPrimOwners> m_Owners;

        /// <summary>Get the simulator the parcel is located in</summary>
        public RegionProxy Simulator { get { return m_Simulator; } }
        /// <summary>Get the list containing prim ownership counts</summary>
        public List<ParcelManager.ParcelPrimOwners> PrimOwners { get { return m_Owners; } }

        /// <summary>
        /// Construct a new instance of the ParcelObjectOwnersReplyEventArgs class
        /// </summary>
        /// <param name="simulator">The simulator the parcel is located in</param>
        /// <param name="primOwners">The list containing prim ownership counts</param>
        public ParcelObjectOwnersReplyEventArgs(RegionProxy simulator, List<ParcelManager.ParcelPrimOwners> primOwners)
        {
            this.m_Simulator = simulator;
            this.m_Owners = primOwners;
        }
    }

    /// <summary>Contains the data returned when all parcel data has been retrieved from a simulator</summary>
    public class SimParcelsDownloadedEventArgs : EventArgs
    {
        private readonly RegionProxy m_Simulator;
        private readonly InternalDictionary<int, Parcel> m_Parcels;
        private readonly int[,] m_ParcelMap;

        /// <summary>Get the simulator the parcel data was retrieved from</summary>
        public RegionProxy Simulator { get { return m_Simulator; } }
        /// <summary>A dictionary containing the parcel data where the key correlates to the ParcelMap entry</summary>
        public InternalDictionary<int, Parcel> Parcels { get { return m_Parcels; } }
        /// <summary>Get the multidimensional array containing a x,y grid mapped
        /// to each 64x64 parcel's LocalID.</summary>
        public int[,] ParcelMap { get { return m_ParcelMap; } }

        /// <summary>
        /// Construct a new instance of the SimParcelsDownloadedEventArgs class
        /// </summary>
        /// <param name="simulator">The simulator the parcel data was retrieved from</param>
        /// <param name="simParcels">The dictionary containing the parcel data</param>
        /// <param name="parcelMap">The multidimensional array containing a x,y grid mapped
        /// to each 64x64 parcel's LocalID.</param>
        public SimParcelsDownloadedEventArgs(RegionProxy simulator, InternalDictionary<int, Parcel> simParcels, int[,] parcelMap)
        {
            this.m_Simulator = simulator;
            this.m_Parcels = simParcels;
            this.m_ParcelMap = parcelMap;
        }
    }

    /// <summary>Contains the data returned when a <see cref="RequestForceSelectObjects"/> request</summary>
    public class ForceSelectObjectsReplyEventArgs : EventArgs
    {
        private readonly RegionProxy m_Simulator;
        private readonly List<uint> m_ObjectIDs;
        private readonly bool m_ResetList;

        /// <summary>Get the simulator the parcel data was retrieved from</summary>
        public RegionProxy Simulator { get { return m_Simulator; } }
        /// <summary>Get the list of primitive IDs</summary>
        public List<uint> ObjectIDs { get { return m_ObjectIDs; } }
        /// <summary>true if the list is clean and contains the information
        /// only for a given request</summary>
        public bool ResetList { get { return m_ResetList; } }

        /// <summary>
        /// Construct a new instance of the ForceSelectObjectsReplyEventArgs class
        /// </summary>
        /// <param name="simulator">The simulator the parcel data was retrieved from</param>
        /// <param name="objectIDs">The list of primitive IDs</param>
        /// <param name="resetList">true if the list is clean and contains the information
        /// only for a given request</param>
        public ForceSelectObjectsReplyEventArgs(RegionProxy simulator, List<uint> objectIDs, bool resetList)
        {
            this.m_Simulator = simulator;
            this.m_ObjectIDs = objectIDs;
            this.m_ResetList = resetList;
        }
    }

    /// <summary>Contains data when the media data for a parcel the avatar is on changes</summary>
    public class ParcelMediaUpdateReplyEventArgs : EventArgs
    {
        private readonly RegionProxy m_Simulator;
        private readonly ParcelMedia m_ParcelMedia;

        /// <summary>Get the simulator the parcel media data was updated in</summary>
        public RegionProxy Simulator { get { return m_Simulator; } }
        /// <summary>Get the updated media information</summary>
        public ParcelMedia Media { get { return m_ParcelMedia; } }

        /// <summary>
        /// Construct a new instance of the ParcelMediaUpdateReplyEventArgs class
        /// </summary>
        /// <param name="simulator">the simulator the parcel media data was updated in</param>
        /// <param name="media">The updated media information</param>
        public ParcelMediaUpdateReplyEventArgs(RegionProxy simulator, ParcelMedia media)
        {
            this.m_Simulator = simulator;
            this.m_ParcelMedia = media;
        }
    }

    /// <summary>Contains the media command for a parcel the agent is currently on</summary>
    public class ParcelMediaCommandEventArgs : EventArgs
    {
        private readonly RegionProxy m_Simulator;
        private readonly uint m_Sequence;
        private readonly ParcelFlags m_ParcelFlags;
        private readonly ParcelMediaCommand m_MediaCommand;
        private readonly float m_Time;

        /// <summary>Get the simulator the parcel media command was issued in</summary>
        public RegionProxy Simulator { get { return m_Simulator; } }
        /// <summary></summary>
        public uint Sequence { get { return m_Sequence; } }
        /// <summary></summary>
        public ParcelFlags ParcelFlags { get { return m_ParcelFlags; } }
        /// <summary>Get the media command that was sent</summary>
        public ParcelMediaCommand MediaCommand { get { return m_MediaCommand; } }
        /// <summary></summary>
        public float Time { get { return m_Time; } }

        /// <summary>
        /// Construct a new instance of the ParcelMediaCommandEventArgs class
        /// </summary>
        /// <param name="simulator">The simulator the parcel media command was issued in</param>
        /// <param name="sequence"></param>
        /// <param name="flags"></param>
        /// <param name="command">The media command that was sent</param>
        /// <param name="time"></param>
        public ParcelMediaCommandEventArgs(RegionProxy simulator, uint sequence, ParcelFlags flags, ParcelMediaCommand command, float time)
        {
            this.m_Simulator = simulator;
            this.m_Sequence = sequence;
            this.m_ParcelFlags = flags;
            this.m_MediaCommand = command;
            this.m_Time = time;
        }
    }
    #endregion
}
