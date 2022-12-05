namespace CoolProxy.Plugins.InventoryBrowser
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
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.expandAllFoldersToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.collapseAllFoldersToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.emptyTrashToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.createToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newFolderToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newScriptToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newNotecardToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newGestureToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.filterToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.inventoryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.button2 = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.inventoryBrowser = new CoolProxy.Plugins.InventoryBrowser.InventoryBrowser();
            this.folderContextMenuStrip.SuspendLayout();
            this.assetContextMenuStrip.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.panel1.SuspendLayout();
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
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem1,
            this.createToolStripMenuItem,
            this.filterToolStripMenuItem,
            this.inventoryToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.menuStrip1.Size = new System.Drawing.Size(274, 24);
            this.menuStrip1.TabIndex = 3;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.expandAllFoldersToolStripMenuItem,
            this.collapseAllFoldersToolStripMenuItem,
            this.toolStripSeparator2,
            this.emptyTrashToolStripMenuItem});
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(69, 20);
            this.toolStripMenuItem1.Text = "Inventory";
            // 
            // expandAllFoldersToolStripMenuItem
            // 
            this.expandAllFoldersToolStripMenuItem.Name = "expandAllFoldersToolStripMenuItem";
            this.expandAllFoldersToolStripMenuItem.Size = new System.Drawing.Size(177, 22);
            this.expandAllFoldersToolStripMenuItem.Text = "Expand All Folders";
            this.expandAllFoldersToolStripMenuItem.Click += new System.EventHandler(this.expandAllFoldersToolStripMenuItem_Click);
            // 
            // collapseAllFoldersToolStripMenuItem
            // 
            this.collapseAllFoldersToolStripMenuItem.Name = "collapseAllFoldersToolStripMenuItem";
            this.collapseAllFoldersToolStripMenuItem.Size = new System.Drawing.Size(177, 22);
            this.collapseAllFoldersToolStripMenuItem.Text = "Collapse All Folders";
            this.collapseAllFoldersToolStripMenuItem.Click += new System.EventHandler(this.collapseAllFoldersToolStripMenuItem_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(174, 6);
            // 
            // emptyTrashToolStripMenuItem
            // 
            this.emptyTrashToolStripMenuItem.Enabled = false;
            this.emptyTrashToolStripMenuItem.Name = "emptyTrashToolStripMenuItem";
            this.emptyTrashToolStripMenuItem.Size = new System.Drawing.Size(177, 22);
            this.emptyTrashToolStripMenuItem.Text = "Empty Trash";
            this.emptyTrashToolStripMenuItem.Click += new System.EventHandler(this.emptyTrashToolStripMenuItem_Click);
            // 
            // createToolStripMenuItem
            // 
            this.createToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newFolderToolStripMenuItem,
            this.newScriptToolStripMenuItem,
            this.newNotecardToolStripMenuItem,
            this.newGestureToolStripMenuItem});
            this.createToolStripMenuItem.Name = "createToolStripMenuItem";
            this.createToolStripMenuItem.Size = new System.Drawing.Size(53, 20);
            this.createToolStripMenuItem.Text = "Create";
            // 
            // newFolderToolStripMenuItem
            // 
            this.newFolderToolStripMenuItem.Name = "newFolderToolStripMenuItem";
            this.newFolderToolStripMenuItem.Size = new System.Drawing.Size(141, 22);
            this.newFolderToolStripMenuItem.Text = "New Folder";
            this.newFolderToolStripMenuItem.Click += new System.EventHandler(this.newFolderToolStripMenuItem_Click);
            // 
            // newScriptToolStripMenuItem
            // 
            this.newScriptToolStripMenuItem.Name = "newScriptToolStripMenuItem";
            this.newScriptToolStripMenuItem.Size = new System.Drawing.Size(141, 22);
            this.newScriptToolStripMenuItem.Text = "New Script";
            this.newScriptToolStripMenuItem.Click += new System.EventHandler(this.newScriptToolStripMenuItem_Click);
            // 
            // newNotecardToolStripMenuItem
            // 
            this.newNotecardToolStripMenuItem.Name = "newNotecardToolStripMenuItem";
            this.newNotecardToolStripMenuItem.Size = new System.Drawing.Size(141, 22);
            this.newNotecardToolStripMenuItem.Text = "New Note";
            this.newNotecardToolStripMenuItem.Click += new System.EventHandler(this.newNotecardToolStripMenuItem_Click);
            // 
            // newGestureToolStripMenuItem
            // 
            this.newGestureToolStripMenuItem.Name = "newGestureToolStripMenuItem";
            this.newGestureToolStripMenuItem.Size = new System.Drawing.Size(141, 22);
            this.newGestureToolStripMenuItem.Text = "New Gesture";
            this.newGestureToolStripMenuItem.Click += new System.EventHandler(this.newGestureToolStripMenuItem_Click);
            // 
            // filterToolStripMenuItem
            // 
            this.filterToolStripMenuItem.Name = "filterToolStripMenuItem";
            this.filterToolStripMenuItem.Size = new System.Drawing.Size(45, 20);
            this.filterToolStripMenuItem.Text = "Filter";
            // 
            // inventoryToolStripMenuItem
            // 
            this.inventoryToolStripMenuItem.Name = "inventoryToolStripMenuItem";
            this.inventoryToolStripMenuItem.Size = new System.Drawing.Size(54, 20);
            this.inventoryToolStripMenuItem.Text = "Search";
            // 
            // textBox1
            // 
            this.textBox1.Dock = System.Windows.Forms.DockStyle.Top;
            this.textBox1.Location = new System.Drawing.Point(0, 0);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(274, 20);
            this.textBox1.TabIndex = 0;
            this.textBox1.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.comboBox1);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.button2);
            this.panel1.Controls.Add(this.button1);
            this.panel1.Controls.Add(this.textBox1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 24);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(274, 49);
            this.panel1.TabIndex = 4;
            // 
            // comboBox1
            // 
            this.comboBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Location = new System.Drawing.Point(149, 24);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(121, 21);
            this.comboBox1.TabIndex = 4;
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(114, 27);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(32, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Filter:";
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(30, 22);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(26, 23);
            this.button2.TabIndex = 2;
            this.button2.Text = "+";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.expandAllFoldersToolStripMenuItem_Click);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(3, 22);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(26, 23);
            this.button1.TabIndex = 1;
            this.button1.Text = "-";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.collapseAllFoldersToolStripMenuItem_Click);
            // 
            // inventoryBrowser
            // 
            this.inventoryBrowser.AutoScroll = true;
            this.inventoryBrowser.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(39)))), ((int)(((byte)(39)))), ((int)(((byte)(39)))));
            this.inventoryBrowser.Dock = System.Windows.Forms.DockStyle.Fill;
            this.inventoryBrowser.ImageList = this.inventoryImageList;
            this.inventoryBrowser.Location = new System.Drawing.Point(0, 73);
            this.inventoryBrowser.Name = "inventoryBrowser";
            this.inventoryBrowser.Proxy = null;
            this.inventoryBrowser.SelectionColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(119)))), ((int)(((byte)(164)))));
            this.inventoryBrowser.Size = new System.Drawing.Size(274, 419);
            this.inventoryBrowser.TabIndex = 5;
            // 
            // InventoryBrowserForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(274, 492);
            this.Controls.Add(this.inventoryBrowser);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.menuStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.MinimumSize = new System.Drawing.Size(275, 400);
            this.Name = "InventoryBrowserForm";
            this.Text = "Inventory";
            this.folderContextMenuStrip.ResumeLayout(false);
            this.assetContextMenuStrip.ResumeLayout(false);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

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
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem inventoryToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem createToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem filterToolStripMenuItem;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.ToolStripMenuItem newFolderToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem newScriptToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem newNotecardToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem expandAllFoldersToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem collapseAllFoldersToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem emptyTrashToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem newGestureToolStripMenuItem;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private CoolProxy.Plugins.InventoryBrowser.InventoryBrowser inventoryBrowser;
    }
}