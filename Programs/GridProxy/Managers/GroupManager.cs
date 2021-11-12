using OpenMetaverse;
using OpenMetaverse.Http;
using OpenMetaverse.Interfaces;
using OpenMetaverse.Messages.Linden;
using OpenMetaverse.Packets;
using OpenMetaverse.StructuredData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static GridProxy.RegionManager;

namespace GridProxy
{
    public class GroupManager
    {
        #region Delegates

        /// <summary>The event subscribers. null if no subcribers</summary>
        private EventHandler<CurrentGroupsEventArgs> m_CurrentGroups;

        /// <summary>Raises the CurrentGroups event</summary>
        /// <param name="e">A CurrentGroupsEventArgs object containing the
        /// data sent from the simulator</param>
        protected virtual void OnCurrentGroups(CurrentGroupsEventArgs e)
        {
            EventHandler<CurrentGroupsEventArgs> handler = m_CurrentGroups;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>Thread sync lock object</summary>
        private readonly object m_CurrentGroupsLock = new object();

        /// <summary>Raised when the simulator sends us data containing
        /// our current group membership</summary>
        public event EventHandler<CurrentGroupsEventArgs> CurrentGroups
        {
            add { lock (m_CurrentGroupsLock) { m_CurrentGroups += value; } }
            remove { lock (m_CurrentGroupsLock) { m_CurrentGroups -= value; } }
        }

        /// <summary>The event subscribers. null if no subcribers</summary>
        private EventHandler<GroupNamesEventArgs> m_GroupNames;

        /// <summary>Raises the GroupNamesReply event</summary>
        /// <param name="e">A GroupNamesEventArgs object containing the
        /// data response from the simulator</param>
        protected virtual void OnGroupNamesReply(GroupNamesEventArgs e)
        {
            EventHandler<GroupNamesEventArgs> handler = m_GroupNames;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>Thread sync lock object</summary>
        private readonly object m_GroupNamesLock = new object();

        /// <summary>Raised when the simulator responds to a RequestGroupName 
        /// or RequestGroupNames request</summary>
        public event EventHandler<GroupNamesEventArgs> GroupNamesReply
        {
            add { lock (m_GroupNamesLock) { m_GroupNames += value; } }
            remove { lock (m_GroupNamesLock) { m_GroupNames -= value; } }
        }

        /// <summary>The event subscribers. null if no subcribers</summary>
        private EventHandler<GroupProfileEventArgs> m_GroupProfile;

        /// <summary>Raises the GroupProfile event</summary>
        /// <param name="e">An GroupProfileEventArgs object containing the
        /// data returned from the simulator</param>
        protected virtual void OnGroupProfile(GroupProfileEventArgs e)
        {
            EventHandler<GroupProfileEventArgs> handler = m_GroupProfile;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>Thread sync lock object</summary>
        private readonly object m_GroupProfileLock = new object();

        /// <summary>Raised when the simulator responds to a <see cref="RequestGroupProfile"/> request</summary>
        public event EventHandler<GroupProfileEventArgs> GroupProfile
        {
            add { lock (m_GroupProfileLock) { m_GroupProfile += value; } }
            remove { lock (m_GroupProfileLock) { m_GroupProfile -= value; } }
        }

        /// <summary>The event subscribers. null if no subcribers</summary>
        private EventHandler<GroupMembersReplyEventArgs> m_GroupMembers;

        /// <summary>Raises the GroupMembers event</summary>
        /// <param name="e">A GroupMembersEventArgs object containing the
        /// data returned from the simulator</param>
        protected virtual void OnGroupMembersReply(GroupMembersReplyEventArgs e)
        {
            EventHandler<GroupMembersReplyEventArgs> handler = m_GroupMembers;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>Thread sync lock object</summary>
        private readonly object m_GroupMembersLock = new object();

        /// <summary>Raised when the simulator responds to a <see cref="RequestGroupMembers"/> request</summary>
        public event EventHandler<GroupMembersReplyEventArgs> GroupMembersReply
        {
            add { lock (m_GroupMembersLock) { m_GroupMembers += value; } }
            remove { lock (m_GroupMembersLock) { m_GroupMembers -= value; } }
        }

        /// <summary>The event subscribers. null if no subcribers</summary>
        private EventHandler<GroupRolesDataReplyEventArgs> m_GroupRoles;

        /// <summary>Raises the GroupRolesDataReply event</summary>
        /// <param name="e">A GroupRolesDataReplyEventArgs object containing the
        /// data returned from the simulator</param>
        protected virtual void OnGroupRoleDataReply(GroupRolesDataReplyEventArgs e)
        {
            EventHandler<GroupRolesDataReplyEventArgs> handler = m_GroupRoles;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>Thread sync lock object</summary>
        private readonly object m_GroupRolesLock = new object();

        /// <summary>Raised when the simulator responds to a <see cref="RequestGroupRoleData"/> request</summary>
        public event EventHandler<GroupRolesDataReplyEventArgs> GroupRoleDataReply
        {
            add { lock (m_GroupRolesLock) { m_GroupRoles += value; } }
            remove { lock (m_GroupRolesLock) { m_GroupRoles -= value; } }
        }

        /// <summary>The event subscribers. null if no subcribers</summary>
        private EventHandler<GroupRolesMembersReplyEventArgs> m_GroupRoleMembers;

        /// <summary>Raises the GroupRoleMembersReply event</summary>
        /// <param name="e">A GroupRolesRoleMembersReplyEventArgs object containing the
        /// data returned from the simulator</param>
        protected virtual void OnGroupRoleMembers(GroupRolesMembersReplyEventArgs e)
        {
            EventHandler<GroupRolesMembersReplyEventArgs> handler = m_GroupRoleMembers;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>Thread sync lock object</summary>
        private readonly object m_GroupRolesMembersLock = new object();

        /// <summary>Raised when the simulator responds to a <see cref="RequestGroupRolesMembers"/> request</summary>
        public event EventHandler<GroupRolesMembersReplyEventArgs> GroupRoleMembersReply
        {
            add { lock (m_GroupRolesMembersLock) { m_GroupRoleMembers += value; } }
            remove { lock (m_GroupRolesMembersLock) { m_GroupRoleMembers -= value; } }
        }

        /// <summary>The event subscribers. null if no subcribers</summary>
        private EventHandler<GroupTitlesReplyEventArgs> m_GroupTitles;


        /// <summary>Raises the GroupTitlesReply event</summary>
        /// <param name="e">A GroupTitlesReplyEventArgs object containing the
        /// data returned from the simulator</param>
        protected virtual void OnGroupTitles(GroupTitlesReplyEventArgs e)
        {
            EventHandler<GroupTitlesReplyEventArgs> handler = m_GroupTitles;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>Thread sync lock object</summary>
        private readonly object m_GroupTitlesLock = new object();

        /// <summary>Raised when the simulator responds to a <see cref="RequestGroupTitles"/> request</summary>
        public event EventHandler<GroupTitlesReplyEventArgs> GroupTitlesReply
        {
            add { lock (m_GroupTitlesLock) { m_GroupTitles += value; } }
            remove { lock (m_GroupTitlesLock) { m_GroupTitles -= value; } }
        }

        /// <summary>The event subscribers. null if no subcribers</summary>
        private EventHandler<GroupAccountSummaryReplyEventArgs> m_GroupAccountSummary;

        /// <summary>Raises the GroupAccountSummary event</summary>
        /// <param name="e">A GroupAccountSummaryReplyEventArgs object containing the
        /// data returned from the simulator</param>
        protected virtual void OnGroupAccountSummaryReply(GroupAccountSummaryReplyEventArgs e)
        {
            EventHandler<GroupAccountSummaryReplyEventArgs> handler = m_GroupAccountSummary;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>Thread sync lock object</summary>
        private readonly object m_GroupAccountSummaryLock = new object();

        /// <summary>Raised when a response to a RequestGroupAccountSummary is returned
        /// by the simulator</summary>
        public event EventHandler<GroupAccountSummaryReplyEventArgs> GroupAccountSummaryReply
        {
            add { lock (m_GroupAccountSummaryLock) { m_GroupAccountSummary += value; } }
            remove { lock (m_GroupAccountSummaryLock) { m_GroupAccountSummary -= value; } }
        }

        /// <summary>The event subscribers. null if no subcribers</summary>
        private EventHandler<GroupCreatedReplyEventArgs> m_GroupCreated;

        /// <summary>Raises the GroupCreated event</summary>
        /// <param name="e">An GroupCreatedEventArgs object containing the
        /// data returned from the simulator</param>
        protected virtual void OnGroupCreatedReply(GroupCreatedReplyEventArgs e)
        {
            EventHandler<GroupCreatedReplyEventArgs> handler = m_GroupCreated;
            if (handler != null)
                handler(this, e);
        }
        /// <summary>Thread sync lock object</summary>
        private readonly object m_GroupCreatedLock = new object();

        /// <summary>Raised when a request to create a group is successful</summary>
        public event EventHandler<GroupCreatedReplyEventArgs> GroupCreatedReply
        {
            add { lock (m_GroupCreatedLock) { m_GroupCreated += value; } }
            remove { lock (m_GroupCreatedLock) { m_GroupCreated -= value; } }
        }

        /// <summary>The event subscribers. null if no subcribers</summary>
        private EventHandler<GroupOperationEventArgs> m_GroupJoined;

        /// <summary>Raises the GroupJoined event</summary>
        /// <param name="e">A GroupOperationEventArgs object containing the
        /// result of the operation returned from the simulator</param>
        protected virtual void OnGroupJoinedReply(GroupOperationEventArgs e)
        {
            EventHandler<GroupOperationEventArgs> handler = m_GroupJoined;
            if (handler != null)
                handler(this, e);
        }
        /// <summary>Thread sync lock object</summary>
        private readonly object m_GroupJoinedLock = new object();

        /// <summary>Raised when a request to join a group either
        /// fails or succeeds</summary>
        public event EventHandler<GroupOperationEventArgs> GroupJoinedReply
        {
            add { lock (m_GroupJoinedLock) { m_GroupJoined += value; } }
            remove { lock (m_GroupJoinedLock) { m_GroupJoined -= value; } }
        }

        /// <summary>The event subscribers. null if no subcribers</summary>
        private EventHandler<GroupOperationEventArgs> m_GroupLeft;

        /// <summary>Raises the GroupLeft event</summary>
        /// <param name="e">A GroupOperationEventArgs object containing the
        /// result of the operation returned from the simulator</param>
        protected virtual void OnGroupLeaveReply(GroupOperationEventArgs e)
        {
            EventHandler<GroupOperationEventArgs> handler = m_GroupLeft;
            if (handler != null)
                handler(this, e);
        }
        /// <summary>Thread sync lock object</summary>
        private readonly object m_GroupLeftLock = new object();

        /// <summary>Raised when a request to leave a group either
        /// fails or succeeds</summary>
        public event EventHandler<GroupOperationEventArgs> GroupLeaveReply
        {
            add { lock (m_GroupLeftLock) { m_GroupLeft += value; } }
            remove { lock (m_GroupLeftLock) { m_GroupLeft -= value; } }
        }

        /// <summary>The event subscribers. null if no subcribers</summary>
        private EventHandler<GroupDroppedEventArgs> m_GroupDropped;

        /// <summary>Raises the GroupDropped event</summary>
        /// <param name="e">An GroupDroppedEventArgs object containing the
        /// the group your agent left</param>
        protected virtual void OnGroupDropped(GroupDroppedEventArgs e)
        {
            EventHandler<GroupDroppedEventArgs> handler = m_GroupDropped;
            if (handler != null)
                handler(this, e);
        }
        /// <summary>Thread sync lock object</summary>
        private readonly object m_GroupDroppedLock = new object();

        /// <summary>Raised when A group is removed from the group server</summary>
        public event EventHandler<GroupDroppedEventArgs> GroupDropped
        {
            add { lock (m_GroupDroppedLock) { m_GroupDropped += value; } }
            remove { lock (m_GroupDroppedLock) { m_GroupDropped -= value; } }
        }

        /// <summary>The event subscribers. null if no subcribers</summary>
        private EventHandler<GroupOperationEventArgs> m_GroupMemberEjected;

        /// <summary>Raises the GroupMemberEjected event</summary>
        /// <param name="e">An GroupMemberEjectedEventArgs object containing the
        /// data returned from the simulator</param>
        protected virtual void OnGroupMemberEjected(GroupOperationEventArgs e)
        {
            EventHandler<GroupOperationEventArgs> handler = m_GroupMemberEjected;
            if (handler != null)
                handler(this, e);
        }
        /// <summary>Thread sync lock object</summary>
        private readonly object m_GroupMemberEjectedLock = new object();

        /// <summary>Raised when a request to eject a member from a group either
        /// fails or succeeds</summary>
        public event EventHandler<GroupOperationEventArgs> GroupMemberEjected
        {
            add { lock (m_GroupMemberEjectedLock) { m_GroupMemberEjected += value; } }
            remove { lock (m_GroupMemberEjectedLock) { m_GroupMemberEjected -= value; } }
        }

        /// <summary>The event subscribers. null if no subcribers</summary>
        private EventHandler<GroupNoticesListReplyEventArgs> m_GroupNoticesListReply;

        /// <summary>Raises the GroupNoticesListReply event</summary>
        /// <param name="e">An GroupNoticesListReplyEventArgs object containing the
        /// data returned from the simulator</param>
        protected virtual void OnGroupNoticesListReply(GroupNoticesListReplyEventArgs e)
        {
            EventHandler<GroupNoticesListReplyEventArgs> handler = m_GroupNoticesListReply;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>Thread sync lock object</summary>
        private readonly object m_GroupNoticesListReplyLock = new object();

        /// <summary>Raised when the simulator sends us group notices</summary>
        /// <seealso cref="RequestGroupNoticesList"/>
        public event EventHandler<GroupNoticesListReplyEventArgs> GroupNoticesListReply
        {
            add { lock (m_GroupNoticesListReplyLock) { m_GroupNoticesListReply += value; } }
            remove { lock (m_GroupNoticesListReplyLock) { m_GroupNoticesListReply -= value; } }
        }

        /// <summary>The event subscribers. null if no subcribers</summary>
        private EventHandler<GroupInvitationEventArgs> m_GroupInvitation;

        /// <summary>Raises the GroupInvitation event</summary>
        /// <param name="e">An GroupInvitationEventArgs object containing the
        /// data returned from the simulator</param>
        protected virtual void OnGroupInvitation(GroupInvitationEventArgs e)
        {
            EventHandler<GroupInvitationEventArgs> handler = m_GroupInvitation;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>Thread sync lock object</summary>
        private readonly object m_GroupInvitationLock = new object();

        /// <summary>Raised when another agent invites our avatar to join a group</summary>
        public event EventHandler<GroupInvitationEventArgs> GroupInvitation
        {
            add { lock (m_GroupInvitationLock) { m_GroupInvitation += value; } }
            remove { lock (m_GroupInvitationLock) { m_GroupInvitation -= value; } }
        }

        /// <summary>The event subscribers. null if no subcribers</summary>
        private EventHandler<BannedAgentsEventArgs> m_BannedAgents;

        /// <summary>Raises the BannedAgents event</summary>
        /// <param name="e">An BannedAgentsEventArgs object containing the
        /// data returned from the simulator</param>
        protected virtual void OnBannedAgents(BannedAgentsEventArgs e)
        {
            EventHandler<BannedAgentsEventArgs> handler = m_BannedAgents;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>Thread sync lock object</summary>
        private readonly object m_BannedAgentsLock = new object();

        /// <summary>Raised when another agent invites our avatar to join a group</summary>
        public event EventHandler<BannedAgentsEventArgs> BannedAgents
        {
            add { lock (m_BannedAgentsLock) { m_BannedAgents += value; } }
            remove { lock (m_BannedAgentsLock) { m_BannedAgents -= value; } }
        }

        #endregion Delegates

        private ProxyFrame Proxy;

        /// <summary>Currently-active group members requests</summary>
        private List<UUID> GroupMembersRequests;
        /// <summary>Currently-active group roles requests</summary>
        private List<UUID> GroupRolesRequests;
        /// <summary>Currently-active group role-member requests</summary>
        private List<UUID> GroupRolesMembersRequests;
        /// <summary>Dictionary keeping group members while request is in progress</summary>
        private InternalDictionary<UUID, Dictionary<UUID, GroupMember>> TempGroupMembers;
        /// <summary>Dictionary keeping mebmer/role mapping while request is in progress</summary>
        private InternalDictionary<UUID, List<KeyValuePair<UUID, UUID>>> TempGroupRolesMembers;
        /// <summary>Dictionary keeping GroupRole information while request is in progress</summary>
        private InternalDictionary<UUID, Dictionary<UUID, GroupRole>> TempGroupRoles;
        /// <summary>Caches group name lookups</summary>
        public InternalDictionary<UUID, string> GroupName2KeyCache;
        /// <summary>Dictionary of users groups</summary>
        public InternalDictionary<UUID, Group> GroupList;

        public GroupManager(ProxyFrame frame)
        {
            Proxy = frame;

            TempGroupMembers = new InternalDictionary<UUID, Dictionary<UUID, GroupMember>>();
            GroupMembersRequests = new List<UUID>();
            TempGroupRoles = new InternalDictionary<UUID, Dictionary<UUID, GroupRole>>();
            GroupRolesRequests = new List<UUID>();
            TempGroupRolesMembers = new InternalDictionary<UUID, List<KeyValuePair<UUID, UUID>>>();
            GroupRolesMembersRequests = new List<UUID>();
            GroupName2KeyCache = new InternalDictionary<UUID, string>();
            GroupList = new InternalDictionary<UUID, Group>();

            Proxy.Network.AddEventDelegate("AgentGroupDataUpdate", AgentGroupDataUpdateHandler);

            Proxy.Network.AddDelegate(PacketType.AgentDropGroup, Direction.Incoming, AgentDropGroupHandler);
            Proxy.Network.AddDelegate(PacketType.GroupTitlesReply, Direction.Incoming, GroupTitlesReplyHandler);
            Proxy.Network.AddDelegate(PacketType.GroupProfileReply, Direction.Incoming, GroupProfileReplyHandler);
            Proxy.Network.AddDelegate(PacketType.GroupMembersReply, Direction.Incoming, GroupMembersHandler);
            Proxy.Network.AddDelegate(PacketType.GroupRoleDataReply, Direction.Incoming, GroupRoleDataReplyHandler);
            Proxy.Network.AddDelegate(PacketType.GroupRoleMembersReply, Direction.Incoming, GroupRoleMembersReplyHandler);
            Proxy.Network.AddDelegate(PacketType.GroupActiveProposalItemReply, Direction.Incoming, GroupActiveProposalItemHandler);
            Proxy.Network.AddDelegate(PacketType.GroupVoteHistoryItemReply, Direction.Incoming, GroupVoteHistoryItemHandler);
            Proxy.Network.AddDelegate(PacketType.GroupAccountSummaryReply, Direction.Incoming, GroupAccountSummaryReplyHandler);
            Proxy.Network.AddDelegate(PacketType.CreateGroupReply, Direction.Incoming, CreateGroupReplyHandler);
            Proxy.Network.AddDelegate(PacketType.JoinGroupReply, Direction.Incoming, JoinGroupReplyHandler);
            Proxy.Network.AddDelegate(PacketType.LeaveGroupReply, Direction.Incoming, LeaveGroupReplyHandler);
            Proxy.Network.AddDelegate(PacketType.UUIDGroupNameReply, Direction.Incoming, UUIDGroupNameReplyHandler);
            Proxy.Network.AddDelegate(PacketType.EjectGroupMemberReply, Direction.Incoming, EjectGroupMemberReplyHandler);
            Proxy.Network.AddDelegate(PacketType.GroupNoticesListReply, Direction.Incoming, GroupNoticesListReplyHandler);

            //Proxy.Network.RegisterEventCallback("AgentDropGroup", new Caps.EventQueueCallback(AgentDropGroupMessageHandler));
        }

        private void AgentGroupDataUpdateHandler(string name, IMessage message, RegionManager.RegionProxy region)
        {
            AgentGroupDataUpdateMessage msg = (AgentGroupDataUpdateMessage)message;

            for (int i = 0; i < msg.GroupDataBlock.Length; i++)
            {
                UUID group_id = msg.GroupDataBlock[i].GroupID;

                Group group;
                if (!GroupList.TryGetValue(group_id, out group))
                {
                    group = new Group();
                    group.ID = group_id;
                }

                group.InsigniaID = msg.GroupDataBlock[i].GroupInsigniaID;
                group.Name = msg.GroupDataBlock[i].GroupName;
                group.Contribution = msg.GroupDataBlock[i].Contribution;
                group.AcceptNotices = msg.GroupDataBlock[i].AcceptNotices;
                group.Powers = msg.GroupDataBlock[i].GroupPowers;
                group.ListInProfile = msg.NewGroupDataBlock[i].ListInProfile;

                GroupList.Dictionary[group_id] = group;
            }

            // todo: event
        }

        #region Public Methods

        /// <summary>
        /// Request a current list of groups the avatar is a member of.
        /// </summary>
        /// <remarks>CAPS Event Queue must be running for this to work since the results
        /// come across CAPS.</remarks>
        public void RequestCurrentGroups()
        {
            AgentDataUpdateRequestPacket request = new AgentDataUpdateRequestPacket();

            request.AgentData.AgentID = Proxy.Agent.AgentID;
            request.AgentData.SessionID = Proxy.Agent.SessionID;

            Proxy.Network.InjectPacket(request, Direction.Outgoing);
        }

        /// <summary>
        /// Lookup name of group based on groupID
        /// </summary>
        /// <param name="groupID">groupID of group to lookup name for.</param>
        public void RequestGroupName(UUID groupID)
        {
            // if we already have this in the cache, return from cache instead of making a request
            if (GroupName2KeyCache.ContainsKey(groupID))
            {
                Dictionary<UUID, string> groupNames = new Dictionary<UUID, string>();
                lock (GroupName2KeyCache.Dictionary)
                    groupNames.Add(groupID, GroupName2KeyCache.Dictionary[groupID]);

                if (m_GroupNames != null)
                {
                    OnGroupNamesReply(new GroupNamesEventArgs(groupNames));
                }
            }

            else
            {
                UUIDGroupNameRequestPacket req = new UUIDGroupNameRequestPacket();
                UUIDGroupNameRequestPacket.UUIDNameBlockBlock[] block = new UUIDGroupNameRequestPacket.UUIDNameBlockBlock[1];
                block[0] = new UUIDGroupNameRequestPacket.UUIDNameBlockBlock();
                block[0].ID = groupID;
                req.UUIDNameBlock = block;
                Proxy.Network.InjectPacket(req, Direction.Outgoing);
            }
        }

        /// <summary>
        /// Request lookup of multiple group names
        /// </summary>
        /// <param name="groupIDs">List of group IDs to request.</param>
        public void RequestGroupNames(List<UUID> groupIDs)
        {
            Dictionary<UUID, string> groupNames = new Dictionary<UUID, string>();
            lock (GroupName2KeyCache.Dictionary)
            {
                foreach (UUID groupID in groupIDs)
                {
                    if (GroupName2KeyCache.ContainsKey(groupID))
                        groupNames[groupID] = GroupName2KeyCache.Dictionary[groupID];
                }
            }

            if (groupIDs.Count > 0)
            {
                UUIDGroupNameRequestPacket req = new UUIDGroupNameRequestPacket();
                UUIDGroupNameRequestPacket.UUIDNameBlockBlock[] block = new UUIDGroupNameRequestPacket.UUIDNameBlockBlock[groupIDs.Count];

                for (int i = 0; i < groupIDs.Count; i++)
                {
                    block[i] = new UUIDGroupNameRequestPacket.UUIDNameBlockBlock();
                    block[i].ID = groupIDs[i];
                }

                req.UUIDNameBlock = block;
                Proxy.Network.InjectPacket(req, Direction.Outgoing);
            }

            // fire handler from cache
            if (groupNames.Count > 0 && m_GroupNames != null)
            {
                OnGroupNamesReply(new GroupNamesEventArgs(groupNames));
            }
        }

        /// <summary>Lookup group profile data such as name, enrollment, founder, logo, etc</summary>
        /// <remarks>Subscribe to <code>OnGroupProfile</code> event to receive the results.</remarks>
        /// <param name="group">group ID (UUID)</param>
        public void RequestGroupProfile(UUID group)
        {
            GroupProfileRequestPacket request = new GroupProfileRequestPacket();

            request.AgentData.AgentID = Proxy.Agent.AgentID;
            request.AgentData.SessionID = Proxy.Agent.SessionID;
            request.GroupData.GroupID = group;

            Proxy.Network.InjectPacket(request, Direction.Outgoing);
        }

        /// <summary>Request a list of group members.</summary>
        /// <remarks>Subscribe to <code>OnGroupMembers</code> event to receive the results.</remarks>
        /// <param name="group">group ID (UUID)</param>
        /// <returns>UUID of the request, use to index into cache</returns>
        public UUID RequestGroupMembers(UUID group)
        {
            UUID requestID = UUID.Random();

            if (Proxy.Network.CurrentSim != null && Proxy.Network.CurrentSim.Caps != null &&
                Proxy.Network.CurrentSim.Caps.TryGetValue("GroupMemberData", out string cap))
            {
                Uri url = new Uri(cap);
                CapsClient req = new CapsClient(url);
                req.OnComplete += (client, result, error) =>
                {
                    if (error == null)
                    {
                        GroupMembersHandlerCaps(requestID, result);
                    }
                };

                OSDMap requestData = new OSDMap(1);
                requestData["group_id"] = group;
                req.BeginGetResponse(requestData, OSDFormat.Xml, Proxy.Config.CAPS_TIMEOUT * 4);

                return requestID;
            }

            lock (GroupMembersRequests) GroupMembersRequests.Add(requestID);

            GroupMembersRequestPacket request = new GroupMembersRequestPacket();

            request.AgentData.AgentID = Proxy.Agent.AgentID;
            request.AgentData.SessionID = Proxy.Agent.SessionID;
            request.GroupData.GroupID = group;
            request.GroupData.RequestID = requestID;

            Proxy.Network.InjectPacket(request, Direction.Outgoing);
            return requestID;
        }

        /// <summary>Request group roles</summary>
        /// <remarks>Subscribe to <code>OnGroupRoles</code> event to receive the results.</remarks>
        /// <param name="group">group ID (UUID)</param>
        /// <returns>UUID of the request, use to index into cache</returns>
        public UUID RequestGroupRoles(UUID group)
        {
            UUID requestID = UUID.Random();
            lock (GroupRolesRequests) GroupRolesRequests.Add(requestID);

            GroupRoleDataRequestPacket request = new GroupRoleDataRequestPacket();

            request.AgentData.AgentID = Proxy.Agent.AgentID;
            request.AgentData.SessionID = Proxy.Agent.SessionID;
            request.GroupData.GroupID = group;
            request.GroupData.RequestID = requestID;

            Proxy.Network.InjectPacket(request, Direction.Outgoing);
            return requestID;
        }

        /// <summary>Request members (members,role) role mapping for a group.</summary>
        /// <remarks>Subscribe to <code>OnGroupRolesMembers</code> event to receive the results.</remarks>
        /// <param name="group">group ID (UUID)</param>
        /// <returns>UUID of the request, use to index into cache</returns>
        public UUID RequestGroupRolesMembers(UUID group)
        {
            UUID requestID = UUID.Random();
            lock (GroupRolesRequests) GroupRolesMembersRequests.Add(requestID);

            GroupRoleMembersRequestPacket request = new GroupRoleMembersRequestPacket();
            request.AgentData.AgentID = Proxy.Agent.AgentID;
            request.AgentData.SessionID = Proxy.Agent.SessionID;
            request.GroupData.GroupID = group;
            request.GroupData.RequestID = requestID;
            Proxy.Network.InjectPacket(request, Direction.Outgoing);
            return requestID;
        }

        /// <summary>Request a groups Titles</summary>
        /// <remarks>Subscribe to <code>OnGroupTitles</code> event to receive the results.</remarks>
        /// <param name="group">group ID (UUID)</param>
        /// <returns>UUID of the request, use to index into cache</returns>
        public UUID RequestGroupTitles(UUID group)
        {
            UUID requestID = UUID.Random();

            GroupTitlesRequestPacket request = new GroupTitlesRequestPacket();

            request.AgentData.AgentID = Proxy.Agent.AgentID;
            request.AgentData.SessionID = Proxy.Agent.SessionID;
            request.AgentData.GroupID = group;
            request.AgentData.RequestID = requestID;

            Proxy.Network.InjectPacket(request, Direction.Outgoing);
            return requestID;
        }

        /// <summary>Begin to get the group account summary</summary>
        /// <remarks>Subscribe to the <code>OnGroupAccountSummary</code> event to receive the results.</remarks>
        /// <param name="group">group ID (UUID)</param>
        /// <param name="intervalDays">How long of an interval</param>
        /// <param name="currentInterval">Which interval (0 for current, 1 for last)</param>
        public void RequestGroupAccountSummary(UUID group, int intervalDays, int currentInterval)
        {
            GroupAccountSummaryRequestPacket p = new GroupAccountSummaryRequestPacket();
            p.AgentData.AgentID = Proxy.Agent.AgentID;
            p.AgentData.SessionID = Proxy.Agent.SessionID;
            p.AgentData.GroupID = group;
            p.MoneyData.RequestID = UUID.Random();
            p.MoneyData.CurrentInterval = currentInterval;
            p.MoneyData.IntervalDays = intervalDays;
            Proxy.Network.InjectPacket(p, Direction.Outgoing);
        }

        /// <summary>Invites a user to a group</summary>
        /// <param name="group">The group to invite to</param>
        /// <param name="roles">A list of roles to invite a person to</param>
        /// <param name="personkey">Key of person to invite</param>
        public void Invite(UUID group, List<UUID> roles, UUID personkey)
        {
            InviteGroupRequestPacket igp = new InviteGroupRequestPacket();

            igp.AgentData = new InviteGroupRequestPacket.AgentDataBlock();
            igp.AgentData.AgentID = Proxy.Agent.AgentID;
            igp.AgentData.SessionID = Proxy.Agent.SessionID;

            igp.GroupData = new InviteGroupRequestPacket.GroupDataBlock();
            igp.GroupData.GroupID = group;

            igp.InviteData = new InviteGroupRequestPacket.InviteDataBlock[roles.Count];

            for (int i = 0; i < roles.Count; i++)
            {
                igp.InviteData[i] = new InviteGroupRequestPacket.InviteDataBlock();
                igp.InviteData[i].InviteeID = personkey;
                igp.InviteData[i].RoleID = roles[i];
            }

            Proxy.Network.InjectPacket(igp, Direction.Outgoing);
        }

        /// <summary>Set a group as the current active group</summary>
        /// <param name="id">group ID (UUID)</param>
        public void ActivateGroup(UUID id)
        {
            ActivateGroupPacket activate = new ActivateGroupPacket();
            activate.AgentData.AgentID = Proxy.Agent.AgentID;
            activate.AgentData.SessionID = Proxy.Agent.SessionID;
            activate.AgentData.GroupID = id;

            Proxy.Network.InjectPacket(activate, Direction.Outgoing);
        }

        /// <summary>Change the role that determines your active title</summary>
        /// <param name="group">Group ID to use</param>
        /// <param name="role">Role ID to change to</param>
        public void ActivateTitle(UUID group, UUID role)
        {
            GroupTitleUpdatePacket gtu = new GroupTitleUpdatePacket();
            gtu.AgentData.AgentID = Proxy.Agent.AgentID;
            gtu.AgentData.SessionID = Proxy.Agent.SessionID;
            gtu.AgentData.TitleRoleID = role;
            gtu.AgentData.GroupID = group;

            Proxy.Network.InjectPacket(gtu, Direction.Outgoing);
        }

        /// <summary>Set this avatar's tier contribution</summary>
        /// <param name="group">Group ID to change tier in</param>
        /// <param name="contribution">amount of tier to donate</param>
        public void SetGroupContribution(UUID group, int contribution)
        {
            SetGroupContributionPacket sgp = new SetGroupContributionPacket();
            sgp.AgentData.AgentID = Proxy.Agent.AgentID;
            sgp.AgentData.SessionID = Proxy.Agent.SessionID;
            sgp.Data.GroupID = group;
            sgp.Data.Contribution = contribution;

            Proxy.Network.InjectPacket(sgp, Direction.Outgoing);
        }

        /// <summary>
        /// Save wheather agent wants to accept group notices and list this group in their profile
        /// </summary>
        /// <param name="groupID">Group <see cref="UUID"/></param>
        /// <param name="acceptNotices">Accept notices from this group</param>
        /// <param name="listInProfile">List this group in the profile</param>
        public void SetGroupAcceptNotices(UUID groupID, bool acceptNotices, bool listInProfile)
        {
            SetGroupAcceptNoticesPacket p = new SetGroupAcceptNoticesPacket();
            p.AgentData.AgentID = Proxy.Agent.AgentID;
            p.AgentData.SessionID = Proxy.Agent.SessionID;
            p.Data.GroupID = groupID;
            p.Data.AcceptNotices = acceptNotices;
            p.NewData.ListInProfile = listInProfile;

            Proxy.Network.InjectPacket(p, Direction.Outgoing);
        }

        /// <summary>Request to join a group</summary>
        /// <remarks>Subscribe to <code>OnGroupJoined</code> event for confirmation.</remarks>
        /// <param name="id">group ID (UUID) to join.</param>
        public void RequestJoinGroup(UUID id)
        {
            JoinGroupRequestPacket join = new JoinGroupRequestPacket();
            join.AgentData.AgentID = Proxy.Agent.AgentID;
            join.AgentData.SessionID = Proxy.Agent.SessionID;

            join.GroupData.GroupID = id;

            Proxy.Network.InjectPacket(join, Direction.Outgoing);
        }

        /// <summary>
        /// Request to create a new group. If the group is successfully
        /// created, L$100 will automatically be deducted
        /// </summary>
        /// <remarks>Subscribe to <code>OnGroupCreated</code> event to receive confirmation.</remarks>
        /// <param name="group">Group struct containing the new group info</param>
        public void RequestCreateGroup(Group group)
        {
            OpenMetaverse.Packets.CreateGroupRequestPacket cgrp = new CreateGroupRequestPacket();
            cgrp.AgentData = new CreateGroupRequestPacket.AgentDataBlock();
            cgrp.AgentData.AgentID = Proxy.Agent.AgentID;
            cgrp.AgentData.SessionID = Proxy.Agent.SessionID;

            cgrp.GroupData = new CreateGroupRequestPacket.GroupDataBlock();
            cgrp.GroupData.AllowPublish = group.AllowPublish;
            cgrp.GroupData.Charter = Utils.StringToBytes(group.Charter);
            cgrp.GroupData.InsigniaID = group.InsigniaID;
            cgrp.GroupData.MaturePublish = group.MaturePublish;
            cgrp.GroupData.MembershipFee = group.MembershipFee;
            cgrp.GroupData.Name = Utils.StringToBytes(group.Name);
            cgrp.GroupData.OpenEnrollment = group.OpenEnrollment;
            cgrp.GroupData.ShowInList = group.ShowInList;

            Proxy.Network.InjectPacket(cgrp, Direction.Outgoing);
        }

        /// <summary>Update a group's profile and other information</summary>
        /// <param name="id">Groups ID (UUID) to update.</param>
        /// <param name="group">Group struct to update.</param>
        public void UpdateGroup(UUID id, Group group)
        {
            OpenMetaverse.Packets.UpdateGroupInfoPacket cgrp = new UpdateGroupInfoPacket();
            cgrp.AgentData = new UpdateGroupInfoPacket.AgentDataBlock();
            cgrp.AgentData.AgentID = Proxy.Agent.AgentID;
            cgrp.AgentData.SessionID = Proxy.Agent.SessionID;

            cgrp.GroupData = new UpdateGroupInfoPacket.GroupDataBlock();
            cgrp.GroupData.GroupID = id;
            cgrp.GroupData.AllowPublish = group.AllowPublish;
            cgrp.GroupData.Charter = Utils.StringToBytes(group.Charter);
            cgrp.GroupData.InsigniaID = group.InsigniaID;
            cgrp.GroupData.MaturePublish = group.MaturePublish;
            cgrp.GroupData.MembershipFee = group.MembershipFee;
            cgrp.GroupData.OpenEnrollment = group.OpenEnrollment;
            cgrp.GroupData.ShowInList = group.ShowInList;

            Proxy.Network.InjectPacket(cgrp, Direction.Outgoing);
        }

        /// <summary>Eject a user from a group</summary>
        /// <param name="group">Group ID to eject the user from</param>
        /// <param name="member">Avatar's key to eject</param>
        public void EjectUser(UUID group, UUID member)
        {
            OpenMetaverse.Packets.EjectGroupMemberRequestPacket eject = new EjectGroupMemberRequestPacket();
            eject.AgentData = new EjectGroupMemberRequestPacket.AgentDataBlock();
            eject.AgentData.AgentID = Proxy.Agent.AgentID;
            eject.AgentData.SessionID = Proxy.Agent.SessionID;

            eject.GroupData = new EjectGroupMemberRequestPacket.GroupDataBlock();
            eject.GroupData.GroupID = group;

            eject.EjectData = new EjectGroupMemberRequestPacket.EjectDataBlock[1];
            eject.EjectData[0] = new EjectGroupMemberRequestPacket.EjectDataBlock();
            eject.EjectData[0].EjecteeID = member;

            Proxy.Network.InjectPacket(eject, Direction.Outgoing);
        }

        /// <summary>Update role information</summary>
        /// <param name="role">Modified role to be updated</param>
        public void UpdateRole(GroupRole role)
        {
            OpenMetaverse.Packets.GroupRoleUpdatePacket gru = new GroupRoleUpdatePacket();
            gru.AgentData.AgentID = Proxy.Agent.AgentID;
            gru.AgentData.SessionID = Proxy.Agent.SessionID;
            gru.AgentData.GroupID = role.GroupID;
            gru.RoleData = new GroupRoleUpdatePacket.RoleDataBlock[1];
            gru.RoleData[0] = new GroupRoleUpdatePacket.RoleDataBlock();
            gru.RoleData[0].Name = Utils.StringToBytes(role.Name);
            gru.RoleData[0].Description = Utils.StringToBytes(role.Description);
            gru.RoleData[0].Powers = (ulong)role.Powers;
            gru.RoleData[0].RoleID = role.ID;
            gru.RoleData[0].Title = Utils.StringToBytes(role.Title);
            gru.RoleData[0].UpdateType = (byte)GroupRoleUpdate.UpdateAll;
            Proxy.Network.InjectPacket(gru, Direction.Outgoing);
        }

        /// <summary>Create a new group role</summary>
        /// <param name="group">Group ID to update</param>
        /// <param name="role">Role to create</param>
        public void CreateRole(UUID group, GroupRole role)
        {
            OpenMetaverse.Packets.GroupRoleUpdatePacket gru = new GroupRoleUpdatePacket();
            gru.AgentData.AgentID = Proxy.Agent.AgentID;
            gru.AgentData.SessionID = Proxy.Agent.SessionID;
            gru.AgentData.GroupID = group;
            gru.RoleData = new GroupRoleUpdatePacket.RoleDataBlock[1];
            gru.RoleData[0] = new GroupRoleUpdatePacket.RoleDataBlock();
            gru.RoleData[0].RoleID = UUID.Random();
            gru.RoleData[0].Name = Utils.StringToBytes(role.Name);
            gru.RoleData[0].Description = Utils.StringToBytes(role.Description);
            gru.RoleData[0].Powers = (ulong)role.Powers;
            gru.RoleData[0].Title = Utils.StringToBytes(role.Title);
            gru.RoleData[0].UpdateType = (byte)GroupRoleUpdate.Create;
            Proxy.Network.InjectPacket(gru, Direction.Outgoing);
        }

        /// <summary>Delete a group role</summary>
        /// <param name="group">Group ID to update</param>
        /// <param name="roleID">Role to delete</param>
        public void DeleteRole(UUID group, UUID roleID)
        {
            OpenMetaverse.Packets.GroupRoleUpdatePacket gru = new GroupRoleUpdatePacket();
            gru.AgentData.AgentID = Proxy.Agent.AgentID;
            gru.AgentData.SessionID = Proxy.Agent.SessionID;
            gru.AgentData.GroupID = group;
            gru.RoleData = new GroupRoleUpdatePacket.RoleDataBlock[1];
            gru.RoleData[0] = new GroupRoleUpdatePacket.RoleDataBlock();
            gru.RoleData[0].RoleID = roleID;
            gru.RoleData[0].Name = Utils.StringToBytes(string.Empty);
            gru.RoleData[0].Description = Utils.StringToBytes(string.Empty);
            gru.RoleData[0].Powers = 0u;
            gru.RoleData[0].Title = Utils.StringToBytes(string.Empty);
            gru.RoleData[0].UpdateType = (byte)GroupRoleUpdate.Delete;
            Proxy.Network.InjectPacket(gru, Direction.Outgoing);
        }

        /// <summary>Remove an avatar from a role</summary>
        /// <param name="group">Group ID to update</param>
        /// <param name="role">Role ID to be removed from</param>
        /// <param name="member">Avatar's Key to remove</param>
        public void RemoveFromRole(UUID group, UUID role, UUID member)
        {
            OpenMetaverse.Packets.GroupRoleChangesPacket grc = new GroupRoleChangesPacket();
            grc.AgentData.AgentID = Proxy.Agent.AgentID;
            grc.AgentData.SessionID = Proxy.Agent.SessionID;
            grc.AgentData.GroupID = group;
            grc.RoleChange = new GroupRoleChangesPacket.RoleChangeBlock[1];
            grc.RoleChange[0] = new GroupRoleChangesPacket.RoleChangeBlock();
            //Add to members and role
            grc.RoleChange[0].MemberID = member;
            grc.RoleChange[0].RoleID = role;
            //1 = Remove From Role TODO: this should be in an enum
            grc.RoleChange[0].Change = 1;
            Proxy.Network.InjectPacket(grc, Direction.Outgoing);
        }

        /// <summary>Assign an avatar to a role</summary>
        /// <param name="group">Group ID to update</param>
        /// <param name="role">Role ID to assign to</param>
        /// <param name="member">Avatar's ID to assign to role</param>
        public void AddToRole(UUID group, UUID role, UUID member)
        {
            OpenMetaverse.Packets.GroupRoleChangesPacket grc = new GroupRoleChangesPacket();
            grc.AgentData.AgentID = Proxy.Agent.AgentID;
            grc.AgentData.SessionID = Proxy.Agent.SessionID;
            grc.AgentData.GroupID = group;
            grc.RoleChange = new GroupRoleChangesPacket.RoleChangeBlock[1];
            grc.RoleChange[0] = new GroupRoleChangesPacket.RoleChangeBlock();
            //Add to members and role
            grc.RoleChange[0].MemberID = member;
            grc.RoleChange[0].RoleID = role;
            //0 = Add to Role TODO: this should be in an enum
            grc.RoleChange[0].Change = 0;
            Proxy.Network.InjectPacket(grc, Direction.Outgoing);
        }

        /// <summary>Request the group notices list</summary>
        /// <param name="group">Group ID to fetch notices for</param>
        public void RequestGroupNoticesList(UUID group)
        {
            OpenMetaverse.Packets.GroupNoticesListRequestPacket gnl = new GroupNoticesListRequestPacket();
            gnl.AgentData.AgentID = Proxy.Agent.AgentID;
            gnl.AgentData.SessionID = Proxy.Agent.SessionID;
            gnl.Data.GroupID = group;
            Proxy.Network.InjectPacket(gnl, Direction.Outgoing);
        }

        /// <summary>Request a group notice by key</summary>
        /// <param name="noticeID">ID of group notice</param>
        public void RequestGroupNotice(UUID noticeID)
        {
            OpenMetaverse.Packets.GroupNoticeRequestPacket gnr = new GroupNoticeRequestPacket();
            gnr.AgentData.AgentID = Proxy.Agent.AgentID;
            gnr.AgentData.SessionID = Proxy.Agent.SessionID;
            gnr.Data.GroupNoticeID = noticeID;
            Proxy.Network.InjectPacket(gnr, Direction.Outgoing);
        }

        /// <summary>Send out a group notice</summary>
        /// <param name="group">Group ID to update</param>
        /// <param name="notice"><code>GroupNotice</code> structure containing notice data</param>
        public void SendGroupNotice(UUID group, GroupNotice notice)
        {
            Proxy.Agent.InstantMessage(Proxy.Agent.Name, group, notice.Subject + "|" + notice.Message,
                UUID.Zero, InstantMessageDialog.GroupNotice, InstantMessageOnline.Online,
                Vector3.Zero, UUID.Zero, notice.SerializeAttachment());
        }

        /// <summary>Start a group proposal (vote)</summary>
        /// <param name="group">The Group ID to send proposal to</param>
        /// <param name="prop"><code>GroupProposal</code> structure containing the proposal</param>
        public void StartProposal(UUID group, GroupProposal prop)
        {
            StartGroupProposalPacket p = new StartGroupProposalPacket();
            p.AgentData.AgentID = Proxy.Agent.AgentID;
            p.AgentData.SessionID = Proxy.Agent.SessionID;
            p.ProposalData.GroupID = group;
            p.ProposalData.ProposalText = Utils.StringToBytes(prop.VoteText);
            p.ProposalData.Quorum = prop.Quorum;
            p.ProposalData.Majority = prop.Majority;
            p.ProposalData.Duration = prop.Duration;
            Proxy.Network.InjectPacket(p, Direction.Outgoing);
        }

        /// <summary>Request to leave a group</summary>
        /// <remarks>Subscribe to <code>OnGroupLeft</code> event to receive confirmation</remarks>
        /// <param name="groupID">The group to leave</param>
        public void LeaveGroup(UUID groupID)
        {
            LeaveGroupRequestPacket p = new LeaveGroupRequestPacket();
            p.AgentData.AgentID = Proxy.Agent.AgentID;
            p.AgentData.SessionID = Proxy.Agent.SessionID;
            p.GroupData.GroupID = groupID;

            Proxy.Network.InjectPacket(p, Direction.Outgoing);
        }

        /// <summary>
        /// Gets the URI of the cpability for handling group bans
        /// </summary>
        /// <param name="groupID">Group ID</param>
        /// <returns>null, if the feature is not supported, or URI of the capability</returns>
        public Uri GetGroupAPIUri(UUID groupID)
        {
            Uri ret = null;

            if (Proxy.LoggedIn
                && Proxy.Network.CurrentSim != null
                && Proxy.Network.CurrentSim.Caps.Count > 0)
            {
                if(Proxy.Network.CurrentSim.Caps.TryGetValue("GroupAPIv1", out string url))
                {
                    ret = new Uri(string.Format("{0}?group_id={1}", url, groupID.ToString()));
                }
            }

            return ret;
        }

        /// <summary>
        /// Request a list of residents banned from joining a group
        /// </summary>
        /// <param name="groupID">UUID of the group</param>
        public void RequestBannedAgents(UUID groupID)
        {
            RequestBannedAgents(groupID, null);
        }

        /// <summary>
        /// Request a list of residents banned from joining a group
        /// </summary>
        /// <param name="groupID">UUID of the group</param>
        /// <param name="callback">Callback on request completition</param>
        public void RequestBannedAgents(UUID groupID, EventHandler<BannedAgentsEventArgs> callback)
        {
            Uri uri = GetGroupAPIUri(groupID);
            if (uri == null) return;

            CapsClient req = new CapsClient(uri);
            req.OnComplete += (client, result, error) =>
            {
                try
                {

                    if (error != null)
                    {
                        throw error;
                    }
                    else
                    {
                        UUID gid = ((OSDMap)result)["group_id"];
                        var banList = (OSDMap)((OSDMap)result)["ban_list"];
                        Dictionary<UUID, DateTime> bannedAgents = new Dictionary<UUID, DateTime>(banList.Count);

                        foreach (var id in banList.Keys)
                        {
                            bannedAgents[new UUID(id)] = ((OSDMap)banList[id])["ban_date"].AsDate();
                        }

                        var ret = new BannedAgentsEventArgs(groupID, true, bannedAgents);
                        OnBannedAgents(ret);
                        if (callback != null) try { callback(this, ret); }
                            catch { }
                    }

                }
                catch (Exception ex)
                {
                    Logger.Log("Failed to get a list of banned group members: " + ex.Message, Helpers.LogLevel.Warning);
                    var ret = new BannedAgentsEventArgs(groupID, false, null);
                    OnBannedAgents(ret);
                    if (callback != null) try { callback(this, ret); }
                        catch { }
                }

            };

            req.BeginGetResponse(Proxy.Config.CAPS_TIMEOUT);
        }

        /// <summary>
        /// Request that group of agents be banned or unbanned from the group
        /// </summary>
        /// <param name="groupID">Group ID</param>
        /// <param name="action">Ban/Unban action</param>
        /// <param name="agents">Array of agents UUIDs to ban</param>
        public void RequestBanAction(UUID groupID, GroupBanAction action, UUID[] agents)
        {
            RequestBanAction(groupID, action, agents, null);
        }

        /// <summary>
        /// Request that group of agents be banned or unbanned from the group
        /// </summary>
        /// <param name="groupID">Group ID</param>
        /// <param name="action">Ban/Unban action</param>
        /// <param name="agents">Array of agents UUIDs to ban</param>
        /// <param name="callback">Callback</param>
        public void RequestBanAction(UUID groupID, GroupBanAction action, UUID[] agents, EventHandler<EventArgs> callback)
        {
            Uri uri = GetGroupAPIUri(groupID);
            if (uri == null) return;

            CapsClient req = new CapsClient(uri);
            req.OnComplete += (client, result, error) =>
            {
                if (callback != null) try { callback(this, EventArgs.Empty); }
                    catch { }
            };

            OSDMap OSDRequest = new OSDMap();
            OSDRequest["ban_action"] = (int)action;
            OSDArray banIDs = new OSDArray(agents.Length);
            foreach (var agent in agents)
            {
                banIDs.Add(agent);
            }
            OSDRequest["ban_ids"] = banIDs;

            req.BeginGetResponse(OSDRequest, OSDFormat.Xml, Proxy.Config.CAPS_TIMEOUT);
        }



        #endregion

        #region Packet Handlers

        protected void AgentGroupDataUpdateMessageHandler(string capsKey, IMessage message, Simulator simulator)
        {
            if (m_CurrentGroups != null)
            {
                AgentGroupDataUpdateMessage msg = (AgentGroupDataUpdateMessage)message;

                Dictionary<UUID, Group> currentGroups = new Dictionary<UUID, Group>();
                for (int i = 0; i < msg.GroupDataBlock.Length; i++)
                {
                    Group group = new Group();
                    group.ID = msg.GroupDataBlock[i].GroupID;
                    group.InsigniaID = msg.GroupDataBlock[i].GroupInsigniaID;
                    group.Name = msg.GroupDataBlock[i].GroupName;
                    group.Contribution = msg.GroupDataBlock[i].Contribution;
                    group.AcceptNotices = msg.GroupDataBlock[i].AcceptNotices;
                    group.Powers = msg.GroupDataBlock[i].GroupPowers;
                    group.ListInProfile = msg.NewGroupDataBlock[i].ListInProfile;

                    currentGroups.Add(group.ID, group);

                    lock (GroupName2KeyCache.Dictionary)
                    {
                        if (!GroupName2KeyCache.Dictionary.ContainsKey(group.ID))
                            GroupName2KeyCache.Dictionary.Add(group.ID, group.Name);
                    }
                }
                OnCurrentGroups(new CurrentGroupsEventArgs(currentGroups));
            }
        }

        /// <summary>Process an incoming packet and raise the appropriate events</summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">The EventArgs object containing the packet data</param>
        protected Packet AgentDropGroupHandler(Packet packet, RegionProxy sim)
        {
            if (m_GroupDropped != null)
            {
                OnGroupDropped(new GroupDroppedEventArgs(((AgentDropGroupPacket)packet).AgentData.GroupID));
            }

            return packet;
        }

        protected void AgentDropGroupMessageHandler(string capsKey, IMessage message, Simulator simulator)
        {
            if (m_GroupDropped != null)
            {
                AgentDropGroupMessage msg = (AgentDropGroupMessage)message;
                for (int i = 0; i < msg.AgentDataBlock.Length; i++)
                {
                    OnGroupDropped(new GroupDroppedEventArgs(msg.AgentDataBlock[i].GroupID));
                }
            }
        }

        /// <summary>Process an incoming packet and raise the appropriate events</summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">The EventArgs object containing the packet data</param>
        protected Packet GroupProfileReplyHandler(Packet packet, RegionProxy sim)
        {
            if (m_GroupProfile != null)
            {
                GroupProfileReplyPacket profile = (GroupProfileReplyPacket)packet;
                Group group = new Group();

                group.ID = profile.GroupData.GroupID;
                group.AllowPublish = profile.GroupData.AllowPublish;
                group.Charter = Utils.BytesToString(profile.GroupData.Charter);
                group.FounderID = profile.GroupData.FounderID;
                group.GroupMembershipCount = profile.GroupData.GroupMembershipCount;
                group.GroupRolesCount = profile.GroupData.GroupRolesCount;
                group.InsigniaID = profile.GroupData.InsigniaID;
                group.MaturePublish = profile.GroupData.MaturePublish;
                group.MembershipFee = profile.GroupData.MembershipFee;
                group.MemberTitle = Utils.BytesToString(profile.GroupData.MemberTitle);
                group.Money = profile.GroupData.Money;
                group.Name = Utils.BytesToString(profile.GroupData.Name);
                group.OpenEnrollment = profile.GroupData.OpenEnrollment;
                group.OwnerRole = profile.GroupData.OwnerRole;
                group.Powers = (GroupPowers)profile.GroupData.PowersMask;
                group.ShowInList = profile.GroupData.ShowInList;

                OnGroupProfile(new GroupProfileEventArgs(group));
            }

            return packet;
        }

        /// <summary>Process an incoming packet and raise the appropriate events</summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">The EventArgs object containing the packet data</param>
        protected Packet GroupNoticesListReplyHandler(Packet packet, RegionProxy sim)
        {
            if (m_GroupNoticesListReply != null)
            {
                GroupNoticesListReplyPacket reply = (GroupNoticesListReplyPacket)packet;

                List<GroupNoticesListEntry> notices = new List<GroupNoticesListEntry>();

                foreach (GroupNoticesListReplyPacket.DataBlock entry in reply.Data)
                {
                    GroupNoticesListEntry notice = new GroupNoticesListEntry();
                    notice.FromName = Utils.BytesToString(entry.FromName);
                    notice.Subject = Utils.BytesToString(entry.Subject);
                    notice.NoticeID = entry.NoticeID;
                    notice.Timestamp = entry.Timestamp;
                    notice.HasAttachment = entry.HasAttachment;
                    notice.AssetType = (AssetType)entry.AssetType;

                    notices.Add(notice);
                }

                OnGroupNoticesListReply(new GroupNoticesListReplyEventArgs(reply.AgentData.GroupID, notices));
            }

            return packet;
        }

        /// <summary>Process an incoming packet and raise the appropriate events</summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">The EventArgs object containing the packet data</param>
        protected Packet GroupTitlesReplyHandler(Packet packet, RegionProxy sim)
        {
            if (m_GroupTitles != null)
            {
                GroupTitlesReplyPacket titles = (GroupTitlesReplyPacket)packet;
                Dictionary<UUID, GroupTitle> groupTitleCache = new Dictionary<UUID, GroupTitle>();

                foreach (GroupTitlesReplyPacket.GroupDataBlock block in titles.GroupData)
                {
                    GroupTitle groupTitle = new GroupTitle();

                    groupTitle.GroupID = titles.AgentData.GroupID;
                    groupTitle.RoleID = block.RoleID;
                    groupTitle.Title = Utils.BytesToString(block.Title);
                    groupTitle.Selected = block.Selected;

                    groupTitleCache[block.RoleID] = groupTitle;
                }
                OnGroupTitles(new GroupTitlesReplyEventArgs(titles.AgentData.RequestID, titles.AgentData.GroupID, groupTitleCache));
            }

            return packet;
        }

        /// <summary>Process an incoming packet and raise the appropriate events</summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">The EventArgs object containing the packet data</param>
        protected Packet GroupMembersHandler(Packet packet, RegionProxy sim)
        {
            GroupMembersReplyPacket members = (GroupMembersReplyPacket)packet;
            Dictionary<UUID, GroupMember> groupMemberCache = null;

            lock (GroupMembersRequests)
            {
                // If nothing is registered to receive this RequestID drop the data
                if (GroupMembersRequests.Contains(members.GroupData.RequestID))
                {
                    lock (TempGroupMembers.Dictionary)
                    {
                        if (!TempGroupMembers.TryGetValue(members.GroupData.RequestID, out groupMemberCache))
                        {
                            groupMemberCache = new Dictionary<UUID, GroupMember>();
                            TempGroupMembers[members.GroupData.RequestID] = groupMemberCache;
                        }

                        foreach (GroupMembersReplyPacket.MemberDataBlock block in members.MemberData)
                        {
                            GroupMember groupMember = new GroupMember();

                            groupMember.ID = block.AgentID;
                            groupMember.Contribution = block.Contribution;
                            groupMember.IsOwner = block.IsOwner;
                            groupMember.OnlineStatus = Utils.BytesToString(block.OnlineStatus);
                            groupMember.Powers = (GroupPowers)block.AgentPowers;
                            groupMember.Title = Utils.BytesToString(block.Title);

                            groupMemberCache[block.AgentID] = groupMember;
                        }

                        if (groupMemberCache.Count >= members.GroupData.MemberCount)
                        {
                            GroupMembersRequests.Remove(members.GroupData.RequestID);
                            TempGroupMembers.Remove(members.GroupData.RequestID);
                        }
                    }
                }
            }

            if (m_GroupMembers != null && groupMemberCache != null && groupMemberCache.Count >= members.GroupData.MemberCount)
            {
                OnGroupMembersReply(new GroupMembersReplyEventArgs(members.GroupData.RequestID, members.GroupData.GroupID, groupMemberCache));
            }

            return packet;
        }

        protected void GroupMembersHandlerCaps(UUID requestID, OSD result)
        {
            try
            {
                OSDMap res = (OSDMap)result;
                int memberCount = res["member_count"];
                OSDArray titlesOSD = (OSDArray)res["titles"];
                string[] titles = new string[titlesOSD.Count];
                for (int i = 0; i < titlesOSD.Count; i++)
                {
                    titles[i] = titlesOSD[i];
                }
                UUID groupID = res["group_id"];
                GroupPowers defaultPowers = (GroupPowers)(ulong)((OSDMap)res["defaults"])["default_powers"];
                OSDMap membersOSD = (OSDMap)res["members"];
                Dictionary<UUID, GroupMember> groupMembers = new Dictionary<UUID, GroupMember>(membersOSD.Count);
                foreach (var memberID in membersOSD.Keys)
                {
                    OSDMap member = (OSDMap)membersOSD[memberID];

                    GroupMember groupMember = new GroupMember();
                    groupMember.ID = (UUID)memberID;
                    groupMember.Contribution = member["donated_square_meters"];
                    groupMember.IsOwner = "Y" == member["owner"].AsString();
                    groupMember.OnlineStatus = member["last_login"];
                    groupMember.Powers = defaultPowers;
                    if (member.ContainsKey("powers"))
                    {
                        groupMember.Powers = (GroupPowers)(ulong)member["powers"];
                    }
                    groupMember.Title = titles[(int)member["title"]];

                    groupMembers[groupMember.ID] = groupMember;
                }

                OnGroupMembersReply(new GroupMembersReplyEventArgs(requestID, groupID, groupMembers));
            }
            catch (Exception ex)
            {
                Logger.Log("Failed to decode result of GroupMemberData capability: ", Helpers.LogLevel.Error, ex);
            }
        }

        /// <summary>Process an incoming packet and raise the appropriate events</summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">The EventArgs object containing the packet data</param>
        protected Packet GroupRoleDataReplyHandler(Packet packet, RegionProxy sim)
        {
            GroupRoleDataReplyPacket roles = (GroupRoleDataReplyPacket)packet;
            Dictionary<UUID, GroupRole> groupRoleCache = null;

            lock (GroupRolesRequests)
            {
                // If nothing is registered to receive this RequestID drop the data
                if (GroupRolesRequests.Contains(roles.GroupData.RequestID))
                {
                    lock (TempGroupRoles.Dictionary)
                    {
                        if (!TempGroupRoles.TryGetValue(roles.GroupData.RequestID, out groupRoleCache))
                        {
                            groupRoleCache = new Dictionary<UUID, GroupRole>();
                            TempGroupRoles[roles.GroupData.RequestID] = groupRoleCache;
                        }

                        foreach (GroupRoleDataReplyPacket.RoleDataBlock block in roles.RoleData)
                        {
                            GroupRole groupRole = new GroupRole();

                            groupRole.GroupID = roles.GroupData.GroupID;
                            groupRole.ID = block.RoleID;
                            groupRole.Description = Utils.BytesToString(block.Description);
                            groupRole.Name = Utils.BytesToString(block.Name);
                            groupRole.Powers = (GroupPowers)block.Powers;
                            groupRole.Title = Utils.BytesToString(block.Title);

                            groupRoleCache[block.RoleID] = groupRole;
                        }

                        if (groupRoleCache.Count >= roles.GroupData.RoleCount)
                        {
                            GroupRolesRequests.Remove(roles.GroupData.RequestID);
                            TempGroupRoles.Remove(roles.GroupData.RequestID);
                        }
                    }
                }
            }

            if (m_GroupRoles != null && groupRoleCache != null && groupRoleCache.Count >= roles.GroupData.RoleCount)
            {
                OnGroupRoleDataReply(new GroupRolesDataReplyEventArgs(roles.GroupData.RequestID, roles.GroupData.GroupID, groupRoleCache));
            }

            return packet;
        }

        /// <summary>Process an incoming packet and raise the appropriate events</summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">The EventArgs object containing the packet data</param>
        protected Packet GroupRoleMembersReplyHandler(Packet packet, RegionProxy sim)
        {
            GroupRoleMembersReplyPacket members = (GroupRoleMembersReplyPacket)packet;
            List<KeyValuePair<UUID, UUID>> groupRoleMemberCache = null;

            try
            {
                lock (GroupRolesMembersRequests)
                {
                    // If nothing is registered to receive this RequestID drop the data
                    if (GroupRolesMembersRequests.Contains(members.AgentData.RequestID))
                    {
                        lock (TempGroupRolesMembers.Dictionary)
                        {
                            if (!TempGroupRolesMembers.TryGetValue(members.AgentData.RequestID, out groupRoleMemberCache))
                            {
                                groupRoleMemberCache = new List<KeyValuePair<UUID, UUID>>();
                                TempGroupRolesMembers[members.AgentData.RequestID] = groupRoleMemberCache;
                            }

                            foreach (GroupRoleMembersReplyPacket.MemberDataBlock block in members.MemberData)
                            {
                                KeyValuePair<UUID, UUID> rolemember =
                                    new KeyValuePair<UUID, UUID>(block.RoleID, block.MemberID);

                                groupRoleMemberCache.Add(rolemember);
                            }

                            if (groupRoleMemberCache.Count >= members.AgentData.TotalPairs)
                            {
                                GroupRolesMembersRequests.Remove(members.AgentData.RequestID);
                                TempGroupRolesMembers.Remove(members.AgentData.RequestID);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex.Message, Helpers.LogLevel.Error, ex);
            }

            if (m_GroupRoleMembers != null && groupRoleMemberCache != null && groupRoleMemberCache.Count >= members.AgentData.TotalPairs)
            {
                OnGroupRoleMembers(new GroupRolesMembersReplyEventArgs(members.AgentData.RequestID, members.AgentData.GroupID, groupRoleMemberCache));
            }

            return packet;
        }

        /// <summary>Process an incoming packet and raise the appropriate events</summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">The EventArgs object containing the packet data</param>
        protected Packet GroupActiveProposalItemHandler(Packet packet, RegionProxy sim)
        {
            //GroupActiveProposalItemReplyPacket proposal = (GroupActiveProposalItemReplyPacket)packet;

            // TODO: Create a proposal struct to represent the fields in a proposal item

            return packet;
        }

        /// <summary>Process an incoming packet and raise the appropriate events</summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">The EventArgs object containing the packet data</param>
        protected Packet GroupVoteHistoryItemHandler(Packet packet, RegionProxy sim)
        {
            //GroupVoteHistoryItemReplyPacket history = (GroupVoteHistoryItemReplyPacket)packet;

            // TODO: This was broken in the official viewer when I was last trying to work  on it

            return packet;
        }

        /// <summary>Process an incoming packet and raise the appropriate events</summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">The EventArgs object containing the packet data</param>
        protected Packet GroupAccountSummaryReplyHandler(Packet packet, RegionProxy sim)
        {
            if (m_GroupAccountSummary != null)
            {
                GroupAccountSummaryReplyPacket summary = (GroupAccountSummaryReplyPacket)packet;
                GroupAccountSummary account = new GroupAccountSummary();

                account.Balance = summary.MoneyData.Balance;
                account.CurrentInterval = summary.MoneyData.CurrentInterval;
                account.GroupTaxCurrent = summary.MoneyData.GroupTaxCurrent;
                account.GroupTaxEstimate = summary.MoneyData.GroupTaxEstimate;
                account.IntervalDays = summary.MoneyData.IntervalDays;
                account.LandTaxCurrent = summary.MoneyData.LandTaxCurrent;
                account.LandTaxEstimate = summary.MoneyData.LandTaxEstimate;
                account.LastTaxDate = Utils.BytesToString(summary.MoneyData.LastTaxDate);
                account.LightTaxCurrent = summary.MoneyData.LightTaxCurrent;
                account.LightTaxEstimate = summary.MoneyData.LightTaxEstimate;
                account.NonExemptMembers = summary.MoneyData.NonExemptMembers;
                account.ObjectTaxCurrent = summary.MoneyData.ObjectTaxCurrent;
                account.ObjectTaxEstimate = summary.MoneyData.ObjectTaxEstimate;
                account.ParcelDirFeeCurrent = summary.MoneyData.ParcelDirFeeCurrent;
                account.ParcelDirFeeEstimate = summary.MoneyData.ParcelDirFeeEstimate;
                account.StartDate = Utils.BytesToString(summary.MoneyData.StartDate);
                account.TaxDate = Utils.BytesToString(summary.MoneyData.TaxDate);
                account.TotalCredits = summary.MoneyData.TotalCredits;
                account.TotalDebits = summary.MoneyData.TotalDebits;

                OnGroupAccountSummaryReply(new GroupAccountSummaryReplyEventArgs(summary.AgentData.GroupID, account));
            }

            return packet;
        }

        /// <summary>Process an incoming packet and raise the appropriate events</summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">The EventArgs object containing the packet data</param>
        protected Packet CreateGroupReplyHandler(Packet packet, RegionProxy sim)
        {
            if (m_GroupCreated != null)
            {
                CreateGroupReplyPacket reply = (CreateGroupReplyPacket)packet;

                string message = Utils.BytesToString(reply.ReplyData.Message);

                OnGroupCreatedReply(new GroupCreatedReplyEventArgs(reply.ReplyData.GroupID, reply.ReplyData.Success, message));
            }

            return packet;
        }

        /// <summary>Process an incoming packet and raise the appropriate events</summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">The EventArgs object containing the packet data</param>
        protected Packet JoinGroupReplyHandler(Packet packet, RegionProxy sim)
        {
            if (m_GroupJoined != null)
            {
                JoinGroupReplyPacket reply = (JoinGroupReplyPacket)packet;

                OnGroupJoinedReply(new GroupOperationEventArgs(reply.GroupData.GroupID, reply.GroupData.Success));
            }

            return packet;
        }

        /// <summary>Process an incoming packet and raise the appropriate events</summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">The EventArgs object containing the packet data</param>
        protected Packet LeaveGroupReplyHandler(Packet packet, RegionProxy sim)
        {
            if (m_GroupLeft != null)
            {
                LeaveGroupReplyPacket reply = (LeaveGroupReplyPacket)packet;

                OnGroupLeaveReply(new GroupOperationEventArgs(reply.GroupData.GroupID, reply.GroupData.Success));
            }

            return packet;
        }

        /// <summary>Process an incoming packet and raise the appropriate events</summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">The EventArgs object containing the packet data</param>
        private Packet UUIDGroupNameReplyHandler(Packet packet, RegionProxy sim)
        {
            UUIDGroupNameReplyPacket reply = (UUIDGroupNameReplyPacket)packet;
            UUIDGroupNameReplyPacket.UUIDNameBlockBlock[] blocks = reply.UUIDNameBlock;

            Dictionary<UUID, string> groupNames = new Dictionary<UUID, string>();

            foreach (UUIDGroupNameReplyPacket.UUIDNameBlockBlock block in blocks)
            {
                groupNames.Add(block.ID, Utils.BytesToString(block.GroupName));
                if (!GroupName2KeyCache.ContainsKey(block.ID))
                    GroupName2KeyCache.Add(block.ID, Utils.BytesToString(block.GroupName));
            }

            if (m_GroupNames != null)
            {
                OnGroupNamesReply(new GroupNamesEventArgs(groupNames));
            }

            return packet;
        }

        /// <summary>Process an incoming packet and raise the appropriate events</summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">The EventArgs object containing the packet data</param>
        protected Packet EjectGroupMemberReplyHandler(Packet packet, RegionProxy sim)
        {
            EjectGroupMemberReplyPacket reply = (EjectGroupMemberReplyPacket)packet;

            // TODO: On Success remove the member from the cache(s)

            if (m_GroupMemberEjected != null)
            {
                OnGroupMemberEjected(new GroupOperationEventArgs(reply.GroupData.GroupID, reply.EjectData.Success));
            }

            return packet;
        }

        #endregion Packet Handlers
    }
}
