using OpenMetaverse.StructuredData;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CoolProxy
{
    public partial class PreferencesForm
    {
        List<string> pluginList = new List<string>();

        bool TellToRestart = false;

        private void dataGridView1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
            else e.Effect = DragDropEffects.None;
        }

        private void dataGridView1_DragDrop(object sender, DragEventArgs e)
        {
            // Handle FileDrop data.
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                // Assign the file names to a string array, in 
                // case the user has selected multiple files.
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                try
                {
                    AddPlugins(files);

                    SavePlugins();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    return;
                }
            }
        }

        void AddPlugins(string[] files)
        {
            foreach (string str in files)
            {
                if (pluginList.Contains(str))
                    continue;

                if (Path.GetExtension(str) != ".dll")
                    continue;

                pluginList.Add(str);

                // Get the file version.
                FileVersionInfo myFileVersionInfo = FileVersionInfo.GetVersionInfo(str);

                pluginsDataGridView.Rows.Add(Path.GetFileName(str), myFileVersionInfo.FileVersion, str);
            }
        }


        private void SavePlugins(bool is_startup = false)
        {
            pluginList.Clear();
            OSDArray osd_array = new OSDArray();
            foreach (DataGridViewRow row in pluginsDataGridView.Rows)
            {
                string str = (string)row.Cells[2].Value;
                pluginList.Add(str);
                osd_array.Add(str);
            }

            Program.Frame.Settings.setOSD("PluginList", osd_array);

            if (!is_startup) TellToRestart = true;
        }

        private void removePluginButton_Click(object sender, EventArgs e)
        {
            if (pluginsDataGridView.SelectedRows.Count > 0)
            {
                foreach (DataGridViewRow row in pluginsDataGridView.SelectedRows)
                {
                    string file = (string)row.Cells[2].Value;
                    pluginList.Remove(file);
                    pluginsDataGridView.Rows.Remove(row);
                }

                SavePlugins();
            }
        }

        private void addPluginButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFile = new OpenFileDialog();
            openFile.Filter = "Plugins|*.dll";
            openFile.Multiselect = true;

            if (openFile.ShowDialog() == DialogResult.OK)
            {
                AddPlugins(openFile.FileNames);
                SavePlugins();
            }
        }

        private void pluginsDataGridView_SelectionChanged(object sender, EventArgs e)
        {
            bool can_move_up = pluginsDataGridView.Rows.Count > 0;

            if (can_move_up)
            {
                if (pluginsDataGridView.Rows[0].Selected)
                {
                    can_move_up = false;
                }
            }

            int i = pluginsDataGridView.Rows.GetLastRow(DataGridViewElementStates.Visible);
            bool can_move_down = true;

            if (i == -1) can_move_down = false;
            else if (pluginsDataGridView.Rows[i].Selected) can_move_down = false;

            movePluginUp.Enabled = can_move_up;
            movePluginDown.Enabled = can_move_down;
            removePluginButton.Enabled = pluginsDataGridView.SelectedRows.Count > 0;
        }

        private void movePluginUp_Click(object sender, EventArgs e)
        {
            pluginsDataGridView.SuspendLayout();

            Dictionary<int, DataGridViewRow> rows = new Dictionary<int, DataGridViewRow>();

            List<int> selected = new List<int>();

            foreach (DataGridViewRow row in pluginsDataGridView.Rows)
            {
                rows.Add(row.Index, row);
                if (row.Selected) selected.Add(row.Index);
            }

            int first = pluginsDataGridView.FirstDisplayedScrollingRowIndex;

            pluginsDataGridView.Rows.Clear();

            foreach (var pair in rows)
            {
                bool was_selected = selected.Contains(pair.Key);

                if (was_selected)
                {
                    int index = pair.Key;
                    if (index > 0) index--;
                    pluginsDataGridView.Rows.Insert(index, pair.Value);
                }
                else
                {
                    pluginsDataGridView.Rows.Add(pair.Value);
                }
            }

            foreach (DataGridViewRow row in pluginsDataGridView.Rows)
            {
                row.Selected = selected.Contains(row.Index + 1);
            }

            pluginsDataGridView.FirstDisplayedScrollingRowIndex = first;

            pluginsDataGridView.ResumeLayout();

            SavePlugins();
        }

        private void movePluginDown_Click(object sender, EventArgs e)
        {
            pluginsDataGridView.SuspendLayout();

            Dictionary<int, DataGridViewRow> rows = new Dictionary<int, DataGridViewRow>();

            List<int> selected = new List<int>();

            foreach (DataGridViewRow row in pluginsDataGridView.Rows)
            {
                rows.Add(row.Index, row);
                if (row.Selected) selected.Add(row.Index);
            }

            int first = pluginsDataGridView.FirstDisplayedScrollingRowIndex;
            pluginsDataGridView.Rows.Clear();

            foreach (var pair in rows.Reverse())
            {
                if (selected.Contains(pair.Key))
                    pluginsDataGridView.Rows.Insert(1, pair.Value);
                else
                    pluginsDataGridView.Rows.Insert(0, pair.Value);
            }

            foreach (DataGridViewRow row in pluginsDataGridView.Rows)
            {
                row.Selected = selected.Contains(row.Index - 1);
            }

            pluginsDataGridView.FirstDisplayedScrollingRowIndex = first;

            pluginsDataGridView.ResumeLayout();

            SavePlugins();
        }
    }
}
