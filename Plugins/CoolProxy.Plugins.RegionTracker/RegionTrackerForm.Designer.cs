
namespace CoolProxy.Plugins.RegionTracker
{
    partial class RegionTrackerForm
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
            this.regionsDataGridView = new System.Windows.Forms.DataGridView();
            this.dataGridViewTextBoxColumn4 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn5 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn6 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.regionsContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.copyRegionIPToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.copyRegionIDToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)(this.regionsDataGridView)).BeginInit();
            this.regionsContextMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // regionsDataGridView
            // 
            this.regionsDataGridView.AllowUserToAddRows = false;
            this.regionsDataGridView.AllowUserToDeleteRows = false;
            this.regionsDataGridView.AllowUserToResizeRows = false;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.Silver;
            this.regionsDataGridView.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            this.regionsDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.regionsDataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dataGridViewTextBoxColumn4,
            this.dataGridViewTextBoxColumn5,
            this.dataGridViewTextBoxColumn6});
            this.regionsDataGridView.ContextMenuStrip = this.regionsContextMenu;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.ControlDark;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.regionsDataGridView.DefaultCellStyle = dataGridViewCellStyle2;
            this.regionsDataGridView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.regionsDataGridView.Location = new System.Drawing.Point(0, 0);
            this.regionsDataGridView.Name = "regionsDataGridView";
            this.regionsDataGridView.ReadOnly = true;
            this.regionsDataGridView.RowHeadersVisible = false;
            dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.ControlDark;
            this.regionsDataGridView.RowsDefaultCellStyle = dataGridViewCellStyle3;
            this.regionsDataGridView.RowTemplate.Height = 18;
            this.regionsDataGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.regionsDataGridView.Size = new System.Drawing.Size(439, 149);
            this.regionsDataGridView.TabIndex = 5;
            // 
            // dataGridViewTextBoxColumn4
            // 
            this.dataGridViewTextBoxColumn4.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.dataGridViewTextBoxColumn4.HeaderText = "Region Name";
            this.dataGridViewTextBoxColumn4.Name = "dataGridViewTextBoxColumn4";
            this.dataGridViewTextBoxColumn4.ReadOnly = true;
            // 
            // dataGridViewTextBoxColumn5
            // 
            this.dataGridViewTextBoxColumn5.HeaderText = "IP Address";
            this.dataGridViewTextBoxColumn5.Name = "dataGridViewTextBoxColumn5";
            this.dataGridViewTextBoxColumn5.ReadOnly = true;
            this.dataGridViewTextBoxColumn5.Width = 150;
            // 
            // dataGridViewTextBoxColumn6
            // 
            this.dataGridViewTextBoxColumn6.HeaderText = "Agents";
            this.dataGridViewTextBoxColumn6.Name = "dataGridViewTextBoxColumn6";
            this.dataGridViewTextBoxColumn6.ReadOnly = true;
            this.dataGridViewTextBoxColumn6.Width = 70;
            // 
            // regionsContextMenu
            // 
            this.regionsContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.copyRegionIPToolStripMenuItem,
            this.copyRegionIDToolStripMenuItem});
            this.regionsContextMenu.Name = "regionsContextMenu";
            this.regionsContextMenu.ShowImageMargin = false;
            this.regionsContextMenu.Size = new System.Drawing.Size(132, 48);
            this.regionsContextMenu.Opening += new System.ComponentModel.CancelEventHandler(this.regionsContextMenu_Opening);
            // 
            // copyRegionIPToolStripMenuItem
            // 
            this.copyRegionIPToolStripMenuItem.Name = "copyRegionIPToolStripMenuItem";
            this.copyRegionIPToolStripMenuItem.Size = new System.Drawing.Size(131, 22);
            this.copyRegionIPToolStripMenuItem.Text = "Copy Region IP";
            this.copyRegionIPToolStripMenuItem.Click += new System.EventHandler(this.copyRegionIPToolStripMenuItem_Click);
            // 
            // copyRegionIDToolStripMenuItem
            // 
            this.copyRegionIDToolStripMenuItem.Name = "copyRegionIDToolStripMenuItem";
            this.copyRegionIDToolStripMenuItem.Size = new System.Drawing.Size(131, 22);
            this.copyRegionIDToolStripMenuItem.Text = "Copy Region ID";
            this.copyRegionIDToolStripMenuItem.Click += new System.EventHandler(this.copyRegionIDToolStripMenuItem_Click);
            // 
            // RegionTrackerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(439, 149);
            this.Controls.Add(this.regionsDataGridView);
            this.Name = "RegionTrackerForm";
            this.Text = "Region Tracker";
            ((System.ComponentModel.ISupportInitialize)(this.regionsDataGridView)).EndInit();
            this.regionsContextMenu.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView regionsDataGridView;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn4;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn5;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn6;
        private System.Windows.Forms.ContextMenuStrip regionsContextMenu;
        private System.Windows.Forms.ToolStripMenuItem copyRegionIPToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem copyRegionIDToolStripMenuItem;
    }
}