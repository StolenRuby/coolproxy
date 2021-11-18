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
    public partial class AvatarAnimationsForm : Form
    {
        public AvatarAnimationsForm(AgentDisplayName name, Dictionary<UUID, DateTime> anims)
        {
            InitializeComponent();

            Text = name.LegacyFullName + "'s Played Animations";

            foreach(var pair in anims)
            {
                animsDataGridView.Rows.Add(pair.Key, pair.Value);
            }
        }

        private void copyToInventoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UUID folder_id = CoolProxy.Frame.Inventory.SuitcaseID != UUID.Zero ?
                CoolProxy.Frame.Inventory.FindSuitcaseFolderForType(FolderType.Animation) :
                CoolProxy.Frame.Inventory.FindFolderForType(FolderType.Animation);

            foreach (DataGridViewRow row in animsDataGridView.SelectedRows)
            {
                UUID anim_id = (UUID)row.Cells[0].Value;
                UUID item_id = UUID.Random();

                CoolProxy.Frame.OpenSim.XInventory.AddItem(folder_id, item_id, anim_id, AssetType.Animation, InventoryType.Animation, 0, anim_id.ToString(), "", DateTime.UtcNow, (item_succes) =>
                {
                    if (item_succes)
                    {
                        CoolProxy.Frame.Inventory.RequestFetchInventory(item_id, CoolProxy.Frame.Agent.AgentID, false);
                    }
                    else CoolProxy.Frame.SayToUser("Failed to forge!");
                });
            }
        }

        private void animsContextMenu_Opening(object sender, CancelEventArgs e)
        {
            int count = animsDataGridView.SelectedRows.Count;
            if (count == 0)
            {
                e.Cancel = true;
                return;
            }
        }
    }
}
