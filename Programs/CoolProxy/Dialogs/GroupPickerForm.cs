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

namespace CoolProxy
{
    public partial class GroupPickerForm : Form
    {
        public string GroupName { get; set; } = string.Empty;
        public UUID GroupID { get; set; } = UUID.Zero;

        public GroupPickerForm(GroupPowers powers)
        {
            InitializeComponent();

            CoolProxy.Frame.Groups.GroupList.ForEach(group =>
            {
                if(group.Powers.HasFlag(powers))
                {
                    dataGridView1.Rows.Add(group.Name, group.ID);
                }
            });

            this.TopMost = CoolProxy.Settings.getBool("KeepCoolProxyOnTop");
            CoolProxy.Settings.getSetting("KeepCoolProxyOnTop").OnChanged += (x, y) => { this.TopMost = (bool)y.Value; };
        }

        private void dataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                button2.Enabled = true;
                GroupID = (UUID)dataGridView1.SelectedRows[0].Cells[1].Value;
                GroupName = (string)dataGridView1.SelectedRows[0].Cells[0].Value;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
