
namespace CoolProxy.Plugins.ServiceTools
{
    partial class XInventoryServiceForm
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
            this.button1 = new System.Windows.Forms.Button();
            this.label11 = new System.Windows.Forms.Label();
            this.nameTextbox = new System.Windows.Forms.TextBox();
            this.label111 = new System.Windows.Forms.Label();
            this.descTextBox = new System.Windows.Forms.TextBox();
            this.itemidlabel = new System.Windows.Forms.Label();
            this.itemIDTextbox = new System.Windows.Forms.TextBox();
            this.label222 = new System.Windows.Forms.Label();
            this.assetIDTextBox = new System.Windows.Forms.TextBox();
            this.label333 = new System.Windows.Forms.Label();
            this.creatorIDTextBox = new System.Windows.Forms.TextBox();
            this.assetTypeCombo = new System.Windows.Forms.ComboBox();
            this.invTypeCombo = new System.Windows.Forms.ComboBox();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label555 = new System.Windows.Forms.Label();
            this.creatorDataTextBox = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.folderIDTextBox = new System.Windows.Forms.TextBox();
            this.button2 = new System.Windows.Forms.Button();
            this.numericUpDown1 = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.button3 = new System.Windows.Forms.Button();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.uriLabel = new System.Windows.Forms.Label();
            this.targetUriTextbox = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).BeginInit();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(12, 365);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(268, 49);
            this.button1.TabIndex = 0;
            this.button1.Text = "Add Item";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(12, 9);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(38, 13);
            this.label11.TabIndex = 4;
            this.label11.Text = "Name:";
            // 
            // nameTextbox
            // 
            this.nameTextbox.Location = new System.Drawing.Point(12, 25);
            this.nameTextbox.Name = "nameTextbox";
            this.nameTextbox.Size = new System.Drawing.Size(268, 20);
            this.nameTextbox.TabIndex = 3;
            // 
            // label111
            // 
            this.label111.AutoSize = true;
            this.label111.Location = new System.Drawing.Point(12, 48);
            this.label111.Name = "label111";
            this.label111.Size = new System.Drawing.Size(63, 13);
            this.label111.TabIndex = 6;
            this.label111.Text = "Description:";
            // 
            // descTextBox
            // 
            this.descTextBox.Location = new System.Drawing.Point(12, 64);
            this.descTextBox.Name = "descTextBox";
            this.descTextBox.Size = new System.Drawing.Size(268, 20);
            this.descTextBox.TabIndex = 5;
            // 
            // itemidlabel
            // 
            this.itemidlabel.AutoSize = true;
            this.itemidlabel.Location = new System.Drawing.Point(12, 87);
            this.itemidlabel.Name = "itemidlabel";
            this.itemidlabel.Size = new System.Drawing.Size(44, 13);
            this.itemidlabel.TabIndex = 8;
            this.itemidlabel.Text = "Item ID:";
            // 
            // itemIDTextbox
            // 
            this.itemIDTextbox.Location = new System.Drawing.Point(12, 103);
            this.itemIDTextbox.Name = "itemIDTextbox";
            this.itemIDTextbox.Size = new System.Drawing.Size(225, 20);
            this.itemIDTextbox.TabIndex = 7;
            // 
            // label222
            // 
            this.label222.AutoSize = true;
            this.label222.Location = new System.Drawing.Point(12, 126);
            this.label222.Name = "label222";
            this.label222.Size = new System.Drawing.Size(50, 13);
            this.label222.TabIndex = 10;
            this.label222.Text = "Asset ID:";
            // 
            // assetIDTextBox
            // 
            this.assetIDTextBox.Location = new System.Drawing.Point(12, 142);
            this.assetIDTextBox.Name = "assetIDTextBox";
            this.assetIDTextBox.Size = new System.Drawing.Size(268, 20);
            this.assetIDTextBox.TabIndex = 9;
            // 
            // label333
            // 
            this.label333.AutoSize = true;
            this.label333.Location = new System.Drawing.Point(12, 165);
            this.label333.Name = "label333";
            this.label333.Size = new System.Drawing.Size(58, 13);
            this.label333.TabIndex = 12;
            this.label333.Text = "Creator ID:";
            // 
            // creatorIDTextBox
            // 
            this.creatorIDTextBox.Location = new System.Drawing.Point(12, 181);
            this.creatorIDTextBox.Name = "creatorIDTextBox";
            this.creatorIDTextBox.Size = new System.Drawing.Size(214, 20);
            this.creatorIDTextBox.TabIndex = 11;
            this.creatorIDTextBox.Text = "00000000-0000-0000-0000-000000000000";
            // 
            // assetTypeCombo
            // 
            this.assetTypeCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.assetTypeCombo.FormattingEnabled = true;
            this.assetTypeCombo.Location = new System.Drawing.Point(12, 300);
            this.assetTypeCombo.Name = "assetTypeCombo";
            this.assetTypeCombo.Size = new System.Drawing.Size(121, 21);
            this.assetTypeCombo.TabIndex = 13;
            this.assetTypeCombo.SelectedIndexChanged += new System.EventHandler(this.assetTypeCombo_SelectedIndexChanged);
            // 
            // invTypeCombo
            // 
            this.invTypeCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.invTypeCombo.FormattingEnabled = true;
            this.invTypeCombo.Location = new System.Drawing.Point(159, 300);
            this.invTypeCombo.Name = "invTypeCombo";
            this.invTypeCombo.Size = new System.Drawing.Size(121, 21);
            this.invTypeCombo.TabIndex = 14;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(12, 284);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(63, 13);
            this.label7.TabIndex = 15;
            this.label7.Text = "Asset Type:";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(156, 284);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(81, 13);
            this.label8.TabIndex = 16;
            this.label8.Text = "Inventory Type:";
            // 
            // label555
            // 
            this.label555.AutoSize = true;
            this.label555.Location = new System.Drawing.Point(12, 204);
            this.label555.Name = "label555";
            this.label555.Size = new System.Drawing.Size(70, 13);
            this.label555.TabIndex = 18;
            this.label555.Text = "Creator Data:";
            // 
            // creatorDataTextBox
            // 
            this.creatorDataTextBox.Location = new System.Drawing.Point(12, 220);
            this.creatorDataTextBox.Name = "creatorDataTextBox";
            this.creatorDataTextBox.Size = new System.Drawing.Size(268, 20);
            this.creatorDataTextBox.TabIndex = 17;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 243);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 13);
            this.label2.TabIndex = 20;
            this.label2.Text = "Folder ID:";
            // 
            // folderIDTextBox
            // 
            this.folderIDTextBox.Location = new System.Drawing.Point(12, 259);
            this.folderIDTextBox.Name = "folderIDTextBox";
            this.folderIDTextBox.Size = new System.Drawing.Size(268, 20);
            this.folderIDTextBox.TabIndex = 19;
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(243, 101);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(37, 23);
            this.button2.TabIndex = 21;
            this.button2.Text = "?";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // numericUpDown1
            // 
            this.numericUpDown1.Location = new System.Drawing.Point(159, 334);
            this.numericUpDown1.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.numericUpDown1.Name = "numericUpDown1";
            this.numericUpDown1.Size = new System.Drawing.Size(121, 20);
            this.numericUpDown1.TabIndex = 22;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(118, 338);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(35, 13);
            this.label3.TabIndex = 23;
            this.label3.Text = "Flags:";
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(232, 179);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(48, 23);
            this.button3.TabIndex = 24;
            this.button3.Text = "Set";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // checkBox1
            // 
            this.checkBox1.Appearance = System.Windows.Forms.Appearance.Button;
            this.checkBox1.Location = new System.Drawing.Point(12, 334);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(100, 24);
            this.checkBox1.TabIndex = 25;
            this.checkBox1.Text = "Advanced";
            this.checkBox1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.checkBox1.UseVisualStyleBackColor = true;
            this.checkBox1.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged);
            // 
            // uriLabel
            // 
            this.uriLabel.AutoSize = true;
            this.uriLabel.Location = new System.Drawing.Point(12, 370);
            this.uriLabel.Name = "uriLabel";
            this.uriLabel.Size = new System.Drawing.Size(63, 13);
            this.uriLabel.TabIndex = 27;
            this.uriLabel.Text = "Target URI:";
            this.uriLabel.Visible = false;
            // 
            // targetUriTextbox
            // 
            this.targetUriTextbox.Location = new System.Drawing.Point(12, 386);
            this.targetUriTextbox.Name = "targetUriTextbox";
            this.targetUriTextbox.Size = new System.Drawing.Size(268, 20);
            this.targetUriTextbox.TabIndex = 26;
            this.targetUriTextbox.Visible = false;
            // 
            // XInventoryServiceForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(291, 424);
            this.Controls.Add(this.uriLabel);
            this.Controls.Add(this.targetUriTextbox);
            this.Controls.Add(this.checkBox1);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.numericUpDown1);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.folderIDTextBox);
            this.Controls.Add(this.label555);
            this.Controls.Add(this.creatorDataTextBox);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.invTypeCombo);
            this.Controls.Add(this.assetTypeCombo);
            this.Controls.Add(this.label333);
            this.Controls.Add(this.creatorIDTextBox);
            this.Controls.Add(this.label222);
            this.Controls.Add(this.assetIDTextBox);
            this.Controls.Add(this.itemidlabel);
            this.Controls.Add(this.itemIDTextbox);
            this.Controls.Add(this.label111);
            this.Controls.Add(this.descTextBox);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.nameTextbox);
            this.Controls.Add(this.button1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.HelpButton = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "XInventoryServiceForm";
            this.Text = "New Inventory Item...";
            this.HelpButtonClicked += new System.ComponentModel.CancelEventHandler(this.XInventoryServiceForm_HelpButtonClicked);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.XInventoryServiceForm_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.TextBox nameTextbox;
        private System.Windows.Forms.Label label111;
        private System.Windows.Forms.TextBox descTextBox;
        private System.Windows.Forms.Label itemidlabel;
        private System.Windows.Forms.TextBox itemIDTextbox;
        private System.Windows.Forms.Label label222;
        private System.Windows.Forms.TextBox assetIDTextBox;
        private System.Windows.Forms.Label label333;
        private System.Windows.Forms.TextBox creatorIDTextBox;
        private System.Windows.Forms.ComboBox assetTypeCombo;
        private System.Windows.Forms.ComboBox invTypeCombo;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label555;
        private System.Windows.Forms.TextBox creatorDataTextBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox folderIDTextBox;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.NumericUpDown numericUpDown1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.Label uriLabel;
        private System.Windows.Forms.TextBox targetUriTextbox;
    }
}