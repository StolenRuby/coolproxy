
namespace CoolProxy.Plugins.AvatarTracker
{
    partial class AvatarTrackerForm
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            this.avatarTrackerGridView = new System.Windows.Forms.DataGridView();
            this.avatarKeyColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.avatarPosColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.avatarDistColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.avatarTrackerGridView)).BeginInit();
            this.SuspendLayout();
            // 
            // avatarTrackerGridView
            // 
            this.avatarTrackerGridView.AllowUserToAddRows = false;
            this.avatarTrackerGridView.AllowUserToDeleteRows = false;
            this.avatarTrackerGridView.AllowUserToResizeRows = false;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.Silver;
            this.avatarTrackerGridView.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            this.avatarTrackerGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.avatarTrackerGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.avatarKeyColumn,
            this.avatarPosColumn,
            this.avatarDistColumn});
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.ControlDark;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.avatarTrackerGridView.DefaultCellStyle = dataGridViewCellStyle2;
            this.avatarTrackerGridView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.avatarTrackerGridView.Location = new System.Drawing.Point(0, 0);
            this.avatarTrackerGridView.Name = "avatarTrackerGridView";
            this.avatarTrackerGridView.ReadOnly = true;
            this.avatarTrackerGridView.RowHeadersVisible = false;
            this.avatarTrackerGridView.RowTemplate.Height = 18;
            this.avatarTrackerGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.avatarTrackerGridView.Size = new System.Drawing.Size(324, 126);
            this.avatarTrackerGridView.TabIndex = 5;
            // 
            // avatarKeyColumn
            // 
            this.avatarKeyColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.avatarKeyColumn.DataPropertyName = "Name";
            this.avatarKeyColumn.HeaderText = "Avatar Name";
            this.avatarKeyColumn.Name = "avatarKeyColumn";
            this.avatarKeyColumn.ReadOnly = true;
            // 
            // avatarPosColumn
            // 
            this.avatarPosColumn.DataPropertyName = "FormattedPos";
            this.avatarPosColumn.HeaderText = "Position";
            this.avatarPosColumn.Name = "avatarPosColumn";
            this.avatarPosColumn.ReadOnly = true;
            this.avatarPosColumn.Width = 80;
            // 
            // avatarDistColumn
            // 
            this.avatarDistColumn.DataPropertyName = "DistanceStr";
            this.avatarDistColumn.HeaderText = "Dist.";
            this.avatarDistColumn.Name = "avatarDistColumn";
            this.avatarDistColumn.ReadOnly = true;
            this.avatarDistColumn.Width = 60;
            // 
            // AvatarTrackerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(324, 126);
            this.Controls.Add(this.avatarTrackerGridView);
            this.Name = "AvatarTrackerForm";
            this.Text = "Avatar Tracker";
            ((System.ComponentModel.ISupportInitialize)(this.avatarTrackerGridView)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView avatarTrackerGridView;
        private System.Windows.Forms.DataGridViewTextBoxColumn avatarKeyColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn avatarPosColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn avatarDistColumn;
    }
}