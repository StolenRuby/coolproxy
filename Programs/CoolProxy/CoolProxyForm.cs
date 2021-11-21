using GridProxy;
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



        public CoolProxyForm()
        {

            bool createdNew;
            var appMutex = new Mutex(true, "CoolProxyApp", out createdNew);

            if (!createdNew && !CoolProxy.Settings.getBool("AllowMultipleInstances"))
            {
                MessageBox.Show("Cool Proxy is already running!");
                this.Load += (x, y) => Close();
                return;
            }

            // log4net
            if (FireEventAppender.Instance != null)
            {
                FireEventAppender.Instance.MessageLoggedEvent += (x, y) => {

                    string s = String.Format("{0} [{1}] {2} {3}", y.LoggingEvent.TimeStamp, y.LoggingEvent.Level,
                        y.LoggingEvent.RenderedMessage, y.LoggingEvent.ExceptionObject);
                    Console.WriteLine(s);
                };
            }


            inventoryBrowserForm = new InventoryBrowserForm();

            guiManager = new GUIManager(this);
            gridManager = new GridListManager();

            InitializeComponent();

            // Regions
            CoolProxy.Frame.Network.OnNewRegion += onNewRegion;
            CoolProxy.Frame.Network.OnHandshake += onHandshake;

            // Asset Transfers
            CoolProxy.Frame.Assets.DownloadProgress += Assets_DownloadProgress;

            uploadsGridView.Rows.Add(Properties.Resources.Inv_Mesh, "C:\\assets\\somemesh.slmesh", "Queued");
            uploadsGridView.Rows.Add(Properties.Resources.Inv_Script, "C:\\assets\\cool script.lsl", "Queued");

            comboBox2.DataSource = Enum.GetValues(typeof(AssetType));
            comboBox2.SelectedItem = AssetType.Unknown;

            blacklistDataGridView.Rows.Add(UUID.Random(), DateTime.UtcNow);
            blacklistDataGridView.Rows.Add(UUID.Random(), DateTime.UtcNow);
            blacklistDataGridView.Rows.Add(UUID.Random(), DateTime.UtcNow);
            blacklistDataGridView.Rows.Add(UUID.Random(), DateTime.UtcNow);

            downloadsGridView.ShowCellToolTips = false;
            downloadsGridView.CellToolTipTextNeeded += DownloadsGridView_CellToolTipTextNeeded;


            // Asset Logging
            soundsDataGridView.DoubleBuffered(true);
            animsDataGridView.DoubleBuffered(true);

            CoolProxy.Frame.Network.AddDelegate(PacketType.SoundTrigger, Direction.Incoming, onSoundTrigger);
            CoolProxy.Frame.Network.AddDelegate(PacketType.AttachedSound, Direction.Incoming, onAttachedSound);
            CoolProxy.Frame.Network.AddDelegate(PacketType.PreloadSound, Direction.Incoming, onPreloadSound);

            CoolProxy.Frame.Avatars.AvatarAnimation += Avatars_AvatarAnimation;

            CoolProxy.Frame.Objects.ObjectUpdate += Objects_ObjectUpdate;


            // Settings
            LoadGrids();

            gridManager.OnGridAdded += OnGridAdded;
            gridsComboBox.SelectedItem = CoolProxy.Settings.getString("LastGridUsed");

            this.TopMost = CoolProxy.Settings.getBool("KeepCoolProxyOnTop");
            CoolProxy.Settings.getSetting("KeepCoolProxyOnTop").OnChanged += (x, y) => { this.TopMost = (bool)y.Value; };

            avatarTracker = new AvTrackerTest(avatarTrackerGridView, CoolProxy.Frame);

            flowLayoutPanel1.DoubleBuffered(true);

            CoolProxy.Frame.OnNewChatCommand += ChatCommandAdded;

            // Login masking
            CoolProxy.Frame.Login.AddLoginRequestDelegate(handleLoginRequest);
            CoolProxy.Frame.Login.AddLoginResponseDelegate(handleLoginResponse);

            firstTimeMinimized = CoolProxy.Settings.getBool("AlertStillRunning");

            AddTrayOption("Avatar Tracker", tabNameToolStripMenuItem_Click, null, "Avatar Tracker");
            AddTrayOption("Regions", tabNameToolStripMenuItem_Click, null, "Regions");
            AddTrayOption("ToolBox", tabNameToolStripMenuItem_Click, null, "ToolBox");
            AddTrayOption("Asset Logging", tabNameToolStripMenuItem_Click, null, "Asset Logging");
            AddTrayOption("Asset Transfers", tabNameToolStripMenuItem_Click, null, "Asset Transfers");
            AddTrayOption("-", null, null, "");
            AddTrayOption("Inventory Browser", inventoryBrowserToolStripMenuItem_Click, null, "");
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

        private void Objects_ObjectUpdate(object sender, GridProxy.PrimEventArgs e)
        {
            if (e.Prim.Sound != UUID.Zero)
                LogSound("Loop", e.Prim.Sound, e.Prim.OwnerID);
        }

        List<UUID> loggedSounds = new List<UUID>();

        private void LogSound(string type, UUID id, UUID owner)
        {
            if (soundsDataGridView.InvokeRequired) soundsDataGridView.BeginInvoke(new Action(() => LogSound(type, id, owner)));
            else
            {
                lock(loggedSounds)
                {
                    if(!loggedSounds.Contains(id))
                    {
                        soundsDataGridView.Rows.Add(type, id.ToString(), DateTime.Now.ToString());
                        loggedSounds.Add(id);
                    }
                }
            }
        }

        private Packet onSoundTrigger(Packet packet, RegionManager.RegionProxy sim)
        {
            SoundTriggerPacket soundTriggerPacket = (SoundTriggerPacket)packet;
            LogSound("Trigger", soundTriggerPacket.SoundData.SoundID, soundTriggerPacket.SoundData.OwnerID);
            return packet;
        }

        private Packet onAttachedSound(Packet packet, RegionManager.RegionProxy sim)
        {
            AttachedSoundPacket attachedSoundPacket = (AttachedSoundPacket)packet;
            LogSound("Attached", attachedSoundPacket.DataBlock.SoundID, attachedSoundPacket.DataBlock.OwnerID);
            return packet;
        }

        private Packet onPreloadSound(Packet packet, RegionManager.RegionProxy sim)
        {
            PreloadSoundPacket preloadSoundPacket = (PreloadSoundPacket)packet;

            foreach(var block in preloadSoundPacket.DataBlock)
            {
                LogSound("Preload", block.SoundID, block.OwnerID);
            }

            return packet;
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


        internal void AddInventoryItemOption(string label, CoolGUI.Controls.HandleInventory handle)
        {
            inventoryBrowserForm.AddInventoryItemOption(label, handle);
        }

        internal void AddInventoryFolderOption(string label, CoolGUI.Controls.HandleInventoryFolder handle)
        {
            inventoryBrowserForm.AddInventoryFolderOption(label, handle);
        }

        internal void AddInventoryItemOption(string label, CoolGUI.Controls.HandleInventory handle, AssetType assetType)
        {
            inventoryBrowserForm.AddInventoryItemOption(label, handle, assetType);
        }

        internal void AddInventoryItemOption(string label, CoolGUI.Controls.HandleInventory handle, InventoryType invType)
        {
            inventoryBrowserForm.AddInventoryItemOption(label, handle, invType);
        }

        public class TrayOption
        {
            public string Label { get; set; }
            public EventHandler Option { get; set; }
            public TrayIconEnable Enable { get; set; }
            public TrayIconEnable Checked { get; set; }
            public object Tag { get; set; }

            public TrayOption(string label, EventHandler option, TrayIconEnable enabled, TrayIconEnable check, object tag)
            {
                Label = label;
                Option = option;
                Enable = enabled;
                Checked = check;
                Tag = tag;
            }
        }

        List<TrayOption> TrayOptions = new List<TrayOption>();

        internal void AddTrayOption(string label, EventHandler option, TrayIconEnable opening = null, object tag = null)
        {
            TrayOptions.Add(new TrayOption(label, option, opening, null, tag));
        }

        internal void AddTrayCheck(string label, EventHandler option, TrayIconEnable check, object tag = null)
        {
            TrayOptions.Add(new TrayOption(label, option, null, check, tag));
        }

        private void trayContextMenu_Opening(object sender, CancelEventArgs e)
        {
            trayContextMenu.Items.Clear();
            foreach (var option in TrayOptions)
            {
                var item = trayContextMenu.Items.Add(option.Label);
                if(item is ToolStripMenuItem)
                {
                    ToolStripMenuItem menu_item = (ToolStripMenuItem)item;

                    menu_item.Tag = option.Tag;

                    if (option.Enable != null)
                        menu_item.Enabled = option.Enable();

                    if (option.Checked != null)
                        menu_item.Checked = option.Checked();

                    menu_item.Click += option.Option;
                }
            }
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
            OSD osd = CoolProxy.Settings.getOSD("PluginList");
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

        private void handleLoginRequest(object sender, XmlRpcRequestEventArgs e)
        {
            Hashtable requestData;
            try
            {
                requestData = (Hashtable)e.m_Request.Params[0];
            }
            catch (Exception ex)
            {
                OpenMetaverse.Logger.Log(ex.Message, Helpers.LogLevel.Error);
                return;
            }

            bool spoof_mac = CoolProxy.Settings.getBool("SpoofMac");
            bool spoof_id0 = CoolProxy.Settings.getBool("SpoofId0");

            if (requestData.ContainsKey("mac") && spoof_mac)
            {
                requestData["mac"] = CoolProxy.Settings.getSetting("SpecifiedMacAddress");
            }

            if (requestData.ContainsKey("id0") && spoof_id0)
            {
                requestData["id0"] = CoolProxy.Settings.getSetting("SpecifiedId0Address");
            }
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

            string first_name = (string)responseData["first_name"];
            string last_name = (string)responseData["last_name"];

            string full_name = first_name;
            if (last_name.ToLower() != "resident")
                full_name += " " + last_name;

            this.Invoke(new Action(() =>
            {
                this.Text = "Cool Proxy - " + full_name;
                trayIcon.Text = this.Text;
            }));

            CoolProxy.Settings.setString("LastAccountUsed", full_name);
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
            if (this.WindowState == FormWindowState.Minimized && CoolProxy.Settings.getBool("MinimizeCoolProxyToTray"))
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

                    CoolProxy.Settings.setBool("AlertStillRunning", false);
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
                        Activator.CreateInstance(t, CoolProxy.Settings, guiManager, CoolProxy.Frame);
                        count++;

                        //ConstructorInfo info = t.GetConstructor(new Type[] { typeof(SettingsManager), typeof(GUIManager), typeof(CoolProxyFrame) });
                        //if(info != null)
                        //{

                        //    CoolProxyPlugin plugin = (CoolProxyPlugin)info.Invoke(new object[] { CoolProxy.Settings, guiManager, CoolProxy.Frame });
                        //}
                    }
                    else if (t.IsSubclassOf(typeof(Command)))
                    {
                        Command command = (Command)Activator.CreateInstance(t, CoolProxy.Settings, CoolProxy.Frame);
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

        private void forgeToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            UUID folder_id = CoolProxy.Frame.Inventory.SuitcaseID != UUID.Zero ?
                CoolProxy.Frame.Inventory.FindSuitcaseFolderForType(FolderType.Sound) :
                CoolProxy.Frame.Inventory.FindFolderForType(FolderType.Sound);

            foreach (DataGridViewRow row in soundsDataGridView.SelectedRows)
            {
                UUID sound_id = UUID.Parse((string)row.Cells[1].Value);
                UUID item_id = UUID.Random();

                CoolProxy.Frame.OpenSim.XInventory.AddItem(folder_id, item_id, sound_id, AssetType.Sound, InventoryType.Sound, 0, sound_id.ToString(), "", DateTime.UtcNow, (item_succes) =>
                {
                    if (item_succes)
                    {
                        CoolProxy.Frame.Inventory.RequestFetchInventory(item_id, CoolProxy.Frame.Agent.AgentID, false);
                    }
                    else CoolProxy.Frame.SayToUser("Failed to forge!");
                });
            }
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


            form.TopMost = CoolProxy.Settings.getBool("KeepCoolProxyOnTop");
            CoolProxy.Settings.getSetting("KeepCoolProxyOnTop").OnChanged += (x, y) => { form.TopMost = (bool)y.Value; };
        }

        private void CoolProxyForm_Load(object sender, EventArgs e)
        {
            inventoryBrowserForm.InitOptions();

            //AddToggleFormQuick("Inventory", "Inventory Browser", inventoryBrowserForm);

            LoadPlugins();

            for(int i = 0; i < tabControl1.TabPages.Count; i++)
            {
                TabPage t = tabControl1.TabPages[i];

                string tab_name = string.Join("", t.Text.Split(' '));

                string popped_setting = "Tab" + tab_name + "Popped";
                string open_setting = "Tab" + tab_name + "Open";

                if (CoolProxy.Settings.getBool(popped_setting))
                {
                    PopTab(t, !CoolProxy.Settings.getBool(open_setting));
                }
            }

            if (TrayOptions.Last().Label != "-")
                AddTrayOption("-", null, null, "");

            AddTrayOption("Quit CoolProxy", quitCoolProxyToolStripMenuItem_Click, null, "");
        }

        Dictionary<string, Size> formSizeList = new Dictionary<string, Size>()
        {
            { "FormLoginSize", new Size(475, 305) },
            { "FormAvatarTrackerSize", new Size(339, 164) },
            { "FormRegionsSize", new Size(400, 173) },
            { "FormToolBoxSize", new Size(251, 521) },
            { "FormAssetLoggingSize", new Size(462, 510) },
            { "FormAssetTransfersSize", new Size(400, 280) },
            { "FormPacketCreatorSize", new Size(400, 280) },
            { "FormSettingsSize", new Size(477, 305) }
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
            testForm.TopMost = CoolProxy.Settings.getBool("KeepCoolProxyOnTop");
            testForm.ShowIcon = false;
            CoolProxy.Settings.getSetting("KeepCoolProxyOnTop").OnChanged += (x, y) => { testForm.TopMost = (bool)y.Value; };

            openTabs.Add(page.Text, testForm);

            string tab_name = string.Join("", page.Text.Split(' '));

            string size_setting = "Form" + tab_name + "Size";
            string popped_setting = "Tab" + tab_name + "Popped";
            string open_setting = "Tab" + tab_name + "Open";


            CoolProxy.Settings.setBool(popped_setting, true);

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
                    testForm.Activate();
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

                CoolProxy.Settings.setBool(popped_setting, false);
            };


            testForm.FormClosing += (x, y) =>
            {
                CoolProxy.Settings.setBool(open_setting, false);
                y.Cancel = true;
                testForm.Hide();
            };

            testForm.VisibleChanged += (x, y) =>
            {
                if(testForm.Visible)
                {
                    CoolProxy.Settings.setBool(open_setting, true);
                }
            };

            page.Controls.Add(bring_back);
            page.Controls.Add(show);

            if (!hide)
                testForm.Show();
        }

        private void tabNameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem item = sender as ToolStripMenuItem;

            if (openTabs.TryGetValue((string)item.Tag, out Form form))
            {
                if (form.Visible)
                {
                    form.WindowState = FormWindowState.Normal;
                    form.Activate();
                }
                else form.Show();
            }
            else
            {
                this.Show();
                this.WindowState = FormWindowState.Normal;
                this.Focus();

                TabPage page = tabControl1.TabPages.Cast<TabPage>().First(x => x.Text == (string)item.Tag);

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
        }

        private void button4_Click(object sender, EventArgs e)
        {
            new LoginMaskingForm().ShowDialog();
        }

        private void clearCacheButton_Click(object sender, EventArgs e)
        {
            if(MessageBox.Show("Clear asset cache now?", "", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                string dir = CoolProxy.Frame.Config.ASSET_CACHE_DIR;
                foreach (var file in Directory.GetFiles(dir))
                {
                    File.Delete(file);
                }
                OpenMetaverse.Logger.Log("Asset Cache cleared!", Helpers.LogLevel.Info);
            }
        }

        private void inventoryBrowserToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!inventoryBrowserForm.Visible)
                inventoryBrowserForm.Show();
            else
            {
                inventoryBrowserForm.WindowState = FormWindowState.Normal;
                inventoryBrowserForm.Activate();
            }
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

            CoolProxy.Settings.setOSD("PluginList", osd_array);

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

            form.TopMost = CoolProxy.Settings.getBool("KeepCoolProxyOnTop");
            CoolProxy.Settings.getSetting("KeepCoolProxyOnTop").OnChanged += (x, y) => { form.TopMost = (bool)y.Value; };

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
                OSDMap grid = new OSDMap();
                grid["label"] = "Cool Proxy";
                grid["keyname"] = "cool.proxy";
                grid["system_grid"] = false;

                OSDArray identifiers = new OSDArray();
                identifiers.Add("agent");
                identifiers.Add("account");
                grid["login_identifier_types"] = identifiers;

                grid["login_page"] = string.Format("http://{0}:{1}/splash", CoolProxy.Frame.Config.clientFacingAddress, CoolProxy.Frame.Config.loginPort);
                grid["login_uri"] = string.Format("http://{0}:{1}/login", CoolProxy.Frame.Config.clientFacingAddress, CoolProxy.Frame.Config.loginPort);
                grid["slurl_base"] = string.Format("http://{0}:{1}/slurl", CoolProxy.Frame.Config.clientFacingAddress, CoolProxy.Frame.Config.loginPort);
                grid["web_profile_url"] = string.Format("http://{0}:{1}/profile", CoolProxy.Frame.Config.clientFacingAddress, CoolProxy.Frame.Config.loginPort);

                grids_map["cool.proxy"] = grid;

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
            if (CoolProxy.Settings.getBool("StartProxyAtLaunch"))
            {
                ToggleProxy();

                if (CoolProxy.Settings.getBool("HideProxyAtLaunch"))
                {
                    this.WindowState = FormWindowState.Minimized;
                }
            }
        }
    }
}
