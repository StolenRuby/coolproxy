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

            foreach(Setting s in CoolProxy.Settings.getSettings())
            {
                this.comboBox1.Items.Add(s.Name);
            }

            this.comboBox1.SelectedIndex = 0;

            this.TopMost = CoolProxy.Settings.getBool("KeepCoolProxyOnTop");
            CoolProxy.Settings.getSetting("KeepCoolProxyOnTop").OnChanged += (x, y) => { this.TopMost = (bool)y.Value; };
        }

        private void label6_Click(object sender, EventArgs e)
        {

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            Setting s = CoolProxy.Settings.getSetting(this.comboBox1.Text);
            if(s != null)
            {
                this.textBox1.Text = s.Comment;

                string type = s.Type;

                if(type == "string")
                {
                    this.textBox2.Visible = true;
                    this.textBox2.Text = s.Value.ToString();
                }
                else
                {
                    this.textBox2.Visible = false;
                }
                
                if (type == "bool")
                {
                    this.radioButton1.Visible = true;
                    this.radioButton2.Visible = true;

                    bool b = (bool)s.Value;

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
                    this.pictureBox1.Visible = true;
                    this.numericUpDown6.Visible = true;

                    Color4 color = (Color4)s.Value;

                    this.numericUpDown6.Value = (decimal)color.A;
                }
                else
                {
                    this.numericUpDown6.Visible = false;
                    this.colorLabel.Visible = false;
                    this.pictureBox1.Visible = false;
                }
                
                if (type == "integer")
                {
                    this.numericUpDown1.Visible = true;
                    this.label1.Visible = true;
                    this.numericUpDown1.DecimalPlaces = 0;
                    this.numericUpDown1.Value = (int)s.Value;
                }
                else if(type == "double")
                {
                    this.numericUpDown1.Visible = true;
                    this.label1.Visible = true;
                    this.numericUpDown1.DecimalPlaces = 7;
                    this.numericUpDown1.Value = (decimal)(double)s.Value;
                }
                else
                {
                    this.numericUpDown1.Visible = false;
                    this.label1.Visible = false;
                }
                
                if (type == "vector")
                {
                    this.numericUpDown2.Visible = true;
                    this.numericUpDown3.Visible = true;
                    this.numericUpDown5.Visible = true;
                    this.numericUpDown4.Visible = false;

                    this.label2.Visible = true;
                    this.label5.Visible = true;
                    this.label3.Visible = true;
                    this.label4.Visible = false;

                    Vector3 vector = (Vector3)s.Value;

                    this.numericUpDown2.Value = (decimal)vector.X;
                    this.numericUpDown5.Value = (decimal)vector.Y;
                    this.numericUpDown3.Value = (decimal)vector.Z;
                }
                else if (type == "quaternion")
                {
                    this.numericUpDown2.Visible = true;
                    this.numericUpDown3.Visible = true;
                    this.numericUpDown4.Visible = true;
                    this.numericUpDown5.Visible = true;

                    this.label2.Visible = true;
                    this.label3.Visible = true;
                    this.label4.Visible = true;
                    this.label5.Visible = true;

                    Quaternion quaternion = (Quaternion)s.Value;

                    this.numericUpDown2.Value = (decimal)quaternion.X;
                    this.numericUpDown5.Value = (decimal)quaternion.Y;
                    this.numericUpDown3.Value = (decimal)quaternion.Z;
                    this.numericUpDown4.Value = (decimal)quaternion.W;
                }
                else
                {
                    this.numericUpDown2.Visible = false;
                    this.numericUpDown3.Visible = false;
                    this.numericUpDown4.Visible = false;
                    this.numericUpDown5.Visible = false;

                    this.label2.Visible = false;
                    this.label3.Visible = false;
                    this.label4.Visible = false;
                    this.label5.Visible = false;
                }
            }
        }
    }
}
