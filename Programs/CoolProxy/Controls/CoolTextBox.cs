﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CoolProxy
{
    public partial class CoolTextBox : System.Windows.Forms.TextBox
    {
        [Browsable(true)]
        [Category("Cool Proxy Settings")]
        public string Setting
        { get; set; }

        [Browsable(true)]
        [Category("Cool Proxy Settings")]
        public string EnabledSetting
        { get; set; }

        public CoolTextBox()
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
                    base.Text = CoolProxy.Frame.Settings != null ? CoolProxy.Frame.Settings.getString(Setting) : "";
                    base.TextChanged += TextBox_TextChanged;
                }

                if (this.EnabledSetting != string.Empty && this.EnabledSetting != null)
                {
                    if (CoolProxy.Frame.Settings != null)
                    {
                        CoolProxy.Frame.Settings.getSetting(EnabledSetting).OnChanged += Setting_OnChanged;
                    }
                    base.Enabled = CoolProxy.Frame.Settings != null ? CoolProxy.Frame.Settings.getBool(EnabledSetting) : false;
                }
            }
        }

        private void Setting_OnChanged(object source, GridProxy.SettingChangedEventArgs e)
        {
            base.TextChanged -= TextBox_TextChanged;
            this.Text = (string)e.Value;
            base.TextChanged += TextBox_TextChanged;
        }

        private void TextBox_TextChanged(object sender, EventArgs e)
        {
            if (this.Setting != null)
            {
                if (this.Setting != string.Empty)
                {
                    CoolProxy.Frame.Settings.setString(Setting, base.Text);
                }
            }
        }
    }
}
