
namespace CoolProxy.Plugins.InventoryBrowser
{
    partial class InventoryBrowser
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.innerInventoryBrowserControl1 = new CoolProxy.Plugins.InventoryBrowser.InventoryBrowser.InnerInventoryBrowserControl();
            this.SuspendLayout();
            // 
            // innerInventoryBrowserControl1
            // 
            this.innerInventoryBrowserControl1.Folder = null;
            this.innerInventoryBrowserControl1.ImageList = null;
            this.innerInventoryBrowserControl1.Location = new System.Drawing.Point(0, 0);
            this.innerInventoryBrowserControl1.Name = "innerInventoryBrowserControl1";
            this.innerInventoryBrowserControl1.Size = new System.Drawing.Size(50, 50);
            this.innerInventoryBrowserControl1.TabIndex = 0;
            this.innerInventoryBrowserControl1.TabStop = false;
            // 
            // InvTest
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.Controls.Add(this.innerInventoryBrowserControl1);
            this.Name = "InvTest";
            this.Size = new System.Drawing.Size(286, 442);
            this.ResumeLayout(false);

        }

        #endregion
        private InnerInventoryBrowserControl innerInventoryBrowserControl1;
    }
}
