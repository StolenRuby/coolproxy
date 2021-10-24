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
using System.Collections.Generic;
using System.Text;
using OpenMetaverse.Http;
using OpenMetaverse.Packets;
using OpenMetaverse.Interfaces;
using OpenMetaverse.Messages.Linden;
using OpenMetaverse.StructuredData;
using OpenMetaverse;
using static GridProxy.RegionManager;
using System.Linq;

namespace GridProxy
{
    /// <summary>
    /// Retrieve friend status notifications, and retrieve avatar names and
    /// profiles
    /// </summary>
    public class AvatarManager
    {
        const int MAX_UUIDS_PER_PACKET = 100;

        #region Events
        /// <summary>The event subscribers, null of no subscribers</summary>
        private EventHandler<AvatarAnimationEventArgs> m_AvatarAnimation;

        ///<summary>Raises the AvatarAnimation Event</summary>
        /// <param name="e">An AvatarAnimationEventArgs object containing
        /// the data sent from the simulator</param>
        protected virtual void OnAvatarAnimation(AvatarAnimationEventArgs e)
        {
            EventHandler<AvatarAnimationEventArgs> handler = m_AvatarAnimation;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>Thread sync lock object</summary>
        private readonly object m_AvatarAnimationLock = new object();

        /// <summary>Raised when the simulator sends us data containing
        /// an agents animation playlist</summary>
        public event EventHandler<AvatarAnimationEventArgs> AvatarAnimation
        {
            add { lock (m_AvatarAnimationLock) { m_AvatarAnimation += value; } }
            remove { lock (m_AvatarAnimationLock) { m_AvatarAnimation -= value; } }
        }

        /// <summary>The event subscribers, null of no subscribers</summary>
        private EventHandler<AvatarAppearanceEventArgs> m_AvatarAppearance;

        ///<summary>Raises the AvatarAppearance Event</summary>
        /// <param name="e">A AvatarAppearanceEventArgs object containing
        /// the data sent from the simulator</param>
        protected virtual void OnAvatarAppearance(AvatarAppearanceEventArgs e)
        {
            EventHandler<AvatarAppearanceEventArgs> handler = m_AvatarAppearance;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>Thread sync lock object</summary>
        private readonly object m_AvatarAppearanceLock = new object();

        /// <summary>Raised when the simulator sends us data containing
        /// the appearance information for an agent</summary>
        public event EventHandler<AvatarAppearanceEventArgs> AvatarAppearance
        {
            add { lock (m_AvatarAppearanceLock) { m_AvatarAppearance += value; } }
            remove { lock (m_AvatarAppearanceLock) { m_AvatarAppearance -= value; } }
        }

        /// <summary>The event subscribers, null of no subscribers</summary>
        private EventHandler<UUIDNameReplyEventArgs> m_UUIDNameReply;

        ///<summary>Raises the UUIDNameReply Event</summary>
        /// <param name="e">A UUIDNameReplyEventArgs object containing
        /// the data sent from the simulator</param>
        protected virtual void OnUUIDNameReply(UUIDNameReplyEventArgs e)
        {
            EventHandler<UUIDNameReplyEventArgs> handler = m_UUIDNameReply;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>Thread sync lock object</summary>
        private readonly object m_UUIDNameReplyLock = new object();

        /// <summary>Raised when the simulator sends us data containing
        /// agent names/id values</summary>
        public event EventHandler<UUIDNameReplyEventArgs> UUIDNameReply
        {
            add { lock (m_UUIDNameReplyLock) { m_UUIDNameReply += value; } }
            remove { lock (m_UUIDNameReplyLock) { m_UUIDNameReply -= value; } }
        }

        /// <summary>The event subscribers, null of no subscribers</summary>
        private EventHandler<AvatarInterestsReplyEventArgs> m_AvatarInterestsReply;

        ///<summary>Raises the AvatarInterestsReply Event</summary>
        /// <param name="e">A AvatarInterestsReplyEventArgs object containing
        /// the data sent from the simulator</param>
        protected virtual void OnAvatarInterestsReply(AvatarInterestsReplyEventArgs e)
        {
            EventHandler<AvatarInterestsReplyEventArgs> handler = m_AvatarInterestsReply;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>Thread sync lock object</summary>
        private readonly object m_AvatarInterestsReplyLock = new object();

        /// <summary>Raised when the simulator sends us data containing
        /// the interests listed in an agents profile</summary>
        public event EventHandler<AvatarInterestsReplyEventArgs> AvatarInterestsReply
        {
            add { lock (m_AvatarInterestsReplyLock) { m_AvatarInterestsReply += value; } }
            remove { lock (m_AvatarInterestsReplyLock) { m_AvatarInterestsReply -= value; } }
        }

        /// <summary>The event subscribers, null of no subscribers</summary>
        private EventHandler<AvatarPropertiesReplyEventArgs> m_AvatarPropertiesReply;

        ///<summary>Raises the AvatarPropertiesReply Event</summary>
        /// <param name="e">A AvatarPropertiesReplyEventArgs object containing
        /// the data sent from the simulator</param>
        protected virtual void OnAvatarPropertiesReply(AvatarPropertiesReplyEventArgs e)
        {
            EventHandler<AvatarPropertiesReplyEventArgs> handler = m_AvatarPropertiesReply;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>Thread sync lock object</summary>
        private readonly object m_AvatarPropertiesReplyLock = new object();

        /// <summary>Raised when the simulator sends us data containing
        /// profile property information for an agent</summary>
        public event EventHandler<AvatarPropertiesReplyEventArgs> AvatarPropertiesReply
        {
            add { lock (m_AvatarPropertiesReplyLock) { m_AvatarPropertiesReply += value; } }
            remove { lock (m_AvatarPropertiesReplyLock) { m_AvatarPropertiesReply -= value; } }
        }

        /// <summary>The event subscribers, null of no subscribers</summary>
        private EventHandler<AvatarGroupsReplyEventArgs> m_AvatarGroupsReply;

        ///<summary>Raises the AvatarGroupsReply Event</summary>
        /// <param name="e">A AvatarGroupsReplyEventArgs object containing
        /// the data sent from the simulator</param>
        protected virtual void OnAvatarGroupsReply(AvatarGroupsReplyEventArgs e)
        {
            EventHandler<AvatarGroupsReplyEventArgs> handler = m_AvatarGroupsReply;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>Thread sync lock object</summary>
        private readonly object m_AvatarGroupsReplyLock = new object();

        /// <summary>Raised when the simulator sends us data containing
        /// the group membership an agent is a member of</summary>
        public event EventHandler<AvatarGroupsReplyEventArgs> AvatarGroupsReply
        {
            add { lock (m_AvatarGroupsReplyLock) { m_AvatarGroupsReply += value; } }
            remove { lock (m_AvatarGroupsReplyLock) { m_AvatarGroupsReply -= value; } }
        }

        /// <summary>The event subscribers, null of no subscribers</summary>
        private EventHandler<AvatarPickerReplyEventArgs> m_AvatarPickerReply;

        ///<summary>Raises the AvatarPickerReply Event</summary>
        /// <param name="e">A AvatarPickerReplyEventArgs object containing
        /// the data sent from the simulator</param>
        protected virtual void OnAvatarPickerReply(AvatarPickerReplyEventArgs e)
        {
            EventHandler<AvatarPickerReplyEventArgs> handler = m_AvatarPickerReply;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>Thread sync lock object</summary>
        private readonly object m_AvatarPickerReplyLock = new object();

        /// <summary>Raised when the simulator sends us data containing
        /// name/id pair</summary>
        public event EventHandler<AvatarPickerReplyEventArgs> AvatarPickerReply
        {
            add { lock (m_AvatarPickerReplyLock) { m_AvatarPickerReply += value; } }
            remove { lock (m_AvatarPickerReplyLock) { m_AvatarPickerReply -= value; } }
        }

        /// <summary>The event subscribers, null of no subscribers</summary>
        private EventHandler<ViewerEffectPointAtEventArgs> m_ViewerEffectPointAt;

        ///<summary>Raises the ViewerEffectPointAt Event</summary>
        /// <param name="e">A ViewerEffectPointAtEventArgs object containing
        /// the data sent from the simulator</param>
        protected virtual void OnViewerEffectPointAt(ViewerEffectPointAtEventArgs e)
        {
            EventHandler<ViewerEffectPointAtEventArgs> handler = m_ViewerEffectPointAt;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>Thread sync lock object</summary>
        private readonly object m_ViewerEffectPointAtLock = new object();

        /// <summary>Raised when the simulator sends us data containing
        /// the objects and effect when an agent is pointing at</summary>
        public event EventHandler<ViewerEffectPointAtEventArgs> ViewerEffectPointAt
        {
            add { lock (m_ViewerEffectPointAtLock) { m_ViewerEffectPointAt += value; } }
            remove { lock (m_ViewerEffectPointAtLock) { m_ViewerEffectPointAt -= value; } }
        }

        /// <summary>The event subscribers, null of no subscribers</summary>
        private EventHandler<ViewerEffectLookAtEventArgs> m_ViewerEffectLookAt;

        ///<summary>Raises the ViewerEffectLookAt Event</summary>
        /// <param name="e">A ViewerEffectLookAtEventArgs object containing
        /// the data sent from the simulator</param>
        protected virtual void OnViewerEffectLookAt(ViewerEffectLookAtEventArgs e)
        {
            EventHandler<ViewerEffectLookAtEventArgs> handler = m_ViewerEffectLookAt;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>Thread sync lock object</summary>
        private readonly object m_ViewerEffectLookAtLock = new object();

        /// <summary>Raised when the simulator sends us data containing
        /// the objects and effect when an agent is looking at</summary>
        public event EventHandler<ViewerEffectLookAtEventArgs> ViewerEffectLookAt
        {
            add { lock (m_ViewerEffectLookAtLock) { m_ViewerEffectLookAt += value; } }
            remove { lock (m_ViewerEffectLookAtLock) { m_ViewerEffectLookAt -= value; } }
        }

        /// <summary>The event subscribers, null of no subscribers</summary>
        private EventHandler<ViewerEffectEventArgs> m_ViewerEffect;

        ///<summary>Raises the ViewerEffect Event</summary>
        /// <param name="e">A ViewerEffectEventArgs object containing
        /// the data sent from the simulator</param>
        protected virtual void OnViewerEffect(ViewerEffectEventArgs e)
        {
            EventHandler<ViewerEffectEventArgs> handler = m_ViewerEffect;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>Thread sync lock object</summary>
        private readonly object m_ViewerEffectLock = new object();

        /// <summary>Raised when the simulator sends us data containing
        /// an agents viewer effect information</summary>
        public event EventHandler<ViewerEffectEventArgs> ViewerEffect
        {
            add { lock (m_ViewerEffectLock) { m_ViewerEffect += value; } }
            remove { lock (m_ViewerEffectLock) { m_ViewerEffect -= value; } }
        }

        /// <summary>The event subscribers, null of no subscribers</summary>
        private EventHandler<AvatarPicksReplyEventArgs> m_AvatarPicksReply;

        ///<summary>Raises the AvatarPicksReply Event</summary>
        /// <param name="e">A AvatarPicksReplyEventArgs object containing
        /// the data sent from the simulator</param>
        protected virtual void OnAvatarPicksReply(AvatarPicksReplyEventArgs e)
        {
            EventHandler<AvatarPicksReplyEventArgs> handler = m_AvatarPicksReply;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>Thread sync lock object</summary>
        private readonly object m_AvatarPicksReplyLock = new object();

        /// <summary>Raised when the simulator sends us data containing
        /// the top picks from an agents profile</summary>
        public event EventHandler<AvatarPicksReplyEventArgs> AvatarPicksReply
        {
            add { lock (m_AvatarPicksReplyLock) { m_AvatarPicksReply += value; } }
            remove { lock (m_AvatarPicksReplyLock) { m_AvatarPicksReply -= value; } }
        }

        /// <summary>The event subscribers, null of no subscribers</summary>
        private EventHandler<PickInfoReplyEventArgs> m_PickInfoReply;

        ///<summary>Raises the PickInfoReply Event</summary>
        /// <param name="e">A PickInfoReplyEventArgs object containing
        /// the data sent from the simulator</param>
        protected virtual void OnPickInfoReply(PickInfoReplyEventArgs e)
        {
            EventHandler<PickInfoReplyEventArgs> handler = m_PickInfoReply;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>Thread sync lock object</summary>
        private readonly object m_PickInfoReplyLock = new object();

        /// <summary>Raised when the simulator sends us data containing
        /// the Pick details</summary>
        public event EventHandler<PickInfoReplyEventArgs> PickInfoReply
        {
            add { lock (m_PickInfoReplyLock) { m_PickInfoReply += value; } }
            remove { lock (m_PickInfoReplyLock) { m_PickInfoReply -= value; } }
        }

        /// <summary>The event subscribers, null of no subscribers</summary>
        private EventHandler<AvatarClassifiedReplyEventArgs> m_AvatarClassifiedReply;

        ///<summary>Raises the AvatarClassifiedReply Event</summary>
        /// <param name="e">A AvatarClassifiedReplyEventArgs object containing
        /// the data sent from the simulator</param>
        protected virtual void OnAvatarClassifiedReply(AvatarClassifiedReplyEventArgs e)
        {
            EventHandler<AvatarClassifiedReplyEventArgs> handler = m_AvatarClassifiedReply;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>Thread sync lock object</summary>
        private readonly object m_AvatarClassifiedReplyLock = new object();

        /// <summary>Raised when the simulator sends us data containing
        /// the classified ads an agent has placed</summary>
        public event EventHandler<AvatarClassifiedReplyEventArgs> AvatarClassifiedReply
        {
            add { lock (m_AvatarClassifiedReplyLock) { m_AvatarClassifiedReply += value; } }
            remove { lock (m_AvatarClassifiedReplyLock) { m_AvatarClassifiedReply -= value; } }
        }

        /// <summary>The event subscribers, null of no subscribers</summary>
        private EventHandler<ClassifiedInfoReplyEventArgs> m_ClassifiedInfoReply;

        ///<summary>Raises the ClassifiedInfoReply Event</summary>
        /// <param name="e">A ClassifiedInfoReplyEventArgs object containing
        /// the data sent from the simulator</param>
        protected virtual void OnClassifiedInfoReply(ClassifiedInfoReplyEventArgs e)
        {
            EventHandler<ClassifiedInfoReplyEventArgs> handler = m_ClassifiedInfoReply;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>Thread sync lock object</summary>
        private readonly object m_ClassifiedInfoReplyLock = new object();

        /// <summary>Raised when the simulator sends us data containing
        /// the details of a classified ad</summary>
        public event EventHandler<ClassifiedInfoReplyEventArgs> ClassifiedInfoReply
        {
            add { lock (m_ClassifiedInfoReplyLock) { m_ClassifiedInfoReply += value; } }
            remove { lock (m_ClassifiedInfoReplyLock) { m_ClassifiedInfoReply -= value; } }
        }

        /// <summary>The event subscribers, null of no subscribers</summary>
        private EventHandler<DisplayNameUpdateEventArgs> m_DisplayNameUpdate;

        ///<summary>Raises the DisplayNameUpdate Event</summary>
        /// <param name="e">A DisplayNameUpdateEventArgs object containing
        /// the data sent from the simulator</param>
        protected virtual void OnDisplayNameUpdate(DisplayNameUpdateEventArgs e)
        {
            EventHandler<DisplayNameUpdateEventArgs> handler = m_DisplayNameUpdate;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>Thread sync lock object</summary>
        private readonly object m_DisplayNameUpdateLock = new object();

        /// <summary>Raised when the simulator sends us data containing
        /// the details of display name change</summary>
        public event EventHandler<DisplayNameUpdateEventArgs> DisplayNameUpdate
        {
            add { lock (m_DisplayNameUpdateLock) { m_DisplayNameUpdate += value; } }
            remove { lock (m_DisplayNameUpdateLock) { m_DisplayNameUpdate -= value; } }
        }

        #endregion Events

        #region Delegates
        /// <summary>
        /// Callback giving results when fetching display names
        /// </summary>
        /// <param name="success">If the request was successful</param>
        /// <param name="names">Array of display names</param>
        /// <param name="badIDs">Array of UUIDs that could not be fetched</param>
        public delegate void DisplayNamesCallback(bool success, AgentDisplayName[] names, UUID[] badIDs);
        #endregion Delegates

        private ProxyFrame Proxy;

        /// <summary>
        /// Represents other avatars
        /// </summary>
        /// <param name="client"></param>
        public AvatarManager(ProxyFrame frame)
        {
            Proxy = frame;

            // Avatar appearance callback
            Proxy.Network.AddDelegate(PacketType.AvatarAppearance, Direction.Incoming, AvatarAppearanceHandler);

            // Avatar profile callbacks
            Proxy.Network.AddDelegate(PacketType.AvatarPropertiesReply, Direction.Incoming, AvatarPropertiesHandler);
            // Proxy.Network.RegisterCallback(PacketType.AvatarStatisticsReply, AvatarStatisticsHandler);
            Proxy.Network.AddDelegate(PacketType.AvatarInterestsReply, Direction.Incoming, AvatarInterestsHandler);

            // Avatar group callback
            Proxy.Network.AddDelegate(PacketType.AvatarGroupsReply, Direction.Incoming, AvatarGroupsReplyHandler);
            //Proxy.Network.RegisterEventCallback("AgentGroupDataUpdate", AvatarGroupsReplyMessageHandler);
            //Proxy.Network.RegisterEventCallback("AvatarGroupsReply", AvatarGroupsReplyMessageHandler);

            // Viewer effect callback
            Proxy.Network.AddDelegate(PacketType.ViewerEffect, Direction.Incoming, ViewerEffectHandler);

            // Other callbacks
            Proxy.Network.AddDelegate(PacketType.UUIDNameReply, Direction.Incoming, UUIDNameReplyHandler);
            Proxy.Network.AddDelegate(PacketType.AvatarPickerReply, Direction.Incoming, AvatarPickerReplyHandler);
            Proxy.Network.AddDelegate(PacketType.AvatarAnimation, Direction.Incoming, AvatarAnimationHandler);

            // Picks callbacks
            Proxy.Network.AddDelegate(PacketType.AvatarPicksReply, Direction.Incoming, AvatarPicksReplyHandler);
            Proxy.Network.AddDelegate(PacketType.PickInfoReply, Direction.Incoming, PickInfoReplyHandler);

            // Classifieds callbacks
            Proxy.Network.AddDelegate(PacketType.AvatarClassifiedReply, Direction.Incoming, AvatarClassifiedReplyHandler);
            Proxy.Network.AddDelegate(PacketType.ClassifiedInfoReply, Direction.Incoming, ClassifiedInfoReplyHandler);

            //Proxy.Network.RegisterEventCallback("DisplayNameUpdate", DisplayNameUpdateMessageHandler);
        }

        /// <summary>Tracks the specified avatar on your map</summary>
        /// <param name="preyID">Avatar ID to track</param>
        public void RequestTrackAgent(UUID preyID)
        {
            TrackAgentPacket p = new TrackAgentPacket();
            p.AgentData.AgentID = Proxy.Agent.AgentID;
            p.AgentData.SessionID = Proxy.Agent.SessionID;
            p.TargetData.PreyID = preyID;
            Proxy.Network.InjectPacket(p, Direction.Outgoing);
        }

        /// <summary>
        /// Request a single avatar name
        /// </summary>
        /// <param name="id">The avatar key to retrieve a name for</param>
        public void RequestAvatarName(UUID id)
        {
            UUIDNameRequestPacket request = new UUIDNameRequestPacket();
            request.UUIDNameBlock = new UUIDNameRequestPacket.UUIDNameBlockBlock[1];
            request.UUIDNameBlock[0] = new UUIDNameRequestPacket.UUIDNameBlockBlock();
            request.UUIDNameBlock[0].ID = id;

            Proxy.Network.InjectPacket(request, Direction.Outgoing);
        }

        /// <summary>
        /// Request a list of avatar names
        /// </summary>
        /// <param name="ids">The avatar keys to retrieve names for</param>
        public void RequestAvatarNames(List<UUID> ids)
        {
            int m = MAX_UUIDS_PER_PACKET;
            int n = ids.Count / m; // Number of full requests to make
            int i = 0;

            UUIDNameRequestPacket request;

            for (int j = 0; j < n; j++)
            {
                request = new UUIDNameRequestPacket();
                request.UUIDNameBlock = new UUIDNameRequestPacket.UUIDNameBlockBlock[m];

                for (; i < (j + 1) * m; i++)
                {
                    request.UUIDNameBlock[i % m] = new UUIDNameRequestPacket.UUIDNameBlockBlock();
                    request.UUIDNameBlock[i % m].ID = ids[i];
                }

                Proxy.Network.InjectPacket(request, Direction.Outgoing);
            }

            // Get any remaining names after left after the full requests
            if (ids.Count > n * m)
            {
                request = new UUIDNameRequestPacket();
                request.UUIDNameBlock = new UUIDNameRequestPacket.UUIDNameBlockBlock[ids.Count - n * m];

                for (; i < ids.Count; i++)
                {
                    request.UUIDNameBlock[i % m] = new UUIDNameRequestPacket.UUIDNameBlockBlock();
                    request.UUIDNameBlock[i % m].ID = ids[i];
                }

                Proxy.Network.InjectPacket(request, Direction.Outgoing);
            }
        }

        /// <summary>
        /// Check if Display Names functionality is available
        /// </summary>
        /// <returns>True if Display name functionality is available</returns>
        public bool DisplayNamesAvailable()
        {
            return (Proxy.Network.CurrentSim != null && Proxy.Network.CurrentSim.Caps != null) && Proxy.Network.CurrentSim.Caps.ContainsKey("GetDisplayNames");
        }

        /// <summary>
        /// Request retrieval of display names (max 90 names per request)
        /// </summary>
        /// <param name="ids">List of UUIDs to lookup</param>
        /// <param name="callback">Callback to report result of the operation</param>
        public void GetDisplayNames(List<UUID> ids, DisplayNamesCallback callback)
        {
            if (!DisplayNamesAvailable() || ids.Count == 0)
            {
                callback(false, null, null);
            }

            StringBuilder query = new StringBuilder();
            for (int i = 0; i < ids.Count && i < 90; i++)
            {
                query.AppendFormat("ids={0}", ids[i]);
                if (i < ids.Count - 1)
                {
                    query.Append("&");
                }
            }

            Uri uri = new Uri(Proxy.Network.CurrentSim.Caps["GetDisplayNames"] + "/?" + query);

            CapsClient cap = new CapsClient(uri);
            cap.OnComplete += (CapsClient client, OSD result, Exception error) =>
                                  {
                                      try
                                      {
                                          if (error != null)
                                              throw error;
                                          GetDisplayNamesMessage msg = new GetDisplayNamesMessage();
                                          msg.Deserialize((OSDMap)result);
                                          callback(true, msg.Agents, msg.BadIDs);
                                      }
                                      catch (Exception ex)
                                      {
                                          Logger.Log("Failed to call GetDisplayNames capability: ",
                                                     Helpers.LogLevel.Warning, ex);
                                          callback(false, null, null);
                                      }
                                  };
            cap.BeginGetResponse(null, String.Empty, Proxy.Config.CAPS_TIMEOUT);
        }

        /// <summary>
        /// Start a request for Avatar Properties
        /// </summary>
        /// <param name="avatarid"></param>
        public void RequestAvatarProperties(UUID avatarid)
        {
            AvatarPropertiesRequestPacket aprp = new AvatarPropertiesRequestPacket();

            aprp.AgentData.AgentID = Proxy.Agent.AgentID;
            aprp.AgentData.SessionID = Proxy.Agent.SessionID;
            aprp.AgentData.AvatarID = avatarid;

            Proxy.Network.InjectPacket(aprp, Direction.Outgoing);
        }

        /// <summary>
        /// Search for an avatar (first name, last name)
        /// </summary>
        /// <param name="name">The name to search for</param>
        /// <param name="queryID">An ID to associate with this query</param>
        public void RequestAvatarNameSearch(string name, UUID queryID)
        {
            AvatarPickerRequestPacket aprp = new AvatarPickerRequestPacket();

            aprp.AgentData.AgentID = Proxy.Agent.AgentID;
            aprp.AgentData.SessionID = Proxy.Agent.SessionID;
            aprp.AgentData.QueryID = queryID;
            aprp.Data.Name = Utils.StringToBytes(name);

            Proxy.Network.InjectPacket(aprp, Direction.Outgoing);
        }

        /// <summary>
        /// Start a request for Avatar Picks
        /// </summary>
        /// <param name="avatarid">UUID of the avatar</param>
        public void RequestAvatarPicks(UUID avatarid)
        {
            GenericMessagePacket gmp = new GenericMessagePacket();

            gmp.AgentData.AgentID = Proxy.Agent.AgentID;
            gmp.AgentData.SessionID = Proxy.Agent.SessionID;
            gmp.AgentData.TransactionID = UUID.Zero;

            gmp.MethodData.Method = Utils.StringToBytes("avatarpicksrequest");
            gmp.MethodData.Invoice = UUID.Zero;
            gmp.ParamList = new GenericMessagePacket.ParamListBlock[1];
            gmp.ParamList[0] = new GenericMessagePacket.ParamListBlock();
            gmp.ParamList[0].Parameter = Utils.StringToBytes(avatarid.ToString());

            Proxy.Network.InjectPacket(gmp, Direction.Outgoing);
        }

        /// <summary>
        /// Start a request for Avatar Classifieds
        /// </summary>
        /// <param name="avatarid">UUID of the avatar</param>
        public void RequestAvatarClassified(UUID avatarid)
        {
            GenericMessagePacket gmp = new GenericMessagePacket();

            gmp.AgentData.AgentID = Proxy.Agent.AgentID;
            gmp.AgentData.SessionID = Proxy.Agent.SessionID;
            gmp.AgentData.TransactionID = UUID.Zero;

            gmp.MethodData.Method = Utils.StringToBytes("avatarclassifiedsrequest");
            gmp.MethodData.Invoice = UUID.Zero;
            gmp.ParamList = new GenericMessagePacket.ParamListBlock[1];
            gmp.ParamList[0] = new GenericMessagePacket.ParamListBlock();
            gmp.ParamList[0].Parameter = Utils.StringToBytes(avatarid.ToString());

            Proxy.Network.InjectPacket(gmp, Direction.Outgoing);
        }

        /// <summary>
        /// Start a request for details of a specific profile pick
        /// </summary>
        /// <param name="avatarid">UUID of the avatar</param>
        /// <param name="pickid">UUID of the profile pick</param>
        public void RequestPickInfo(UUID avatarid, UUID pickid)
        {
            GenericMessagePacket gmp = new GenericMessagePacket();

            gmp.AgentData.AgentID = Proxy.Agent.AgentID;
            gmp.AgentData.SessionID = Proxy.Agent.SessionID;
            gmp.AgentData.TransactionID = UUID.Zero;

            gmp.MethodData.Method = Utils.StringToBytes("pickinforequest");
            gmp.MethodData.Invoice = UUID.Zero;
            gmp.ParamList = new GenericMessagePacket.ParamListBlock[2];
            gmp.ParamList[0] = new GenericMessagePacket.ParamListBlock();
            gmp.ParamList[0].Parameter = Utils.StringToBytes(avatarid.ToString());
            gmp.ParamList[1] = new GenericMessagePacket.ParamListBlock();
            gmp.ParamList[1].Parameter = Utils.StringToBytes(pickid.ToString());

            Proxy.Network.InjectPacket(gmp, Direction.Outgoing);
        }

        /// <summary>
        /// Start a request for details of a specific profile classified
        /// </summary>
        /// <param name="avatarid">UUID of the avatar</param>
        /// <param name="classifiedid">UUID of the profile classified</param>
        public void RequestClassifiedInfo(UUID avatarid, UUID classifiedid)
        {
            GenericMessagePacket gmp = new GenericMessagePacket();

            gmp.AgentData.AgentID = Proxy.Agent.AgentID;
            gmp.AgentData.SessionID = Proxy.Agent.SessionID;
            gmp.AgentData.TransactionID = UUID.Zero;

            gmp.MethodData.Method = Utils.StringToBytes("classifiedinforequest");
            gmp.MethodData.Invoice = UUID.Zero;
            gmp.ParamList = new GenericMessagePacket.ParamListBlock[2];
            gmp.ParamList[0] = new GenericMessagePacket.ParamListBlock();
            gmp.ParamList[0].Parameter = Utils.StringToBytes(avatarid.ToString());
            gmp.ParamList[1] = new GenericMessagePacket.ParamListBlock();
            gmp.ParamList[1].Parameter = Utils.StringToBytes(classifiedid.ToString());

            Proxy.Network.InjectPacket(gmp, Direction.Outgoing);
        }

        #region Packet Handlers

        /// <summary>Process an incoming packet and raise the appropriate events</summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">The EventArgs object containing the packet data</param>
        protected Packet UUIDNameReplyHandler(Packet packet, RegionProxy simulator)
        {
            if (m_UUIDNameReply != null)
            {
                Dictionary<UUID, string> names = new Dictionary<UUID, string>();
                UUIDNameReplyPacket reply = (UUIDNameReplyPacket)packet;

                foreach (UUIDNameReplyPacket.UUIDNameBlockBlock block in reply.UUIDNameBlock)
                {
                    names[block.ID] = Utils.BytesToString(block.FirstName) +
                        " " + Utils.BytesToString(block.LastName);
                }

                OnUUIDNameReply(new UUIDNameReplyEventArgs(names));
            }

            return packet;
        }

        /// <summary>Process an incoming packet and raise the appropriate events</summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">The EventArgs object containing the packet data</param>
        protected Packet AvatarAnimationHandler(Packet packet, RegionProxy simulator)
        {
            AvatarAnimationPacket data = (AvatarAnimationPacket)packet;

            List<Animation> signaledAnimations = new List<Animation>(data.AnimationList.Length);

            for (int i = 0; i < data.AnimationList.Length; i++)
            {
                Animation animation = new Animation();
                animation.AnimationID = data.AnimationList[i].AnimID;
                animation.AnimationSequence = data.AnimationList[i].AnimSequenceID;
                if (i < data.AnimationSourceList.Length)
                {
                    animation.AnimationSourceObjectID = data.AnimationSourceList[i].ObjectID;
                }

                signaledAnimations.Add(animation);
            }

            Avatar avatar = simulator.ObjectsAvatars.Values.FirstOrDefault(avi => avi.ID == data.Sender.ID);
            if (avatar != default(Avatar))
            {
                avatar.Animations = signaledAnimations;
            }

            OnAvatarAnimation(new AvatarAnimationEventArgs(data.Sender.ID, signaledAnimations));

            return packet;
        }

        /// <summary>Process an incoming packet and raise the appropriate events</summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">The EventArgs object containing the packet data</param>
        protected Packet AvatarAppearanceHandler(Packet packet, RegionProxy simulator)
        {
            if (m_AvatarAppearance != null)// || Proxy.Config.AVATAR_TRACKING)
            {
                AvatarAppearancePacket appearance = (AvatarAppearancePacket)packet;

                List<byte> visualParams = new List<byte>();
                foreach (AvatarAppearancePacket.VisualParamBlock block in appearance.VisualParam)
                {
                    visualParams.Add(block.ParamValue);
                }

                Primitive.TextureEntry textureEntry = new Primitive.TextureEntry(appearance.ObjectData.TextureEntry, 0,
                        appearance.ObjectData.TextureEntry.Length);

                Primitive.TextureEntryFace defaultTexture = textureEntry.DefaultTexture;
                Primitive.TextureEntryFace[] faceTextures = textureEntry.FaceTextures;

                byte appearanceVersion = 0;
                int COFVersion = 0;
                AppearanceFlags appearanceFlags = 0;

                if (appearance.AppearanceData != null && appearance.AppearanceData.Length > 0)
                {
                    appearanceVersion = appearance.AppearanceData[0].AppearanceVersion;
                    COFVersion = appearance.AppearanceData[0].CofVersion;
                    appearanceFlags = (AppearanceFlags)appearance.AppearanceData[0].Flags;
                }

                Avatar av = simulator.ObjectsAvatars.Values.FirstOrDefault((Avatar a) => { return a.ID == appearance.Sender.ID; });
                if (av != null)
                {
                    av.Textures = textureEntry;
                    av.VisualParameters = visualParams.ToArray();
                    av.AppearanceVersion = appearanceVersion;
                    av.COFVersion = COFVersion;
                    av.AppearanceFlags = appearanceFlags;
                }

                OnAvatarAppearance(new AvatarAppearanceEventArgs(simulator, appearance.Sender.ID, appearance.Sender.IsTrial,
                    defaultTexture, faceTextures, visualParams, appearanceVersion, COFVersion, appearanceFlags));
            }

            return packet;
        }

        /// <summary>Process an incoming packet and raise the appropriate events</summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">The EventArgs object containing the packet data</param>
        protected Packet AvatarPropertiesHandler(Packet packet, RegionProxy simulator)
        {
            if (m_AvatarPropertiesReply != null)
            {
                AvatarPropertiesReplyPacket reply = (AvatarPropertiesReplyPacket)packet;
                Avatar.AvatarProperties properties = new Avatar.AvatarProperties();

                properties.ProfileImage = reply.PropertiesData.ImageID;
                properties.FirstLifeImage = reply.PropertiesData.FLImageID;
                properties.Partner = reply.PropertiesData.PartnerID;
                properties.AboutText = Utils.BytesToString(reply.PropertiesData.AboutText);
                properties.FirstLifeText = Utils.BytesToString(reply.PropertiesData.FLAboutText);
                properties.BornOn = Utils.BytesToString(reply.PropertiesData.BornOn);
                //properties.CharterMember = Utils.BytesToString(reply.PropertiesData.CharterMember);
                uint charter = Utils.BytesToUInt(reply.PropertiesData.CharterMember);
                if (charter == 0)
                {
                    properties.CharterMember = "Resident";
                }
                else if (charter == 2)
                {
                    properties.CharterMember = "Charter";
                }
                else if (charter == 3)
                {
                    properties.CharterMember = "Linden";
                }
                else
                {
                    properties.CharterMember = Utils.BytesToString(reply.PropertiesData.CharterMember);
                }
                properties.Flags = (ProfileFlags)reply.PropertiesData.Flags;
                properties.ProfileURL = Utils.BytesToString(reply.PropertiesData.ProfileURL);

                OnAvatarPropertiesReply(new AvatarPropertiesReplyEventArgs(reply.AgentData.AvatarID, properties));
            }

            return packet;
        }

        /// <summary>Process an incoming packet and raise the appropriate events</summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">The EventArgs object containing the packet data</param>
        protected Packet AvatarInterestsHandler(Packet packet, RegionProxy region)
        {
            if (m_AvatarInterestsReply != null)
            {
                AvatarInterestsReplyPacket airp = (AvatarInterestsReplyPacket)packet;
                Avatar.Interests interests = new Avatar.Interests();

                interests.WantToMask = airp.PropertiesData.WantToMask;
                interests.WantToText = Utils.BytesToString(airp.PropertiesData.WantToText);
                interests.SkillsMask = airp.PropertiesData.SkillsMask;
                interests.SkillsText = Utils.BytesToString(airp.PropertiesData.SkillsText);
                interests.LanguagesText = Utils.BytesToString(airp.PropertiesData.LanguagesText);

                OnAvatarInterestsReply(new AvatarInterestsReplyEventArgs(airp.AgentData.AvatarID, interests));
            }

            return packet;
        }

        /// <summary>
        /// EQ Message fired when someone nearby changes their display name
        /// </summary>
        /// <param name="capsKey">The message key</param>
        /// <param name="message">the IMessage object containing the deserialized data sent from the simulator</param>
        /// <param name="simulator">The <see cref="Simulator"/> which originated the packet</param>
        protected void DisplayNameUpdateMessageHandler(string capsKey, IMessage message, Simulator simulator)
        {
            if (m_DisplayNameUpdate != null)
            {
                DisplayNameUpdateMessage msg = (DisplayNameUpdateMessage)message;
                OnDisplayNameUpdate(new DisplayNameUpdateEventArgs(msg.OldDisplayName, msg.DisplayName));
            }
        }

        /// <summary>
        /// Crossed region handler for message that comes across the EventQueue. Sent to an agent
        /// when the agent crosses a sim border into a new region.
        /// </summary>
        /// <param name="capsKey">The message key</param>
        /// <param name="message">the IMessage object containing the deserialized data sent from the simulator</param>
        /// <param name="simulator">The <see cref="Simulator"/> which originated the packet</param>
        protected void AvatarGroupsReplyMessageHandler(string capsKey, IMessage message, Simulator simulator)
        {
            AgentGroupDataUpdateMessage msg = (AgentGroupDataUpdateMessage)message;
            List<AvatarGroup> avatarGroups = new List<AvatarGroup>(msg.GroupDataBlock.Length);
            for (int i = 0; i < msg.GroupDataBlock.Length; i++)
            {
                AvatarGroup avatarGroup = new AvatarGroup();
                avatarGroup.AcceptNotices = msg.GroupDataBlock[i].AcceptNotices;
                avatarGroup.GroupID = msg.GroupDataBlock[i].GroupID;
                avatarGroup.GroupInsigniaID = msg.GroupDataBlock[i].GroupInsigniaID;
                avatarGroup.GroupName = msg.GroupDataBlock[i].GroupName;
                avatarGroup.GroupPowers = msg.GroupDataBlock[i].GroupPowers;
                avatarGroup.ListInProfile = msg.NewGroupDataBlock[i].ListInProfile;

                avatarGroups.Add(avatarGroup);
            }

            OnAvatarGroupsReply(new AvatarGroupsReplyEventArgs(msg.AgentID, avatarGroups));
        }

        /// <summary>Process an incoming packet and raise the appropriate events</summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">The EventArgs object containing the packet data</param>
        protected Packet AvatarGroupsReplyHandler(Packet packet, RegionProxy simulator)
        {
            if (m_AvatarGroupsReply != null)
            {
                AvatarGroupsReplyPacket groups = (AvatarGroupsReplyPacket)packet;
                List<AvatarGroup> avatarGroups = new List<AvatarGroup>(groups.GroupData.Length);

                for (int i = 0; i < groups.GroupData.Length; i++)
                {
                    AvatarGroup avatarGroup = new AvatarGroup();

                    avatarGroup.AcceptNotices = groups.GroupData[i].AcceptNotices;
                    avatarGroup.GroupID = groups.GroupData[i].GroupID;
                    avatarGroup.GroupInsigniaID = groups.GroupData[i].GroupInsigniaID;
                    avatarGroup.GroupName = Utils.BytesToString(groups.GroupData[i].GroupName);
                    avatarGroup.GroupPowers = (GroupPowers)groups.GroupData[i].GroupPowers;
                    avatarGroup.GroupTitle = Utils.BytesToString(groups.GroupData[i].GroupTitle);
                    avatarGroup.ListInProfile = groups.NewGroupData.ListInProfile;

                    avatarGroups.Add(avatarGroup);
                }

                OnAvatarGroupsReply(new AvatarGroupsReplyEventArgs(groups.AgentData.AvatarID, avatarGroups));
            }

            return packet;
        }

        /// <summary>Process an incoming packet and raise the appropriate events</summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">The EventArgs object containing the packet data</param>
        protected Packet AvatarPickerReplyHandler(Packet packet, RegionProxy simulator)
        {
            if (m_AvatarPickerReply != null)
            {
                AvatarPickerReplyPacket reply = (AvatarPickerReplyPacket)packet;
                Dictionary<UUID, string> avatars = new Dictionary<UUID, string>();

                foreach (AvatarPickerReplyPacket.DataBlock block in reply.Data)
                {
                    avatars[block.AvatarID] = Utils.BytesToString(block.FirstName) +
                        " " + Utils.BytesToString(block.LastName);
                }
                OnAvatarPickerReply(new AvatarPickerReplyEventArgs(reply.AgentData.QueryID, avatars));
            }

            return packet;
        }

        /// <summary>Process an incoming packet and raise the appropriate events</summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">The EventArgs object containing the packet data</param>
        protected Packet ViewerEffectHandler(Packet packet, RegionProxy simulator)
        {
            ViewerEffectPacket effect = (ViewerEffectPacket)packet;

            foreach (ViewerEffectPacket.EffectBlock block in effect.Effect)
            {
                EffectType type = (EffectType)block.Type;

                // Each ViewerEffect type uses it's own custom binary format for additional data. Fun eh?
                switch (type)
                {
                    case EffectType.Text:
                        Logger.Log("Received a ViewerEffect of type " + type.ToString() + ", implement me!",
                            Helpers.LogLevel.Warning);
                        break;
                    case EffectType.Icon:
                        Logger.Log("Received a ViewerEffect of type " + type.ToString() + ", implement me!",
                            Helpers.LogLevel.Warning);
                        break;
                    case EffectType.Connector:
                        Logger.Log("Received a ViewerEffect of type " + type.ToString() + ", implement me!",
                            Helpers.LogLevel.Warning);
                        break;
                    case EffectType.FlexibleObject:
                        Logger.Log("Received a ViewerEffect of type " + type.ToString() + ", implement me!",
                            Helpers.LogLevel.Warning);
                        break;
                    case EffectType.AnimalControls:
                        Logger.Log("Received a ViewerEffect of type " + type.ToString() + ", implement me!",
                            Helpers.LogLevel.Warning);
                        break;
                    case EffectType.AnimationObject:
                        Logger.Log("Received a ViewerEffect of type " + type.ToString() + ", implement me!",
                            Helpers.LogLevel.Warning);
                        break;
                    case EffectType.Cloth:
                        Logger.Log("Received a ViewerEffect of type " + type.ToString() + ", implement me!",
                            Helpers.LogLevel.Warning);
                        break;
                    case EffectType.Glow:
                        Logger.Log("Received a Glow ViewerEffect which is not implemented yet",
                            Helpers.LogLevel.Warning);
                        break;
                    case EffectType.Beam:
                    case EffectType.Point:
                    case EffectType.Trail:
                    case EffectType.Sphere:
                    case EffectType.Spiral:
                    case EffectType.Edit:
                        if (m_ViewerEffect != null)
                        {
                            if (block.TypeData.Length == 56)
                            {
                                UUID sourceAvatar = new UUID(block.TypeData, 0);
                                UUID targetObject = new UUID(block.TypeData, 16);
                                Vector3d targetPos = new Vector3d(block.TypeData, 32);
                                OnViewerEffect(new ViewerEffectEventArgs(type, sourceAvatar, targetObject, targetPos, block.Duration, block.ID));
                            }
                            else
                            {
                                Logger.Log("Received a " + type.ToString() +
                                    " ViewerEffect with an incorrect TypeData size of " +
                                    block.TypeData.Length + " bytes", Helpers.LogLevel.Warning);
                            }
                        }
                        break;
                    case EffectType.LookAt:
                        if (m_ViewerEffectLookAt != null)
                        {
                            if (block.TypeData.Length == 57)
                            {
                                UUID sourceAvatar = new UUID(block.TypeData, 0);
                                UUID targetObject = new UUID(block.TypeData, 16);
                                Vector3d targetPos = new Vector3d(block.TypeData, 32);
                                LookAtType lookAt = (LookAtType)block.TypeData[56];

                                OnViewerEffectLookAt(new ViewerEffectLookAtEventArgs(sourceAvatar, targetObject, targetPos, lookAt,
                                    block.Duration, block.ID));
                            }
                            else
                            {
                                Logger.Log("Received a LookAt ViewerEffect with an incorrect TypeData size of " +
                                    block.TypeData.Length + " bytes", Helpers.LogLevel.Warning);
                            }
                        }
                        break;
                    case EffectType.PointAt:
                        if (m_ViewerEffectPointAt != null)
                        {
                            if (block.TypeData.Length == 57)
                            {
                                UUID sourceAvatar = new UUID(block.TypeData, 0);
                                UUID targetObject = new UUID(block.TypeData, 16);
                                Vector3d targetPos = new Vector3d(block.TypeData, 32);
                                PointAtType pointAt = (PointAtType)block.TypeData[56];

                                OnViewerEffectPointAt(new ViewerEffectPointAtEventArgs(simulator, sourceAvatar, targetObject, targetPos,
                                    pointAt, block.Duration, block.ID));
                            }
                            else
                            {
                                Logger.Log("Received a PointAt ViewerEffect with an incorrect TypeData size of " +
                                    block.TypeData.Length + " bytes", Helpers.LogLevel.Warning);
                            }
                        }
                        break;
                    default:
                        Logger.Log("Received a ViewerEffect with an unknown type " + type, Helpers.LogLevel.Warning);
                        break;
                }
            }

            return packet;
        }

        /// <summary>Process an incoming packet and raise the appropriate events</summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">The EventArgs object containing the packet data</param>
        protected Packet AvatarPicksReplyHandler(Packet packet, RegionProxy simulator)
        {
            if (m_AvatarPicksReply == null)
            {
                return packet;
            }

            AvatarPicksReplyPacket p = (AvatarPicksReplyPacket)packet;
            Dictionary<UUID, string> picks = new Dictionary<UUID, string>();

            foreach (AvatarPicksReplyPacket.DataBlock b in p.Data)
            {
                picks.Add(b.PickID, Utils.BytesToString(b.PickName));
            }

            OnAvatarPicksReply(new AvatarPicksReplyEventArgs(p.AgentData.TargetID, picks));

            return packet;
        }

        /// <summary>Process an incoming packet and raise the appropriate events</summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">The EventArgs object containing the packet data</param>
        protected Packet PickInfoReplyHandler(Packet packet, RegionProxy simulator)
        {
            if (m_PickInfoReply != null)
            {
                PickInfoReplyPacket p = (PickInfoReplyPacket)packet;
                ProfilePick ret = new ProfilePick();
                ret.CreatorID = p.Data.CreatorID;
                ret.Desc = Utils.BytesToString(p.Data.Desc);
                ret.Enabled = p.Data.Enabled;
                ret.Name = Utils.BytesToString(p.Data.Name);
                ret.OriginalName = Utils.BytesToString(p.Data.OriginalName);
                ret.ParcelID = p.Data.ParcelID;
                ret.PickID = p.Data.PickID;
                ret.PosGlobal = p.Data.PosGlobal;
                ret.SimName = Utils.BytesToString(p.Data.SimName);
                ret.SnapshotID = p.Data.SnapshotID;
                ret.SortOrder = p.Data.SortOrder;
                ret.TopPick = p.Data.TopPick;
                ret.User = Utils.BytesToString(p.Data.User);

                OnPickInfoReply(new PickInfoReplyEventArgs(ret.PickID, ret));
            }

            return packet;
        }

        /// <summary>Process an incoming packet and raise the appropriate events</summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">The EventArgs object containing the packet data</param>
        protected Packet AvatarClassifiedReplyHandler(Packet packet, RegionProxy simulator)
        {
            if (m_AvatarClassifiedReply != null)
            {
                AvatarClassifiedReplyPacket p = (AvatarClassifiedReplyPacket)packet;
                Dictionary<UUID, string> classifieds = new Dictionary<UUID, string>();

                foreach (AvatarClassifiedReplyPacket.DataBlock b in p.Data)
                {
                    classifieds.Add(b.ClassifiedID, Utils.BytesToString(b.Name));
                }

                OnAvatarClassifiedReply(new AvatarClassifiedReplyEventArgs(p.AgentData.TargetID, classifieds));
            }

            return packet;
        }

        /// <summary>Process an incoming packet and raise the appropriate events</summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">The EventArgs object containing the packet data</param>
        protected Packet ClassifiedInfoReplyHandler(Packet packet, RegionProxy simulator)
        {
            if (m_AvatarClassifiedReply != null)
            {
                ClassifiedInfoReplyPacket p = (ClassifiedInfoReplyPacket)packet;
                ClassifiedAd ret = new ClassifiedAd();
                ret.Desc = Utils.BytesToString(p.Data.Desc);
                ret.Name = Utils.BytesToString(p.Data.Name);
                ret.ParcelID = p.Data.ParcelID;
                ret.ClassifiedID = p.Data.ClassifiedID;
                ret.Position = p.Data.PosGlobal;
                ret.SnapShotID = p.Data.SnapshotID;
                ret.Price = p.Data.PriceForListing;
                ret.ParentEstate = p.Data.ParentEstate;
                ret.ClassifiedFlags = p.Data.ClassifiedFlags;
                ret.Catagory = p.Data.Category;

                OnClassifiedInfoReply(new ClassifiedInfoReplyEventArgs(ret.ClassifiedID, ret));
            }

            return packet;
        }

        #endregion Packet Handlers
    }

    #region EventArgs
    // These are only the ones that needed Simulator replacing with RegionProxy.

    /// <summary>Provides data for the <see cref="AvatarManager.AvatarAppearance"/> event</summary>
    /// <remarks>The <see cref="AvatarManager.AvatarAppearance"/> event occurs when the simulator sends
    /// the appearance data for an avatar</remarks>
    /// <example>
    /// The following code example uses the <see cref="AvatarAppearanceEventArgs.AvatarID"/> and <see cref="AvatarAppearanceEventArgs.VisualParams"/>
    /// properties to display the selected shape of an avatar on the <see cref="Console"/> window.
    /// <code>
    ///     // subscribe to the event
    ///     Proxy.Avatars.AvatarAppearance += Avatars_AvatarAppearance;
    /// 
    ///     // handle the data when the event is raised
    ///     void Avatars_AvatarAppearance(object sender, AvatarAppearanceEventArgs e)
    ///     {
    ///         Console.WriteLine("The Agent {0} is using a {1} shape.", e.AvatarID, (e.VisualParams[31] &gt; 0) : "male" ? "female")
    ///     }
    /// </code>
    /// </example>
    public class AvatarAppearanceEventArgs : EventArgs
    {
        private readonly RegionProxy m_Simulator;
        private readonly UUID m_AvatarID;
        private readonly bool m_IsTrial;
        private readonly Primitive.TextureEntryFace m_DefaultTexture;
        private readonly Primitive.TextureEntryFace[] m_FaceTextures;
        private readonly List<byte> m_VisualParams;
        private readonly byte m_AppearanceVersion;
        private readonly int m_COFVersion;
        private readonly AppearanceFlags m_AppearanceFlags;

        /// <summary>Get the Simulator this request is from of the agent</summary>
        public RegionProxy Simulator { get { return m_Simulator; } }
        /// <summary>Get the ID of the agent</summary>
        public UUID AvatarID { get { return m_AvatarID; } }
        /// <summary>true if the agent is a trial account</summary>
        public bool IsTrial { get { return m_IsTrial; } }
        /// <summary>Get the default agent texture</summary>
        public Primitive.TextureEntryFace DefaultTexture { get { return m_DefaultTexture; } }
        /// <summary>Get the agents appearance layer textures</summary>
        public Primitive.TextureEntryFace[] FaceTextures { get { return m_FaceTextures; } }
        /// <summary>Get the <see cref="VisualParams"/> for the agent</summary>
        public List<byte> VisualParams { get { return m_VisualParams; } }
        /// <summary>Version of the appearance system used.
        /// Value greater than 0 indicates that server side baking is used</summary>
        public byte AppearanceVersion { get { return m_AppearanceVersion; } }
        /// <summary>Version of the Current Outfit Folder the appearance is based on</summary>
        public int COFVersion { get { return m_COFVersion; } }
        /// <summary>Appearance flags, introduced with server side baking, currently unused</summary>
        public AppearanceFlags AppearanceFlags { get { return m_AppearanceFlags; } }

        /// <summary>
        /// Construct a new instance of the AvatarAppearanceEventArgs class
        /// </summary>
        /// <param name="sim">The simulator request was from</param>
        /// <param name="avatarID">The ID of the agent</param>
        /// <param name="isTrial">true of the agent is a trial account</param>
        /// <param name="defaultTexture">The default agent texture</param>
        /// <param name="faceTextures">The agents appearance layer textures</param>
        /// <param name="visualParams">The <see cref="VisualParams"/> for the agent</param>
        public AvatarAppearanceEventArgs(RegionProxy sim, UUID avatarID, bool isTrial, Primitive.TextureEntryFace defaultTexture,
            Primitive.TextureEntryFace[] faceTextures, List<byte> visualParams,
            byte appearanceVersion, int COFVersion, AppearanceFlags appearanceFlags)
        {
            this.m_Simulator = sim;
            this.m_AvatarID = avatarID;
            this.m_IsTrial = isTrial;
            this.m_DefaultTexture = defaultTexture;
            this.m_FaceTextures = faceTextures;
            this.m_VisualParams = visualParams;
            this.m_AppearanceVersion = appearanceVersion;
            this.m_COFVersion = COFVersion;
            this.m_AppearanceFlags = appearanceFlags;
        }
    }

    public class ViewerEffectPointAtEventArgs : EventArgs
    {
        private readonly RegionProxy m_Simulator;
        private readonly UUID m_SourceID;
        private readonly UUID m_TargetID;
        private readonly Vector3d m_TargetPosition;
        private readonly PointAtType m_PointType;
        private readonly float m_Duration;
        private readonly UUID m_EffectID;

        public RegionProxy Simulator { get { return m_Simulator; } }
        public UUID SourceID { get { return m_SourceID; } }
        public UUID TargetID { get { return m_TargetID; } }
        public Vector3d TargetPosition { get { return m_TargetPosition; } }
        public PointAtType PointType { get { return m_PointType; } }
        public float Duration { get { return m_Duration; } }
        public UUID EffectID { get { return m_EffectID; } }

        public ViewerEffectPointAtEventArgs(RegionProxy simulator, UUID sourceID, UUID targetID, Vector3d targetPos, PointAtType pointType, float duration, UUID id)
        {
            this.m_Simulator = simulator;
            this.m_SourceID = sourceID;
            this.m_TargetID = targetID;
            this.m_TargetPosition = targetPos;
            this.m_PointType = pointType;
            this.m_Duration = duration;
            this.m_EffectID = id;
        }
    }

    #endregion
}
