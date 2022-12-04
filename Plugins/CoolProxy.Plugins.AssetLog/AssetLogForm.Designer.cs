
namespace CoolProxy.Plugins.AssetLog
{
    partial class AssetLogForm
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle5 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle6 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle7 = new System.Windows.Forms.DataGridViewCellStyle();
            this.tabControl4 = new System.Windows.Forms.TabControl();
            this.tabPage18 = new System.Windows.Forms.TabPage();
            this.animsDataGridView = new System.Windows.Forms.DataGridView();
            this.Column5 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.animOwnerColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column3 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column4 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tabPage19 = new System.Windows.Forms.TabPage();
            this.soundsDataGridView = new System.Windows.Forms.DataGridView();
            this.soundTypeColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.soundKeyColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.soundOwnerColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.blacklistTab = new System.Windows.Forms.TabPage();
            this.blacklistDataGridView = new System.Windows.Forms.DataGridView();
            this.blacklistKeyColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.blacklistDateColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.panel3 = new System.Windows.Forms.Panel();
            this.button37 = new System.Windows.Forms.Button();
            this.label25 = new System.Windows.Forms.Label();
            this.button31 = new System.Windows.Forms.Button();
            this.textBox18 = new System.Windows.Forms.TextBox();
            this.blacklistContextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.clearBlacklistToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.saveXMLToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loadXMLToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.soundsListContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.playLocallyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.playInworldToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator9 = new System.Windows.Forms.ToolStripSeparator();
            this.blacklistToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tabControl4.SuspendLayout();
            this.tabPage18.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.animsDataGridView)).BeginInit();
            this.tabPage19.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.soundsDataGridView)).BeginInit();
            this.blacklistTab.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.blacklistDataGridView)).BeginInit();
            this.panel3.SuspendLayout();
            this.blacklistContextMenuStrip.SuspendLayout();
            this.soundsListContextMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl4
            // 
            this.tabControl4.Controls.Add(this.tabPage18);
            this.tabControl4.Controls.Add(this.tabPage19);
            this.tabControl4.Controls.Add(this.blacklistTab);
            this.tabControl4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl4.Location = new System.Drawing.Point(0, 0);
            this.tabControl4.Name = "tabControl4";
            this.tabControl4.SelectedIndex = 0;
            this.tabControl4.Size = new System.Drawing.Size(462, 301);
            this.tabControl4.TabIndex = 1;
            // 
            // tabPage18
            // 
            this.tabPage18.Controls.Add(this.animsDataGridView);
            this.tabPage18.Location = new System.Drawing.Point(4, 22);
            this.tabPage18.Name = "tabPage18";
            this.tabPage18.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage18.Size = new System.Drawing.Size(454, 275);
            this.tabPage18.TabIndex = 0;
            this.tabPage18.Text = "Animations";
            this.tabPage18.UseVisualStyleBackColor = true;
            // 
            // animsDataGridView
            // 
            this.animsDataGridView.AllowUserToAddRows = false;
            this.animsDataGridView.AllowUserToDeleteRows = false;
            this.animsDataGridView.AllowUserToResizeRows = false;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.Silver;
            this.animsDataGridView.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            this.animsDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.animsDataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Column5,
            this.animOwnerColumn,
            this.Column3,
            this.Column4});
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.ControlDark;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.animsDataGridView.DefaultCellStyle = dataGridViewCellStyle2;
            this.animsDataGridView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.animsDataGridView.Location = new System.Drawing.Point(3, 3);
            this.animsDataGridView.Name = "animsDataGridView";
            this.animsDataGridView.ReadOnly = true;
            this.animsDataGridView.RowHeadersVisible = false;
            this.animsDataGridView.RowTemplate.Height = 18;
            this.animsDataGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.animsDataGridView.Size = new System.Drawing.Size(448, 269);
            this.animsDataGridView.TabIndex = 2;
            this.animsDataGridView.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.animsDataGridView_CellDoubleClick);
            // 
            // Column5
            // 
            this.Column5.HeaderText = "agent_id";
            this.Column5.Name = "Column5";
            this.Column5.ReadOnly = true;
            this.Column5.Visible = false;
            // 
            // animOwnerColumn
            // 
            this.animOwnerColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.animOwnerColumn.HeaderText = "Avatar";
            this.animOwnerColumn.Name = "animOwnerColumn";
            this.animOwnerColumn.ReadOnly = true;
            // 
            // Column3
            // 
            this.Column3.HeaderText = "Playing";
            this.Column3.Name = "Column3";
            this.Column3.ReadOnly = true;
            this.Column3.Width = 75;
            // 
            // Column4
            // 
            this.Column4.HeaderText = "Logged";
            this.Column4.Name = "Column4";
            this.Column4.ReadOnly = true;
            this.Column4.Width = 75;
            // 
            // tabPage19
            // 
            this.tabPage19.Controls.Add(this.soundsDataGridView);
            this.tabPage19.Location = new System.Drawing.Point(4, 22);
            this.tabPage19.Name = "tabPage19";
            this.tabPage19.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage19.Size = new System.Drawing.Size(454, 275);
            this.tabPage19.TabIndex = 1;
            this.tabPage19.Text = "Sounds";
            this.tabPage19.UseVisualStyleBackColor = true;
            // 
            // soundsDataGridView
            // 
            this.soundsDataGridView.AllowUserToAddRows = false;
            this.soundsDataGridView.AllowUserToDeleteRows = false;
            this.soundsDataGridView.AllowUserToResizeRows = false;
            dataGridViewCellStyle3.BackColor = System.Drawing.Color.Silver;
            this.soundsDataGridView.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle3;
            this.soundsDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.soundsDataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.soundTypeColumn,
            this.soundKeyColumn,
            this.soundOwnerColumn});
            this.soundsDataGridView.ContextMenuStrip = this.soundsListContextMenu;
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle4.BackColor = System.Drawing.SystemColors.ControlDark;
            dataGridViewCellStyle4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle4.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle4.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle4.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle4.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.soundsDataGridView.DefaultCellStyle = dataGridViewCellStyle4;
            this.soundsDataGridView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.soundsDataGridView.Location = new System.Drawing.Point(3, 3);
            this.soundsDataGridView.Name = "soundsDataGridView";
            this.soundsDataGridView.ReadOnly = true;
            this.soundsDataGridView.RowHeadersVisible = false;
            this.soundsDataGridView.RowTemplate.Height = 18;
            this.soundsDataGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.soundsDataGridView.Size = new System.Drawing.Size(448, 269);
            this.soundsDataGridView.TabIndex = 3;
            // 
            // soundTypeColumn
            // 
            this.soundTypeColumn.HeaderText = "Type";
            this.soundTypeColumn.Name = "soundTypeColumn";
            this.soundTypeColumn.ReadOnly = true;
            this.soundTypeColumn.Width = 55;
            // 
            // soundKeyColumn
            // 
            this.soundKeyColumn.HeaderText = "Asset ID";
            this.soundKeyColumn.Name = "soundKeyColumn";
            this.soundKeyColumn.ReadOnly = true;
            this.soundKeyColumn.Width = 215;
            // 
            // soundOwnerColumn
            // 
            this.soundOwnerColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.soundOwnerColumn.HeaderText = "Time";
            this.soundOwnerColumn.Name = "soundOwnerColumn";
            this.soundOwnerColumn.ReadOnly = true;
            // 
            // blacklistTab
            // 
            this.blacklistTab.Controls.Add(this.blacklistDataGridView);
            this.blacklistTab.Controls.Add(this.panel3);
            this.blacklistTab.Location = new System.Drawing.Point(4, 22);
            this.blacklistTab.Name = "blacklistTab";
            this.blacklistTab.Padding = new System.Windows.Forms.Padding(3);
            this.blacklistTab.Size = new System.Drawing.Size(454, 275);
            this.blacklistTab.TabIndex = 2;
            this.blacklistTab.Text = "Blacklist";
            this.blacklistTab.UseVisualStyleBackColor = true;
            // 
            // blacklistDataGridView
            // 
            this.blacklistDataGridView.AllowUserToAddRows = false;
            this.blacklistDataGridView.AllowUserToDeleteRows = false;
            this.blacklistDataGridView.AllowUserToResizeRows = false;
            dataGridViewCellStyle5.BackColor = System.Drawing.Color.Silver;
            this.blacklistDataGridView.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle5;
            this.blacklistDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.blacklistDataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.blacklistKeyColumn,
            this.blacklistDateColumn});
            dataGridViewCellStyle6.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle6.BackColor = System.Drawing.SystemColors.ControlDark;
            dataGridViewCellStyle6.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle6.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle6.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle6.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle6.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.blacklistDataGridView.DefaultCellStyle = dataGridViewCellStyle6;
            this.blacklistDataGridView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.blacklistDataGridView.Location = new System.Drawing.Point(3, 3);
            this.blacklistDataGridView.Name = "blacklistDataGridView";
            this.blacklistDataGridView.ReadOnly = true;
            this.blacklistDataGridView.RowHeadersVisible = false;
            dataGridViewCellStyle7.BackColor = System.Drawing.SystemColors.ControlDark;
            this.blacklistDataGridView.RowsDefaultCellStyle = dataGridViewCellStyle7;
            this.blacklistDataGridView.RowTemplate.Height = 18;
            this.blacklistDataGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.blacklistDataGridView.Size = new System.Drawing.Size(448, 240);
            this.blacklistDataGridView.TabIndex = 7;
            // 
            // blacklistKeyColumn
            // 
            this.blacklistKeyColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.blacklistKeyColumn.HeaderText = "Asset ID";
            this.blacklistKeyColumn.Name = "blacklistKeyColumn";
            this.blacklistKeyColumn.ReadOnly = true;
            // 
            // blacklistDateColumn
            // 
            this.blacklistDateColumn.HeaderText = "Date Added";
            this.blacklistDateColumn.Name = "blacklistDateColumn";
            this.blacklistDateColumn.ReadOnly = true;
            this.blacklistDateColumn.Width = 175;
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.button37);
            this.panel3.Controls.Add(this.label25);
            this.panel3.Controls.Add(this.button31);
            this.panel3.Controls.Add(this.textBox18);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel3.Location = new System.Drawing.Point(3, 243);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(448, 29);
            this.panel3.TabIndex = 8;
            // 
            // button37
            // 
            this.button37.Location = new System.Drawing.Point(413, 6);
            this.button37.Name = "button37";
            this.button37.Size = new System.Drawing.Size(27, 23);
            this.button37.TabIndex = 15;
            this.button37.Text = "⛭";
            this.button37.UseVisualStyleBackColor = true;
            // 
            // label25
            // 
            this.label25.AutoSize = true;
            this.label25.Location = new System.Drawing.Point(57, 11);
            this.label25.Name = "label25";
            this.label25.Size = new System.Drawing.Size(50, 13);
            this.label25.TabIndex = 13;
            this.label25.Text = "Asset ID:";
            // 
            // button31
            // 
            this.button31.Location = new System.Drawing.Point(346, 6);
            this.button31.Name = "button31";
            this.button31.Size = new System.Drawing.Size(61, 23);
            this.button31.TabIndex = 11;
            this.button31.Text = "Add";
            this.button31.UseVisualStyleBackColor = true;
            // 
            // textBox18
            // 
            this.textBox18.Location = new System.Drawing.Point(113, 8);
            this.textBox18.Name = "textBox18";
            this.textBox18.Size = new System.Drawing.Size(227, 20);
            this.textBox18.TabIndex = 10;
            this.textBox18.Text = "00000000-0000-0000-0000-000000000000";
            // 
            // blacklistContextMenuStrip
            // 
            this.blacklistContextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.clearBlacklistToolStripMenuItem,
            this.toolStripSeparator3,
            this.saveXMLToolStripMenuItem,
            this.loadXMLToolStripMenuItem});
            this.blacklistContextMenuStrip.Name = "blacklistContextMenuStrip";
            this.blacklistContextMenuStrip.ShowImageMargin = false;
            this.blacklistContextMenuStrip.Size = new System.Drawing.Size(123, 76);
            // 
            // clearBlacklistToolStripMenuItem
            // 
            this.clearBlacklistToolStripMenuItem.Name = "clearBlacklistToolStripMenuItem";
            this.clearBlacklistToolStripMenuItem.Size = new System.Drawing.Size(122, 22);
            this.clearBlacklistToolStripMenuItem.Text = "Clear Blacklist";
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(119, 6);
            // 
            // saveXMLToolStripMenuItem
            // 
            this.saveXMLToolStripMenuItem.Name = "saveXMLToolStripMenuItem";
            this.saveXMLToolStripMenuItem.Size = new System.Drawing.Size(122, 22);
            this.saveXMLToolStripMenuItem.Text = "Save XML";
            // 
            // loadXMLToolStripMenuItem
            // 
            this.loadXMLToolStripMenuItem.Name = "loadXMLToolStripMenuItem";
            this.loadXMLToolStripMenuItem.Size = new System.Drawing.Size(122, 22);
            this.loadXMLToolStripMenuItem.Text = "Load XML";
            // 
            // soundsListContextMenu
            // 
            this.soundsListContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.playLocallyToolStripMenuItem,
            this.playInworldToolStripMenuItem,
            this.toolStripSeparator9,
            this.blacklistToolStripMenuItem});
            this.soundsListContextMenu.Name = "soundsListContextMenu";
            this.soundsListContextMenu.ShowImageMargin = false;
            this.soundsListContextMenu.Size = new System.Drawing.Size(115, 76);
            this.soundsListContextMenu.Opening += new System.ComponentModel.CancelEventHandler(this.soundsListContextMenu_Opening);
            // 
            // playLocallyToolStripMenuItem
            // 
            this.playLocallyToolStripMenuItem.Name = "playLocallyToolStripMenuItem";
            this.playLocallyToolStripMenuItem.Size = new System.Drawing.Size(114, 22);
            this.playLocallyToolStripMenuItem.Text = "Play Locally";
            this.playLocallyToolStripMenuItem.Click += new System.EventHandler(this.playLocallyToolStripMenuItem_Click);
            // 
            // playInworldToolStripMenuItem
            // 
            this.playInworldToolStripMenuItem.Name = "playInworldToolStripMenuItem";
            this.playInworldToolStripMenuItem.Size = new System.Drawing.Size(114, 22);
            this.playInworldToolStripMenuItem.Text = "Play Inworld";
            this.playInworldToolStripMenuItem.Click += new System.EventHandler(this.playInworldToolStripMenuItem_Click);
            // 
            // toolStripSeparator9
            // 
            this.toolStripSeparator9.Name = "toolStripSeparator9";
            this.toolStripSeparator9.Size = new System.Drawing.Size(111, 6);
            // 
            // blacklistToolStripMenuItem
            // 
            this.blacklistToolStripMenuItem.Name = "blacklistToolStripMenuItem";
            this.blacklistToolStripMenuItem.Size = new System.Drawing.Size(114, 22);
            this.blacklistToolStripMenuItem.Text = "Blacklist";
            this.blacklistToolStripMenuItem.Click += new System.EventHandler(this.blacklistToolStripMenuItem_Click);
            // 
            // AssetLogForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(462, 301);
            this.Controls.Add(this.tabControl4);
            this.Name = "AssetLogForm";
            this.Text = "Asset Log";
            this.tabControl4.ResumeLayout(false);
            this.tabPage18.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.animsDataGridView)).EndInit();
            this.tabPage19.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.soundsDataGridView)).EndInit();
            this.blacklistTab.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.blacklistDataGridView)).EndInit();
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            this.blacklistContextMenuStrip.ResumeLayout(false);
            this.soundsListContextMenu.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl4;
        private System.Windows.Forms.TabPage tabPage18;
        private System.Windows.Forms.DataGridView animsDataGridView;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column5;
        private System.Windows.Forms.DataGridViewTextBoxColumn animOwnerColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column3;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column4;
        private System.Windows.Forms.TabPage tabPage19;
        private System.Windows.Forms.DataGridView soundsDataGridView;
        private System.Windows.Forms.DataGridViewTextBoxColumn soundTypeColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn soundKeyColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn soundOwnerColumn;
        private System.Windows.Forms.TabPage blacklistTab;
        private System.Windows.Forms.DataGridView blacklistDataGridView;
        private System.Windows.Forms.DataGridViewTextBoxColumn blacklistKeyColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn blacklistDateColumn;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Button button37;
        private System.Windows.Forms.Label label25;
        private System.Windows.Forms.Button button31;
        private System.Windows.Forms.TextBox textBox18;
        private System.Windows.Forms.ContextMenuStrip blacklistContextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem clearBlacklistToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem saveXMLToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem loadXMLToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip soundsListContextMenu;
        private System.Windows.Forms.ToolStripMenuItem playLocallyToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem playInworldToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator9;
        private System.Windows.Forms.ToolStripMenuItem blacklistToolStripMenuItem;
    }
}