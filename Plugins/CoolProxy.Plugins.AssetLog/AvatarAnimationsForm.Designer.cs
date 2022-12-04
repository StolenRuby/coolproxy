
namespace CoolProxy.Plugins.AssetLog
{
    partial class AvatarAnimationsForm
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
            this.animsDataGridView = new System.Windows.Forms.DataGridView();
            this.animsContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.copyToInventoryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.animIDColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.animsDataGridView)).BeginInit();
            this.animsContextMenu.SuspendLayout();
            this.SuspendLayout();
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
            this.animIDColumn,
            this.Column1});
            this.animsDataGridView.ContextMenuStrip = this.animsContextMenu;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.ControlDark;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.animsDataGridView.DefaultCellStyle = dataGridViewCellStyle2;
            this.animsDataGridView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.animsDataGridView.Location = new System.Drawing.Point(0, 0);
            this.animsDataGridView.Name = "animsDataGridView";
            this.animsDataGridView.ReadOnly = true;
            this.animsDataGridView.RowHeadersVisible = false;
            this.animsDataGridView.RowTemplate.Height = 18;
            this.animsDataGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.animsDataGridView.Size = new System.Drawing.Size(392, 458);
            this.animsDataGridView.TabIndex = 3;
            // 
            // animsContextMenu
            // 
            this.animsContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.copyToInventoryToolStripMenuItem});
            this.animsContextMenu.Name = "animsContextMenu";
            this.animsContextMenu.ShowImageMargin = false;
            this.animsContextMenu.Size = new System.Drawing.Size(145, 26);
            this.animsContextMenu.Opening += new System.ComponentModel.CancelEventHandler(this.animsContextMenu_Opening);
            // 
            // copyToInventoryToolStripMenuItem
            // 
            this.copyToInventoryToolStripMenuItem.Name = "copyToInventoryToolStripMenuItem";
            this.copyToInventoryToolStripMenuItem.Size = new System.Drawing.Size(144, 22);
            this.copyToInventoryToolStripMenuItem.Text = "Copy to Inventory";
            this.copyToInventoryToolStripMenuItem.Click += new System.EventHandler(this.copyToInventoryToolStripMenuItem_Click);
            // 
            // animIDColumn
            // 
            this.animIDColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.animIDColumn.HeaderText = "Animation ID";
            this.animIDColumn.Name = "animIDColumn";
            this.animIDColumn.ReadOnly = true;
            // 
            // Column1
            // 
            this.Column1.HeaderText = "Time";
            this.Column1.Name = "Column1";
            this.Column1.ReadOnly = true;
            this.Column1.Width = 140;
            // 
            // AvatarAnimationsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(392, 458);
            this.Controls.Add(this.animsDataGridView);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AvatarAnimationsForm";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "AvatarAnimationsForm";
            ((System.ComponentModel.ISupportInitialize)(this.animsDataGridView)).EndInit();
            this.animsContextMenu.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView animsDataGridView;
        private System.Windows.Forms.ContextMenuStrip animsContextMenu;
        private System.Windows.Forms.ToolStripMenuItem copyToInventoryToolStripMenuItem;
        private System.Windows.Forms.DataGridViewTextBoxColumn animIDColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column1;
    }
}