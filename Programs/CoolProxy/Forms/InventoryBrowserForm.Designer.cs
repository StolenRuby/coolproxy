namespace CoolProxy
{
    partial class InventoryBrowserForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(InventoryBrowserForm));
            this.folderContextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.saveAsToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.copyFolderIDToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.uploadItemHereToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.assetContextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.saveAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.copyItemIDToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.copyAssetIDToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.inventoryImageList = new System.Windows.Forms.ImageList(this.components);
            this.inventoryTree1 = new CoolGUI.Controls.InventoryTree();
            this.folderContextMenuStrip.SuspendLayout();
            this.assetContextMenuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // folderContextMenuStrip
            // 
            this.folderContextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.saveAsToolStripMenuItem1,
            this.copyFolderIDToolStripMenuItem,
            this.toolStripSeparator1,
            this.uploadItemHereToolStripMenuItem});
            this.folderContextMenuStrip.Name = "folderContextMenuStrip";
            this.folderContextMenuStrip.Size = new System.Drawing.Size(168, 76);
            // 
            // saveAsToolStripMenuItem1
            // 
            this.saveAsToolStripMenuItem1.Name = "saveAsToolStripMenuItem1";
            this.saveAsToolStripMenuItem1.Size = new System.Drawing.Size(167, 22);
            this.saveAsToolStripMenuItem1.Text = "Save As...";
            // 
            // copyFolderIDToolStripMenuItem
            // 
            this.copyFolderIDToolStripMenuItem.Name = "copyFolderIDToolStripMenuItem";
            this.copyFolderIDToolStripMenuItem.Size = new System.Drawing.Size(167, 22);
            this.copyFolderIDToolStripMenuItem.Text = "Copy Folder ID";
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(164, 6);
            // 
            // uploadItemHereToolStripMenuItem
            // 
            this.uploadItemHereToolStripMenuItem.Name = "uploadItemHereToolStripMenuItem";
            this.uploadItemHereToolStripMenuItem.Size = new System.Drawing.Size(167, 22);
            this.uploadItemHereToolStripMenuItem.Text = "Upload Item Here";
            // 
            // assetContextMenuStrip
            // 
            this.assetContextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.saveAsToolStripMenuItem,
            this.copyItemIDToolStripMenuItem,
            this.copyAssetIDToolStripMenuItem});
            this.assetContextMenuStrip.Name = "assetContextMenuStrip";
            this.assetContextMenuStrip.Size = new System.Drawing.Size(148, 70);
            // 
            // saveAsToolStripMenuItem
            // 
            this.saveAsToolStripMenuItem.Name = "saveAsToolStripMenuItem";
            this.saveAsToolStripMenuItem.Size = new System.Drawing.Size(147, 22);
            this.saveAsToolStripMenuItem.Text = "Save As...";
            // 
            // copyItemIDToolStripMenuItem
            // 
            this.copyItemIDToolStripMenuItem.Name = "copyItemIDToolStripMenuItem";
            this.copyItemIDToolStripMenuItem.Size = new System.Drawing.Size(147, 22);
            this.copyItemIDToolStripMenuItem.Text = "Copy Item ID";
            // 
            // copyAssetIDToolStripMenuItem
            // 
            this.copyAssetIDToolStripMenuItem.Name = "copyAssetIDToolStripMenuItem";
            this.copyAssetIDToolStripMenuItem.Size = new System.Drawing.Size(147, 22);
            this.copyAssetIDToolStripMenuItem.Text = "Copy Asset ID";
            // 
            // inventoryImageList
            // 
            this.inventoryImageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("inventoryImageList.ImageStream")));
            this.inventoryImageList.TransparentColor = System.Drawing.Color.Transparent;
            this.inventoryImageList.Images.SetKeyName(0, "Inv_Alpha.png");
            this.inventoryImageList.Images.SetKeyName(1, "Inv_Animation.png");
            this.inventoryImageList.Images.SetKeyName(2, "Inv_BodyShape.png");
            this.inventoryImageList.Images.SetKeyName(3, "Inv_CallingCard.png");
            this.inventoryImageList.Images.SetKeyName(4, "Inv_Clothing.png");
            this.inventoryImageList.Images.SetKeyName(5, "Inv_Eye.png");
            this.inventoryImageList.Images.SetKeyName(6, "Inv_FolderClosed.png");
            this.inventoryImageList.Images.SetKeyName(7, "Inv_FolderOpen.png");
            this.inventoryImageList.Images.SetKeyName(8, "Inv_FolderClosedToxic.png");
            this.inventoryImageList.Images.SetKeyName(9, "Inv_FolderOpenToxic.png");
            this.inventoryImageList.Images.SetKeyName(10, "Inv_Gesture.png");
            this.inventoryImageList.Images.SetKeyName(11, "Inv_Gloves.png");
            this.inventoryImageList.Images.SetKeyName(12, "Inv_Hair.png");
            this.inventoryImageList.Images.SetKeyName(13, "Inv_Invalid.png");
            this.inventoryImageList.Images.SetKeyName(14, "Inv_Jacket.png");
            this.inventoryImageList.Images.SetKeyName(15, "Inv_Landmark.png");
            this.inventoryImageList.Images.SetKeyName(16, "Inv_Link.png");
            this.inventoryImageList.Images.SetKeyName(17, "Inv_LinkFolder.png");
            this.inventoryImageList.Images.SetKeyName(18, "Inv_LinkItem.png");
            this.inventoryImageList.Images.SetKeyName(19, "Inv_LookFolderClosed.png");
            this.inventoryImageList.Images.SetKeyName(20, "Inv_LookFolderOpen.png");
            this.inventoryImageList.Images.SetKeyName(21, "Inv_LostClosed.png");
            this.inventoryImageList.Images.SetKeyName(22, "Inv_LostOpen.png");
            this.inventoryImageList.Images.SetKeyName(23, "Inv_Mesh.png");
            this.inventoryImageList.Images.SetKeyName(24, "Inv_Notecard.png");
            this.inventoryImageList.Images.SetKeyName(25, "Inv_Object.png");
            this.inventoryImageList.Images.SetKeyName(26, "Inv_Object_Multi.png");
            this.inventoryImageList.Images.SetKeyName(27, "Inv_Pants.png");
            this.inventoryImageList.Images.SetKeyName(28, "Inv_Physics.png");
            this.inventoryImageList.Images.SetKeyName(29, "Inv_Script.png");
            this.inventoryImageList.Images.SetKeyName(30, "Inv_Shirt.png");
            this.inventoryImageList.Images.SetKeyName(31, "Inv_Shoe.png");
            this.inventoryImageList.Images.SetKeyName(32, "Inv_Skin.png");
            this.inventoryImageList.Images.SetKeyName(33, "Inv_Skirt.png");
            this.inventoryImageList.Images.SetKeyName(34, "Inv_Snapshot.png");
            this.inventoryImageList.Images.SetKeyName(35, "Inv_Socks.png");
            this.inventoryImageList.Images.SetKeyName(36, "Inv_Sound.png");
            this.inventoryImageList.Images.SetKeyName(37, "Inv_StockFolderClosed.png");
            this.inventoryImageList.Images.SetKeyName(38, "Inv_StockFolderOpen.png");
            this.inventoryImageList.Images.SetKeyName(39, "Inv_SysClosed.png");
            this.inventoryImageList.Images.SetKeyName(40, "Inv_SysOpen.png");
            this.inventoryImageList.Images.SetKeyName(41, "Inv_Tattoo.png");
            this.inventoryImageList.Images.SetKeyName(42, "Inv_Texture.png");
            this.inventoryImageList.Images.SetKeyName(43, "Inv_TrashClosed.png");
            this.inventoryImageList.Images.SetKeyName(44, "Inv_TrashOpen.png");
            this.inventoryImageList.Images.SetKeyName(45, "Inv_Underpants.png");
            this.inventoryImageList.Images.SetKeyName(46, "Inv_Undershirt.png");
            this.inventoryImageList.Images.SetKeyName(47, "Inv_VersionFolderClosed.png");
            this.inventoryImageList.Images.SetKeyName(48, "Inv_VersionFolderOpen.png");
            this.inventoryImageList.Images.SetKeyName(49, "Inv_SettingsDay.png");
            this.inventoryImageList.Images.SetKeyName(50, "Inv_SettingsSky.png");
            this.inventoryImageList.Images.SetKeyName(51, "Inv_SettingsWater.png");
            // 
            // inventoryTree1
            // 
            this.inventoryTree1.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.inventoryTree1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.inventoryTree1.Frame = null;
            this.inventoryTree1.ImageIndex = 0;
            this.inventoryTree1.ImageList = this.inventoryImageList;
            this.inventoryTree1.Location = new System.Drawing.Point(0, 0);
            this.inventoryTree1.Name = "inventoryTree1";
            this.inventoryTree1.SelectedImageIndex = 0;
            this.inventoryTree1.Size = new System.Drawing.Size(274, 492);
            this.inventoryTree1.TabIndex = 2;
            // 
            // InventoryBrowserForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(274, 492);
            this.Controls.Add(this.inventoryTree1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "InventoryBrowserForm";
            this.Text = "Inventory";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.InventoryBrowserForm_FormClosing);
            this.folderContextMenuStrip.ResumeLayout(false);
            this.assetContextMenuStrip.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.ImageList inventoryImageList;
        private System.Windows.Forms.ContextMenuStrip folderContextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem saveAsToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem copyFolderIDToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip assetContextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem saveAsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem copyItemIDToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem copyAssetIDToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem uploadItemHereToolStripMenuItem;
        private CoolGUI.Controls.InventoryTree inventoryTree1;
    }
}