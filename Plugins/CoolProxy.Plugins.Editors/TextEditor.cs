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

            this.TopMost = frame.Settings.getBool("KeepCoolProxyOnTop");
            frame.Settings.getSetting("KeepCoolProxyOnTop").OnChanged += (x, y) => { this.TopMost = (bool)y.Value; };
        }

        private void button1_Click(object sender, EventArgs e)
        {
            UUID asset_id = UUID.Random();

            byte[] data = Utils.StringToBytes(richTextBox1.Text);

            EditorsPlugin.ROBUST.Assets.UploadAsset(asset_id, mItem.AssetType, mItem.Name, mItem.Description, mItem.CreatorID, data, (success, new_id) =>
            {
                if (success)
                {
                    UUID folder_id = Proxy.Inventory.SuitcaseID;

                    UUID item_id = UUID.Random();

                    EditorsPlugin.ROBUST.Inventory.AddItem(folder_id, item_id, new_id, mItem.AssetType, mItem.InventoryType, mItem.Flags, mItem.Name, mItem.Description, DateTime.UtcNow, (created) =>
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

        private void TextEditor_Load(object sender, EventArgs e)
        {
            var del = new Action<InventoryItem>((InventoryItem i) =>
            {
                EditorsPlugin.ROBUST.Assets.DownloadAsset(i.AssetUUID, (success, data) =>
                {
                    if (success)
                    {
                        this.Invoke(new Action(() =>
                        {
                            richTextBox1.Text = Utils.BytesToString(data);
                            richTextBox1.Enabled = true;
                        }));
                    }
                    else
                    {
                        MessageBox.Show("Failed to download asset!");
                        this.Close();
                    }
                });
            });

            if (mItem.AssetUUID == UUID.Zero)
            {
                EditorsPlugin.ROBUST.Inventory.GetItem(mItem.UUID, mItem.OwnerID, (fetched_item) =>
                {
                    if (fetched_item != null)
                    {
                        del(fetched_item);
                    }
                    else
                    {
                        MessageBox.Show("Failed to fetch item!");
                        this.Close();
                    }
                });
            }
            else del(mItem);
        }
    }
}
