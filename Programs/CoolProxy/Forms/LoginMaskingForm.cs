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
    public partial class LoginMaskingForm : Form
    {
        public LoginMaskingForm()
        {
            InitializeComponent();

            this.TopMost = CoolProxy.Frame.Settings.getBool("KeepCoolProxyOnTop");
            CoolProxy.Frame.Settings.getSetting("KeepCoolProxyOnTop").OnChanged += (x, y) => { this.TopMost = (bool)y.Value; };


            checkBox5.Checked = CoolProxy.Frame.Settings.getBool("SpoofVersion");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }


        private void replaceMacCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            bool is_checked = replaceMacCheckbox.Checked;
            macHashTextbox.Enabled = is_checked;
            randomMacHashButton.Enabled = is_checked;

            macHashTextbox.TextChanged -= macHashTextbox_TextChanged;

            macHashTextbox.Text = is_checked ? CoolProxy.Frame.Settings.getString("SpecifiedMacAddress") : "00000000000000000000000000000000";

            macHashTextbox.TextChanged += macHashTextbox_TextChanged;
        }

        private void replaceID0Checkbox_CheckedChanged(object sender, EventArgs e)
        {
            bool is_checked = replaceID0Checkbox.Checked;
            id0HashTextbox.Enabled = is_checked;
            randomID0HashButton.Enabled = is_checked;

            id0HashTextbox.TextChanged -= id0HashTextbox_TextChanged;

            id0HashTextbox.Text = is_checked ? CoolProxy.Frame.Settings.getString("SpecifiedId0Address") : "00000000000000000000000000000000";

            id0HashTextbox.TextChanged += id0HashTextbox_TextChanged;
        }
        private void randomMacHashButton_OnClick(object sender, EventArgs e)
        {
            Random random = new Random();
            macHashTextbox.Text = Utils.MD5String(random.Next(9001, 10000000).ToString());
        }

        private void randomID0HashButton_OnClick(object sender, EventArgs e)
        {
            Random random = new Random();
            id0HashTextbox.Text = Utils.MD5String(random.Next(9001, 10000000).ToString());
        }

        private void macHashTextbox_TextChanged(object sender, EventArgs e)
        {
            CoolProxy.Frame.Settings.setString("SpecifiedMacAddress", macHashTextbox.Text);
        }

        private void id0HashTextbox_TextChanged(object sender, EventArgs e)
        {
            CoolProxy.Frame.Settings.setString("SpecifiedId0Address", id0HashTextbox.Text);
        }

        private void channelTextbox_TextChanged(object sender, EventArgs e)
        {
            if (channelTextbox.Enabled)
                CoolProxy.Frame.Settings.setString("SpecifiedVersionChannel", channelTextbox.Text);
        }

        private void checkBox5_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox checkBox = sender as CheckBox;

            bool spoof_version = checkBox.Checked;

            if (!spoof_version)
                versionMajor.Enabled = versionMinor.Enabled = versionPatch.Enabled = versionBuild.Enabled = spoof_version;

            versionMajor.Value = spoof_version ? CoolProxy.Frame.Settings.getInteger("SpecifiedVersionMajor") : 0;
            versionMinor.Value = spoof_version ? CoolProxy.Frame.Settings.getInteger("SpecifiedVersionMinor") : 0;
            versionPatch.Value = spoof_version ? CoolProxy.Frame.Settings.getInteger("SpecifiedVersionPatch") : 0;
            versionBuild.Value = spoof_version ? CoolProxy.Frame.Settings.getInteger("SpecifiedVersionBuild") : 0;

            if (spoof_version)
                versionMajor.Enabled = versionMinor.Enabled = versionPatch.Enabled = versionBuild.Enabled = spoof_version;

            channelTextbox.Enabled = spoof_version;
            channelTextbox.Text = spoof_version ? CoolProxy.Frame.Settings.getString("SpecifiedVersionChannel") : "Unchanged";

            CoolProxy.Frame.Settings.setBool("SpoofVersion", spoof_version);
        }

        private void versionMajor_ValueChanged(object sender, EventArgs e)
        {
            L33T.GUI.NumericUpDown spinner = sender as L33T.GUI.NumericUpDown;

            if (spinner.Enabled)
                CoolProxy.Frame.Settings.setInteger((string)spinner.Tag, (int)spinner.Value);
        }
    }
}
