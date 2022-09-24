
namespace CoolProxy.Plugins.GridIMHacks
{
    partial class SpecialTeleportForm
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
            this.sendTeleportButton = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.setRegionButton = new System.Windows.Forms.Button();
            this.targetRegionName = new System.Windows.Forms.TextBox();
            this.numericUpDown3 = new System.Windows.Forms.NumericUpDown();
            this.numericUpDown2 = new System.Windows.Forms.NumericUpDown();
            this.numericUpDown1 = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.setTargetButton = new System.Windows.Forms.Button();
            this.targetAgentName = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).BeginInit();
            this.SuspendLayout();
            // 
            // sendTeleportButton
            // 
            this.sendTeleportButton.Location = new System.Drawing.Point(87, 94);
            this.sendTeleportButton.Name = "sendTeleportButton";
            this.sendTeleportButton.Size = new System.Drawing.Size(127, 23);
            this.sendTeleportButton.TabIndex = 31;
            this.sendTeleportButton.Text = "Teleport";
            this.sendTeleportButton.UseVisualStyleBackColor = true;
            this.sendTeleportButton.Click += new System.EventHandler(this.sendTeleportButton_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(14, 65);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(47, 13);
            this.label3.TabIndex = 41;
            this.label3.Text = "Position:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(17, 40);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(44, 13);
            this.label1.TabIndex = 40;
            this.label1.Text = "Region:";
            // 
            // setRegionButton
            // 
            this.setRegionButton.Location = new System.Drawing.Point(239, 35);
            this.setRegionButton.Name = "setRegionButton";
            this.setRegionButton.Size = new System.Drawing.Size(48, 23);
            this.setRegionButton.TabIndex = 38;
            this.setRegionButton.Text = "Set";
            this.setRegionButton.UseVisualStyleBackColor = true;
            this.setRegionButton.Click += new System.EventHandler(this.setRegionButton_Click);
            // 
            // targetRegionName
            // 
            this.targetRegionName.Location = new System.Drawing.Point(67, 37);
            this.targetRegionName.Name = "targetRegionName";
            this.targetRegionName.ReadOnly = true;
            this.targetRegionName.Size = new System.Drawing.Size(166, 20);
            this.targetRegionName.TabIndex = 39;
            this.targetRegionName.Text = "(no target)";
            // 
            // numericUpDown3
            // 
            this.numericUpDown3.DecimalPlaces = 3;
            this.numericUpDown3.Location = new System.Drawing.Point(221, 63);
            this.numericUpDown3.Maximum = new decimal(new int[] {
            2048,
            0,
            0,
            0});
            this.numericUpDown3.Name = "numericUpDown3";
            this.numericUpDown3.Size = new System.Drawing.Size(66, 20);
            this.numericUpDown3.TabIndex = 37;
            this.numericUpDown3.Value = new decimal(new int[] {
            30,
            0,
            0,
            0});
            // 
            // numericUpDown2
            // 
            this.numericUpDown2.DecimalPlaces = 3;
            this.numericUpDown2.Location = new System.Drawing.Point(144, 63);
            this.numericUpDown2.Maximum = new decimal(new int[] {
            2048,
            0,
            0,
            0});
            this.numericUpDown2.Name = "numericUpDown2";
            this.numericUpDown2.Size = new System.Drawing.Size(66, 20);
            this.numericUpDown2.TabIndex = 36;
            this.numericUpDown2.Value = new decimal(new int[] {
            128,
            0,
            0,
            0});
            // 
            // numericUpDown1
            // 
            this.numericUpDown1.DecimalPlaces = 3;
            this.numericUpDown1.Location = new System.Drawing.Point(67, 63);
            this.numericUpDown1.Maximum = new decimal(new int[] {
            2048,
            0,
            0,
            0});
            this.numericUpDown1.Name = "numericUpDown1";
            this.numericUpDown1.Size = new System.Drawing.Size(66, 20);
            this.numericUpDown1.TabIndex = 35;
            this.numericUpDown1.Value = new decimal(new int[] {
            128,
            0,
            0,
            0});
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(20, 14);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(41, 13);
            this.label2.TabIndex = 34;
            this.label2.Text = "Avatar:";
            // 
            // setTargetButton
            // 
            this.setTargetButton.Location = new System.Drawing.Point(239, 9);
            this.setTargetButton.Name = "setTargetButton";
            this.setTargetButton.Size = new System.Drawing.Size(48, 23);
            this.setTargetButton.TabIndex = 32;
            this.setTargetButton.Text = "Set";
            this.setTargetButton.UseVisualStyleBackColor = true;
            this.setTargetButton.Click += new System.EventHandler(this.setTargetButton_Click);
            // 
            // targetAgentName
            // 
            this.targetAgentName.Location = new System.Drawing.Point(67, 11);
            this.targetAgentName.Name = "targetAgentName";
            this.targetAgentName.ReadOnly = true;
            this.targetAgentName.Size = new System.Drawing.Size(166, 20);
            this.targetAgentName.TabIndex = 33;
            this.targetAgentName.Text = "(no target)";
            // 
            // SpecialTeleportForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(301, 130);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.setRegionButton);
            this.Controls.Add(this.targetRegionName);
            this.Controls.Add(this.numericUpDown3);
            this.Controls.Add(this.numericUpDown2);
            this.Controls.Add(this.numericUpDown1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.setTargetButton);
            this.Controls.Add(this.targetAgentName);
            this.Controls.Add(this.sendTeleportButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SpecialTeleportForm";
            this.ShowIcon = false;
            this.Text = "Special Teleport";
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button sendTeleportButton;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button setRegionButton;
        private System.Windows.Forms.TextBox targetRegionName;
        private System.Windows.Forms.NumericUpDown numericUpDown3;
        private System.Windows.Forms.NumericUpDown numericUpDown2;
        private System.Windows.Forms.NumericUpDown numericUpDown1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button setTargetButton;
        private System.Windows.Forms.TextBox targetAgentName;
    }
}