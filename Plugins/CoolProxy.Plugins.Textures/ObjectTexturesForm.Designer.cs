
namespace CoolProxy.Plugins.Textures
{
    partial class ObjectTexturesForm
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            this.texturesDataGridView = new System.Windows.Forms.DataGridView();
            this.animKeyColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.copyToInvButton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.texturesDataGridView)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // texturesDataGridView
            // 
            this.texturesDataGridView.AllowUserToAddRows = false;
            this.texturesDataGridView.AllowUserToDeleteRows = false;
            this.texturesDataGridView.AllowUserToResizeRows = false;
            dataGridViewCellStyle3.BackColor = System.Drawing.Color.Silver;
            this.texturesDataGridView.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle3;
            this.texturesDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.texturesDataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.animKeyColumn,
            this.Column1});
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle4.BackColor = System.Drawing.SystemColors.ControlDark;
            dataGridViewCellStyle4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle4.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle4.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle4.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle4.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.texturesDataGridView.DefaultCellStyle = dataGridViewCellStyle4;
            this.texturesDataGridView.Location = new System.Drawing.Point(12, 12);
            this.texturesDataGridView.MultiSelect = false;
            this.texturesDataGridView.Name = "texturesDataGridView";
            this.texturesDataGridView.ReadOnly = true;
            this.texturesDataGridView.RowHeadersVisible = false;
            this.texturesDataGridView.RowTemplate.Height = 18;
            this.texturesDataGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.texturesDataGridView.Size = new System.Drawing.Size(324, 219);
            this.texturesDataGridView.TabIndex = 3;
            this.texturesDataGridView.SelectionChanged += new System.EventHandler(this.texturesDataGridView_SelectionChanged);
            // 
            // animKeyColumn
            // 
            this.animKeyColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.animKeyColumn.HeaderText = "Asset ID";
            this.animKeyColumn.Name = "animKeyColumn";
            this.animKeyColumn.ReadOnly = true;
            // 
            // Column1
            // 
            this.Column1.HeaderText = "Type";
            this.Column1.Name = "Column1";
            this.Column1.ReadOnly = true;
            // 
            // pictureBox1
            // 
            this.pictureBox1.BackColor = System.Drawing.SystemColors.ControlDark;
            this.pictureBox1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.pictureBox1.Location = new System.Drawing.Point(352, 12);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(180, 180);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox1.TabIndex = 4;
            this.pictureBox1.TabStop = false;
            // 
            // copyToInvButton
            // 
            this.copyToInvButton.Location = new System.Drawing.Point(352, 198);
            this.copyToInvButton.Name = "copyToInvButton";
            this.copyToInvButton.Size = new System.Drawing.Size(180, 33);
            this.copyToInvButton.TabIndex = 5;
            this.copyToInvButton.Text = "Copy to Inventory";
            this.copyToInvButton.UseVisualStyleBackColor = true;
            this.copyToInvButton.Click += new System.EventHandler(this.copyToInvButton_Click);
            // 
            // ObjectTexturesForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(544, 244);
            this.Controls.Add(this.copyToInvButton);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.texturesDataGridView);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.Name = "ObjectTexturesForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Object Textures";
            ((System.ComponentModel.ISupportInitialize)(this.texturesDataGridView)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView texturesDataGridView;
        private System.Windows.Forms.DataGridViewTextBoxColumn animKeyColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column1;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Button copyToInvButton;
    }
}