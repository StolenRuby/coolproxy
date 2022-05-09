using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace L33T.GUI
{
    public partial class TextBox : System.Windows.Forms.TextBox
    {
        [Browsable(true)]
        [Category("Cool Proxy Settings")]
        public string Setting
        { get; set; }

        [Browsable(true)]
        [Category("Cool Proxy Settings")]
        public string EnabledSetting
        { get; set; }

        public TextBox()
        {
            InitializeComponent();

            this.HandleCreated += TextBox_HandleCreated;
        }

        private void TextBox_HandleCreated(object sender, EventArgs e)
        {
            if (this.Setting != null)
            {
                if (this.Setting != string.Empty && this.Setting != null)
                {
                    base.Text = CoolProxy.CoolProxy.Frame?.Settings != null ? CoolProxy.CoolProxy.Frame.Settings.getString(Setting) : "";
                    base.TextChanged += TextBox_TextChanged;
                }

                if (this.EnabledSetting != string.Empty && this.EnabledSetting != null)
                {
                    if (CoolProxy.CoolProxy.Frame?.Settings != null)
                    {
                        CoolProxy.CoolProxy.Frame.Settings.getSetting(EnabledSetting).OnChanged += Setting_OnChanged;
                    }
                    base.Enabled = CoolProxy.CoolProxy.Frame?.Settings != null ? CoolProxy.CoolProxy.Frame.Settings.getBool(EnabledSetting) : false;
                }
            }
        }

        private void Setting_OnChanged(object source, GridProxy.SettingChangedEventArgs e)
        {
            //base.changed -= TextBox_TextChanged;
            this.Enabled = (bool)e.Value;
            //base.TextChanged += TextBox_TextChanged;
        }

        private void TextBox_TextChanged(object sender, EventArgs e)
        {
            if (this.Setting != null)
            {
                if (this.Setting != string.Empty)
                {
                    CoolProxy.CoolProxy.Frame.Settings.setString(Setting, base.Text);
                }
            }
        }
    }
}
