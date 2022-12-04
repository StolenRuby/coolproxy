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
    public partial class ProxyAddressDialog : Form
    {
        public ProxyAddressDialog()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Program.Frame.Config.loginPort = (ushort)numericUpDown2.Value;

            if (Start())
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        bool Start()
        {
            try
            {
                Program.Frame.Start();
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            return false;
        }
    }
}
