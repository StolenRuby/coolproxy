﻿using System;
using System.ComponentModel;

namespace CoolGUI.Controls
{
    public partial class CheckBox : System.Windows.Forms.CheckBox
    {
        [Browsable(true)]
        [Category("Cool Proxy Settings")]
        public string Setting
        { get; set; }

        [Browsable(true)]
        [Category("Cool Proxy Settings")]
        public string EnabledSetting
        { get; set; }

        public CheckBox()
        {
            InitializeComponent();
            
            this.HandleCreated += CheckBox_HandleCreated;
        }

        private void CheckBox_HandleCreated(object sender, EventArgs e)
        {
            if (this.Setting != null)
            {
                if (this.Setting != string.Empty && this.Setting != null)
                {

                    if (CoolProxy.CoolProxy.Settings != null)
                    {
                        CoolProxy.CoolProxy.Settings.getSetting(Setting).OnChanged += Setting_OnChanged;
                    }

                    base.Checked = CoolProxy.CoolProxy.Settings != null ? CoolProxy.CoolProxy.Settings.getBool(Setting) : false;
                    base.CheckedChanged += CPCheckbox_CheckedChanged;
                }

                if (this.EnabledSetting != string.Empty && this.EnabledSetting != null)
                {
                    if(CoolProxy.CoolProxy.Settings != null)
                    {
                        CoolProxy.CoolProxy.Settings.getSetting(EnabledSetting).OnChanged += Enabled_OnChanged;
                    }
                    base.Enabled = CoolProxy.CoolProxy.Settings != null ? CoolProxy.CoolProxy.Settings.getBool(EnabledSetting) : false;
                }
            }
        }
        private void Setting_OnChanged(object source, CoolProxy.SettingChangedEventArgs e)
        {
            this.CheckedChanged -= CPCheckbox_CheckedChanged;
            base.Checked = (bool)e.Value;
            this.CheckedChanged += CPCheckbox_CheckedChanged;
        }

        private void Enabled_OnChanged(object source, CoolProxy.SettingChangedEventArgs e)
        {
            base.Enabled = (bool)e.Value;
        }

        private void CPCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            if (this.Setting != null)
            {
                if (this.Setting != string.Empty)
                {
                    CoolProxy.CoolProxy.Settings.setBool(Setting, base.Checked);
                }
            }
        }
    }
}
