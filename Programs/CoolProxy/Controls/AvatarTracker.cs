using GridProxy;
using OpenMetaverse;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static GridProxy.RegionManager;

namespace CoolGUI.Controls
{
    public partial class AvatarTracker : DataGridView
    {
        private ProxyFrame _Frame;

        /// <summary>
        /// Gets or sets the GridClient associated with this control
        /// </summary>
        public ProxyFrame Frame
        {
            get { return _Frame; }
            set { if (value != null) InitializeClient(value); }
        }


        private DoubleDictionary<uint, UUID, TrackedAvatar> _TrackedAvatars = new DoubleDictionary<uint, UUID, TrackedAvatar>();
        private Dictionary<UUID, TrackedAvatar> _UntrackedAvatars = new Dictionary<UUID, TrackedAvatar>();


        public delegate void AvatarCallback(TrackedAvatar trackedAvatar);

        /// <summary>
        /// Triggered when the user double clicks on an avatar in the list
        /// </summary>
        public event AvatarCallback OnAvatarDoubleClick;

        /// <summary>
        /// Triggered when a new avatar is added to the list
        /// </summary>
        public event AvatarCallback OnAvatarAdded;

        /// <summary>
        /// Triggered when an avatar is removed from the list
        /// </summary>
        public event AvatarCallback OnAvatarRemoved;


        public AvatarTracker()
        {
            InitializeComponent();
        }

        public AvatarTracker(IContainer container)
        {
            container.Add(this);

            InitializeComponent();


            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();

            this.avatarKeyColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.avatarPosColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.avatarDistColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.avatarClientColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this)).BeginInit();
            this.SuspendLayout();
            // 
            // avatarTrackerGridView
            // 
            this.AllowUserToAddRows = false;
            this.AllowUserToDeleteRows = false;
            this.AllowUserToResizeRows = false;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.Silver;
            this.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            this.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.avatarKeyColumn,
            this.avatarPosColumn,
            this.avatarDistColumn,
            this.avatarClientColumn});
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.ControlDark;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.DefaultCellStyle = dataGridViewCellStyle2;
            this.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Location = new System.Drawing.Point(0, 0);
            this.Name = "avatarTrackerGridView";
            this.ReadOnly = true;
            this.RowHeadersVisible = false;
            this.RowTemplate.Height = 18;
            this.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.Size = new System.Drawing.Size(372, 199);
            this.TabIndex = 2;
            // 
            // avatarKeyColumn
            // 
            this.avatarKeyColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.avatarKeyColumn.HeaderText = "Avatar Name";
            this.avatarKeyColumn.Name = "avatarKeyColumn";
            this.avatarKeyColumn.ReadOnly = true;
            // 
            // avatarPosColumn
            // 
            this.avatarPosColumn.HeaderText = "Position";
            this.avatarPosColumn.Name = "avatarPosColumn";
            this.avatarPosColumn.ReadOnly = true;
            this.avatarPosColumn.Width = 80;
            // 
            // avatarDistColumn
            // 
            this.avatarDistColumn.HeaderText = "Dist.";
            this.avatarDistColumn.Name = "avatarDistColumn";
            this.avatarDistColumn.ReadOnly = true;
            this.avatarDistColumn.Width = 60;
            // 
            // avatarClientColumn
            // 
            this.avatarClientColumn.HeaderText = "Client";
            this.avatarClientColumn.Name = "avatarClientColumn";
            this.avatarClientColumn.ReadOnly = true;
            this.avatarClientColumn.Width = 90;
            // 
            // AvatarTrackerControl
            // 
            //this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            //this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            //this.Controls.Add(this.avatarTrackerGridView);
            this.Name = "AvatarTrackerControl";
            this.Size = new System.Drawing.Size(372, 199);
            ((System.ComponentModel.ISupportInitialize)(this)).EndInit();
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.DataGridViewTextBoxColumn avatarKeyColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn avatarPosColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn avatarDistColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn avatarClientColumn;



        private void InitializeClient(ProxyFrame frame)
        {
            _Frame = frame;
            //Frame.Avatars.AvatarAppearance += Avatars_OnAvatarAppearance;
            //Frame.Avatars.UUIDNameReply += new EventHandler<UUIDNameReplyEventArgs>(Avatars_UUIDNameReply);
            Frame.Grid.CoarseLocationUpdate += Grid_CoarseLocationUpdate;
            //Frame.Network.SimChanged += Network_OnCurrentSimChanged;
            Frame.Objects.AvatarUpdate += Objects_OnNewAvatar;
            Frame.Objects.TerseObjectUpdate += Objects_OnObjectUpdated;

            OnAvatarAdded += AvatarTrackerControl_OnAvatarAdded;

        }

        private void AvatarTrackerControl_OnAvatarAdded(TrackedAvatar trackedAvatar)
        {
            Frame.SayToUser(trackedAvatar.Name + " added to tracker!");
        }

        private void Objects_OnObjectUpdated(object sender, GridProxy.TerseObjectUpdateEventArgs e)
        {
            bool found;
            lock (_TrackedAvatars)
                found = _TrackedAvatars.ContainsKey(e.Update.LocalID);

            if (found)
            {
                Avatar av;
                if (e.Region.ObjectsAvatars.TryGetValue(e.Update.LocalID, out av))
                    UpdateAvatar(av);
            }
        }

        private void Objects_OnNewAvatar(object sender, AvatarUpdateEventArgs e)
        {
            UpdateAvatar(e.Avatar);
        }

        private void UpdateAvatar(Avatar avatar)
        {
            if (!this.IsHandleCreated) return;

            if (this.InvokeRequired) this.BeginInvoke((MethodInvoker)delegate { UpdateAvatar(avatar); });
            else
            {
                TrackedAvatar trackedAvatar;
                bool found;

                lock (_UntrackedAvatars)
                    found = _UntrackedAvatars.TryGetValue(avatar.ID, out trackedAvatar);

                if (found)
                {
                    trackedAvatar.Name = avatar.Name;
                    trackedAvatar.DataGridViewItem.Cells[0].Value = avatar.Name;
                    trackedAvatar.DataGridViewItem.Cells[0].Style.ForeColor = Color.FromKnownColor(KnownColor.ControlText);

                    lock (_TrackedAvatars) _TrackedAvatars.Add(avatar.LocalID, avatar.ID, trackedAvatar);
                    _UntrackedAvatars.Remove(avatar.ID);
                }

                lock (_TrackedAvatars)
                    found = _TrackedAvatars.TryGetValue(avatar.ID, out trackedAvatar);

                if (found)
                {
                    trackedAvatar.Avatar = avatar;
                    trackedAvatar.Name = avatar.Name;
                    trackedAvatar.ID = avatar.ID;

                    trackedAvatar.DataGridViewItem.Cells[1].Value = string.Format("{0:0}, {1:0}, {2:0}", avatar.Position.X, avatar.Position.Y, avatar.Position.Z);
                    string strDist = avatar.ID == Frame.Agent.AgentID ? "--" : (int)Vector3.Distance(Frame.Agent.SimPosition, avatar.Position) + "m";
                    trackedAvatar.DataGridViewItem.Cells[2].Value = strDist;
                }
                else
                {
                    AddAvatar(avatar.ID, avatar, Vector3.Zero);
                }

                //avatarTrackerGridView.Rows.Sort();
            }
        }

        private void Grid_CoarseLocationUpdate(object sender, GridProxy.CoarseLocationUpdateEventArgs e)
        {
            UpdateCoarseInfo(e.Simulator, e.NewEntries, e.RemovedEntries);
        }

        private void UpdateCoarseInfo(RegionProxy sim, List<UUID> newEntries, List<UUID> removedEntries)
        {
            if (this.InvokeRequired) this.BeginInvoke((MethodInvoker)delegate { UpdateCoarseInfo(sim, newEntries, removedEntries); });
            else
            {
                if (sim == null) return;

                if (removedEntries != null)
                {
                    for (int i = 0; i < removedEntries.Count; i++)
                        RemoveAvatar(removedEntries[i]);
                }

                if (newEntries != null)
                {
                    for (int i = 0; i < newEntries.Count; i++)
                    {
                        int index = -1;

                        if (this.Rows.Count > 0)
                        {
                            DataGridViewRow row = this.Rows
                                .Cast<DataGridViewRow>()
                                .Where(r => (UUID)r.Tag == newEntries[i])
                                .First();

                            index = row.Index;
                        }

                        if (index == -1)
                        {
                            Vector3 coarsePos;
                            if (!sim.AvatarPositions.TryGetValue(newEntries[i], out coarsePos))
                                continue;

                            AddAvatar(newEntries[i], null, coarsePos);
                        }
                    }
                }
            }
        }

        private void AddAvatar(UUID avatarID, Avatar avatar, Vector3 coarsePosition)
        {
            if (!this.IsHandleCreated) return;

            if (this.InvokeRequired) this.BeginInvoke((MethodInvoker)delegate { AddAvatar(avatar.ID, avatar, coarsePosition); });
            else
            {
                TrackedAvatar trackedAvatar = new TrackedAvatar();
                trackedAvatar.CoarseLocation = coarsePosition;
                trackedAvatar.ID = avatarID;


                int id = this.Rows.Add(avatarID.ToString(), "?, ?, ?", "?", "Unknown");
                this.Rows[id].Cells[2].Style.ForeColor = Color.Gray;
                this.Rows[id].Cells[3].Style.ForeColor = Color.Gray;

                trackedAvatar.DataGridViewItem = this.Rows[id];

                trackedAvatar.DataGridViewItem.Tag = avatarID;

                string strDist = avatarID == Frame.Agent.AgentID ? "--" : (int)Vector3.Distance(Frame.Agent.SimPosition, coarsePosition) + "m";
                trackedAvatar.DataGridViewItem.Cells[2].Value = strDist;

                if (avatar != null)
                {
                    trackedAvatar.Name = avatar.Name;
                    trackedAvatar.DataGridViewItem.Cells[0].Value = avatar.Name;

                    lock (_TrackedAvatars)
                    {
                        if (_TrackedAvatars.ContainsKey(avatarID))
                            _TrackedAvatars.Remove(avatarID);

                        _TrackedAvatars.Add(avatar.LocalID, avatarID, trackedAvatar);
                    }

                    if (OnAvatarAdded != null)
                    {
                        try { OnAvatarAdded(trackedAvatar); }
                        catch (Exception ex) { Logger.Log(ex.Message, Helpers.LogLevel.Error, ex); }
                    }
                }
                else
                {
                    lock (_UntrackedAvatars)
                    {
                        _UntrackedAvatars.Add(avatarID, trackedAvatar);

                        trackedAvatar.DataGridViewItem.Cells[0].Style.ForeColor = Color.FromKnownColor(KnownColor.GrayText);

                        if (avatarID == Frame.Agent.AgentID)
                        {
                            trackedAvatar.Name = Frame.Agent.Name;
                            trackedAvatar.DataGridViewItem.Cells[0].Value = Frame.Agent.Name;
                        }

                        //else if (_Client.Network.Connected)
                        //    Frame.Avatars.RequestAvatarName(avatarID);
                    }
                }

            }
        }

        private void RemoveAvatar(UUID id)
        {
            if (!this.IsHandleCreated) return;

            if (this.InvokeRequired) this.BeginInvoke((MethodInvoker)delegate { RemoveAvatar(id); });
            else
            {
                TrackedAvatar trackedAvatar;

                lock (_TrackedAvatars)
                {
                    if (_TrackedAvatars.TryGetValue(id, out trackedAvatar))
                    {
                        this.Rows.Remove(trackedAvatar.DataGridViewItem);
                        //this.Items.Remove(trackedAvatar.ListViewItem);
                        _TrackedAvatars.Remove(id);
                    }
                }

                lock (_UntrackedAvatars)
                {
                    if (_UntrackedAvatars.TryGetValue(id, out trackedAvatar))
                    {
                        this.Rows.Remove(trackedAvatar.DataGridViewItem);
                        //this.Items.Remove(trackedAvatar.ListViewItem);
                        _UntrackedAvatars.Remove(trackedAvatar.ID);
                    }
                }

                if (OnAvatarRemoved != null)
                {
                    try { OnAvatarRemoved(trackedAvatar); }
                    catch (Exception ex) { Logger.Log(ex.Message, Helpers.LogLevel.Error, ex); }
                }
            }
        }

    }

    /// <summary>
    /// Contains any available information for an avatar in the simulator.
    /// A null value for .Avatar indicates coarse data for an avatar outside of visible range.
    /// </summary>
    public class TrackedAvatar
    {
        /// <summary>Assigned if the avatar is within visible range</summary>
        public Avatar Avatar = null;

        /// <summary>Last known coarse location of avatar</summary>
        public Vector3 CoarseLocation;

        /// <summary>Avatar ID</summary>
        public UUID ID;

        /// <summary>ListViewItem associated with this avatar</summary>
        public DataGridViewRow DataGridViewItem;

        /// <summary>Populated by RequestAvatarName if avatar is not visible</summary>
        public string Name = "(Loading...)";
    }
}
