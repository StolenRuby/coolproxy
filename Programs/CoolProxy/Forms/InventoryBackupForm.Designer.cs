namespace CoolProxy
{
    partial class InventoryBackupForm
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
            this.downloadsDataGridView = new System.Windows.Forms.DataGridView();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.backupIconColumn = new System.Windows.Forms.DataGridViewImageColumn();
            this.backupNameColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.backupStatusColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.downloadsDataGridView)).BeginInit();
            this.SuspendLayout();
            // 
            // downloadsDataGridView
            // 
            this.downloadsDataGridView.AllowUserToAddRows = false;
            this.downloadsDataGridView.AllowUserToDeleteRows = false;
            this.downloadsDataGridView.AllowUserToResizeRows = false;
            this.downloadsDataGridView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.downloadsDataGridView.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            this.downloadsDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.downloadsDataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.backupIconColumn,
            this.backupNameColumn,
            this.backupStatusColumn});
            this.downloadsDataGridView.EnableHeadersVisualStyles = false;
            this.downloadsDataGridView.Location = new System.Drawing.Point(12, 12);
            this.downloadsDataGridView.Name = "downloadsDataGridView";
            this.downloadsDataGridView.RowHeadersVisible = false;
            this.downloadsDataGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.downloadsDataGridView.Size = new System.Drawing.Size(312, 159);
            this.downloadsDataGridView.TabIndex = 0;
            // 
            // progressBar1
            // 
            this.progressBar1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBar1.Location = new System.Drawing.Point(12, 177);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(312, 23);
            this.progressBar1.TabIndex = 1;
            // 
            // backupIconColumn
            // 
            this.backupIconColumn.HeaderText = "";
            this.backupIconColumn.Name = "backupIconColumn";
            this.backupIconColumn.ReadOnly = true;
            this.backupIconColumn.Width = 20;
            // 
            // backupNameColumn
            // 
            this.backupNameColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.backupNameColumn.HeaderText = "Name";
            this.backupNameColumn.Name = "backupNameColumn";
            this.backupNameColumn.ReadOnly = true;
            this.backupNameColumn.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.backupNameColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // backupStatusColumn
            // 
            this.backupStatusColumn.HeaderText = "Status";
            this.backupStatusColumn.Name = "backupStatusColumn";
            this.backupStatusColumn.ReadOnly = true;
            this.backupStatusColumn.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.backupStatusColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // InventoryBackupForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(336, 212);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.downloadsDataGridView);
            this.Name = "InventoryBackupForm";
            this.Text = "Inventory Backup";
            this.TopMost = true;
            ((System.ComponentModel.ISupportInitialize)(this.downloadsDataGridView)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView downloadsDataGridView;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.DataGridViewImageColumn backupIconColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn backupNameColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn backupStatusColumn;
    }
}