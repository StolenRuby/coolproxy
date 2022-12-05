using CoolProxy.Plugins.ToolBox.Components;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CoolProxy.Plugins.ToolBox
{
    public partial class ToolBoxForm : Form
    {
        ToolboxPlugin Plugin;

        public ToolBoxForm(ToolboxPlugin plugin)
        {
            Plugin = plugin;
            InitializeComponent();

            this.Shown += (x, y) =>
            {
                this.flowLayoutPanel1.Controls.Clear();
                plugin.UpdateToolbox();
                UpdateSize();
            };
        }

        internal void AddToolboxItem(ToolBoxControl button)
        {
            flowLayoutPanel1.Controls.Add(button);
        }

        private void editToolBoxToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new ToolBoxEditor(Plugin).ShowDialog();
            UpdateSize();
        }

        internal void ClearTools()
        {
            flowLayoutPanel1.Controls.Clear();
            UpdateSize();
        }

        private void UpdateSize()
        {
            int height = 0;

            foreach(Control c in flowLayoutPanel1.Controls)
            {
                height += c.Height + flowLayoutPanel1.Margin.Vertical + c.Margin.Vertical;
            }

            int max_size = 600;

            if(height < max_size)
            {
                this.Size = new Size(252, height + 40 );
            }
            else
            {
                this.Size = new Size(270, max_size);
            }
        }
    }
}
