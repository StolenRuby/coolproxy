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
    public partial class TouchSpammerForm : Form
    {
        private CoolProxyFrame Proxy;

        uint TargetLocalID = 0;

        public TouchSpammerForm(CoolProxyFrame frame)
        {
            Proxy = frame;
            InitializeComponent();
        }

        private void setTargetButton_Click(object sender, EventArgs e)
        {
            var selection = Proxy.Agent.Selection;
            if(selection.Length == 1)
            {
                TargetLocalID = selection[0];
                checkBox1.Enabled = true;
                if (Proxy.Network.CurrentSim.ObjectsPrimitives.TryGetValue(selection[0], out Primitive prim))
                {
                    label3.Text = Convert.ToString(selection[0]);
                    if (prim.Properties != null)
                    {
                        label4.Text = prim.Properties.Name;
                    }
                    else label4.Text = "(unknown)";
                }
            }
            else
            {
                Proxy.SayToUser("You need to be selecting exactly one prim.");
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            setTargetButton.Enabled = !checkBox1.Checked;
            numericUpDown1.Enabled = !checkBox1.Checked;
            timer1.Enabled = checkBox1.Checked;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            Proxy.Agent.Touch(TargetLocalID);
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            timer1.Interval = (int)numericUpDown1.Value;
        }
    }
}
