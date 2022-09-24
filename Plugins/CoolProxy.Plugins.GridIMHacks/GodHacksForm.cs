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
    public partial class GodHacksForm : Form
    {
        CoolProxyFrame Frame;

        private UUID TargetID = UUID.Zero;
        private UUID GodID = UUID.Zero;

        private readonly UUID MrOpenSim = new UUID("6571e388-6218-4574-87db-f9379718315e");

        public GodHacksForm(CoolProxyFrame frame)
        {
            InitializeComponent();
            Frame = frame;

            GodID = MrOpenSim;
            textBox4.Text = GodID.ToString();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            AvatarPickerSearchForm avatarPickerSearch = new AvatarPickerSearchForm();
            avatarPickerSearch.StartPosition = FormStartPosition.Manual;
            avatarPickerSearch.Location = new Point(this.Location.X + this.Width, this.Location.Y);

            if (avatarPickerSearch.ShowDialog() == DialogResult.OK)
            {
                TargetID = avatarPickerSearch.SelectedID;
                //textBox1.Text = TargetID.ToString();
                textBox3.Text = avatarPickerSearch.SelectedName;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            AvatarPickerSearchForm avatarPickerSearch = new AvatarPickerSearchForm();
            avatarPickerSearch.StartPosition = FormStartPosition.Manual;
            avatarPickerSearch.Location = new Point(this.Location.X + this.Width, this.Location.Y);

            if (avatarPickerSearch.ShowDialog() == DialogResult.OK)
            {
                GodID = avatarPickerSearch.SelectedID;
                textBox4.Text = GodID.ToString();

                button5.Enabled = true;
                button6.Enabled = true;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            GodID = MrOpenSim;
            textBox4.Text = GodID.ToString();
            button5.Enabled = false;
            button6.Enabled = false;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Frame.OpenSim.GridIM.SendGridIM(GodID, string.Empty, TargetID, InstantMessageDialog.OpenSimKickUser, false, "Kicked by an admin", TargetID, false, Frame.Agent.SimPosition, Frame.Network.CurrentSim.ID, 0, new byte[1] { 0 }, Utils.GetUnixTime());
        }

        private void button5_Click(object sender, EventArgs e)
        {
            Frame.OpenSim.GridIM.SendGridIM(GodID, string.Empty, TargetID, InstantMessageDialog.OpenSimKickUser, false, "Frozen by an admin", TargetID, false, Frame.Agent.SimPosition, Frame.Network.CurrentSim.ID, 0, new byte[1] { 1 }, Utils.GetUnixTime());
        }

        private void button6_Click(object sender, EventArgs e)
        {
            Frame.OpenSim.GridIM.SendGridIM(GodID, string.Empty, TargetID, InstantMessageDialog.OpenSimKickUser, false, "Unfrozen by an admin", TargetID, false, Frame.Agent.SimPosition, Frame.Network.CurrentSim.ID, 0, new byte[1] { 2 }, Utils.GetUnixTime());
        }

        private void button7_Click(object sender, EventArgs e)
        {
            Frame.OpenSim.GridIM.SendGridIM(GodID, string.Empty, TargetID, InstantMessageDialog.GodLikeRequestTeleport, false, "@" + Frame.Network.CurrentSim.GridURI, TargetID, false, Frame.Agent.SimPosition, Frame.Network.CurrentSim.ID, 0, new byte[0], Utils.GetUnixTime());
        }
    }
}
