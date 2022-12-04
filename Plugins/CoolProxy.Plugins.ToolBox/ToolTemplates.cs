using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CoolProxy.Plugins.ToolBox
{
    public class ToolBoxControl : UserControl
    {
        public bool Default { get; set; } = true;
        public string ID { get; set; } = string.Empty;

        public ToolBoxControl() : base()
        {

        }
    };

    public class SimpleToggleFormButton : ToolBoxControl
    {
        CheckBox mCheckbox = null;

        public SimpleToggleFormButton(string label, Form form)
        {
            mCheckbox = new CheckBox();
            mCheckbox.Text = label;
            mCheckbox.Location = new System.Drawing.Point(5, 1);
            mCheckbox.Size = new System.Drawing.Size(220, 22);
            mCheckbox.Appearance = Appearance.Button;
            mCheckbox.TextAlign = ContentAlignment.MiddleCenter;

            mCheckbox.Checked = form.Visible;

            form.VisibleChanged += (x, y) =>
            {
                mCheckbox.Checked = form.Visible;
            };

            mCheckbox.CheckedChanged += (x, y) =>
            {
                form.Visible = mCheckbox.Checked;
            };

            form.FormClosing += (x, y) =>
            {
                y.Cancel = true;
                form.Visible = false;
            };

            this.Size = new System.Drawing.Size(230, 23);
            this.Controls.Add(mCheckbox);
        }
    };

    public class SimpleButton : ToolBoxControl
    {
        Button mButton = null;

        public SimpleButton(string label, EventHandler clicked) : base()
        {
            mButton = new Button();
            mButton.Text = label;
            mButton.Location = new System.Drawing.Point(5, 1);
            mButton.Size = new System.Drawing.Size(220, 22);
            mButton.Click += clicked;

            this.Size = new System.Drawing.Size(230, 23);
            this.Controls.Add(mButton);
        }
    }
    public class SimpleLabel : ToolBoxControl
    {
        Label mLabel = null;

        public string Label { get { return mLabel.Text; } }

        public SimpleLabel(string label)
        {
            mLabel = new Label();
            mLabel.Text = label;
            mLabel.Location = new System.Drawing.Point(5, 1);
            mLabel.Size = new System.Drawing.Size(220, 14);

            this.Size = new System.Drawing.Size(230, 14);
            this.Controls.Add(mLabel);
        }
    }

    public class SimpleCheckbox : ToolBoxControl
    {
        CheckBox mCheckbox = null;

        public SimpleCheckbox(string label)
        {
            mCheckbox = new CheckBox();
            mCheckbox.Text = label;
            mCheckbox.Location = new System.Drawing.Point(10, 1);
            mCheckbox.Size = new System.Drawing.Size(220, 18);

            this.Size = new System.Drawing.Size(230, 19);
            this.Controls.Add(mCheckbox);
        }
    }

    public class SimpleSeparator : ToolBoxControl
    {
        public SimpleSeparator()
        {
            PictureBox box = new PictureBox();
            box.Size = new System.Drawing.Size(220, 1);
            box.Location = new System.Drawing.Point(5, 0);
            box.BackColor = Color.Gray;

            this.Size = new Size(230, 1);
            this.Controls.Add(box);
        }
    }
}
