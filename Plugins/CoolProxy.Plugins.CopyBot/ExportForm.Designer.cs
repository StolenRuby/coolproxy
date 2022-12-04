namespace CoolProxy.Plugins.CopyBot
{
    partial class ExportForm
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle7 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle8 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle9 = new System.Windows.Forms.DataGridViewCellStyle();
            this.selectWearablesButton = new System.Windows.Forms.Button();
            this.selectAllButton = new System.Windows.Forms.Button();
            this.selectObjectsButton = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.closeButton = new System.Windows.Forms.Button();
            this.countLabel = new System.Windows.Forms.Label();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.saveAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.importToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.forgeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.splitButton1 = new CoolProxy.Controls.SplitButton();
            this.checkBox1 = new CoolProxy.Controls.CheckBox();
            this.dataGridView = new System.Windows.Forms.DataGridView();
            this.Column1 = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.assetIconColumn = new System.Windows.Forms.DataGridViewImageColumn();
            this.StatusColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column3 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column4 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.checkBox2 = new CoolProxy.Controls.CheckBox();
            this.contextMenuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView)).BeginInit();
            this.SuspendLayout();
            // 
            // selectWearablesButton
            // 
            this.selectWearablesButton.Location = new System.Drawing.Point(145, 219);
            this.selectWearablesButton.Name = "selectWearablesButton";
            this.selectWearablesButton.Size = new System.Drawing.Size(86, 23);
            this.selectWearablesButton.TabIndex = 1;
            this.selectWearablesButton.Text = "Wearables";
            this.selectWearablesButton.UseVisualStyleBackColor = true;
            this.selectWearablesButton.Click += new System.EventHandler(this.selectWearablesButton_Click);
            // 
            // selectAllButton
            // 
            this.selectAllButton.Location = new System.Drawing.Point(12, 219);
            this.selectAllButton.Name = "selectAllButton";
            this.selectAllButton.Size = new System.Drawing.Size(53, 23);
            this.selectAllButton.TabIndex = 2;
            this.selectAllButton.Text = "All";
            this.selectAllButton.UseVisualStyleBackColor = true;
            this.selectAllButton.Click += new System.EventHandler(this.selectAllButton_Click);
            // 
            // selectObjectsButton
            // 
            this.selectObjectsButton.Location = new System.Drawing.Point(71, 219);
            this.selectObjectsButton.Name = "selectObjectsButton";
            this.selectObjectsButton.Size = new System.Drawing.Size(68, 23);
            this.selectObjectsButton.TabIndex = 3;
            this.selectObjectsButton.Text = "Objects";
            this.selectObjectsButton.UseVisualStyleBackColor = true;
            this.selectObjectsButton.Click += new System.EventHandler(this.selectObjectsButton_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 203);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(87, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "Select/Deselect:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(14, 268);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(78, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "Names loaded:";
            // 
            // closeButton
            // 
            this.closeButton.Location = new System.Drawing.Point(12, 289);
            this.closeButton.Name = "closeButton";
            this.closeButton.Size = new System.Drawing.Size(105, 23);
            this.closeButton.TabIndex = 7;
            this.closeButton.Text = "Cancel";
            this.closeButton.UseVisualStyleBackColor = true;
            this.closeButton.Click += new System.EventHandler(this.closeButton_Click);
            // 
            // countLabel
            // 
            this.countLabel.AutoSize = true;
            this.countLabel.Location = new System.Drawing.Point(95, 268);
            this.countLabel.Name = "countLabel";
            this.countLabel.Size = new System.Drawing.Size(30, 13);
            this.countLabel.TabIndex = 9;
            this.countLabel.Text = "0 / 0";
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.saveAsToolStripMenuItem,
            this.importToolStripMenuItem,
            this.forgeToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.ShowImageMargin = false;
            this.contextMenuStrip1.Size = new System.Drawing.Size(99, 70);
            // 
            // saveAsToolStripMenuItem
            // 
            this.saveAsToolStripMenuItem.Name = "saveAsToolStripMenuItem";
            this.saveAsToolStripMenuItem.Size = new System.Drawing.Size(98, 22);
            this.saveAsToolStripMenuItem.Text = "Save As...";
            this.saveAsToolStripMenuItem.Click += new System.EventHandler(this.saveAsToolStripMenuItem_Click);
            // 
            // importToolStripMenuItem
            // 
            this.importToolStripMenuItem.Name = "importToolStripMenuItem";
            this.importToolStripMenuItem.Size = new System.Drawing.Size(98, 22);
            this.importToolStripMenuItem.Text = "Import";
            this.importToolStripMenuItem.Click += new System.EventHandler(this.importToolStripMenuItem_Click);
            // 
            // forgeToolStripMenuItem
            // 
            this.forgeToolStripMenuItem.Name = "forgeToolStripMenuItem";
            this.forgeToolStripMenuItem.Size = new System.Drawing.Size(98, 22);
            this.forgeToolStripMenuItem.Text = "Forge";
            this.forgeToolStripMenuItem.Click += new System.EventHandler(this.forgeToolStripMenuItem_Click);
            // 
            // splitButton1
            // 
            this.splitButton1.AutoSize = true;
            this.splitButton1.ContextMenuStrip = this.contextMenuStrip1;
            this.splitButton1.Location = new System.Drawing.Point(126, 289);
            this.splitButton1.Name = "splitButton1";
            this.splitButton1.Size = new System.Drawing.Size(105, 23);
            this.splitButton1.SplitMenuStrip = this.contextMenuStrip1;
            this.splitButton1.TabIndex = 20;
            this.splitButton1.Text = "Save As...";
            this.splitButton1.UseVisualStyleBackColor = true;
            this.splitButton1.Click += new System.EventHandler(this.saveButton_Click);
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.EnabledSetting = null;
            this.checkBox1.Location = new System.Drawing.Point(15, 247);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Setting = null;
            this.checkBox1.Size = new System.Drawing.Size(108, 17);
            this.checkBox1.TabIndex = 8;
            this.checkBox1.Text = "Download Assets";
            this.checkBox1.UseVisualStyleBackColor = true;
            // 
            // dataGridView
            // 
            this.dataGridView.AllowUserToAddRows = false;
            this.dataGridView.AllowUserToDeleteRows = false;
            this.dataGridView.AllowUserToResizeColumns = false;
            this.dataGridView.AllowUserToResizeRows = false;
            dataGridViewCellStyle7.BackColor = System.Drawing.Color.Silver;
            this.dataGridView.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle7;
            this.dataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView.ColumnHeadersVisible = false;
            this.dataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Column1,
            this.assetIconColumn,
            this.StatusColumn,
            this.Column2,
            this.Column3,
            this.Column4});
            dataGridViewCellStyle8.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle8.BackColor = System.Drawing.SystemColors.ControlDark;
            dataGridViewCellStyle8.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle8.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle8.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle8.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle8.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dataGridView.DefaultCellStyle = dataGridViewCellStyle8;
            this.dataGridView.Location = new System.Drawing.Point(12, 12);
            this.dataGridView.Name = "dataGridView";
            this.dataGridView.RowHeadersVisible = false;
            this.dataGridView.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            dataGridViewCellStyle9.BackColor = System.Drawing.SystemColors.ControlDark;
            this.dataGridView.RowsDefaultCellStyle = dataGridViewCellStyle9;
            this.dataGridView.RowTemplate.Height = 18;
            this.dataGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridView.Size = new System.Drawing.Size(219, 185);
            this.dataGridView.TabIndex = 22;
            // 
            // Column1
            // 
            this.Column1.HeaderText = "";
            this.Column1.Name = "Column1";
            this.Column1.Width = 20;
            // 
            // assetIconColumn
            // 
            this.assetIconColumn.HeaderText = "";
            this.assetIconColumn.Name = "assetIconColumn";
            this.assetIconColumn.ReadOnly = true;
            this.assetIconColumn.Width = 20;
            // 
            // StatusColumn
            // 
            this.StatusColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.StatusColumn.HeaderText = "Name";
            this.StatusColumn.Name = "StatusColumn";
            this.StatusColumn.ReadOnly = true;
            // 
            // Column2
            // 
            this.Column2.HeaderText = "type";
            this.Column2.Name = "Column2";
            this.Column2.Visible = false;
            // 
            // Column3
            // 
            this.Column3.HeaderText = "id";
            this.Column3.Name = "Column3";
            this.Column3.Visible = false;
            // 
            // Column4
            // 
            this.Column4.HeaderText = "uuid";
            this.Column4.Name = "Column4";
            this.Column4.Visible = false;
            // 
            // checkBox2
            // 
            this.checkBox2.AutoSize = true;
            this.checkBox2.EnabledSetting = null;
            this.checkBox2.Location = new System.Drawing.Point(134, 248);
            this.checkBox2.Name = "checkBox2";
            this.checkBox2.Setting = null;
            this.checkBox2.Size = new System.Drawing.Size(100, 17);
            this.checkBox2.TabIndex = 23;
            this.checkBox2.Text = "Fetch Inventory";
            this.checkBox2.UseVisualStyleBackColor = true;
            // 
            // ExportForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(243, 324);
            this.Controls.Add(this.checkBox2);
            this.Controls.Add(this.dataGridView);
            this.Controls.Add(this.splitButton1);
            this.Controls.Add(this.countLabel);
            this.Controls.Add(this.checkBox1);
            this.Controls.Add(this.closeButton);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.selectObjectsButton);
            this.Controls.Add(this.selectAllButton);
            this.Controls.Add(this.selectWearablesButton);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ExportForm";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Export...";
            this.TopMost = true;
            this.Load += new System.EventHandler(this.OtherExportForm_Load);
            this.contextMenuStrip1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button selectWearablesButton;
        private System.Windows.Forms.Button selectAllButton;
        private System.Windows.Forms.Button selectObjectsButton;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button closeButton;
        private CoolProxy.Controls.CheckBox checkBox1;
        private System.Windows.Forms.Label countLabel;
        private CoolProxy.Controls.SplitButton splitButton1;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem saveAsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem importToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem forgeToolStripMenuItem;
        private System.Windows.Forms.DataGridView dataGridView;
        private System.Windows.Forms.DataGridViewCheckBoxColumn Column1;
        private System.Windows.Forms.DataGridViewImageColumn assetIconColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn StatusColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column2;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column3;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column4;
        private CoolProxy.Controls.CheckBox checkBox2;
    }
}