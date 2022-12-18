using OpenMetaverse;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CoolProxy.Plugins.ClientAO
{
    public partial class ClientAOEditor : Form
    {
        private ClientAOPlugin Plugin;

        private AOState SelectedState = null;

        bool IsOpen = true;

        Size OpenMinSize = new Size(293, 428);
        Size ClosedMinSize = new Size(293, 110);

        Point PreviousButtonClosedPos = new Point(12, 40);
        Point PreviousButtonOpenPos = new Point(12, 355);

        Point NextButtonClosedPos = new Point(80, 40);
        Point NextButtonOpenPos = new Point(128, 355);

        Size ButtonSizeOpen = new Size(110, 23);
        Size ButtonSizeClosed = new Size(60, 23);

        static readonly Font RegularFont = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Regular);
        static readonly Font BoldFont = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Bold);

        public ClientAOEditor(ClientAOPlugin plugin)
        {
            Plugin = plugin;
            InitializeComponent();

            var states = ClientAOPlugin.DefaultAnimToState.Values.Distinct().ToArray();
            comboBox1.Items.AddRange(states);
            comboBox1.SelectedIndex = 0;
        }

        void RefreshList()
        {
            dataGridView.Rows.Clear();
            string state_name = comboBox1.SelectedItem.ToString();
            if (Plugin.Overrides.TryGetValue(state_name, out SelectedState))
            {
                dataGridView.SelectionChanged -= dataGridView_SelectionChanged;

                foreach (var entry in SelectedState.Entries)
                {
                    int i = dataGridView.Rows.Add(entry.Name);
                    dataGridView.Rows[i].Tag = entry;
                }

                checkBox1.Checked = SelectedState.Cycle;
                checkBox2.Checked = SelectedState.Randomise;

                checkBox1.Enabled = true;
                checkBox2.Enabled = checkBox1.Checked;

                UpdateListButtons();

                dataGridView.SelectionChanged += dataGridView_SelectionChanged;
            }
        }

        public void UpdateUI()
        {
            aoNameComboBox.Text = Plugin.AOName;
            RefreshList();
        }

        public void UpdatePlaying()
        {
            var current_anims = Plugin.Current.Select(x => x.CurrentAnim.AssetID);
            foreach(DataGridViewRow row in dataGridView.Rows)
            {
                if(current_anims.Contains(((AOAnim)row.Tag).AssetID))
                {
                    row.Cells[0].Style.Font = BoldFont;
                }
                else
                {
                    row.Cells[0].Style.Font = RegularFont;
                }
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            RefreshList();
            UpdatePlaying();
        }

        void changeAnim(bool next)
        {
            Dictionary<UUID, bool> anims = new Dictionary<UUID, bool>();
            foreach (var state in Plugin.Current)
            {
                var id = state.CurrentAnim.AssetID;
                if (next ? state.Next() : state.Previous())
                {
                    anims.Add(id, false);
                    anims.Add(state.CurrentAnim.AssetID, true);
                }
            }

            if (anims.Count > 0)
            {
                Plugin.Proxy.Agent.Animate(anims, false);
                UpdatePlaying();
            }
        }

        private void nextAnimButton_Click(object sender, EventArgs e)
        {
            changeAnim(true);
        }

        private void previousAnimButton_Click(object sender, EventArgs e)
        {
            changeAnim(false);
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            SelectedState.Cycle = checkBox1.Checked;
            checkBox2.Enabled = checkBox1.Checked;
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            SelectedState.Randomise = checkBox2.Checked;
        }

        void UpdateCollapse()
        {
            collapseButton.Text = IsOpen ? "⯅" : "⯆";

            foreach (Control control in Controls)
            {
                if (control != nextAnimButton && control != previousAnimButton && control != aoNameComboBox && control != saveOrEditButton)
                {
                    control.Visible = IsOpen;
                }
            }

            this.MinimumSize = IsOpen ? OpenMinSize : ClosedMinSize;
            this.Size = IsOpen ? OpenMinSize : ClosedMinSize;

            previousAnimButton.Location = IsOpen ? PreviousButtonOpenPos : PreviousButtonClosedPos;
            previousAnimButton.Size = IsOpen ? ButtonSizeOpen : ButtonSizeClosed;

            nextAnimButton.Location = IsOpen ? NextButtonOpenPos : NextButtonClosedPos;
            nextAnimButton.Size = IsOpen ? ButtonSizeOpen : ButtonSizeClosed;

            saveOrEditButton.Text = IsOpen ? "✓" : "🔧";
            saveOrEditButton.Enabled = !IsOpen;

            otherSitsCheckBox.Visible = !IsOpen;

            //aoNameComboBox.DropDownStyle = IsOpen ? ComboBoxStyle.DropDown : ComboBoxStyle.DropDownList;
        }

        private void collapseButton_Click(object sender, EventArgs e)
        {
            IsOpen = !IsOpen;
            UpdateCollapse();
        }

        private void saveOrEditButton_Click(object sender, EventArgs e)
        {
            IsOpen = !IsOpen;
            UpdateCollapse();
        }

        private void dataGridView_SelectionChanged(object sender, EventArgs e)
        {
            UpdateListButtons();
        }

        void UpdateListButtons()
        {
            var index = dataGridView.SelectedRows.Count > 0 ? dataGridView.SelectedRows[0]?.Index ?? 0 : 0;

            removeAnimButton.Enabled = dataGridView.SelectedRows.Count > 0;
            moveUpButton.Enabled = index > 0 && removeAnimButton.Enabled;
            moveDownButton.Enabled = index < dataGridView.Rows.Count - 1 && removeAnimButton.Enabled;
        }

        private void removeAnimButton_Click(object sender, EventArgs e)
        {
            var selected = (AOAnim)dataGridView.SelectedRows[0].Tag;
            int index = SelectedState.Entries.IndexOf(selected);
            SelectedState.Entries.RemoveAt(index);
            dataGridView.Rows.RemoveAt(index);
        }

        private void moveUpButton_Click(object sender, EventArgs e)
        {
            moveRow(dataGridView.SelectedRows[0], true);
        }

        private void moveDownButton_Click(object sender, EventArgs e)
        {
            moveRow(dataGridView.SelectedRows[0], false);
        }

        void moveRow(DataGridViewRow row, bool up)
        {
            dataGridView.SelectionChanged -= dataGridView_SelectionChanged;

            int dir = up ? -1 : 1;

            var selected = (AOAnim)row.Tag;

            var entries = SelectedState.Entries;

            int find = entries.IndexOf(selected);

            entries.RemoveAt(find);
            entries.Insert(find + dir, selected);

            dataGridView.Rows.Remove(row);
            dataGridView.Rows.Insert(find + dir, row);

            dataGridView.SelectionChanged += dataGridView_SelectionChanged;

            row.Selected = true;
        }

        private void ClientAOEditor_DragOver(object sender, DragEventArgs e)
        {
            var o = e.Data.GetData(e.Data.GetFormats()[0]);
            if (o is InventoryItem)
            {
                var item = o as InventoryItem;

                if (item.InventoryType == InventoryType.Notecard)
                    e.Effect = DragDropEffects.Move;
            }
        }

        private void ClientAOEditor_DragDrop(object sender, DragEventArgs e)
        {
            var item = e.Data.GetData(e.Data.GetFormats()[0]) as InventoryItem;
            Plugin.LoadNotecard(item);
        }
    }
}
