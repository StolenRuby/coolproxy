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

namespace CoolProxy.Plugins.GridIMHacks
{
    public partial class SpecialTeleportForm : Form
    {
        private CoolProxyFrame Proxy;

        private readonly UUID MrOpenSim = new UUID("6571e388-6218-4574-87db-f9379718315e");

        private UUID TargetID = UUID.Zero;
        private UUID RegionID = UUID.Zero;

        public SpecialTeleportForm(CoolProxyFrame frame)
        {
            Proxy = frame;
            InitializeComponent();
        }

        private void setTargetButton_Click(object sender, EventArgs e)
        {
            using(AvatarPickerSearchForm picker = new AvatarPickerSearchForm())
            {
                if(picker.ShowDialog() == DialogResult.OK)
                {
                    TargetID = picker.SelectedID;
                    targetAgentName.Text = picker.SelectedName;
                }
            }
        }

        private void setRegionButton_Click(object sender, EventArgs e)
        {
            using(RegionPickerForm picker = new RegionPickerForm())
            {
                if(picker.ShowDialog() == DialogResult.OK)
                {
                    GridIMHacksPlugin.ROBUST.Gatekeeper.LinkRegion(picker.RegionName, out RegionID, out string image_url);

                    targetRegionName.Text = picker.RegionName;
                }
            }
        }

        private void sendTeleportButton_Click(object sender, EventArgs e)
        {
            Vector3 position = new Vector3((float)numericUpDown1.Value, (float)numericUpDown2.Value, (float)numericUpDown3.Value);
            GridIMHacksPlugin.ROBUST.IM.SendGridIM(MrOpenSim, "", TargetID, InstantMessageDialog.GodLikeRequestTeleport, false, "", TargetID, false, position, RegionID, 0, new byte[0], Utils.GetUnixTime());
        }
    }
}
