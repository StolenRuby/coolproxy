
namespace CoolProxy.Plugins.MagicRez
{
    partial class MagicRezForm
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
            this.label2 = new System.Windows.Forms.Label();
            this.setTargetButton = new System.Windows.Forms.Button();
            this.targetAgentName = new System.Windows.Forms.TextBox();
            this.targetAgentKey = new System.Windows.Forms.TextBox();
            this.enableMagicRez = new System.Windows.Forms.CheckBox();
            this.setParcelOwner = new System.Windows.Forms.Button();
            this.setEstateOwner = new System.Windows.Forms.Button();
            this.changePermsGranter = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(57, 13);
            this.label2.TabIndex = 19;
            this.label2.Text = "Rezzer ID:";
            // 
            // setTargetButton
            // 
            this.setTargetButton.Location = new System.Drawing.Point(184, 23);
            this.setTargetButton.Name = "setTargetButton";
            this.setTargetButton.Size = new System.Drawing.Size(48, 23);
            this.setTargetButton.TabIndex = 0;
            this.setTargetButton.Text = "Set";
            this.setTargetButton.UseVisualStyleBackColor = true;
            this.setTargetButton.Click += new System.EventHandler(this.setTargetButton_Click);
            // 
            // targetAgentName
            // 
            this.targetAgentName.Location = new System.Drawing.Point(12, 25);
            this.targetAgentName.Name = "targetAgentName";
            this.targetAgentName.ReadOnly = true;
            this.targetAgentName.Size = new System.Drawing.Size(166, 20);
            this.targetAgentName.TabIndex = 17;
            this.targetAgentName.Text = "(no target)";
            // 
            // targetAgentKey
            // 
            this.targetAgentKey.Location = new System.Drawing.Point(12, 51);
            this.targetAgentKey.Name = "targetAgentKey";
            this.targetAgentKey.ReadOnly = true;
            this.targetAgentKey.Size = new System.Drawing.Size(220, 20);
            this.targetAgentKey.TabIndex = 16;
            this.targetAgentKey.Text = "00000000-0000-0000-0000-000000000000";
            // 
            // enableMagicRez
            // 
            this.enableMagicRez.Appearance = System.Windows.Forms.Appearance.Button;
            this.enableMagicRez.Enabled = false;
            this.enableMagicRez.Location = new System.Drawing.Point(12, 135);
            this.enableMagicRez.Name = "enableMagicRez";
            this.enableMagicRez.Size = new System.Drawing.Size(220, 24);
            this.enableMagicRez.TabIndex = 3;
            this.enableMagicRez.Text = "Enable Magic Rez";
            this.enableMagicRez.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.enableMagicRez.UseVisualStyleBackColor = true;
            this.enableMagicRez.CheckedChanged += new System.EventHandler(this.enableMagicRez_CheckedChanged);
            // 
            // setParcelOwner
            // 
            this.setParcelOwner.Location = new System.Drawing.Point(12, 77);
            this.setParcelOwner.Name = "setParcelOwner";
            this.setParcelOwner.Size = new System.Drawing.Size(105, 23);
            this.setParcelOwner.TabIndex = 21;
            this.setParcelOwner.Text = "Parcel Owner";
            this.setParcelOwner.UseVisualStyleBackColor = true;
            this.setParcelOwner.Click += new System.EventHandler(this.setParcelOwner_Click);
            // 
            // setEstateOwner
            // 
            this.setEstateOwner.Location = new System.Drawing.Point(127, 77);
            this.setEstateOwner.Name = "setEstateOwner";
            this.setEstateOwner.Size = new System.Drawing.Size(105, 23);
            this.setEstateOwner.TabIndex = 1;
            this.setEstateOwner.Text = "Estate Owner";
            this.setEstateOwner.UseVisualStyleBackColor = true;
            this.setEstateOwner.Click += new System.EventHandler(this.setEstateOwner_Click);
            // 
            // changePermsGranter
            // 
            this.changePermsGranter.AutoSize = true;
            this.changePermsGranter.Location = new System.Drawing.Point(27, 110);
            this.changePermsGranter.Name = "changePermsGranter";
            this.changePermsGranter.Size = new System.Drawing.Size(154, 17);
            this.changePermsGranter.TabIndex = 2;
            this.changePermsGranter.Text = "Change Permission Granter";
            this.changePermsGranter.UseVisualStyleBackColor = true;
            // 
            // MagicRezForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(246, 169);
            this.Controls.Add(this.changePermsGranter);
            this.Controls.Add(this.setEstateOwner);
            this.Controls.Add(this.setParcelOwner);
            this.Controls.Add(this.enableMagicRez);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.setTargetButton);
            this.Controls.Add(this.targetAgentName);
            this.Controls.Add(this.targetAgentKey);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MagicRezForm";
            this.ShowIcon = false;
            this.Text = "Magic Rez";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button setTargetButton;
        private System.Windows.Forms.TextBox targetAgentName;
        private System.Windows.Forms.TextBox targetAgentKey;
        private System.Windows.Forms.CheckBox enableMagicRez;
        private System.Windows.Forms.Button setParcelOwner;
        private System.Windows.Forms.Button setEstateOwner;
        private System.Windows.Forms.CheckBox changePermsGranter;
    }
}