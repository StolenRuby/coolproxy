
namespace CoolProxy.Plugins.Messages
{
    partial class MessageLogForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MessageLogForm));
            this.button1 = new System.Windows.Forms.Button();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.noFilterToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fewerSpammyMessagesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fewerSpammyMessagesnoSoundsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.objectUpdatesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.button2 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.button4 = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.listViewSessions = new ListViewNoFlicker();
            this.columnHeaderCounter = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderDirection = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderNet = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderType = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderSummary = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(5, 5);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(28, 23);
            this.button1.TabIndex = 3;
            this.button1.Text = "...";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.noFilterToolStripMenuItem,
            this.fewerSpammyMessagesToolStripMenuItem,
            this.fewerSpammyMessagesnoSoundsToolStripMenuItem,
            this.objectUpdatesToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(275, 92);
            // 
            // noFilterToolStripMenuItem
            // 
            this.noFilterToolStripMenuItem.Name = "noFilterToolStripMenuItem";
            this.noFilterToolStripMenuItem.Size = new System.Drawing.Size(274, 22);
            this.noFilterToolStripMenuItem.Text = "No filter";
            this.noFilterToolStripMenuItem.Click += new System.EventHandler(this.noFilterToolStripMenuItem_Click);
            // 
            // fewerSpammyMessagesToolStripMenuItem
            // 
            this.fewerSpammyMessagesToolStripMenuItem.Name = "fewerSpammyMessagesToolStripMenuItem";
            this.fewerSpammyMessagesToolStripMenuItem.Size = new System.Drawing.Size(274, 22);
            this.fewerSpammyMessagesToolStripMenuItem.Tag = resources.GetString("fewerSpammyMessagesToolStripMenuItem.Tag");
            this.fewerSpammyMessagesToolStripMenuItem.Text = "Fewer spammy messages";
            this.fewerSpammyMessagesToolStripMenuItem.Click += new System.EventHandler(this.noFilterToolStripMenuItem_Click);
            // 
            // fewerSpammyMessagesnoSoundsToolStripMenuItem
            // 
            this.fewerSpammyMessagesnoSoundsToolStripMenuItem.Name = "fewerSpammyMessagesnoSoundsToolStripMenuItem";
            this.fewerSpammyMessagesnoSoundsToolStripMenuItem.Size = new System.Drawing.Size(274, 22);
            this.fewerSpammyMessagesnoSoundsToolStripMenuItem.Tag = resources.GetString("fewerSpammyMessagesnoSoundsToolStripMenuItem.Tag");
            this.fewerSpammyMessagesnoSoundsToolStripMenuItem.Text = "Fewer spammy messages (no sounds)";
            this.fewerSpammyMessagesnoSoundsToolStripMenuItem.Click += new System.EventHandler(this.noFilterToolStripMenuItem_Click);
            // 
            // objectUpdatesToolStripMenuItem
            // 
            this.objectUpdatesToolStripMenuItem.Name = "objectUpdatesToolStripMenuItem";
            this.objectUpdatesToolStripMenuItem.Size = new System.Drawing.Size(274, 22);
            this.objectUpdatesToolStripMenuItem.Tag = "ObjectUpdateCached ObjectUpdate ObjectUpdateCompressed ImprovedTerseObjectUpdate " +
    "KillObject RequestMultipleObjects";
            this.objectUpdatesToolStripMenuItem.Text = "Object updates";
            this.objectUpdatesToolStripMenuItem.Click += new System.EventHandler(this.noFilterToolStripMenuItem_Click);
            // 
            // button2
            // 
            this.button2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button2.Location = new System.Drawing.Point(349, 5);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(28, 23);
            this.button2.TabIndex = 4;
            this.button2.Text = "✔";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // button3
            // 
            this.button3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button3.Location = new System.Drawing.Point(308, 34);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(69, 23);
            this.button3.TabIndex = 5;
            this.button3.Text = "Clear";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // textBox1
            // 
            this.textBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox1.Location = new System.Drawing.Point(39, 7);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(304, 20);
            this.textBox1.TabIndex = 6;
            this.textBox1.Text = resources.GetString("textBox1.Text");
            // 
            // button4
            // 
            this.button4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button4.Enabled = false;
            this.button4.Location = new System.Drawing.Point(197, 532);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(180, 23);
            this.button4.TabIndex = 7;
            this.button4.Text = "Send to Message Builder";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(8, 38);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(139, 13);
            this.label1.TabIndex = 8;
            this.label1.Text = "Showing 0 messages from 0";
            // 
            // textBox2
            // 
            this.textBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox2.Location = new System.Drawing.Point(5, 308);
            this.textBox2.Multiline = true;
            this.textBox2.Name = "textBox2";
            this.textBox2.ReadOnly = true;
            this.textBox2.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBox2.Size = new System.Drawing.Size(372, 218);
            this.textBox2.TabIndex = 9;
            // 
            // listViewSessions
            // 
            this.listViewSessions.AllowColumnReorder = true;
            this.listViewSessions.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listViewSessions.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderCounter,
            this.columnHeaderDirection,
            this.columnHeaderNet,
            this.columnHeaderType,
            this.columnHeaderSummary});
            this.listViewSessions.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.listViewSessions.FullRowSelect = true;
            this.listViewSessions.GridLines = true;
            this.listViewSessions.HideSelection = false;
            this.listViewSessions.Location = new System.Drawing.Point(5, 63);
            this.listViewSessions.MultiSelect = false;
            this.listViewSessions.Name = "listViewSessions";
            this.listViewSessions.Size = new System.Drawing.Size(372, 239);
            this.listViewSessions.TabIndex = 1;
            this.listViewSessions.UseCompatibleStateImageBehavior = false;
            this.listViewSessions.View = System.Windows.Forms.View.Details;
            this.listViewSessions.SelectedIndexChanged += new System.EventHandler(this.listViewSessions_SelectedIndexChanged);
            // 
            // columnHeaderCounter
            // 
            this.columnHeaderCounter.Tag = "number";
            this.columnHeaderCounter.Text = "#";
            this.columnHeaderCounter.Width = 52;
            // 
            // columnHeaderDirection
            // 
            this.columnHeaderDirection.Tag = "string";
            this.columnHeaderDirection.Text = "";
            this.columnHeaderDirection.Width = 37;
            // 
            // columnHeaderNet
            // 
            this.columnHeaderNet.Text = "Host";
            this.columnHeaderNet.Width = 150;
            // 
            // columnHeaderType
            // 
            this.columnHeaderType.Tag = "string";
            this.columnHeaderType.Text = "Name";
            this.columnHeaderType.Width = 184;
            // 
            // columnHeaderSummary
            // 
            this.columnHeaderSummary.Text = "Summary";
            this.columnHeaderSummary.Width = 120;
            // 
            // MessageLoggerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(384, 561);
            this.Controls.Add(this.textBox2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.button4);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.listViewSessions);
            this.MinimumSize = new System.Drawing.Size(400, 600);
            this.Name = "MessageLoggerForm";
            this.ShowIcon = false;
            this.Text = "Message Log";
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private ListViewNoFlicker listViewSessions;
        private System.Windows.Forms.ColumnHeader columnHeaderCounter;
        private System.Windows.Forms.ColumnHeader columnHeaderDirection;
        private System.Windows.Forms.ColumnHeader columnHeaderType;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ColumnHeader columnHeaderNet;
        private System.Windows.Forms.ColumnHeader columnHeaderSummary;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem noFilterToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem fewerSpammyMessagesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem fewerSpammyMessagesnoSoundsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem objectUpdatesToolStripMenuItem;
    }
}