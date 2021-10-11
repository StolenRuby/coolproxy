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
    public partial class InventoryBackupSettingsForm : Form
    {
        public InventoryBackupSettingsForm()
        {
            InitializeComponent();

            this.TopMost = CoolProxy.Settings.getBool("KeepCoolProxyOnTop");
            CoolProxy.Settings.getSetting("KeepCoolProxyOnTop").OnChanged += (x, y) => { this.TopMost = (bool)y.Value; };
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
