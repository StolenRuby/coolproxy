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

namespace CoolProxy.Plugins.Editors
{
    public partial class TextEditor : Form
    {
        private InventoryItem mItem;

        private CoolProxyFrame Proxy;

        public TextEditor(InventoryItem item, CoolProxyFrame frame)
        {
            mItem = item;
            Proxy = frame;

            InitializeComponent();

            this.Text = "Text Editor - " + item.Name;

            Proxy.OpenSim.Assets.DownloadAsset(item.AssetUUID, (success, data) =>
            {
                if (success)
                {
                    richTextBox1.Text = Utils.BytesToString(data);
                }
                else MessageBox.Show("Failed to download asset!");
            });

            this.TopMost = frame.Settings.getBool("KeepCoolProxyOnTop");
            frame.Settings.getSetting("KeepCoolProxyOnTop").OnChanged += (x, y) => { this.TopMost = (bool)y.Value; };
        }

        private void button1_Click(object sender, EventArgs e)
        {
            UUID asset_id = UUID.Random();

            byte[] data = Utils.StringToBytes(richTextBox1.Text);

            Proxy.OpenSim.Assets.UploadAsset(asset_id, mItem.AssetType, mItem.Name, mItem.Description, mItem.CreatorID, data, (success, new_id) =>
            {
                if (success)
                {
                    UUID folder_id = Proxy.Inventory.SuitcaseID;

                    UUID item_id = UUID.Random();

                    Proxy.OpenSim.XInventory.AddItem(folder_id, item_id, new_id, mItem.AssetType, mItem.InventoryType, mItem.Flags, mItem.Name, mItem.Description, DateTime.UtcNow, (created) =>
                    {
                        if(created)
                        {
                            Proxy.Inventory.RequestFetchInventory(item_id, Proxy.Agent.AgentID, false);
                        }
                        else Proxy.AlertMessage("Failed to upload item!", false);
                    });
                }
                else Proxy.AlertMessage("Failed to upload asset!", false);
            });
        }
    }
}
