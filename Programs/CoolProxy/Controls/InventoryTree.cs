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

using Nwc.XmlRpc;
using OpenMetaverse;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using GridProxy;
using FolderUpdatedEventArgs = GridProxy.FolderUpdatedEventArgs;

namespace CoolGUI.Controls
{

    public delegate void HandleInventory(InventoryItem inventoryItem);
    public delegate void HandleInventoryFolder(InventoryFolder inventoryFolder);

    /// <summary>
    /// TreeView GUI component for browsing a client's inventory
    /// </summary>
    public class InventoryTree : TreeView
    {
        private ProxyFrame _Frame;

        private ContextMenuStrip _ContextMenu;
        private UUID _SelectedItemID;

        /// <summary>
        /// Gets or sets the context menu associated with this control
        /// </summary>
        public ContextMenuStrip Menu
        {
            get { return _ContextMenu; }
            set { _ContextMenu = value; }
        }

        /// <summary>
        /// Gets or sets the GridClient associated with this control
        /// </summary>
        public ProxyFrame Frame
        {
            get { return _Frame; }
            set { if (value != null) InitializeClient(value); }
        }

        /// <summary>
        /// TreeView control for an unspecified client's inventory
        /// </summary>
        public InventoryTree()
        {
            _ContextMenu = new ContextMenuStrip();
            _ContextMenu.ShowImageMargin = false;

            this.NodeMouseClick += new TreeNodeMouseClickEventHandler(InventoryTree_NodeMouseClick);
            this.BeforeExpand += new TreeViewCancelEventHandler(InventoryTree_BeforeExpand);

            this.AfterExpand += InventoryTree_AfterExpand;
            this.AfterCollapse += InventoryTree_AfterCollapse;
        }

        private int GetFolderIcon(InventoryFolder folder, bool open)
        {
            if (folder.Name == "#Firestorm" && folder.ParentUUID == _Frame.Inventory.InventoryRoot) return open ? 9 : 8;
            if (folder.Name == "Animation Overrides" && folder.ParentUUID == _Frame.Inventory.InventoryRoot) return open ? 40 : 39;

            switch (folder.PreferredType)
            {
                case FolderType.None:
                    return open ? 7 : 6;
                case FolderType.Trash:
                    return open ? 44 : 43;
                case FolderType.LostAndFound:
                    return open ? 22 : 21;
                case FolderType.Outfit:
                    return open ? 20 : 19;
                case FolderType.Animation:
                case FolderType.BodyPart:
                case FolderType.CallingCard:
                case FolderType.Clothing:
                case FolderType.CurrentOutfit:
                case FolderType.Favorites:
                case FolderType.Gesture:
                case FolderType.Landmark:
                case FolderType.LSLText:
                case FolderType.MyOutfits:
                case FolderType.Notecard:
                case FolderType.Object:
                case FolderType.Inbox:
                case FolderType.Snapshot:
                case FolderType.Sound:
                case FolderType.Suitcase:
                case FolderType.Texture:
                case FolderType.Settings:
                    return open ? 40 : 39;
                default:
                    return open ? 7 : 6;
            }
        }

        private void InventoryTree_AfterCollapse(object sender, TreeViewEventArgs e)
        {
            InventoryFolder folder = (InventoryFolder)Frame.Inventory.Store[new UUID(e.Node.Name)];
            e.Node.ImageIndex = e.Node.SelectedImageIndex = GetFolderIcon(folder, false);
        }

        private void InventoryTree_AfterExpand(object sender, TreeViewEventArgs e)
        {
            InventoryFolder folder = (InventoryFolder)Frame.Inventory.Store[new UUID(e.Node.Name)];
            e.Node.ImageIndex = e.Node.SelectedImageIndex = GetFolderIcon(folder, true);
        }

        /// <summary>
        /// TreeView control for the specified client's inventory
        /// </summary>
        /// <param name="client"></param>
        public InventoryTree(ProxyFrame frame) : this ()
        {
            InitializeClient(frame);
        }

        /// <summary>
        /// Thread-safe method for clearing the TreeView control
        /// </summary>
        public void ClearNodes()
        {
            if (this.InvokeRequired) this.BeginInvoke((MethodInvoker)delegate { ClearNodes(); });
            else this.Nodes.Clear();
        }

        /// <summary>
        /// Thread-safe method for collapsing a TreeNode in the control
        /// </summary>
        /// <param name="node"></param>
        public void CollapseNode(TreeNode node)
        {
            if (this.InvokeRequired) this.BeginInvoke((MethodInvoker)delegate { CollapseNode(node); });
            else if (!node.IsExpanded) node.Collapse();
        }

        /// <summary>
        /// Thread-safe method for expanding a TreeNode in the control
        /// </summary>
        /// <param name="node"></param>
        public void ExpandNode(TreeNode node)
        {
            if (this.InvokeRequired) this.BeginInvoke((MethodInvoker)delegate { ExpandNode(node); });
            else if (!node.IsExpanded) node.Expand();
        }

        /// <summary>
        /// Thread-safe method for updating the contents of the specified folder UUID
        /// </summary>
        /// <param name="folderID"></param>
        public void UpdateFolder(UUID folderID)
        {
            if (this.InvokeRequired) this.BeginInvoke((MethodInvoker)delegate { UpdateFolder(folderID); });
            else
            {
                TreeNode node = null;
                TreeNodeCollection children;

                TreeNode[] found = Nodes.Find(folderID.ToString(), true);
                if (found.Length > 0)
                {
                    node = found[0];
                    children = node.Nodes;
                }
                else
                {
                    OpenMetaverse.Logger.Log("Received update for unknown TreeView node " + folderID, Helpers.LogLevel.Warning);
                    return;
                }

                this.SuspendLayout();
                this.BeginUpdate();

                children.Clear();

                List<InventoryBase> contents = Frame.Inventory.Store.GetContents(folderID);
                if (contents.Count == 0)
                {
                    TreeNode add = children.Add(null, "(empty)");
                    add.ForeColor = Color.FromKnownColor(KnownColor.ScrollBar);
                    add.ImageIndex = add.SelectedImageIndex = 52;
                }
                else
                {
                    List<TreeNode> nodes = new List<TreeNode>();

                    foreach (InventoryBase inv in contents)
                    {
                        string key = inv.UUID.ToString();

                        TreeNode _node = null;

                        if (inv is InventoryFolder)
                        {
                            _node = new TreeNode();
                            _node.ForeColor = Color.FromKnownColor(KnownColor.ScrollBar);
                            _node.Name = key;
                            _node.Text = inv.Name;

                            InventoryFolder folder = inv as InventoryFolder;
                            _node.ImageIndex = _node.SelectedImageIndex = GetFolderIcon(folder, false);
                            _node.Tag = folder;

                            TreeNode load = _node.Nodes.Add(null, "(loading...)");
                            load.ForeColor = Color.FromKnownColor(KnownColor.ScrollBar);
                            load.ImageIndex = load.SelectedImageIndex = 52;
                        }
                        else
                        {
                            InventoryItem item = inv as InventoryItem;
                            PermissionMask mask = item.Permissions.OwnerMask;

                            string perm_str = string.Empty;
                            if ((mask & PermissionMask.Copy) == 0)
                                perm_str += " (no copy)";
                            if ((mask & PermissionMask.Modify) == 0)
                                perm_str += " (no modify)";
                            if ((mask & PermissionMask.Transfer) == 0)
                                perm_str += " (no transfer)";

                            _node = new TreeNode();
                            _node.Name = key;
                            _node.Text = inv.Name + perm_str;
                            _node.ForeColor = Color.FromKnownColor(KnownColor.ScrollBar);
                            _node.Tag = item;

                            if (inv is InventoryAnimation)
                                _node.ImageIndex = _node.SelectedImageIndex = 2;
                            else if (inv is InventoryGesture)
                                _node.ImageIndex = _node.SelectedImageIndex = 10;
                            else if (inv is InventoryNotecard)
                                _node.ImageIndex = _node.SelectedImageIndex = 24;
                            else if (inv is InventoryLandmark)
                                _node.ImageIndex = _node.SelectedImageIndex = 15;
                            else if (inv is InventoryCallingCard)
                                _node.ImageIndex = _node.SelectedImageIndex = 3;
                            else if (inv is InventoryLSL)
                                _node.ImageIndex = _node.SelectedImageIndex = 29;
                            else if(inv is InventoryMesh)
                                _node.ImageIndex = _node.SelectedImageIndex = 23;
                            else if (inv is InventoryObject)
                            {
                                InventoryObject obj = inv as InventoryObject;
                                if(obj.ItemFlags.HasFlag(InventoryItemFlags.ObjectHasMultipleItems))
                                    _node.ImageIndex = _node.SelectedImageIndex = 26;
                                else
                                    _node.ImageIndex = _node.SelectedImageIndex = 25;
                            }
                            else if (inv is InventorySound)
                                _node.ImageIndex = _node.SelectedImageIndex = 36;
                            else if (inv is InventoryTexture)
                                _node.ImageIndex = _node.SelectedImageIndex = 42;
                            else if (inv is InventorySnapshot)
                                _node.ImageIndex = _node.SelectedImageIndex = 34;
                            else if(inv is InventorySettings)
                            {
                                InventorySettings settings = inv as InventorySettings;
                                switch(settings.Type)
                                {
                                    case SettingType.DayCycle:
                                        _node.ImageIndex = _node.SelectedImageIndex = 49;
                                        break;
                                    case SettingType.Water:
                                        _node.ImageIndex = _node.SelectedImageIndex = 51;
                                        break;
                                    case SettingType.Sky:
                                    default:
                                        {
                                            _node.ImageIndex = _node.SelectedImageIndex = 50;
                                            break;
                                        }
                                }
                            }
                            else if (inv is InventoryWearable)
                            {
                                InventoryWearable wearable = inv as InventoryWearable;
                                switch (wearable.WearableType)
                                {
                                    case WearableType.Alpha:
                                        _node.ImageIndex = _node.SelectedImageIndex = 0;
                                        break;
                                    case WearableType.Eyes:
                                        _node.ImageIndex = _node.SelectedImageIndex = 5;
                                        break;
                                    case WearableType.Gloves:
                                        _node.ImageIndex = _node.SelectedImageIndex = 11;
                                        break;
                                    case WearableType.Hair:
                                        _node.ImageIndex = _node.SelectedImageIndex = 12;
                                        break;
                                    case WearableType.Jacket:
                                        _node.ImageIndex = _node.SelectedImageIndex = 14;
                                        break;
                                    case WearableType.Pants:
                                        _node.ImageIndex = _node.SelectedImageIndex = 27;
                                        break;
                                    case WearableType.Physics:
                                        _node.ImageIndex = _node.SelectedImageIndex = 28;
                                        break;
                                    case WearableType.Shape:
                                        _node.ImageIndex = _node.SelectedImageIndex = 2;
                                        break;
                                    case WearableType.Shirt:
                                        _node.ImageIndex = _node.SelectedImageIndex = 30;
                                        break;
                                    case WearableType.Shoes:
                                        _node.ImageIndex = _node.SelectedImageIndex = 31;
                                        break;
                                    case WearableType.Skin:
                                        _node.ImageIndex = _node.SelectedImageIndex = 32;
                                        break;
                                    case WearableType.Skirt:
                                        _node.ImageIndex = _node.SelectedImageIndex = 30;
                                        break;
                                    case WearableType.Socks:
                                        _node.ImageIndex = _node.SelectedImageIndex = 35;
                                        break;
                                    case WearableType.Tattoo:
                                        _node.ImageIndex = _node.SelectedImageIndex = 41;
                                        break;
                                    case WearableType.Underpants:
                                        _node.ImageIndex = _node.SelectedImageIndex = 45;
                                        break;
                                    case WearableType.Undershirt:
                                        _node.ImageIndex = _node.SelectedImageIndex = 46;
                                        break;
                                    default:
                                        _node.ImageIndex = _node.SelectedImageIndex = 13;
                                        break;
                                }
                            }
                            else
                                _node.ImageIndex = _node.SelectedImageIndex = 52;
                        }

                        nodes.Add(_node);
                    }

                    nodes.Sort(SortNodes);
                    children.AddRange(nodes.ToArray());
                }

                this.EndUpdate();
                this.ResumeLayout();
            }
        }

        private int SortNodes(TreeNode tx, TreeNode ty)
        {
            if (tx.Tag is InventoryFolder && ty.Tag is InventoryFolder)
            {
                InventoryFolder fa = (InventoryFolder)tx.Tag;
                InventoryFolder fb = (InventoryFolder)ty.Tag;

                if (fa.PreferredType != FolderType.None && fb.PreferredType == FolderType.None)
                    return -1;
                else if (fa.PreferredType == FolderType.None && fb.PreferredType != FolderType.None)
                    return 1;
                else return string.Compare(fa.Name, fb.Name);
            }
            else if (tx.Tag is InventoryItem && ty.Tag is InventoryItem)
            {
                return -DateTime.Compare(((InventoryItem)tx.Tag).CreationDate, ((InventoryItem)ty.Tag).CreationDate);
            }
            else if (tx.Tag is InventoryFolder && ty.Tag is InventoryItem)
                return -1;
            else if (ty.Tag is InventoryFolder && tx.Tag is InventoryItem)
                return 1;
            else return string.Compare(tx.Name, ty.Name);
        }

        private void InitializeClient(ProxyFrame frame)
        {
            _Frame = frame;

            _Frame.Inventory.FolderUpdated += Inventory_OnFolderUpdated;
            _Frame.Login.AddLoginResponseDelegate(new XmlRpcResponseDelegate(OnLoginResponse));
        }

        private void OnLoginResponse(XmlRpcResponse response)
        {
            System.Collections.Hashtable values = (System.Collections.Hashtable)response.Value;
            if (!values.Contains("agent_id") || !values.Contains("session_id") || !values.Contains("secure_session_id"))
                return;

            if (values.Contains("inventory-root"))
            {
                var inventoryRoot = new UUID((string)((System.Collections.Hashtable)(((System.Collections.ArrayList)values["inventory-root"])[0]))["folder_id"]);
                var _node = this.Nodes.Add(inventoryRoot.ToString(), "Inventory");

                _node.ForeColor = Color.FromKnownColor(KnownColor.ScrollBar);

                InventoryFolder folder = _Frame.Inventory.Store.RootFolder;
                _node.ImageIndex = _node.SelectedImageIndex = GetFolderIcon(folder, false);
                _node.Tag = folder;

                UpdateFolder(inventoryRoot);
            }
        }

        private void Inventory_OnFolderUpdated(object sender, FolderUpdatedEventArgs e)
        {
            UpdateFolder(e.FolderID);
        }


        private Dictionary<string, HandleInventory> allTypeOptions = new Dictionary<string, HandleInventory>();
        private Dictionary<string, HandleInventoryFolder> folderOptions = new Dictionary<string, HandleInventoryFolder>();

        private Dictionary<InventoryType, Dictionary<string, HandleInventory>> invTypeOptions = new Dictionary<InventoryType, Dictionary<string, HandleInventory>>();

        private Dictionary<AssetType, Dictionary<string, HandleInventory>> assetTypeOptions = new Dictionary<AssetType, Dictionary<string, HandleInventory>>();


        void InventoryTree_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                _SelectedItemID = new UUID(e.Node.Name);
                this.SelectedNode = e.Node;

                if (_SelectedItemID.Equals(UUID.Zero))
                    return;

                InventoryBase entry = Frame.Inventory.Store[_SelectedItemID];
                
                _ContextMenu.Items.Clear();
                
                if(entry is InventoryItem)
                {
                    InventoryItem item = entry as InventoryItem;

                    foreach (var pair in allTypeOptions)
                    {
                        _ContextMenu.Items.Add(pair.Key, null, (x, y) => pair.Value(item));
                    }

                    Dictionary<string, HandleInventory> dict = null;
                    if (invTypeOptions.TryGetValue(item.InventoryType, out dict))
                    {
                        _ContextMenu.Items.Add("-");
                        foreach (var pair in dict)
                        {
                            _ContextMenu.Items.Add(pair.Key, null, (x, y) => pair.Value(item));
                        }
                    }

                    if (assetTypeOptions.TryGetValue(item.AssetType, out dict))
                    {
                        _ContextMenu.Items.Add("-");
                        foreach (var pair in dict)
                        {
                            _ContextMenu.Items.Add(pair.Key, null, (x, y) => pair.Value(item));
                        }
                    }
                }
                else if(entry is InventoryFolder)
                {
                    InventoryFolder folder = entry as InventoryFolder;
                    foreach (var pair in folderOptions)
                    {
                        _ContextMenu.Items.Add(pair.Key, null, (x, y) => pair.Value(folder));
                    }
                }
                
                _ContextMenu.Show(this, e.Location);
            }
        }

        private void InventoryTree_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            InventoryFolder folder = (InventoryFolder)Frame.Inventory.Store[new UUID(e.Node.Name)];
            Frame.Inventory.RequestFolderContents(folder.UUID, _Frame.Agent.AgentID, true, true, InventorySortOrder.ByDate | InventorySortOrder.FoldersByName | InventorySortOrder.SystemFoldersToTop);
        }

        internal void AddInventoryItemOption(string label, HandleInventory handle, AssetType assetType)
        {
            if (!assetTypeOptions.ContainsKey(assetType))
                assetTypeOptions.Add(assetType, new Dictionary<string, HandleInventory>());

            assetTypeOptions[assetType].Add(label, handle);
        }

        internal void AddInventoryItemOption(string label, HandleInventory handle, InventoryType invType)
        {
            if (!invTypeOptions.ContainsKey(invType))
                invTypeOptions.Add(invType, new Dictionary<string, HandleInventory>());

            invTypeOptions[invType].Add(label, handle);
        }

        internal void AddInventoryItemOption(string label, HandleInventory handle)
        {
            allTypeOptions.Add(label, handle);
        }

        internal void AddInventoryFolderOption(string label, HandleInventoryFolder handle)
        {
            folderOptions.Add(label, handle);
        }
    }

}

