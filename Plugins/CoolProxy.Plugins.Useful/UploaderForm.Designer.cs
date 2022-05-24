namespace CoolProxy.Plugins.Useful
{
    partial class UploaderForm
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
            this.components = new System.ComponentModel.Container();
            this.uploadButton = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.assetTypeCombo = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.inventoryTypeCombo = new System.Windows.Forms.ComboBox();
            this.flagsLabel = new System.Windows.Forms.Label();
            this.wearableTypeCombo = new System.Windows.Forms.ComboBox();
            this.uploadProgressBar = new System.Windows.Forms.ProgressBar();
            this.label1 = new System.Windows.Forms.Label();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.chooseFileButton = new System.Windows.Forms.Button();
            this.radioButton1 = new System.Windows.Forms.RadioButton();
            this.radioButton2 = new System.Windows.Forms.RadioButton();
            this.radioButton3 = new System.Windows.Forms.RadioButton();
            this.label5 = new System.Windows.Forms.Label();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.SuspendLayout();
            // 
            // uploadButton
            // 
            this.uploadButton.Location = new System.Drawing.Point(287, 143);
            this.uploadButton.Name = "uploadButton";
            this.uploadButton.Size = new System.Drawing.Size(97, 21);
            this.uploadButton.TabIndex = 1;
            this.uploadButton.Text = "Upload Asset";
            this.uploadButton.UseVisualStyleBackColor = true;
            this.uploadButton.Click += new System.EventHandler(this.uploadButton_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(18, 64);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(63, 13);
            this.label2.TabIndex = 8;
            this.label2.Text = "Asset Type:";
            // 
            // assetTypeCombo
            // 
            this.assetTypeCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.assetTypeCombo.FormattingEnabled = true;
            this.assetTypeCombo.Location = new System.Drawing.Point(15, 82);
            this.assetTypeCombo.Name = "assetTypeCombo";
            this.assetTypeCombo.Size = new System.Drawing.Size(127, 21);
            this.assetTypeCombo.TabIndex = 7;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(151, 64);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(81, 13);
            this.label3.TabIndex = 10;
            this.label3.Text = "Inventory Type:";
            // 
            // inventoryTypeCombo
            // 
            this.inventoryTypeCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.inventoryTypeCombo.FormattingEnabled = true;
            this.inventoryTypeCombo.Location = new System.Drawing.Point(148, 82);
            this.inventoryTypeCombo.Name = "inventoryTypeCombo";
            this.inventoryTypeCombo.Size = new System.Drawing.Size(113, 21);
            this.inventoryTypeCombo.TabIndex = 9;
            // 
            // flagsLabel
            // 
            this.flagsLabel.AutoSize = true;
            this.flagsLabel.Location = new System.Drawing.Point(270, 64);
            this.flagsLabel.Name = "flagsLabel";
            this.flagsLabel.Size = new System.Drawing.Size(83, 13);
            this.flagsLabel.TabIndex = 12;
            this.flagsLabel.Text = "Wearable Type:";
            // 
            // wearableTypeCombo
            // 
            this.wearableTypeCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.wearableTypeCombo.FormattingEnabled = true;
            this.wearableTypeCombo.Location = new System.Drawing.Point(267, 82);
            this.wearableTypeCombo.Name = "wearableTypeCombo";
            this.wearableTypeCombo.Size = new System.Drawing.Size(117, 21);
            this.wearableTypeCombo.TabIndex = 11;
            // 
            // uploadProgressBar
            // 
            this.uploadProgressBar.Location = new System.Drawing.Point(15, 144);
            this.uploadProgressBar.Name = "uploadProgressBar";
            this.uploadProgressBar.Size = new System.Drawing.Size(266, 20);
            this.uploadProgressBar.TabIndex = 17;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(18, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(26, 13);
            this.label1.TabIndex = 18;
            this.label1.Text = "File:";
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(15, 25);
            this.textBox1.Name = "textBox1";
            this.textBox1.ReadOnly = true;
            this.textBox1.Size = new System.Drawing.Size(314, 20);
            this.textBox1.TabIndex = 19;
            // 
            // chooseFileButton
            // 
            this.chooseFileButton.Location = new System.Drawing.Point(335, 24);
            this.chooseFileButton.Name = "chooseFileButton";
            this.chooseFileButton.Size = new System.Drawing.Size(49, 22);
            this.chooseFileButton.TabIndex = 20;
            this.chooseFileButton.Text = "...";
            this.chooseFileButton.UseVisualStyleBackColor = true;
            this.chooseFileButton.Click += new System.EventHandler(this.button1_Click);
            // 
            // radioButton1
            // 
            this.radioButton1.AutoSize = true;
            this.radioButton1.Location = new System.Drawing.Point(86, 115);
            this.radioButton1.Name = "radioButton1";
            this.radioButton1.Size = new System.Drawing.Size(48, 17);
            this.radioButton1.TabIndex = 21;
            this.radioButton1.Text = "UDP";
            this.radioButton1.UseVisualStyleBackColor = true;
            this.radioButton1.MouseHover += new System.EventHandler(this.radioButton_MouseHover);
            // 
            // radioButton2
            // 
            this.radioButton2.AutoSize = true;
            this.radioButton2.Location = new System.Drawing.Point(147, 115);
            this.radioButton2.Name = "radioButton2";
            this.radioButton2.Size = new System.Drawing.Size(49, 17);
            this.radioButton2.TabIndex = 22;
            this.radioButton2.Text = "Caps";
            this.radioButton2.UseVisualStyleBackColor = true;
            this.radioButton2.MouseHover += new System.EventHandler(this.radioButton_MouseHover);
            // 
            // radioButton3
            // 
            this.radioButton3.AutoSize = true;
            this.radioButton3.Checked = true;
            this.radioButton3.Location = new System.Drawing.Point(210, 115);
            this.radioButton3.Name = "radioButton3";
            this.radioButton3.Size = new System.Drawing.Size(70, 17);
            this.radioButton3.TabIndex = 23;
            this.radioButton3.TabStop = true;
            this.radioButton3.Text = "ROBUST";
            this.radioButton3.UseVisualStyleBackColor = true;
            this.radioButton3.MouseHover += new System.EventHandler(this.radioButton_MouseHover);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(18, 117);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(62, 13);
            this.label5.TabIndex = 24;
            this.label5.Text = "Upload Via:";
            // 
            // UploaderForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(397, 175);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.radioButton3);
            this.Controls.Add(this.radioButton2);
            this.Controls.Add(this.radioButton1);
            this.Controls.Add(this.chooseFileButton);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.uploadProgressBar);
            this.Controls.Add(this.uploadButton);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.wearableTypeCombo);
            this.Controls.Add(this.assetTypeCombo);
            this.Controls.Add(this.flagsLabel);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.inventoryTypeCombo);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "UploaderForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "Asset Upload Utility";
            this.Activated += new System.EventHandler(this.UploaderForm_Activated);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button uploadButton;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox assetTypeCombo;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox inventoryTypeCombo;
        private System.Windows.Forms.Label flagsLabel;
        private System.Windows.Forms.ComboBox wearableTypeCombo;
        private System.Windows.Forms.ProgressBar uploadProgressBar;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Button chooseFileButton;
        private System.Windows.Forms.RadioButton radioButton1;
        private System.Windows.Forms.RadioButton radioButton2;
        private System.Windows.Forms.RadioButton radioButton3;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ToolTip toolTip1;
    }
}