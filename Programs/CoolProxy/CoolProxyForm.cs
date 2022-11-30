﻿using GridProxy;
using L33T.GUI;
using Nwc.XmlRpc;
using OpenMetaverse;
using OpenMetaverse.Http;
using OpenMetaverse.Packets;
using OpenMetaverse.StructuredData;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using static GridProxy.RegionManager;

namespace CoolProxy
{
    public partial class CoolProxyForm : Form
    {
        private bool firstTimeMinimized = true;

        private GridListManager gridManager;

        private GUIManager guiManager;

        public GUIManager GUI { get { return guiManager; } }

        private InventoryBrowserForm inventoryBrowserForm;

        private AvTrackerTest avatarTracker;

        private Dictionary<UUID, DateTime> AssetBlacklist = new Dictionary<UUID, DateTime>();


        public CoolProxyForm()
        {

            bool createdNew;
            var appMutex = new Mutex(true, "CoolProxyApp", out createdNew);

            if (!createdNew && !CoolProxy.Frame.Settings.getBool("AllowMultipleInstances"))
            {
                MessageBox.Show("Cool Proxy is already running!");
                this.Load += (x, y) => Close();
                return;
            }

            InitializeComponent();

            debugRTB.Text = "Cool Proxy>";
            debugRTB.SelectionStart = debugRTB.Text.Length;

            // log4net
            if (FireEventAppender.Instance != null)
            {
                FireEventAppender.Instance.MessageLoggedEvent += Instance_MessageLoggedEvent;
            }

            OpenMetaverse.Logger.Log("[GUI] CoolProxy GUI launched!", Helpers.LogLevel.Info);

            inventoryBrowserForm = new InventoryBrowserForm();

            guiManager = new GUIManager(this);
            gridManager = new GridListManager();

            CoolProxy.Frame.RegisterModuleInterface<IGUI>(guiManager);

            // Regions
            CoolProxy.Frame.Network.OnNewRegion += onNewRegion;
            CoolProxy.Frame.Network.OnHandshake += onHandshake;

            // Asset Transfers
            CoolProxy.Frame.Assets.DownloadProgress += Assets_DownloadProgress;

            uploadsGridView.Rows.Add(Properties.Resources.Inv_Mesh, "C:\\assets\\somemesh.slmesh", "Queued");
            uploadsGridView.Rows.Add(Properties.Resources.Inv_Script, "C:\\assets\\cool script.lsl", "Queued");

            comboBox2.DataSource = Enum.GetValues(typeof(AssetType));
            comboBox2.SelectedItem = AssetType.Unknown;

            LoadBlacklist();

            downloadsGridView.ShowCellToolTips = false;
            downloadsGridView.CellToolTipTextNeeded += DownloadsGridView_CellToolTipTextNeeded;

            tabPage2.HorizontalScroll.Enabled = false;
            tabPage2.HorizontalScroll.Visible = false;
            tabPage2.HorizontalScroll.Maximum = 0;
            tabPage2.AutoScroll = true;

            // Asset Logging
            soundsDataGridView.DoubleBuffered(true);
            animsDataGridView.DoubleBuffered(true);

            CoolProxy.Frame.Network.AddDelegate(PacketType.SoundTrigger, Direction.Incoming, onSoundTrigger);
            CoolProxy.Frame.Network.AddDelegate(PacketType.AttachedSound, Direction.Incoming, onAttachedSound);
            CoolProxy.Frame.Network.AddDelegate(PacketType.PreloadSound, Direction.Incoming, onPreloadSound);

            CoolProxy.Frame.Objects.ObjectDataBlockUpdate += Objects_ObjectDataBlockUpdate;

            CoolProxy.Frame.Avatars.AvatarAnimation += Avatars_AvatarAnimation;


            // Settings
            LoadGrids();

            gridManager.OnGridAdded += OnGridAdded;
            gridsComboBox.SelectedItem = CoolProxy.Frame.Settings.getString("LastGridUsed");

            this.TopMost = CoolProxy.Frame.Settings.getBool("KeepCoolProxyOnTop");
            CoolProxy.Frame.Settings.getSetting("KeepCoolProxyOnTop").OnChanged += (x, y) => { this.TopMost = (bool)y.Value; };

            avatarTracker = new AvTrackerTest(avatarTrackerGridView, CoolProxy.Frame);

            flowLayoutPanel1.DoubleBuffered(true);

            //CoolProxy.Frame.OnNewChatCommand += ChatCommandAdded;

            // Login masking
            CoolProxy.Frame.Login.AddLoginResponseDelegate(handleLoginResponse);

            firstTimeMinimized = CoolProxy.Frame.Settings.getBool("AlertStillRunning");

            AddMainMenuOption(new MenuOption("TOGGLE_AVATAR_TRACKER", "Avatar Tracker", true)
            {
                Clicked = tabNameToolStripMenuItem_Click,
                Checked = isPoppedTabOpen,
                Tag = "Avatar Tracker"
            });

            AddMainMenuOption(new MenuOption("TOGGLE_REGIONS", "Regions", false)
            {
                Clicked = tabNameToolStripMenuItem_Click,
                Checked = isPoppedTabOpen,
                Tag = "Regions"
            });

            AddMainMenuOption(new MenuOption("TOGGLE_TOOLBOX", "ToolBox", true)
            {
                Clicked = tabNameToolStripMenuItem_Click,
                Checked = isPoppedTabOpen,
                Tag = "ToolBox"
            });

            AddMainMenuOption(new MenuOption("TOGGLE_ASSET_LOG", "Asset Log", true)
            {
                Clicked = tabNameToolStripMenuItem_Click,
                Checked = isPoppedTabOpen,
                Tag = "Asset Logging"
            });

            AddMainMenuOption(new MenuOption("TOGGLE_TRANSFERS", "Asset Transfers", false)
            {
                Clicked = tabNameToolStripMenuItem_Click,
                Checked = isPoppedTabOpen,
                Tag = "Asset Transfers"
            });

            AddMainMenuOption(new MenuOption("TOGGLE_INVENTORY", "Inventory Browser", true)
            {
                Clicked = inventoryBrowserToolStripMenuItem_Click,
                Checked = (x) => inventoryBrowserForm.Visible
            });


            string cmd_prefix = CoolProxy.Frame.Settings.getString("ChatCommandPrefix");
            cmdPrefixCombo.SelectedItem = cmd_prefix;


            var version = Application.ProductVersion;
            var split = version.Split('.');
            label12.Text = "Version " + split[0] + "." + split[1];


            if(!CoolProxy.IsDebugMode)
            {
                loadPluginTestButton.Visible = false;
                tabControl5.TabPages.Remove(tabPage20);
                tabControl4.TabPages.Remove(blacklistTab);
            }

            regionsDataGridView.DoubleBuffered(true);

            var region_tracker_timer = new System.Windows.Forms.Timer();
            region_tracker_timer.Tick += Region_tracker_timer_Tick;
            region_tracker_timer.Interval = 1000;
            region_tracker_timer.Start();
        }

        private void Region_tracker_timer_Tick(object sender, EventArgs e)
        {
            foreach(DataGridViewRow row in regionsDataGridView.Rows)
            {
                RegionProxy region = (RegionProxy)row.Tag;
                row.Cells[0].Style.ForeColor = region.Connected ? region == CoolProxy.Frame.Network.CurrentSim ? Color.Green : Color.Black : Color.Maroon;
                row.Cells[2].Value = region.Connected ? region.AvatarPositions.Count.ToString() : "?";
            }
        }

        private void LoadBlacklist()
        {
            string app_settings_dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "CoolProxy\\");
            string filename = Path.Combine(app_settings_dir, "blacklist.xml");

            if(File.Exists(filename))
            {
                byte[] xml = File.ReadAllBytes(filename);
                OSD osd = OSDParser.DeserializeLLSDXml(xml);
                OSDMap map = (OSDMap)osd;

                foreach(KeyValuePair<string, OSD> pair in map)
                {
                    UUID asset_id = UUID.Parse(pair.Key);
                    DateTime date = ((OSDDate)pair.Value).AsDate();
                    blacklistDataGridView.Rows.Add(asset_id, date.ToString());
                    AssetBlacklist.Add(asset_id, date);
                }
            }

            //AssetBlacklist.Add(UUID.Parse("89556747-24cb-43ed-920b-47caed15465f"), DateTime.Now);
        }

        private void SaveBlacklist()
        {
            OSDMap blacklist = new OSDMap();

            foreach(var pair in AssetBlacklist)
            {
                blacklist.Add(pair.Key.ToString(), pair.Value);
            }


            string app_settings_dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "CoolProxy\\");
            string filename = Path.Combine(app_settings_dir, "blacklist.xml");

            byte[] xml = OSDParser.SerializeLLSDXmlToBytes((OSD)blacklist);
            File.WriteAllBytes(filename, xml);
        }

        private void BlacklistAsset(UUID asset_id)
        {
            if(!AssetBlacklist.ContainsKey(asset_id))
            {
                var now = DateTime.Now;
                AssetBlacklist.Add(asset_id, now);
                blacklistDataGridView.Rows.Add(asset_id, now.ToString());

                SaveBlacklist();
            }
        }

        int currentLine = 0;

        void Instance_MessageLoggedEvent(object sender, MessageLoggedEventArgs e)
        {
            if (this.IsDisposed || this.Disposing)
                return;

            if (InvokeRequired)
            {
                BeginInvoke(new MethodInvoker(() => Instance_MessageLoggedEvent(sender, e)));
            }
            else
            {
                //string s = String.Format(/*"{0}*/ "[{1}] {2} {3}", e.LoggingEvent.TimeStamp, e.LoggingEvent.Level,
                //    e.LoggingEvent.RenderedMessage, e.LoggingEvent.ExceptionObject);

                var i = debugRTB.GetFirstCharIndexFromLine(currentLine);


                string s = e.LoggingEvent.RenderedMessage;

                Regex reg = new Regex(@"\[.*\]");
                Match match = reg.Match(s);

                debugRTB.SelectionStart = i;
                debugRTB.SelectionLength = debugRTB.Text.Length - i;

                string so_far = debugRTB.Text.Substring(i);

                debugRTB.Cut();

                if (match.Success)
                {
                    string log_str = string.Format("[{0}] ", e.LoggingEvent.Level);

                    int start_index = log_str.Length;

                    log_str += s.Substring(match.Index, match.Length);
                    log_str += s.Substring(match.Index + match.Length);
                    log_str += "\n";

                    debugRTB.AppendText(log_str);
                    debugRTB.AppendText(so_far);

                    debugRTB.Select(start_index + i, match.Length);
                    debugRTB.SelectionColor = Color.Cyan;
                    debugRTB.DeselectAll();

                    currentLine = debugRTB.GetLineFromCharIndex(debugRTB.Text.Length);
                    debugRTB.SelectionStart = debugRTB.Text.Length;
                }
                else
                {
                    debugRTB.AppendText(s + "\n");
                    debugRTB.AppendText(so_far);

                    debugRTB.SelectionStart = debugRTB.Text.Length;

                }
            }
        }

        private void debugRTB_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Back)
            {
                if (currentLine > 0)
                {
                    if (debugRTB.Lines[debugRTB.Lines.Length - 1].Length < 12)
                        e.SuppressKeyPress = true;
                }
                else
                {
                    if (debugRTB.Text.Length < 12)
                        e.SuppressKeyPress = true;
                }
            }
            else if (e.KeyCode == Keys.Return)
            {
                var i = debugRTB.GetFirstCharIndexFromLine(currentLine);

                i += 11;

                if (debugRTB.Text.Length > i)
                {
                    string cmd = debugRTB.Text.Substring(i);

                    if (cmd == "clear")
                    {
                        debugRTB.Text = "Cool Proxy>";
                        debugRTB.SelectionStart = 11;
                        currentLine = 0;
                        e.SuppressKeyPress = true;
                        return;
                    }
                    else
                    {
                        string output = "\n" + CoolProxy.Frame.RunCommand(cmd);
                        debugRTB.AppendText(output);
                    }
                }

                debugRTB.AppendText("\nCool Proxy>");
                debugRTB.SelectionStart = debugRTB.Text.Length;

                currentLine = debugRTB.GetLineFromCharIndex(debugRTB.Text.Length);

                e.SuppressKeyPress = true;
                return;
            }

            switch(e.KeyCode)
            {
                case Keys.Up:
                case Keys.Down:
                case Keys.Left:
                case Keys.Right:
                    e.SuppressKeyPress = true;
                    return;
            }

            if (debugRTB.SelectionStart != debugRTB.Text.Length)
            {
                debugRTB.SelectionStart = debugRTB.Text.Length;
                debugRTB.ScrollToCaret();
            }
        }

        ////// Chat Commands

        private bool ChatCommandOnLeft = true;

        private Point ChatCommandLabelLeft = new Point(23, 29);
        private Size ChatCommandTextBoxSize = new Size(167, 20);
        private int ChatCommandTextBoxOffset = 15;
        private int ChatCommandYOffset = 40;
        private int ChatCommandXOffset = 245;

        private int ChatCommandRowCount = 0;

        private ToolTip ChatCommandToolTip = new ToolTip()
        {
            AutoPopDelay = 5000,
            InitialDelay = 1000,
            ReshowDelay = 500,
            // Force the ToolTip text to be displayed whether or not the form is active.
            ShowAlways = true
        };

        private void ChatCommandAdded(string command, string name, string info)
        {
            if(ChatCommandOnLeft && ChatCommandRowCount > 4)
            {
                chatCommandsPanel.Size = new Size(chatCommandsPanel.Width, chatCommandsPanel.Height + 40);
                label38.Size = new Size(label38.Width, label38.Height + 40);
            }

            int x = ChatCommandOnLeft ? ChatCommandLabelLeft.X : ChatCommandXOffset;
            int y = ChatCommandLabelLeft.Y + (ChatCommandYOffset * ChatCommandRowCount);

            Label label = new Label();
            label.Text = name;
            label.Location = new Point(x, y);
            label.AutoSize = true;

            CoolTextBox textBox = new CoolTextBox();
            textBox.Text = command;
            textBox.Location = new Point(x, y + ChatCommandTextBoxOffset);
            textBox.Size = ChatCommandTextBoxSize;
            textBox.Enabled = false;

            ChatCommandToolTip.SetToolTip(label, info);

            chatCommandsPanel.Controls.Add(label);
            chatCommandsPanel.Controls.Add(textBox);

            ChatCommandOnLeft = !ChatCommandOnLeft;

            if (ChatCommandOnLeft) ChatCommandRowCount++;
        }

        private void onHandshake(RegionManager.RegionProxy proxy)
        {
            if (regionsDataGridView.InvokeRequired) regionsDataGridView.BeginInvoke(new Action(() => onHandshake(proxy)));
            else
            {
                foreach(DataGridViewRow row in regionsDataGridView.Rows)
                {
                    //if((string)row.Cells[1].Value == proxy.RemoteEndPoint.ToString())
                    if((RegionManager.RegionProxy)row.Tag == proxy)
                    {
                        row.Cells[0].Value = proxy.Name;
                        return;
                    }
                }
            }
        }

        private void onNewRegion(RegionManager.RegionProxy proxy)
        {
            AddRegion(proxy);
        }

        void AddRegion(RegionManager.RegionProxy proxy)
        {
            if (regionsDataGridView.InvokeRequired) regionsDataGridView.BeginInvoke(new Action(() => AddRegion(proxy)));
            else
            {
                int r = regionsDataGridView.Rows.Add("???", proxy.RemoteEndPoint.ToString(), "?");
                regionsDataGridView.Rows[r].Tag = proxy;
            }
        }

        private void Avatars_AvatarAnimation(object sender, AvatarAnimationEventArgs e)
        {
            UUID[] anims = e.Animations.Select(x => x.AnimationID).ToArray();
            LogAnimation(anims, e.AvatarID);
        }

        List<UUID> default_animations = Animations.ToDictionary().Keys.ToList();

        Dictionary<UUID, Tuple<int, Dictionary<UUID, DateTime>>> AvatarAnimations = new Dictionary<UUID, Tuple<int, Dictionary<UUID, DateTime>>>();

        string quick_format_name(AgentDisplayName name)
        {
            if(name.IsDefaultDisplayName)
            {
                if(name.LegacyLastName.ToLower() == "resident")
                {
                    return name.LegacyFirstName;
                }
                else
                {
                    return name.LegacyFirstName + " " + name.LegacyLastName;
                }
            }
            else
            {
                return name.DisplayName + " (" + name.UserName + ")";
            }
        }

        void LogAnimation(UUID[] anims, UUID avatar)
        {
            if (animsDataGridView.InvokeRequired) animsDataGridView.BeginInvoke(new Action(() => LogAnimation(anims, avatar)));
            else
            {
                Dictionary<UUID, DateTime> logged_anims = null;

                if(!AvatarAnimations.TryGetValue(avatar, out var tuple))
                {
                    int index = animsDataGridView.Rows.Add(avatar.ToString(), avatar.ToString(), anims.Length, 0);
                    logged_anims = new Dictionary<UUID, DateTime>();
                    tuple = new Tuple<int, Dictionary<UUID, DateTime>>(index, logged_anims);
                    AvatarAnimations[avatar] = tuple;

                    CoolProxy.Frame.Avatars.GetDisplayNames(new List<UUID>() { avatar }, (success, names, z) =>
                    {
                        if (success)
                        {
                            foreach (var name in names)
                            {
                                if (name.ID == avatar)
                                {
                                    UpdateLogName(name, index);
                                    break;
                                }
                            }
                        }
                    });
                }
                else
                {
                    logged_anims = tuple.Item2;
                }

                lock(logged_anims)
                {
                    foreach(UUID anim in anims)
                    {
                        if (!default_animations.Contains(anim))
                        {
                            logged_anims[anim] = DateTime.UtcNow;
                        }
                    }
                }

                animsDataGridView.Rows[tuple.Item1].Cells[2].Value = anims.Length.ToString();
                animsDataGridView.Rows[tuple.Item1].Cells[3].Value = logged_anims.Count.ToString();
            }
        }

        private void UpdateLogName(AgentDisplayName name, int index)
        {
            if (animsDataGridView.InvokeRequired) animsDataGridView.BeginInvoke(new Action(() => UpdateLogName(name, index)));
            else
            {
                animsDataGridView.Rows[index].Tag = name;
                animsDataGridView.Rows[index].Cells[1].Value = quick_format_name(name);
            }
        }

        List<UUID> loggedSounds = new List<UUID>();

        Font boldFont = null;
        Font BoldFont
        {
            get
            {
                if (boldFont == null)
                    boldFont = new Font(soundsDataGridView.DefaultCellStyle.Font, FontStyle.Bold);
                return boldFont;
            }
        }

        private void LogSound(string type, UUID id, UUID owner, bool blacklisted)
        {
            if (soundsDataGridView.InvokeRequired) soundsDataGridView.BeginInvoke(new Action(() => LogSound(type, id, owner, blacklisted)));
            else
            {
                lock(loggedSounds)
                {
                    if(!loggedSounds.Contains(id))
                    {
                        int row = soundsDataGridView.Rows.Add(type, id.ToString(), DateTime.Now.ToString());
                        if(blacklisted)
                            soundsDataGridView.Rows[row].Cells[1].Style.Font = BoldFont;
                        loggedSounds.Add(id);
                    }
                }
            }
        }

        private Packet onSoundTrigger(Packet packet, RegionManager.RegionProxy sim)
        {
            SoundTriggerPacket soundTriggerPacket = (SoundTriggerPacket)packet;
            bool blacklisted = AssetBlacklist.ContainsKey(soundTriggerPacket.SoundData.SoundID);
            LogSound("Trigger", soundTriggerPacket.SoundData.SoundID, soundTriggerPacket.SoundData.OwnerID, blacklisted);
            return blacklisted ? null : packet;
        }

        private Packet onAttachedSound(Packet packet, RegionManager.RegionProxy sim)
        {
            AttachedSoundPacket attachedSoundPacket = (AttachedSoundPacket)packet;
            bool blacklisted = AssetBlacklist.ContainsKey(attachedSoundPacket.DataBlock.SoundID);
            LogSound("Attached", attachedSoundPacket.DataBlock.SoundID, attachedSoundPacket.DataBlock.OwnerID, blacklisted);
            return blacklisted ? null : packet;
        }

        private Packet onPreloadSound(Packet packet, RegionManager.RegionProxy sim)
        {
            PreloadSoundPacket preloadSoundPacket = (PreloadSoundPacket)packet;

            foreach(var block in preloadSoundPacket.DataBlock)
            {
                bool blacklisted = AssetBlacklist.ContainsKey(block.SoundID);
                LogSound("Preload", block.SoundID, block.OwnerID, blacklisted);

                if(blacklisted)
                {
                    block.SoundID = UUID.Zero;
                }
            }

            return preloadSoundPacket;
        }

        private void Objects_ObjectDataBlockUpdate(object sender, ObjectDataBlockUpdateEventArgs e)
        {
            bool blacklisted_sound = AssetBlacklist.ContainsKey(e.Block.Sound);
            LogSound("Loop", e.Block.Sound, e.Prim.OwnerID, blacklisted_sound);

            if (blacklisted_sound)
            {
                e.Block.Sound = UUID.Zero;
            }

            bool blacklisted_textures = false;

            // todo: materials etc
            if(AssetBlacklist.ContainsKey(e.Update.Textures.DefaultTexture?.TextureID ?? UUID.Zero))
            {
                e.Update.Textures.DefaultTexture.TextureID = UUID.Zero;
                blacklisted_textures = true;
            }

            foreach(var face in e.Update.Textures.FaceTextures)
            {
                if (face == null) continue;
                if (AssetBlacklist.ContainsKey(face.TextureID))
                {
                    face.TextureID = UUID.Zero;
                    blacklisted_textures = true;
                }
            }

            if(blacklisted_textures)
            {
                e.Block.TextureEntry = e.Update.Textures.GetBytes();
            }
        }

        private void DownloadsGridView_CellToolTipTextNeeded(object sender, DataGridViewCellToolTipTextNeededEventArgs e)
        {
            var row = downloadsGridView.Rows[e.RowIndex];
            var extra = (ExtraDownloadInfo)row.Tag;

            string info = string.Empty;
            if(extra.FileName != string.Empty)
            {
                info += "Filename: " + extra.FileName + "\n";
            }
            info += "Time:" + extra.Time.ToString();
            
            e.ToolTipText = info;
        }

        private Bitmap getIconForAssetType(AssetType assetType)
        {
            switch(assetType)
            {
                case AssetType.Texture:
                    return Properties.Resources.Inv_Texture;
                case AssetType.Sound:
                    return Properties.Resources.Inv_Sound;
                case AssetType.CallingCard:
                    return Properties.Resources.Inv_CallingCard;
                case AssetType.Landmark:
                    return Properties.Resources.Inv_Landmark;
                case AssetType.LSLText:
                    return Properties.Resources.Inv_Script;
                case AssetType.Clothing:
                    return Properties.Resources.Inv_Shirt;
                case AssetType.Object:
                    return Properties.Resources.Inv_Object;
                case AssetType.Notecard:
                    return Properties.Resources.Inv_Notecard;
                case AssetType.LSLBytecode:
                    return Properties.Resources.Inv_Script;
                case AssetType.TextureTGA:
                    return Properties.Resources.Inv_Texture;
                case AssetType.Bodypart:
                    return Properties.Resources.Inv_Skin;
                case AssetType.SoundWAV:
                    return Properties.Resources.Inv_Sound;
                case AssetType.ImageTGA:
                    return Properties.Resources.Inv_Texture;
                case AssetType.ImageJPEG:
                    return Properties.Resources.Inv_Texture;
                case AssetType.Animation:
                    return Properties.Resources.Inv_Animation;
                case AssetType.Gesture:
                    return Properties.Resources.Inv_Gesture;
                case AssetType.Mesh:
                    return Properties.Resources.Inv_Mesh;
                case AssetType.Settings:
                    return Properties.Resources.Inv_SettingsDay;
            }

            return Properties.Resources.Inv_Shirt;
        }

        private Dictionary<UUID, DataGridViewRow> downloadManagerRows = new Dictionary<UUID, DataGridViewRow>();

        public class ExtraDownloadInfo
        {
            public string FileName { get; set; } = string.Empty;
            public DateTime Time { get; private set; }

            public ExtraDownloadInfo()
            {
                Time = DateTime.UtcNow;
            }
        }

        private void Assets_DownloadProgress(AssetDownloadProgressEventArgs event_args)
        {
            if(downloadsGridView.InvokeRequired)
            {
                downloadsGridView.BeginInvoke(new Action(() => { Assets_DownloadProgress(event_args); }));
                return;
            }
            //downloadsGridView.Rows.Add(Properties.Resources.Inv_Object, "Object", "dd4c1258-ffba-42ec-a9b0-6fe3528cde61", "Queued");
            //downloadsGridView.Rows.Add(Properties.Resources.Inv_Script, "LSLText", "e82b658a-6b16-4dd3-8f1d-9ecc7a29a2e3", "Queued");
            //downloadsGridView.Rows.Add(Properties.Resources.Inv_Script, "LSLText", "4ac131f9-ffc0-45f2-b3e3-4d9891e9f18e", "Queued");
            //downloadsGridView.Rows.Add(Properties.Resources.Inv_Notecard, "Notecard", "33809646-d87f-46a0-9a66-f5d0c1b90f5c", "Queued");
            //downloadsGridView.Rows.Add(Properties.Resources.Inv_Landmark, "Landmark", "9d0e7c42-cbfa-47b5-92ea-d1600c471073", "Queued");

            lock (downloadManagerRows)
            {
                DataGridViewRow row;
                if (downloadManagerRows.TryGetValue(event_args.AssetID, out row))
                {
                    if (event_args.Status == AssetDownloadState.Downloading)
                        row.Cells[3].Value = Convert.ToString((int)(((float)event_args.Received / (float)event_args.Total) * 100.0f)) + "%";
                    else
                        row.Cells[3].Value = event_args.Status.ToString();
                }
                else
                {
                    int index = downloadsGridView.Rows.Add(getIconForAssetType(event_args.Type), event_args.Type.ToString(), event_args.AssetID.ToString(), event_args.Status.ToString());
                    row = downloadsGridView.Rows[index];
                    row.Tag = new ExtraDownloadInfo();
                    downloadManagerRows.Add(event_args.AssetID, row);
                }
            }
        }

        public void AddSingleMenuItem(string label, HandleAvatarPicker handle)
        {
            avatarTracker.AddSingleMenuItem(label, handle);
        }


        public void AddMultipleMenuItem(string label, HandleAvatarPickerList handle)
        {
            avatarTracker.AddMultipleMenuItem(label, handle);
        }


        internal void AddInventoryItemOption(string label, CoolGUI.Controls.HandleInventory handle, CoolGUI.Controls.EnableInventory enable = null)
        {
            inventoryBrowserForm.AddInventoryItemOption(label, handle, enable);
        }

        internal void AddInventoryFolderOption(string label, CoolGUI.Controls.HandleInventoryFolder handle, CoolGUI.Controls.EnableInventoryFolder enable = null)
        {
            inventoryBrowserForm.AddInventoryFolderOption(label, handle, enable);
        }

        internal void AddInventoryItemOption(string label, CoolGUI.Controls.HandleInventory handle, AssetType assetType, CoolGUI.Controls.EnableInventory enable = null)
        {
            inventoryBrowserForm.AddInventoryItemOption(label, handle, assetType, enable);
        }

        internal void AddInventoryItemOption(string label, CoolGUI.Controls.HandleInventory handle, InventoryType invType, CoolGUI.Controls.EnableInventory enable = null)
        {
            inventoryBrowserForm.AddInventoryItemOption(label, handle, invType, enable);
        }

        internal void AddMainMenuOption(MenuOption option)
        {
            AddMenuOption(option);
        }

        internal void AddSettingsTab(string label, Panel panel)
        {
            TabPage page = new TabPage();
            page.Text = label;
            page.Controls.Add(panel);
            miscPluginsTabControl.TabPages.Add(page);
        }

        private void LoadPlugins()
        {
            OSD osd = CoolProxy.Frame.Settings.getOSD("PluginList");
            OSDArray arr = (OSDArray)osd;
            foreach (string str in arr)
            {
                bool add = false;

                int ret = LoadPlugin(str);

                if(ret == -1)
                {
                    if (MessageBox.Show(Path.GetFileName(str) + " threw an exception while loading, would you like to remove it?", "", MessageBoxButtons.YesNo) == DialogResult.No)
                    {
                        add = true;
                    }
                }
                else if (ret == 0)
                {
                    if (MessageBox.Show(Path.GetFileName(str) + " didn't load any commands or plugins, would you like to remove it?", "", MessageBoxButtons.YesNo) == DialogResult.No)
                    {
                        add = true;
                    }
                }
                else
                {
                    add = true;
                }

                if(add)
                {
                    pluginList.Add(str);

                    // Get the file version.
                    FileVersionInfo myFileVersionInfo = FileVersionInfo.GetVersionInfo(str);

                    pluginsDataGridView.Rows.Add(Path.GetFileName(str), myFileVersionInfo.FileVersion, str);
                }
            }

            SavePlugins(true);
        }

        private void OnGridAdded(GridInfo gridInfo)
        {
            if(this.InvokeRequired)
            {
                this.BeginInvoke(new Action(() => { OnGridAdded(gridInfo); }));
                return;
            }

            LoadGrids();
            gridsComboBox.SelectedItem = gridInfo.Name;
        }

        private void LoadGrids()
        {
            gridsComboBox.SelectedIndexChanged -= gridsComboBox_SelectedIndexChanged;
            gridsDataGridView.SelectionChanged -= gridsDataGridView_SelectionChanged;

            gridsComboBox.Items.Clear();
            gridsDataGridView.Rows.Clear();

            gridsComboBox.Items.AddRange(gridManager.getGridNames());

            foreach (var grid_info in gridManager.getGrids())
            {
                int i = gridsDataGridView.Rows.Add(grid_info.Name, grid_info.LoginURI);
                gridsDataGridView.Rows[i].Tag = grid_info;
            }

            gridsComboBox.SelectedIndexChanged += gridsComboBox_SelectedIndexChanged;
            gridsDataGridView.SelectionChanged += gridsDataGridView_SelectionChanged;
        }

        private void handleLoginResponse(XmlRpcResponse response)
        {
            Hashtable responseData;
            try
            {
                responseData = (Hashtable)response.Value;
            }
            catch (Exception e)
            {
                OpenMetaverse.Logger.Log(e.Message, Helpers.LogLevel.Error);
                return;
            }

            if (responseData.ContainsKey("login"))
            {
                if ((string)responseData["login"] != "true")
                {
                    return;
                }
            }

            CoolProxy.Frame.IsLindenGrid = CoolProxy.Frame.Settings.getBool("LindenGridSelected");

            string first_name = (string)responseData["first_name"];
            string last_name = (string)responseData["last_name"];

            first_name = first_name.Replace("\"", "");

            string full_name = first_name;
            if (last_name.ToLower() != "resident")
                full_name += " " + last_name;

            this.Invoke(new Action(() =>
            {
                this.Text = "Cool Proxy - " + full_name;
                trayIcon.Text = this.Text;
            }));

            CoolProxy.Frame.Settings.setString("LastAccountUsed", full_name);
        }



        /////////////////////////////////////////////////////////////////////////
        /// Asset Transfers Panel
        /////////////////////////////////////////////////////////////////////////

        private void button37_Click(object sender, EventArgs e)
        {
            Point p = PointToClient(MousePosition);
            blacklistContextMenuStrip.Show(this, p);
        }

        ////////////////////////////////////////////////////////////////////////
        /// Avatar Tracker Panel
        ////////////////////////////////////////////////////////////////////////
        ///



        ////////////////////////////////////////////////////////////////////////
        /// Settings Panel
        ////////////////////////////////////////////////////////////////////////

        private void addGridButton_Click(object sender, EventArgs e)
        {
            string url = addGridTextbox.Text + "/get_grid_info";

            if(!url.StartsWith("http://") && !url.StartsWith("https://"))
            {
                url = "http://" + url;
            }

            string result = "";
            Task.Run(() =>
            {
                HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create(url);
                using (WebResponse myResponse = myRequest.GetResponse())
                {
                    using (StreamReader sr = new StreamReader(myResponse.GetResponseStream(), System.Text.Encoding.UTF8))
                    {
                        result = sr.ReadToEnd();
                    }
                }
            }).ContinueWith(x =>
            {
                try
                {
                    var test = XDocument.Parse(result).Element("gridinfo");
                    var dict = test.Elements().ToDictionary(el => el.Name.ToString(), el => el.Value);

                    string error;
                    if(!gridManager.addGridFromDictionary(dict, out error))
                    {
                        MessageBox.Show(error);
                    }
                }
                catch
                {
                    MessageBox.Show("Invalid GridInfo!");
                }
            });
        }

        private void removeGridButton_Click(object sender, EventArgs e)
        {
            var row = gridsDataGridView.SelectedRows[0];
            var info = row.Tag as GridInfo;
            gridManager.removeGrid(info.Name);
            gridsDataGridView.Rows.Remove(row);
        }

        private void gridsDataGridView_SelectionChanged(object sender, EventArgs e)
        {
            var cell = gridsDataGridView.SelectedRows[0];
            var info = cell.Tag as GridInfo;
            removeGridButton.Enabled = !info.IsLindenGrid;
        }

        private void showDebugSettingsButton_Click(object sender, EventArgs e)
        {
            DebugSettingForm debug = new DebugSettingForm();
            debug.ShowDialog();
        }

        //////////////////////////////////////////////////////////////////////////
        /// Misc Tools Panel
        //////////////////////////////////////////////////////////////////////////

        Dictionary<string, GroupBox> toolCategories = new Dictionary<string, GroupBox>();

        private GroupBox GetGroupBox(string category)
        {
            GroupBox groupBox;
            if (!toolCategories.TryGetValue(category, out groupBox))
            {
                groupBox = new GroupBox();
                groupBox.Text = category;
                groupBox.Width = 200;
                groupBox.Height = 300;
                groupBox.AutoSize = true;

                flowLayoutPanel1.Controls.Add(groupBox);
                toolCategories.Add(category, groupBox);
            }

            return groupBox;
        }


        private Button createToolButton(string label, GroupBox groupBox)
        {
            Button button = new Button();
            button.Text = label;
            button.Height = 23;
            button.Width = 200;
            if (groupBox.Controls.Count > 0)
            {
                var previous = groupBox.Controls[groupBox.Controls.Count - 1];
                button.Location = new Point(5, previous.Location.Y + previous.Height + 5);
            }
            else
                button.Location = new Point(5, 15);
            button.Margin = new Padding(3);

            return button;
        }

        private ComboBox createToolComboBox(GroupBox groupBox)
        {
            ComboBox comboBox = new ComboBox();
            //comboBox.Text = label;
            comboBox.Height = 23;
            comboBox.Width = 200;
            comboBox.DropDownStyle = ComboBoxStyle.DropDownList;

            comboBox.MouseWheel += (x, y) => ((HandledMouseEventArgs)y).Handled = true;

            if (groupBox.Controls.Count > 0)
            {
                var previous = groupBox.Controls[groupBox.Controls.Count - 1];
                comboBox.Location = new Point(5, previous.Location.Y + previous.Height + 5);
            }
            else
                comboBox.Location = new Point(5, 15);
            comboBox.Margin = new Padding(3);

            groupBox.Controls.Add(comboBox);

            return comboBox;
        }

        private CoolGUI.Controls.CheckBox createToolCheckbox(string label, GroupBox groupBox, bool button_style = false)
        {
            CoolGUI.Controls.CheckBox checkBox = new CoolGUI.Controls.CheckBox();
            checkBox.Text = label;
            checkBox.Height = 20;
            checkBox.Width = 180;

            if (button_style)
            {
                checkBox.Appearance = Appearance.Button;
                checkBox.TextAlign = ContentAlignment.MiddleCenter;
                checkBox.AutoSize = false;
                checkBox.Height = 23;
                checkBox.Width = 200;
            }

            int x = button_style ? 5 : 15;

            if (groupBox.Controls.Count > 0)
            {
                var previous = groupBox.Controls[groupBox.Controls.Count - 1];
                checkBox.Location = new Point(x, previous.Location.Y + previous.Height + 5);
            }
            else
                checkBox.Location = new Point(x, 15);
            checkBox.Margin = new Padding(3);

            return checkBox;
        }


        public void AddToolButton(string category, string label, EventHandler eventHandler)
        {
            var groupBox = GetGroupBox(category);
            var button = createToolButton(label, groupBox);

            button.Click += eventHandler;
            groupBox.Controls.Add(button);
        }


        public CoolGUI.Controls.CheckBox AddToolCheckbox(string category, string label, EventHandler handler, bool button_style = false)
        {
            var groupBox = GetGroupBox(category);
            var checkBox = createToolCheckbox(label, groupBox, button_style);

            checkBox.CheckedChanged += handler;
            groupBox.Controls.Add(checkBox);

            return checkBox;
        }

        public CoolGUI.Controls.CheckBox AddToolCheckbox(string category, string label, string setting)
        {
            var groupBox = GetGroupBox(category);
            var checkBox = createToolCheckbox(label, groupBox);

            checkBox.Setting = setting;
            groupBox.Controls.Add(checkBox);

            return checkBox;
        }


        public void AddToolLabel(string category, string text)
        {
            var groupBox = GetGroupBox(category);

            Label label = new Label();
            label.Text = text;
            label.Height = 14;
            label.Width = 180;
            if (groupBox.Controls.Count > 0)
            {
                var previous = groupBox.Controls[groupBox.Controls.Count - 1];
                label.Location = new Point(5, previous.Location.Y + previous.Height + 5);
            }
            else
                label.Location = new Point(5, 15);
            label.Margin = new Padding(3);

            groupBox.Controls.Add(label);
        }

        public void AddToolComboBox(string category, EventHandler handler, object[] values, object default_item)
        {
            var groupBox = GetGroupBox(category);
            var comboBox = createToolComboBox(groupBox);

            if (values != null)
                comboBox.Items.AddRange(values);

            if (default_item != null)
                comboBox.SelectedItem = default_item;

            comboBox.SelectedValueChanged += handler;
            groupBox.Controls.Add(comboBox);
        }


        /////////////////////////////////////////////////////////////////////////
        /// Login Panel
        /////////////////////////////////////////////////////////////////////////

        private void toggleProxyButton_Click(object sender, EventArgs e)
        {
            ToggleProxy();
        }

        private void ToggleProxy()
        {
            if (CoolProxy.Frame.Started)
            {
                button1.Text = "Start Proxy";
                CoolProxy.Frame.Stop();
                textBox1.Enabled = numericUpDown2.Enabled = true;
            }
            else
            {
                button1.Text = "Stop Proxy";
                CoolProxy.Frame.Start();
                textBox1.Enabled = numericUpDown2.Enabled = false;
            }
        }

        private void editGridsButton_Click(object sender, EventArgs e)
        {
            settingsTabControl.SelectedTab = gridsTabPage;
            tabControl1.SelectedTab = settingsTabPage;
        }

        private void gridsComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            string gridname = (string)gridsComboBox.SelectedItem;
            gridManager.selectGrid(gridname);

            var info = gridManager.getInfoFromName(gridname);

            CoolProxy.Frame.Config.remoteLoginUri = new Uri(info.LoginURI);
            CoolProxy.Frame.Config.is_linden_grid = info.IsLindenGrid;
            //MessageBox.Show(CoolProxy.Frame.Config.remoteLoginUri.ToString());

            // Update account list...
            //accountsComboBox.Items.Clear();
            //Dictionary<string, SpoofingInfo> accounts;
            //if(userSpoofingInfoList.TryGetValue(info.Name, out accounts))
            //    accountsComboBox.Items.AddRange(accounts.Keys.ToArray());

            //accountsComboBox.Items.Add("- New Login -");
            //accountsComboBox.SelectedIndex = 0;
        }

        /////////////////////////////////////////////////////////////////////////
        /// Main Form
        /////////////////////////////////////////////////////////////////////////

        private void CoolProxyForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            //if (CoolProxy.Frame.Started)
            //{
            //    if (MessageBox.Show("Are you sure you want to quit?", "Cool Proxy is still active", MessageBoxButtons.YesNo) == DialogResult.Yes)
            //    {
            //        CoolProxy.Frame.Stop();
            //    }
            //    else
            //        e.Cancel = true;
            //}

            if(!coolProxyIsQuitting)
            {
                e.Cancel = true;
                this.WindowState = FormWindowState.Minimized;
            }
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized && CoolProxy.Frame.Settings.getBool("MinimizeCoolProxyToTray"))
            {
                this.Hide();
                //trayIcon.Visible = true;

                if (firstTimeMinimized)
                {
                    trayIcon.BalloonTipIcon = ToolTipIcon.Info;
                    trayIcon.BalloonTipText = "Cool Proxy is still running! Double click the tray icon to re-open the GUI, or right-click for a context menu.";
                    trayIcon.ShowBalloonTip(1500);
                    firstTimeMinimized = false;
                }
            }
        }

        private void trayIcon_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (!this.Visible)
                {
                    this.Show();
                    this.WindowState = FormWindowState.Normal;
                    this.Activate();

                    CoolProxy.Frame.Settings.setBool("AlertStillRunning", false);
                }
                else
                {
                    this.WindowState = FormWindowState.Normal;
                    this.Activate();
                }
                //trayIcon.Visible = false;
            }
        }

        private void tabControl1_DrawItem(object sender, DrawItemEventArgs e)
        {
            Graphics g = e.Graphics;
            Brush _textBrush;

            // Get the item from the collection.
            TabPage _tabPage = tabControl1.TabPages[e.Index];

            // Get the real bounds for the tab rectangle.
            Rectangle _tabBounds = tabControl1.GetTabRect(e.Index);

            if (e.State == DrawItemState.Selected)
            {

                // Draw a different background color, and don't paint a focus rectangle.
                //_textBrush = new SolidBrush(Color.Red);
                _textBrush = new SolidBrush(e.ForeColor);
                g.FillRectangle(Brushes.Gray, e.Bounds);
            }
            else
            {
                _textBrush = new System.Drawing.SolidBrush(e.ForeColor);
                e.DrawBackground();
            }

            // Use our own font.
            Font _tabFont = new Font("Arial", (float)10.0, FontStyle.Bold, GraphicsUnit.Pixel);

            // Draw string. Center the text.
            StringFormat _stringFlags = new StringFormat();
            _stringFlags.Alignment = StringAlignment.Center;
            _stringFlags.LineAlignment = StringAlignment.Center;
            g.DrawString(_tabPage.Text, _tabFont, _textBrush, _tabBounds, new StringFormat(_stringFlags));
        }

        //private void onKeyPressed(object sender, KeyPressedEventArgs e)
        //{
        //    if (e.Modifier == L33T.GUI.ModifierKeys.Control && e.Key == Keys.E)
        //    {
        //        trayContextMenu.Show(Cursor.Position);
        //    }
        //}

        ////////////////////////////////////////////////////////////////////////
        /// Test Stuff
        ////////////////////////////////////////////////////////////////////////

        private void loadPluginTestButton_Click(object sender, EventArgs e)
        {
            using(OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Filter = "DLL Files|*.dll";
                if(dialog.ShowDialog() == DialogResult.OK)
                {
                    string filename = dialog.FileName;
                    LoadPlugin(filename);
                }
            }
        }



        public int LoadPlugin(string name)
        {
            int count = 0;
            try
            {
                Assembly assembly = Assembly.LoadFile(Path.GetFullPath(name));
                foreach (Type t in assembly.GetTypes())
                {
                    if (t.IsSubclassOf(typeof(CoolProxyPlugin)))
                    {
                        //Activator.CreateInstance(t, CoolProxy.Settings, guiManager, CoolProxy.Frame);
                        Activator.CreateInstance(t, CoolProxy.Frame);
                        count++;

                        //ConstructorInfo info = t.GetConstructor(new Type[] { typeof(SettingsManager), typeof(GUIManager), typeof(CoolProxyFrame) });
                        //if(info != null)
                        //{

                        //    CoolProxyPlugin plugin = (CoolProxyPlugin)info.Invoke(new object[] { CoolProxy.Settings, guiManager, CoolProxy.Frame });
                        //}
                    }
                    else if (t.IsSubclassOf(typeof(Command)))
                    {
                        Command command = (Command)Activator.CreateInstance(t, CoolProxy.Frame);
                        CoolProxy.Frame.AddChatCommand(command);
                        count++;

                        //ConstructorInfo info = t.GetConstructor(new Type[] { typeof(SettingsManager), typeof(CoolProxyFrame) });
                        //if (info != null)
                        //{
                        //    Command command = (Command)info.Invoke(new object[] { CoolProxy.Settings, CoolProxy.Frame });
                        //    CoolProxy.Frame.AddChatCommand(command);
                        //}
                    }
                }
            }
            catch (Exception e)
            {
                OpenMetaverse.Logger.Log("LoadPlugin exception", Helpers.LogLevel.Error, e);
                return -1;
            }

            return count;
        }

        public void AddToggleFormQuick(string cat, string name, Form form)
        {
            CheckBox ASTCheck = AddToolCheckbox(cat, name, (x, y) =>
            {
                CheckBox checkbox = x as CheckBox;

                if (checkbox.Checked)
                    form.Show();
                else
                    form.Hide();
            }, true);

            form.VisibleChanged += (x, y) =>
            {
                ASTCheck.Checked = form.Visible;
            };

            form.FormClosing += (x, y) =>
            {
                y.Cancel = true;
                form.Hide();
                ASTCheck.Checked = false;
            };

            form.Deactivate += HandleFormDeactivated;
            form.Activated += HandleFormActivated;

            form.TopMost = CoolProxy.Frame.Settings.getBool("KeepCoolProxyOnTop");
            CoolProxy.Frame.Settings.getSetting("KeepCoolProxyOnTop").OnChanged += (x, y) => { form.TopMost = (bool)y.Value; };
        }

        private void CoolProxyForm_Load(object sender, EventArgs e)
        {
            if(CoolProxy.Frame.Settings.getBool("FirstRun"))
            {
                OSDArray plugins = new OSDArray();

                plugins.Add("CoolProxy.Plugins.OpenSim.dll");
                plugins.Add("CoolProxy.Plugins.NotecardMagic.dll");

                plugins.Add("CoolProxy.Plugins.Useful.dll");
                plugins.Add("CoolProxy.Plugins.ClientAO.dll");
                plugins.Add("CoolProxy.Plugins.CopyBot.dll");
                plugins.Add("CoolProxy.Plugins.DynamicGroupTitle.dll");
                plugins.Add("CoolProxy.Plugins.FancyBeams.dll");
                plugins.Add("CoolProxy.Plugins.KeyTool.dll");
                plugins.Add("CoolProxy.Plugins.LocalGodMode.dll");
                plugins.Add("CoolProxy.Plugins.MimicTool.dll");
                plugins.Add("CoolProxy.Plugins.Textures.dll");

                plugins.Add("CoolProxy.Plugins.InventoryBackup.dll");
                plugins.Add("CoolProxy.Plugins.Masking.dll");
                plugins.Add("CoolProxy.Plugins.Editors.dll");
                plugins.Add("CoolProxy.Plugins.GridIMHacks.dll");
                plugins.Add("CoolProxy.Plugins.HackedProfileEditor.dll");
                plugins.Add("CoolProxy.Plugins.MagicRez.dll");
                plugins.Add("CoolProxy.Plugins.MegaPrimMaker.dll");
                plugins.Add("CoolProxy.Plugins.ServiceTools.dll");
                plugins.Add("CoolProxy.Plugins.Spammers.dll");
                plugins.Add("CoolProxy.Plugins.SuperSuitcase.dll");
                plugins.Add("CoolProxy.Plugins.GetAvatar.dll");

                CoolProxy.Frame.Settings.setOSD("PluginList", plugins);
            }

            LoadPlugins();

            foreach (var cmd in CoolProxy.Frame.Commands.Values)
            {
                ChatCommandAdded(cmd.CMD, cmd.Name, cmd.Description);
            }

            for (int i = 0; i < tabControl1.TabPages.Count; i++)
            {
                TabPage t = tabControl1.TabPages[i];

                string tab_name = string.Join("", t.Text.Split(' '));

                string popped_setting = "Tab" + tab_name + "Popped";
                string open_setting = "Tab" + tab_name + "Open";

                if (CoolProxy.Frame.Settings.getBool(popped_setting))
                {
                    PopTab(t, !CoolProxy.Frame.Settings.getBool(open_setting));
                }
            }

            LoadMainMenu();
            SaveMainMenu();
            ApplyMainMenu();

            CoolProxy.Frame.Settings.setBool("FirstRun", false);
        }

        Dictionary<string, Size> formSizeList = new Dictionary<string, Size>()
        {
            { "FormLoginSize", new Size(475, 305) },
            { "FormAvatarTrackerSize", new Size(339, 164) },
            { "FormRegionsSize", new Size(400, 173) },
            { "FormToolBoxSize", new Size(251, 521) },
            { "FormAssetLoggingSize", new Size(462, 510) },
            { "FormAssetTransfersSize", new Size(400, 280) },
            { "FormSettingsSize", new Size(477, 305) },
            { "FormConsoleSize", new Size(650, 400) }
        };

        Dictionary<string, Form> openTabs = new Dictionary<string, Form>();

        private void tabControl1_DoubleClick(object sender, EventArgs e)
        {
            TabPage page = tabControl1.SelectedTab;
            PopTab(page, false);
        }

        private void PopTab(TabPage page, bool hide)
        {
            if(openTabs.TryGetValue(page.Text, out Form existing))
            {
                existing.Focus();
                return;
            }

            Form testForm = new Form();
            testForm.Size = new Size(400, 300);
            testForm.Text = page.Text;
            testForm.Tag = page;
            testForm.TopMost = CoolProxy.Frame.Settings.getBool("KeepCoolProxyOnTop");
            testForm.ShowIcon = false;
            CoolProxy.Frame.Settings.getSetting("KeepCoolProxyOnTop").OnChanged += (x, y) => { testForm.TopMost = (bool)y.Value; };

            openTabs.Add(page.Text, testForm);

            string tab_name = string.Join("", page.Text.Split(' '));

            string size_setting = "Form" + tab_name + "Size";
            string popped_setting = "Tab" + tab_name + "Popped";
            string open_setting = "Tab" + tab_name + "Open";
            string pos_settings = "Tab" + tab_name + "Pos";


            CoolProxy.Frame.Settings.setBool(popped_setting, true);

            Vector3 form_pos = CoolProxy.Frame.Settings.getVector(pos_settings);
            if (form_pos != Vector3.Zero)
            {
                testForm.StartPosition = FormStartPosition.Manual;
                testForm.Location = new Point((int)form_pos.X, (int)form_pos.Y);
            }

            Size size;
            if(formSizeList.TryGetValue(size_setting, out size))
            {
                testForm.Size = size;
            }
            
            foreach(Control control in page.Controls)
                control.Parent = testForm;

            LinkLabel bring_back = new LinkLabel();
            bring_back.Text = "Bring back " + page.Text;
            bring_back.Location = new Point(20, 20);
            bring_back.AutoSize = true;

            LinkLabel show = new LinkLabel();
            show.Text = "Show";
            show.Location = new Point(20, 40);
            show.AutoSize = true;

            show.Click += (x, y) =>
            {
                if (testForm.Visible)
                {
                    if (testForm.WindowState == FormWindowState.Minimized)
                        testForm.WindowState = FormWindowState.Normal;

                    testForm.Activate();
                }
                else
                    testForm.Show();
            };

            bring_back.Click += (x, y) =>
            {
                if (testForm.WindowState == FormWindowState.Minimized)
                    testForm.WindowState = FormWindowState.Normal;

                formSizeList[size_setting] = testForm.Size;

                TabPage returning_page = (TabPage)testForm.Tag;

                returning_page.Controls.Remove(bring_back);
                returning_page.Controls.Remove(show);

                foreach (Control control in testForm.Controls)
                    control.Parent = page;

                testForm.Close();

                openTabs.Remove(returning_page.Text);

                CoolProxy.Frame.Settings.setBool(popped_setting, false);
            };


            testForm.FormClosing += (x, y) =>
            {
                CoolProxy.Frame.Settings.setBool(open_setting, false);
                y.Cancel = true;
                testForm.Hide();
            };

            testForm.Move += (x, y) =>
            {
                CoolProxy.Frame.Settings.setVector(pos_settings, new Vector3(testForm.Location.X, testForm.Location.Y, 0.0f));
            };

            testForm.VisibleChanged += (x, y) =>
            {
                if(testForm.Visible)
                {
                    CoolProxy.Frame.Settings.setBool(open_setting, true);
                }
            };

            testForm.Activated += HandleFormActivated;
            testForm.Deactivate +=HandleFormDeactivated;

            inventoryBrowserForm.Activated += HandleFormActivated;
            inventoryBrowserForm.Deactivate += HandleFormDeactivated;

            page.Controls.Add(bring_back);
            page.Controls.Add(show);

            if (!hide)
                testForm.Show();
        }

        private void HandleFormActivated(object sender, EventArgs e)
        {
            var form = sender as Form;
            form.Opacity = 1.0f;
        }

        private void HandleFormDeactivated(object sender, EventArgs e)
        {
            var form = sender as Form;
            if (!form.Disposing)
                form.Opacity = 0.5f;
        }

        private bool isPoppedTabOpen(object user_data)
        {
            if (openTabs.TryGetValue((string)user_data, out Form form))
            {
                return form.Visible;
            }

            return false;
        }

        private void tabNameToolStripMenuItem_Click(object user_data)
        {
            if (openTabs.TryGetValue((string)user_data, out Form form))
            {
                if (form.Visible)
                    form.Hide();
                else form.Show();
            }
            else
            {
                this.Show();
                this.WindowState = FormWindowState.Normal;
                this.Focus();

                TabPage page = tabControl1.TabPages.Cast<TabPage>().First(x => x.Text == (string)user_data);

                tabControl1.SelectedTab = page;
            }
        }

        private bool coolProxyIsQuitting = false;

        private void quitCoolProxyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (CoolProxy.Frame.Started)
            {
                if (MessageBox.Show(this, "Are you sure you want to quit?", "Cool Proxy is still active", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    CoolProxy.Frame.Stop();
                    coolProxyIsQuitting = true;
                    this.Close();
                }
            }
            else
            {
                coolProxyIsQuitting = true;
                this.Close();
            }
        }


        private void soundsListContextMenu_Opening(object sender, CancelEventArgs e)
        {
            int count = soundsDataGridView.SelectedRows.Count;
            if (count == 0)
            {
                e.Cancel = true;
                return;
            }

            bool single = count == 1;
            playLocallyToolStripMenuItem.Visible = single;
            playInworldToolStripMenuItem.Visible = single;
            toolStripSeparator9.Visible = single;
        }

        private void playLocallyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UUID sound_id = UUID.Parse((string)soundsDataGridView.SelectedRows[0].Cells[1].Value);
            TriggerSound(sound_id, true);
        }

        private void playInworldToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UUID sound_id = UUID.Parse((string)soundsDataGridView.SelectedRows[0].Cells[1].Value);
            TriggerSound(sound_id, false);
        }

        void TriggerSound(UUID sound_id, bool local)
        {
            SoundTriggerPacket packet = new SoundTriggerPacket();
            packet.SoundData.SoundID = sound_id;
            packet.SoundData.Position = CoolProxy.Frame.Agent.SimPosition;
            packet.SoundData.Handle = CoolProxy.Frame.Network.CurrentSim.Handle;
            packet.SoundData.Gain = 1.0f;

            CoolProxy.Frame.Network.CurrentSim.Inject(packet, local ? GridProxy.Direction.Incoming : GridProxy.Direction.Outgoing);
        }

        private void soundsDataGridView_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            int count = soundsDataGridView.SelectedRows.Count;
            if (count == 1)
            {
                UUID sound_id = UUID.Parse((string)soundsDataGridView.SelectedRows[0].Cells[1].Value);
                TriggerSound(sound_id, true);
            }
        }

        private void copyRegionIPToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(regionsDataGridView.SelectedRows.Count == 1)
            {
                RegionProxy region = (RegionProxy)regionsDataGridView.SelectedRows[0].Tag;
                Clipboard.SetText(region.RemoteEndPoint.ToString());
            }
        }

        private void copyRegionIDToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (regionsDataGridView.SelectedRows.Count == 1)
            {
                RegionProxy region = (RegionProxy)regionsDataGridView.SelectedRows[0].Tag;
                Clipboard.SetText(region.ID.ToString());
            }
        }

        private void regionsContextMenu_Opening(object sender, CancelEventArgs e)
        {
            if (regionsDataGridView.SelectedRows.Count == 0)
            {
                e.Cancel = true;
                return;
            }

            var region = (RegionProxy)regionsDataGridView.SelectedRows[0].Tag;
        }

        private void clearCacheButton_Click(object sender, EventArgs e)
        {
            if(MessageBox.Show("Clear asset cache now?", "", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                string dir = CoolProxy.Frame.Settings.getString("AssetCacheDir");
                foreach (var file in Directory.GetFiles(dir))
                {
                    File.Delete(file);
                }
                OpenMetaverse.Logger.Log("Asset Cache cleared!", Helpers.LogLevel.Info);
            }
        }

        private void inventoryBrowserToolStripMenuItem_Click(object user_data)
        {
            if (!inventoryBrowserForm.Visible)
                inventoryBrowserForm.Show();
            else
                inventoryBrowserForm.Hide();
        }

        private void dataGridView1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
            else e.Effect = DragDropEffects.None;
        }

        List<string> pluginList = new List<string>();

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

        bool HasBeenToldToRestart = false;

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

            CoolProxy.Frame.Settings.setOSD("PluginList", osd_array);

            if(!HasBeenToldToRestart && !is_startup)
            {
                HasBeenToldToRestart = true;
                MessageBox.Show("You need to restart CoolProxy for plugins to reload!");
            }    
        }

        private void removePluginButton_Click(object sender, EventArgs e)
        {
            if(pluginsDataGridView.SelectedRows.Count > 0)
            {
                foreach(DataGridViewRow row in pluginsDataGridView.SelectedRows)
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

            foreach(var pair in rows)
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

        private void animsDataGridView_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            var row = animsDataGridView.Rows[e.RowIndex];
            UUID agent_id = UUID.Parse((string)row.Cells[0].Value);
            AgentDisplayName name = (AgentDisplayName)row.Tag;
            var form = new AvatarAnimationsForm(name, AvatarAnimations[agent_id].Item2);

            form.TopMost = CoolProxy.Frame.Settings.getBool("KeepCoolProxyOnTop");
            CoolProxy.Frame.Settings.getSetting("KeepCoolProxyOnTop").OnChanged += (x, y) => { form.TopMost = (bool)y.Value; };

            form.Show();
        }

        private void addCoolProxyToViewerButton_Click(object sender, EventArgs e)
        {
            string sl_user_data = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SecondLife\\user_settings\\");

            if (!Directory.Exists(sl_user_data))
            {
                MessageBox.Show("user_settings directory not found! Do you not have the official LL viewer installed?");
                return;
            }

            string settings_path = Path.Combine(sl_user_data, "grids.xml");

            OSDMap grids_map;

            if (File.Exists(settings_path))
            {
                grids_map = (OSDMap)OSDParser.DeserializeLLSDXml(File.ReadAllBytes(settings_path));
            }
            else grids_map = new OSDMap();

            if(!grids_map.ContainsKey("cool.proxy"))
            {
                string address = CoolProxy.Frame.Settings.getString("GridProxyListenAddress");
                int port = CoolProxy.Frame.Settings.getInteger("GridProxyListenPort");

                OSDMap grid = new OSDMap();
                grid["label"] = "Cool Proxy" + (port != 8080 ? " (" + port.ToString() + ")" : string.Empty);
                grid["keyname"] = "cool.proxy" + (port != 8080 ? "." + port.ToString() : string.Empty);
                grid["system_grid"] = false;

                OSDArray identifiers = new OSDArray();
                identifiers.Add("agent");
                identifiers.Add("account");
                grid["login_identifier_types"] = identifiers;

                grid["login_page"] = string.Format("http://{0}:{1}/splash", address, port);
                grid["login_uri"] = string.Format("http://{0}:{1}/login", address, port);
                grid["slurl_base"] = string.Format("http://{0}:{1}/slurl", address, port);
                grid["web_profile_url"] = string.Format("http://{0}:{1}/profile", address, port);

                grids_map["cool.proxy" + (port != 8080 ? "." + port.ToString() : string.Empty)] = grid;

                byte[] data = OSDParser.SerializeLLSDXmlBytes(grids_map);
                File.WriteAllBytes(settings_path, data);

                MessageBox.Show("Done! CoolProxy will now be on the grid list! (Ctrl+Shift+G if you can't see it!");
            }
            else
            {
                MessageBox.Show("CoolProxy is already on your grids list!");
            }
        }

        private void CoolProxyForm_Shown(object sender, EventArgs e)
        {
            if (CoolProxy.Frame.Settings.getBool("StartProxyAtLaunch"))
            {
                ToggleProxy();

                if (CoolProxy.Frame.Settings.getBool("HideProxyAtLaunch"))
                {
                    this.WindowState = FormWindowState.Minimized;
                }
            }
        }

        private void githubLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://github.com/StolenRuby/coolproxy");
        }

        private void cmdPrefixCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            string prefix = cmdPrefixCombo.SelectedItem.ToString();
            CoolProxy.Frame.Settings.setString("ChatCommandPrefix", prefix);
        }

        private void blacklistToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in soundsDataGridView.SelectedRows)
            {
                UUID asset_id = UUID.Parse((string)row.Cells[1].Value);
                BlacklistAsset(asset_id);
            }
        }


        //////////////////////////////////////////////////////////////////////

        Dictionary<string, MenuOption> MainMenuOptions = new Dictionary<string, MenuOption>();

        MenuFolder MainMenu = new MenuFolder("MAIN_MENU", "Main Menu");

        List<string> KnownMenuNames = new List<string>();

        void AddMenuOption(MenuOption option)
        {
            if (MainMenuOptions.ContainsKey(option.Name))
            {
                OpenMetaverse.Logger.Log("MenuOption `" + option.Name + "` already exists!", Helpers.LogLevel.Warning);
                return;
            }

            MainMenuOptions.Add(option.Name, option);
        }

        private void AddMenuItems(List<MenuItem> items, ToolStripItemCollection dropDownItems)
        {
            foreach (var item in items)
            {
                if (item is MenuSeparator)
                {
                    dropDownItems.Add("-");
                }
                else if (item is MenuFolder)
                {
                    var folder = item as MenuFolder;

                    ToolStripMenuItem p = (ToolStripMenuItem)dropDownItems.Add(item.Label);
                    p.ForeColor = folder.Color;

                    AddMenuItems(folder.SubItems, p.DropDownItems);
                }
                else if (item is MenuOption)
                {
                    var opt = item as MenuOption;
                    ToolStripMenuItem e = (ToolStripMenuItem)dropDownItems.Add(opt.Label);
                    e.Tag = opt;

                    e.Click += (x, y) => opt.Clicked?.Invoke(opt.Tag);
                    //e.Checked = opt.Checked?.Invoke(e) ?? false;
                    e.ForeColor = item.Color;
                }
            }
        }

        #region treeview stuff
        private void treeView1_ItemDrag(object sender, ItemDragEventArgs e)
        {
            // Move the dragged node when the left mouse button is used.  
            if (e.Button == MouseButtons.Left)
            {
                DoDragDrop(e.Item, DragDropEffects.Move);
            }

            //// Copy the dragged node when the right mouse button is used.  
            //else if (e.Button == MouseButtons.Right)
            //{
            //    DoDragDrop(e.Item, DragDropEffects.Copy);
            //}
        }

        private void treeView1_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = e.AllowedEffect;
        }

        private void treeView1_DragOver(object sender, DragEventArgs e)
        {
            // Retrieve the client coordinates of the mouse position.  
            Point targetPoint = treeView1.PointToClient(new Point(e.X, e.Y));

            // Select the node at the mouse position.  
            treeView1.SelectedNode = treeView1.GetNodeAt(targetPoint);
        }

        private void treeView1_DragDrop(object sender, DragEventArgs e)
        {
            // Retrieve the client coordinates of the drop location.  
            Point targetPoint = treeView1.PointToClient(new Point(e.X, e.Y));

            // Retrieve the node at the drop location.  
            TreeNode targetNode = treeView1.GetNodeAt(targetPoint);

            // Retrieve the node that was dragged.  
            TreeNode draggedNode = (TreeNode)e.Data.GetData(typeof(TreeNode));

            // Confirm that the node at the drop location is not   
            // the dragged node or a descendant of the dragged node.  
            if (!draggedNode.Equals(targetNode) && !ContainsNode(draggedNode, targetNode))
            {
                // If it is a move operation, remove the node from its current   
                // location and add it to the node at the drop location.  
                if (e.Effect == DragDropEffects.Move)
                {
                    MenuItem item = draggedNode.Tag as MenuItem;
                    MenuFolder old_parent = draggedNode.Parent.Tag as MenuFolder;

                    draggedNode.Remove();

                    if(targetNode.Text.StartsWith("<"))
                    {
                        MenuFolder fol = targetNode.Tag as MenuFolder;
                        targetNode.Nodes.Add(draggedNode);
                        fol.SubItems.Add(item);

                        old_parent.SubItems.Remove(item);
                    }
                    else
                    {
                        MenuFolder fol = targetNode.Parent.Tag as MenuFolder;
                        targetNode.Parent.Nodes.Insert(targetNode.Index + 1, draggedNode);
                        old_parent.SubItems.Remove(item);
                        fol.SubItems.Insert(targetNode.Index + 1, draggedNode.Tag as MenuItem);
                    }
                }

                // If it is a copy operation, clone the dragged node   
                // and add it to the node at the drop location.  
                //else if (e.Effect == DragDropEffects.Copy)
                //{
                //    targetNode.Nodes.Add((TreeNode)draggedNode.Clone());
                //}

                // Expand the node at the location   
                // to show the dropped node.  
                targetNode.Expand();
            }
        }

        private bool ContainsNode(TreeNode node1, TreeNode node2)
        {
            // Check the parent node of the second node.  
            if (node2.Parent == null) return false;
            if (node2.Parent.Equals(node1)) return true;

            // If the parent node is not null or equal to the first node,   
            // call the ContainsNode method recursively using the parent of   
            // the second node.  
            return ContainsNode(node1, node2.Parent);
        }

        private void treeView1_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            treeView1.SelectedNode = e.Node;
        }
        #endregion

        #region menu editor options
        private void menuEditorMenu_Opening(object sender, CancelEventArgs e)
        {
            var node = treeView1.SelectedNode;
            if (node == null)
            {
                e.Cancel = true;
                return;
            }

            removeToolStripMenuItem.Visible = node.Parent != null;

            bool folder = node.Text.StartsWith("<");

            setColourToolStripMenuItem.Visible = (folder && node.Parent != null) || node.Tag is MenuOption;
            addFolderToolStripMenuItem.Visible = addOptionToolStripMenuItem.Visible = addSeparatorToolStripMenuItem.Visible = folder;
        }

        private void addSeparatorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var node = treeView1.SelectedNode;
            var add = node.Nodes.Insert(0, "-- Separator --");

            var folder = node.Tag as MenuFolder;
            var sep = new MenuSeparator();
            folder.SubItems.Insert(0, sep);
            add.Tag = sep;

            node.Expand();
        }

        private void removeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var node = treeView1.SelectedNode;
            var item = node.Tag as MenuItem;

            var parent = node.Parent.Tag as MenuFolder;
            parent.SubItems.Remove(item);
            treeView1.SelectedNode.Remove();
        }

        private void addOptionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var list = MainMenuOptions.Keys.ToArray();
            ListDialogBox listDialogBox = new ListDialogBox(list);
            listDialogBox.TopMost = this.TopMost;
            if(listDialogBox.ShowDialog() == DialogResult.OK)
            {
                var opt = MainMenuOptions[listDialogBox.SelectedOption];

                var node = treeView1.SelectedNode;

                var folder = node.Tag as MenuFolder;
                folder.SubItems.Insert(0, opt);

                var add = node.Nodes.Insert(0, opt.Name, opt.Label);
                add.Tag = opt;

                node.Expand();
            }
        }

        private void addFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TextBoxDialogBox textBoxDialogBox = new TextBoxDialogBox(string.Empty, "Enter a folder name");
            textBoxDialogBox.TopMost = this.TopMost;
            if(textBoxDialogBox.ShowDialog() == DialogResult.OK)
            {
                var node = treeView1.SelectedNode;

                MenuFolder folder = new MenuFolder("FOLDER_" + textBoxDialogBox.Result.Replace(" ", "_").ToUpper(), textBoxDialogBox.Result);
                var n = node.Nodes.Insert(0, folder.Name, "<" + folder.Label + ">");
                n.Tag = folder;

                MenuFolder parent = node.Tag as MenuFolder;
                parent.SubItems.Insert(0, folder);

                node.Expand();
            }
        }

        private void setColourToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ColorDialog colorDialog = new ColorDialog();
            if(colorDialog.ShowDialog() == DialogResult.OK)
            {
                treeView1.SelectedNode.ForeColor = colorDialog.Color;
                var item = treeView1.SelectedNode.Tag as MenuItem;
                item.Color = colorDialog.Color;
            }
        }
        #endregion

        private void resetMainMenuBtn_Click(object sender, EventArgs e)
        {
            KnownMenuNames.Clear();
            MainMenu.SubItems.Clear();

            AddMissingMenuOptions();
            RefreshMenuTree();
            ApplyMainMenu();
        }

        private void reloadMainMenuBtn_Click(object sender, EventArgs e)
        {
            LoadMainMenu();
            ApplyMainMenu();
        }

        void AddMissingMenuOptions()
        {
            foreach (var option in MainMenuOptions.Values)
            {
                if (!KnownMenuNames.Contains(option.Name))
                {
                    KnownMenuNames.Add(option.Name);

                    if (option.Default)
                    {
                        var folder = GetEntryFolder(option.DefaultPath);
                        folder.SubItems.Add(option);
                    }
                }
            }
        }

        private void ApplyMainMenu()
        {
            testMenu.Items.Clear();

            AddMenuItems(MainMenu.SubItems, testMenu.Items);

            testMenu.Items.Add("-");
            testMenu.Items.Add("Quit Cool Proxy").Click += quitCoolProxyToolStripMenuItem_Click;
        }

        private void LoadMainMenu()
        {
            MainMenu = new MenuFolder("MAIN_MENU", "Main Menu");

            string app_settings_dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "CoolProxy\\");
            string filename = Path.Combine(app_settings_dir, "main_menu.xml");

            if (File.Exists(filename))
            {
                byte[] xml = File.ReadAllBytes(filename);
                OSD osd = OSDParser.DeserializeLLSDXml(xml);
                OSDMap map = (OSDMap)osd;

                OSDArray known = (OSDArray)map["known"];
                OSDMap root = (OSDMap)map["root"];

                KnownMenuNames = known.Select(x => x.AsString()).ToList();
                MainMenu.SubItems = OSDToFolderContents((OSDArray)root["children"]);
            }

            AddMissingMenuOptions();

            RefreshMenuTree();
        }
        
        private void SaveMainMenu()
        {
            OSDMap menu = MenuItemToMap(MainMenu);

            OSDArray known = new OSDArray();
            foreach (var opt in KnownMenuNames)
            {
                known.Add(opt);
            }

            OSDMap map = new OSDMap();
            map["root"] = menu;
            map["known"] = known;

            string app_settings_dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "CoolProxy\\");
            string filename = Path.Combine(app_settings_dir, "main_menu.xml");

            byte[] xml = OSDParser.SerializeLLSDXmlToBytes((OSD)map);
            File.WriteAllBytes(filename, xml);
        }

        void RefreshMenuTree()
        {
            treeView1.Nodes.Clear();

            var a = treeView1.Nodes.Add(MainMenu.Name, "<" + MainMenu.Label + ">");
            a.Tag = MainMenu;
            a.ForeColor = MainMenu.Color;


            AddTreeNodeItems(MainMenu, a.Nodes);

            treeView1.ExpandAll();
        }

        private MenuFolder GetEntryFolder(string[] defaultPath)
        {
            if (defaultPath == null) return MainMenu;

            var top = MainMenu;

            foreach (var fol in defaultPath)
            {
                string id = "FOLDER_" + fol.Replace(" ", "_").ToUpper();

                var find = top.SubItems.Find(x => x.Name == id);
                if (find != null)
                {
                    top = find as MenuFolder;
                }
                else
                {
                    MenuFolder mf = new MenuFolder(id, fol);
                    top.SubItems.Add(mf);
                    top = mf;
                }
            }

            return top;
        }

        private List<MenuItem> OSDToFolderContents(OSDArray array)
        {
            List<MenuItem> items = new List<MenuItem>();

            foreach (OSDMap map in array)
            {
                string type = map["type"];

                if (type == "separator")
                {
                    items.Add(new MenuSeparator());
                    continue;
                }

                string name = map["name"];
                string label = map["label"];
                Color4 color4 = map["color"].AsColor4();

                Color color = Color.FromArgb((int)(255 * color4.R), (int)(255 * color4.G), (int)(255 * color4.B));

                if (type == "option")
                {
                    if (MainMenuOptions.TryGetValue(name, out MenuOption opt))
                    {
                        items.Add(opt);
                        opt.Color = color;
                    }
                }
                else if (type == "folder")
                {
                    MenuFolder folder = new MenuFolder(name, label);
                    folder.Color = color;
                    folder.SubItems = OSDToFolderContents((OSDArray)map["children"]);

                    items.Add(folder);
                }
            }

            return items;
        }

        private OSDMap MenuItemToMap(MenuItem item)
        {
            OSDMap map = new OSDMap();
            if (item is MenuSeparator)
            {
                map["type"] = "separator";
            }
            else
            {
                bool is_folder = item is MenuFolder;
                Color4 colour4 = new Color4(item.Color.R, item.Color.G, item.Color.B, item.Color.A);

                map["type"] = is_folder ? "folder" : "option";
                map["name"] = item.Name;
                map["label"] = item.Label;
                map["color"] = OSD.FromColor4(colour4);

                if (is_folder)
                {
                    var folder = item as MenuFolder;
                    OSDArray children = new OSDArray();
                    foreach (var child in folder.SubItems)
                    {
                        children.Add(MenuItemToMap(child));
                    }

                    map["children"] = children;
                }
            }

            return map;
        }

        private void AddTreeNodeItems(MenuFolder parent, TreeNodeCollection nodes)
        {
            foreach (var item in parent.SubItems)
            {
                if (item is MenuSeparator)
                {
                    var sep = nodes.Add("-- Separator --");
                    sep.Tag = item;

                }
                else if (item is MenuFolder)
                {
                    var fol = item as MenuFolder;

                    var add = nodes.Add(fol.Name, "<" + fol.Label + ">");
                    add.Tag = fol;
                    add.ForeColor = fol.Color;

                    AddTreeNodeItems(fol, add.Nodes);
                }
                else if (item is MenuItem)
                {
                    var add = nodes.Add(item.Label);
                    add.Tag = item;
                    add.ForeColor = item.Color;
                }
            }
        }

        private void testMenuButton_Click(object sender, EventArgs e)
        {
            ApplyMainMenu();
            testMenu.Show(Cursor.Position);
        }

        private void saveMenuChangesBtn_Click(object sender, EventArgs e)
        {
            SaveMainMenu();
        }

        private void testMenu_Opening(object sender, CancelEventArgs e)
        {
            foreach(ToolStripItem item in testMenu.Items)
            {
                if(item is ToolStripMenuItem)
                {
                    var tsmi = item as ToolStripMenuItem;

                    if (tsmi.Tag is MenuOption)
                    {
                        var opt = item.Tag as MenuOption;

                        tsmi.Checked = opt.Checked?.Invoke(opt.Tag) ?? false;
                        tsmi.Enabled = opt.Enabled?.Invoke(opt.Tag) ?? true;
                    }

                    RecursiveMenuOpening(tsmi.DropDownItems);
                }
            }
        }

        private void RecursiveMenuOpening(ToolStripItemCollection dropDownItems)
        {
            foreach (ToolStripItem ddi in dropDownItems)
            {
                if(ddi is ToolStripMenuItem)
                {
                    var item = ddi as ToolStripMenuItem;

                    if (item.Tag is MenuOption)
                    {
                        var opt = item.Tag as MenuOption;

                        item.Checked = opt.Checked?.Invoke(opt.Tag) ?? false;
                        item.Enabled = opt.Enabled?.Invoke(opt.Tag) ?? true;
                    }

                    RecursiveMenuOpening(item.DropDownItems);
                }
            }
        }
    }
}
