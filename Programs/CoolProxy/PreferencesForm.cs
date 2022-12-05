using Nwc.XmlRpc;
using OpenMetaverse;
using OpenMetaverse.StructuredData;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace CoolProxy
{
    public partial class PreferencesForm : Form, IGUI
    {
        Mutex AppMutex;
        private bool coolProxyIsQuitting = false;

        private GridListManager gridManager;

        public PreferencesForm()
        {
            AppMutex = new Mutex(true, "CoolProxyApp", out bool createdNew);

            if (!createdNew && !Program.Frame.Settings.getBool("AllowMultipleInstances"))
            {
                MessageBox.Show("Cool Proxy is already running!");
                this.Load += (x, y) => Close();
                return;
            }

            InitializeComponent();

            OpenMetaverse.Logger.Log("[GUI] CoolProxy GUI launched!", Helpers.LogLevel.Info);

            gridManager = new GridListManager();

            Program.Frame.RegisterModuleInterface<IGUI>(this);

            chatCmdTab.HorizontalScroll.Enabled = false;
            chatCmdTab.HorizontalScroll.Visible = false;
            chatCmdTab.HorizontalScroll.Maximum = 0;
            chatCmdTab.AutoScroll = true;

            // Settings
            LoadGrids();

            gridManager.OnGridAdded += OnGridAdded;
            gridsComboBox.SelectedItem = Program.Frame.Settings.getString("LastGridUsed");

            this.TopMost = Program.Frame.Settings.getBool("KeepCoolProxyOnTop");
            Program.Frame.Settings.getSetting("KeepCoolProxyOnTop").OnChanged += (x, y) => { this.TopMost = (bool)y.Value; };

            Program.Frame.Login.AddLoginResponseDelegate(handleLoginResponse);

            string cmd_prefix = Program.Frame.Settings.getString("ChatCommandPrefix");
            cmdPrefixCombo.SelectedItem = cmd_prefix;


            var version = Application.ProductVersion;
            var split = version.Split('.');
            label12.Text = "Version " + split[0] + "." + split[1];


            if(!Program.IsDebugMode)
            {
                loadPluginTestButton.Visible = false;
            }

            RegisterForm("preferences", this);
        }

        public void AddMainMenuOption(MenuOption option)
        {
            AddMenuOption(option);
        }

        public void AddSettingsTab(string label, Panel panel)
        {
            TabPage page = new TabPage();
            page.Text = label;
            page.Controls.Add(panel);
            miscPluginsTabControl.TabPages.Add(page);
        }

        #region Tabs

        ////////////////////////////////////////////////////////////////////////
        /// General
        ////////////////////////////////////////////////////////////////////////

        private void editGridsButton_Click(object sender, EventArgs e)
        {
            settingsTabControl.SelectedTab = gridsTab;
        }

        private void gridsComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            string gridname = (string)gridsComboBox.SelectedItem;
            gridManager.selectGrid(gridname);

            var info = gridManager.getInfoFromName(gridname);

            Program.Frame.Config.remoteLoginUri = new Uri(info.LoginURI);
            Program.Frame.Config.is_linden_grid = info.IsLindenGrid;
        }

        ////////////////////////////////////////////////////////////////////////
        /// Chat Commands
        ////////////////////////////////////////////////////////////////////////

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
            if (ChatCommandOnLeft && ChatCommandRowCount > 4)
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

            Controls.TextBox textBox = new Controls.TextBox();
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

        private void cmdPrefixCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            string prefix = cmdPrefixCombo.SelectedItem.ToString();
            Program.Frame.Settings.setString("ChatCommandPrefix", prefix);
        }

        ////////////////////////////////////////////////////////////////////////
        /// Grid Manager
        ////////////////////////////////////////////////////////////////////////

        private void OnGridAdded(GridInfo gridInfo)
        {
            if (this.InvokeRequired)
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

        private void addGridButton_Click(object sender, EventArgs e)
        {
            string url = addGridTextbox.Text + "/get_grid_info";

            if (!url.StartsWith("http://") && !url.StartsWith("https://"))
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
                    if (!gridManager.addGridFromDictionary(dict, out error))
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

        ////////////////////////////////////////////////////////////////////////
        /// Advanced
        ////////////////////////////////////////////////////////////////////////

        private void showDebugSettingsButton_Click(object sender, EventArgs e)
        {
            DebugSettingForm debug = new DebugSettingForm();
            debug.ShowDialog();
        }

        private void loadPluginTestButton_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Filter = "DLL Files|*.dll";
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    string filename = dialog.FileName;
                    LoadPlugin(filename);
                }
            }
        }

        private void clearCacheButton_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Clear asset cache now?", "", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                string dir = Program.Frame.Settings.getString("AssetCacheDir");
                foreach (var file in Directory.GetFiles(dir))
                {
                    File.Delete(file);
                }
                OpenMetaverse.Logger.Log("Asset Cache cleared!", Helpers.LogLevel.Info);
            }
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

            if (!grids_map.ContainsKey("cool.proxy"))
            {
                string address = Program.Frame.Settings.getString("GridProxyListenAddress");
                int port = Program.Frame.Settings.getInteger("GridProxyListenPort");

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


        #endregion

        #region Login

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

            Program.Frame.IsLindenGrid = Program.Frame.Settings.getBool("LindenGridSelected");

            string first_name = (string)responseData["first_name"];
            string last_name = (string)responseData["last_name"];

            first_name = first_name.Replace("\"", "");

            string full_name = first_name;
            if (last_name.ToLower() != "resident")
                full_name += " " + last_name;

            this.Invoke(new Action(() =>
            {
                //this.Text = "Cool Proxy - " + full_name;
                trayIcon.Text = "Cool Proxy - " + full_name;
            }));

            Program.Frame.Settings.setString("LastAccountUsed", full_name);
        }

        #endregion

        #region Plugins

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
                        Activator.CreateInstance(t, Program.Frame);
                        count++;
                    }
                    else if (t.IsSubclassOf(typeof(Command)))
                    {
                        Command command = (Command)Activator.CreateInstance(t, Program.Frame);
                        Program.Frame.AddChatCommand(command);
                        count++;
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

        private void LoadPlugins()
        {
            OSD osd = Program.Frame.Settings.getOSD("PluginList");
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

        #endregion

        #region Form Stuff

        private void CoolProxyForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if(!coolProxyIsQuitting)
            {
                e.Cancel = true;

                Form form = sender as Form;
                form.Hide();
            }
        }

        private void settingsTabControl_DrawItem(object sender, DrawItemEventArgs e)
        {
            Graphics g = e.Graphics;
            Brush _textBrush;

            // Get the item from the collection.
            TabPage _tabPage = settingsTabControl.TabPages[e.Index];

            // Get the real bounds for the tab rectangle.
            Rectangle _tabBounds = settingsTabControl.GetTabRect(e.Index);

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
            _stringFlags.Alignment = StringAlignment.Near;
            _stringFlags.LineAlignment = StringAlignment.Center;
            _tabBounds.X += 5;
            _tabBounds.Width -= 5;
            g.DrawString(_tabPage.Text, _tabFont, _textBrush, _tabBounds, new StringFormat(_stringFlags));
        }

        private void CoolProxyForm_Load(object sender, EventArgs e)
        {
            if (Program.Frame.Settings.getBool("FirstRun"))
            {
                OSDArray plugins = new OSDArray();

                plugins.Add("CoolProxy.Plugins.ToolBox.dll");
                plugins.Add("CoolProxy.Plugins.AvatarTracker.dll");
                plugins.Add("CoolProxy.Plugins.RegionTracker.dll");
                plugins.Add("CoolProxy.Plugins.InventoryBrowser.dll");

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
                plugins.Add("CoolProxy.Plugins.Messages.dll");

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
                plugins.Add("CoolProxy.Plugins.AssetLog.dll");

                Program.Frame.Settings.setOSD("PluginList", plugins);
            }

            LoadPlugins();

            foreach (var cmd in Program.Frame.Commands.Values)
            {
                ChatCommandAdded(cmd.CMD, cmd.Name, cmd.Description);
            }

            LoadMainMenu();
            SaveMainMenu();
            ApplyMainMenu();

            Program.Frame.Settings.setBool("FirstRun", false);
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

        private void CoolProxyForm_Shown(object sender, EventArgs e)
        {
            this.Hide();

            bool started = false;

            if (Program.Frame.Settings.getBool("StartProxyAtLaunch"))
            {
                try
                {
                    Program.Frame.Start();
                    started = true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }

            if (!started)
            {
                if (new ProxyAddressDialog().ShowDialog() != DialogResult.OK)
                {
                    Environment.Exit(0);
                    return;
                }
            }

            trayIcon.Visible = true;

            trayIcon.BalloonTipIcon = ToolTipIcon.None;
            trayIcon.BalloonTipTitle = string.Empty;
            trayIcon.BalloonTipText = string.Format("Proxy is running on http://{0}:{1}/", Program.Frame.Config.clientFacingAddress.ToString(), Program.Frame.Config.loginPort);
            trayIcon.ShowBalloonTip(2000);
        }

        #endregion

        #region Tray Stuff

        private void trayIcon_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (!this.Visible)
                {
                    this.Show();
                    this.WindowState = FormWindowState.Normal;
                    this.Activate();

                    Program.Frame.Settings.setBool("AlertStillRunning", false);
                }
                else
                {
                    this.WindowState = FormWindowState.Normal;
                    this.Activate();
                }
                //trayIcon.Visible = false;
            }
        }

        private void quitCoolProxyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Program.Frame.Started)
            {
                if (MessageBox.Show(this, "Are you sure you want to quit?", "Cool Proxy is still active", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    Program.Frame.Stop();
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

        #endregion

        private void githubLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://github.com/StolenRuby/coolproxy");
        }

        private void applyButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }


        Dictionary<string, Form> Forms = new Dictionary<string, Form>();

        public void RegisterForm(string id, Form form)
        {
            Forms.Add(id, form);

            form.Activated += HandleFormActivated;
            form.Deactivate += HandleFormDeactivated;

            form.FormClosing += CoolProxyForm_FormClosing;

            form.TopMost = Program.Frame.Settings.getBool("KeepCoolProxyOnTop");
            Program.Frame.Settings.getSetting("KeepCoolProxyOnTop").OnChanged += (x, y) => { form.TopMost = (bool)y.Value; };
        }
    }
}
