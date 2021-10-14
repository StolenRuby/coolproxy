using Nwc.XmlRpc;
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
    public class AgentManager
    {
        private ProxyFrame Frame;


        public UUID AgentID { get; private set; } = UUID.Zero;
        public UUID SessionID { get; private set; } = UUID.Zero;
        public UUID SecureSessionID { get; private set; } = UUID.Zero;
        public string FirstName { get; private set; } = string.Empty;
        public string LastName { get; private set; } = string.Empty;
        public UUID InventoryRoot { get; private set; }

        internal uint localID;
        internal Vector3 relativePosition;
        internal Quaternion relativeRotation = Quaternion.Identity;
        internal Vector4 collisionPlane;
        internal Vector3 velocity;
        internal Vector3 acceleration;
        internal Vector3 angularVelocity;
        internal uint sittingOn;
        //internal int lastInterpolation;

        public Vector3 RelativePosition { get { return relativePosition; } set { relativePosition = value; } }
        /// <summary>Current rotation of the agent as a relative rotation from
        /// the simulator, or the parent object if we are sitting on something</summary>
        public Quaternion RelativeRotation { get { return relativeRotation; } set { relativeRotation = value; } }
        /// <summary>Current position of the agent in the simulator</summary>
        public Vector3 SimPosition
        {
            get
            {
                // simple case, agent not seated
                if (sittingOn == 0)
                {
                    return relativePosition;
                }

                // a bit more complicatated, agent sitting on a prim
                Primitive p = null;
                Vector3 fullPosition = relativePosition;

                if (Frame.Network.CurrentSim.ObjectsPrimitives.TryGetValue(sittingOn, out p))
                {
                    fullPosition = p.Position + relativePosition * p.Rotation;
                }

                // go up the hiearchy trying to find the root prim
                while (p != null && p.ParentID != 0)
                {
                    Avatar av;
                    if (Frame.Network.CurrentSim.ObjectsAvatars.TryGetValue(p.ParentID, out av))
                    {
                        p = av;
                        fullPosition += p.Position;
                    }
                    else
                    {
                        if (Frame.Network.CurrentSim.ObjectsPrimitives.TryGetValue(p.ParentID, out p))
                        {
                            fullPosition += p.Position;
                        }
                    }
                }

                if (p != null) // we found the root prim
                {
                    return fullPosition;
                }

                // Didn't find the seat's root prim, try returning coarse loaction
                if (Frame.Network.CurrentSim.avatarPositions.TryGetValue(AgentID, out fullPosition))
                {
                    return fullPosition;
                }

                OpenMetaverse.Logger.Log("Failed to determine agents sim position", Helpers.LogLevel.Warning);

                return relativePosition;
            }
        }
        /// <summary>
        /// A <seealso cref="Quaternion"/> representing the agents current rotation
        /// </summary>
        public Quaternion SimRotation
        {
            get
            {
                if (sittingOn != 0)
                {
                    Primitive parent;
                    if (Frame.Network.CurrentSim != null && Frame.Network.CurrentSim.ObjectsPrimitives.TryGetValue(sittingOn, out parent))
                    {
                        return relativeRotation * parent.Rotation;
                    }
                    else
                    {
                        OpenMetaverse.Logger.Log("Currently sitting on object " + sittingOn + " which is not tracked, SimRotation will be inaccurate",
                            Helpers.LogLevel.Warning);
                        return relativeRotation;
                    }
                }
                else
                {
                    return relativeRotation;
                }
            }
        }

        private string _name;
        public string Name
        {
            get
            {
                if (_name == null)
                {
                    _name = FirstName;
                    if (LastName.ToLower() != "resident")
                        _name += " " + LastName;
                }
                return _name;
            }
        }


        internal UUID CurrentSelection = UUID.Zero;

        internal List<uint> SelectedLocalIDs = new List<uint>();

        public uint[] Selection
        {
            get
            {
                return SelectedLocalIDs.ToArray();
            }
        }

        public void ClearSelection()
        {
            SelectedLocalIDs.Clear();
        }


        public AgentManager(ProxyFrame frame)
        {
            Frame = frame;

            Frame.Login.AddLoginResponseDelegate(HandleLoginResponse);

            Frame.Network.AddDelegate(PacketType.ViewerEffect, Direction.Outgoing, ViewerEffectHandle);

            Frame.Network.AddDelegate(PacketType.ObjectSelect, Direction.Outgoing, ObjectSelectHandle);
            Frame.Network.AddDelegate(PacketType.ObjectDeselect, Direction.Outgoing, ObjectDeselectHandle);
        }

        private void HandleLoginResponse(XmlRpcResponse response)
        {

            System.Collections.Hashtable values = (System.Collections.Hashtable)response.Value;
            if (values.ContainsKey("login"))
            {
                if((string)values["login"] != "true")
                {
                    return;
                }
            }
            else return;

            if (values.Contains("agent_id"))
                AgentID = new UUID((string)values["agent_id"]);
            if (values.Contains("session_id"))
                SessionID = new UUID((string)values["session_id"]);
            if (values.Contains("secure_session_id"))
                SecureSessionID = new UUID((string)values["secure_session_id"]);

            if (values.Contains("first_name"))
                FirstName = (string)values["first_name"];
            if (values.Contains("last_name"))
                LastName = (string)values["last_name"];

            if (values.Contains("inventory-root"))
                InventoryRoot = new UUID((string)((System.Collections.Hashtable)(((System.Collections.ArrayList)values["inventory-root"])[0]))["folder_id"]);
        }


        private Packet ObjectSelectHandle(Packet packet, RegionProxy region)
        {
            ObjectSelectPacket select = packet as ObjectSelectPacket;
            foreach (ObjectSelectPacket.ObjectDataBlock block in select.ObjectData)
            {
                if (SelectedLocalIDs.Contains(block.ObjectLocalID) == false)
                    SelectedLocalIDs.Add(block.ObjectLocalID);
            }
            //Frame.SayToUser("Items selected: " + SelectedLocalIDs.Count.ToString());
            return packet;
        }

        private Packet ObjectDeselectHandle(Packet packet, RegionProxy region)
        {
            ObjectDeselectPacket select = packet as ObjectDeselectPacket;
            foreach (ObjectDeselectPacket.ObjectDataBlock block in select.ObjectData)
            {
                if (SelectedLocalIDs.Contains(block.ObjectLocalID))
                    SelectedLocalIDs.Remove(block.ObjectLocalID);
            }
            //Frame.SayToUser("Items selected: " + SelectedLocalIDs.Count.ToString());
            return packet;
        }


        /// <summary>
        /// shitty thing for tracking what object the user is selecting
        /// </summary>
        /// <param name="packet"></param>
        /// <param name="region"></param>
        /// <returns></returns>
        private Packet ViewerEffectHandle(Packet packet, RegionProxy region)
        {
            ViewerEffectPacket effect = packet as ViewerEffectPacket;
            if (effect.AgentData.AgentID == Frame.Agent.AgentID)
            {
                foreach (ViewerEffectPacket.EffectBlock block in effect.Effect)
                {
                    if (block.Type == (byte)EffectType.PointAt)
                    {
                        if (block.TypeData.Length == 57)
                        {
                            UUID sourceAvatar = new UUID(block.TypeData, 0);
                            UUID targetObject = new UUID(block.TypeData, 16);
                            Vector3d targetPos = new Vector3d(block.TypeData, 32);
                            PointAtType pointAt = (PointAtType)block.TypeData[56];

                            CurrentSelection = targetObject;
                        }
                    }
                }
            }

            return packet;
        }


        #region Touch and grab

        /// <summary>
        /// Grabs an object
        /// </summary>
        /// <param name="objectLocalID">an unsigned integer of the objects ID within the simulator</param>
        /// <seealso cref="Simulator.ObjectsPrimitives"/>
        public void Grab(uint objectLocalID)
        {
            Grab(objectLocalID, Vector3.Zero, Vector3.Zero, Vector3.Zero, 0, Vector3.Zero, Vector3.Zero, Vector3.Zero);
        }

        /// <summary>
        /// Overload: Grab a simulated object
        /// </summary>
        /// <param name="objectLocalID">an unsigned integer of the objects ID within the simulator</param>
        /// <param name="grabOffset"></param>
        /// <param name="uvCoord">The texture coordinates to grab</param>
        /// <param name="stCoord">The surface coordinates to grab</param>
        /// <param name="faceIndex">The face of the position to grab</param>
        /// <param name="position">The region coordinates of the position to grab</param>
        /// <param name="normal">The surface normal of the position to grab (A normal is a vector perpindicular to the surface)</param>
        /// <param name="binormal">The surface binormal of the position to grab (A binormal is a vector tangen to the surface
        /// pointing along the U direction of the tangent space</param>
        public void Grab(uint objectLocalID, Vector3 grabOffset, Vector3 uvCoord, Vector3 stCoord, int faceIndex, Vector3 position,
            Vector3 normal, Vector3 binormal)
        {
            ObjectGrabPacket grab = new ObjectGrabPacket();

            grab.AgentData.AgentID = Frame.Agent.AgentID;
            grab.AgentData.SessionID = Frame.Agent.SessionID;

            grab.ObjectData.LocalID = objectLocalID;
            grab.ObjectData.GrabOffset = grabOffset;

            grab.SurfaceInfo = new ObjectGrabPacket.SurfaceInfoBlock[1];
            grab.SurfaceInfo[0] = new ObjectGrabPacket.SurfaceInfoBlock();
            grab.SurfaceInfo[0].UVCoord = uvCoord;
            grab.SurfaceInfo[0].STCoord = stCoord;
            grab.SurfaceInfo[0].FaceIndex = faceIndex;
            grab.SurfaceInfo[0].Position = position;
            grab.SurfaceInfo[0].Normal = normal;
            grab.SurfaceInfo[0].Binormal = binormal;

            Frame.Network.InjectPacket(grab, Direction.Outgoing);
        }

        /// <summary>
        /// Drag an object
        /// </summary>
        /// <param name="objectID"><seealso cref="UUID"/> of the object to drag</param>
        /// <param name="grabPosition">Drag target in region coordinates</param>
        public void GrabUpdate(UUID objectID, Vector3 grabPosition)
        {
            GrabUpdate(objectID, grabPosition, Vector3.Zero, Vector3.Zero, Vector3.Zero, 0, Vector3.Zero, Vector3.Zero, Vector3.Zero);
        }

        /// <summary>
        /// Overload: Drag an object
        /// </summary>
        /// <param name="objectID"><seealso cref="UUID"/> of the object to drag</param>
        /// <param name="grabPosition">Drag target in region coordinates</param>
        /// <param name="grabOffset"></param>
        /// <param name="uvCoord">The texture coordinates to grab</param>
        /// <param name="stCoord">The surface coordinates to grab</param>
        /// <param name="faceIndex">The face of the position to grab</param>
        /// <param name="position">The region coordinates of the position to grab</param>
        /// <param name="normal">The surface normal of the position to grab (A normal is a vector perpindicular to the surface)</param>
        /// <param name="binormal">The surface binormal of the position to grab (A binormal is a vector tangen to the surface
        /// pointing along the U direction of the tangent space</param>
        public void GrabUpdate(UUID objectID, Vector3 grabPosition, Vector3 grabOffset, Vector3 uvCoord, Vector3 stCoord, int faceIndex, Vector3 position,
            Vector3 normal, Vector3 binormal)
        {
            ObjectGrabUpdatePacket grab = new ObjectGrabUpdatePacket();
            grab.AgentData.AgentID = Frame.Agent.AgentID;
            grab.AgentData.SessionID = Frame.Agent.SessionID;

            grab.ObjectData.ObjectID = objectID;
            grab.ObjectData.GrabOffsetInitial = grabOffset;
            grab.ObjectData.GrabPosition = grabPosition;
            grab.ObjectData.TimeSinceLast = 0;

            grab.SurfaceInfo = new ObjectGrabUpdatePacket.SurfaceInfoBlock[1];
            grab.SurfaceInfo[0] = new ObjectGrabUpdatePacket.SurfaceInfoBlock();
            grab.SurfaceInfo[0].UVCoord = uvCoord;
            grab.SurfaceInfo[0].STCoord = stCoord;
            grab.SurfaceInfo[0].FaceIndex = faceIndex;
            grab.SurfaceInfo[0].Position = position;
            grab.SurfaceInfo[0].Normal = normal;
            grab.SurfaceInfo[0].Binormal = binormal;

            Frame.Network.InjectPacket(grab, Direction.Outgoing);
        }

        /// <summary>
        /// Release a grabbed object
        /// </summary>
        /// <param name="objectLocalID">The Objects Simulator Local ID</param>
        /// <seealso cref="Simulator.ObjectsPrimitives"/>
        /// <seealso cref="Grab"/>
        /// <seealso cref="GrabUpdate"/>
        public void DeGrab(uint objectLocalID)
        {
            DeGrab(objectLocalID, Vector3.Zero, Vector3.Zero, 0, Vector3.Zero, Vector3.Zero, Vector3.Zero);
        }

        /// <summary>
        /// Release a grabbed object
        /// </summary>
        /// <param name="objectLocalID">The Objects Simulator Local ID</param>
        /// <param name="uvCoord">The texture coordinates to grab</param>
        /// <param name="stCoord">The surface coordinates to grab</param>
        /// <param name="faceIndex">The face of the position to grab</param>
        /// <param name="position">The region coordinates of the position to grab</param>
        /// <param name="normal">The surface normal of the position to grab (A normal is a vector perpindicular to the surface)</param>
        /// <param name="binormal">The surface binormal of the position to grab (A binormal is a vector tangen to the surface
        /// pointing along the U direction of the tangent space</param>
        public void DeGrab(uint objectLocalID, Vector3 uvCoord, Vector3 stCoord, int faceIndex, Vector3 position,
            Vector3 normal, Vector3 binormal)
        {
            ObjectDeGrabPacket degrab = new ObjectDeGrabPacket();
            degrab.AgentData.AgentID = Frame.Agent.AgentID;
            degrab.AgentData.SessionID = Frame.Agent.SessionID;

            degrab.ObjectData.LocalID = objectLocalID;

            degrab.SurfaceInfo = new ObjectDeGrabPacket.SurfaceInfoBlock[1];
            degrab.SurfaceInfo[0] = new ObjectDeGrabPacket.SurfaceInfoBlock();
            degrab.SurfaceInfo[0].UVCoord = uvCoord;
            degrab.SurfaceInfo[0].STCoord = stCoord;
            degrab.SurfaceInfo[0].FaceIndex = faceIndex;
            degrab.SurfaceInfo[0].Position = position;
            degrab.SurfaceInfo[0].Normal = normal;
            degrab.SurfaceInfo[0].Binormal = binormal;

            Frame.Network.InjectPacket(degrab, Direction.Outgoing);
        }

        /// <summary>
        /// Touches an object
        /// </summary>
        /// <param name="objectLocalID">an unsigned integer of the objects ID within the simulator</param>
        /// <seealso cref="Simulator.ObjectsPrimitives"/>
        public void Touch(uint objectLocalID)
        {
            Frame.Agent.Grab(objectLocalID);
            Frame.Agent.DeGrab(objectLocalID);
        }

        #endregion Touch and grab

        #region Teleporting

        /// <summary>
        /// Teleports agent to their stored home location
        /// </summary>
        /// <returns>true on successful teleport to home location</returns>
        public void GoHome()
        {
            Teleport(UUID.Zero);
        }

        /// <summary>
        /// Teleport agent to a landmark
        /// </summary>
        /// <param name="landmark"><seealso cref="UUID"/> of the landmark to teleport agent to</param>
        /// <returns>true on success, false on failure</returns>
        public void Teleport(UUID landmark)
        {
            //teleportStat = TeleportStatus.None;
            //teleportEvent.Reset();
            TeleportLandmarkRequestPacket p = new TeleportLandmarkRequestPacket();
            p.Info = new TeleportLandmarkRequestPacket.InfoBlock();
            p.Info.AgentID = Frame.Agent.AgentID;
            p.Info.SessionID = Frame.Agent.SessionID;
            p.Info.LandmarkID = landmark;
            Frame.Network.InjectPacket(p, Direction.Outgoing);

            //teleportEvent.WaitOne(Client.Settings.TELEPORT_TIMEOUT, false);

            //if (teleportStat == TeleportStatus.None ||
            //    teleportStat == TeleportStatus.Start ||
            //    teleportStat == TeleportStatus.Progress)
            //{
            //    teleportMessage = "Teleport timed out.";
            //    teleportStat = TeleportStatus.Failed;
            //}

            //return (teleportStat == TeleportStatus.Finished);
        }

        /// <summary>
        /// Attempt to look up a simulator name and teleport to the discovered
        /// destination
        /// </summary>
        /// <param name="simName">Region name to look up</param>
        /// <param name="position">Position to teleport to</param>
        /// <returns>True if the lookup and teleport were successful, otherwise
        /// false</returns>
        public void Teleport(string simName, Vector3 position)
        {
            Teleport(simName, position, new Vector3(0, 1.0f, 0));
        }

        /// <summary>
        /// Attempt to look up a simulator name and teleport to the discovered
        /// destination
        /// </summary>
        /// <param name="simName">Region name to look up</param>
        /// <param name="position">Position to teleport to</param>
        /// <param name="lookAt">Target to look at</param>
        /// <returns>True if the lookup and teleport were successful, otherwise
        /// false</returns>
        public void Teleport(string simName, Vector3 position, Vector3 lookAt)
        {
            if (Frame.Network.CurrentSim == null)
                return;

            //teleportStat = TeleportStatus.None;

            if (simName != Frame.Network.CurrentSim.Name)
            {
                // Teleporting to a foreign sim
                GridRegion region;

                if (Frame.Grid.GetGridRegion(simName, GridLayerType.Objects, out region))
                {
                    Teleport(region.RegionHandle, position, lookAt);
                }
            }
            else
            {
                // Teleporting to the sim we're already in
                Teleport(Frame.Network.CurrentSim.Handle, position, lookAt);
            }
        }

        /// <summary>
        /// Teleport agent to another region
        /// </summary>
        /// <param name="regionHandle">handle of region to teleport agent to</param>
        /// <param name="position"><seealso cref="Vector3"/> position in destination sim to teleport to</param>
        /// <returns>true on success, false on failure</returns>
        /// <remarks>This call is blocking</remarks>
        public void Teleport(ulong regionHandle, Vector3 position)
        {
            Teleport(regionHandle, position, new Vector3(0.0f, 1.0f, 0.0f));
        }

        /// <summary>
        /// Teleport agent to another region
        /// </summary>
        /// <param name="regionHandle">handle of region to teleport agent to</param>
        /// <param name="position"><seealso cref="Vector3"/> position in destination sim to teleport to</param>
        /// <param name="lookAt"><seealso cref="Vector3"/> direction in destination sim agent will look at</param>
        /// <returns>true on success, false on failure</returns>
        /// <remarks>This call is blocking</remarks>
        public void Teleport(ulong regionHandle, Vector3 position, Vector3 lookAt)
        {
            RequestTeleport(regionHandle, position, lookAt);
        }

        /// <summary>
        /// Request teleport to a another simulator
        /// </summary>
        /// <param name="regionHandle">handle of region to teleport agent to</param>
        /// <param name="position"><seealso cref="Vector3"/> position in destination sim to teleport to</param>
        public void RequestTeleport(ulong regionHandle, Vector3 position)
        {
            RequestTeleport(regionHandle, position, new Vector3(0.0f, 1.0f, 0.0f));
        }

        /// <summary>
        /// Request teleport to a another simulator
        /// </summary>
        /// <param name="regionHandle">handle of region to teleport agent to</param>
        /// <param name="position"><seealso cref="Vector3"/> position in destination sim to teleport to</param>
        /// <param name="lookAt"><seealso cref="Vector3"/> direction in destination sim agent will look at</param>
        public void RequestTeleport(ulong regionHandle, Vector3 position, Vector3 lookAt)
        {
            if (Frame.Network.CurrentSim != null &&
                Frame.Network.CurrentSim.Caps != null)// &&
               // Client.Network.CurrentSim.Caps.IsEventQueueRunning)
            {
                TeleportLocationRequestPacket teleport = new TeleportLocationRequestPacket();
                teleport.AgentData.AgentID = Frame.Agent.AgentID;
                teleport.AgentData.SessionID = Frame.Agent.SessionID;
                teleport.Info.LookAt = lookAt;
                teleport.Info.Position = position;
                teleport.Info.RegionHandle = regionHandle;

                OpenMetaverse.Logger.Log("Requesting teleport to region handle " + regionHandle.ToString(), Helpers.LogLevel.Info);

                Frame.Network.InjectPacket(teleport, Direction.Outgoing);
            }
            else
            {
                //teleportMessage = "CAPS event queue is not running";
                //teleportEvent.Set();
                //teleportStat = TeleportStatus.Failed;
            }
        }

        /// <summary>
        /// Teleport agent to a landmark
        /// </summary>
        /// <param name="landmark"><seealso cref="UUID"/> of the landmark to teleport agent to</param>
        public void RequestTeleport(UUID landmark)
        {
            TeleportLandmarkRequestPacket p = new TeleportLandmarkRequestPacket();
            p.Info = new TeleportLandmarkRequestPacket.InfoBlock();
            p.Info.AgentID = Frame.Agent.AgentID;
            p.Info.SessionID = Frame.Agent.SessionID;
            p.Info.LandmarkID = landmark;
            Frame.Network.InjectPacket(p, Direction.Outgoing);
        }

        /// <summary>
        /// Send a teleport lure to another avatar with default "Join me in ..." invitation message
        /// </summary>
        /// <param name="targetID">target avatars <seealso cref="UUID"/> to lure</param>
        public void SendTeleportLure(UUID targetID)
        {
            SendTeleportLure(targetID, "Join me in " + Frame.Network.CurrentSim.Name + "!");
        }

        /// <summary>
        /// Send a teleport lure to another avatar with custom invitation message
        /// </summary>
        /// <param name="targetID">target avatars <seealso cref="UUID"/> to lure</param>
        /// <param name="message">custom message to send with invitation</param>
        public void SendTeleportLure(UUID targetID, string message)
        {
            StartLurePacket p = new StartLurePacket();
            p.AgentData.AgentID = Frame.Agent.AgentID;
            p.AgentData.SessionID = Frame.Agent.SessionID;
            p.Info.LureType = 0;
            p.Info.Message = Utils.StringToBytes(message);
            p.TargetData = new StartLurePacket.TargetDataBlock[] { new StartLurePacket.TargetDataBlock() };
            p.TargetData[0].TargetID = targetID;
            Frame.Network.InjectPacket(p, Direction.Outgoing);
        }

        /// <summary>
        /// Respond to a teleport lure by either accepting it and initiating 
        /// the teleport, or denying it
        /// </summary>
        /// <param name="requesterID"><seealso cref="UUID"/> of the avatar sending the lure</param>
        /// <param name="sessionID">IM session <seealso cref="UUID"/> of the incoming lure request</param>
        /// <param name="accept">true to accept the lure, false to decline it</param>
        public void TeleportLureRespond(UUID requesterID, UUID sessionID, bool accept)
        {
            if (accept)
            {
                TeleportLureRequestPacket lure = new TeleportLureRequestPacket();

                lure.Info.AgentID = Frame.Agent.AgentID;
                lure.Info.SessionID = Frame.Agent.SessionID;
                lure.Info.LureID = sessionID;
                lure.Info.TeleportFlags = (uint)TeleportFlags.ViaLure;

                Frame.Network.InjectPacket(lure, Direction.Outgoing);
            }
            else
            {
                InstantMessage(Name, requesterID, String.Empty, sessionID,
                    accept ? InstantMessageDialog.AcceptTeleport : InstantMessageDialog.DenyTeleport,
                    InstantMessageOnline.Offline, this.SimPosition, UUID.Zero, Utils.EmptyBytes);
            }
        }

        #endregion Teleporting



        /////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////
        ///



        /// <summary>
        /// Send an Instant Message to another Avatar
        /// </summary>
        /// <param name="target">The recipients <see cref="UUID"/></param>
        /// <param name="message">A <see cref="string"/> containing the message to send</param>
        public void InstantMessage(UUID target, string message)
        {
            InstantMessage(Name, target, message, AgentID.Equals(target) ? AgentID : target ^ AgentID,
                InstantMessageDialog.MessageFromAgent, InstantMessageOnline.Offline, this.SimPosition,
                UUID.Zero, Utils.EmptyBytes);
        }

        /// <summary>
        /// Send an Instant Message to an existing group chat or conference chat
        /// </summary>
        /// <param name="target">The recipients <see cref="UUID"/></param>
        /// <param name="message">A <see cref="string"/> containing the message to send</param>
        /// <param name="imSessionID">IM session ID (to differentiate between IM windows)</param>
        public void InstantMessage(UUID target, string message, UUID imSessionID)
        {
            InstantMessage(Name, target, message, imSessionID,
                InstantMessageDialog.MessageFromAgent, InstantMessageOnline.Offline, this.SimPosition,
                UUID.Zero, Utils.EmptyBytes);
        }

        /// <summary>
        /// Send an Instant Message
        /// </summary>
        /// <param name="fromName">The name this IM will show up as being from</param>
        /// <param name="target">Key of Avatar</param>
        /// <param name="message">Text message being sent</param>
        /// <param name="imSessionID">IM session ID (to differentiate between IM windows)</param>
        /// <param name="conferenceIDs">IDs of sessions for a conference</param>
        public void InstantMessage(string fromName, UUID target, string message, UUID imSessionID,
            UUID[] conferenceIDs)
        {
            byte[] binaryBucket;

            if (conferenceIDs != null && conferenceIDs.Length > 0)
            {
                binaryBucket = new byte[16 * conferenceIDs.Length];
                for (int i = 0; i < conferenceIDs.Length; ++i)
                    Buffer.BlockCopy(conferenceIDs[i].GetBytes(), 0, binaryBucket, i * 16, 16);
            }
            else
            {
                binaryBucket = Utils.EmptyBytes;
            }

            InstantMessage(fromName, target, message, imSessionID, InstantMessageDialog.MessageFromAgent,
                InstantMessageOnline.Offline, Vector3.Zero, UUID.Zero, binaryBucket);
        }

        /// <summary>
        /// Send an Instant Message
        /// </summary>
        /// <param name="fromName">The name this IM will show up as being from</param>
        /// <param name="target">Key of Avatar</param>
        /// <param name="message">Text message being sent</param>
        /// <param name="imSessionID">IM session ID (to differentiate between IM windows)</param>
        /// <param name="dialog">Type of instant message to send</param>
        /// <param name="offline">Whether to IM offline avatars as well</param>
        /// <param name="position">Senders Position</param>
        /// <param name="regionID">RegionID Sender is In</param>
        /// <param name="binaryBucket">Packed binary data that is specific to
        /// the dialog type</param>
        public void InstantMessage(string fromName, UUID target, string message, UUID imSessionID,
            InstantMessageDialog dialog, InstantMessageOnline offline, Vector3 position, UUID regionID,
            byte[] binaryBucket)
        {
            if (target != UUID.Zero)
            {
                ImprovedInstantMessagePacket im = new ImprovedInstantMessagePacket();

                if (imSessionID.Equals(UUID.Zero) || imSessionID.Equals(AgentID))
                    imSessionID = AgentID.Equals(target) ? AgentID : target ^ AgentID;

                im.AgentData.AgentID = Frame.Agent.AgentID;
                im.AgentData.SessionID = Frame.Agent.SessionID;

                im.MessageBlock.Dialog = (byte)dialog;
                im.MessageBlock.FromAgentName = Utils.StringToBytes(fromName);
                im.MessageBlock.FromGroup = false;
                im.MessageBlock.ID = imSessionID;
                im.MessageBlock.Message = Utils.StringToBytes(message);
                im.MessageBlock.Offline = (byte)offline;
                im.MessageBlock.ToAgentID = target;

                if (binaryBucket != null)
                    im.MessageBlock.BinaryBucket = binaryBucket;
                else
                    im.MessageBlock.BinaryBucket = Utils.EmptyBytes;

                // These fields are mandatory, even if we don't have valid values for them
                im.MessageBlock.Position = Vector3.Zero;
                //TODO: Allow region id to be correctly set by caller or fetched from Client.*
                im.MessageBlock.RegionID = regionID;

                // Send the message
                Frame.Network.CurrentSim.Inject(im, Direction.Outgoing);
            }
            else
            {
                OpenMetaverse.Logger.Log(String.Format("Suppressing instant message \"{0}\" to UUID.Zero", message),
                    Helpers.LogLevel.Error);
            }
        }
    }
}
