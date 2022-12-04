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

namespace CoolProxy.Plugins.AssetLog
{
    public partial class AvatarAnimationsForm : Form
    {
        private CoolProxyFrame Proxy;

        public AvatarAnimationsForm(CoolProxyFrame frame, AgentDisplayName name, Dictionary<UUID, DateTime> anims)
        {
            Proxy = frame;
            InitializeComponent();

            Text = name.LegacyFullName + "'s Played Animations";

            foreach(var pair in anims)
            {
                animsDataGridView.Rows.Add(pair.Key, pair.Value);
            }
        }

        private void copyToInventoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UUID folder_id = Proxy.Inventory.SuitcaseID != UUID.Zero ?
                Proxy.Inventory.FindSuitcaseFolderForType(FolderType.Animation) :
                Proxy.Inventory.FindFolderForType(FolderType.Animation);

            foreach (DataGridViewRow row in animsDataGridView.SelectedRows)
            {
                UUID anim_id = (UUID)row.Cells[0].Value;
                UUID item_id = UUID.Random();

                //Program.Frame.OpenSim.XInventory.AddItem(folder_id, item_id, anim_id, AssetType.Animation, InventoryType.Animation, 0, anim_id.ToString(), "", DateTime.UtcNow, (item_succes) =>
                //{
                //    if (item_succes)
                //    {
                //        Program.Frame.Inventory.RequestFetchInventory(item_id, Program.Frame.Agent.AgentID, false);
                //    }
                //    else Program.Frame.SayToUser("Failed to forge!");
                //});
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
