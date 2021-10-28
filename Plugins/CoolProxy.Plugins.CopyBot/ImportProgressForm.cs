using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CoolProxy.Plugins.CopyBot
{
    public partial class ImportProgressForm : Form
    {
        public ImportProgressForm()
        {
            InitializeComponent();
        }

        public void ReportProgress(int current, int max, bool done = false)
        {
            progressBar1.Maximum = max;
            progressBar1.Value = current;

            if(done)
            {
                button1.Text = "Finish";
                button1.Click -= button1_Click;
                button1.Click += finish_click;
            }
        }

        private void finish_click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            CopyBotPlugin.Instance.CancelImport();
            this.Close();
        }
    }
}
