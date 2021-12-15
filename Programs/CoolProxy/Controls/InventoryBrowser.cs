using CoolProxy;
using GridProxy;
using Nwc.XmlRpc;
using OpenMetaverse;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CoolGUI.Controls
{
    public delegate void HandleInventory(InventoryItem inventoryItem);
    public delegate bool EnableInventory(InventoryItem inventoryItem);
    public delegate void HandleInventoryFolder(InventoryFolder inventoryFolder);
    public delegate bool EnableInventoryFolder(InventoryFolder inventoryFolder);

    public delegate void NodeAddedEvent(FakeInvNode node);

    public partial class InventoryBrowser : UserControl
    {
        private FakeInvFolder RootFolder
        {
            get { return innerInventoryBrowserControl1.Folder; }
            set { innerInventoryBrowserControl1.Folder = value; }
        }

        private ProxyFrame _Proxy;

        [Browsable(false)]
        public ProxyFrame Proxy
        {
            get { return _Proxy; }
            set { if (value != null) InitializeClient(value); }
        }

        [Browsable(true)]
        [Category("Appearance")]
        public ImageList ImageList
        {
            get { return innerInventoryBrowserControl1.ImageList; }
            set { innerInventoryBrowserControl1.ImageList = value; }
        }

        [Browsable(true)]
        [Category("Appearance")]
        public Color SelectionColor
        {
            get
            {
                return innerInventoryBrowserControl1.SelectionBrush.Color;
            }
            set
            {
                innerInventoryBrowserControl1.SelectionBrush = new SolidBrush(value);
            }
        }

        public event NodeAddedEvent NodeAdded;

        private ContextMenuStrip contextMenuStrip;

        public InventoryBrowser()
        {
            InitializeComponent();

            contextMenuStrip = new ContextMenuStrip();
            contextMenuStrip.ShowImageMargin = false;

            innerInventoryBrowserControl1.OnMenu += InnerInventoryBrowserControl1_OnMenu;

            this.Resize += (x, y) => innerInventoryBrowserControl1.Refresh();
        }

        private void InitializeClient(ProxyFrame frame)
        {
            _Proxy = frame;
            _Proxy.Login.AddLoginResponseDelegate(onLoginResponse);
        }

        // this override stops the control scrolling back to the top every time it changes focus
        protected override Point ScrollToControl(Control activeControl)
        {
            return this.AutoScrollPosition;
        }

        private void onLoginResponse(XmlRpcResponse response)
        {
            System.Collections.Hashtable values = (System.Collections.Hashtable)response.Value;
            if (!values.Contains("agent_id") || !values.Contains("session_id") || !values.Contains("secure_session_id"))
                return;

            if (values.Contains("inventory-root"))
            {
                InventoryFolder folder = _Proxy.Inventory.Store.RootFolder;

                RootFolder = CreateFolder(folder);
                RootFolder.Expanded = true;

                LoadStore(folder.UUID, RootFolder);

                Proxy.Inventory.Store.InventoryObjectAdded += Store_InventoryObjectAdded;
                Proxy.Inventory.Store.InventoryObjectUpdated += Store_InventoryObjectUpdated;
                Proxy.Inventory.Store.InventoryObjectRemoved += Store_InventoryObjectRemoved;
            }
        }

        private void LoadStore(UUID folderID, FakeInvFolder parent)
        {
            if (this.InvokeRequired) this.BeginInvoke((MethodInvoker)delegate { LoadStore(folderID, parent); });
            else
            {
                List<InventoryBase> contents = Proxy.Inventory.Store.GetContents(folderID);
                if (contents.Count != 0)
                {
                    foreach (InventoryBase inv in contents)
                    {
                        if (inv is InventoryFolder)
                        {
                            var folder = CreateFolder(inv as InventoryFolder);
                            parent.Children.Add(folder);
                            LoadStore(inv.UUID, folder);
                        }
                        else
                        {
                            var item = CreateItem(inv as InventoryItem);
                            parent.Children.Add(item);
                        }
                    }

                    parent.Children.Sort(SortTest);
                }
            }
        }

        public bool SelectByUUID(UUID uuid)
        {
            var find = RootFolder.Find(uuid, true);
            if (find != null)
            {
                innerInventoryBrowserControl1.SelectedNodes.ForEach(x => x.Selected = false);
                innerInventoryBrowserControl1.SelectedNodes.Clear();
                innerInventoryBrowserControl1.SelectedNodes.Add(find);

                find.Selected = true;

                var folder = find.Parent;
                while (folder != null)
                {
                    folder.Expanded = true;
                    folder = folder.Parent;
                }

                Refresh();

                return true;
            }

            return false;
        }

        #region Context Menu

        struct FolderOptionPair
        {
            public HandleInventoryFolder Handle;
            public EnableInventoryFolder Enable;
        }

        struct ItemOptionPair
        {
            public HandleInventory Handle;
            public EnableInventory Enable;
        }

        private Dictionary<string, ItemOptionPair> allTypeOptions = new Dictionary<string, ItemOptionPair>();
        private Dictionary<string, FolderOptionPair> folderOptions = new Dictionary<string, FolderOptionPair>();
        private Dictionary<InventoryType, Dictionary<string, ItemOptionPair>> invTypeOptions = new Dictionary<InventoryType, Dictionary<string, ItemOptionPair>>();
        private Dictionary<AssetType, Dictionary<string, ItemOptionPair>> assetTypeOptions = new Dictionary<AssetType, Dictionary<string, ItemOptionPair>>();

        private void InnerInventoryBrowserControl1_OnMenu(List<InventoryBase> selection)
        {
            contextMenuStrip.Items.Clear();
            
            if(selection.Count == 1)
            {
                var entry = selection[0];
                if (entry is InventoryItem)
                {
                    InventoryItem item = entry as InventoryItem;

                    foreach (var pair in allTypeOptions)
                    {
                        var add = contextMenuStrip.Items.Add(pair.Key, null, (x, y) => pair.Value.Handle(item));
                        add.Enabled = pair.Value.Enable?.Invoke(item) ?? true;
                    }

                    Dictionary<string, ItemOptionPair> dict = null;
                    if (invTypeOptions.TryGetValue(item.InventoryType, out dict))
                    {
                        contextMenuStrip.Items.Add("-");
                        foreach (var pair in dict)
                        {
                            var add = contextMenuStrip.Items.Add(pair.Key, null, (x, y) => pair.Value.Handle(item));
                            add.Enabled = pair.Value.Enable?.Invoke(item) ?? true;
                        }
                    }

                    if (assetTypeOptions.TryGetValue(item.AssetType, out dict))
                    {
                        contextMenuStrip.Items.Add("-");
                        foreach (var pair in dict)
                        {
                            var add = contextMenuStrip.Items.Add(pair.Key, null, (x, y) => pair.Value.Handle(item));
                            add.Enabled = pair.Value.Enable?.Invoke(item) ?? true;
                        }
                    }
                }
                else if (entry is InventoryFolder)
                {
                    InventoryFolder folder = entry as InventoryFolder;
                    foreach (var pair in folderOptions)
                    {
                        var item = contextMenuStrip.Items.Add(pair.Key, null, (x, y) => pair.Value.Handle(folder));
                        item.Enabled = pair.Value.Enable?.Invoke(folder) ?? true;
                    }
                }
            }

            contextMenuStrip.Show(this, this.PointToClient(Cursor.Position));
        }

        internal void AddInventoryItemOption(string label, HandleInventory handle, AssetType assetType, EnableInventory enable = null)
        {
            if (!assetTypeOptions.ContainsKey(assetType))
                assetTypeOptions.Add(assetType, new Dictionary<string, ItemOptionPair>());

            assetTypeOptions[assetType].Add(label, new ItemOptionPair()
            {
                Handle = handle,
                Enable = enable
            });
        }

        internal void AddInventoryItemOption(string label, HandleInventory handle, InventoryType invType, EnableInventory enable = null)
        {
            if (!invTypeOptions.ContainsKey(invType))
                invTypeOptions.Add(invType, new Dictionary<string, ItemOptionPair>());

            invTypeOptions[invType].Add(label, new ItemOptionPair()
            {
                Handle = handle,
                Enable = enable
            });
        }

        internal void AddInventoryItemOption(string label, HandleInventory handle, EnableInventory enable = null)
        {
            allTypeOptions.Add(label, new ItemOptionPair()
            {
                Handle = handle,
                Enable = enable
            });
        }

        internal void AddInventoryFolderOption(string label, HandleInventoryFolder handle, EnableInventoryFolder enable = null)
        {
            folderOptions.Add(label, new FolderOptionPair()
            {
                Handle = handle,
                Enable = enable
            });
        }

        #endregion

        #region Node Creation

        FakeInvFolder CreateFolder(InventoryFolder folder)
        {
            FakeInvFolder node = new FakeInvFolder();
            node.ID = folder.UUID;
            node.Name = folder.Name;
            node.Item = folder;
            node.IconIndex = GetFolderIcon(folder, false);
            node.OpenIconIndex = GetFolderIcon(folder, true);
            return node;
        }

        FakeInvItem CreateItem(InventoryItem item)
        {
            FakeInvItem node = new FakeInvItem();
            node.ID = item.UUID;
            node.Name = item.Name;
            node.Item = item;
            node.PermString = PermStringQuick(item);
            node.IconIndex = GetItemIcon(item);
            return node;
        }

        private int GetFolderIcon(InventoryFolder folder, bool open)
        {
            if (folder.Name == "#Firestorm" && folder.ParentUUID == Proxy.Inventory.InventoryRoot) return open ? 9 : 8;
            if (folder.Name == "Animation Overrides" && folder.ParentUUID == Proxy.Inventory.InventoryRoot) return open ? 40 : 39;

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

        private int GetItemIcon(InventoryBase inv)
        {
            if (inv is InventoryAnimation)
                return 2;
            else if (inv is InventoryGesture)
                return 10;
            else if (inv is InventoryNotecard)
                return 24;
            else if (inv is InventoryLandmark)
                return 15;
            else if (inv is InventoryCallingCard)
                return 3;
            else if (inv is InventoryLSL)
                return 29;
            else if (inv is InventoryMesh)
                return 23;
            else if (inv is InventoryObject)
            {
                InventoryObject obj = inv as InventoryObject;
                if (obj.ItemFlags.HasFlag(InventoryItemFlags.ObjectHasMultipleItems))
                    return 26;
                else
                    return 25;
            }
            else if (inv is InventorySound)
                return 36;
            else if (inv is InventoryTexture)
                return 42;
            else if (inv is InventorySnapshot)
                return 34;
            else if (inv is InventorySettings)
            {
                InventorySettings settings = inv as InventorySettings;
                switch (settings.Type)
                {
                    case SettingType.DayCycle:
                        return 49;
                    case SettingType.Water:
                        return 51;
                    case SettingType.Sky:
                    default:
                        {
                            return 50;
                        }
                }
            }
            else if (inv is InventoryWearable)
            {
                InventoryWearable wearable = inv as InventoryWearable;
                switch (wearable.WearableType)
                {
                    case WearableType.Alpha:
                        return 0;
                    case WearableType.Eyes:
                        return 5;
                    case WearableType.Gloves:
                        return 11;
                    case WearableType.Hair:
                        return 12;
                    case WearableType.Jacket:
                        return 14;
                    case WearableType.Pants:
                        return 27;
                    case WearableType.Physics:
                        return 28;
                    case WearableType.Shape:
                        return 2;
                    case WearableType.Shirt:
                        return 30;
                    case WearableType.Shoes:
                        return 31;
                    case WearableType.Skin:
                        return 32;
                    case WearableType.Skirt:
                        return 30;
                    case WearableType.Socks:
                        return 35;
                    case WearableType.Tattoo:
                        return 41;
                    case WearableType.Underpants:
                        return 45;
                    case WearableType.Undershirt:
                        return 46;
                    default:
                        return 13;
                }
            }

            return 1337;
        }

        private string PermStringQuick(InventoryItem item)
        {
            PermissionMask mask = item.Permissions.OwnerMask;

            string perm_str = string.Empty;
            if (!(item is InventoryCallingCard))
            {
                if ((mask & PermissionMask.Copy) == 0)
                    perm_str += " (no copy)";
                if ((mask & PermissionMask.Modify) == 0)
                    perm_str += " (no modify)";
                if ((mask & PermissionMask.Transfer) == 0)
                    perm_str += " (no transfer)";
            }

            return perm_str;
        }

        #endregion

        private void Store_InventoryObjectAdded(object sender, GridProxy.InventoryObjectAddedEventArgs e)
        {
            AddItem(e.Obj);
        }

        private void Store_InventoryObjectRemoved(object sender, GridProxy.InventoryObjectRemovedEventArgs e)
        {
            RemoveItem(e.Obj);
        }

        private void Store_InventoryObjectUpdated(object sender, GridProxy.InventoryObjectUpdatedEventArgs e)
        {
            UpdateItem(e.OldObject, e.NewObject);
        }

        private void AddItem(InventoryBase item)
        {
            if (this.InvokeRequired) this.BeginInvoke((MethodInvoker)delegate { AddItem(item); });
            else
            {
                var parent = RootFolder.Find(item.ParentUUID, true);
                if (parent == null)
                {
                    OpenMetaverse.Logger.Log("Received update for unknown TreeView node " + item.ParentUUID, Helpers.LogLevel.Warning);
                    return;
                }

                if(parent is FakeInvFolder)
                {
                    FakeInvNode node = null;
                    if(item is InventoryFolder)
                    {
                        node = CreateFolder(item as InventoryFolder);
                    }
                    else
                    {
                        node = CreateItem(item as InventoryItem);
                    }

                    var folder = parent as FakeInvFolder;

                    folder.Children.Add(node);
                    folder.Children.Sort(SortTest);

                    NodeAdded?.Invoke(node);

                    if (folder.Expanded) Refresh();
                }
            }
        }

        private void UpdateItem(InventoryBase old, InventoryBase item)
        {
            if (this.InvokeRequired) this.BeginInvoke((MethodInvoker)delegate { UpdateItem(old, item); });
            else
            {
                var node = RootFolder.Find(item.UUID, true);
                if (node == null)
                {
                    OpenMetaverse.Logger.Log("Received update for unknown TreeView node " + item.ParentUUID, Helpers.LogLevel.Warning);
                    return;
                }

                if (node.Parent.ID != item.ParentUUID)
                {
                    var parent = RootFolder.Find(item.ParentUUID, true);
                    if (parent != null)
                    {
                        node.Parent.Children.Remove(node);

                        if (parent is FakeInvFolder)
                        {
                            (parent as FakeInvFolder).Children.Add(node);
                        }
                        else
                        {
                            (parent as FakeInvItem).PermString = PermStringQuick(item as InventoryItem);
                        }
                    }
                }

                node.Name = item.Name;

                node.Parent.Children.Sort(SortTest);

                if (node.Parent.Expanded) Refresh();
            }
        }

        private void RemoveItem(InventoryBase item)
        {
            if (this.InvokeRequired) this.BeginInvoke((MethodInvoker)delegate { RemoveItem(item); });
            else
            {
                var found = RootFolder.Find(item.UUID, true);
                if (found == null)
                {
                    OpenMetaverse.Logger.Log("Received update for unknown TreeView node " + item.ParentUUID, Helpers.LogLevel.Warning);
                    return;
                }

                found.Parent.Children.Remove(found);
                
                Refresh();
            }
        }

        #region Sorting and Filtering

        private int SortTest(FakeInvNode tx, FakeInvNode ty)
        {
            if (tx == null) return 0;

            if (tx.Item is InventoryFolder && ty.Item is InventoryFolder)
            {
                InventoryFolder fa = (InventoryFolder)tx.Item;
                InventoryFolder fb = (InventoryFolder)ty.Item;

                if (fa.PreferredType != FolderType.None && fb.PreferredType == FolderType.None)
                    return -1;
                else if (fa.PreferredType == FolderType.None && fb.PreferredType != FolderType.None)
                    return 1;
                else return string.Compare(fa.Name, fb.Name);
            }
            else if (tx.Item is InventoryItem && ty.Item is InventoryItem)
            {
                return -DateTime.Compare(((InventoryItem)tx.Item).CreationDate, ((InventoryItem)ty.Item).CreationDate);
            }
            else if (tx.Item is InventoryFolder && ty.Item is InventoryItem)
                return -1;
            else if (ty.Item is InventoryFolder && tx.Item is InventoryItem)
                return 1;
            else return string.Compare(tx.Name, ty.Name);
        }

        public void SetTypeFilter(InventoryType type)
        {
            innerInventoryBrowserControl1.SetTypeFilter(type);
        }

        public void SetNameFilter(string find)
        {
            innerInventoryBrowserControl1.SetNameFilter(find);
        }

        public void ExpandAll()
        {
            innerInventoryBrowserControl1.ExpandAll();
        }

        public void CollapseAll()
        {
            innerInventoryBrowserControl1.CollapseAll();
        }

        #endregion

        #region Inner Inventory

        internal class InnerInventoryBrowserControl : UserControl
        {
            private FakeInvFolder _Folder = null;

            public FakeInvFolder Folder
            {
                get { return _Folder; }
                set
                {
                    _Folder = value;
                    Refresh();
                }
            }

            public ImageList ImageList { get; set; } = null;

            public InnerInventoryBrowserControl()
            {
                this.Paint += InventoryBrowserControl_Paint;

                this.MouseClick += InventoryBrowserControl_MouseClick;
                this.DoubleClick += InventoryBrowserControl_DoubleClick;
                this.MouseMove += InventoryBrowserControl_MouseMove;

                this.DoubleBuffered = true;

                NodeHeight = this.Font.Height + 2;
                this.FontChanged += (x, y) => NodeHeight = this.Font.Height + 2;
            }

            private void InventoryBrowserControl_MouseMove(object sender, MouseEventArgs e)
            {
                int cursor_y = this.PointToClient(Cursor.Position).Y;
                int n = cursor_y < this.Height - 1 ? cursor_y / NodeHeight : -1;

                if (n != OverIndex)
                {
                    OverIndex = n;
                    Refresh();
                }
            }

            private void InventoryBrowserControl_DoubleClick(object sender, EventArgs e)
            {
                if (this.SelectedNodes.Count < 1) return;

                if (this.SelectedNodes[0] is FakeInvFolder)
                {
                    var folder = this.SelectedNodes[0] as FakeInvFolder;
                    folder.Expanded = !folder.Expanded;
                    Refresh();
                }
            }

            private int NodeHeight = 20;

            public List<FakeInvNode> SelectedNodes { get; } = new List<FakeInvNode>();

            private void InventoryBrowserControl_MouseClick(object sender, MouseEventArgs e)
            {
                int y = e.Y;
                int n = y / NodeHeight;
                var node = GetClicked(n);

                bool replace_selection = false;
                bool show_menu = false;

                if (e.Button == MouseButtons.Left)
                {
                    if (Control.ModifierKeys == Keys.Control)
                    {
                        if (SelectedNodes.Contains(node))
                        {
                            SelectedNodes.Remove(node);
                            node.Selected = false;
                        }
                        else
                        {
                            SelectedNodes.Add(node);
                            node.Selected = true;
                        }
                    }
                    else if (Control.ModifierKeys == Keys.Shift)
                    {
                        if (SelectedNodes.Count > 0)
                        {
                            var previous = SelectedNodes.Last();
                            if (previous.Parent == node.Parent)
                            {
                                var index_a = previous.Parent.Children.IndexOf(node);
                                var index_b = previous.Parent.Children.IndexOf(previous);

                                if (index_a > index_b)
                                {
                                    int swap = index_a;
                                    index_a = index_b;
                                    index_b = swap;
                                }

                                for (int i = index_a; i <= index_b; i++)
                                {
                                    var child = previous.Parent.Children[i];
                                    if (!SelectedNodes.Contains(child))
                                        SelectedNodes.Add(child);
                                    previous.Parent.Children[i].Selected = true;
                                }
                            }
                        }
                    }
                    else
                    {
                        replace_selection = true;
                    }
                }
                else if (e.Button == MouseButtons.Right)
                {
                    if (node == null) return;
                    replace_selection = !node.Selected;
                    show_menu = true;
                }

                if (replace_selection)
                {
                    SelectedNodes.ForEach(x => x.Selected = false);
                    SelectedNodes.Clear();
                    if (node != null)
                    {
                        SelectedNodes.Add(node);
                        node.Selected = true;
                    }
                }

                Refresh();

                if (show_menu && OnMenu != null)
                {
                    var items = SelectedNodes.Select(x => x.Item).ToList();
                    OnMenu?.Invoke(items);
                }
            }

            public delegate void CallInvMenu(List<InventoryBase> selection);

            public event CallInvMenu OnMenu;

            FakeInvNode GetClicked(int index)
            {
                int n = 0;
                return DigForIndex(Folder, ref n, index);
            }

            private FakeInvNode DigForIndex(FakeInvNode node, ref int n, int target)
            {
                if (n == target) return node;

                n++;

                if (node is FakeInvFolder)
                {
                    var folder = node as FakeInvFolder;
                    if (folder.Expanded)
                    {
                        foreach (var child in folder.Children)
                        {
                            if (!child.Visible) continue;

                            var r = DigForIndex(child, ref n, target);
                            if (r != null) return r;
                        }
                    }
                }

                return null;
            }

            int OverIndex = 0;

            private void InventoryBrowserControl_Paint(object sender, PaintEventArgs e)
            {
                if (Folder == null || !Folder.Visible) return;

                var a = -this.Location.Y;
                var b = this.Parent.Size.Height;

                float x = 0.0f;
                float y = 0.0f;
                float max_x = 0.0f;
                float max_y = 0.0f;

                DrawNode(Folder, e.Graphics, x, ref y, ref max_x, ref max_y, a - 10, a + b);

                int fake_min_width = (int)max_x + 10;

                //this.MinimumSize = new Size(250, (int)max_y + NodeHeight + 1);
                //this.Size = new Size(500, (int)max_y + NodeHeight + 1);
                this.Height = (int)max_y + NodeHeight + 1;

                bool scroll_visible = this.Height > this.Parent.Height;

                this.Width = this.Parent.Width <= fake_min_width ? fake_min_width : this.Parent.Width - (scroll_visible ? 17 : 0);

                e.Graphics.DrawRectangle(Pens.CornflowerBlue, 0, OverIndex * NodeHeight, this.Width, NodeHeight);
            }

            public SolidBrush SelectionBrush = (SolidBrush)Brushes.CadetBlue;

            private int DrawNode(FakeInvNode node, Graphics graphics, float x, ref float y, ref float max_x, ref float max_y, int hmin, int hmax)
            {
                int node_height = node.Visible ? NodeHeight : 0;

                if (y > hmin && y < hmax)
                {
                    if (node.Selected)
                    {
                        graphics.FillRectangle(SelectionBrush, 0, y, this.Width, NodeHeight);

                        if (SelectedNodes.Count > 0 && node == SelectedNodes[SelectedNodes.Count - 1])
                            graphics.DrawRectangle(Pens.CornflowerBlue, 0, y, this.Width, NodeHeight);
                    }

                    if (node.IconIndex != -1 && ImageList != null && ImageList.Images.Count > node.IconIndex)
                    {
                        if (node is FakeInvFolder)
                        {
                            var folder = node as FakeInvFolder;
                            graphics.DrawImage(ImageList.Images[folder.Expanded ? folder.OpenIconIndex : folder.IconIndex], x + 1, y + 1, NodeHeight - 2, NodeHeight - 2);
                        }
                        else
                        {
                            graphics.DrawImage(ImageList.Images[node.IconIndex], x + 1, y + 1, NodeHeight - 2, NodeHeight - 2);
                        }
                    }

                    graphics.DrawString(node.Name, Font, Brushes.LightGray, x + 15, y + 1);
                }

                var size = graphics.MeasureString(node.Name, Font);
                var t = x + 15 + size.Width;
                if (t > max_x)
                {
                    max_x = t;
                }

                if (node is FakeInvFolder)
                {
                    var folder = node as FakeInvFolder;
                    if (folder.Expanded)
                    {
                        float start = y;

                        foreach (var child in folder.Children)
                        {
                            if (!child.Visible) continue;
                            y += NodeHeight;

                            if (y > max_y)
                                max_y = y;

                            node_height += DrawNode(child, graphics, x + 10, ref y, ref max_x, ref max_y, hmin, hmax);
                        }

                        if (node.Selected)
                            graphics.DrawRectangle(Pens.CornflowerBlue, 0, start, this.Width - 1, node_height);
                    }
                }
                else
                {
                    if (y > hmin && y < hmax)
                    {
                        var perm_string = (node as FakeInvItem).PermString;

                        //size = graphics.MeasureString(node.Name, Font);
                        float n = x + 10 + size.Width;
                        graphics.DrawString(perm_string, Font, Brushes.DarkGray, n, y + 1);

                        size = graphics.MeasureString(perm_string, Font);
                        n += size.Width;
                        if (n > max_x)
                        {
                            max_x = n;
                        }
                    }
                }

                return node_height;
            }

            public void SetSelectedNode(FakeInvNode node)
            {
                var find = Folder?.Find(node.ID) ?? null;
                if (find != null)
                {
                    SelectedNodes.ForEach(x => x.Selected = false);
                    SelectedNodes.Clear();
                    SelectedNodes.Add(node);
                    node.Selected = true;

                    var parent = node.Parent;
                    while (parent != null)
                    {
                        parent.Expanded = true;
                        parent = parent.Parent;
                    }
                }
            }

            bool HasFilter = false;
            InventoryType TypeFilter = InventoryType.Unknown;
            string NameFilter = string.Empty;

            public void SetTypeFilter(InventoryType type)
            {
                TypeFilter = type;
                HasFilter = TypeFilter != InventoryType.Unknown || NameFilter.Length > 0;

                int count = UpdateFilter(Folder);
                Folder.Visible = HasFilter ? count > 0 || (NameFilter.Length > 0 ? Folder.Name.ToLower().Contains(NameFilter) : false) : true;
                Folder.Expanded = true;

                Refresh();
            }

            public void SetNameFilter(string find)
            {
                NameFilter = find.ToLower();
                HasFilter = TypeFilter != InventoryType.Unknown || NameFilter.Length > 0;

                int count = UpdateFilter(Folder);
                Folder.Visible = HasFilter ? count > 0 || (NameFilter.Length > 0 ? Folder.Name.ToLower().Contains(NameFilter) : false) : true;
                Folder.Expanded = true;

                Refresh();
            }

            private int UpdateFilter(FakeInvFolder folder)
            {
                int visible_children_count = 0;
                foreach (var child in folder.Children)
                {
                    if (child is FakeInvFolder)
                        visible_children_count += UpdateFilter(child as FakeInvFolder);
                    else
                    {
                        var item = child.Item as InventoryItem;
                        bool visible = true;

                        if (HasFilter)
                        {
                            if (TypeFilter != InventoryType.Unknown)
                                visible = item.InventoryType == TypeFilter && item.Name.ToLower().Contains(NameFilter);
                            else
                                visible = item.Name.ToLower().Contains(NameFilter);
                        }

                        child.Visible = visible;
                        if (child.Visible) visible_children_count++;
                    }
                }

                bool folder_vis = true;
                if (HasFilter)
                {
                    if (NameFilter.Length > 0)
                    {
                        folder_vis = folder.Name.ToLower().Contains(NameFilter) || visible_children_count > 0;
                    }
                    else
                    {
                        folder_vis = visible_children_count > 0;
                    }
                }

                if (folder_vis) visible_children_count++;

                folder.Visible = folder_vis;
                //folder.Expanded = HasFilter;

                return visible_children_count;
            }

            public void ExpandAll()
            {
                RecursiveExpand(Folder, true);
                Refresh();
            }

            public void CollapseAll()
            {
                RecursiveExpand(Folder, false);
                Refresh();
            }

            private void RecursiveExpand(FakeInvFolder folder, bool expand)
            {
                foreach (var child in folder.Children)
                {
                    if (child is FakeInvFolder)
                        RecursiveExpand(child as FakeInvFolder, expand);
                }

                folder.Expanded = expand;
            }
        }
        #endregion
    }

    #region Node Classes
    public class FakeInvNode
    {
        public string Name { get; set; } = string.Empty;

        public UUID ID { get; set; } = UUID.Zero;

        public int IconIndex { get; set; } = -1;

        public bool Visible { get; set; } = true;

        public bool Selected { get; set; } = false;

        public FakeInvFolder Parent { get; internal set; } = null;

        public InventoryBase Item { get; internal set; } = null;
    }

    public class FakeInvFolder : FakeInvNode
    {
        public ObservableCollection<FakeInvNode> Children { get; set; } = new ObservableCollection<FakeInvNode>();

        public bool Expanded { get; set; } = false;

        public int OpenIconIndex { get; set; } = -1;

        public FakeInvFolder()
        {
            Children.CollectionChanged += Children_CollectionChanged;
        }

        private void Children_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (FakeInvNode item in e.NewItems)
                {
                    if (item.Parent != null)
                    {
                        item.Parent.Children.Remove(item);
                    }

                    item.Parent = this;
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (FakeInvNode item in e.OldItems)
                {
                    item.Parent = null;
                }
            }
        }

        public FakeInvNode Find(UUID id, bool search_children = false)
        {
            if (ID == id) return this;

            foreach (var child in Children)
            {
                if (child is FakeInvFolder)
                {
                    var folder = child as FakeInvFolder;
                    if (folder.ID == id) return folder;

                    if (search_children)
                    {
                        var ret = folder.Find(id, search_children);
                        if (ret != null) return ret;
                    }
                }
                else
                {
                    if (child.ID == id)
                        return child;
                }
            }

            return null;
        }
    }

    public class FakeInvItem : FakeInvNode
    {
        public string PermString { get; set; }
    }
    #endregion
}
