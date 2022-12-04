using GridProxy;
using OpenMetaverse;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static GridProxy.RegionManager;
using CoarseLocationUpdateEventArgs = GridProxy.CoarseLocationUpdateEventArgs;

namespace CoolProxy.Plugins.AvatarTracker
{
    public delegate void HandleAvatarPicker(UUID target);
    public delegate void HandleAvatarPickerList(List<UUID> targets);

    public class AvTrackerTest
    {
        private CoolProxyFrame Frame;
        private DataGridView dataGridView;

        public Dictionary<UUID, AvatarInTracker> trackerDictionary = new Dictionary<UUID, AvatarInTracker>();

        public class AvatarInTracker
        {
            public UUID UUID { get; private set; }

            public Avatar Avatar { get; set; }

            public string Name { get; set; } = null;

            public Vector3 CoarsePosition { get; set; } = new Vector3();

            public float Distance { get; set; } = 0.0f;

            public Color TagColor { get; set; } = Color.Gray;

            public AvatarInTracker(UUID id)
            {
                UUID = id;
            }

            public DataGridViewRow Row { get; set; } = null;
        }

        List<AvatarInTracker> avatarsInTracker = new List<AvatarInTracker>();

        ContextMenuStrip contextMenuStrip;


        private Dictionary<string, HandleAvatarPicker> singleSelectionMenu = new Dictionary<string, HandleAvatarPicker>();
        private Dictionary<string, HandleAvatarPickerList> multipleSelectionMenu = new Dictionary<string, HandleAvatarPickerList>();


        public void AddSingleMenuItem(string label, HandleAvatarPicker handle)
        {
            if (label == "-")
                label = "__SEPERATOR__" + singleSelectionMenu.Count.ToString(); // just to make it unique
            singleSelectionMenu.Add(label, handle);
        }


        public void AddMultipleMenuItem(string label, HandleAvatarPickerList handle)
        {
            multipleSelectionMenu.Add(label, handle);
        }



        public AvTrackerTest(DataGridView gridview, CoolProxyFrame frame)
        {
            dataGridView = gridview;
            Frame = frame;

            Frame.Grid.CoarseLocationUpdate += Grid_CoarseLocationUpdate;
            Frame.Objects.AvatarUpdate += Objects_AvatarUpdate;
            Frame.Objects.TerseObjectUpdate += Objects_TerseObjectUpdate;

            Frame.Network.OnRegionChanged += OnRegionChanged;
            
            contextMenuStrip = new ContextMenuStrip();
            contextMenuStrip.Opening += ContextMenuStrip_Opening;
            contextMenuStrip.ShowImageMargin = false;

            dataGridView.DoubleBuffered(true);
            dataGridView.ContextMenuStrip = contextMenuStrip;
            dataGridView.Sort(dataGridView.Columns[2], ListSortDirection.Ascending);

            Timer timer = new Timer();
            timer.Interval = 100;
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void OnRegionChanged(RegionProxy proxy)
        {
            lock (avatarsInTracker)
            {
                avatarsInTracker.Clear();
                dataGridView.Rows.Clear();
                trackerDictionary.Clear();
            }
        }

        private void ContextMenuStrip_Opening(object sender, CancelEventArgs e)
        {
            List<UUID> avatars = getSelectedAvatars();
            int count = avatars.Count;

            contextMenuStrip.Items.Clear();

            if (count < 1)
                e.Cancel = true;
            else if(count == 1)
            {
                UUID first = avatars[0];
                foreach(var item in singleSelectionMenu)
                {
                    if (item.Key.StartsWith("__SEPERATOR__"))
                        contextMenuStrip.Items.Add("-");
                    else
                        contextMenuStrip.Items.Add(item.Key, null, (x, y) => item.Value(first));
                }
            }
            else
            {
                foreach (var item in multipleSelectionMenu)
                {
                    contextMenuStrip.Items.Add(item.Key, null, (x, y) => item.Value(avatars));
                }
            }
        }

        List<UUID> getSelectedAvatars()
        {
            List<UUID> avatars = new List<UUID>();

            if (dataGridView.SelectedRows.Count < 1)
                return avatars;


            foreach (DataGridViewRow row in dataGridView.SelectedRows)
            {
                AvatarInTracker av = (AvatarInTracker)row.Tag;
                avatars.Add(av.UUID);
            }

            return avatars;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            dataGridView.SuspendLayout();

            Vector3 my_pos = Frame.Agent.SimPosition;

            List<UUID> selected = getSelectedAvatars();

            lock(avatarsInTracker)
            {
                var list = avatarsInTracker.ToArray();

                foreach (var item in list)
                {
                    Vector3 pos = item.Avatar != null ? item.Avatar.Position : item.CoarsePosition;

                    float dist = Vector3.Distance(my_pos, pos);

                    string pos_str = string.Format("{0:0}, {1:0}, {2:0}", pos.X, pos.Y, pos.Z);

                    DataGridViewRow row = item.Row;

                    if(row == null)
                    {
                        int id = dataGridView.Rows.Add(item.Name, pos_str, Math.Round(dist, 2));
                        row = item.Row = dataGridView.Rows[id];
                    }
                    else
                    {
                        row.Cells[0].Value = item.Name ?? item.UUID.ToString();
                        row.Cells[1].Value = pos_str;
                        row.Cells[2].Value = Math.Round(dist, 2);
                    }

                    row.Cells[2].Style.ForeColor = dist2Color(dist);
                    row.Tag = item;
                    row.Selected = selected.Contains(item.UUID);
                }
            }

            if (dataGridView.SortedColumn != null)
                dataGridView.Sort(dataGridView.SortedColumn, dataGridView.SortOrder == SortOrder.Ascending ? ListSortDirection.Ascending : ListSortDirection.Descending);

            dataGridView.ResumeLayout();
        }

        Color dist2Color(float dist)
        {
            if (dist < 10)
                return Color.Green;
            if (dist < 20)
                return Color.Yellow;
            if (dist < 100)
                return Color.Red;
            return Color.Gray;
        }

        private void Objects_TerseObjectUpdate(object sender, GridProxy.TerseObjectUpdateEventArgs e)
        {
            var entry = avatarsInTracker.FirstOrDefault(x => x.UUID == e.Prim.ID);
            if (entry != default(AvatarInTracker))
            {
                Avatar av;
                if (e.Region.ObjectsAvatars.TryGetValue(e.Prim.LocalID, out av))
                {
                    UpdateAvatar(av);
                }
            }
        }

        private void Objects_AvatarUpdate(object sender, AvatarUpdateEventArgs e)
        {
            UpdateAvatar(e.Avatar);
        }

        private void UpdateAvatar(Avatar avatar)
        {
            //if (dataGridView.InvokeRequired) dataGridView.Invoke((MethodInvoker)delegate { UpdateAvatar(avatar); });
            //else
            {
                var existing = avatarsInTracker.FirstOrDefault(x => x.UUID == avatar.ID);
                if (existing == default(AvatarInTracker))
                {
                    existing = new AvatarInTracker(avatar.ID);

                    existing.Name = avatar.Name;

                    GetDisplayName(existing);
                    
                    if(!trackerDictionary.ContainsKey(avatar.ID))
                    {
                        avatarsInTracker.Add(existing);
                        trackerDictionary.Add(avatar.ID, existing);
                    }
                }
                else
                {
                    existing.Avatar = avatar;
                }
            }
        }

        private void DataGridView_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            var row = dataGridView.Rows[e.RowIndex];

            if(e.ColumnIndex == 3)
            {
                row.Cells[3].Style.ForeColor = Color.Green;
            }
        }

        Dictionary<UUID, string> PitifulNameCache = new Dictionary<UUID, string>();

        void GetDisplayName(AvatarInTracker entry)
        {
            if(PitifulNameCache.ContainsKey(entry.UUID))
            {
                entry.Name = PitifulNameCache[entry.UUID];
                return;
            }

            PitifulNameCache[entry.UUID] = entry.Name;
            Frame.Avatars.GetDisplayNames(new List<UUID> { entry.UUID }, (success, names, rejects) =>
            {
                if (success)
                {
                    foreach (var name in names)
                    {
                        if (name.ID == entry.UUID)
                        {
                            if (name.IsDefaultDisplayName)
                            {
                                entry.Name = name.DisplayName;
                            }
                            else
                            {
                                entry.Name = string.Format("{0} ({1})", name.DisplayName, name.UserName.ToLower());
                            }
                            PitifulNameCache[entry.UUID] = entry.Name;
                            break;
                        }
                    }
                }
            });
        }

        void UpdateList(RegionProxy sim, List<UUID> added, List<UUID> removed)
        {
            if (sim != Frame.Network.CurrentSim)
                return;

            if (dataGridView.InvokeRequired) dataGridView.Invoke((MethodInvoker)delegate { UpdateList(sim, added, removed); });
            else
            {
                foreach (UUID key in added)
                {
                    var entry = new AvatarInTracker(key);

                    GetDisplayName(entry);

                    Vector3 pos;
                    if (sim.AvatarPositions.TryGetValue(key, out pos))
                        entry.CoarsePosition = pos;

                    if(!trackerDictionary.ContainsKey(entry.UUID))
                    {
                        avatarsInTracker.Add(entry);
                        trackerDictionary.Add(entry.UUID, entry);
                    }
                }

                var to_remove = avatarsInTracker.Where(x => removed.Contains(x.UUID)).ToArray();
                foreach (var t in to_remove)
                {
                    dataGridView.Rows.Remove(t.Row);
                    avatarsInTracker.Remove(t);
                    trackerDictionary.Remove(t.UUID);
                }
            }
        }

        private void Grid_CoarseLocationUpdate(object sender, CoarseLocationUpdateEventArgs e)
        {
            UpdateList(e.Simulator, e.NewEntries, e.RemovedEntries);
        }
    }

    //public static class ExtensionMethods
    //{
    //    public static void DoubleBuffered(this DataGridView dgv, bool setting)
    //    {
    //        Type dgvType = dgv.GetType();
    //        PropertyInfo pi = dgvType.GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic);
    //        pi.SetValue(dgv, setting, null);
    //    }

    //    public static void DoubleBuffered(this FlowLayoutPanel flp, bool setting)
    //    {
    //        Type dgvType = flp.GetType();
    //        PropertyInfo pi = dgvType.GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic);
    //        pi.SetValue(flp, setting, null);
    //    }
    //}
}
