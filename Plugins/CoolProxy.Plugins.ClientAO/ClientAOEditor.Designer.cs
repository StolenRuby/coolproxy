
namespace CoolProxy.Plugins.ClientAO
{
    partial class ClientAOEditor
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle5 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle6 = new System.Windows.Forms.DataGridViewCellStyle();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.dataGridView = new System.Windows.Forms.DataGridView();
            this.Column1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.checkBox2 = new System.Windows.Forms.CheckBox();
            this.moveDownButton = new System.Windows.Forms.Button();
            this.removeAnimButton = new System.Windows.Forms.Button();
            this.moveUpButton = new System.Windows.Forms.Button();
            this.previousAnimButton = new System.Windows.Forms.Button();
            this.nextAnimButton = new System.Windows.Forms.Button();
            this.checkBox3 = new CoolProxy.Controls.CheckBox();
            this.numericUpDown1 = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.collapseButton = new System.Windows.Forms.Button();
            this.checkBox5 = new System.Windows.Forms.CheckBox();
            this.checkBox6 = new System.Windows.Forms.CheckBox();
            this.aoNameComboBox = new System.Windows.Forms.ComboBox();
            this.saveOrEditButton = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.button4 = new System.Windows.Forms.Button();
            this.otherSitsCheckBox = new CoolProxy.Controls.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).BeginInit();
            this.SuspendLayout();
            // 
            // comboBox1
            // 
            this.comboBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Location = new System.Drawing.Point(12, 88);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(253, 21);
            this.comboBox1.TabIndex = 0;
            this.comboBox1.SelectedIndexChanged += new System.EventHandler(this.comboBox1_SelectedIndexChanged);
            // 
            // dataGridView
            // 
            this.dataGridView.AllowUserToAddRows = false;
            this.dataGridView.AllowUserToDeleteRows = false;
            this.dataGridView.AllowUserToResizeColumns = false;
            this.dataGridView.AllowUserToResizeRows = false;
            dataGridViewCellStyle4.BackColor = System.Drawing.Color.Silver;
            this.dataGridView.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle4;
            this.dataGridView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView.ColumnHeadersVisible = false;
            this.dataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Column1});
            dataGridViewCellStyle5.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle5.BackColor = System.Drawing.SystemColors.ControlDark;
            dataGridViewCellStyle5.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle5.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle5.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle5.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle5.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dataGridView.DefaultCellStyle = dataGridViewCellStyle5;
            this.dataGridView.Location = new System.Drawing.Point(12, 115);
            this.dataGridView.MultiSelect = false;
            this.dataGridView.Name = "dataGridView";
            this.dataGridView.ReadOnly = true;
            this.dataGridView.RowHeadersVisible = false;
            this.dataGridView.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            dataGridViewCellStyle6.BackColor = System.Drawing.SystemColors.ControlDark;
            this.dataGridView.RowsDefaultCellStyle = dataGridViewCellStyle6;
            this.dataGridView.RowTemplate.Height = 18;
            this.dataGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridView.Size = new System.Drawing.Size(226, 185);
            this.dataGridView.TabIndex = 23;
            this.dataGridView.SelectionChanged += new System.EventHandler(this.dataGridView_SelectionChanged);
            // 
            // Column1
            // 
            this.Column1.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.Column1.HeaderText = "Column1";
            this.Column1.Name = "Column1";
            this.Column1.ReadOnly = true;
            // 
            // checkBox1
            // 
            this.checkBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.checkBox1.AutoSize = true;
            this.checkBox1.Enabled = false;
            this.checkBox1.Location = new System.Drawing.Point(18, 306);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(52, 17);
            this.checkBox1.TabIndex = 24;
            this.checkBox1.Text = "Cycle";
            this.checkBox1.UseVisualStyleBackColor = true;
            this.checkBox1.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged);
            // 
            // checkBox2
            // 
            this.checkBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.checkBox2.AutoSize = true;
            this.checkBox2.Enabled = false;
            this.checkBox2.Location = new System.Drawing.Point(82, 306);
            this.checkBox2.Name = "checkBox2";
            this.checkBox2.Size = new System.Drawing.Size(108, 17);
            this.checkBox2.TabIndex = 25;
            this.checkBox2.Text = "Randomise Order";
            this.checkBox2.UseVisualStyleBackColor = true;
            this.checkBox2.CheckedChanged += new System.EventHandler(this.checkBox2_CheckedChanged);
            // 
            // moveDownButton
            // 
            this.moveDownButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.moveDownButton.Enabled = false;
            this.moveDownButton.Location = new System.Drawing.Point(244, 160);
            this.moveDownButton.Name = "moveDownButton";
            this.moveDownButton.Size = new System.Drawing.Size(21, 39);
            this.moveDownButton.TabIndex = 28;
            this.moveDownButton.Text = "⯆";
            this.moveDownButton.UseVisualStyleBackColor = true;
            this.moveDownButton.Click += new System.EventHandler(this.moveDownButton_Click);
            // 
            // removeAnimButton
            // 
            this.removeAnimButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.removeAnimButton.Enabled = false;
            this.removeAnimButton.Location = new System.Drawing.Point(244, 280);
            this.removeAnimButton.Name = "removeAnimButton";
            this.removeAnimButton.Size = new System.Drawing.Size(21, 20);
            this.removeAnimButton.TabIndex = 27;
            this.removeAnimButton.Text = "🗑";
            this.removeAnimButton.UseVisualStyleBackColor = true;
            this.removeAnimButton.Click += new System.EventHandler(this.removeAnimButton_Click);
            // 
            // moveUpButton
            // 
            this.moveUpButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.moveUpButton.Enabled = false;
            this.moveUpButton.Location = new System.Drawing.Point(244, 115);
            this.moveUpButton.Name = "moveUpButton";
            this.moveUpButton.Size = new System.Drawing.Size(21, 39);
            this.moveUpButton.TabIndex = 26;
            this.moveUpButton.Text = "⯅";
            this.moveUpButton.UseVisualStyleBackColor = true;
            this.moveUpButton.Click += new System.EventHandler(this.moveUpButton_Click);
            // 
            // previousAnimButton
            // 
            this.previousAnimButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.previousAnimButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.previousAnimButton.Location = new System.Drawing.Point(12, 355);
            this.previousAnimButton.Name = "previousAnimButton";
            this.previousAnimButton.Size = new System.Drawing.Size(110, 23);
            this.previousAnimButton.TabIndex = 29;
            this.previousAnimButton.Text = "←";
            this.previousAnimButton.UseVisualStyleBackColor = true;
            this.previousAnimButton.Click += new System.EventHandler(this.previousAnimButton_Click);
            // 
            // nextAnimButton
            // 
            this.nextAnimButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.nextAnimButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.nextAnimButton.Location = new System.Drawing.Point(128, 355);
            this.nextAnimButton.Name = "nextAnimButton";
            this.nextAnimButton.Size = new System.Drawing.Size(110, 23);
            this.nextAnimButton.TabIndex = 30;
            this.nextAnimButton.Text = "→";
            this.nextAnimButton.UseVisualStyleBackColor = true;
            this.nextAnimButton.Click += new System.EventHandler(this.nextAnimButton_Click);
            // 
            // checkBox3
            // 
            this.checkBox3.AutoSize = true;
            this.checkBox3.Checked = true;
            this.checkBox3.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox3.EnabledSetting = null;
            this.checkBox3.Location = new System.Drawing.Point(18, 42);
            this.checkBox3.Name = "checkBox3";
            this.checkBox3.Setting = "OverrideSits";
            this.checkBox3.Size = new System.Drawing.Size(86, 17);
            this.checkBox3.TabIndex = 31;
            this.checkBox3.Text = "Override Sits";
            this.checkBox3.UseVisualStyleBackColor = true;
            // 
            // numericUpDown1
            // 
            this.numericUpDown1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.numericUpDown1.Location = new System.Drawing.Point(186, 329);
            this.numericUpDown1.Name = "numericUpDown1";
            this.numericUpDown1.Size = new System.Drawing.Size(79, 20);
            this.numericUpDown1.TabIndex = 33;
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(15, 331);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(102, 13);
            this.label1.TabIndex = 34;
            this.label1.Text = "Cycle time (second):";
            // 
            // collapseButton
            // 
            this.collapseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.collapseButton.Location = new System.Drawing.Point(244, 355);
            this.collapseButton.Name = "collapseButton";
            this.collapseButton.Size = new System.Drawing.Size(21, 23);
            this.collapseButton.TabIndex = 35;
            this.collapseButton.Text = "⯆";
            this.collapseButton.UseVisualStyleBackColor = true;
            this.collapseButton.Click += new System.EventHandler(this.collapseButton_Click);
            // 
            // checkBox5
            // 
            this.checkBox5.AutoSize = true;
            this.checkBox5.Enabled = false;
            this.checkBox5.Location = new System.Drawing.Point(110, 42);
            this.checkBox5.Name = "checkBox5";
            this.checkBox5.Size = new System.Drawing.Size(67, 17);
            this.checkBox5.TabIndex = 36;
            this.checkBox5.Text = "Be smart";
            this.checkBox5.UseVisualStyleBackColor = true;
            // 
            // checkBox6
            // 
            this.checkBox6.AutoSize = true;
            this.checkBox6.Location = new System.Drawing.Point(18, 65);
            this.checkBox6.Name = "checkBox6";
            this.checkBox6.Size = new System.Drawing.Size(163, 17);
            this.checkBox6.TabIndex = 37;
            this.checkBox6.Text = "Disable Stands in Mouselook";
            this.checkBox6.UseVisualStyleBackColor = true;
            // 
            // aoNameComboBox
            // 
            this.aoNameComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.aoNameComboBox.Enabled = false;
            this.aoNameComboBox.FormattingEnabled = true;
            this.aoNameComboBox.Location = new System.Drawing.Point(12, 12);
            this.aoNameComboBox.Name = "aoNameComboBox";
            this.aoNameComboBox.Size = new System.Drawing.Size(226, 21);
            this.aoNameComboBox.TabIndex = 38;
            // 
            // saveOrEditButton
            // 
            this.saveOrEditButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.saveOrEditButton.Enabled = false;
            this.saveOrEditButton.Location = new System.Drawing.Point(244, 13);
            this.saveOrEditButton.Name = "saveOrEditButton";
            this.saveOrEditButton.Size = new System.Drawing.Size(21, 20);
            this.saveOrEditButton.TabIndex = 39;
            this.saveOrEditButton.Text = "✓";
            this.saveOrEditButton.UseVisualStyleBackColor = true;
            this.saveOrEditButton.Click += new System.EventHandler(this.saveOrEditButton_Click);
            // 
            // button3
            // 
            this.button3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button3.Enabled = false;
            this.button3.Location = new System.Drawing.Point(244, 39);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(21, 20);
            this.button3.TabIndex = 40;
            this.button3.Text = "🗑";
            this.button3.UseVisualStyleBackColor = true;
            // 
            // button4
            // 
            this.button4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button4.Enabled = false;
            this.button4.Location = new System.Drawing.Point(217, 39);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(21, 20);
            this.button4.TabIndex = 41;
            this.button4.Text = "+";
            this.button4.UseVisualStyleBackColor = true;
            // 
            // otherSitsCheckBox
            // 
            this.otherSitsCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.otherSitsCheckBox.AutoSize = true;
            this.otherSitsCheckBox.Checked = true;
            this.otherSitsCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.otherSitsCheckBox.EnabledSetting = null;
            this.otherSitsCheckBox.Location = new System.Drawing.Point(217, 363);
            this.otherSitsCheckBox.Name = "otherSitsCheckBox";
            this.otherSitsCheckBox.Setting = "OverrideSits";
            this.otherSitsCheckBox.Size = new System.Drawing.Size(43, 17);
            this.otherSitsCheckBox.TabIndex = 42;
            this.otherSitsCheckBox.Text = "Sits";
            this.otherSitsCheckBox.UseVisualStyleBackColor = true;
            this.otherSitsCheckBox.Visible = false;
            // 
            // ClientAOEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(277, 389);
            this.Controls.Add(this.otherSitsCheckBox);
            this.Controls.Add(this.button4);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.saveOrEditButton);
            this.Controls.Add(this.aoNameComboBox);
            this.Controls.Add(this.checkBox6);
            this.Controls.Add(this.checkBox5);
            this.Controls.Add(this.collapseButton);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.numericUpDown1);
            this.Controls.Add(this.checkBox3);
            this.Controls.Add(this.nextAnimButton);
            this.Controls.Add(this.previousAnimButton);
            this.Controls.Add(this.moveDownButton);
            this.Controls.Add(this.removeAnimButton);
            this.Controls.Add(this.moveUpButton);
            this.Controls.Add(this.checkBox2);
            this.Controls.Add(this.checkBox1);
            this.Controls.Add(this.dataGridView);
            this.Controls.Add(this.comboBox1);
            this.MinimumSize = new System.Drawing.Size(293, 428);
            this.Name = "ClientAOEditor";
            this.ShowIcon = false;
            this.Text = "Animation Override";
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.DataGridView dataGridView;
        private System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.CheckBox checkBox2;
        private System.Windows.Forms.Button moveDownButton;
        private System.Windows.Forms.Button removeAnimButton;
        private System.Windows.Forms.Button moveUpButton;
        private System.Windows.Forms.Button previousAnimButton;
        private System.Windows.Forms.Button nextAnimButton;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column1;
        private CoolProxy.Controls.CheckBox checkBox3;
        private System.Windows.Forms.NumericUpDown numericUpDown1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button collapseButton;
        private System.Windows.Forms.CheckBox checkBox5;
        private System.Windows.Forms.CheckBox checkBox6;
        private System.Windows.Forms.ComboBox aoNameComboBox;
        private System.Windows.Forms.Button saveOrEditButton;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button button4;
        private CoolProxy.Controls.CheckBox otherSitsCheckBox;
    }
}