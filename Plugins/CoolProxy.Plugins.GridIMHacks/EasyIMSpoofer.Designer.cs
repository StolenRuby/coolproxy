
namespace CoolProxy.Plugins.GridIMHacks
{
    partial class EasyIMSpoofer
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
            this.sendMessageButton = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.setRecipientButton = new System.Windows.Forms.Button();
            this.recipientAgentName = new System.Windows.Forms.TextBox();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.setTargetButton = new System.Windows.Forms.Button();
            this.targetAgentName = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // sendMessageButton
            // 
            this.sendMessageButton.Location = new System.Drawing.Point(86, 95);
            this.sendMessageButton.Name = "sendMessageButton";
            this.sendMessageButton.Size = new System.Drawing.Size(132, 23);
            this.sendMessageButton.TabIndex = 44;
            this.sendMessageButton.Text = "Send Message";
            this.sendMessageButton.UseVisualStyleBackColor = true;
            this.sendMessageButton.Click += new System.EventHandler(this.sendMessageButton_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(14, 68);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(53, 13);
            this.label3.TabIndex = 52;
            this.label3.Text = "Message:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 42);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(55, 13);
            this.label1.TabIndex = 51;
            this.label1.Text = "Recipient:";
            // 
            // setRecipientButton
            // 
            this.setRecipientButton.Location = new System.Drawing.Point(245, 37);
            this.setRecipientButton.Name = "setRecipientButton";
            this.setRecipientButton.Size = new System.Drawing.Size(48, 23);
            this.setRecipientButton.TabIndex = 49;
            this.setRecipientButton.Text = "Set";
            this.setRecipientButton.UseVisualStyleBackColor = true;
            this.setRecipientButton.Click += new System.EventHandler(this.setRecipientButton_Click);
            // 
            // recipientAgentName
            // 
            this.recipientAgentName.Location = new System.Drawing.Point(73, 39);
            this.recipientAgentName.Name = "recipientAgentName";
            this.recipientAgentName.ReadOnly = true;
            this.recipientAgentName.Size = new System.Drawing.Size(166, 20);
            this.recipientAgentName.TabIndex = 50;
            this.recipientAgentName.Text = "(no target)";
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(73, 65);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(220, 20);
            this.textBox1.TabIndex = 48;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(22, 16);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(44, 13);
            this.label2.TabIndex = 47;
            this.label2.Text = "Sender:";
            // 
            // setTargetButton
            // 
            this.setTargetButton.Location = new System.Drawing.Point(245, 11);
            this.setTargetButton.Name = "setTargetButton";
            this.setTargetButton.Size = new System.Drawing.Size(48, 23);
            this.setTargetButton.TabIndex = 45;
            this.setTargetButton.Text = "Set";
            this.setTargetButton.UseVisualStyleBackColor = true;
            this.setTargetButton.Click += new System.EventHandler(this.setTargetButton_Click);
            // 
            // targetAgentName
            // 
            this.targetAgentName.Location = new System.Drawing.Point(73, 13);
            this.targetAgentName.Name = "targetAgentName";
            this.targetAgentName.ReadOnly = true;
            this.targetAgentName.Size = new System.Drawing.Size(166, 20);
            this.targetAgentName.TabIndex = 46;
            this.targetAgentName.Text = "(no target)";
            // 
            // EasyIMSpoofer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(305, 129);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.setRecipientButton);
            this.Controls.Add(this.recipientAgentName);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.setTargetButton);
            this.Controls.Add(this.targetAgentName);
            this.Controls.Add(this.sendMessageButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "EasyIMSpoofer";
            this.ShowIcon = false;
            this.Text = "Easy IM Spoofer";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button sendMessageButton;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button setRecipientButton;
        private System.Windows.Forms.TextBox recipientAgentName;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button setTargetButton;
        private System.Windows.Forms.TextBox targetAgentName;
    }
}