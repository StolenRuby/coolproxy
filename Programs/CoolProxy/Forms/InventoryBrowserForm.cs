﻿using CoolGUI.Controls;
using OpenMetaverse;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CoolProxy
{
    public partial class InventoryBrowserForm : Form
    {
        public InventoryBrowserForm()
        {
            InitializeComponent();
            inventoryBrowser.Proxy = CoolProxy.Frame;

            this.TopMost = CoolProxy.Settings.getBool("KeepCoolProxyOnTop");
            CoolProxy.Settings.getSetting("KeepCoolProxyOnTop").OnChanged += (x, y) => { this.TopMost = (bool)y.Value; };

            comboBox1.Items.Add("All Types");
            var types = Enum.GetValues(typeof(InventoryType)).Cast<InventoryType>().Select(x => x.ToString()).Distinct().ToList();
            types.Remove("Unknown");
            types.Remove("Folder");
            types.Remove("RootCategory");
            types.Remove("Attachment");
            comboBox1.Items.AddRange(types.ToArray());
            comboBox1.SelectedIndex = 0;

            comboBox1.SelectedIndexChanged += ComboBox1_SelectedIndexChanged;

            inventoryBrowser.NodeAdded += InventoryBrowser_NodeAdded;
        }

        private void ComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            string type = comboBox1.SelectedItem.ToString();
            if (!Enum.TryParse(type, out InventoryType etype))
            {
                etype = InventoryType.Unknown;
            }
            inventoryBrowser.SetTypeFilter(etype);
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            inventoryBrowser.SetNameFilter(textBox1.Text);
        }

        private void InventoryBrowserForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }

        internal void AddInventoryItemOption(string label, HandleInventory handle, InventoryType invType, EnableInventory enable = null)
        {
            inventoryBrowser.AddInventoryItemOption(label, handle, invType, enable);
        }

        internal void AddInventoryItemOption(string label, HandleInventory handle, AssetType assetType, EnableInventory enable = null)
        {
            inventoryBrowser.AddInventoryItemOption(label, handle, assetType, enable);
        }

        internal void AddInventoryItemOption(string label, HandleInventory handle, EnableInventory enable = null)
        {
            inventoryBrowser.AddInventoryItemOption(label, handle, enable);
        }

        internal void AddInventoryFolderOption(string label, HandleInventoryFolder handle, EnableInventoryFolder enable = null)
        {
            inventoryBrowser.AddInventoryFolderOption(label, handle, enable);
        }

        private UUID ExpectedUUIDForSelection = UUID.Zero;

        private void InventoryBrowser_NodeAdded(FakeInvNode node)
        {
            if (node.ID == ExpectedUUIDForSelection)
            {
                inventoryBrowser.SelectByUUID(node.ID);
                ExpectedUUIDForSelection = UUID.Zero;
            }
        }

        private void newFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            inventoryBrowser.SelectByUUID(CoolProxy.Frame.Inventory.CreateFolder(CoolProxy.Frame.Inventory.InventoryRoot, "New Folder"));
        }

        private void CreateNewItem(string name, AssetType asset_type, InventoryType inventory_type)
        {
            UUID folder_id = CoolProxy.Frame.Inventory.FindFolderForType(asset_type);
            CoolProxy.Frame.Inventory.RequestCreateItem(folder_id, name, string.Empty, asset_type, UUID.Zero, inventory_type, PermissionMask.All, SelectNewItem);
        }

        private void newScriptToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CreateNewItem("New Script", AssetType.LSLText, InventoryType.LSL);
        }

        private void newNotecardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CreateNewItem("New Note", AssetType.Notecard, InventoryType.Notecard);
        }

        private void newGestureToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CreateNewItem("New Gesture", AssetType.Gesture, InventoryType.Gesture);
        }

        private void SelectNewItem(bool success, InventoryItem item)
        {
            if(success)
            {
                ExpectedUUIDForSelection = item.UUID;
                CoolProxy.Frame.Inventory.InjectFetchInventoryReply(item);
            }
        }

        private void expandAllFoldersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            inventoryBrowser.ExpandAll();
        }

        private void collapseAllFoldersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            inventoryBrowser.CollapseAll();
        }

        private void emptyTrashToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CoolProxy.Frame.Inventory.EmptyTrash();
        }
    }
}
