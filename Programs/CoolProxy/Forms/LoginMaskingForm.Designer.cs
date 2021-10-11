
namespace CoolProxy
{
    partial class LoginMaskingForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.checkBox5 = new CoolGUI.Controls.CheckBox();
            this.macHashTextbox = new System.Windows.Forms.TextBox();
            this.randomMacHashButton = new System.Windows.Forms.Button();
            this.id0HashTextbox = new System.Windows.Forms.TextBox();
            this.randomID0HashButton = new System.Windows.Forms.Button();
            this.channelTextbox = new System.Windows.Forms.TextBox();
            this.versionPatch = new L33T.GUI.NumericUpDown();
            this.versionMinor = new L33T.GUI.NumericUpDown();
            this.versionBuild = new L33T.GUI.NumericUpDown();
            this.replaceID0Checkbox = new CoolGUI.Controls.CheckBox();
            this.versionMajor = new L33T.GUI.NumericUpDown();
            this.replaceMacCheckbox = new CoolGUI.Controls.CheckBox();
            this.button1 = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.versionPatch)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.versionMinor)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.versionBuild)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.versionMajor)).BeginInit();
            this.SuspendLayout();
            // 
            // checkBox5
            // 
            this.checkBox5.AutoSize = true;
            this.checkBox5.EnabledSetting = null;
            this.checkBox5.Location = new System.Drawing.Point(39, 21);
            this.checkBox5.Name = "checkBox5";
            this.checkBox5.Setting = "";
            this.checkBox5.Size = new System.Drawing.Size(64, 17);
            this.checkBox5.TabIndex = 73;
            this.checkBox5.Text = "Version:";
            this.checkBox5.UseVisualStyleBackColor = true;
            this.checkBox5.CheckedChanged += new System.EventHandler(this.checkBox5_CheckedChanged);
            // 
            // macHashTextbox
            // 
            this.macHashTextbox.Enabled = false;
            this.macHashTextbox.Location = new System.Drawing.Point(109, 82);
            this.macHashTextbox.Name = "macHashTextbox";
            this.macHashTextbox.Size = new System.Drawing.Size(212, 20);
            this.macHashTextbox.TabIndex = 72;
            this.macHashTextbox.Text = "00000000000000000000000000000000";
            this.macHashTextbox.TextChanged += new System.EventHandler(this.macHashTextbox_TextChanged);
            // 
            // randomMacHashButton
            // 
            this.randomMacHashButton.Enabled = false;
            this.randomMacHashButton.Location = new System.Drawing.Point(327, 82);
            this.randomMacHashButton.Name = "randomMacHashButton";
            this.randomMacHashButton.Size = new System.Drawing.Size(21, 20);
            this.randomMacHashButton.TabIndex = 62;
            this.randomMacHashButton.Text = "#";
            this.randomMacHashButton.UseVisualStyleBackColor = true;
            this.randomMacHashButton.Click += new System.EventHandler(this.randomMacHashButton_OnClick);
            // 
            // id0HashTextbox
            // 
            this.id0HashTextbox.Enabled = false;
            this.id0HashTextbox.Location = new System.Drawing.Point(109, 109);
            this.id0HashTextbox.Name = "id0HashTextbox";
            this.id0HashTextbox.Size = new System.Drawing.Size(212, 20);
            this.id0HashTextbox.TabIndex = 71;
            this.id0HashTextbox.Text = "00000000000000000000000000000000";
            this.id0HashTextbox.TextChanged += new System.EventHandler(this.id0HashTextbox_TextChanged);
            // 
            // randomID0HashButton
            // 
            this.randomID0HashButton.Enabled = false;
            this.randomID0HashButton.Location = new System.Drawing.Point(327, 108);
            this.randomID0HashButton.Name = "randomID0HashButton";
            this.randomID0HashButton.Size = new System.Drawing.Size(21, 20);
            this.randomID0HashButton.TabIndex = 63;
            this.randomID0HashButton.Text = "#";
            this.randomID0HashButton.UseVisualStyleBackColor = true;
            this.randomID0HashButton.Click += new System.EventHandler(this.randomID0HashButton_OnClick);
            // 
            // channelTextbox
            // 
            this.channelTextbox.Enabled = false;
            this.channelTextbox.Location = new System.Drawing.Point(109, 19);
            this.channelTextbox.MaxLength = 32;
            this.channelTextbox.Name = "channelTextbox";
            this.channelTextbox.Size = new System.Drawing.Size(212, 20);
            this.channelTextbox.TabIndex = 64;
            this.channelTextbox.Text = "Unchanged";
            this.channelTextbox.TextChanged += new System.EventHandler(this.channelTextbox_TextChanged);
            // 
            // versionPatch
            // 
            this.versionPatch.Enabled = false;
            this.versionPatch.EnabledSetting = "";
            this.versionPatch.Location = new System.Drawing.Point(203, 45);
            this.versionPatch.Maximum = new decimal(new int[] {
            99,
            0,
            0,
            0});
            this.versionPatch.Name = "versionPatch";
            this.versionPatch.Setting = null;
            this.versionPatch.Size = new System.Drawing.Size(41, 20);
            this.versionPatch.TabIndex = 67;
            this.versionPatch.Tag = "SpecifiedVersionPatch";
            this.versionPatch.ValueChanged += new System.EventHandler(this.versionMajor_ValueChanged);
            // 
            // versionMinor
            // 
            this.versionMinor.Enabled = false;
            this.versionMinor.EnabledSetting = "";
            this.versionMinor.Location = new System.Drawing.Point(156, 45);
            this.versionMinor.Maximum = new decimal(new int[] {
            99,
            0,
            0,
            0});
            this.versionMinor.Name = "versionMinor";
            this.versionMinor.Setting = null;
            this.versionMinor.Size = new System.Drawing.Size(41, 20);
            this.versionMinor.TabIndex = 66;
            this.versionMinor.Tag = "SpecifiedVersionMinor";
            this.versionMinor.ValueChanged += new System.EventHandler(this.versionMajor_ValueChanged);
            // 
            // versionBuild
            // 
            this.versionBuild.Enabled = false;
            this.versionBuild.EnabledSetting = "";
            this.versionBuild.Location = new System.Drawing.Point(250, 45);
            this.versionBuild.Maximum = new decimal(new int[] {
            9999999,
            0,
            0,
            0});
            this.versionBuild.Name = "versionBuild";
            this.versionBuild.Setting = null;
            this.versionBuild.Size = new System.Drawing.Size(71, 20);
            this.versionBuild.TabIndex = 68;
            this.versionBuild.Tag = "SpecifiedVersionBuild";
            this.versionBuild.ValueChanged += new System.EventHandler(this.versionMajor_ValueChanged);
            // 
            // replaceID0Checkbox
            // 
            this.replaceID0Checkbox.AutoSize = true;
            this.replaceID0Checkbox.EnabledSetting = null;
            this.replaceID0Checkbox.Location = new System.Drawing.Point(39, 109);
            this.replaceID0Checkbox.Name = "replaceID0Checkbox";
            this.replaceID0Checkbox.Setting = "SpoofId0";
            this.replaceID0Checkbox.Size = new System.Drawing.Size(46, 17);
            this.replaceID0Checkbox.TabIndex = 70;
            this.replaceID0Checkbox.Text = "ID0:";
            this.replaceID0Checkbox.UseVisualStyleBackColor = true;
            this.replaceID0Checkbox.CheckedChanged += new System.EventHandler(this.replaceID0Checkbox_CheckedChanged);
            // 
            // versionMajor
            // 
            this.versionMajor.Enabled = false;
            this.versionMajor.EnabledSetting = "";
            this.versionMajor.Location = new System.Drawing.Point(109, 45);
            this.versionMajor.Maximum = new decimal(new int[] {
            99,
            0,
            0,
            0});
            this.versionMajor.Name = "versionMajor";
            this.versionMajor.Setting = "";
            this.versionMajor.Size = new System.Drawing.Size(41, 20);
            this.versionMajor.TabIndex = 65;
            this.versionMajor.Tag = "SpecifiedVersionMajor";
            this.versionMajor.ValueChanged += new System.EventHandler(this.versionMajor_ValueChanged);
            // 
            // replaceMacCheckbox
            // 
            this.replaceMacCheckbox.AutoSize = true;
            this.replaceMacCheckbox.EnabledSetting = null;
            this.replaceMacCheckbox.Location = new System.Drawing.Point(39, 84);
            this.replaceMacCheckbox.Name = "replaceMacCheckbox";
            this.replaceMacCheckbox.Setting = "SpoofMac";
            this.replaceMacCheckbox.Size = new System.Drawing.Size(52, 17);
            this.replaceMacCheckbox.TabIndex = 69;
            this.replaceMacCheckbox.Text = "MAC:";
            this.replaceMacCheckbox.UseVisualStyleBackColor = true;
            this.replaceMacCheckbox.CheckedChanged += new System.EventHandler(this.replaceMacCheckbox_CheckedChanged);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(151, 149);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(123, 23);
            this.button1.TabIndex = 75;
            this.button1.Text = "Okay";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // LoginMaskingForm
            // 
            this.AcceptButton = this.button1;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(415, 190);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.checkBox5);
            this.Controls.Add(this.macHashTextbox);
            this.Controls.Add(this.randomMacHashButton);
            this.Controls.Add(this.id0HashTextbox);
            this.Controls.Add(this.randomID0HashButton);
            this.Controls.Add(this.channelTextbox);
            this.Controls.Add(this.versionPatch);
            this.Controls.Add(this.versionMinor);
            this.Controls.Add(this.versionBuild);
            this.Controls.Add(this.replaceID0Checkbox);
            this.Controls.Add(this.versionMajor);
            this.Controls.Add(this.replaceMacCheckbox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "LoginMaskingForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Login Masking";
            ((System.ComponentModel.ISupportInitialize)(this.versionPatch)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.versionMinor)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.versionBuild)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.versionMajor)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private CoolGUI.Controls.CheckBox checkBox5;
        private System.Windows.Forms.TextBox macHashTextbox;
        private System.Windows.Forms.Button randomMacHashButton;
        private System.Windows.Forms.TextBox id0HashTextbox;
        private System.Windows.Forms.Button randomID0HashButton;
        private System.Windows.Forms.TextBox channelTextbox;
        private L33T.GUI.NumericUpDown versionPatch;
        private L33T.GUI.NumericUpDown versionMinor;
        private L33T.GUI.NumericUpDown versionBuild;
        private CoolGUI.Controls.CheckBox replaceID0Checkbox;
        private L33T.GUI.NumericUpDown versionMajor;
        private CoolGUI.Controls.CheckBox replaceMacCheckbox;
        private System.Windows.Forms.Button button1;
    }
}