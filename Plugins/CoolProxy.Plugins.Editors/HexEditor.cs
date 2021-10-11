using Be.Windows.Forms;
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
    public partial class HexEditor : Form
    {
        private InventoryItem mItem;

        private CoolProxyFrame Proxy;

        public HexEditor(InventoryItem item, EditorsPlugin plugin)
        {
            Proxy = plugin.Proxy;

            mItem = item;
            InitializeComponent();

            this.Text = "Hex Editor - " + item.Name;

            Proxy.OpenSim.Assets.DownloadAsset(item.AssetUUID, (success, data) =>
            {
                if (success)
                {
                    hexBoxRequest.ByteProvider = new DynamicByteProvider(data);
                }
                else MessageBox.Show("Failed to download asset!");
            });

            this.TopMost = plugin.Settings.getBool("KeepCoolProxyOnTop");
            plugin.Settings.getSetting("KeepCoolProxyOnTop").OnChanged += (x, y) => { this.TopMost = (bool)y.Value; };
        }

        private void button1_Click(object sender, EventArgs e)
        {
            UUID creator_id = checkBox1.Checked ? Proxy.Agent.AgentID : mItem.CreatorID;

            var prov = hexBoxRequest.ByteProvider;
            byte[] data = new byte[prov.Length];
            for(int i = 0; i < data.Length; i++)
            {
                data[i] = prov.ReadByte(i);
            }


            UUID asset_id = UUID.Random();

            Proxy.OpenSim.Assets.UploadAsset(asset_id, mItem.AssetType, mItem.Name, mItem.Description, creator_id, data, (success, new_id) =>
            {
                if (success)
                {
                    UUID folder_id = Proxy.Inventory.SuitcaseID;

                    UUID item_id = UUID.Random();

                    int creation_date = (int)Utils.DateTimeToUnixTime(checkBox2.Checked ? DateTime.UtcNow : mItem.CreationDate);

                    bool full_perm = checkBox3.Checked;

                    uint next_owner_mask = full_perm ? (uint)PermissionMask.All : (uint)mItem.Permissions.NextOwnerMask;
                    uint owner_mask = full_perm ? (uint)PermissionMask.All : (uint)mItem.Permissions.OwnerMask;
                    uint group_mas = full_perm ? 0 : (uint)mItem.Permissions.GroupMask;
                    uint everyone_mask = full_perm ? (uint)PermissionMask.All : (uint)mItem.Permissions.EveryoneMask;
                    uint base_mask = full_perm ? (uint)PermissionMask.All : (uint)mItem.Permissions.BaseMask;

                    Proxy.OpenSim.XInventory.AddItem(folder_id, item_id, new_id, Proxy.Agent.AgentID, mItem.AssetType, mItem.InventoryType, mItem.Flags, mItem.Name, mItem.Description, creation_date,
                            next_owner_mask, owner_mask, base_mask, everyone_mask, group_mas, mItem.GroupID, mItem.GroupOwned, (uint)mItem.SalePrice, mItem.SaleType, string.Empty, string.Empty, (created) =>
                            {
                                if (created)
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
