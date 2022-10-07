namespace CoolProxy.Plugins.CopyBot
{
    partial class ImportForm
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle10 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle11 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle12 = new System.Windows.Forms.DataGridViewCellStyle();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.forgeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.importToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.Column4 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.StatusColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.assetIconColumn = new System.Windows.Forms.DataGridViewImageColumn();
            this.Column1 = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.dataGridView = new System.Windows.Forms.DataGridView();
            this.Column3 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.splitButton1 = new CoolGUI.Controls.SplitButton();
            this.checkBox1 = new CoolGUI.Controls.CheckBox();
            this.closeButton = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.selectObjectsButton = new System.Windows.Forms.Button();
            this.selectAllButton = new System.Windows.Forms.Button();
            this.selectWearablesButton = new System.Windows.Forms.Button();
            this.checkBox2 = new CoolGUI.Controls.CheckBox();
            this.uploadToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView)).BeginInit();
            this.SuspendLayout();
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.importToolStripMenuItem,
            this.forgeToolStripMenuItem,
            this.uploadToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.ShowImageMargin = false;
            this.contextMenuStrip1.Size = new System.Drawing.Size(156, 92);
            // 
            // forgeToolStripMenuItem
            // 
            this.forgeToolStripMenuItem.Name = "forgeToolStripMenuItem";
            this.forgeToolStripMenuItem.Size = new System.Drawing.Size(155, 22);
            this.forgeToolStripMenuItem.Text = "Forge";
            this.forgeToolStripMenuItem.Click += new System.EventHandler(this.forgeToolStripMenuItem_Click);
            // 
            // importToolStripMenuItem
            // 
            this.importToolStripMenuItem.Name = "importToolStripMenuItem";
            this.importToolStripMenuItem.Size = new System.Drawing.Size(155, 22);
            this.importToolStripMenuItem.Text = "Import";
            this.importToolStripMenuItem.Click += new System.EventHandler(this.importToolStripMenuItem_Click);
            // 
            // Column4
            // 
            this.Column4.HeaderText = "uuid";
            this.Column4.Name = "Column4";
            this.Column4.Visible = false;
            // 
            // Column2
            // 
            this.Column2.HeaderText = "type";
            this.Column2.Name = "Column2";
            this.Column2.Visible = false;
            // 
            // StatusColumn
            // 
            this.StatusColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.StatusColumn.HeaderText = "Name";
            this.StatusColumn.Name = "StatusColumn";
            this.StatusColumn.ReadOnly = true;
            // 
            // assetIconColumn
            // 
            this.assetIconColumn.HeaderText = "";
            this.assetIconColumn.Name = "assetIconColumn";
            this.assetIconColumn.ReadOnly = true;
            this.assetIconColumn.Width = 20;
            // 
            // Column1
            // 
            this.Column1.HeaderText = "";
            this.Column1.Name = "Column1";
            this.Column1.Width = 20;
            // 
            // dataGridView
            // 
            this.dataGridView.AllowUserToAddRows = false;
            this.dataGridView.AllowUserToDeleteRows = false;
            this.dataGridView.AllowUserToResizeColumns = false;
            this.dataGridView.AllowUserToResizeRows = false;
            dataGridViewCellStyle10.BackColor = System.Drawing.Color.Silver;
            this.dataGridView.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle10;
            this.dataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView.ColumnHeadersVisible = false;
            this.dataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Column1,
            this.assetIconColumn,
            this.StatusColumn,
            this.Column2,
            this.Column3,
            this.Column4});
            dataGridViewCellStyle11.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle11.BackColor = System.Drawing.SystemColors.ControlDark;
            dataGridViewCellStyle11.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle11.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle11.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle11.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle11.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dataGridView.DefaultCellStyle = dataGridViewCellStyle11;
            this.dataGridView.Location = new System.Drawing.Point(10, 10);
            this.dataGridView.Name = "dataGridView";
            this.dataGridView.RowHeadersVisible = false;
            this.dataGridView.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            dataGridViewCellStyle12.BackColor = System.Drawing.SystemColors.ControlDark;
            this.dataGridView.RowsDefaultCellStyle = dataGridViewCellStyle12;
            this.dataGridView.RowTemplate.Height = 18;
            this.dataGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridView.Size = new System.Drawing.Size(219, 185);
            this.dataGridView.TabIndex = 32;
            // 
            // Column3
            // 
            this.Column3.HeaderText = "id";
            this.Column3.Name = "Column3";
            this.Column3.Visible = false;
            // 
            // splitButton1
            // 
            this.splitButton1.AutoSize = true;
            this.splitButton1.ContextMenuStrip = this.contextMenuStrip1;
            this.splitButton1.Location = new System.Drawing.Point(124, 270);
            this.splitButton1.Name = "splitButton1";
            this.splitButton1.Size = new System.Drawing.Size(105, 23);
            this.splitButton1.SplitMenuStrip = this.contextMenuStrip1;
            this.splitButton1.TabIndex = 31;
            this.splitButton1.Text = "Import";
            this.splitButton1.UseVisualStyleBackColor = true;
            this.splitButton1.Click += new System.EventHandler(this.splitButton1_Click);
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.EnabledSetting = null;
            this.checkBox1.Location = new System.Drawing.Point(18, 248);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Setting = null;
            this.checkBox1.Size = new System.Drawing.Size(96, 17);
            this.checkBox1.TabIndex = 29;
            this.checkBox1.Text = "Keep Positions";
            this.checkBox1.UseVisualStyleBackColor = true;
            // 
            // closeButton
            // 
            this.closeButton.Location = new System.Drawing.Point(10, 270);
            this.closeButton.Name = "closeButton";
            this.closeButton.Size = new System.Drawing.Size(105, 23);
            this.closeButton.TabIndex = 28;
            this.closeButton.Text = "Cancel";
            this.closeButton.UseVisualStyleBackColor = true;
            this.closeButton.Click += new System.EventHandler(this.button1_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(10, 201);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(87, 13);
            this.label1.TabIndex = 26;
            this.label1.Text = "Select/Deselect:";
            // 
            // selectObjectsButton
            // 
            this.selectObjectsButton.Location = new System.Drawing.Point(69, 217);
            this.selectObjectsButton.Name = "selectObjectsButton";
            this.selectObjectsButton.Size = new System.Drawing.Size(68, 23);
            this.selectObjectsButton.TabIndex = 25;
            this.selectObjectsButton.Text = "Objects";
            this.selectObjectsButton.UseVisualStyleBackColor = true;
            this.selectObjectsButton.Click += new System.EventHandler(this.selectObjectsButton_Click);
            // 
            // selectAllButton
            // 
            this.selectAllButton.Location = new System.Drawing.Point(10, 217);
            this.selectAllButton.Name = "selectAllButton";
            this.selectAllButton.Size = new System.Drawing.Size(53, 23);
            this.selectAllButton.TabIndex = 24;
            this.selectAllButton.Text = "All";
            this.selectAllButton.UseVisualStyleBackColor = true;
            this.selectAllButton.Click += new System.EventHandler(this.selectAllButton_Click);
            // 
            // selectWearablesButton
            // 
            this.selectWearablesButton.Location = new System.Drawing.Point(143, 217);
            this.selectWearablesButton.Name = "selectWearablesButton";
            this.selectWearablesButton.Size = new System.Drawing.Size(86, 23);
            this.selectWearablesButton.TabIndex = 23;
            this.selectWearablesButton.Text = "Wearables";
            this.selectWearablesButton.UseVisualStyleBackColor = true;
            this.selectWearablesButton.Click += new System.EventHandler(this.selectWearablesButton_Click);
            // 
            // checkBox2
            // 
            this.checkBox2.AutoSize = true;
            this.checkBox2.EnabledSetting = null;
            this.checkBox2.Location = new System.Drawing.Point(130, 248);
            this.checkBox2.Name = "checkBox2";
            this.checkBox2.Setting = null;
            this.checkBox2.Size = new System.Drawing.Size(94, 17);
            this.checkBox2.TabIndex = 33;
            this.checkBox2.Text = "Upload Assets";
            this.checkBox2.UseVisualStyleBackColor = true;
            // 
            // uploadToolStripMenuItem
            // 
            this.uploadToolStripMenuItem.Name = "uploadToolStripMenuItem";
            this.uploadToolStripMenuItem.Size = new System.Drawing.Size(155, 22);
            this.uploadToolStripMenuItem.Text = "Upload";
            this.uploadToolStripMenuItem.Click += new System.EventHandler(this.uploadToolStripMenuItem_Click);
            // 
            // ImportForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(239, 301);
            this.Controls.Add(this.checkBox2);
            this.Controls.Add(this.dataGridView);
            this.Controls.Add(this.splitButton1);
            this.Controls.Add(this.checkBox1);
            this.Controls.Add(this.closeButton);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.selectObjectsButton);
            this.Controls.Add(this.selectAllButton);
            this.Controls.Add(this.selectWearablesButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ImportForm";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Import Options";
            this.TopMost = true;
            this.Load += new System.EventHandler(this.PostBuild);
            this.contextMenuStrip1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem importToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem forgeToolStripMenuItem;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column4;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column2;
        private System.Windows.Forms.DataGridViewTextBoxColumn StatusColumn;
        private System.Windows.Forms.DataGridViewImageColumn assetIconColumn;
        private System.Windows.Forms.DataGridViewCheckBoxColumn Column1;
        private System.Windows.Forms.DataGridView dataGridView;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column3;
        private CoolGUI.Controls.SplitButton splitButton1;
        private CoolGUI.Controls.CheckBox checkBox1;
        private System.Windows.Forms.Button closeButton;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button selectObjectsButton;
        private System.Windows.Forms.Button selectAllButton;
        private System.Windows.Forms.Button selectWearablesButton;
        private CoolGUI.Controls.CheckBox checkBox2;
        private System.Windows.Forms.ToolStripMenuItem uploadToolStripMenuItem;
    }
}