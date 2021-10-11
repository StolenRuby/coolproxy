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

namespace CoolProxy.Plugins.Spammers
{
    public partial class IMSpamForm : Form
    {
        CoolProxyFrame Frame;

        public IMSpamForm(CoolProxyFrame frame)
        {
            Frame = frame;
            InitializeComponent();
        }

        UUID TargetID = UUID.Zero;
        string SpamMessage;
        bool RandomiseSessions = true;

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if(checkBox1.Checked)
            {
                textBox1.Enabled = false;
                textBox2.Enabled = false;
                numericUpDown1.Enabled = false;
                checkBox2.Enabled = false;

                if (TargetID != UUID.Zero)
                {
                    RandomiseSessions = checkBox2.Checked;
                    checkBox1.Text = "Disable";
                    SpamMessage = textBox2.Text;
                    timer1.Start();
                }
                else
                {
                    checkBox1.Checked = false;
                }
            }
            else
            {
                checkBox1.Text = "Enable";
                textBox1.Enabled = true;
                textBox2.Enabled = true;
                numericUpDown1.Enabled = true;
                checkBox2.Enabled = true;
                timer1.Stop();
            }
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            timer1.Interval = (int)numericUpDown1.Value;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            Frame.Agent.InstantMessage(TargetID, SpamMessage, RandomiseSessions ? UUID.Random() : TargetID);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            AvatarPickerSearchForm avatarPickerSearch = new AvatarPickerSearchForm();
            avatarPickerSearch.StartPosition = FormStartPosition.Manual;
            avatarPickerSearch.Location = new Point(this.Location.X + this.Width, this.Location.Y);

            if (avatarPickerSearch.ShowDialog() == DialogResult.OK)
            {
                TargetID = avatarPickerSearch.SelectedID;
                textBox1.Text = TargetID.ToString();
                textBox3.Text = avatarPickerSearch.SelectedName;
                checkBox1.Enabled = true;
            }
        }
    }
}
