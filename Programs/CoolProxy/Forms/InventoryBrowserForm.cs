using CoolGUI.Controls;
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
            inventoryTree1.Frame = CoolProxy.Frame;

            this.TopMost = CoolProxy.Settings.getBool("KeepCoolProxyOnTop");
            CoolProxy.Settings.getSetting("KeepCoolProxyOnTop").OnChanged += (x, y) => { this.TopMost = (bool)y.Value; };
        }

        internal void InitOptions()
        {
            CoolProxy.GUI.AddInventoryFolderOption("Save As...", handleSaveFolderAs);
        }

        private void InventoryBrowserForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }

        internal void AddInventoryItemOption(string label, HandleInventory handle, InventoryType invType)
        {
            inventoryTree1.AddInventoryItemOption(label, handle, invType);
        }

        internal void AddInventoryItemOption(string label, HandleInventory handle, AssetType assetType)
        {
            inventoryTree1.AddInventoryItemOption(label, handle, assetType);
        }

        internal void AddInventoryItemOption(string label, HandleInventory handle)
        {
            inventoryTree1.AddInventoryItemOption(label, handle);
        }

        internal void AddInventoryFolderOption(string label, HandleInventoryFolder handle)
        {
            inventoryTree1.AddInventoryFolderOption(label, handle);
        }

        private void handleSaveFolderAs(InventoryFolder inventoryFolder)
        {
            InventoryBackupSettingsForm inventoryBackupSettingsForm = new InventoryBackupSettingsForm();

            inventoryBackupSettingsForm.StartPosition = FormStartPosition.Manual;
            inventoryBackupSettingsForm.Location = new Point(this.Location.X + this.Width, this.Location.Y);

            if (inventoryBackupSettingsForm.ShowDialog() == DialogResult.OK)
            {
            }
        }
    }
}
