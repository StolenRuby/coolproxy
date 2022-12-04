using OpenMetaverse;
using OpenMetaverse.StructuredData;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CoolProxy
{
    public partial class PreferencesForm
    {

        Dictionary<string, MenuOption> MainMenuOptions = new Dictionary<string, MenuOption>();

        MenuFolder MainMenu = new MenuFolder("MAIN_MENU", "Main Menu");

        List<string> KnownMenuNames = new List<string>();

        void AddMenuOption(MenuOption option)
        {
            if (MainMenuOptions.ContainsKey(option.Name))
            {
                OpenMetaverse.Logger.Log("MenuOption `" + option.Name + "` already exists!", Helpers.LogLevel.Warning);
                return;
            }

            MainMenuOptions.Add(option.Name, option);
        }

        private void AddMenuItems(List<MenuItem> items, ToolStripItemCollection dropDownItems)
        {
            foreach (var item in items)
            {
                if (item is MenuSeparator)
                {
                    dropDownItems.Add("-");
                }
                else if (item is MenuFolder)
                {
                    var folder = item as MenuFolder;

                    ToolStripMenuItem p = (ToolStripMenuItem)dropDownItems.Add(item.Label);
                    p.ForeColor = folder.Color;

                    AddMenuItems(folder.SubItems, p.DropDownItems);
                }
                else if (item is MenuOption)
                {
                    var opt = item as MenuOption;
                    ToolStripMenuItem e = (ToolStripMenuItem)dropDownItems.Add(opt.Label);
                    e.Tag = opt;

                    e.Click += (x, y) => opt.Clicked?.Invoke(opt.Tag);
                    //e.Checked = opt.Checked?.Invoke(e) ?? false;
                    e.ForeColor = item.Color;
                }
            }
        }

        #region treeview stuff
        private void treeView1_ItemDrag(object sender, ItemDragEventArgs e)
        {
            // Move the dragged node when the left mouse button is used.  
            if (e.Button == MouseButtons.Left)
            {
                DoDragDrop(e.Item, DragDropEffects.Move);
            }

            //// Copy the dragged node when the right mouse button is used.  
            //else if (e.Button == MouseButtons.Right)
            //{
            //    DoDragDrop(e.Item, DragDropEffects.Copy);
            //}
        }

        private void treeView1_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = e.AllowedEffect;
        }

        private void treeView1_DragOver(object sender, DragEventArgs e)
        {
            // Retrieve the client coordinates of the mouse position.  
            Point targetPoint = treeView1.PointToClient(new Point(e.X, e.Y));

            // Select the node at the mouse position.  
            treeView1.SelectedNode = treeView1.GetNodeAt(targetPoint);
        }

        private void treeView1_DragDrop(object sender, DragEventArgs e)
        {
            // Retrieve the client coordinates of the drop location.  
            Point targetPoint = treeView1.PointToClient(new Point(e.X, e.Y));

            // Retrieve the node at the drop location.  
            TreeNode targetNode = treeView1.GetNodeAt(targetPoint);

            // Retrieve the node that was dragged.  
            TreeNode draggedNode = (TreeNode)e.Data.GetData(typeof(TreeNode));

            // Confirm that the node at the drop location is not   
            // the dragged node or a descendant of the dragged node.  
            if (!draggedNode.Equals(targetNode) && !ContainsNode(draggedNode, targetNode))
            {
                // If it is a move operation, remove the node from its current   
                // location and add it to the node at the drop location.  
                if (e.Effect == DragDropEffects.Move)
                {
                    MenuItem item = draggedNode.Tag as MenuItem;
                    MenuFolder old_parent = draggedNode.Parent.Tag as MenuFolder;

                    draggedNode.Remove();

                    if (targetNode.Text.StartsWith("<"))
                    {
                        MenuFolder fol = targetNode.Tag as MenuFolder;
                        targetNode.Nodes.Add(draggedNode);
                        fol.SubItems.Add(item);

                        old_parent.SubItems.Remove(item);
                    }
                    else
                    {
                        MenuFolder fol = targetNode.Parent.Tag as MenuFolder;
                        targetNode.Parent.Nodes.Insert(targetNode.Index + 1, draggedNode);
                        old_parent.SubItems.Remove(item);
                        fol.SubItems.Insert(targetNode.Index + 1, draggedNode.Tag as MenuItem);
                    }
                }

                // If it is a copy operation, clone the dragged node   
                // and add it to the node at the drop location.  
                //else if (e.Effect == DragDropEffects.Copy)
                //{
                //    targetNode.Nodes.Add((TreeNode)draggedNode.Clone());
                //}

                // Expand the node at the location   
                // to show the dropped node.  
                targetNode.Expand();
            }
        }

        private bool ContainsNode(TreeNode node1, TreeNode node2)
        {
            // Check the parent node of the second node.  
            if (node2.Parent == null) return false;
            if (node2.Parent.Equals(node1)) return true;

            // If the parent node is not null or equal to the first node,   
            // call the ContainsNode method recursively using the parent of   
            // the second node.  
            return ContainsNode(node1, node2.Parent);
        }

        private void treeView1_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            treeView1.SelectedNode = e.Node;
        }
        #endregion

        #region menu editor options
        private void menuEditorMenu_Opening(object sender, CancelEventArgs e)
        {
            var node = treeView1.SelectedNode;
            if (node == null)
            {
                e.Cancel = true;
                return;
            }

            removeToolStripMenuItem.Visible = node.Parent != null;

            bool folder = node.Text.StartsWith("<");

            setColourToolStripMenuItem.Visible = (folder && node.Parent != null) || node.Tag is MenuOption;
            addFolderToolStripMenuItem.Visible = addOptionToolStripMenuItem.Visible = addSeparatorToolStripMenuItem.Visible = folder;
        }

        private void addSeparatorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var node = treeView1.SelectedNode;
            var add = node.Nodes.Insert(0, "-- Separator --");

            var folder = node.Tag as MenuFolder;
            var sep = new MenuSeparator();
            folder.SubItems.Insert(0, sep);
            add.Tag = sep;

            node.Expand();
        }

        private void removeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var node = treeView1.SelectedNode;
            var item = node.Tag as MenuItem;

            var parent = node.Parent.Tag as MenuFolder;
            parent.SubItems.Remove(item);
            treeView1.SelectedNode.Remove();
        }

        private void addOptionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var list = MainMenuOptions.Keys.ToArray();
            ListDialogBox listDialogBox = new ListDialogBox(list);
            listDialogBox.TopMost = this.TopMost;
            if (listDialogBox.ShowDialog() == DialogResult.OK)
            {
                var opt = MainMenuOptions[listDialogBox.SelectedOption];

                var node = treeView1.SelectedNode;

                var folder = node.Tag as MenuFolder;
                folder.SubItems.Insert(0, opt);

                var add = node.Nodes.Insert(0, opt.Name, opt.Label);
                add.Tag = opt;

                node.Expand();
            }
        }

        private void addFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TextBoxDialogBox textBoxDialogBox = new TextBoxDialogBox(string.Empty, "Enter a folder name");
            textBoxDialogBox.TopMost = this.TopMost;
            if (textBoxDialogBox.ShowDialog() == DialogResult.OK)
            {
                var node = treeView1.SelectedNode;

                MenuFolder folder = new MenuFolder("FOLDER_" + textBoxDialogBox.Result.Replace(" ", "_").ToUpper(), textBoxDialogBox.Result);
                var n = node.Nodes.Insert(0, folder.Name, "<" + folder.Label + ">");
                n.Tag = folder;

                MenuFolder parent = node.Tag as MenuFolder;
                parent.SubItems.Insert(0, folder);

                node.Expand();
            }
        }

        private void setColourToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ColorDialog colorDialog = new ColorDialog();
            if (colorDialog.ShowDialog() == DialogResult.OK)
            {
                treeView1.SelectedNode.ForeColor = colorDialog.Color;
                var item = treeView1.SelectedNode.Tag as MenuItem;
                item.Color = colorDialog.Color;
            }
        }
        #endregion

        private void resetMainMenuBtn_Click(object sender, EventArgs e)
        {
            KnownMenuNames.Clear();
            MainMenu.SubItems.Clear();

            AddMissingMenuOptions();
            RefreshMenuTree();
            ApplyMainMenu();
        }

        private void reloadMainMenuBtn_Click(object sender, EventArgs e)
        {
            LoadMainMenu();
            ApplyMainMenu();
        }

        void AddMissingMenuOptions()
        {
            foreach (var option in MainMenuOptions.Values)
            {
                if (!KnownMenuNames.Contains(option.Name))
                {
                    KnownMenuNames.Add(option.Name);

                    if (option.Default)
                    {
                        var folder = GetEntryFolder(option.DefaultPath);
                        folder.SubItems.Add(option);
                    }
                }
            }
        }

        private void ApplyMainMenu()
        {
            testMenu.Items.Clear();

            AddMenuItems(MainMenu.SubItems, testMenu.Items);

            testMenu.Items.Add("-");
            testMenu.Items.Add("Quit Cool Proxy").Click += quitCoolProxyToolStripMenuItem_Click;
        }

        private void LoadMainMenu()
        {
            MainMenu = new MenuFolder("MAIN_MENU", "Main Menu");

            string app_settings_dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "CoolProxy\\");
            string filename = Path.Combine(app_settings_dir, "main_menu.xml");

            if (File.Exists(filename))
            {
                byte[] xml = File.ReadAllBytes(filename);
                OSD osd = OSDParser.DeserializeLLSDXml(xml);
                OSDMap map = (OSDMap)osd;

                OSDArray known = (OSDArray)map["known"];
                OSDMap root = (OSDMap)map["root"];

                KnownMenuNames = known.Select(x => x.AsString()).ToList();
                MainMenu.SubItems = OSDToFolderContents((OSDArray)root["children"]);
            }

            AddMissingMenuOptions();

            RefreshMenuTree();
        }

        private void SaveMainMenu()
        {
            OSDMap menu = MenuItemToMap(MainMenu);

            OSDArray known = new OSDArray();
            foreach (var opt in KnownMenuNames)
            {
                known.Add(opt);
            }

            OSDMap map = new OSDMap();
            map["root"] = menu;
            map["known"] = known;

            string app_settings_dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "CoolProxy\\");
            string filename = Path.Combine(app_settings_dir, "main_menu.xml");

            byte[] xml = OSDParser.SerializeLLSDXmlToBytes((OSD)map);
            File.WriteAllBytes(filename, xml);
        }

        void RefreshMenuTree()
        {
            treeView1.Nodes.Clear();

            var a = treeView1.Nodes.Add(MainMenu.Name, "<" + MainMenu.Label + ">");
            a.Tag = MainMenu;
            a.ForeColor = MainMenu.Color;


            AddTreeNodeItems(MainMenu, a.Nodes);

            treeView1.ExpandAll();
        }

        private MenuFolder GetEntryFolder(string[] defaultPath)
        {
            if (defaultPath == null) return MainMenu;

            var top = MainMenu;

            foreach (var fol in defaultPath)
            {
                string id = "FOLDER_" + fol.Replace(" ", "_").ToUpper();

                var find = top.SubItems.Find(x => x.Name == id);
                if (find != null)
                {
                    top = find as MenuFolder;
                }
                else
                {
                    MenuFolder mf = new MenuFolder(id, fol);
                    top.SubItems.Add(mf);
                    top = mf;
                }
            }

            return top;
        }

        private List<MenuItem> OSDToFolderContents(OSDArray array)
        {
            List<MenuItem> items = new List<MenuItem>();

            foreach (OSDMap map in array)
            {
                string type = map["type"];

                if (type == "separator")
                {
                    items.Add(new MenuSeparator());
                    continue;
                }

                string name = map["name"];
                string label = map["label"];
                Color4 color4 = map["color"].AsColor4();

                Color color = Color.FromArgb((int)(255 * color4.R), (int)(255 * color4.G), (int)(255 * color4.B));

                if (type == "option")
                {
                    if (MainMenuOptions.TryGetValue(name, out MenuOption opt))
                    {
                        items.Add(opt);
                        opt.Color = color;
                    }
                }
                else if (type == "folder")
                {
                    MenuFolder folder = new MenuFolder(name, label);
                    folder.Color = color;
                    folder.SubItems = OSDToFolderContents((OSDArray)map["children"]);

                    items.Add(folder);
                }
            }

            return items;
        }

        private OSDMap MenuItemToMap(MenuItem item)
        {
            OSDMap map = new OSDMap();
            if (item is MenuSeparator)
            {
                map["type"] = "separator";
            }
            else
            {
                bool is_folder = item is MenuFolder;
                Color4 colour4 = new Color4(item.Color.R, item.Color.G, item.Color.B, item.Color.A);

                map["type"] = is_folder ? "folder" : "option";
                map["name"] = item.Name;
                map["label"] = item.Label;
                map["color"] = OSD.FromColor4(colour4);

                if (is_folder)
                {
                    var folder = item as MenuFolder;
                    OSDArray children = new OSDArray();
                    foreach (var child in folder.SubItems)
                    {
                        children.Add(MenuItemToMap(child));
                    }

                    map["children"] = children;
                }
            }

            return map;
        }

        private void AddTreeNodeItems(MenuFolder parent, TreeNodeCollection nodes)
        {
            foreach (var item in parent.SubItems)
            {
                if (item is MenuSeparator)
                {
                    var sep = nodes.Add("-- Separator --");
                    sep.Tag = item;

                }
                else if (item is MenuFolder)
                {
                    var fol = item as MenuFolder;

                    var add = nodes.Add(fol.Name, "<" + fol.Label + ">");
                    add.Tag = fol;
                    add.ForeColor = fol.Color;

                    AddTreeNodeItems(fol, add.Nodes);
                }
                else if (item is MenuItem)
                {
                    var add = nodes.Add(item.Label);
                    add.Tag = item;
                    add.ForeColor = item.Color;
                }
            }
        }

        private void testMenuButton_Click(object sender, EventArgs e)
        {
            ApplyMainMenu();
            testMenu.Show(Cursor.Position);
        }

        private void saveMenuChangesBtn_Click(object sender, EventArgs e)
        {
            SaveMainMenu();
        }

        private void testMenu_Opening(object sender, CancelEventArgs e)
        {
            foreach (ToolStripItem item in testMenu.Items)
            {
                if (item is ToolStripMenuItem)
                {
                    var tsmi = item as ToolStripMenuItem;

                    if (tsmi.Tag is MenuOption)
                    {
                        var opt = item.Tag as MenuOption;

                        tsmi.Checked = opt.Checked?.Invoke(opt.Tag) ?? false;
                        tsmi.Enabled = opt.Enabled?.Invoke(opt.Tag) ?? true;
                    }

                    RecursiveMenuOpening(tsmi.DropDownItems);
                }
            }
        }

        private void RecursiveMenuOpening(ToolStripItemCollection dropDownItems)
        {
            foreach (ToolStripItem ddi in dropDownItems)
            {
                if (ddi is ToolStripMenuItem)
                {
                    var item = ddi as ToolStripMenuItem;

                    if (item.Tag is MenuOption)
                    {
                        var opt = item.Tag as MenuOption;

                        item.Checked = opt.Checked?.Invoke(opt.Tag) ?? false;
                        item.Enabled = opt.Enabled?.Invoke(opt.Tag) ?? true;
                    }

                    RecursiveMenuOpening(item.DropDownItems);
                }
            }
        }
    }
}
