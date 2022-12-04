using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CoolProxy.Controls
{
    public partial class NumericUpDown : System.Windows.Forms.NumericUpDown
    {
        [Browsable(true)]
        [Category("Cool Proxy Settings")]
        public string Setting
        { get; set; }

        [Browsable(true)]
        [Category("Cool Proxy Settings")]
        public string EnabledSetting
        { get; set; }

        public NumericUpDown()
        {
            InitializeComponent();

            base.Minimum = 0;
            this.HandleCreated += NumericUpDown_HandleCreated;
        }

        private void NumericUpDown_HandleCreated(object sender, EventArgs e)
        {
            if (this.Setting != null)
            {
                if (this.Setting != string.Empty && this.Setting != null)
                {
                    base.Value = Program.Frame?.Settings != null ? Program.Frame.Settings.getInteger(Setting) : 0;
                    base.ValueChanged += NumericUpDown_ValueChanged;
                }

                if (this.EnabledSetting != string.Empty && this.EnabledSetting != null)
                {
                    if (Program.Frame?.Settings != null)
                    {
                        Program.Frame.Settings.getSetting(EnabledSetting).OnChanged += EnabledSetting_OnChanged;
                    }
                    base.Enabled = Program.Frame?.Settings != null ? Program.Frame.Settings.getBool(EnabledSetting) : false;
                }
            }
        }

        private void Setting_OnChanged(object source, GridProxy.SettingChangedEventArgs e)
        {
            base.ValueChanged -= NumericUpDown_ValueChanged;
            this.Value = (int)e.Value;
            base.ValueChanged += NumericUpDown_ValueChanged;
        }

        private void EnabledSetting_OnChanged(object source, GridProxy.SettingChangedEventArgs e)
        {
            //base.ValueChanged -= NumericUpDown_ValueChanged;
            this.Enabled = (bool)e.Value;
            //base.ValueChanged += NumericUpDown_ValueChanged;
        }

        private void NumericUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (this.Setting != null)
            {
                if (this.Setting != string.Empty)
                {
                    Program.Frame.Settings.setInteger(Setting, (int)base.Value);
                }
            }
        }
    }
}
