using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CoolProxy.Plugins.ToolBox
{
    public partial class ToolBoxEditor : Form
    {
        ToolboxPlugin Plugin;

        List<string> Used = new List<string>();

        public ToolBoxEditor(ToolboxPlugin toolbox)
        {
            Plugin = toolbox;
            InitializeComponent();

            var tools = toolbox.GetToolbx();
            foreach(var tool in tools)
            {
                int i = -1;
                if(tool is SimpleSeparator)
                {
                    i = toolsDGV.Rows.Add("- Separator -");
                }
                else if(tool is SimpleLabel)
                {
                    var label = tool as SimpleLabel;
                    i = toolsDGV.Rows.Add("Label {text: " + label.Label + "}");
                }
                else
                {
                    i = toolsDGV.Rows.Add(tool.ID);
                    Used.Add(tool.ID);
                }

                if(i != -1)
                {
                    toolsDGV.Rows[i].Tag = tool;
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (toolsDGV.SelectedRows.Count != 1) return;
            var row = toolsDGV.SelectedRows[0];

            int i = row.Index;

            toolsDGV.Rows.Remove(row);
            toolsDGV.Rows.Insert(i - 1, row);
            toolsDGV.ClearSelection();
            row.Selected = true;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (toolsDGV.SelectedRows.Count != 1) return;
            var row = toolsDGV.SelectedRows[0];

            int i = row.Index;

            toolsDGV.Rows.Remove(row);
            toolsDGV.Rows.Insert(i + 1, row);
            toolsDGV.ClearSelection();
            row.Selected = true;
        }

        private void ToolBoxEditor_FormClosing(object sender, FormClosingEventArgs e)
        {
            List<ToolBoxControl> controls = new List<ToolBoxControl>();
            foreach(DataGridViewRow row in toolsDGV.Rows)
            {
                controls.Add(row.Tag as ToolBoxControl);
            }

            Plugin.ApplyToolBox(controls.ToArray());
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var available = Plugin.GetAvailable().Where(x => !Used.Contains(x.ID));
            var names = available.Select(x => x.ID).ToArray();

            ListDialogBox dialogBox = new ListDialogBox(names);
            dialogBox.TopMost = true;
            if(dialogBox.ShowDialog() == DialogResult.OK)
            {
                string str = dialogBox.SelectedOption;

                var tool = available.First(x => x.ID == str);

                int i = toolsDGV.Rows.Add(tool.ID);
                toolsDGV.Rows[i].Tag = tool;
                Used.Add(tool.ID);
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (toolsDGV.SelectedRows.Count != 1) return;
            var row = toolsDGV.SelectedRows[0];

            ToolBoxControl control = row.Tag as ToolBoxControl;

            toolsDGV.Rows.Remove(row);
            Used.Remove(control.ID);
        }

        private void toolsDGV_SelectionChanged(object sender, EventArgs e)
        {

            if (toolsDGV.SelectedRows.Count != 1) return;
            var row = toolsDGV.SelectedRows[0];

            int i = row.Index;
            
            button1.Enabled = i > 0;
            button3.Enabled = i < toolsDGV.Rows.Count - 1;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            SimpleSeparator separator = new SimpleSeparator();
            int i = toolsDGV.Rows.Add("- Separator -");
            toolsDGV.Rows[i].Tag = separator;
        }

        private void button6_Click(object sender, EventArgs e)
        {
            TextBoxDialogBox dialogBox = new TextBoxDialogBox("", "Enter a label text");
            if(dialogBox.ShowDialog() == DialogResult.OK)
            {
                SimpleLabel label = new SimpleLabel(dialogBox.Result);
                int i = toolsDGV.Rows.Add("Label: {text: " + label.Label + "}");
                toolsDGV.Rows[i].Tag = label;
            }
        }
    }
}
