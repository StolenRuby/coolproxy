namespace CoolProxy
{
    partial class DebugSettingForm
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
            this.settingsComboBox = new System.Windows.Forms.ComboBox();
            this.descTextbox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.stringTextbox = new System.Windows.Forms.TextBox();
            this.colourPicker = new System.Windows.Forms.PictureBox();
            this.numericUpDown1 = new System.Windows.Forms.NumericUpDown();
            this.xUpDown = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.zUpDown = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.wUpDown = new System.Windows.Forms.NumericUpDown();
            this.label4 = new System.Windows.Forms.Label();
            this.numericUpDown5 = new System.Windows.Forms.NumericUpDown();
            this.label5 = new System.Windows.Forms.Label();
            this.radioButton1 = new System.Windows.Forms.RadioButton();
            this.radioButton2 = new System.Windows.Forms.RadioButton();
            this.resetButton = new System.Windows.Forms.Button();
            this.colorLabel = new System.Windows.Forms.Label();
            this.yUpDown = new System.Windows.Forms.NumericUpDown();
            this.label7 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.colourPicker)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.xUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.zUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.wUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown5)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.yUpDown)).BeginInit();
            this.SuspendLayout();
            // 
            // settingsComboBox
            // 
            this.settingsComboBox.FormattingEnabled = true;
            this.settingsComboBox.Location = new System.Drawing.Point(12, 12);
            this.settingsComboBox.Name = "settingsComboBox";
            this.settingsComboBox.Size = new System.Drawing.Size(301, 21);
            this.settingsComboBox.TabIndex = 0;
            this.settingsComboBox.SelectedIndexChanged += new System.EventHandler(this.comboBox1_SelectedIndexChanged);
            // 
            // descTextbox
            // 
            this.descTextbox.Location = new System.Drawing.Point(12, 39);
            this.descTextbox.Multiline = true;
            this.descTextbox.Name = "descTextbox";
            this.descTextbox.ReadOnly = true;
            this.descTextbox.Size = new System.Drawing.Size(301, 82);
            this.descTextbox.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 129);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(37, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Value:";
            this.label1.Visible = false;
            // 
            // stringTextbox
            // 
            this.stringTextbox.Location = new System.Drawing.Point(12, 129);
            this.stringTextbox.Name = "stringTextbox";
            this.stringTextbox.Size = new System.Drawing.Size(301, 20);
            this.stringTextbox.TabIndex = 10;
            this.stringTextbox.Visible = false;
            this.stringTextbox.TextChanged += new System.EventHandler(this.stringTextbox_TextChanged);
            // 
            // colourPicker
            // 
            this.colourPicker.BackColor = System.Drawing.Color.Maroon;
            this.colourPicker.Location = new System.Drawing.Point(61, 125);
            this.colourPicker.Name = "colourPicker";
            this.colourPicker.Size = new System.Drawing.Size(25, 25);
            this.colourPicker.TabIndex = 11;
            this.colourPicker.TabStop = false;
            this.colourPicker.Visible = false;
            // 
            // numericUpDown1
            // 
            this.numericUpDown1.Location = new System.Drawing.Point(52, 127);
            this.numericUpDown1.Maximum = new decimal(new int[] {
            2147483647,
            0,
            0,
            0});
            this.numericUpDown1.Minimum = new decimal(new int[] {
            2147483647,
            0,
            0,
            -2147483648});
            this.numericUpDown1.Name = "numericUpDown1";
            this.numericUpDown1.Size = new System.Drawing.Size(113, 20);
            this.numericUpDown1.TabIndex = 12;
            this.numericUpDown1.Visible = false;
            this.numericUpDown1.ValueChanged += new System.EventHandler(this.numericUpDown1_ValueChanged);
            // 
            // xUpDown
            // 
            this.xUpDown.Location = new System.Drawing.Point(52, 129);
            this.xUpDown.Maximum = new decimal(new int[] {
            2147483647,
            0,
            0,
            0});
            this.xUpDown.Minimum = new decimal(new int[] {
            2147483647,
            0,
            0,
            -2147483648});
            this.xUpDown.Name = "xUpDown";
            this.xUpDown.Size = new System.Drawing.Size(74, 20);
            this.xUpDown.TabIndex = 14;
            this.xUpDown.Visible = false;
            this.xUpDown.ValueChanged += new System.EventHandler(this.vectorOrQuat_ValueChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(29, 131);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(17, 13);
            this.label2.TabIndex = 13;
            this.label2.Text = "X:";
            this.label2.Visible = false;
            // 
            // zUpDown
            // 
            this.zUpDown.Location = new System.Drawing.Point(52, 155);
            this.zUpDown.Maximum = new decimal(new int[] {
            2147483647,
            0,
            0,
            0});
            this.zUpDown.Minimum = new decimal(new int[] {
            2147483647,
            0,
            0,
            -2147483648});
            this.zUpDown.Name = "zUpDown";
            this.zUpDown.Size = new System.Drawing.Size(74, 20);
            this.zUpDown.TabIndex = 16;
            this.zUpDown.Visible = false;
            this.zUpDown.ValueChanged += new System.EventHandler(this.vectorOrQuat_ValueChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(29, 157);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(17, 13);
            this.label3.TabIndex = 15;
            this.label3.Text = "Z:";
            this.label3.Visible = false;
            // 
            // wUpDown
            // 
            this.wUpDown.Location = new System.Drawing.Point(167, 155);
            this.wUpDown.Maximum = new decimal(new int[] {
            2147483647,
            0,
            0,
            0});
            this.wUpDown.Minimum = new decimal(new int[] {
            2147483647,
            0,
            0,
            -2147483648});
            this.wUpDown.Name = "wUpDown";
            this.wUpDown.Size = new System.Drawing.Size(74, 20);
            this.wUpDown.TabIndex = 20;
            this.wUpDown.Visible = false;
            this.wUpDown.ValueChanged += new System.EventHandler(this.vectorOrQuat_ValueChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(144, 157);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(21, 13);
            this.label4.TabIndex = 19;
            this.label4.Text = "W:";
            this.label4.Visible = false;
            // 
            // numericUpDown5
            // 
            this.numericUpDown5.Location = new System.Drawing.Point(167, 129);
            this.numericUpDown5.Maximum = new decimal(new int[] {
            2147483647,
            0,
            0,
            0});
            this.numericUpDown5.Minimum = new decimal(new int[] {
            2147483647,
            0,
            0,
            -2147483648});
            this.numericUpDown5.Name = "numericUpDown5";
            this.numericUpDown5.Size = new System.Drawing.Size(74, 20);
            this.numericUpDown5.TabIndex = 18;
            this.numericUpDown5.Visible = false;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(144, 131);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(17, 13);
            this.label5.TabIndex = 17;
            this.label5.Text = "Y:";
            this.label5.Visible = false;
            // 
            // radioButton1
            // 
            this.radioButton1.AutoSize = true;
            this.radioButton1.Location = new System.Drawing.Point(12, 130);
            this.radioButton1.Name = "radioButton1";
            this.radioButton1.Size = new System.Drawing.Size(47, 17);
            this.radioButton1.TabIndex = 21;
            this.radioButton1.TabStop = true;
            this.radioButton1.Text = "True";
            this.radioButton1.UseVisualStyleBackColor = true;
            this.radioButton1.Visible = false;
            this.radioButton1.CheckedChanged += new System.EventHandler(this.bool_CheckedChanged);
            // 
            // radioButton2
            // 
            this.radioButton2.AutoSize = true;
            this.radioButton2.Location = new System.Drawing.Point(12, 153);
            this.radioButton2.Name = "radioButton2";
            this.radioButton2.Size = new System.Drawing.Size(50, 17);
            this.radioButton2.TabIndex = 22;
            this.radioButton2.TabStop = true;
            this.radioButton2.Text = "False";
            this.radioButton2.UseVisualStyleBackColor = true;
            this.radioButton2.Visible = false;
            this.radioButton2.CheckedChanged += new System.EventHandler(this.bool_CheckedChanged);
            // 
            // resetButton
            // 
            this.resetButton.Location = new System.Drawing.Point(12, 181);
            this.resetButton.Name = "resetButton";
            this.resetButton.Size = new System.Drawing.Size(149, 23);
            this.resetButton.TabIndex = 23;
            this.resetButton.Text = "Reset to default";
            this.resetButton.UseVisualStyleBackColor = true;
            this.resetButton.Click += new System.EventHandler(this.resetButton_Click);
            // 
            // colorLabel
            // 
            this.colorLabel.AutoSize = true;
            this.colorLabel.Location = new System.Drawing.Point(15, 131);
            this.colorLabel.Name = "colorLabel";
            this.colorLabel.Size = new System.Drawing.Size(40, 13);
            this.colorLabel.TabIndex = 24;
            this.colorLabel.Text = "Colour:";
            this.colorLabel.Visible = false;
            // 
            // yUpDown
            // 
            this.yUpDown.Location = new System.Drawing.Point(167, 129);
            this.yUpDown.Maximum = new decimal(new int[] {
            2147483647,
            0,
            0,
            0});
            this.yUpDown.Minimum = new decimal(new int[] {
            2147483647,
            0,
            0,
            -2147483648});
            this.yUpDown.Name = "yUpDown";
            this.yUpDown.Size = new System.Drawing.Size(74, 20);
            this.yUpDown.TabIndex = 26;
            this.yUpDown.Visible = false;
            this.yUpDown.ValueChanged += new System.EventHandler(this.vectorOrQuat_ValueChanged);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(124, 131);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(37, 13);
            this.label7.TabIndex = 25;
            this.label7.Text = "Alpha:";
            this.label7.Visible = false;
            // 
            // DebugSettingForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(325, 212);
            this.Controls.Add(this.yUpDown);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.colorLabel);
            this.Controls.Add(this.resetButton);
            this.Controls.Add(this.radioButton2);
            this.Controls.Add(this.radioButton1);
            this.Controls.Add(this.wUpDown);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.numericUpDown5);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.zUpDown);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.xUpDown);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.numericUpDown1);
            this.Controls.Add(this.colourPicker);
            this.Controls.Add(this.stringTextbox);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.descTextbox);
            this.Controls.Add(this.settingsComboBox);
            this.Name = "DebugSettingForm";
            this.Text = "Debug Setting";
            this.TopMost = true;
            ((System.ComponentModel.ISupportInitialize)(this.colourPicker)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.xUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.zUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.wUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown5)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.yUpDown)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox settingsComboBox;
        private System.Windows.Forms.TextBox descTextbox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox stringTextbox;
        private System.Windows.Forms.PictureBox colourPicker;
        private System.Windows.Forms.NumericUpDown numericUpDown1;
        private System.Windows.Forms.NumericUpDown xUpDown;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown zUpDown;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.NumericUpDown wUpDown;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.NumericUpDown numericUpDown5;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.RadioButton radioButton1;
        private System.Windows.Forms.RadioButton radioButton2;
        private System.Windows.Forms.Button resetButton;
        private System.Windows.Forms.Label colorLabel;
        private System.Windows.Forms.NumericUpDown yUpDown;
        private System.Windows.Forms.Label label7;
    }
}