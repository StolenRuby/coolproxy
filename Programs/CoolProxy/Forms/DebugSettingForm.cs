using GridProxy;
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
    public partial class DebugSettingForm : Form
    {
        public DebugSettingForm()
        {
            InitializeComponent();

            foreach(Setting s in Program.Frame.Settings.getSettings())
            {
                this.settingsComboBox.Items.Add(s.Name);
            }

            this.settingsComboBox.SelectedIndex = 0;

            this.TopMost = Program.Frame.Settings.getBool("KeepCoolProxyOnTop");
            Program.Frame.Settings.getSetting("KeepCoolProxyOnTop").OnChanged += (x, y) => { this.TopMost = (bool)y.Value; };
        }

        Setting SelectedSetting = null;

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateUI();
        }

        public void UpdateUI()
        {
            SelectedSetting = null; // null so events dont trigger while changing
            Setting setting = Program.Frame.Settings.getSetting(this.settingsComboBox.Text);

            if(setting != null)
            {
                this.descTextbox.Text = setting.Comment;

                string type = setting.Type;

                if(type == "string")
                {
                    this.stringTextbox.Visible = true;
                    this.stringTextbox.Text = setting.Value.ToString();
                }
                else
                {
                    this.stringTextbox.Visible = false;
                }
                
                if (type == "bool")
                {
                    this.radioButton1.Visible = true;
                    this.radioButton2.Visible = true;

                    bool b = (bool)setting.Value;

                    this.radioButton1.Checked = b;
                    this.radioButton2.Checked = !b;
                }
                else
                {
                    this.radioButton1.Visible = false;
                    this.radioButton2.Visible = false;
                }
                
                if (type == "color")
                {
                    this.colorLabel.Visible = true;
                    this.colourPicker.Visible = true;
                    this.yUpDown.Visible = true;

                    Color4 color = (Color4)setting.Value;

                    this.yUpDown.Value = (decimal)color.A;
                }
                else
                {
                    this.yUpDown.Visible = false;
                    this.colorLabel.Visible = false;
                    this.colourPicker.Visible = false;
                }
                
                if (type == "integer")
                {
                    this.numericUpDown1.Visible = true;
                    this.label1.Visible = true;
                    this.numericUpDown1.DecimalPlaces = 0;
                    this.numericUpDown1.Value = (int)setting.Value;
                }
                else if(type == "double")
                {
                    this.numericUpDown1.Visible = true;
                    this.label1.Visible = true;
                    this.numericUpDown1.DecimalPlaces = 7;
                    this.numericUpDown1.Value = (decimal)(double)setting.Value;
                }
                else
                {
                    this.numericUpDown1.Visible = false;
                    this.label1.Visible = false;
                }
                
                if (type == "vector")
                {
                    this.xUpDown.Visible = true;
                    this.zUpDown.Visible = true;
                    this.numericUpDown5.Visible = true;
                    this.wUpDown.Visible = false;

                    this.label2.Visible = true;
                    this.label5.Visible = true;
                    this.label3.Visible = true;
                    this.label4.Visible = false;

                    Vector3 vector = (Vector3)setting.Value;

                    this.xUpDown.Value = (decimal)vector.X;
                    this.numericUpDown5.Value = (decimal)vector.Y;
                    this.zUpDown.Value = (decimal)vector.Z;
                }
                else if (type == "quaternion")
                {
                    this.xUpDown.Visible = true;
                    this.zUpDown.Visible = true;
                    this.wUpDown.Visible = true;
                    this.numericUpDown5.Visible = true;

                    this.label2.Visible = true;
                    this.label3.Visible = true;
                    this.label4.Visible = true;
                    this.label5.Visible = true;

                    Quaternion quaternion = (Quaternion)setting.Value;

                    this.xUpDown.Value = (decimal)quaternion.X;
                    this.numericUpDown5.Value = (decimal)quaternion.Y;
                    this.zUpDown.Value = (decimal)quaternion.Z;
                    this.wUpDown.Value = (decimal)quaternion.W;
                }
                else
                {
                    this.xUpDown.Visible = false;
                    this.zUpDown.Visible = false;
                    this.wUpDown.Visible = false;
                    this.numericUpDown5.Visible = false;

                    this.label2.Visible = false;
                    this.label3.Visible = false;
                    this.label4.Visible = false;
                    this.label5.Visible = false;
                }

                SelectedSetting = setting;
            }
        }

        private void stringTextbox_TextChanged(object sender, EventArgs e)
        {
            if (SelectedSetting?.Type == "string")
            {
                SelectedSetting.Value = stringTextbox.Text;
            }
        }

        private void vectorOrQuat_ValueChanged(object sender, EventArgs e)
        {
            if (SelectedSetting?.Type == "vector")
            {
                Vector3 current = (Vector3)SelectedSetting.Value;

                current.X = (float)xUpDown.Value;
                current.Y = (float)yUpDown.Value;
                current.Z = (float)zUpDown.Value;

                SelectedSetting.Value = current;
            }
            else if (SelectedSetting?.Type == "quaternion")
            {
                Quaternion current = (Quaternion)SelectedSetting.Value;

                current.X = (float)xUpDown.Value;
                current.Y = (float)yUpDown.Value;
                current.Z = (float)zUpDown.Value;
                current.W = (float)wUpDown.Value;

                SelectedSetting.Value = current;
            }
        }

        private void bool_CheckedChanged(object sender, EventArgs e)
        {
            if (SelectedSetting?.Type == "bool")
            {
                SelectedSetting.Value = radioButton1.Checked;
            }
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            if (SelectedSetting?.Type == "integer")
            {
                SelectedSetting.Value = (int)numericUpDown1.Value;
            }
            else if (SelectedSetting?.Type == "double")
            {
                SelectedSetting.Value = (double)numericUpDown1.Value;
            }
        }

        private void resetButton_Click(object sender, EventArgs e)
        {
            if(SelectedSetting != null)
            {
                SelectedSetting.Value = SelectedSetting.Default;
                UpdateUI();
            }
        }
    }
}
