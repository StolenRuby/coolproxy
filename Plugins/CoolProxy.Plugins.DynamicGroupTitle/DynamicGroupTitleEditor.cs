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
    public partial class DynamicGroupTitleEditor : Form
    {
        private CoolProxyFrame Proxy;

        public UUID GroupID = UUID.Zero;

        List<GroupRole> Roles;

        private GroupRole Role;

        string[] Titles;
        int TitleIndex;

        Timer TitleTimer;

        public DynamicGroupTitleEditor(CoolProxyFrame frame)
        {
            Proxy = frame;
            InitializeComponent();

            Proxy.Groups.GroupRoleDataReply += Groups_GroupRoleDataReply;

            TitleTimer = new Timer();
            TitleTimer.Tick += UpdateTitle;
        }

        private void UpdateTitle(object sender, EventArgs e)
        {
            Role.Title = Titles[TitleIndex];

            Proxy.Groups.UpdateRole(Role);
            Proxy.Groups.ActivateGroup(GroupID);

            if (++TitleIndex >= Titles.Length)
                TitleIndex = 0;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            GroupPickerForm groupPickerForm = new GroupPickerForm(GroupPowers.RoleProperties);
            groupPickerForm.StartPosition = FormStartPosition.Manual;
            groupPickerForm.Location = new Point(this.Location.X + this.Width, this.Location.Y);

            if(groupPickerForm.ShowDialog() == DialogResult.OK)
            {
                comboBox4.Enabled = false;
                checkBox1.Enabled = false;
                comboBox4.Items.Clear();

                textBox1.Text = groupPickerForm.GroupName;
                textBox2.Text = groupPickerForm.GroupID.ToString();
                GroupID = groupPickerForm.GroupID;
                Proxy.Groups.RequestGroupRoles(GroupID);
            }
        }

        private void Groups_GroupRoleDataReply(object sender, GroupRolesDataReplyEventArgs e)
        {
            if (e.GroupID == GroupID)
            {
                checkBox1.Enabled = true;

                Roles = e.Roles.Values.ToList();

                comboBox4.Items.Clear();
                comboBox4.Items.AddRange(Roles.Select(x => x.Name).ToArray());

                if (comboBox4.Items.Count > 0)
                {
                    comboBox4.Enabled = true;
                    comboBox4.SelectedIndex = 0;
                    Role = Roles.First();
                }
            }
        }

        private void button36_Click(object sender, EventArgs e)
        {
            listBox1.Items.Add(textBox19.Text);
            listBox1.SelectedIndex = listBox1.Items.Count - 1;
            textBox19.Text = "";
        }

        private void button38_Click(object sender, EventArgs e)
        {
            listBox1.Items.RemoveAt(listBox1.SelectedIndex);
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateButtons();
        }

        void UpdateButtons()
        {
            bool not_running = !TitleTimer.Enabled;
            bool enabled = listBox1.SelectedItems.Count > 0 && not_running;

            button1.Enabled = not_running;
            comboBox4.Enabled = comboBox4.Items.Count > 0 && not_running;

            button37.Enabled = enabled;
            button38.Enabled = enabled;
            button39.Enabled = enabled;

            listBox1.Enabled = not_running;
            textBox19.Enabled = not_running;
            button36.Enabled = not_running;
            numericUpDown4.Enabled = not_running;
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if(checkBox1.Checked)
            {
                Titles = listBox1.Items.Cast<string>().ToArray();

                if(Titles.Length == 0)
                {
                    checkBox1.Checked = false;
                    return;
                }

                TitleIndex = 0;
                TitleTimer.Interval = (int)numericUpDown4.Value;
                TitleTimer.Enabled = true;
                checkBox1.Text = "Disable";
            }
            else
            {
                TitleTimer.Enabled = false;
                checkBox1.Text = "Enable";
            }

            UpdateButtons();
        }

        private void comboBox4_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(comboBox4.SelectedIndex != -1)
            {
                Role = Roles[comboBox4.SelectedIndex];
            }
        }
    }
}
