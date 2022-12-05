using GridProxy;
using OpenMetaverse;
using OpenMetaverse.Packets;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CoolProxy.Plugins.MagicRez
{
    public partial class MagicRezForm : Form
    {
        private CoolProxyFrame Proxy;
        private IMagicRez MagicRez;

        private UUID targetAgentID = UUID.Zero;

        public MagicRezForm(CoolProxyFrame frame, IMagicRez magicRez)
        {
            Proxy = frame;
            MagicRez = magicRez;
            InitializeComponent();
        }

        private void setTargetButton_Click(object sender, EventArgs e)
        {
            AvatarPickerSearchForm avatarPickerSearch = new AvatarPickerSearchForm();
            avatarPickerSearch.StartPosition = FormStartPosition.Manual;
            avatarPickerSearch.Location = new Point(this.Location.X + this.Width, this.Location.Y);

            if (avatarPickerSearch.ShowDialog() == DialogResult.OK)
            {
                targetAgentID = avatarPickerSearch.SelectedID;
                targetAgentKey.Text = avatarPickerSearch.SelectedID.ToString();
                targetAgentName.Text = avatarPickerSearch.SelectedName;

                enableMagicRez.Enabled = true;
            }
        }

        private void enableMagicRez_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox checkBox = (CheckBox)sender;

            if (checkBox.Checked)
            {
                checkBox.Text = "Disable Magic Rez";
                Proxy.Network.AddDelegate(PacketType.RezObject, Direction.Outgoing, handleRezObject);
            }
            else
            {

                checkBox.Text = "Enable Magic Rez";
                Proxy.Network.RemoveDelegate(PacketType.RezObject, Direction.Outgoing, handleRezObject);
            }
        }

        private Packet handleRezObject(Packet packet, RegionManager.RegionProxy sim)
        {
            if (packet.Header.Reliable)
            {
                Proxy.Network.SpoofAck(packet.Header.Sequence);
            }

            RezObjectPacket rezObjectPacket = (RezObjectPacket)packet;

            InventoryItem item = Proxy.Inventory.FetchItem(rezObjectPacket.InventoryData.ItemID, rezObjectPacket.InventoryData.OwnerID, 5000);
            if(item != null)
            {
                MagicRez.Rez(item.AssetUUID, rezObjectPacket.RezData.RayEnd, targetAgentID, UUID.Zero, string.Empty, changePermsGranter.Checked);
            }
            else
            {
                Proxy.AlertMessage("Failed to get inventory item!", false);
            }

            //Proxy.OpenSim.XInventory.GetItem(rezObjectPacket.InventoryData.ItemID, Proxy.Agent.AgentID, (item) =>
            //{
            //    if (item != null)
            //    {
            //        MagicRez.Rez(item.AssetUUID, rezObjectPacket.RezData.RayEnd, targatAgentID, UUID.Zero, string.Empty, changePermsGranter.Checked);
            //    }
            //    else
            //    {
            //        Proxy.AlertMessage("Failed to get inventory item!", false);
            //    }
            //});

            return null;
        }

        private void setEstateOwner_Click(object sender, EventArgs e)
        {
            targetAgentID = Proxy.Network.CurrentSim.Owner;
            targetAgentKey.Text = targetAgentID.ToString();
            targetAgentName.Text = "(estate owner)";
            enableMagicRez.Enabled = true;
        }

        private void setParcelOwner_Click(object sender, EventArgs e)
        {
            int id = Proxy.Parcels.GetParcelLocalID(Proxy.Network.CurrentSim, Proxy.Agent.SimPosition);

            if (Proxy.Network.CurrentSim.Parcels.TryGetValue(id, out GridProxy.Parcel parcel))
            {
                targetAgentID = parcel.OwnerID;
                targetAgentKey.Text = parcel.OwnerID.ToString();
                targetAgentName.Text = "(parcel owner)";
                enableMagicRez.Enabled = true;
            }
            else MessageBox.Show("Parcel info not loaded!");
        }
    }
}
