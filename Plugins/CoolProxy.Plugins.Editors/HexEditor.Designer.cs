
namespace CoolProxy.Plugins.Editors
{
    partial class HexEditor
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
            this.hexBoxRequest = new Be.Windows.Forms.HexBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.button1 = new System.Windows.Forms.Button();
            this.checkBox3 = new CoolGUI.Controls.CheckBox();
            this.checkBox2 = new CoolGUI.Controls.CheckBox();
            this.checkBox1 = new CoolGUI.Controls.CheckBox();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // hexBoxRequest
            // 
            this.hexBoxRequest.Dock = System.Windows.Forms.DockStyle.Fill;
            this.hexBoxRequest.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.hexBoxRequest.LineInfoForeColor = System.Drawing.Color.Empty;
            this.hexBoxRequest.LineInfoVisible = true;
            this.hexBoxRequest.Location = new System.Drawing.Point(0, 46);
            this.hexBoxRequest.Name = "hexBoxRequest";
            this.hexBoxRequest.ShadowSelectionColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(60)))), ((int)(((byte)(188)))), ((int)(((byte)(255)))));
            this.hexBoxRequest.Size = new System.Drawing.Size(639, 404);
            this.hexBoxRequest.StringViewVisible = true;
            this.hexBoxRequest.TabIndex = 25;
            this.hexBoxRequest.UseFixedBytesPerLine = true;
            this.hexBoxRequest.VScrollBarVisible = true;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.checkBox3);
            this.panel1.Controls.Add(this.checkBox2);
            this.panel1.Controls.Add(this.checkBox1);
            this.panel1.Controls.Add(this.button1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(639, 46);
            this.panel1.TabIndex = 26;
            // 
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button1.Location = new System.Drawing.Point(533, 12);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(92, 23);
            this.button1.TabIndex = 0;
            this.button1.Text = "Upload";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // checkBox3
            // 
            this.checkBox3.AutoSize = true;
            this.checkBox3.EnabledSetting = null;
            this.checkBox3.Location = new System.Drawing.Point(321, 16);
            this.checkBox3.Name = "checkBox3";
            this.checkBox3.Setting = "EditorsChangePermissions";
            this.checkBox3.Size = new System.Drawing.Size(69, 17);
            this.checkBox3.TabIndex = 3;
            this.checkBox3.Text = "Full Perm";
            this.checkBox3.UseVisualStyleBackColor = true;
            // 
            // checkBox2
            // 
            this.checkBox2.AutoSize = true;
            this.checkBox2.EnabledSetting = null;
            this.checkBox2.Location = new System.Drawing.Point(173, 16);
            this.checkBox2.Name = "checkBox2";
            this.checkBox2.Setting = "EditorsChangeCreationDate";
            this.checkBox2.Size = new System.Drawing.Size(129, 17);
            this.checkBox2.TabIndex = 2;
            this.checkBox2.Text = "Update Creation Date";
            this.checkBox2.UseVisualStyleBackColor = true;
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.EnabledSetting = null;
            this.checkBox1.Location = new System.Drawing.Point(19, 16);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Setting = "EditorsChangeCreator";
            this.checkBox1.Size = new System.Drawing.Size(130, 17);
            this.checkBox1.TabIndex = 1;
            this.checkBox1.Text = "Change Creator to Me";
            this.checkBox1.UseVisualStyleBackColor = true;
            // 
            // HexEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(639, 450);
            this.Controls.Add(this.hexBoxRequest);
            this.Controls.Add(this.panel1);
            this.Name = "HexEditor";
            this.Text = "HexEditor";
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private Be.Windows.Forms.HexBox hexBoxRequest;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button button1;
        private CoolGUI.Controls.CheckBox checkBox2;
        private CoolGUI.Controls.CheckBox checkBox1;
        private CoolGUI.Controls.CheckBox checkBox3;
    }
}