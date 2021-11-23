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
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Text;
using System.Runtime.Serialization;
using OpenMetaverse.Http;
using OpenMetaverse.Messages.Linden;
using OpenMetaverse.StructuredData;
using OpenMetaverse.Packets;
using OpenMetaverse;
using Nwc.XmlRpc;
using GridProxy;
using static GridProxy.RegionManager;
using System.Threading.Tasks;

namespace GridProxy
{
    /// <summary>
    /// Tools for dealing with agents inventory
    /// </summary>
    [Serializable()]
    public class InventoryManager
    {
        /// <summary>Used for converting shadow_id to asset_id</summary>
        public static readonly UUID MAGIC_ID = new UUID("3c115e51-04f4-523c-9fa6-98aff1034730");

        protected struct InventorySearch
        {
            public UUID Folder;
            public UUID Owner;
            public string[] Path;
            public int Level;
        }

        #region Delegates

        /// <summary>
        /// Callback for inventory item creation finishing
        /// </summary>
        /// <param name="success">Whether the request to create an inventory
        /// item succeeded or not</param>
        /// <param name="item">Inventory item being created. If success is
        /// false this will be null</param>
        public delegate void ItemCreatedCallback(bool success, InventoryItem item);

        /// <summary>
        /// Callback for an inventory item being create from an uploaded asset
        /// </summary>
        /// <param name="success">true if inventory item creation was successful</param>
        /// <param name="status"></param>
        /// <param name="itemID"></param>
        /// <param name="assetID"></param>
        public delegate void ItemCreatedFromAssetCallback(bool success, string status, UUID itemID, UUID assetID);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        public delegate void ItemCopiedCallback(InventoryBase item);

        /// <summary>The event subscribers, null of no subscribers</summary>
        private EventHandler<ItemReceivedEventArgs> m_ItemReceived;

        ///<summary>Raises the ItemReceived Event</summary>
        /// <param name="e">A ItemReceivedEventArgs object containing
        /// the data sent from the simulator</param>
        protected virtual void OnItemReceived(ItemReceivedEventArgs e)
        {
            EventHandler<ItemReceivedEventArgs> handler = m_ItemReceived;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>Thread sync lock object</summary>
        private readonly object m_ItemReceivedLock = new object();

        /// <summary>Raised when the simulator sends us data containing
        /// ...</summary>
        public event EventHandler<ItemReceivedEventArgs> ItemReceived
        {
            add { lock (m_ItemReceivedLock) { m_ItemReceived += value; } }
            remove { lock (m_ItemReceivedLock) { m_ItemReceived -= value; } }
        }


        /// <summary>The event subscribers, null of no subscribers</summary>
        private EventHandler<FolderUpdatedEventArgs> m_FolderUpdated;

        ///<summary>Raises the FolderUpdated Event</summary>
        /// <param name="e">A FolderUpdatedEventArgs object containing
        /// the data sent from the simulator</param>
        protected virtual void OnFolderUpdated(FolderUpdatedEventArgs e)
        {
            EventHandler<FolderUpdatedEventArgs> handler = m_FolderUpdated;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>Thread sync lock object</summary>
        private readonly object m_FolderUpdatedLock = new object();

        /// <summary>Raised when the simulator sends us data containing
        /// ...</summary>
        public event EventHandler<FolderUpdatedEventArgs> FolderUpdated
        {
            add { lock (m_FolderUpdatedLock) { m_FolderUpdated += value; } }
            remove { lock (m_FolderUpdatedLock) { m_FolderUpdated -= value; } }
        }


        /// <summary>The event subscribers, null of no subscribers</summary>
        private EventHandler<InventoryObjectOfferedEventArgs> m_InventoryObjectOffered;

        ///<summary>Raises the InventoryObjectOffered Event</summary>
        /// <param name="e">A InventoryObjectOfferedEventArgs object containing
        /// the data sent from the simulator</param>
        protected virtual void OnInventoryObjectOffered(InventoryObjectOfferedEventArgs e)
        {
            EventHandler<InventoryObjectOfferedEventArgs> handler = m_InventoryObjectOffered;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>Thread sync lock object</summary>
        private readonly object m_InventoryObjectOfferedLock = new object();

        /// <summary>Raised when the simulator sends us data containing
        /// an inventory object sent by another avatar or primitive</summary>
        public event EventHandler<InventoryObjectOfferedEventArgs> InventoryObjectOffered
        {
            add { lock (m_InventoryObjectOfferedLock) { m_InventoryObjectOffered += value; } }
            remove { lock (m_InventoryObjectOfferedLock) { m_InventoryObjectOffered -= value; } }
        }

        /// <summary>The event subscribers, null of no subscribers</summary>
        private EventHandler<TaskItemReceivedEventArgs> m_TaskItemReceived;

        ///<summary>Raises the TaskItemReceived Event</summary>
        /// <param name="e">A TaskItemReceivedEventArgs object containing
        /// the data sent from the simulator</param>
        protected virtual void OnTaskItemReceived(TaskItemReceivedEventArgs e)
        {
            EventHandler<TaskItemReceivedEventArgs> handler = m_TaskItemReceived;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>Thread sync lock object</summary>
        private readonly object m_TaskItemReceivedLock = new object();

        /// <summary>Raised when the simulator sends us data containing
        /// ...</summary>
        public event EventHandler<TaskItemReceivedEventArgs> TaskItemReceived
        {
            add { lock (m_TaskItemReceivedLock) { m_TaskItemReceived += value; } }
            remove { lock (m_TaskItemReceivedLock) { m_TaskItemReceived -= value; } }
        }


        /// <summary>The event subscribers, null of no subscribers</summary>
        private EventHandler<FindObjectByPathReplyEventArgs> m_FindObjectByPathReply;

        ///<summary>Raises the FindObjectByPath Event</summary>
        /// <param name="e">A FindObjectByPathEventArgs object containing
        /// the data sent from the simulator</param>
        protected virtual void OnFindObjectByPathReply(FindObjectByPathReplyEventArgs e)
        {
            EventHandler<FindObjectByPathReplyEventArgs> handler = m_FindObjectByPathReply;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>Thread sync lock object</summary>
        private readonly object m_FindObjectByPathReplyLock = new object();

        /// <summary>Raised when the simulator sends us data containing
        /// ...</summary>
        public event EventHandler<FindObjectByPathReplyEventArgs> FindObjectByPathReply
        {
            add { lock (m_FindObjectByPathReplyLock) { m_FindObjectByPathReply += value; } }
            remove { lock (m_FindObjectByPathReplyLock) { m_FindObjectByPathReply -= value; } }
        }


        /// <summary>The event subscribers, null of no subscribers</summary>
        private EventHandler<TaskInventoryReplyEventArgs> m_TaskInventoryReply;

        ///<summary>Raises the TaskInventoryReply Event</summary>
        /// <param name="e">A TaskInventoryReplyEventArgs object containing
        /// the data sent from the simulator</param>
        protected virtual bool OnTaskInventoryReply(TaskInventoryReplyEventArgs e)
        {
            EventHandler<TaskInventoryReplyEventArgs> handler = m_TaskInventoryReply;
            if (handler != null)
                handler(this, e);

            return e.Handled;
        }

        /// <summary>Thread sync lock object</summary>
        private readonly object m_TaskInventoryReplyLock = new object();

        /// <summary>Raised when the simulator sends us data containing
        /// ...</summary>
        public event EventHandler<TaskInventoryReplyEventArgs> TaskInventoryReply
        {
            add { lock (m_TaskInventoryReplyLock) { m_TaskInventoryReply += value; } }
            remove { lock (m_TaskInventoryReplyLock) { m_TaskInventoryReply -= value; } }
        }

        /// <summary>
        /// Reply received when uploading an inventory asset
        /// </summary>
        /// <param name="success">Has upload been successful</param>
        /// <param name="status">Error message if upload failed</param>
        /// <param name="itemID">Inventory asset UUID</param>
        /// <param name="assetID">New asset UUID</param>
        public delegate void InventoryUploadedAssetCallback(bool success, string status, UUID itemID, UUID assetID);


        /// <summary>The event subscribers, null of no subscribers</summary>
        private EventHandler<SaveAssetToInventoryEventArgs> m_SaveAssetToInventory;

        ///<summary>Raises the SaveAssetToInventory Event</summary>
        /// <param name="e">A SaveAssetToInventoryEventArgs object containing
        /// the data sent from the simulator</param>
        protected virtual void OnSaveAssetToInventory(SaveAssetToInventoryEventArgs e)
        {
            EventHandler<SaveAssetToInventoryEventArgs> handler = m_SaveAssetToInventory;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>Thread sync lock object</summary>
        private readonly object m_SaveAssetToInventoryLock = new object();

        /// <summary>Raised when the simulator sends us data containing
        /// ...</summary>
        public event EventHandler<SaveAssetToInventoryEventArgs> SaveAssetToInventory
        {
            add { lock (m_SaveAssetToInventoryLock) { m_SaveAssetToInventory += value; } }
            remove { lock (m_SaveAssetToInventoryLock) { m_SaveAssetToInventory -= value; } }
        }

        /// <summary>
        /// Delegate that is invoked when script upload is completed
        /// </summary>
        /// <param name="uploadSuccess">Has upload succeded (note, there still might be compile errors)</param>
        /// <param name="uploadStatus">Upload status message</param>
        /// <param name="compileSuccess">Is compilation successful</param>
        /// <param name="compileMessages">If compilation failed, list of error messages, null on compilation success</param>
        /// <param name="itemID">Script inventory UUID</param>
        /// <param name="assetID">Script's new asset UUID</param>
        public delegate void ScriptUpdatedCallback(bool uploadSuccess, string uploadStatus, bool compileSuccess, List<string> compileMessages, UUID itemID, UUID assetID);

        /// <summary>The event subscribers, null of no subscribers</summary>
        private EventHandler<ScriptRunningReplyEventArgs> m_ScriptRunningReply;

        ///<summary>Raises the ScriptRunningReply Event</summary>
        /// <param name="e">A ScriptRunningReplyEventArgs object containing
        /// the data sent from the simulator</param>
        protected virtual void OnScriptRunningReply(ScriptRunningReplyEventArgs e)
        {
            EventHandler<ScriptRunningReplyEventArgs> handler = m_ScriptRunningReply;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>Thread sync lock object</summary>
        private readonly object m_ScriptRunningReplyLock = new object();

        /// <summary>Raised when the simulator sends us data containing
        /// ...</summary>
        public event EventHandler<ScriptRunningReplyEventArgs> ScriptRunningReply
        {
            add { lock (m_ScriptRunningReplyLock) { m_ScriptRunningReply += value; } }
            remove { lock (m_ScriptRunningReplyLock) { m_ScriptRunningReply -= value; } }
        }
        #endregion Delegates

        #region String Arrays

        /// <summary>Partial mapping of FolderTypes to folder names</summary>
        private static readonly string[] _NewFolderNames = new string[]
        {
            "Textures",         //  0
            "Sounds",           //  1
            "Calling Cards",    //  2
            "Landmarks",        //  3
            String.Empty,       //  4
            "Clothing",         //  5
            "Objects",          //  6
            "Notecards",        //  7
            "My Inventory",     //  8
            String.Empty,       //  9
            "Scripts",          // 10
            String.Empty,       // 11
            String.Empty,       // 12
            "Body Parts",       // 13
            "Trash",            // 14
            "Photo Album",      // 15
            "Lost And Found",   // 16
            String.Empty,       // 17
            String.Empty,       // 18
            String.Empty,       // 19
            "Animations",       // 20
            "Gestures",         // 21
            String.Empty,       // 22
            "Favorites",        // 23
            String.Empty,       // 24
            String.Empty,       // 25
            "New Folder",       // 26
            "New Folder",       // 27
            "New Folder",       // 28
            "New Folder",       // 29
            "New Folder",       // 30
            "New Folder",       // 31
            "New Folder",       // 32
            "New Folder",       // 33
            "New Folder",       // 34
            "New Folder",       // 35
            "New Folder",       // 36
            "New Folder",       // 37
            "New Folder",       // 38
            "New Folder",       // 39
            "New Folder",       // 40
            "New Folder",       // 41
            "New Folder",       // 42
            "New Folder",       // 43
            "New Folder",       // 44
            "New Folder",       // 45
            "Current Outfit",   // 46
            "New Outfit",       // 47
            "My Outfits",       // 48
            "Meshes",           // 49
            "Received Items",   // 50
            "Merchant Outbox",  // 51
            "Basic Root",       // 52
            "Marketplace Listings",   // 53
            "New Stock",      // 54
            "New Folder",       // 55
            "Settings",         // 56
        };


        #endregion String Arrays

        private ProxyFrame Frame;
        private Inventory _Store;
        //private Random _RandNumbers = new Random();
        private object _CallbacksLock = new object();
        private uint _CallbackPos;
        private Dictionary<uint, ItemCreatedCallback> _ItemCreatedCallbacks = new Dictionary<uint, ItemCreatedCallback>();
        private Dictionary<uint, ItemCopiedCallback> _ItemCopiedCallbacks = new Dictionary<uint, ItemCopiedCallback>();
        private Dictionary<uint, InventoryType> _ItemInventoryTypeRequest = new Dictionary<uint, InventoryType>();
        private List<InventorySearch> _Searches = new List<InventorySearch>();
        public UUID InventoryRoot { get; private set; }
        public UUID LibraryRoot { get; private set; }

        #region Properties

        /// <summary>
        /// Get this agents Inventory data
        /// </summary>
        public Inventory Store { get { return _Store; } }

        #endregion Properties

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="client">Reference to the GridClient object</param>
        public InventoryManager(ProxyFrame plugin)
        {
            Frame = plugin;
            Frame.Network.AddDelegate(PacketType.UpdateCreateInventoryItem, Direction.Incoming, new PacketDelegate(onUpdateCreateInventoryItem));
            Frame.Network.AddDelegate(PacketType.SaveAssetIntoInventory, Direction.Incoming, new PacketDelegate(onSaveAssetIntoInventory));
            Frame.Network.AddDelegate(PacketType.BulkUpdateInventory, Direction.Incoming, new PacketDelegate(onBulkUpdateInventory));
            Frame.Network.AddDelegate(PacketType.MoveInventoryItem, Direction.Incoming, new PacketDelegate(onMoveInventoryItem));
            Frame.Network.AddDelegate(PacketType.InventoryDescendents, Direction.Incoming, new PacketDelegate(onInventoryDescendents));
            Frame.Network.AddDelegate(PacketType.FetchInventoryReply, Direction.Incoming, new PacketDelegate(onFetchInventoryReply));
            Frame.Network.AddDelegate(PacketType.ReplyTaskInventory, Direction.Incoming, new PacketDelegate(onReplyTaskInventory));
            
            //Client.Network.RegisterEventCallback("BulkUpdateInventory", new Caps.EventQueueCallback(BulkUpdateInventoryCapHandler));
            //Client.Network.RegisterEventCallback("ScriptRunningReply", new Caps.EventQueueCallback(ScriptRunningReplyMessageHandler));

            // Watch for inventory given to us through instant message            
            //Client.Self.IM += Self_IM;

            Frame.Login.AddLoginResponseDelegate(new XmlRpcResponseDelegate(onLoginResponse));
        }

        private void onLoginResponse(XmlRpcResponse response)
        {
            System.Collections.Hashtable values = (System.Collections.Hashtable)response.Value;
            if (!values.Contains("agent_id") || !values.Contains("session_id") || !values.Contains("secure_session_id"))
                return;
            
            LoginResponseData replyData = new LoginResponseData();
            replyData.Parse(values);

            // Initialize the store here so we know who owns it:
            _Store = new Inventory(Frame, this, Frame.Agent.AgentID);
            OpenMetaverse.Logger.DebugLog("Setting InventoryRoot to " + replyData.InventoryRoot.ToString());
            InventoryFolder rootFolder = new InventoryFolder(replyData.InventoryRoot);
            rootFolder.Name = String.Empty;
            rootFolder.ParentUUID = UUID.Zero;
            _Store.RootFolder = rootFolder;
            _Store[replyData.InventoryRoot] = rootFolder;
            InventoryRoot = replyData.InventoryRoot;

            for (int i = 0; i < replyData.InventorySkeleton.Length; i++)
                _Store.UpdateNodeFor(replyData.InventorySkeleton[i]);
            InventoryFolder libraryRootFolder = new InventoryFolder(replyData.LibraryRoot);
            libraryRootFolder.Name = String.Empty;
            libraryRootFolder.ParentUUID = UUID.Zero;
            _Store.LibraryFolder = libraryRootFolder;
            LibraryRoot = replyData.LibraryRoot;

            for (int i = 0; i < replyData.LibrarySkeleton.Length; i++)
                _Store.UpdateNodeFor(replyData.LibrarySkeleton[i]);
        }

        private Packet onUpdateCreateInventoryItem(Packet packet, RegionProxy region)
        {
            UpdateCreateInventoryItemPacket reply = packet as UpdateCreateInventoryItemPacket;

            foreach (UpdateCreateInventoryItemPacket.InventoryDataBlock dataBlock in reply.InventoryData)
            {
                if (dataBlock.InvType == (sbyte)InventoryType.Folder)
                {
                    OpenMetaverse.Logger.Log("Received InventoryFolder in an UpdateCreateInventoryItem packet, this should not happen!",
                        Helpers.LogLevel.Error);
                    continue;
                }

                InventoryItem item = CreateInventoryItem((InventoryType)dataBlock.InvType, dataBlock.ItemID);
                item.AssetType = (AssetType)dataBlock.Type;
                item.AssetUUID = dataBlock.AssetID;
                item.CreationDate = Utils.UnixTimeToDateTime(dataBlock.CreationDate);
                item.CreatorID = dataBlock.CreatorID;
                item.Description = Utils.BytesToString(dataBlock.Description);
                item.Flags = dataBlock.Flags;
                item.GroupID = dataBlock.GroupID;
                item.GroupOwned = dataBlock.GroupOwned;
                item.Name = Utils.BytesToString(dataBlock.Name);
                item.OwnerID = dataBlock.OwnerID;
                item.ParentUUID = dataBlock.FolderID;
                item.Permissions = new Permissions(
                        dataBlock.BaseMask,
                        dataBlock.EveryoneMask,
                        dataBlock.GroupMask,
                        dataBlock.NextOwnerMask,
                        dataBlock.OwnerMask);
                item.SalePrice = dataBlock.SalePrice;
                item.SaleType = (SaleType)dataBlock.SaleType;

                /* 
                 * When attaching new objects, an UpdateCreateInventoryItem packet will be
                 * returned by the server that has a FolderID/ParentUUID of zero. It is up
                 * to the client to make sure that the item gets a good folder, otherwise
                 * it will end up inaccesible in inventory.
                 */
                if (item.ParentUUID == UUID.Zero)
                {
                    // assign default folder for type
                    item.ParentUUID = FindFolderForType(item.AssetType);

                    OpenMetaverse.Logger.Log("Received an item through UpdateCreateInventoryItem with no parent folder, assigning to folder " +
                        item.ParentUUID, Helpers.LogLevel.Info);

                    // send update to the sim
                    RequestUpdateItem(item);
                }

                // Update the local copy
                _Store[item.UUID] = item;

                // Look for an "item created" callback
                ItemCreatedCallback createdCallback;
                if (_ItemCreatedCallbacks.TryGetValue(dataBlock.CallbackID, out createdCallback))
                {
                    _ItemCreatedCallbacks.Remove(dataBlock.CallbackID);

                    try { createdCallback(true, item); }
                    catch (Exception ex) { OpenMetaverse.Logger.Log(ex.Message, Helpers.LogLevel.Error, null, ex); }

                    return null;
                }

                // TODO: Is this callback even triggered when items are copied?
                // Look for an "item copied" callback
                ItemCopiedCallback copyCallback;
                if (_ItemCopiedCallbacks.TryGetValue(dataBlock.CallbackID, out copyCallback))
                {
                    _ItemCopiedCallbacks.Remove(dataBlock.CallbackID);

                    try { copyCallback(item); }
                    catch (Exception ex) { OpenMetaverse.Logger.Log(ex.Message, Helpers.LogLevel.Error, null, ex); }
                }

                //This is triggered when an item is received from a task
                if (m_TaskItemReceived != null)
                {
                    OnTaskItemReceived(new TaskItemReceivedEventArgs(item.UUID, dataBlock.FolderID,
                        item.CreatorID, item.AssetUUID, item.InventoryType));
                }
            }
            return packet;
        }

        private Packet onSaveAssetIntoInventory(Packet packet, RegionProxy region)
        {
            if (m_SaveAssetToInventory != null)
            {
                SaveAssetIntoInventoryPacket save = (SaveAssetIntoInventoryPacket)packet;
                OnSaveAssetToInventory(new SaveAssetToInventoryEventArgs(save.InventoryData.ItemID, save.InventoryData.NewAssetID));
            }
            return packet;
        }

        private Packet onBulkUpdateInventory(Packet packet, RegionProxy region)
        {
            BulkUpdateInventoryPacket update = packet as BulkUpdateInventoryPacket;

            if (update.FolderData.Length > 0 && update.FolderData[0].FolderID != UUID.Zero)
            {
                foreach (BulkUpdateInventoryPacket.FolderDataBlock dataBlock in update.FolderData)
                {
                    InventoryFolder folder;
                    if (!_Store.Contains(dataBlock.FolderID))
                    {
                        folder = new InventoryFolder(dataBlock.FolderID);
                    }
                    else
                    {
                        folder = (InventoryFolder)_Store[dataBlock.FolderID];
                    }

                    if (dataBlock.Name != null)
                    {
                        folder.Name = Utils.BytesToString(dataBlock.Name);
                    }
                    folder.OwnerID = update.AgentData.AgentID;
                    folder.ParentUUID = dataBlock.ParentID;
                    _Store[folder.UUID] = folder;
                }
            }

            if (update.ItemData.Length > 0 && update.ItemData[0].ItemID != UUID.Zero)
            {
                for (int i = 0; i < update.ItemData.Length; i++)
                {
                    BulkUpdateInventoryPacket.ItemDataBlock dataBlock = update.ItemData[i];

                    InventoryItem item = SafeCreateInventoryItem((InventoryType)dataBlock.InvType, dataBlock.ItemID);

                    item.AssetType = (AssetType)dataBlock.Type;
                    if (dataBlock.AssetID != UUID.Zero) item.AssetUUID = dataBlock.AssetID;
                    item.CreationDate = Utils.UnixTimeToDateTime(dataBlock.CreationDate);
                    item.CreatorID = dataBlock.CreatorID;
                    item.Description = Utils.BytesToString(dataBlock.Description);
                    item.Flags = dataBlock.Flags;
                    item.GroupID = dataBlock.GroupID;
                    item.GroupOwned = dataBlock.GroupOwned;
                    item.Name = Utils.BytesToString(dataBlock.Name);
                    item.OwnerID = dataBlock.OwnerID;
                    item.ParentUUID = dataBlock.FolderID;
                    item.Permissions = new Permissions(
                        dataBlock.BaseMask,
                        dataBlock.EveryoneMask,
                        dataBlock.GroupMask,
                        dataBlock.NextOwnerMask,
                        dataBlock.OwnerMask);
                    item.SalePrice = dataBlock.SalePrice;
                    item.SaleType = (SaleType)dataBlock.SaleType;

                    _Store[item.UUID] = item;

                    // Look for an "item created" callback
                    ItemCreatedCallback callback;
                    if (_ItemCreatedCallbacks.TryGetValue(dataBlock.CallbackID, out callback))
                    {
                        _ItemCreatedCallbacks.Remove(dataBlock.CallbackID);

                        try { callback(true, item); }
                        catch (Exception ex) { OpenMetaverse.Logger.Log(ex.Message, Helpers.LogLevel.Error, null, ex); }
                    }

                    // Look for an "item copied" callback
                    ItemCopiedCallback copyCallback;
                    if (_ItemCopiedCallbacks.TryGetValue(dataBlock.CallbackID, out copyCallback))
                    {
                        _ItemCopiedCallbacks.Remove(dataBlock.CallbackID);

                        try { copyCallback(item); }
                        catch (Exception ex) { OpenMetaverse.Logger.Log(ex.Message, Helpers.LogLevel.Error, null, ex); }
                    }
                }
            }
            return packet;
        }

        private Packet onMoveInventoryItem(Packet packet, RegionProxy region)
        {
            MoveInventoryItemPacket move = (MoveInventoryItemPacket)packet;

            for (int i = 0; i < move.InventoryData.Length; i++)
            {
                // FIXME: Do something here
                string newName = Utils.BytesToString(move.InventoryData[i].NewName);

                OpenMetaverse.Logger.Log(String.Format(
                    "MoveInventoryItemHandler: Item {0} is moving to Folder {1} with new name \"{2}\". Someone write this function!",
                    move.InventoryData[i].ItemID.ToString(), move.InventoryData[i].FolderID.ToString(),
                    newName), Helpers.LogLevel.Warning);
            }
            return packet;
        }

        private Packet onInventoryDescendents(Packet packet, RegionProxy region)
        {
            InventoryDescendentsPacket reply = (InventoryDescendentsPacket)packet;

            if (reply.AgentData.Descendents > 0)
            {
                // InventoryDescendantsReply sends a null folder if the parent doesnt contain any folders
                if (reply.FolderData[0].FolderID != UUID.Zero)
                {
                    // Iterate folders in this packet
                    for (int i = 0; i < reply.FolderData.Length; i++)
                    {
                        // If folder already exists then ignore, we assume the version cache
                        // logic is working and if the folder is stale then it should not be present.

                        if (!_Store.Contains(reply.FolderData[i].FolderID))
                        {
                            InventoryFolder folder = new InventoryFolder(reply.FolderData[i].FolderID);
                            folder.ParentUUID = reply.FolderData[i].ParentID;
                            folder.Name = Utils.BytesToString(reply.FolderData[i].Name);
                            folder.PreferredType = (FolderType)reply.FolderData[i].Type;
                            folder.OwnerID = reply.AgentData.OwnerID;

                            _Store[folder.UUID] = folder;
                        }
                    }
                }

                // InventoryDescendantsReply sends a null item if the parent doesnt contain any items.
                if (reply.ItemData[0].ItemID != UUID.Zero)
                {
                    // Iterate items in this packet
                    for (int i = 0; i < reply.ItemData.Length; i++)
                    {
                        if (reply.ItemData[i].ItemID != UUID.Zero)
                        {
                            InventoryItem item;
                            /* 
                             * Objects that have been attached in-world prior to being stored on the 
                             * asset server are stored with the InventoryType of 0 (Texture) 
                             * instead of 17 (Attachment) 
                             * 
                             * This corrects that behavior by forcing Object Asset types that have an 
                             * invalid InventoryType with the proper InventoryType of Attachment.
                             */
                            if ((AssetType)reply.ItemData[i].Type == AssetType.Object
                                && (InventoryType)reply.ItemData[i].InvType == InventoryType.Texture)
                            {
                                item = CreateInventoryItem(InventoryType.Attachment, reply.ItemData[i].ItemID);
                                item.InventoryType = InventoryType.Attachment;
                            }
                            else
                            {
                                item = CreateInventoryItem((InventoryType)reply.ItemData[i].InvType, reply.ItemData[i].ItemID);
                                item.InventoryType = (InventoryType)reply.ItemData[i].InvType;
                            }

                            item.ParentUUID = reply.ItemData[i].FolderID;
                            item.CreatorID = reply.ItemData[i].CreatorID;
                            item.AssetType = (AssetType)reply.ItemData[i].Type;
                            item.AssetUUID = reply.ItemData[i].AssetID;
                            item.CreationDate = Utils.UnixTimeToDateTime((uint)reply.ItemData[i].CreationDate);
                            item.Description = Utils.BytesToString(reply.ItemData[i].Description);
                            item.Flags = reply.ItemData[i].Flags;
                            item.Name = Utils.BytesToString(reply.ItemData[i].Name);
                            item.GroupID = reply.ItemData[i].GroupID;
                            item.GroupOwned = reply.ItemData[i].GroupOwned;
                            item.Permissions = new Permissions(
                                reply.ItemData[i].BaseMask,
                                reply.ItemData[i].EveryoneMask,
                                reply.ItemData[i].GroupMask,
                                reply.ItemData[i].NextOwnerMask,
                                reply.ItemData[i].OwnerMask);
                            item.SalePrice = reply.ItemData[i].SalePrice;
                            item.SaleType = (SaleType)reply.ItemData[i].SaleType;
                            item.OwnerID = reply.AgentData.OwnerID;

                            _Store[item.UUID] = item;
                        }
                    }
                }
            }

            InventoryFolder parentFolder = null;

            if (_Store.Contains(reply.AgentData.FolderID) &&
                _Store[reply.AgentData.FolderID] is InventoryFolder)
            {
                parentFolder = _Store[reply.AgentData.FolderID] as InventoryFolder;
            }
            else
            {
                OpenMetaverse.Logger.Log("Don't have a reference to FolderID " + reply.AgentData.FolderID.ToString() +
                    " or it is not a folder", Helpers.LogLevel.Error);
                return packet;
            }

            if (reply.AgentData.Version < parentFolder.Version)
            {
                OpenMetaverse.Logger.Log("Got an outdated InventoryDescendents packet for folder " + parentFolder.Name +
                    ", this version = " + reply.AgentData.Version + ", latest version = " + parentFolder.Version,
                    Helpers.LogLevel.Warning);
                return packet;
            }

            parentFolder.Version = reply.AgentData.Version;
            // FIXME: reply.AgentData.Descendants is not parentFolder.DescendentCount if we didn't 
            // request items and folders
            parentFolder.DescendentCount = reply.AgentData.Descendents;
            _Store.GetNodeFor(reply.AgentData.FolderID).NeedsUpdate = false;

            #region FindObjectsByPath Handling

            if (_Searches.Count > 0)
            {
                lock (_Searches)
                {
                    StartSearch:

                    // Iterate over all of the outstanding searches
                    for (int i = 0; i < _Searches.Count; i++)
                    {
                        InventorySearch search = _Searches[i];
                        List<InventoryBase> folderContents = _Store.GetContents(search.Folder);

                        // Iterate over all of the inventory objects in the base search folder
                        for (int j = 0; j < folderContents.Count; j++)
                        {
                            // Check if this inventory object matches the current path node
                            if (folderContents[j].Name == search.Path[search.Level])
                            {
                                if (search.Level == search.Path.Length - 1)
                                {
                                    OpenMetaverse.Logger.DebugLog("Finished path search of " + String.Join("/", search.Path), null);

                                    // This is the last node in the path, fire the callback and clean up
                                    if (m_FindObjectByPathReply != null)
                                    {
                                        OnFindObjectByPathReply(new FindObjectByPathReplyEventArgs(String.Join("/", search.Path),
                                            folderContents[j].UUID));
                                    }

                                    // Remove this entry and restart the loop since we are changing the collection size
                                    _Searches.RemoveAt(i);
                                    goto StartSearch;
                                }
                                else
                                {
                                    // We found a match but it is not the end of the path, request the next level
                                    OpenMetaverse.Logger.DebugLog(String.Format("Matched level {0}/{1} in a path search of {2}",
                                        search.Level, search.Path.Length - 1, String.Join("/", search.Path)), null);

                                    search.Folder = folderContents[j].UUID;
                                    search.Level++;
                                    _Searches[i] = search;

                                    RequestFolderContents(search.Folder, search.Owner, true, true,
                                        InventorySortOrder.ByName);
                                }
                            }
                        }
                    }
                }
            }

            #endregion FindObjectsByPath Handling

            // Callback for inventory folder contents being updated
            OnFolderUpdated(new FolderUpdatedEventArgs(parentFolder.UUID, true));
            return packet;
        }

        private Packet onFetchInventoryReply(Packet packet, RegionProxy region)
        {
            FetchInventoryReplyPacket reply = packet as FetchInventoryReplyPacket;

            foreach (FetchInventoryReplyPacket.InventoryDataBlock dataBlock in reply.InventoryData)
            {
                if (dataBlock.InvType == (sbyte)InventoryType.Folder)
                {
                    OpenMetaverse.Logger.Log("Received FetchInventoryReply for an inventory folder, this should not happen!",
                        Helpers.LogLevel.Error);
                    continue;
                }

                InventoryItem item = CreateInventoryItem((InventoryType)dataBlock.InvType, dataBlock.ItemID);
                item.AssetType = (AssetType)dataBlock.Type;
                item.AssetUUID = dataBlock.AssetID;
                item.CreationDate = Utils.UnixTimeToDateTime(dataBlock.CreationDate);
                item.CreatorID = dataBlock.CreatorID;
                item.Description = Utils.BytesToString(dataBlock.Description);
                item.Flags = dataBlock.Flags;
                item.GroupID = dataBlock.GroupID;
                item.GroupOwned = dataBlock.GroupOwned;
                item.InventoryType = (InventoryType)dataBlock.InvType;
                item.Name = Utils.BytesToString(dataBlock.Name);
                item.OwnerID = dataBlock.OwnerID;
                item.ParentUUID = dataBlock.FolderID;
                item.Permissions = new Permissions(
                    dataBlock.BaseMask,
                    dataBlock.EveryoneMask,
                    dataBlock.GroupMask,
                    dataBlock.NextOwnerMask,
                    dataBlock.OwnerMask);
                item.SalePrice = dataBlock.SalePrice;
                item.SaleType = (SaleType)dataBlock.SaleType;
                item.UUID = dataBlock.ItemID;

                _Store[item.UUID] = item;

                // Fire the callback for an item being fetched
                OnItemReceived(new ItemReceivedEventArgs(item));
            }

            return packet;
        }

        private Packet onReplyTaskInventory(Packet packet, RegionProxy region)
        {
            if (m_TaskInventoryReply != null)
            {
                ReplyTaskInventoryPacket reply = (ReplyTaskInventoryPacket)packet;

                return OnTaskInventoryReply(new TaskInventoryReplyEventArgs(reply.InventoryData.TaskID, reply.InventoryData.Serial,
                    Utils.BytesToString(reply.InventoryData.Filename))) ? null : packet;
            }
            return packet;
        }


        #region Fetch

        /// <summary>
        /// Fetch an inventory item from the dataserver
        /// </summary>
        /// <param name="itemID">The items <seealso cref="UUID"/></param>
        /// <param name="ownerID">The item Owners <seealso cref="OpenMetaverse.UUID"/></param>
        /// <param name="timeoutMS">a integer representing the number of milliseconds to wait for results</param>
        /// <returns>An <seealso cref="InventoryItem"/> object on success, or null if no item was found</returns>
        /// <remarks>Items will also be sent to the <seealso cref="InventoryManager.OnItemReceived"/> event</remarks>
        public InventoryItem FetchItem(UUID itemID, UUID ownerID, int timeoutMS)
        {
            AutoResetEvent fetchEvent = new AutoResetEvent(false);
            InventoryItem fetchedItem = null;

            EventHandler<ItemReceivedEventArgs> callback =
                delegate(object sender, ItemReceivedEventArgs e)
                {
                    if (e.Item.UUID == itemID)
                    {
                        fetchedItem = e.Item;
                        fetchEvent.Set();
                    }
                };

            ItemReceived += callback;
            RequestFetchInventory(itemID, ownerID);

            fetchEvent.WaitOne(timeoutMS, false);
            ItemReceived -= callback;

            return fetchedItem;
        }

        /// <summary>
        /// Request A single inventory item
        /// </summary>
        /// <param name="itemID">The items <seealso cref="OpenMetaverse.UUID"/></param>
        /// <param name="ownerID">The item Owners <seealso cref="OpenMetaverse.UUID"/></param>
        /// <seealso cref="InventoryManager.OnItemReceived"/>
        public void RequestFetchInventory(UUID itemID, UUID ownerID, bool use_caps = true)
        {
            RequestFetchInventory(new List<UUID>(1) { itemID }, new List<UUID>(1) { ownerID }, use_caps);
        }

        /// <summary>
        /// Request inventory items
        /// </summary>
        /// <param name="itemIDs">Inventory items to request</param>
        /// <param name="ownerIDs">Owners of the inventory items</param>
        /// <seealso cref="InventoryManager.OnItemReceived"/>
        public void RequestFetchInventory(List<UUID> itemIDs, List<UUID> ownerIDs, bool use_caps = true)
        {
            if (itemIDs.Count != ownerIDs.Count)
                throw new ArgumentException("itemIDs and ownerIDs must contain the same number of entries");

            if (Frame.Config.HTTP_INVENTORY &&
                Frame.Network.CurrentSim != null
                && use_caps)// &&
                //Client.Network.CurrentSim.Caps.CapabilityURI("FetchInventory2") != null)
            {
                RequestFetchInventoryCap(itemIDs, ownerIDs);
                return;
            }


            FetchInventoryPacket fetch = new FetchInventoryPacket();
            fetch.AgentData = new FetchInventoryPacket.AgentDataBlock();
            fetch.AgentData.AgentID = Frame.Agent.AgentID;
            fetch.AgentData.SessionID = Frame.Agent.SessionID;

            fetch.InventoryData = new FetchInventoryPacket.InventoryDataBlock[itemIDs.Count];
            for (int i = 0; i < itemIDs.Count; i++)
            {
                fetch.InventoryData[i] = new FetchInventoryPacket.InventoryDataBlock();
                fetch.InventoryData[i].ItemID = itemIDs[i];
                fetch.InventoryData[i].OwnerID = ownerIDs[i];
            }

            Frame.Network.InjectPacket(fetch, Direction.Outgoing);
        }

        /// <summary>
        /// Request inventory items via Capabilities
        /// </summary>
        /// <param name="itemIDs">Inventory items to request</param>
        /// <param name="ownerIDs">Owners of the inventory items</param>
        /// <seealso cref="InventoryManager.OnItemReceived"/>
        private void RequestFetchInventoryCap(List<UUID> itemIDs, List<UUID> ownerIDs)
        {
            if (itemIDs.Count != ownerIDs.Count)
                throw new ArgumentException("itemIDs and ownerIDs must contain the same number of entries");

            if (Frame.Config.HTTP_INVENTORY &&
                Frame.Network.CurrentSim != null)// &&
                //Client.Network.CurrentSim.Caps.CapabilityURI("FetchInventory2") != null)
            {
                string cap_url;
                if(!Frame.Network.CurrentSim.Caps.TryGetValue("FetchInventory2", out cap_url))
                    return;

                Uri url = new Uri(cap_url);
                CapsClient request = new CapsClient(url);

                request.OnComplete += (client, result, error) =>
                {
                    if (error == null)
                    {
                        try
                        {
                            OSDMap res = (OSDMap)result;
                            OSDArray itemsOSD = (OSDArray)res["items"];

                            for (int i = 0; i < itemsOSD.Count; i++)
                            {
                                InventoryItem item = InventoryItem.FromOSD(itemsOSD[i]);
                                _Store[item.UUID] = item;
                                OnItemReceived(new ItemReceivedEventArgs(item));
                            }
                        }
                        catch (Exception ex)
                        {
                            OpenMetaverse.Logger.Log("Failed getting data from FetchInventory2 capability.", Helpers.LogLevel.Error, null, ex);
                        }
                    }
                };

                OSDMap OSDRequest = new OSDMap();
                OSDRequest["agent_id"] = Frame.Agent.AgentID;

                OSDArray items = new OSDArray(itemIDs.Count);
                for (int i = 0; i < itemIDs.Count; i++)
                {
                    OSDMap item = new OSDMap(2);
                    item["item_id"] = itemIDs[i];
                    item["owner_id"] = ownerIDs[i];
                    items.Add(item);
                }

                OSDRequest["items"] = items;

                request.BeginGetResponse(OSDRequest, OSDFormat.Xml, Frame.Config.CAPS_TIMEOUT);
            }
        }

        /// <summary>
        /// Get contents of a folder
        /// </summary>
        /// <param name="folder">The <seealso cref="UUID"/> of the folder to search</param>
        /// <param name="owner">The <seealso cref="UUID"/> of the folders owner</param>
        /// <param name="folders">true to retrieve folders</param>
        /// <param name="items">true to retrieve items</param>
        /// <param name="order">sort order to return results in</param>
        /// <param name="timeoutMS">a integer representing the number of milliseconds to wait for results</param>
        /// <returns>A list of inventory items matching search criteria within folder</returns>
        /// <seealso cref="InventoryManager.RequestFolderContents"/>
        /// <remarks>InventoryFolder.DescendentCount will only be accurate if both folders and items are
        /// requested</remarks>
        public List<InventoryBase> FolderContents(UUID folder, UUID owner, bool folders, bool items,
            InventorySortOrder order, int timeoutMS)
        {
            List<InventoryBase> objects = null;
            AutoResetEvent fetchEvent = new AutoResetEvent(false);

            EventHandler<FolderUpdatedEventArgs> callback =
                delegate(object sender, FolderUpdatedEventArgs e)
                {
                    if (e.FolderID == folder
                        && _Store[folder] is InventoryFolder)
                    {
                        // InventoryDescendentsHandler only stores DescendendCount if both folders and items are fetched.
                        if (_Store.GetContents(folder).Count >= ((InventoryFolder)_Store[folder]).DescendentCount)
                        {

                            fetchEvent.Set();
                        }
                    }
                    else
                    {
                        fetchEvent.Set();
                    }
                };

            FolderUpdated += callback;

            RequestFolderContents(folder, owner, folders, items, order);
            if (fetchEvent.WaitOne(timeoutMS, false))
                objects = _Store.GetContents(folder);

            FolderUpdated -= callback;

            return objects;
        }

        /// <summary>
        /// Request the contents of an inventory folder
        /// </summary>
        /// <param name="folder">The folder to search</param>
        /// <param name="owner">The folder owners <seealso cref="UUID"/></param>
        /// <param name="folders">true to return <seealso cref="InventoryManager.InventoryFolder"/>s contained in folder</param>
        /// <param name="items">true to return <seealso cref="InventoryManager.InventoryItem"/>s containd in folder</param>
        /// <param name="order">the sort order to return items in</param>
        /// <seealso cref="InventoryManager.FolderContents"/>
        public void RequestFolderContents(UUID folder, UUID owner, bool folders, bool items,
            InventorySortOrder order, bool use_caps = true)
        {
            //string cap = owner == Plugin.Frame.AgentID ? "FetchInventoryDescendents2" : "FetchLibDescendents2";

            if (Frame.Config.HTTP_INVENTORY &&
                Frame.Network.CurrentSim != null &&
                use_caps)// &&
                //Client.Network.CurrentSim.Caps.CapabilityURI(cap) != null)
            {
                RequestFolderContentsCap(folder, owner, folders, items, order);
                return;
            }

            FetchInventoryDescendentsPacket fetch = new FetchInventoryDescendentsPacket();
            fetch.AgentData.AgentID = Frame.Agent.AgentID;
            fetch.AgentData.SessionID = Frame.Agent.SessionID;

            fetch.InventoryData.FetchFolders = folders;
            fetch.InventoryData.FetchItems = items;
            fetch.InventoryData.FolderID = folder;
            fetch.InventoryData.OwnerID = owner;
            fetch.InventoryData.SortOrder = (int)order;

            Frame.Network.InjectPacket(fetch, Direction.Outgoing);
        }

        /// <summary>
        /// Request the contents of an inventory folder using HTTP capabilities
        /// </summary>
        /// <param name="folderID">The folder to search</param>
        /// <param name="ownerID">The folder owners <seealso cref="UUID"/></param>
        /// <param name="fetchFolders">true to return <seealso cref="InventoryManager.InventoryFolder"/>s contained in folder</param>
        /// <param name="fetchItems">true to return <seealso cref="InventoryManager.InventoryItem"/>s containd in folder</param>
        /// <param name="order">the sort order to return items in</param>
        /// <seealso cref="InventoryManager.FolderContents"/>
        public void RequestFolderContentsCap(UUID folderID, UUID ownerID, bool fetchFolders, bool fetchItems,
            InventorySortOrder order)
        {
            string cap_name = ownerID == Frame.Agent.AgentID ? "FetchInventoryDescendents2" : "FetchLibDescendents2";

            string cap_url;

            if(!Frame.Network.CurrentSim.Caps.TryGetValue(cap_name, out cap_url))
            {
                OpenMetaverse.Logger.Log(cap_name + " capability not available in the current sim", Helpers.LogLevel.Warning);
                OnFolderUpdated(new FolderUpdatedEventArgs(folderID, false));
                return;
            }

            Uri url = new Uri(cap_url);

            InventoryFolder folder = new InventoryFolder(folderID);
            folder.OwnerID = ownerID;
            folder.UUID = folderID;
            RequestFolderContentsCap(new List<InventoryFolder>() { folder }, url, fetchFolders, fetchItems, order);
        }

        public void RequestFolderContentsCap(List<InventoryFolder> batch, Uri url, bool fetchFolders, bool fetchItems, InventorySortOrder order)
        {
            try
            {
                CapsClient request = new CapsClient(url);
                request.OnComplete += (client, result, error) =>
                {
                    try
                    {
                        if (error != null)
                        {
                            throw error;
                        }

                        OSDMap resultMap = ((OSDMap)result);
                        if (resultMap.ContainsKey("folders"))
                        {
                            OSDArray fetchedFolders = (OSDArray)resultMap["folders"];
                            for (int fetchedFolderNr = 0; fetchedFolderNr < fetchedFolders.Count; fetchedFolderNr++)
                            {
                                OSDMap res = (OSDMap)fetchedFolders[fetchedFolderNr];
                                InventoryFolder fetchedFolder = null;

                                if (_Store.Contains(res["folder_id"])
                                    && _Store[res["folder_id"]] is InventoryFolder)
                                {
                                    fetchedFolder = (InventoryFolder)_Store[res["folder_id"]];
                                }
                                else
                                {
                                    fetchedFolder = new InventoryFolder(res["folder_id"]);
                                    _Store[res["folder_id"]] = fetchedFolder;
                                }
                                fetchedFolder.DescendentCount = res["descendents"];
                                fetchedFolder.Version = res["version"];
                                fetchedFolder.OwnerID = res["owner_id"];
                                _Store.GetNodeFor(fetchedFolder.UUID).NeedsUpdate = false;

                                // Do we have any descendants
                                if (fetchedFolder.DescendentCount > 0)
                                {
                                    // Fetch descendent folders
                                    if (res["categories"] is OSDArray)
                                    {
                                        OSDArray folders = (OSDArray)res["categories"];
                                        for (int i = 0; i < folders.Count; i++)
                                        {
                                            OSDMap descFolder = (OSDMap)folders[i];
                                            InventoryFolder folder;
                                            UUID folderID = descFolder.ContainsKey("category_id") ? descFolder["category_id"] : descFolder["folder_id"];
                                            if (!_Store.Contains(folderID))
                                            {
                                                folder = new InventoryFolder(folderID);
                                                folder.ParentUUID = descFolder["parent_id"];
                                                _Store[folderID] = folder;
                                            }
                                            else
                                            {
                                                folder = (InventoryFolder)_Store[folderID];
                                            }

                                            folder.OwnerID = descFolder["agent_id"];
                                            folder.ParentUUID = descFolder["parent_id"];
                                            folder.Name = descFolder["name"];
                                            folder.Version = descFolder["version"];

                                            if (descFolder.ContainsKey("type_default"))
                                                folder.PreferredType = (FolderType)(int)descFolder["type_default"];
                                            else if (descFolder.ContainsKey("type")) // opensim is retarded
                                                folder.PreferredType = (FolderType)(int)descFolder["type"];
                                            else if (descFolder.ContainsKey("preferred_type")) // opensim is retarded
                                                folder.PreferredType = (FolderType)(int)descFolder["preferred_type"];
                                            else
                                                folder.PreferredType = FolderType.None;
                                        }

                                        // Fetch descendent items
                                        OSDArray items = (OSDArray)res["items"];
                                        for (int i = 0; i < items.Count; i++)
                                        {
                                            InventoryItem item = InventoryItem.FromOSD(items[i]);
                                            _Store[item.UUID] = item;
                                        }
                                    }
                                }

                                OnFolderUpdated(new FolderUpdatedEventArgs(res["folder_id"], true));
                            }
                        }
                    }
                    catch (Exception exc)
                    {
                        OpenMetaverse.Logger.Log(string.Format("Failed to fetch inventory descendants: {0}\n{1}", exc.Message, exc.StackTrace.ToString()), Helpers.LogLevel.Warning);
                        foreach (var f in batch)
                        {
                            OnFolderUpdated(new FolderUpdatedEventArgs(f.UUID, false));
                        }
                        return;
                    }

                };

                // Construct request
                OSDArray requestedFolders = new OSDArray(1);
                foreach (var f in batch)
                {
                    OSDMap requestedFolder = new OSDMap(1);
                    requestedFolder["folder_id"] = f.UUID;
                    requestedFolder["owner_id"] = f.OwnerID;
                    requestedFolder["fetch_folders"] = fetchFolders;
                    requestedFolder["fetch_items"] = fetchItems;
                    requestedFolder["sort_order"] = (int)order;

                    requestedFolders.Add(requestedFolder);
                }
                OSDMap req = new OSDMap(1);
                req["folders"] = requestedFolders;

                request.BeginGetResponse(req, OSDFormat.Xml, Frame.Config.CAPS_TIMEOUT);
            }
            catch (Exception ex)
            {
                OpenMetaverse.Logger.Log(string.Format("Failed to fetch inventory descendants: {0}\n{1}", ex.Message, ex.StackTrace.ToString()), Helpers.LogLevel.Warning);
                foreach (var f in batch)
                {
                    OnFolderUpdated(new FolderUpdatedEventArgs(f.UUID, false));
                }
                return;
            }
        }

        #endregion Fetch

        #region Find

        /// <summary>
        /// Returns the UUID of the folder (category) that defaults to
        /// containing 'type'. The folder is not necessarily only for that
        /// type
        /// </summary>
        /// <remarks>This will return the root folder if one does not exist</remarks>
        /// <param name="type"></param>
        /// <returns>The UUID of the desired folder if found, the UUID of the RootFolder
        /// if not found, or UUID.Zero on failure</returns>
        public UUID FindFolderForType(AssetType type)
        {
            if (_Store == null)
            {
                OpenMetaverse.Logger.Log("Inventory is null, FindFolderForType() lookup cannot continue",
                    Helpers.LogLevel.Error);
                return UUID.Zero;
            }

            // Folders go in the root
            if (type == AssetType.Folder)
                return _Store.RootFolder.UUID;

            // Loop through each top-level directory and check if PreferredType
            // matches the requested type
            List<InventoryBase> contents = _Store.GetContents(_Store.RootFolder.UUID);
            foreach (InventoryBase inv in contents)
            {
                if (inv is InventoryFolder)
                {
                    InventoryFolder folder = inv as InventoryFolder;

                    if (folder.PreferredType == (FolderType)type)
                        return folder.UUID;
                }
            }

            // No match found, return Root Folder ID
            return _Store.RootFolder.UUID;
        }

        public UUID FindFolderForType(FolderType type)
        {
            if (_Store == null)
            {
                OpenMetaverse.Logger.Log("Inventory is null, FindFolderForType() lookup cannot continue",
                    Helpers.LogLevel.Error);
                return UUID.Zero;
            }

            List<InventoryBase> contents = _Store.GetContents(_Store.RootFolder.UUID);
            foreach (InventoryBase inv in contents)
            {
                if (inv is InventoryFolder)
                {
                    InventoryFolder folder = inv as InventoryFolder;

                    if (folder.PreferredType == type)
                        return folder.UUID;
                }
            }

            // No match found, return Root Folder ID
            return _Store.RootFolder.UUID;
        }

        public UUID FindSuitcaseFolder()
        {
            if (_Store == null)
            {
                OpenMetaverse.Logger.Log("Inventory is null, FindFolderForType() lookup cannot continue",
                    Helpers.LogLevel.Error);
                return UUID.Zero;
            }

            List<InventoryBase> contents = _Store.GetContents(_Store.RootFolder.UUID);
            foreach (InventoryBase inv in contents)
            {
                if (inv is InventoryFolder)
                {
                    InventoryFolder folder = inv as InventoryFolder;

                    if (folder.Name == "My Suitcase")
                        return folder.UUID;
                }
            }

            // No match found, return Root Folder ID
            return UUID.Zero;
        }

        private UUID suitcaseUUID = UUID.Zero;

        public UUID SuitcaseID
        {
            get
            {
                if (suitcaseUUID == UUID.Zero)
                    suitcaseUUID = FindSuitcaseFolder();
                return suitcaseUUID;
            }
        }

        public UUID FindSuitcaseFolderForType(FolderType type)
        {
            if (_Store == null)
            {
                OpenMetaverse.Logger.Log("Inventory is null, FindFolderForType() lookup cannot continue",
                    Helpers.LogLevel.Error);
                return UUID.Zero;
            }

            List<InventoryBase> contents = _Store.GetContents(SuitcaseID);
            foreach (InventoryBase inv in contents)
            {
                if (inv is InventoryFolder)
                {
                    InventoryFolder folder = inv as InventoryFolder;

                    if (folder.PreferredType == type)
                        return folder.UUID;
                }
            }

            // No match found, return Suitecase ID
            return SuitcaseID;
        }



        /// <summary>
        /// Find an object in inventory using a specific path to search
        /// </summary>
        /// <param name="baseFolder">The folder to begin the search in</param>
        /// <param name="inventoryOwner">The object owners <seealso cref="UUID"/></param>
        /// <param name="path">A string path to search</param>
        /// <param name="timeoutMS">milliseconds to wait for a reply</param>
        /// <returns>Found items <seealso cref="UUID"/> or <seealso cref="UUID.Zero"/> if 
        /// timeout occurs or item is not found</returns>
        public UUID FindObjectByPath(UUID baseFolder, UUID inventoryOwner, string path, int timeoutMS)
        {
            AutoResetEvent findEvent = new AutoResetEvent(false);
            UUID foundItem = UUID.Zero;

            EventHandler<FindObjectByPathReplyEventArgs> callback =
                delegate(object sender, FindObjectByPathReplyEventArgs e)
                {
                    if (e.Path == path)
                    {
                        foundItem = e.InventoryObjectID;
                        findEvent.Set();
                    }
                };

            FindObjectByPathReply += callback;

            RequestFindObjectByPath(baseFolder, inventoryOwner, path);
            findEvent.WaitOne(timeoutMS, false);

            FindObjectByPathReply -= callback;

            return foundItem;
        }

        /// <summary>
        /// Find inventory items by path
        /// </summary>
        /// <param name="baseFolder">The folder to begin the search in</param>
        /// <param name="inventoryOwner">The object owners <seealso cref="UUID"/></param>
        /// <param name="path">A string path to search, folders/objects separated by a '/'</param>
        /// <remarks>Results are sent to the <seealso cref="InventoryManager.OnFindObjectByPath"/> event</remarks>
        public void RequestFindObjectByPath(UUID baseFolder, UUID inventoryOwner, string path)
        {
            if (path == null || path.Length == 0)
                throw new ArgumentException("Empty path is not supported");

            // Store this search
            InventorySearch search;
            search.Folder = baseFolder;
            search.Owner = inventoryOwner;
            search.Path = path.Split('/');
            search.Level = 0;
            lock (_Searches) _Searches.Add(search);

            // Start the search
            RequestFolderContents(baseFolder, inventoryOwner, true, true, InventorySortOrder.ByName);
        }

        /// <summary>
        /// Search inventory Store object for an item or folder
        /// </summary>
        /// <param name="baseFolder">The folder to begin the search in</param>
        /// <param name="path">An array which creates a path to search</param>
        /// <param name="level">Number of levels below baseFolder to conduct searches</param>
        /// <param name="firstOnly">if True, will stop searching after first match is found</param>
        /// <returns>A list of inventory items found</returns>
        public List<InventoryBase> LocalFind(UUID baseFolder, string[] path, int level, bool firstOnly)
        {
            List<InventoryBase> objects = new List<InventoryBase>();
            //List<InventoryFolder> folders = new List<InventoryFolder>();
            List<InventoryBase> contents = _Store.GetContents(baseFolder);

            foreach (InventoryBase inv in contents)
            {
                if (inv.Name.CompareTo(path[level]) == 0)
                {
                    if (level == path.Length - 1)
                    {
                        objects.Add(inv);
                        if (firstOnly) return objects;
                    }
                    else if (inv is InventoryFolder)
                        objects.AddRange(LocalFind(inv.UUID, path, level + 1, firstOnly));
                }
            }

            return objects;
        }

        #endregion Find

        #region Move/Rename

        /// <summary>
        /// Move an inventory item or folder to a new location
        /// </summary>
        /// <param name="item">The <seealso cref="T:InventoryBase"/> item or folder to move</param>
        /// <param name="newParent">The <seealso cref="T:InventoryFolder"/> to move item or folder to</param>
        public void Move(InventoryBase item, InventoryFolder newParent)
        {
            if (item is InventoryFolder)
                MoveFolder(item.UUID, newParent.UUID);
            else
                MoveItem(item.UUID, newParent.UUID);
        }

        /// <summary>
        /// Move an inventory item or folder to a new location and change its name
        /// </summary>
        /// <param name="item">The <seealso cref="T:InventoryBase"/> item or folder to move</param>
        /// <param name="newParent">The <seealso cref="T:InventoryFolder"/> to move item or folder to</param>
        /// <param name="newName">The name to change the item or folder to</param>
        public void Move(InventoryBase item, InventoryFolder newParent, string newName)
        {
            if (item is InventoryFolder)
                MoveFolder(item.UUID, newParent.UUID, newName);
            else
                MoveItem(item.UUID, newParent.UUID, newName);
        }

        /// <summary>
        /// Move and rename a folder
        /// </summary>
        /// <param name="folderID">The source folders <seealso cref="UUID"/></param>
        /// <param name="newparentID">The destination folders <seealso cref="UUID"/></param>
        /// <param name="newName">The name to change the folder to</param>
        public void MoveFolder(UUID folderID, UUID newparentID, string newName)
        {
            UpdateFolderProperties(folderID, newparentID, newName, FolderType.None);
        }

        /// <summary>
        /// Update folder properties
        /// </summary>
        /// <param name="folderID"><seealso cref="UUID"/> of the folder to update</param>
        /// <param name="parentID">Sets folder's parent to <seealso cref="UUID"/></param>
        /// <param name="name">Folder name</param>
        /// <param name="type">Folder type</param>
        public void UpdateFolderProperties(UUID folderID, UUID parentID, string name, FolderType type)
        {
            lock (Store)
            {
                if (_Store.Contains(folderID))
                {
                    InventoryFolder inv = (InventoryFolder)Store[folderID];
                    inv.Name = name;
                    inv.ParentUUID = parentID;
                    inv.PreferredType = type;
                    _Store.UpdateNodeFor(inv);
                }
            }

            UpdateInventoryFolderPacket invFolder = new UpdateInventoryFolderPacket();
            invFolder.AgentData.AgentID = Frame.Agent.AgentID;
            invFolder.AgentData.SessionID = Frame.Agent.SessionID;
            invFolder.FolderData = new UpdateInventoryFolderPacket.FolderDataBlock[1];
            invFolder.FolderData[0] = new UpdateInventoryFolderPacket.FolderDataBlock();
            invFolder.FolderData[0].FolderID = folderID;
            invFolder.FolderData[0].ParentID = parentID;
            invFolder.FolderData[0].Name = Utils.StringToBytes(name);
            invFolder.FolderData[0].Type = (sbyte)type;

            Frame.Network.InjectPacket(invFolder, Direction.Outgoing);
        }

        /// <summary>
        /// Move a folder
        /// </summary>
        /// <param name="folderID">The source folders <seealso cref="UUID"/></param>
        /// <param name="newParentID">The destination folders <seealso cref="UUID"/></param>
        public void MoveFolder(UUID folderID, UUID newParentID)
        {
            lock (Store)
            {
                if (_Store.Contains(folderID))
                {
                    InventoryBase inv = Store[folderID];
                    inv.ParentUUID = newParentID;
                    _Store.UpdateNodeFor(inv);
                }
            }

            MoveInventoryFolderPacket move = new MoveInventoryFolderPacket();
            move.AgentData.AgentID = Frame.Agent.AgentID;
            move.AgentData.SessionID = Frame.Agent.SessionID;
            move.AgentData.Stamp = false; //FIXME: ??

            move.InventoryData = new MoveInventoryFolderPacket.InventoryDataBlock[1];
            move.InventoryData[0] = new MoveInventoryFolderPacket.InventoryDataBlock();
            move.InventoryData[0].FolderID = folderID;
            move.InventoryData[0].ParentID = newParentID;

            Frame.Network.InjectPacket(move, Direction.Outgoing);
        }

        /// <summary>
        /// Move multiple folders, the keys in the Dictionary parameter,
        /// to a new parents, the value of that folder's key.
        /// </summary>
        /// <param name="foldersNewParents">A Dictionary containing the 
        /// <seealso cref="UUID"/> of the source as the key, and the 
        /// <seealso cref="UUID"/> of the destination as the value</param>
        public void MoveFolders(Dictionary<UUID, UUID> foldersNewParents)
        {
            // FIXME: Use two List<UUID> to stay consistent

            lock (Store)
            {
                foreach (KeyValuePair<UUID, UUID> entry in foldersNewParents)
                {
                    if (_Store.Contains(entry.Key))
                    {
                        InventoryBase inv = _Store[entry.Key];
                        inv.ParentUUID = entry.Value;
                        _Store.UpdateNodeFor(inv);
                    }
                }
            }

            //TODO: Test if this truly supports multiple-folder move
            MoveInventoryFolderPacket move = new MoveInventoryFolderPacket();
            move.AgentData.AgentID = Frame.Agent.AgentID;
            move.AgentData.SessionID = Frame.Agent.SessionID;
            move.AgentData.Stamp = false; //FIXME: ??

            move.InventoryData = new MoveInventoryFolderPacket.InventoryDataBlock[foldersNewParents.Count];

            int index = 0;
            foreach (KeyValuePair<UUID, UUID> folder in foldersNewParents)
            {
                MoveInventoryFolderPacket.InventoryDataBlock block = new MoveInventoryFolderPacket.InventoryDataBlock();
                block.FolderID = folder.Key;
                block.ParentID = folder.Value;
                move.InventoryData[index++] = block;
            }

            Frame.Network.InjectPacket(move, Direction.Outgoing);
        }


        /// <summary>
        /// Move an inventory item to a new folder
        /// </summary>
        /// <param name="itemID">The <seealso cref="UUID"/> of the source item to move</param>
        /// <param name="folderID">The <seealso cref="UUID"/> of the destination folder</param>
        public void MoveItem(UUID itemID, UUID folderID)
        {
            MoveItem(itemID, folderID, String.Empty);
        }

        /// <summary>
        /// Move and rename an inventory item
        /// </summary>
        /// <param name="itemID">The <seealso cref="UUID"/> of the source item to move</param>
        /// <param name="folderID">The <seealso cref="UUID"/> of the destination folder</param>
        /// <param name="newName">The name to change the folder to</param>
        public void MoveItem(UUID itemID, UUID folderID, string newName)
        {
            lock (_Store)
            {
                if (_Store.Contains(itemID))
                {
                    InventoryBase inv = _Store[itemID];
                    if (!string.IsNullOrEmpty(newName))
                    {
                        inv.Name = newName;
                    }
                    inv.ParentUUID = folderID;
                    _Store.UpdateNodeFor(inv);
                }
            }

            MoveInventoryItemPacket move = new MoveInventoryItemPacket();
            move.AgentData.AgentID = Frame.Agent.AgentID;
            move.AgentData.SessionID = Frame.Agent.SessionID;
            move.AgentData.Stamp = false; //FIXME: ??

            move.InventoryData = new MoveInventoryItemPacket.InventoryDataBlock[1];
            move.InventoryData[0] = new MoveInventoryItemPacket.InventoryDataBlock();
            move.InventoryData[0].ItemID = itemID;
            move.InventoryData[0].FolderID = folderID;
            move.InventoryData[0].NewName = Utils.StringToBytes(newName);

            Frame.Network.InjectPacket(move, Direction.Outgoing);
        }

        /// <summary>
        /// Move multiple inventory items to new locations
        /// </summary>
        /// <param name="itemsNewParents">A Dictionary containing the 
        /// <seealso cref="UUID"/> of the source item as the key, and the 
        /// <seealso cref="UUID"/> of the destination folder as the value</param>
        public void MoveItems(Dictionary<UUID, UUID> itemsNewParents)
        {
            lock (_Store)
            {
                foreach (KeyValuePair<UUID, UUID> entry in itemsNewParents)
                {
                    if (_Store.Contains(entry.Key))
                    {
                        InventoryBase inv = _Store[entry.Key];
                        inv.ParentUUID = entry.Value;
                        _Store.UpdateNodeFor(inv);
                    }
                }
            }

            MoveInventoryItemPacket move = new MoveInventoryItemPacket();
            move.AgentData.AgentID = Frame.Agent.AgentID;
            move.AgentData.SessionID = Frame.Agent.SessionID;
            move.AgentData.Stamp = false; //FIXME: ??

            move.InventoryData = new MoveInventoryItemPacket.InventoryDataBlock[itemsNewParents.Count];

            int index = 0;
            foreach (KeyValuePair<UUID, UUID> entry in itemsNewParents)
            {
                MoveInventoryItemPacket.InventoryDataBlock block = new MoveInventoryItemPacket.InventoryDataBlock();
                block.ItemID = entry.Key;
                block.FolderID = entry.Value;
                block.NewName = Utils.EmptyBytes;
                move.InventoryData[index++] = block;
            }

            Frame.Network.InjectPacket(move, Direction.Outgoing);
        }

        #endregion Move

        #region Remove

        /// <summary>
        /// Remove descendants of a folder
        /// </summary>
        /// <param name="folder">The <seealso cref="UUID"/> of the folder</param>
        public void RemoveDescendants(UUID folder)
        {
            PurgeInventoryDescendentsPacket purge = new PurgeInventoryDescendentsPacket();
            purge.AgentData.AgentID = Frame.Agent.AgentID;
            purge.AgentData.SessionID = Frame.Agent.SessionID;
            purge.InventoryData.FolderID = folder;
            Frame.Network.InjectPacket(purge, Direction.Outgoing);

            // Update our local copy
            lock (_Store)
            {
                if (_Store.Contains(folder))
                {
                    List<InventoryBase> contents = _Store.GetContents(folder);
                    foreach (InventoryBase obj in contents)
                    {
                        _Store.RemoveNodeFor(obj);
                    }
                }
            }
        }

        /// <summary>
        /// Remove a single item from inventory
        /// </summary>
        /// <param name="item">The <seealso cref="UUID"/> of the inventory item to remove</param>
        public void RemoveItem(UUID item)
        {
            List<UUID> items = new List<UUID>(1);
            items.Add(item);

            Remove(items, null);
        }

        /// <summary>
        /// Remove a folder from inventory
        /// </summary>
        /// <param name="folder">The <seealso cref="UUID"/> of the folder to remove</param>
        public void RemoveFolder(UUID folder)
        {
            List<UUID> folders = new List<UUID>(1);
            folders.Add(folder);

            Remove(null, folders);
        }

        /// <summary>
        /// Remove multiple items or folders from inventory
        /// </summary>
        /// <param name="items">A List containing the <seealso cref="UUID"/>s of items to remove</param>
        /// <param name="folders">A List containing the <seealso cref="UUID"/>s of the folders to remove</param>
        public void Remove(List<UUID> items, List<UUID> folders)
        {
            if ((items == null || items.Count == 0) && (folders == null || folders.Count == 0))
                return;

            RemoveInventoryObjectsPacket rem = new RemoveInventoryObjectsPacket();
            rem.AgentData.AgentID = Frame.Agent.AgentID;
            rem.AgentData.SessionID = Frame.Agent.SessionID;

            if (items == null || items.Count == 0)
            {
                // To indicate that we want no items removed:
                rem.ItemData = new RemoveInventoryObjectsPacket.ItemDataBlock[1];
                rem.ItemData[0] = new RemoveInventoryObjectsPacket.ItemDataBlock();
                rem.ItemData[0].ItemID = UUID.Zero;
            }
            else
            {
                lock (_Store)
                {
                    rem.ItemData = new RemoveInventoryObjectsPacket.ItemDataBlock[items.Count];
                    for (int i = 0; i < items.Count; i++)
                    {
                        rem.ItemData[i] = new RemoveInventoryObjectsPacket.ItemDataBlock();
                        rem.ItemData[i].ItemID = items[i];

                        // Update local copy
                        if (_Store.Contains(items[i]))
                            _Store.RemoveNodeFor(Store[items[i]]);
                    }
                }
            }

            if (folders == null || folders.Count == 0)
            {
                // To indicate we want no folders removed:
                rem.FolderData = new RemoveInventoryObjectsPacket.FolderDataBlock[1];
                rem.FolderData[0] = new RemoveInventoryObjectsPacket.FolderDataBlock();
                rem.FolderData[0].FolderID = UUID.Zero;
            }
            else
            {
                lock (_Store)
                {
                    rem.FolderData = new RemoveInventoryObjectsPacket.FolderDataBlock[folders.Count];
                    for (int i = 0; i < folders.Count; i++)
                    {
                        rem.FolderData[i] = new RemoveInventoryObjectsPacket.FolderDataBlock();
                        rem.FolderData[i].FolderID = folders[i];

                        // Update local copy
                        if (_Store.Contains(folders[i]))
                            _Store.RemoveNodeFor(Store[folders[i]]);
                    }
                }
            }
            Frame.Network.InjectPacket(rem, Direction.Outgoing);
        }

        /// <summary>
        /// Empty the Lost and Found folder
        /// </summary>
        public void EmptyLostAndFound()
        {
            EmptySystemFolder(FolderType.LostAndFound);
        }

        /// <summary>
        /// Empty the Trash folder
        /// </summary>
        public void EmptyTrash()
        {
            EmptySystemFolder(FolderType.Trash);
        }

        private void EmptySystemFolder(FolderType folderType)
        {
            List<InventoryBase> items = _Store.GetContents(_Store.RootFolder);

            UUID folderKey = UUID.Zero;
            foreach (InventoryBase item in items)
            {
                if ((item as InventoryFolder) != null)
                {
                    InventoryFolder folder = item as InventoryFolder;
                    if (folder.PreferredType == folderType)
                    {
                        folderKey = folder.UUID;
                        break;
                    }
                }
            }
            items = _Store.GetContents(folderKey);
            List<UUID> remItems = new List<UUID>();
            List<UUID> remFolders = new List<UUID>();
            foreach (InventoryBase item in items)
            {
                if ((item as InventoryFolder) != null)
                {
                    remFolders.Add(item.UUID);
                }
                else
                {
                    remItems.Add(item.UUID);
                }
            }
            Remove(remItems, remFolders);
        }
        #endregion Remove

        #region Create

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parentFolder"></param>
        /// <param name="name"></param>
        /// <param name="description"></param>
        /// <param name="type"></param>
        /// <param name="assetTransactionID">Proper use is to upload the inventory's asset first, then provide the Asset's TransactionID here.</param>
        /// <param name="invType"></param>
        /// <param name="nextOwnerMask"></param>
        /// <param name="callback"></param>
        public void RequestCreateItem(UUID parentFolder, string name, string description, AssetType type, UUID assetTransactionID,
            InventoryType invType, PermissionMask nextOwnerMask, ItemCreatedCallback callback)
        {
            // Even though WearableType 0 is Shape, in this context it is treated as NOT_WEARABLE
            RequestCreateItem(parentFolder, name, description, type, assetTransactionID, invType, (WearableType)0, nextOwnerMask,
                callback);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parentFolder"></param>
        /// <param name="name"></param>
        /// <param name="description"></param>
        /// <param name="type"></param>
        /// <param name="assetTransactionID">Proper use is to upload the inventory's asset first, then provide the Asset's TransactionID here.</param>
        /// <param name="invType"></param>
        /// <param name="wearableType"></param>
        /// <param name="nextOwnerMask"></param>
        /// <param name="callback"></param>
        public void RequestCreateItem(UUID parentFolder, string name, string description, AssetType type, UUID assetTransactionID,
            InventoryType invType, WearableType wearableType, PermissionMask nextOwnerMask, ItemCreatedCallback callback)
        {
            CreateInventoryItemPacket create = new CreateInventoryItemPacket();
            create.AgentData.AgentID = Frame.Agent.AgentID;
            create.AgentData.SessionID = Frame.Agent.SessionID;

            create.InventoryBlock.CallbackID = RegisterItemCreatedCallback(callback);
            create.InventoryBlock.FolderID = parentFolder;
            create.InventoryBlock.TransactionID = assetTransactionID;
            create.InventoryBlock.NextOwnerMask = (uint)nextOwnerMask;
            create.InventoryBlock.Type = (sbyte)type;
            create.InventoryBlock.InvType = (sbyte)invType;
            create.InventoryBlock.WearableType = (byte)wearableType;
            create.InventoryBlock.Name = Utils.StringToBytes(name);
            create.InventoryBlock.Description = Utils.StringToBytes(description);

            Frame.Network.InjectPacket(create, Direction.Outgoing);
        }

        /// <summary>
        /// Creates a new inventory folder
        /// </summary>
        /// <param name="parentID">ID of the folder to put this folder in</param>
        /// <param name="name">Name of the folder to create</param>
        /// <returns>The UUID of the newly created folder</returns>
        public UUID CreateFolder(UUID parentID, string name)
        {
            return CreateFolder(parentID, name, FolderType.None);
        }

        /// <summary>
        /// Creates a new inventory folder
        /// </summary>
        /// <param name="parentID">ID of the folder to put this folder in</param>
        /// <param name="name">Name of the folder to create</param>
        /// <param name="preferredType">Sets this folder as the default folder
        /// for new assets of the specified type. Use <code>FolderType.None</code>
        /// to create a normal folder, otherwise it will likely create a
        /// duplicate of an existing folder type</param>
        /// <returns>The UUID of the newly created folder</returns>
        /// <remarks>If you specify a preferred type of <code>AsseType.Folder</code>
        /// it will create a new root folder which may likely cause all sorts
        /// of strange problems</remarks>
        public UUID CreateFolder(UUID parentID, string name, FolderType preferredType)
        {
            UUID id = UUID.Random();

            // Assign a folder name if one is not already set
            if (String.IsNullOrEmpty(name))
            {
                if (preferredType >= FolderType.Texture && preferredType <= FolderType.MarkplaceStock)
                {
                    name = _NewFolderNames[(int)preferredType];
                }
                else
                {
                    name = "New Folder";
                }
                if (name == string.Empty)
                {
                    name = "New Folder";
                }
            }

            // Create the new folder locally
            InventoryFolder newFolder = new InventoryFolder(id);
            newFolder.Version = 1;
            newFolder.DescendentCount = 0;
            newFolder.ParentUUID = parentID;
            newFolder.PreferredType = preferredType;
            newFolder.Name = name;
            newFolder.OwnerID = Frame.Agent.AgentID;

            // Update the local store
            try { _Store[newFolder.UUID] = newFolder; }
            catch (InventoryException ie) { OpenMetaverse.Logger.Log(ie.Message, Helpers.LogLevel.Warning, null, ie); }

            // Create the create folder packet and send it
            CreateInventoryFolderPacket create = new CreateInventoryFolderPacket();
            create.AgentData.AgentID = Frame.Agent.AgentID;
            create.AgentData.SessionID = Frame.Agent.SessionID;

            create.FolderData.FolderID = id;
            create.FolderData.ParentID = parentID;
            create.FolderData.Type = (sbyte)preferredType;
            create.FolderData.Name = Utils.StringToBytes(name);

            Frame.Network.InjectPacket(create, Direction.Outgoing);

            return id;
        }

        /// <summary>
        /// Create an inventory item and upload asset data
        /// </summary>
        /// <param name="data">Asset data</param>
        /// <param name="name">Inventory item name</param>
        /// <param name="description">Inventory item description</param>
        /// <param name="assetType">Asset type</param>
        /// <param name="invType">Inventory type</param>
        /// <param name="folderID">Put newly created inventory in this folder</param>
        /// <param name="callback">Delegate that will receive feedback on success or failure</param>
        public void RequestCreateItemFromAsset(byte[] data, string name, string description, AssetType assetType,
            InventoryType invType, UUID folderID, ItemCreatedFromAssetCallback callback)
        {
            Permissions permissions = new Permissions();
            permissions.EveryoneMask = PermissionMask.None;
            permissions.GroupMask = PermissionMask.None;
            permissions.NextOwnerMask = PermissionMask.All;

            RequestCreateItemFromAsset(data, name, description, assetType, invType, folderID, permissions, callback);
        }

        /// <summary>
        /// Create an inventory item and upload asset data
        /// </summary>
        /// <param name="data">Asset data</param>
        /// <param name="name">Inventory item name</param>
        /// <param name="description">Inventory item description</param>
        /// <param name="assetType">Asset type</param>
        /// <param name="invType">Inventory type</param>
        /// <param name="folderID">Put newly created inventory in this folder</param>
        /// <param name="permissions">Permission of the newly created item 
        /// (EveryoneMask, GroupMask, and NextOwnerMask of Permissions struct are supported)</param>
        /// <param name="callback">Delegate that will receive feedback on success or failure</param>
        public void RequestCreateItemFromAsset(byte[] data, string name, string description, AssetType assetType,
            InventoryType invType, UUID folderID, Permissions permissions, ItemCreatedFromAssetCallback callback)
        {
            if (Frame.Network.CurrentSim == null)
                throw new Exception("Active circuit is null!");

            string cap_url;
            if (!Frame.Network.CurrentSim.Caps.TryGetValue("NewFileAgentInventory", out cap_url))
            {
                throw new Exception("NewFileAgentInventory capability is not currently available");
            }

            Uri url = new Uri(cap_url);

            if (url != null)
            {
                OSDMap query = new OSDMap();
                query.Add("folder_id", OSD.FromUUID(folderID));
                query.Add("asset_type", OSD.FromString(Utils.AssetTypeToString(assetType)));
                query.Add("inventory_type", OSD.FromString(Utils.InventoryTypeToString(invType)));
                query.Add("name", OSD.FromString(name));
                query.Add("description", OSD.FromString(description));
                query.Add("everyone_mask", OSD.FromInteger((int)permissions.EveryoneMask));
                query.Add("group_mask", OSD.FromInteger((int)permissions.GroupMask));
                query.Add("next_owner_mask", OSD.FromInteger((int)permissions.NextOwnerMask));
                query.Add("expected_upload_cost", OSD.FromInteger(Frame.Config.UPLOAD_COST));

                // Make the request
                CapsClient request = new CapsClient(url);
                request.OnComplete += CreateItemFromAssetResponse;
                request.UserData = new object[] { callback, data, Frame.Config.CAPS_TIMEOUT, query };

                request.BeginGetResponse(query, OSDFormat.Xml, Frame.Config.CAPS_TIMEOUT);
            }
            else
            {
                throw new Exception("NewFileAgentInventory capability is not currently available");
            }
        }

        /// <summary>
        /// Creates inventory link to another inventory item or folder
        /// </summary>
        /// <param name="folderID">Put newly created link in folder with this UUID</param>
        /// <param name="bse">Inventory item or folder</param>
        /// <param name="callback">Method to call upon creation of the link</param>
        public void CreateLink(UUID folderID, InventoryBase bse, ItemCreatedCallback callback)
        {
            if (bse is InventoryFolder)
            {
                InventoryFolder folder = (InventoryFolder)bse;
                CreateLink(folderID, folder, callback);
            }
            else if (bse is InventoryItem)
            {
                InventoryItem item = (InventoryItem)bse;
                CreateLink(folderID, item.UUID, item.Name, item.Description, AssetType.Link, item.InventoryType, UUID.Random(), callback);
            }
        }

        /// <summary>
        /// Creates inventory link to another inventory item
        /// </summary>
        /// <param name="folderID">Put newly created link in folder with this UUID</param>
        /// <param name="item">Original inventory item</param>
        /// <param name="callback">Method to call upon creation of the link</param>
        public void CreateLink(UUID folderID, InventoryItem item, ItemCreatedCallback callback)
        {
            CreateLink(folderID, item.UUID, item.Name, item.Description, AssetType.Link, item.InventoryType, UUID.Random(), callback);
        }

        /// <summary>
        /// Creates inventory link to another inventory folder
        /// </summary>
        /// <param name="folderID">Put newly created link in folder with this UUID</param>
        /// <param name="folder">Original inventory folder</param>
        /// <param name="callback">Method to call upon creation of the link</param>
        public void CreateLink(UUID folderID, InventoryFolder folder, ItemCreatedCallback callback)
        {
            CreateLink(folderID, folder.UUID, folder.Name, "", AssetType.LinkFolder, InventoryType.Folder, UUID.Random(), callback);
        }

        /// <summary>
        /// Creates inventory link to another inventory item or folder
        /// </summary>
        /// <param name="folderID">Put newly created link in folder with this UUID</param>
        /// <param name="itemID">Original item's UUID</param>
        /// <param name="name">Name</param>
        /// <param name="description">Description</param>
        /// <param name="assetType">Asset Type</param>
        /// <param name="invType">Inventory Type</param>
        /// <param name="transactionID">Transaction UUID</param>
        /// <param name="callback">Method to call upon creation of the link</param>
        public void CreateLink(UUID folderID, UUID itemID, string name, string description, AssetType assetType, InventoryType invType, UUID transactionID, ItemCreatedCallback callback)
        {
            LinkInventoryItemPacket create = new LinkInventoryItemPacket();
            create.AgentData.AgentID = Frame.Agent.AgentID;
            create.AgentData.SessionID = Frame.Agent.SessionID;

            create.InventoryBlock.CallbackID = RegisterItemCreatedCallback(callback);
            lock (_ItemInventoryTypeRequest)
            {
                _ItemInventoryTypeRequest[create.InventoryBlock.CallbackID] = invType;
            }
            create.InventoryBlock.FolderID = folderID;
            create.InventoryBlock.TransactionID = transactionID;
            create.InventoryBlock.OldItemID = itemID;
            create.InventoryBlock.Type = (sbyte)assetType;
            create.InventoryBlock.InvType = (sbyte)invType;
            create.InventoryBlock.Name = Utils.StringToBytes(name);
            create.InventoryBlock.Description = Utils.StringToBytes(description);

            Frame.Network.InjectPacket(create, Direction.Outgoing);
        }

        #endregion Create

        #region Copy

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <param name="newParent"></param>
        /// <param name="newName"></param>
        /// <param name="callback"></param>
        public void RequestCopyItem(UUID item, UUID newParent, string newName, ItemCopiedCallback callback)
        {
            RequestCopyItem(item, newParent, newName, Frame.Agent.AgentID, callback);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <param name="newParent"></param>
        /// <param name="newName"></param>
        /// <param name="oldOwnerID"></param>
        /// <param name="callback"></param>
        public void RequestCopyItem(UUID item, UUID newParent, string newName, UUID oldOwnerID,
            ItemCopiedCallback callback)
        {
            List<UUID> items = new List<UUID>(1);
            items.Add(item);

            List<UUID> folders = new List<UUID>(1);
            folders.Add(newParent);

            List<string> names = new List<string>(1);
            names.Add(newName);

            RequestCopyItems(items, folders, names, oldOwnerID, callback);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="items"></param>
        /// <param name="targetFolders"></param>
        /// <param name="newNames"></param>
        /// <param name="oldOwnerID"></param>
        /// <param name="callback"></param>
        public void RequestCopyItems(List<UUID> items, List<UUID> targetFolders, List<string> newNames,
            UUID oldOwnerID, ItemCopiedCallback callback)
        {
            if (items.Count != targetFolders.Count || (newNames != null && items.Count != newNames.Count))
                throw new ArgumentException("All list arguments must have an equal number of entries");

            uint callbackID = RegisterItemsCopiedCallback(callback);

            CopyInventoryItemPacket copy = new CopyInventoryItemPacket();
            copy.AgentData.AgentID = Frame.Agent.AgentID;
            copy.AgentData.SessionID = Frame.Agent.SessionID;

            copy.InventoryData = new CopyInventoryItemPacket.InventoryDataBlock[items.Count];
            for (int i = 0; i < items.Count; ++i)
            {
                copy.InventoryData[i] = new CopyInventoryItemPacket.InventoryDataBlock();
                copy.InventoryData[i].CallbackID = callbackID;
                copy.InventoryData[i].NewFolderID = targetFolders[i];
                copy.InventoryData[i].OldAgentID = oldOwnerID;
                copy.InventoryData[i].OldItemID = items[i];

                if (newNames != null && !String.IsNullOrEmpty(newNames[i]))
                    copy.InventoryData[i].NewName = Utils.StringToBytes(newNames[i]);
                else
                    copy.InventoryData[i].NewName = Utils.EmptyBytes;
            }

            Frame.Network.InjectPacket(copy, Direction.Outgoing);
        }

        /// <summary>
        /// Request a copy of an asset embedded within a notecard
        /// </summary>
        /// <param name="objectID">Usually UUID.Zero for copying an asset from a notecard</param>
        /// <param name="notecardID">UUID of the notecard to request an asset from</param>
        /// <param name="folderID">Target folder for asset to go to in your inventory</param>
        /// <param name="itemID">UUID of the embedded asset</param>
        /// <param name="callback">callback to run when item is copied to inventory</param>
        public void RequestCopyItemFromNotecard(UUID objectID, UUID notecardID, UUID folderID, UUID itemID, ItemCopiedCallback callback)
        {
            _ItemCopiedCallbacks[0] = callback; //Notecards always use callback ID 0

            string cap_url = string.Empty;
            Frame.Network.CurrentSim.Caps.TryGetValue("CopyInventoryFromNotecard", out cap_url);

            if (cap_url != string.Empty)
            {
                CopyInventoryFromNotecardMessage message = new CopyInventoryFromNotecardMessage();
                message.CallbackID = 0;
                message.FolderID = folderID;
                message.ItemID = itemID;
                message.NotecardID = notecardID;
                message.ObjectID = objectID;

                CapsClient request = new CapsClient(new Uri(cap_url));
                request.BeginGetResponse(message.Serialize(), OSDFormat.Xml, Frame.Config.CAPS_TIMEOUT);
            }
            else
            {
                CopyInventoryFromNotecardPacket copy = new CopyInventoryFromNotecardPacket();
                copy.AgentData.AgentID = Frame.Agent.AgentID;
                copy.AgentData.SessionID = Frame.Agent.SessionID;

                copy.NotecardData.ObjectID = objectID;
                copy.NotecardData.NotecardItemID = notecardID;

                copy.InventoryData = new CopyInventoryFromNotecardPacket.InventoryDataBlock[1];
                copy.InventoryData[0] = new CopyInventoryFromNotecardPacket.InventoryDataBlock();
                copy.InventoryData[0].FolderID = folderID;
                copy.InventoryData[0].ItemID = itemID;

                Frame.Network.InjectPacket(copy, Direction.Outgoing);
            }
        }

        #endregion Copy

        #region Update

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        public void RequestUpdateItem(InventoryItem item)
        {
            List<InventoryItem> items = new List<InventoryItem>(1);
            items.Add(item);

            RequestUpdateItems(items, UUID.Random());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="items"></param>
        public void RequestUpdateItems(List<InventoryItem> items)
        {
            RequestUpdateItems(items, UUID.Random());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="items"></param>
        /// <param name="transactionID"></param>
        public void RequestUpdateItems(List<InventoryItem> items, UUID transactionID)
        {
            UpdateInventoryItemPacket update = new UpdateInventoryItemPacket();
            update.AgentData.AgentID = Frame.Agent.AgentID;
            update.AgentData.SessionID = Frame.Agent.SessionID;
            update.AgentData.TransactionID = transactionID;

            update.InventoryData = new UpdateInventoryItemPacket.InventoryDataBlock[items.Count];
            for (int i = 0; i < items.Count; i++)
            {
                InventoryItem item = items[i];

                UpdateInventoryItemPacket.InventoryDataBlock block = new UpdateInventoryItemPacket.InventoryDataBlock();
                block.BaseMask = (uint)item.Permissions.BaseMask;
                block.CRC = ItemCRC(item);
                block.CreationDate = (int)Utils.DateTimeToUnixTime(item.CreationDate);
                block.CreatorID = item.CreatorID;
                block.Description = Utils.StringToBytes(item.Description);
                block.EveryoneMask = (uint)item.Permissions.EveryoneMask;
                block.Flags = (uint)item.Flags;
                block.FolderID = item.ParentUUID;
                block.GroupID = item.GroupID;
                block.GroupMask = (uint)item.Permissions.GroupMask;
                block.GroupOwned = item.GroupOwned;
                block.InvType = (sbyte)item.InventoryType;
                block.ItemID = item.UUID;
                block.Name = Utils.StringToBytes(item.Name);
                block.NextOwnerMask = (uint)item.Permissions.NextOwnerMask;
                block.OwnerID = item.OwnerID;
                block.OwnerMask = (uint)item.Permissions.OwnerMask;
                block.SalePrice = item.SalePrice;
                block.SaleType = (byte)item.SaleType;
                block.TransactionID = item.TransactionID;
                block.Type = (sbyte)item.AssetType;

                update.InventoryData[i] = block;
            }

            Frame.Network.InjectPacket(update, Direction.Outgoing);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="notecardID"></param>
        /// <param name="callback"></param>
        public void RequestUploadNotecardAsset(byte[] data, UUID notecardID, InventoryUploadedAssetCallback callback)
        {
            if (Frame.Network.CurrentSim == null)
                throw new Exception("ActiceCircuit is null!");

            string cap_url;
            if (!Frame.Network.CurrentSim.Caps.TryGetValue("UpdateNotecardAgentInventory", out cap_url))
            {
                throw new Exception("UpdateNotecardAgentInventory capability is not currently available");
            }

            Uri url = new Uri(cap_url);

            if (url != null)
            {
                OSDMap query = new OSDMap();
                query.Add("item_id", OSD.FromUUID(notecardID));

                // Make the request
                CapsClient request = new CapsClient(url);
                request.OnComplete += UploadInventoryAssetResponse;
                request.UserData = new object[] { new KeyValuePair<InventoryUploadedAssetCallback, byte[]>(callback, data), notecardID };
                request.BeginGetResponse(query, OSDFormat.Xml, Frame.Config.CAPS_TIMEOUT);
            }
            else
            {
                throw new Exception("UpdateNotecardAgentInventory capability is not currently available");
            }
        }

        /// <summary>
        /// Save changes to notecard embedded in object contents
        /// </summary>
        /// <param name="data">Encoded notecard asset data</param>
        /// <param name="notecardID">Notecard UUID</param>
        /// <param name="taskID">Object's UUID</param>
        /// <param name="callback">Called upon finish of the upload with status information</param>
        public void RequestUpdateNotecardTask(byte[] data, UUID notecardID, UUID taskID, InventoryUploadedAssetCallback callback)
        {
            if (Frame.Network.CurrentSim == null)
                throw new Exception("ActiceCircuit is null!");

            string cap_url;
            if (!Frame.Network.CurrentSim.Caps.TryGetValue("UpdateNotecardTaskInventory", out cap_url))
            {
                throw new Exception("UpdateNotecardTaskInventory capability is not currently available");
            }

            Uri url = new Uri(cap_url);

            if (url != null)
            {
                OSDMap query = new OSDMap();
                query.Add("item_id", OSD.FromUUID(notecardID));
                query.Add("task_id", OSD.FromUUID(taskID));

                // Make the request
                CapsClient request = new CapsClient(url);
                request.OnComplete += UploadInventoryAssetResponse;
                request.UserData = new object[] { new KeyValuePair<InventoryUploadedAssetCallback, byte[]>(callback, data), notecardID };
                request.BeginGetResponse(query, OSDFormat.Xml, Frame.Config.CAPS_TIMEOUT);
            }
            else
            {
                throw new Exception("UpdateNotecardTaskInventory capability is not currently available");
            }
        }

        /// <summary>
        /// Upload new gesture asset for an inventory gesture item
        /// </summary>
        /// <param name="data">Encoded gesture asset</param>
        /// <param name="gestureID">Gesture inventory UUID</param>
        /// <param name="callback">Callback whick will be called when upload is complete</param>
        public void RequestUploadGestureAsset(byte[] data, UUID gestureID, InventoryUploadedAssetCallback callback)
        {
            if (Frame.Network.CurrentSim == null)
                throw new Exception("ActiceCircuit is null!");

            string cap_url;
            if(!Frame.Network.CurrentSim.Caps.TryGetValue("UpdateGestureAgentInventory", out cap_url))
            {
                throw new Exception("UpdateGestureAgentInventory capability is not currently available");
            }

            Uri url = new Uri(cap_url);

            if (url != null)
            {
                OSDMap query = new OSDMap();
                query.Add("item_id", OSD.FromUUID(gestureID));

                // Make the request
                CapsClient request = new CapsClient(url);
                request.OnComplete += UploadInventoryAssetResponse;
                request.UserData = new object[] { new KeyValuePair<InventoryUploadedAssetCallback, byte[]>(callback, data), gestureID };
                request.BeginGetResponse(query, OSDFormat.Xml, Frame.Config.CAPS_TIMEOUT);
            }
            else
            {
                throw new Exception("UpdateGestureAgentInventory capability is not currently available");
            }
        }

        /// <summary>
        /// Update an existing script in an agents Inventory
        /// </summary>
        /// <param name="data">A byte[] array containing the encoded scripts contents</param>
        /// <param name="itemID">the itemID of the script</param>
        /// <param name="mono">if true, sets the script content to run on the mono interpreter</param>
        /// <param name="callback"></param>
        public void RequestUpdateScriptAgentInventory(byte[] data, UUID itemID, bool mono, ScriptUpdatedCallback callback)
        {
            string cap_url;
            if (!Frame.Network.CurrentSim.Caps.TryGetValue("UpdateScriptAgent", out cap_url))
            {
                throw new Exception("UpdateScriptAgent capability is not currently available");
            }

            Uri url = new Uri(cap_url);

            if (url != null)
            {
                UpdateScriptAgentRequestMessage msg = new UpdateScriptAgentRequestMessage();
                msg.ItemID = itemID;
                msg.Target = mono ? "mono" : "lsl2";

                CapsClient request = new CapsClient(url);
                request.OnComplete += new CapsClient.CompleteCallback(UpdateScriptAgentInventoryResponse);
                request.UserData = new object[2] { new KeyValuePair<ScriptUpdatedCallback, byte[]>(callback, data), itemID };
                request.BeginGetResponse(msg.Serialize(), OSDFormat.Xml, Frame.Config.CAPS_TIMEOUT);
            }
            else
            {
                throw new Exception("UpdateScriptAgent capability is not currently available");
            }
        }

        /// <summary>
        /// Update an existing script in an task Inventory
        /// </summary>
        /// <param name="data">A byte[] array containing the encoded scripts contents</param>
        /// <param name="itemID">the itemID of the script</param>
        /// <param name="taskID">UUID of the prim containting the script</param>
        /// <param name="mono">if true, sets the script content to run on the mono interpreter</param>
        /// <param name="running">if true, sets the script to running</param>
        /// <param name="callback"></param>
        public void RequestUpdateScriptTask(byte[] data, UUID itemID, UUID taskID, bool mono, bool running, ScriptUpdatedCallback callback)
        {
            string cap_url;
            if (!Frame.Network.CurrentSim.Caps.TryGetValue("UpdateScriptTask", out cap_url))
            {
                throw new Exception("UpdateScriptTask capability is not currently available");
            }

            Uri url = new Uri(cap_url);

            if (url != null)
            {
                UpdateScriptTaskUpdateMessage msg = new UpdateScriptTaskUpdateMessage();
                msg.ItemID = itemID;
                msg.TaskID = taskID;
                msg.ScriptRunning = running;
                msg.Target = mono ? "mono" : "lsl2";

                CapsClient request = new CapsClient(url);
                request.OnComplete += new CapsClient.CompleteCallback(UpdateScriptAgentInventoryResponse);
                request.UserData = new object[2] { new KeyValuePair<ScriptUpdatedCallback, byte[]>(callback, data), itemID };
                request.BeginGetResponse(msg.Serialize(), OSDFormat.Xml, Frame.Config.CAPS_TIMEOUT);
            }
            else
            {
                throw new Exception("UpdateScriptTask capability is not currently available");
            }
        }
        #endregion Update

        #region Rez/Give

        /// <summary>
        /// Rez an object from inventory
        /// </summary>
        /// <param name="simulator">Simulator to place object in</param>
        /// <param name="rotation">Rotation of the object when rezzed</param>
        /// <param name="position">Vector of where to place object</param>
        /// <param name="item">InventoryItem object containing item details</param>
        public UUID RequestRezFromInventory(Simulator simulator, Quaternion rotation, Vector3 position,
            InventoryItem item)
        {
            return RequestRezFromInventory(simulator, rotation, position, item, UUID.Zero, //Client.Self.ActiveGroup, todo: fix this
                UUID.Random(), true);
        }

        /// <summary>
        /// Rez an object from inventory
        /// </summary>
        /// <param name="simulator">Simulator to place object in</param>
        /// <param name="rotation">Rotation of the object when rezzed</param>
        /// <param name="position">Vector of where to place object</param>
        /// <param name="item">InventoryItem object containing item details</param>
        /// <param name="groupOwner">UUID of group to own the object</param>
        public UUID RequestRezFromInventory(Simulator simulator, Quaternion rotation, Vector3 position,
            InventoryItem item, UUID groupOwner)
        {
            return RequestRezFromInventory(simulator, rotation, position, item, groupOwner, UUID.Random(), true);
        }

        /// <summary>
        /// Rez an object from inventory
        /// </summary>
        /// <param name="simulator">Simulator to place object in</param>
        /// <param name="rotation">Rotation of the object when rezzed</param>
        /// <param name="position">Vector of where to place object</param>
        /// <param name="item">InventoryItem object containing item details</param>
        /// <param name="groupOwner">UUID of group to own the object</param>        
        /// <param name="queryID">User defined queryID to correlate replies</param>
        /// <param name="rezSelected">If set to true, the CreateSelected flag
        /// will be set on the rezzed object</param>        
        public UUID RequestRezFromInventory(Simulator simulator, Quaternion rotation, Vector3 position,
            InventoryItem item, UUID groupOwner, UUID queryID, bool rezSelected)
        {
            return RequestRezFromInventory(simulator, UUID.Zero, rotation, position, item, groupOwner, queryID,
                                           rezSelected);
        }

        /// <summary>
        /// Rez an object from inventory
        /// </summary>
        /// <param name="simulator">Simulator to place object in</param>
        /// <param name="taskID">TaskID object when rezzed</param>
        /// <param name="rotation">Rotation of the object when rezzed</param>
        /// <param name="position">Vector of where to place object</param>
        /// <param name="item">InventoryItem object containing item details</param>
        /// <param name="groupOwner">UUID of group to own the object</param>        
        /// <param name="queryID">User defined queryID to correlate replies</param>
        /// <param name="rezSelected">If set to true, the CreateSelected flag
        /// will be set on the rezzed object</param>        
        public UUID RequestRezFromInventory(Simulator simulator, UUID taskID, Quaternion rotation, Vector3 position,
            InventoryItem item, UUID groupOwner, UUID queryID, bool rezSelected)
        {
            RezObjectPacket add = new RezObjectPacket();

            add.AgentData.AgentID = Frame.Agent.AgentID;
            add.AgentData.SessionID = Frame.Agent.SessionID;
            add.AgentData.GroupID = groupOwner;

            add.RezData.FromTaskID = taskID;
            add.RezData.BypassRaycast = 1;
            add.RezData.RayStart = position;
            add.RezData.RayEnd = position;
            add.RezData.RayTargetID = UUID.Zero;
            add.RezData.RayEndIsIntersection = false;
            add.RezData.RezSelected = rezSelected;
            add.RezData.RemoveItem = false;
            add.RezData.ItemFlags = (uint)item.Flags;
            add.RezData.GroupMask = (uint)item.Permissions.GroupMask;
            add.RezData.EveryoneMask = (uint)item.Permissions.EveryoneMask;
            add.RezData.NextOwnerMask = (uint)item.Permissions.NextOwnerMask;

            add.InventoryData.ItemID = item.UUID;
            add.InventoryData.FolderID = item.ParentUUID;
            add.InventoryData.CreatorID = item.CreatorID;
            add.InventoryData.OwnerID = item.OwnerID;
            add.InventoryData.GroupID = item.GroupID;
            add.InventoryData.BaseMask = (uint)item.Permissions.BaseMask;
            add.InventoryData.OwnerMask = (uint)item.Permissions.OwnerMask;
            add.InventoryData.GroupMask = (uint)item.Permissions.GroupMask;
            add.InventoryData.EveryoneMask = (uint)item.Permissions.EveryoneMask;
            add.InventoryData.NextOwnerMask = (uint)item.Permissions.NextOwnerMask;
            add.InventoryData.GroupOwned = item.GroupOwned;
            add.InventoryData.TransactionID = queryID;
            add.InventoryData.Type = (sbyte)item.InventoryType;
            add.InventoryData.InvType = (sbyte)item.InventoryType;
            add.InventoryData.Flags = (uint)item.Flags;
            add.InventoryData.SaleType = (byte)item.SaleType;
            add.InventoryData.SalePrice = item.SalePrice;
            add.InventoryData.Name = Utils.StringToBytes(item.Name);
            add.InventoryData.Description = Utils.StringToBytes(item.Description);
            add.InventoryData.CreationDate = (int)Utils.DateTimeToUnixTime(item.CreationDate);

            Frame.Network.InjectPacket(add, Direction.Outgoing);

            // Remove from store if the item is no copy
            if (Store.Items.ContainsKey(item.UUID) && Store[item.UUID] is InventoryItem)
            {
                InventoryItem invItem = (InventoryItem)Store[item.UUID];
                if ((invItem.Permissions.OwnerMask & PermissionMask.Copy) == PermissionMask.None)
                {
                    Store.RemoveNodeFor(invItem);
                }
            }

            return queryID;
        }

        /// <summary>
        /// DeRez an object from the simulator to the agents Objects folder in the agents Inventory
        /// </summary>
        /// <param name="objectLocalID">The simulator Local ID of the object</param>
        /// <remarks>If objectLocalID is a child primitive in a linkset, the entire linkset will be derezzed</remarks>
        public void RequestDeRezToInventory(uint objectLocalID)
        {
            RequestDeRezToInventory(objectLocalID, DeRezDestination.AgentInventoryTake,
                Frame.Inventory.FindFolderForType(AssetType.Object), UUID.Random());
        }

        /// <summary>
        /// DeRez an object from the simulator and return to inventory
        /// </summary>
        /// <param name="objectLocalID">The simulator Local ID of the object</param>
        /// <param name="destType">The type of destination from the <seealso cref="DeRezDestination"/> enum</param>
        /// <param name="destFolder">The destination inventory folders <seealso cref="UUID"/> -or- 
        /// if DeRezzing object to a tasks Inventory, the Tasks <seealso cref="UUID"/></param>
        /// <param name="transactionID">The transaction ID for this request which
        /// can be used to correlate this request with other packets</param>
        /// <remarks>If objectLocalID is a child primitive in a linkset, the entire linkset will be derezzed</remarks>
        public void RequestDeRezToInventory(uint objectLocalID, DeRezDestination destType, UUID destFolder, UUID transactionID)
        {
            DeRezObjectPacket take = new DeRezObjectPacket();

            take.AgentData.AgentID = Frame.Agent.AgentID;
            take.AgentData.SessionID = Frame.Agent.SessionID;
            take.AgentBlock = new DeRezObjectPacket.AgentBlockBlock();
            take.AgentBlock.GroupID = UUID.Zero;
            take.AgentBlock.Destination = (byte)destType;
            take.AgentBlock.DestinationID = destFolder;
            take.AgentBlock.PacketCount = 1;
            take.AgentBlock.PacketNumber = 1;
            take.AgentBlock.TransactionID = transactionID;

            take.ObjectData = new DeRezObjectPacket.ObjectDataBlock[1];
            take.ObjectData[0] = new DeRezObjectPacket.ObjectDataBlock();
            take.ObjectData[0].ObjectLocalID = objectLocalID;

            Frame.Network.InjectPacket(take, Direction.Outgoing);
        }

        /// <summary>
        /// Rez an item from inventory to its previous simulator location
        /// </summary>
        /// <param name="simulator"></param>
        /// <param name="item"></param>
        /// <param name="queryID"></param>
        /// <returns></returns>
        public UUID RequestRestoreRezFromInventory(Simulator simulator, InventoryItem item, UUID queryID)
        {
            RezRestoreToWorldPacket add = new RezRestoreToWorldPacket();

            add.AgentData.AgentID = Frame.Agent.AgentID;
            add.AgentData.SessionID = Frame.Agent.SessionID;

            add.InventoryData.ItemID = item.UUID;
            add.InventoryData.FolderID = item.ParentUUID;
            add.InventoryData.CreatorID = item.CreatorID;
            add.InventoryData.OwnerID = item.OwnerID;
            add.InventoryData.GroupID = item.GroupID;
            add.InventoryData.BaseMask = (uint)item.Permissions.BaseMask;
            add.InventoryData.OwnerMask = (uint)item.Permissions.OwnerMask;
            add.InventoryData.GroupMask = (uint)item.Permissions.GroupMask;
            add.InventoryData.EveryoneMask = (uint)item.Permissions.EveryoneMask;
            add.InventoryData.NextOwnerMask = (uint)item.Permissions.NextOwnerMask;
            add.InventoryData.GroupOwned = item.GroupOwned;
            add.InventoryData.TransactionID = queryID;
            add.InventoryData.Type = (sbyte)item.InventoryType;
            add.InventoryData.InvType = (sbyte)item.InventoryType;
            add.InventoryData.Flags = (uint)item.Flags;
            add.InventoryData.SaleType = (byte)item.SaleType;
            add.InventoryData.SalePrice = item.SalePrice;
            add.InventoryData.Name = Utils.StringToBytes(item.Name);
            add.InventoryData.Description = Utils.StringToBytes(item.Description);
            add.InventoryData.CreationDate = (int)Utils.DateTimeToUnixTime(item.CreationDate);

            Frame.Network.InjectPacket(add, Direction.Outgoing);

            return queryID;
        }

        /// <summary>
        /// Give an inventory item to another avatar
        /// </summary>
        /// <param name="itemID">The <seealso cref="UUID"/> of the item to give</param>
        /// <param name="itemName">The name of the item</param>
        /// <param name="assetType">The type of the item from the <seealso cref="AssetType"/> enum</param>
        /// <param name="recipient">The <seealso cref="UUID"/> of the recipient</param>
        /// <param name="doEffect">true to generate a beameffect during transfer</param>
        public void GiveItem(UUID itemID, string itemName, AssetType assetType, UUID recipient,
            bool doEffect)
        {
            throw new Exception("Ruby is lazy");
            //byte[] bucket;


            //bucket = new byte[17];
            //bucket[0] = (byte)assetType;
            //Buffer.BlockCopy(itemID.GetBytes(), 0, bucket, 1, 16);

            //Client.Self.InstantMessage(
            //        Client.Self.Name,
            //        recipient,
            //        itemName,
            //        UUID.Random(),
            //        InstantMessageDialog.InventoryOffered,
            //        InstantMessageOnline.Online,
            //        Client.Self.SimPosition,
            //        Client.Network.CurrentSim.ID,
            //        bucket);

            //if (doEffect)
            //{
            //    Client.Self.BeamEffect(Plugin.Frame.AgentID, recipient, Vector3d.Zero,
            //        Plugin.Settings.DEFAULT_EFFECT_COLOR, 1f, UUID.Random());
            //}

            //// Remove from store if the item is no copy
            //if (Store.Items.ContainsKey(itemID) && Store[itemID] is InventoryItem)
            //{
            //    InventoryItem invItem = (InventoryItem)Store[itemID];
            //    if ((invItem.Permissions.OwnerMask & PermissionMask.Copy) == PermissionMask.None)
            //    {
            //        Store.RemoveNodeFor(invItem);
            //    }
            //}
        }

        /// <summary>
        /// Give an inventory Folder with contents to another avatar
        /// </summary>
        /// <param name="folderID">The <seealso cref="UUID"/> of the Folder to give</param>
        /// <param name="folderName">The name of the folder</param>
        /// <param name="assetType">The type of the item from the <seealso cref="AssetType"/> enum</param>
        /// <param name="recipient">The <seealso cref="UUID"/> of the recipient</param>
        /// <param name="doEffect">true to generate a beameffect during transfer</param>
        public void GiveFolder(UUID folderID, string folderName, AssetType assetType, UUID recipient,
            bool doEffect)
        {

            byte[] bucket;

            List<InventoryItem> folderContents = new List<InventoryItem>();

            Frame.Inventory.FolderContents(folderID, Frame.Agent.AgentID, false, true, InventorySortOrder.ByDate, 1000 * 15).ForEach(
                delegate(InventoryBase ib)
                {
                    folderContents.Add(Frame.Inventory.FetchItem(ib.UUID, Frame.Agent.AgentID, 1000 * 10));
                });
            bucket = new byte[17 * (folderContents.Count + 1)];

            //Add parent folder (first item in bucket)
            bucket[0] = (byte)assetType;
            Buffer.BlockCopy(folderID.GetBytes(), 0, bucket, 1, 16);

            //Add contents to bucket after folder
            for (int i = 1; i <= folderContents.Count; ++i)
            {
                bucket[i * 17] = (byte)folderContents[i - 1].AssetType;
                Buffer.BlockCopy(folderContents[i - 1].UUID.GetBytes(), 0, bucket, i * 17 + 1, 16);
            }

            throw new Exception("Ruby is lazy");

            //Client.Self.InstantMessage(
            //        Client.Self.Name,
            //        recipient,
            //        folderName,
            //        UUID.Random(),
            //        InstantMessageDialog.InventoryOffered,
            //        InstantMessageOnline.Online,
            //        Client.Self.SimPosition,
            //        Client.Network.CurrentSim.ID,
            //        bucket);

            //if (doEffect)
            //{
            //    Client.Self.BeamEffect(Plugin.Frame.AgentID, recipient, Vector3d.Zero,
            //        Plugin.Settings.DEFAULT_EFFECT_COLOR, 1f, UUID.Random());
            //}

            //// Remove from store if items were no copy
            //for (int i = 0; i < folderContents.Count; i++)
            //{

            //    if (Store.Items.ContainsKey(folderContents[i].UUID) && Store[folderContents[i].UUID] is InventoryItem)
            //    {
            //        InventoryItem invItem = (InventoryItem)Store[folderContents[i].UUID];
            //        if ((invItem.Permissions.OwnerMask & PermissionMask.Copy) == PermissionMask.None)
            //        {
            //            Store.RemoveNodeFor(invItem);
            //        }
            //    }

            //}
        }

        #endregion Rez/Give

        #region Task

        /// <summary>
        /// Copy or move an <see cref="InventoryItem"/> from agent inventory to a task (primitive) inventory
        /// </summary>
        /// <param name="objectLocalID">The target object</param>
        /// <param name="item">The item to copy or move from inventory</param>
        /// <returns></returns>
        /// <remarks>For items with copy permissions a copy of the item is placed in the tasks inventory,
        /// for no-copy items the object is moved to the tasks inventory</remarks>
        // DocTODO: what does the return UUID correlate to if anything?
        public UUID UpdateTaskInventory(uint objectLocalID, InventoryItem item)
        {
            UUID transactionID = UUID.Random();

            UpdateTaskInventoryPacket update = new UpdateTaskInventoryPacket();
            update.AgentData.AgentID = Frame.Agent.AgentID;
            update.AgentData.SessionID = Frame.Agent.SessionID;
            update.UpdateData.Key = 0;
            update.UpdateData.LocalID = objectLocalID;

            update.InventoryData.ItemID = item.UUID;
            update.InventoryData.FolderID = item.ParentUUID;
            update.InventoryData.CreatorID = item.CreatorID;
            update.InventoryData.OwnerID = item.OwnerID;
            update.InventoryData.GroupID = item.GroupID;
            update.InventoryData.BaseMask = (uint)item.Permissions.BaseMask;
            update.InventoryData.OwnerMask = (uint)item.Permissions.OwnerMask;
            update.InventoryData.GroupMask = (uint)item.Permissions.GroupMask;
            update.InventoryData.EveryoneMask = (uint)item.Permissions.EveryoneMask;
            update.InventoryData.NextOwnerMask = (uint)item.Permissions.NextOwnerMask;
            update.InventoryData.GroupOwned = item.GroupOwned;
            update.InventoryData.TransactionID = transactionID;
            update.InventoryData.Type = (sbyte)item.AssetType;
            update.InventoryData.InvType = (sbyte)item.InventoryType;
            update.InventoryData.Flags = (uint)item.Flags;
            update.InventoryData.SaleType = (byte)item.SaleType;
            update.InventoryData.SalePrice = item.SalePrice;
            update.InventoryData.Name = Utils.StringToBytes(item.Name);
            update.InventoryData.Description = Utils.StringToBytes(item.Description);
            update.InventoryData.CreationDate = (int)Utils.DateTimeToUnixTime(item.CreationDate);
            update.InventoryData.CRC = ItemCRC(item);

            Frame.Network.InjectPacket(update, Direction.Outgoing);

            return transactionID;
        }

        public delegate void TaskInventoryHandler(uint object_id, bool success, List<InventoryBase> inv);

        /// <summary>
        /// Retrieve a listing of the items contained in a task (Primitive)
        /// </summary>
        /// <param name="objectID">The tasks <seealso cref="UUID"/></param>
        /// <param name="objectLocalID">The tasks simulator local ID</param>
        /// <param name="timeoutMS">milliseconds to wait for reply from simulator</param>
        /// <returns>A list containing the inventory items inside the task or null
        /// if a timeout occurs</returns>
        /// <remarks>This request blocks until the response from the simulator arrives 
        /// or timeoutMS is exceeded</remarks>
        public void GetTaskInventory(UUID objectID, uint objectLocalID, int timeoutMS, TaskInventoryHandler inv_callback)
        {
            new Task(() =>
            {
                string filename = null;
                AutoResetEvent taskReplyEvent = new AutoResetEvent(false);

                EventHandler<TaskInventoryReplyEventArgs> callback =
                    delegate (object sender, TaskInventoryReplyEventArgs e)
                    {
                        if (e.ItemID == objectID)
                        {
                            filename = e.AssetFilename;
                            taskReplyEvent.Set();
                            e.Handled = true;
                        }
                    };

                TaskInventoryReply += callback;

                RequestTaskInventory(objectLocalID);

                if (taskReplyEvent.WaitOne(timeoutMS, false))
                {
                    TaskInventoryReply -= callback;

                    if (!string.IsNullOrEmpty(filename))
                    {
                        byte[] assetData = null;
                        ulong xferID = 0;
                        AutoResetEvent taskDownloadEvent = new AutoResetEvent(false);

                        EventHandler<XferReceivedEventArgs> xferCallback =
                            delegate (object sender, XferReceivedEventArgs e)
                            {
                                if (e.Xfer.XferID == xferID)
                                {
                                    assetData = e.Xfer.AssetData;
                                    taskDownloadEvent.Set();
                                }
                            };

                        Frame.Assets.XferReceived += xferCallback;

                        // Start the actual asset xfer
                        xferID = Frame.Assets.RequestAssetXfer(filename, true, false, UUID.Zero, AssetType.Unknown, true);

                        if (taskDownloadEvent.WaitOne(timeoutMS, false))
                        {
                            Frame.Assets.XferReceived -= xferCallback;

                            string taskList = Utils.BytesToString(assetData);
                            inv_callback.Invoke(objectLocalID, true, ParseTaskInventory(taskList));
                        }
                        else
                        {
                            OpenMetaverse.Logger.Log("Timed out waiting for task inventory download for " + filename, Helpers.LogLevel.Warning);
                            Frame.Assets.XferReceived -= xferCallback;
                            inv_callback.Invoke(objectLocalID, false, null);
                        }
                    }
                    else
                    {
                        OpenMetaverse.Logger.DebugLog("Task is empty for " + objectLocalID);
                        inv_callback.Invoke(objectLocalID, true, new List<InventoryBase>(0));
                    }
                }
                else
                {
                    OpenMetaverse.Logger.Log("Timed out waiting for task inventory reply for " + objectLocalID, Helpers.LogLevel.Warning);
                    TaskInventoryReply -= callback;
                    inv_callback.Invoke(objectLocalID, false, null);
                }
            }).Start();
        }

        /// <summary>
        /// Request the contents of a tasks (primitives) inventory from the 
        /// current simulator
        /// </summary>
        /// <param name="objectLocalID">The LocalID of the object</param>
        /// <seealso cref="TaskInventoryReply"/>
        public void RequestTaskInventory(uint objectLocalID)
        {
            RequestTaskInventory(objectLocalID, null);
        }

        /// <summary>
        /// Request the contents of a tasks (primitives) inventory
        /// </summary>
        /// <param name="objectLocalID">The simulator Local ID of the object</param>
        /// <param name="simulator">A reference to the simulator object that contains the object</param>
        /// <seealso cref="TaskInventoryReply"/>
        public void RequestTaskInventory(uint objectLocalID, Simulator simulator)
        {
            RequestTaskInventoryPacket request = new RequestTaskInventoryPacket();
            request.AgentData.AgentID = Frame.Agent.AgentID;
            request.AgentData.SessionID = Frame.Agent.SessionID;
            request.InventoryData.LocalID = objectLocalID;

            Frame.Network.InjectPacket(request, Direction.Outgoing);
        }

        /// <summary>
        /// Move an item from a tasks (Primitive) inventory to the specified folder in the avatars inventory
        /// </summary>
        /// <param name="objectLocalID">LocalID of the object in the simulator</param>
        /// <param name="taskItemID">UUID of the task item to move</param>
        /// <param name="inventoryFolderID">The ID of the destination folder in this agents inventory</param>
        /// <param name="simulator">Simulator Object</param>
        /// <remarks>Raises the <see cref="OnTaskItemReceived"/> event</remarks>
        public void MoveTaskInventory(uint objectLocalID, UUID taskItemID, UUID inventoryFolderID, Simulator simulator)
        {
            MoveTaskInventoryPacket request = new MoveTaskInventoryPacket();
            request.AgentData.AgentID = Frame.Agent.AgentID;
            request.AgentData.SessionID = Frame.Agent.SessionID;

            request.AgentData.FolderID = inventoryFolderID;

            request.InventoryData.ItemID = taskItemID;
            request.InventoryData.LocalID = objectLocalID;

            Frame.Network.InjectPacket(request, Direction.Outgoing);
        }

        /// <summary>
        /// Remove an item from an objects (Prim) Inventory
        /// </summary>
        /// <param name="objectLocalID">LocalID of the object in the simulator</param>
        /// <param name="taskItemID">UUID of the task item to remove</param>
        /// <param name="simulator">Simulator Object</param>
        /// <remarks>You can confirm the removal by comparing the tasks inventory serial before and after the 
        /// request with the <see cref="RequestTaskInventory"/> request combined with
        /// the <seealso cref="TaskInventoryReply"/> event</remarks>
        public void RemoveTaskInventory(uint objectLocalID, UUID taskItemID, Simulator simulator)
        {
            RemoveTaskInventoryPacket remove = new RemoveTaskInventoryPacket();
            remove.AgentData.AgentID = Frame.Agent.AgentID;
            remove.AgentData.SessionID = Frame.Agent.SessionID;

            remove.InventoryData.ItemID = taskItemID;
            remove.InventoryData.LocalID = objectLocalID;

            Frame.Network.InjectPacket(remove, Direction.Outgoing);
        }

        /// <summary>
        /// Copy an InventoryScript item from the Agents Inventory into a primitives task inventory
        /// </summary>
        /// <param name="objectLocalID">An unsigned integer representing a primitive being simulated</param>
        /// <param name="item">An <seealso cref="InventoryItem"/> which represents a script object from the agents inventory</param>
        /// <param name="enableScript">true to set the scripts running state to enabled</param>
        /// <returns>A Unique Transaction ID</returns>
        /// <example>
        /// The following example shows the basic steps necessary to copy a script from the agents inventory into a tasks inventory
        /// and assumes the script exists in the agents inventory.
        /// <code>
        ///    uint primID = 95899503; // Fake prim ID
        ///    UUID scriptID = UUID.Parse("92a7fe8a-e949-dd39-a8d8-1681d8673232"); // Fake Script UUID in Inventory
        ///
        ///    Client.Inventory.FolderContents(Client.Inventory.FindFolderForType(AssetType.LSLText), Plugin.Frame.AgentID, 
        ///        false, true, InventorySortOrder.ByName, 10000);
        ///
        ///    Client.Inventory.RezScript(primID, (InventoryItem)Client.Inventory.Store[scriptID]);
        /// </code>
        /// </example>
        // DocTODO: what does the return UUID correlate to if anything?
        public UUID CopyScriptToTask(uint objectLocalID, InventoryItem item, bool enableScript)
        {
            UUID transactionID = UUID.Random();

            RezScriptPacket ScriptPacket = new RezScriptPacket();
            ScriptPacket.AgentData.AgentID = Frame.Agent.AgentID;
            ScriptPacket.AgentData.SessionID = Frame.Agent.SessionID;

            ScriptPacket.UpdateBlock.ObjectLocalID = objectLocalID;
            ScriptPacket.UpdateBlock.Enabled = enableScript;

            ScriptPacket.InventoryBlock.ItemID = item.UUID;
            ScriptPacket.InventoryBlock.FolderID = item.ParentUUID;
            ScriptPacket.InventoryBlock.CreatorID = item.CreatorID;
            ScriptPacket.InventoryBlock.OwnerID = item.OwnerID;
            ScriptPacket.InventoryBlock.GroupID = item.GroupID;
            ScriptPacket.InventoryBlock.BaseMask = (uint)item.Permissions.BaseMask;
            ScriptPacket.InventoryBlock.OwnerMask = (uint)item.Permissions.OwnerMask;
            ScriptPacket.InventoryBlock.GroupMask = (uint)item.Permissions.GroupMask;
            ScriptPacket.InventoryBlock.EveryoneMask = (uint)item.Permissions.EveryoneMask;
            ScriptPacket.InventoryBlock.NextOwnerMask = (uint)item.Permissions.NextOwnerMask;
            ScriptPacket.InventoryBlock.GroupOwned = item.GroupOwned;
            ScriptPacket.InventoryBlock.TransactionID = transactionID;
            ScriptPacket.InventoryBlock.Type = (sbyte)item.AssetType;
            ScriptPacket.InventoryBlock.InvType = (sbyte)item.InventoryType;
            ScriptPacket.InventoryBlock.Flags = (uint)item.Flags;
            ScriptPacket.InventoryBlock.SaleType = (byte)item.SaleType;
            ScriptPacket.InventoryBlock.SalePrice = item.SalePrice;
            ScriptPacket.InventoryBlock.Name = Utils.StringToBytes(item.Name);
            ScriptPacket.InventoryBlock.Description = Utils.StringToBytes(item.Description);
            ScriptPacket.InventoryBlock.CreationDate = (int)Utils.DateTimeToUnixTime(item.CreationDate);
            ScriptPacket.InventoryBlock.CRC = ItemCRC(item);

            Frame.Network.InjectPacket(ScriptPacket, Direction.Outgoing);

            return transactionID;
        }


        /// <summary>
        /// Request the running status of a script contained in a task (primitive) inventory
        /// </summary>
        /// <param name="objectID">The ID of the primitive containing the script</param>
        /// <param name="scriptID">The ID of the script</param>
        /// <remarks>The <see cref="ScriptRunningReply"/> event can be used to obtain the results of the 
        /// request</remarks>
        /// <seealso cref="ScriptRunningReply"/>
        public void RequestGetScriptRunning(UUID objectID, UUID scriptID)
        {
            GetScriptRunningPacket request = new GetScriptRunningPacket();
            request.Script.ObjectID = objectID;
            request.Script.ItemID = scriptID;

            Frame.Network.InjectPacket(request, Direction.Outgoing);
        }

        /// <summary>
        /// Send a request to set the running state of a script contained in a task (primitive) inventory
        /// </summary>
        /// <param name="objectID">The ID of the primitive containing the script</param>
        /// <param name="scriptID">The ID of the script</param>
        /// <param name="running">true to set the script running, false to stop a running script</param>
        /// <remarks>To verify the change you can use the <see cref="RequestGetScriptRunning"/> method combined
        /// with the <see cref="ScriptRunningReply"/> event</remarks>
        public void RequestSetScriptRunning(UUID objectID, UUID scriptID, bool running)
        {
            SetScriptRunningPacket request = new SetScriptRunningPacket();
            request.AgentData.AgentID = Frame.Agent.AgentID;
            request.AgentData.SessionID = Frame.Agent.SessionID;
            request.Script.Running = running;
            request.Script.ItemID = scriptID;
            request.Script.ObjectID = objectID;

            Frame.Network.InjectPacket(request, Direction.Outgoing);
        }

        #endregion Task

        #region Helper Functions

        private uint RegisterItemCreatedCallback(ItemCreatedCallback callback)
        {
            lock (_CallbacksLock)
            {
                if (_CallbackPos == UInt32.MaxValue)
                    _CallbackPos = 0;

                _CallbackPos++;

                if (_ItemCreatedCallbacks.ContainsKey(_CallbackPos))
                    OpenMetaverse.Logger.Log("Overwriting an existing ItemCreatedCallback", Helpers.LogLevel.Warning);

                _ItemCreatedCallbacks[_CallbackPos] = callback;

                return _CallbackPos;
            }
        }

        private uint RegisterItemsCopiedCallback(ItemCopiedCallback callback)
        {
            lock (_CallbacksLock)
            {
                if (_CallbackPos == UInt32.MaxValue)
                    _CallbackPos = 0;

                _CallbackPos++;

                if (_ItemCopiedCallbacks.ContainsKey(_CallbackPos))
                    OpenMetaverse.Logger.Log("Overwriting an existing ItemsCopiedCallback", Helpers.LogLevel.Warning);

                _ItemCopiedCallbacks[_CallbackPos] = callback;

                return _CallbackPos;
            }
        }

        /// <summary>
        /// Create a CRC from an InventoryItem
        /// </summary>
        /// <param name="iitem">The source InventoryItem</param>
        /// <returns>A uint representing the source InventoryItem as a CRC</returns>
        public static uint ItemCRC(InventoryItem iitem)
        {
            uint CRC = 0;

            // IDs
            CRC += iitem.AssetUUID.CRC(); // AssetID
            CRC += iitem.ParentUUID.CRC(); // FolderID
            CRC += iitem.UUID.CRC(); // ItemID

            // Permission stuff
            CRC += iitem.CreatorID.CRC(); // CreatorID
            CRC += iitem.OwnerID.CRC(); // OwnerID
            CRC += iitem.GroupID.CRC(); // GroupID

            // CRC += another 4 words which always seem to be zero -- unclear if this is a UUID or what
            CRC += (uint)iitem.Permissions.OwnerMask; //owner_mask;      // Either owner_mask or next_owner_mask may need to be
            CRC += (uint)iitem.Permissions.NextOwnerMask; //next_owner_mask; // switched with base_mask -- 2 values go here and in my
            CRC += (uint)iitem.Permissions.EveryoneMask; //everyone_mask;   // study item, the three were identical.
            CRC += (uint)iitem.Permissions.GroupMask; //group_mask;

            // The rest of the CRC fields
            CRC += (uint)iitem.Flags; // Flags
            CRC += (uint)iitem.InventoryType; // InvType
            CRC += (uint)iitem.AssetType; // Type 
            CRC += (uint)Utils.DateTimeToUnixTime(iitem.CreationDate); // CreationDate
            CRC += (uint)iitem.SalePrice;    // SalePrice
            CRC += (uint)((uint)iitem.SaleType * 0x07073096); // SaleType

            return CRC;
        }

        /// <summary>
        /// Reverses a cheesy XORing with a fixed UUID to convert a shadow_id to an asset_id
        /// </summary>
        /// <param name="shadowID">Obfuscated shadow_id value</param>
        /// <returns>Deobfuscated asset_id value</returns>
        public static UUID DecryptShadowID(UUID shadowID)
        {
            return shadowID ^ MAGIC_ID;
        }

        /// <summary>
        /// Does a cheesy XORing with a fixed UUID to convert an asset_id to a shadow_id
        /// </summary>
        /// <param name="assetID">asset_id value to obfuscate</param>
        /// <returns>Obfuscated shadow_id value</returns>
        public static UUID EncryptAssetID(UUID assetID)
        {
            return assetID ^ MAGIC_ID;
        }

        /// <summary>
        /// Wrapper for creating a new <seealso cref="InventoryItem"/> object
        /// </summary>
        /// <param name="type">The type of item from the <seealso cref="InventoryType"/> enum</param>
        /// <param name="id">The <seealso cref="UUID"/> of the newly created object</param>
        /// <returns>An <seealso cref="InventoryItem"/> object with the type and id passed</returns>
        public static InventoryItem CreateInventoryItem(InventoryType type, UUID id)
        {
            switch (type)
            {
                case InventoryType.Texture: return new InventoryTexture(id);
                case InventoryType.Sound: return new InventorySound(id);
                case InventoryType.CallingCard: return new InventoryCallingCard(id);
                case InventoryType.Landmark: return new InventoryLandmark(id);
                case InventoryType.Object: return new InventoryObject(id);
                case InventoryType.Notecard: return new InventoryNotecard(id);
                case InventoryType.Category: return new InventoryCategory(id);
                case InventoryType.LSL: return new InventoryLSL(id);
                case InventoryType.Snapshot: return new InventorySnapshot(id);
                case InventoryType.Attachment: return new InventoryAttachment(id);
                case InventoryType.Wearable: return new InventoryWearable(id);
                case InventoryType.Animation: return new InventoryAnimation(id);
                case InventoryType.Gesture: return new InventoryGesture(id);
                case InventoryType.Settings: return new InventorySettings(id);
                case InventoryType.Mesh: return new InventoryMesh(id);
                default: return new InventoryItem(type, id);
            }
        }

        private InventoryItem SafeCreateInventoryItem(InventoryType InvType, UUID ItemID)
        {
            InventoryItem ret = null;

            if (_Store.Contains(ItemID))
                ret = _Store[ItemID] as InventoryItem;

            if (ret == null)
                ret = CreateInventoryItem(InvType, ItemID);

            return ret;
        }

        private static bool ParseLine(string line, out string key, out string value)
        {
            // Clean up and convert tabs to spaces
            line = line.Trim();
            line = line.Replace('\t', ' ');

            // Shrink all whitespace down to single spaces
            while (line.IndexOf("  ") > 0)
                line = line.Replace("  ", " ");

            if (line.Length > 2)
            {
                int sep = line.IndexOf(' ');
                if (sep > 0)
                {
                    key = line.Substring(0, sep);
                    value = line.Substring(sep + 1);

                    return true;
                }
            }
            else if (line.Length == 1)
            {
                key = line;
                value = String.Empty;
                return true;
            }

            key = null;
            value = null;
            return false;
        }

        /// <summary>
        /// Parse the results of a RequestTaskInventory() response
        /// </summary>
        /// <param name="taskData">A string which contains the data from the task reply</param>
        /// <returns>A List containing the items contained within the tasks inventory</returns>
        public static List<InventoryBase> ParseTaskInventory(string taskData)
        {
            List<InventoryBase> items = new List<InventoryBase>();
            int lineNum = 0;
            string[] lines = taskData.Replace("\r\n", "\n").Split('\n');

            while (lineNum < lines.Length)
            {
                string key, value;
                if (ParseLine(lines[lineNum++], out key, out value))
                {
                    if (key == "inv_object")
                    {
                        #region inv_object

                        // In practice this appears to only be used for folders
                        UUID itemID = UUID.Zero;
                        UUID parentID = UUID.Zero;
                        string name = String.Empty;
                        AssetType assetType = AssetType.Unknown;

                        while (lineNum < lines.Length)
                        {
                            if (ParseLine(lines[lineNum++], out key, out value))
                            {
                                if (key == "{")
                                {
                                    continue;
                                }
                                else if (key == "}")
                                {
                                    break;
                                }
                                else if (key == "obj_id")
                                {
                                    UUID.TryParse(value, out itemID);
                                }
                                else if (key == "parent_id")
                                {
                                    UUID.TryParse(value, out parentID);
                                }
                                else if (key == "type")
                                {
                                    assetType = Utils.StringToAssetType(value);
                                }
                                else if (key == "name")
                                {
                                    name = value.Substring(0, value.IndexOf('|'));
                                }
                            }
                        }

                        if (assetType == AssetType.Folder)
                        {
                            InventoryFolder folder = new InventoryFolder(itemID);
                            folder.Name = name;
                            folder.ParentUUID = parentID;

                            items.Add(folder);
                        }
                        else
                        {
                            InventoryItem item = new InventoryItem(itemID);
                            item.Name = name;
                            item.ParentUUID = parentID;
                            item.AssetType = assetType;

                            items.Add(item);
                        }

                        #endregion inv_object
                    }
                    else if (key == "inv_item")
                    {
                        #region inv_item

                        // Any inventory item that links to an assetID, has permissions, etc
                        UUID itemID = UUID.Zero;
                        UUID assetID = UUID.Zero;
                        UUID parentID = UUID.Zero;
                        UUID creatorID = UUID.Zero;
                        UUID ownerID = UUID.Zero;
                        UUID lastOwnerID = UUID.Zero;
                        UUID groupID = UUID.Zero;
                        bool groupOwned = false;
                        string name = String.Empty;
                        string desc = String.Empty;
                        AssetType assetType = AssetType.Unknown;
                        InventoryType inventoryType = InventoryType.Unknown;
                        DateTime creationDate = Utils.Epoch;
                        uint flags = 0;
                        Permissions perms = Permissions.NoPermissions;
                        SaleType saleType = SaleType.Not;
                        int salePrice = 0;

                        while (lineNum < lines.Length)
                        {
                            if (ParseLine(lines[lineNum++], out key, out value))
                            {
                                if (key == "{")
                                {
                                    continue;
                                }
                                else if (key == "}")
                                {
                                    break;
                                }
                                else if (key == "item_id")
                                {
                                    UUID.TryParse(value, out itemID);
                                }
                                else if (key == "parent_id")
                                {
                                    UUID.TryParse(value, out parentID);
                                }
                                else if (key == "permissions")
                                {
                                    #region permissions

                                    while (lineNum < lines.Length)
                                    {
                                        if (ParseLine(lines[lineNum++], out key, out value))
                                        {
                                            if (key == "{")
                                            {
                                                continue;
                                            }
                                            else if (key == "}")
                                            {
                                                break;
                                            }
                                            else if (key == "creator_mask")
                                            {
                                                // Deprecated
                                                uint val;
                                                if (Utils.TryParseHex(value, out val))
                                                    perms.BaseMask = (PermissionMask)val;
                                            }
                                            else if (key == "base_mask")
                                            {
                                                uint val;
                                                if (Utils.TryParseHex(value, out val))
                                                    perms.BaseMask = (PermissionMask)val;
                                            }
                                            else if (key == "owner_mask")
                                            {
                                                uint val;
                                                if (Utils.TryParseHex(value, out val))
                                                    perms.OwnerMask = (PermissionMask)val;
                                            }
                                            else if (key == "group_mask")
                                            {
                                                uint val;
                                                if (Utils.TryParseHex(value, out val))
                                                    perms.GroupMask = (PermissionMask)val;
                                            }
                                            else if (key == "everyone_mask")
                                            {
                                                uint val;
                                                if (Utils.TryParseHex(value, out val))
                                                    perms.EveryoneMask = (PermissionMask)val;
                                            }
                                            else if (key == "next_owner_mask")
                                            {
                                                uint val;
                                                if (Utils.TryParseHex(value, out val))
                                                    perms.NextOwnerMask = (PermissionMask)val;
                                            }
                                            else if (key == "creator_id")
                                            {

                                                UUID.TryParse(value, out creatorID);
                                            }
                                            else if (key == "owner_id")
                                            {
                                                UUID.TryParse(value, out ownerID);
                                            }
                                            else if (key == "last_owner_id")
                                            {
                                                UUID.TryParse(value, out lastOwnerID);
                                            }
                                            else if (key == "group_id")
                                            {
                                                UUID.TryParse(value, out groupID);
                                            }
                                            else if (key == "group_owned")
                                            {
                                                uint val;
                                                if (UInt32.TryParse(value, out val))
                                                    groupOwned = (val != 0);
                                            }
                                        }
                                    }

                                    #endregion permissions
                                }
                                else if (key == "sale_info")
                                {
                                    #region sale_info

                                    while (lineNum < lines.Length)
                                    {
                                        if (ParseLine(lines[lineNum++], out key, out value))
                                        {
                                            if (key == "{")
                                            {
                                                continue;
                                            }
                                            else if (key == "}")
                                            {
                                                break;
                                            }
                                            else if (key == "sale_type")
                                            {
                                                saleType = Utils.StringToSaleType(value);
                                            }
                                            else if (key == "sale_price")
                                            {
                                                Int32.TryParse(value, out salePrice);
                                            }
                                        }
                                    }

                                    #endregion sale_info
                                }
                                else if (key == "shadow_id")
                                {
                                    UUID shadowID;
                                    if (UUID.TryParse(value, out shadowID))
                                        assetID = DecryptShadowID(shadowID);
                                }
                                else if (key == "asset_id")
                                {
                                    UUID.TryParse(value, out assetID);
                                }
                                else if (key == "type")
                                {
                                    assetType = Utils.StringToAssetType(value);
                                }
                                else if (key == "inv_type")
                                {
                                    inventoryType = Utils.StringToInventoryType(value);
                                }
                                else if (key == "flags")
                                {
                                    UInt32.TryParse(value, out flags);
                                }
                                else if (key == "name")
                                {
                                    name = value.Substring(0, value.IndexOf('|'));
                                }
                                else if (key == "desc")
                                {
                                    desc = value.Substring(0, value.IndexOf('|'));
                                }
                                else if (key == "creation_date")
                                {
                                    uint timestamp;
                                    if (UInt32.TryParse(value, out timestamp))
                                        creationDate = Utils.UnixTimeToDateTime(timestamp);
                                    else
                                        OpenMetaverse.Logger.Log("Failed to parse creation_date " + value, Helpers.LogLevel.Warning);
                                }
                            }
                        }

                        InventoryItem item = CreateInventoryItem(inventoryType, itemID);
                        item.AssetUUID = assetID;
                        item.AssetType = assetType;
                        item.CreationDate = creationDate;
                        item.CreatorID = creatorID;
                        item.Description = desc;
                        item.Flags = flags;
                        item.GroupID = groupID;
                        item.GroupOwned = groupOwned;
                        item.Name = name;
                        item.OwnerID = ownerID;
                        item.LastOwnerID = lastOwnerID;
                        item.ParentUUID = parentID;
                        item.Permissions = perms;
                        item.SalePrice = salePrice;
                        item.SaleType = saleType;

                        items.Add(item);

                        #endregion inv_item
                    }
                    else
                    {
                        OpenMetaverse.Logger.Log("Unrecognized token " + key + " in: " + Environment.NewLine + taskData,
                            Helpers.LogLevel.Error);
                    }
                }
            }

            return items;
        }

        #endregion Helper Functions

        #region Internal Callbacks

        //void Self_IM(object sender, InstantMessageEventArgs e)
        //{
        //    // TODO: MainAvatar.InstantMessageDialog.GroupNotice can also be an inventory offer, should we
        //    // handle it here?

        //    if (m_InventoryObjectOffered != null &&
        //        (e.IM.Dialog == InstantMessageDialog.InventoryOffered
        //        || e.IM.Dialog == InstantMessageDialog.TaskInventoryOffered))
        //    {
        //        AssetType type = AssetType.Unknown;
        //        UUID objectID = UUID.Zero;
        //        bool fromTask = false;

        //        if (e.IM.Dialog == InstantMessageDialog.InventoryOffered)
        //        {
        //            if (e.IM.BinaryBucket.Length == 17)
        //            {
        //                type = (AssetType)e.IM.BinaryBucket[0];
        //                objectID = new UUID(e.IM.BinaryBucket, 1);
        //                fromTask = false;
        //            }
        //            else
        //            {
        //                OpenMetaverse.Logger.Log("Malformed inventory offer from agent", Helpers.LogLevel.Warning, Client);
        //                return;
        //            }
        //        }
        //        else if (e.IM.Dialog == InstantMessageDialog.TaskInventoryOffered)
        //        {
        //            if (e.IM.BinaryBucket.Length == 1)
        //            {
        //                type = (AssetType)e.IM.BinaryBucket[0];
        //                fromTask = true;
        //            }
        //            else
        //            {
        //                OpenMetaverse.Logger.Log("Malformed inventory offer from object", Helpers.LogLevel.Warning, Client);
        //                return;
        //            }
        //        }

        //        // Find the folder where this is going to go
        //        UUID destinationFolderID = FindFolderForType(type);

        //        // Fire the callback
        //        try
        //        {
        //            ImprovedInstantMessagePacket imp = new ImprovedInstantMessagePacket();
        //            imp.AgentData.AgentID = Plugin.Frame.AgentID;
        //            imp.AgentData.SessionID = Plugin.Frame.SessionID;
        //            imp.MessageBlock.FromGroup = false;
        //            imp.MessageBlock.ToAgentID = e.IM.FromAgentID;
        //            imp.MessageBlock.Offline = 0;
        //            imp.MessageBlock.ID = e.IM.IMSessionID;
        //            imp.MessageBlock.Timestamp = 0;
        //            imp.MessageBlock.FromAgentName = Utils.StringToBytes(Client.Self.Name);
        //            imp.MessageBlock.Message = Utils.EmptyBytes;
        //            imp.MessageBlock.ParentEstateID = 0;
        //            imp.MessageBlock.RegionID = UUID.Zero;
        //            imp.MessageBlock.Position = Client.Self.SimPosition;

        //            InventoryObjectOfferedEventArgs args = new InventoryObjectOfferedEventArgs(e.IM, type, objectID, fromTask, destinationFolderID);

        //            OnInventoryObjectOffered(args);

        //            if (args.Accept)
        //            {
        //                // Accept the inventory offer
        //                switch (e.IM.Dialog)
        //                {
        //                    case InstantMessageDialog.InventoryOffered:
        //                        imp.MessageBlock.Dialog = (byte)InstantMessageDialog.InventoryAccepted;
        //                        break;
        //                    case InstantMessageDialog.TaskInventoryOffered:
        //                        imp.MessageBlock.Dialog = (byte)InstantMessageDialog.TaskInventoryAccepted;
        //                        break;
        //                    case InstantMessageDialog.GroupNotice:
        //                        imp.MessageBlock.Dialog = (byte)InstantMessageDialog.GroupNoticeInventoryAccepted;
        //                        break;
        //                }
        //                imp.MessageBlock.BinaryBucket = args.FolderID.GetBytes();
        //            }
        //            else
        //            {
        //                // Decline the inventory offer
        //                switch (e.IM.Dialog)
        //                {
        //                    case InstantMessageDialog.InventoryOffered:
        //                        imp.MessageBlock.Dialog = (byte)InstantMessageDialog.InventoryDeclined;
        //                        break;
        //                    case InstantMessageDialog.TaskInventoryOffered:
        //                        imp.MessageBlock.Dialog = (byte)InstantMessageDialog.TaskInventoryDeclined;
        //                        break;
        //                    case InstantMessageDialog.GroupNotice:
        //                        imp.MessageBlock.Dialog = (byte)InstantMessageDialog.GroupNoticeInventoryDeclined;
        //                        break;
        //                }

        //                imp.MessageBlock.BinaryBucket = Utils.EmptyBytes;
        //            }

        //            Plugin.Frame.proxy.InjectPacket(imp, Direction.Outgoing);
        //        }
        //        catch (Exception ex)
        //        {
        //            OpenMetaverse.Logger.Log(ex.Message, Helpers.LogLevel.Error, null, ex);
        //        }
        //    }
        //}

        private void CreateItemFromAssetResponse(CapsClient client, OSD result, Exception error)
        {
            object[] args = (object[])client.UserData;
            ItemCreatedFromAssetCallback callback = (ItemCreatedFromAssetCallback)args[0];
            byte[] itemData = (byte[])args[1];
            int millisecondsTimeout = (int)args[2];
            OSDMap request = (OSDMap)args[3];

            if (result == null)
            {
                try { callback(false, error.Message, UUID.Zero, UUID.Zero); }
                catch (Exception e) { OpenMetaverse.Logger.Log(e.Message, Helpers.LogLevel.Error, null, e); }
                return;
            }

            if (result.Type == OSDType.Unknown)
            {
                try
                {
                    callback(false, "Failed to parse asset and item UUIDs", UUID.Zero, UUID.Zero);
                }
                catch (Exception e)
                {
                    OpenMetaverse.Logger.Log(e.Message, Helpers.LogLevel.Error, null, e);
                }
            }

            OSDMap contents = (OSDMap)result;

            string status = contents["state"].AsString().ToLower();

            if (status == "upload")
            {
                string uploadURL = contents["uploader"].AsString();

                OpenMetaverse.Logger.DebugLog("CreateItemFromAsset: uploading to " + uploadURL);

                // This makes the assumption that all uploads go to CurrentSim, to avoid
                // the problem of HttpRequestState not knowing anything about simulators
                CapsClient upload = new CapsClient(new Uri(uploadURL));
                upload.OnComplete += CreateItemFromAssetResponse;
                upload.UserData = new object[] { callback, itemData, millisecondsTimeout, request };
                upload.BeginGetResponse(itemData, "application/octet-stream", millisecondsTimeout);
            }
            else if (status == "complete")
            {
                OpenMetaverse.Logger.DebugLog("CreateItemFromAsset: completed");

                if (contents.ContainsKey("new_inventory_item") && contents.ContainsKey("new_asset"))
                {
                    // Request full update on the item in order to update the local store
                    RequestFetchInventory(contents["new_inventory_item"].AsUUID(), Frame.Agent.AgentID);

                    try { callback(true, String.Empty, contents["new_inventory_item"].AsUUID(), contents["new_asset"].AsUUID()); }
                    catch (Exception e) { OpenMetaverse.Logger.Log(e.Message, Helpers.LogLevel.Error, null, e); }
                }
                else
                {
                    try { callback(false, "Failed to parse asset and item UUIDs", UUID.Zero, UUID.Zero); }
                    catch (Exception e) { OpenMetaverse.Logger.Log(e.Message, Helpers.LogLevel.Error, null, e); }
                }
            }
            else
            {
                // Failure
                try { callback(false, status, UUID.Zero, UUID.Zero); }
                catch (Exception e) { OpenMetaverse.Logger.Log(e.Message, Helpers.LogLevel.Error, null, e); }
            }
        }

        private void UploadInventoryAssetResponse(CapsClient client, OSD result, Exception error)
        {
            OSDMap contents = result as OSDMap;
            KeyValuePair<InventoryUploadedAssetCallback, byte[]> kvp = (KeyValuePair<InventoryUploadedAssetCallback, byte[]>)(((object[])client.UserData)[0]);
            InventoryUploadedAssetCallback callback = kvp.Key;
            byte[] itemData = (byte[])kvp.Value;

            if (error == null && contents != null)
            {
                string status = contents["state"].AsString();

                if (status == "upload")
                {
                    Uri uploadURL = contents["uploader"].AsUri();

                    if (uploadURL != null)
                    {
                        // This makes the assumption that all uploads go to CurrentSim, to avoid
                        // the problem of HttpRequestState not knowing anything about simulators
                        CapsClient upload = new CapsClient(uploadURL);
                        upload.OnComplete += UploadInventoryAssetResponse;
                        upload.UserData = new object[2] { kvp, (UUID)(((object[])client.UserData)[1]) };
                        upload.BeginGetResponse(itemData, "application/octet-stream", Frame.Config.CAPS_TIMEOUT);
                    }
                    else
                    {
                        try { callback(false, "Missing uploader URL", UUID.Zero, UUID.Zero); }
                        catch (Exception e) { OpenMetaverse.Logger.Log(e.Message, Helpers.LogLevel.Error, null, e); }
                    }
                }
                else if (status == "complete")
                {
                    if (contents.ContainsKey("new_asset"))
                    {
                        // Request full item update so we keep store in sync
                        RequestFetchInventory((UUID)(((object[])client.UserData)[1]), contents["new_asset"].AsUUID());

                        try { callback(true, String.Empty, (UUID)(((object[])client.UserData)[1]), contents["new_asset"].AsUUID()); }
                        catch (Exception e) { OpenMetaverse.Logger.Log(e.Message, Helpers.LogLevel.Error, null, e); }
                    }
                    else
                    {
                        try { callback(false, "Failed to parse asset and item UUIDs", UUID.Zero, UUID.Zero); }
                        catch (Exception e) { OpenMetaverse.Logger.Log(e.Message, Helpers.LogLevel.Error, null, e); }
                    }
                }
                else
                {
                    try { callback(false, status, UUID.Zero, UUID.Zero); }
                    catch (Exception e) { OpenMetaverse.Logger.Log(e.Message, Helpers.LogLevel.Error, null, e); }
                }
            }
            else
            {
                string message = "Unrecognized or empty response";

                if (error != null)
                {
                    if (error is WebException)
                        message = ((HttpWebResponse)((WebException)error).Response).StatusDescription;

                    if (message == null || message == "None")
                        message = error.Message;
                }

                try { callback(false, message, UUID.Zero, UUID.Zero); }
                catch (Exception e) { OpenMetaverse.Logger.Log(e.Message, Helpers.LogLevel.Error, null, e); }
            }
        }

        private void UpdateScriptAgentInventoryResponse(CapsClient client, OSD result, Exception error)
        {
            KeyValuePair<ScriptUpdatedCallback, byte[]> kvp = (KeyValuePair<ScriptUpdatedCallback, byte[]>)(((object[])client.UserData)[0]);
            ScriptUpdatedCallback callback = kvp.Key;
            byte[] itemData = (byte[])kvp.Value;

            if (result == null)
            {
                try { callback(false, error.Message, false, null, UUID.Zero, UUID.Zero); }
                catch (Exception e) { OpenMetaverse.Logger.Log(e.Message, Helpers.LogLevel.Error, null, e); }
                return;
            }

            OSDMap contents = (OSDMap)result;

            string status = contents["state"].AsString();
            if (status == "upload")
            {
                string uploadURL = contents["uploader"].AsString();

                CapsClient upload = new CapsClient(new Uri(uploadURL));
                upload.OnComplete += new CapsClient.CompleteCallback(UpdateScriptAgentInventoryResponse);
                upload.UserData = new object[2] { kvp, (UUID)(((object[])client.UserData)[1]) };
                upload.BeginGetResponse(itemData, "application/octet-stream", Frame.Config.CAPS_TIMEOUT);
            }
            else if (status == "complete" && callback != null)
            {
                if (contents.ContainsKey("new_asset"))
                {
                    // Request full item update so we keep store in sync
                    RequestFetchInventory((UUID)(((object[])client.UserData)[1]), contents["new_asset"].AsUUID());


                    try
                    {
                        List<string> compileErrors = null;

                        if (contents.ContainsKey("errors"))
                        {
                            OSDArray errors = (OSDArray)contents["errors"];
                            compileErrors = new List<string>(errors.Count);

                            for (int i = 0; i < errors.Count; i++)
                            {
                                compileErrors.Add(errors[i].AsString());
                            }
                        }

                        callback(true,
                            status,
                            contents["compiled"].AsBoolean(),
                            compileErrors,
                            (UUID)(((object[])client.UserData)[1]),
                            contents["new_asset"].AsUUID());
                    }
                    catch (Exception e) { OpenMetaverse.Logger.Log(e.Message, Helpers.LogLevel.Error, null, e); }
                }
                else
                {
                    try { callback(false, "Failed to parse asset UUID", false, null, UUID.Zero, UUID.Zero); }
                    catch (Exception e) { OpenMetaverse.Logger.Log(e.Message, Helpers.LogLevel.Error, null, e); }
                }
            }
            else if (callback != null)
            {
                try { callback(false, status, false, null, UUID.Zero, UUID.Zero); }
                catch (Exception e) { OpenMetaverse.Logger.Log(e.Message, Helpers.LogLevel.Error, null, e); }
            }
        }
        #endregion Internal Handlers

        #region Packet Handlers
        
        protected void BulkUpdateInventoryCapHandler(string capsKey, OpenMetaverse.Interfaces.IMessage message, Simulator simulator)
        {
            BulkUpdateInventoryMessage msg = (BulkUpdateInventoryMessage)message;

            foreach (BulkUpdateInventoryMessage.FolderDataInfo newFolder in msg.FolderData)
            {
                if (newFolder.FolderID == UUID.Zero) continue;

                InventoryFolder folder;
                if (!_Store.Contains(newFolder.FolderID))
                {
                    folder = new InventoryFolder(newFolder.FolderID);
                }
                else
                {
                    folder = (InventoryFolder)_Store[newFolder.FolderID];
                }

                folder.Name = newFolder.Name;
                folder.ParentUUID = newFolder.ParentID;
                folder.PreferredType = newFolder.Type;
                _Store[folder.UUID] = folder;
            }

            foreach (BulkUpdateInventoryMessage.ItemDataInfo newItem in msg.ItemData)
            {
                if (newItem.ItemID == UUID.Zero) continue;
                InventoryType invType = newItem.InvType;

                lock (_ItemInventoryTypeRequest)
                {
                    InventoryType storedType = 0;
                    if (_ItemInventoryTypeRequest.TryGetValue(newItem.CallbackID, out storedType))
                    {
                        _ItemInventoryTypeRequest.Remove(newItem.CallbackID);
                        invType = storedType;
                    }
                }
                InventoryItem item = SafeCreateInventoryItem(invType, newItem.ItemID);

                item.AssetType = newItem.Type;
                item.AssetUUID = newItem.AssetID;
                item.CreationDate = newItem.CreationDate;
                item.CreatorID = newItem.CreatorID;
                item.Description = newItem.Description;
                item.Flags = newItem.Flags;
                item.GroupID = newItem.GroupID;
                item.GroupOwned = newItem.GroupOwned;
                item.Name = newItem.Name;
                item.OwnerID = newItem.OwnerID;
                item.ParentUUID = newItem.FolderID;
                item.Permissions.BaseMask = newItem.BaseMask;
                item.Permissions.EveryoneMask = newItem.EveryoneMask;
                item.Permissions.GroupMask = newItem.GroupMask;
                item.Permissions.NextOwnerMask = newItem.NextOwnerMask;
                item.Permissions.OwnerMask = newItem.OwnerMask;
                item.SalePrice = newItem.SalePrice;
                item.SaleType = newItem.SaleType;

                _Store[item.UUID] = item;

                // Look for an "item created" callback
                ItemCreatedCallback callback;
                if (_ItemCreatedCallbacks.TryGetValue(newItem.CallbackID, out callback))
                {
                    _ItemCreatedCallbacks.Remove(newItem.CallbackID);

                    try { callback(true, item); }
                    catch (Exception ex) { OpenMetaverse.Logger.Log(ex.Message, Helpers.LogLevel.Error, null, ex); }
                }

                // Look for an "item copied" callback
                ItemCopiedCallback copyCallback;
                if (_ItemCopiedCallbacks.TryGetValue(newItem.CallbackID, out copyCallback))
                {
                    _ItemCopiedCallbacks.Remove(newItem.CallbackID);

                    try { copyCallback(item); }
                    catch (Exception ex) { OpenMetaverse.Logger.Log(ex.Message, Helpers.LogLevel.Error, null, ex); }
                }

            }

        }
        
        //protected void ScriptRunningReplyMessageHandler(string capsKey, Interfaces.IMessage message, Simulator simulator)
        //{
        //    if (m_ScriptRunningReply != null)
        //    {
        //        ScriptRunningReplyMessage msg = (ScriptRunningReplyMessage)message;
        //        OnScriptRunningReply(new ScriptRunningReplyEventArgs(msg.ObjectID, msg.ItemID, msg.Mono, msg.Running));
        //    }
        //}

        #endregion Packet Handlers

        #region Useful Proxy Stuff
        private FetchInventoryReplyPacket.InventoryDataBlock InvItemToBlock(InventoryItem item)
        {
            uint crc = ItemCRC(item);

            FetchInventoryReplyPacket.InventoryDataBlock block = new FetchInventoryReplyPacket.InventoryDataBlock();
            block.AssetID = item.AssetUUID;
            block.BaseMask = (uint)item.Permissions.BaseMask;
            block.CRC = crc;
            block.CreationDate = (int)Utils.DateTimeToUnixTime(item.CreationDate);
            block.CreatorID = item.CreatorID;
            block.Description = Utils.StringToBytes(item.Description);
            block.EveryoneMask = (uint)item.Permissions.EveryoneMask;
            block.Flags = item.Flags;
            block.FolderID = item.ParentUUID;
            block.GroupID = item.GroupID;
            block.GroupMask = (uint)item.Permissions.GroupMask;
            block.GroupOwned = item.GroupOwned;
            block.InvType = (sbyte)item.InventoryType;
            block.ItemID = item.UUID;
            block.Name = Utils.StringToBytes(item.Name);
            block.NextOwnerMask = (uint)item.Permissions.NextOwnerMask;
            block.OwnerID = item.OwnerID;
            block.OwnerMask = (uint)item.Permissions.OwnerMask;
            block.SalePrice = item.SalePrice;
            block.SaleType = (byte)item.SaleType;
            block.Type = (sbyte)item.AssetType;

            return block;
        }

        public void InjectFetchInventoryReply(InventoryItem item)
        {
            InjectFetchInventoryReply(new InventoryItem[] { item });
        }

        public void InjectFetchInventoryReply(InventoryItem[] items)
        {
            FetchInventoryReplyPacket firp = new FetchInventoryReplyPacket();
            firp.AgentData.AgentID = Frame.Agent.AgentID;
            firp.InventoryData = new FetchInventoryReplyPacket.InventoryDataBlock[items.Length];

            for (int i = 0; i < items.Length; i++)
                firp.InventoryData[i] = InvItemToBlock(items[i]);

            Frame.Network.CurrentSim.Inject(firp, Direction.Incoming);
        }
        #endregion
    }

    #region EventArgs

    public class InventoryObjectOfferedEventArgs : EventArgs
    {
        private readonly InstantMessage m_Offer;
        private readonly AssetType m_AssetType;
        private readonly UUID m_ObjectID;
        private readonly bool m_FromTask;

        /// <summary>Set to true to accept offer, false to decline it</summary>
        public bool Accept { get; set; }
        /// <summary>The folder to accept the inventory into, if null default folder for <see cref="AssetType"/> will be used</summary>
        public UUID FolderID { get; set; }

        public InstantMessage Offer { get { return m_Offer; } }
        public AssetType AssetType { get { return m_AssetType; } }
        public UUID ObjectID { get { return m_ObjectID; } }
        public bool FromTask { get { return m_FromTask; } }

        public InventoryObjectOfferedEventArgs(InstantMessage offerDetails, AssetType type, UUID objectID, bool fromTask, UUID folderID)
        {
            this.Accept = false;
            this.FolderID = folderID;
            this.m_Offer = offerDetails;
            this.m_AssetType = type;
            this.m_ObjectID = objectID;
            this.m_FromTask = fromTask;
        }
    }

    public class FolderUpdatedEventArgs : EventArgs
    {
        private readonly UUID m_FolderID;
        public UUID FolderID { get { return m_FolderID; } }
        private readonly bool m_Success;
        public bool Success { get { return m_Success; } }

        public FolderUpdatedEventArgs(UUID folderID, bool success)
        {
            this.m_FolderID = folderID;
            this.m_Success = success;
        }
    }

    public class ItemReceivedEventArgs : EventArgs
    {
        private readonly InventoryItem m_Item;

        public InventoryItem Item { get { return m_Item; } }

        public ItemReceivedEventArgs(InventoryItem item)
        {
            this.m_Item = item;
        }
    }

    public class FindObjectByPathReplyEventArgs : EventArgs
    {
        private readonly String m_Path;
        private readonly UUID m_InventoryObjectID;

        public String Path { get { return m_Path; } }
        public UUID InventoryObjectID { get { return m_InventoryObjectID; } }

        public FindObjectByPathReplyEventArgs(string path, UUID inventoryObjectID)
        {
            this.m_Path = path;
            this.m_InventoryObjectID = inventoryObjectID;
        }
    }

    /// <summary>
    /// Callback when an inventory object is accepted and received from a
    /// task inventory. This is the callback in which you actually get
    /// the ItemID, as in ObjectOfferedCallback it is null when received
    /// from a task.
    /// </summary>
    public class TaskItemReceivedEventArgs : EventArgs
    {
        private readonly UUID m_ItemID;
        private readonly UUID m_FolderID;
        private readonly UUID m_CreatorID;
        private readonly UUID m_AssetID;
        private readonly InventoryType m_Type;

        public UUID ItemID { get { return m_ItemID; } }
        public UUID FolderID { get { return m_FolderID; } }
        public UUID CreatorID { get { return m_CreatorID; } }
        public UUID AssetID { get { return m_AssetID; } }
        public InventoryType Type { get { return m_Type; } }

        public TaskItemReceivedEventArgs(UUID itemID, UUID folderID, UUID creatorID, UUID assetID, InventoryType type)
        {
            this.m_ItemID = itemID;
            this.m_FolderID = folderID;
            this.m_CreatorID = creatorID;
            this.m_AssetID = assetID;
            this.m_Type = type;
        }
    }

    public class TaskInventoryReplyEventArgs : EventArgs
    {
        private readonly UUID m_ItemID;
        private readonly Int16 m_Serial;
        private readonly String m_AssetFilename;

        public bool Handled { get; set; } = false;

        public UUID ItemID { get { return m_ItemID; } }
        public Int16 Serial { get { return m_Serial; } }
        public String AssetFilename { get { return m_AssetFilename; } }

        public TaskInventoryReplyEventArgs(UUID itemID, short serial, string assetFilename)
        {
            this.m_ItemID = itemID;
            this.m_Serial = serial;
            this.m_AssetFilename = assetFilename;
        }
    }

    public class SaveAssetToInventoryEventArgs : EventArgs
    {
        private readonly UUID m_ItemID;
        private readonly UUID m_NewAssetID;

        public UUID ItemID { get { return m_ItemID; } }
        public UUID NewAssetID { get { return m_NewAssetID; } }

        public SaveAssetToInventoryEventArgs(UUID itemID, UUID newAssetID)
        {
            this.m_ItemID = itemID;
            this.m_NewAssetID = newAssetID;
        }
    }

    public class ScriptRunningReplyEventArgs : EventArgs
    {
        private readonly UUID m_ObjectID;
        private readonly UUID m_ScriptID;
        private readonly bool m_IsMono;
        private readonly bool m_IsRunning;

        public UUID ObjectID { get { return m_ObjectID; } }
        public UUID ScriptID { get { return m_ScriptID; } }
        public bool IsMono { get { return m_IsMono; } }
        public bool IsRunning { get { return m_IsRunning; } }

        public ScriptRunningReplyEventArgs(UUID objectID, UUID sctriptID, bool isMono, bool isRunning)
        {
            this.m_ObjectID = objectID;
            this.m_ScriptID = sctriptID;
            this.m_IsMono = isMono;
            this.m_IsRunning = isRunning;
        }
    }
    #endregion
}
