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
    public partial class TextBoxDialogBox : Form
    {
        public string Result
        {
            get
            {
                return textBox1.Text;
            }
        }

        public TextBoxDialogBox(string title, string message)
        {
            InitializeComponent();
            this.Text = title;
            label1.Text = message;

            this.TopMost = Program.Frame.Settings.getBool("KeepCoolProxyOnTop");
            Program.Frame.Settings.getSetting("KeepCoolProxyOnTop").OnChanged += (x, y) => { this.TopMost = (bool)y.Value; };
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
