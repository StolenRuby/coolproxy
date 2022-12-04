using CoolProxy.Plugins.NotecardMagic;
using CoolProxy.Plugins.OpenSim;
using CoolProxy.Plugins.ToolBox;
using GridProxy;
using OpenMetaverse;
using OpenMetaverse.Packets;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CoolProxy.Plugins.KeyTool
{
    public enum OpenAssetMode
    {
        Nothing,
        LocalInventory,
        NotecardMagic,
        SuperSuitcase
    }

    public class KeyToolPlugin : CoolProxyPlugin
    {
        public static CoolProxyFrame Proxy { get; private set; }
        public static IROBUST ROBUST { get; private set; }

        internal static INotecardMagic NotecardMagic = null;

        internal static OpenAssetMode Mode = OpenAssetMode.Nothing;

        public KeyToolPlugin(CoolProxyFrame frame)
        {
            Proxy = frame;

            IGUI gui = frame.RequestModuleInterface<IGUI>();

            IToolBox toolbox = frame.RequestModuleInterface<IToolBox>();

            SimpleButton simpleButton = new SimpleButton("Keytool from Clipboard", (x, y) => handleKeyToolMenuOption(null))
            {
                ID = "KEYTOOL_CLIPBOARD_TOOL",
                Default = false
            };

            toolbox.AddTool(simpleButton);
            //gui.AddToolButton("UUID", "KeyTool from Clipboard", (x, y) => handleKeyToolMenuOption(null));


            gui.AddMainMenuOption(new MenuOption("KEYTOOL_FROM_CLIPBOARD", "Keytool from Clipboard", true, "Tools")
            {
                Clicked = handleKeyToolMenuOption
            });


            NotecardMagic = Proxy.RequestModuleInterface<INotecardMagic>();
            ROBUST = Proxy.RequestModuleInterface<IROBUST>();

            if(Proxy.Settings.getBool("FirstRun"))
            {
                if (ROBUST != null) Mode = OpenAssetMode.SuperSuitcase;
                else if (NotecardMagic != null) Mode = OpenAssetMode.NotecardMagic;
                else Mode = OpenAssetMode.LocalInventory;

                Proxy.Settings.setInteger("KeyToolOpenAssetMode", (int)Mode);
            }
            else Mode = (OpenAssetMode)Proxy.Settings.getInteger("KeyToolOpenAssetMode");

            if (Mode == OpenAssetMode.SuperSuitcase && ROBUST == null) Mode = OpenAssetMode.NotecardMagic;
            if (Mode == OpenAssetMode.NotecardMagic && NotecardMagic == null) Mode = OpenAssetMode.LocalInventory;

            AddSettingsTab(gui);

            Proxy.Network.AddDelegate(PacketType.ChatFromViewer, Direction.Outgoing, HandleChatFromViewer);
            Proxy.Network.AddDelegate(PacketType.ChatFromSimulator, Direction.Incoming, HandleChatFromSimulator);
        }

        Dictionary<string, OpenAssetMode> ModeNames = new Dictionary<string, OpenAssetMode>()
        {
            {"Nothing", OpenAssetMode.Nothing },
            {"Notecard Magic", OpenAssetMode.NotecardMagic },
            {"SuperSuitcase Add", OpenAssetMode.SuperSuitcase },
            {"Local Inventory Inject", OpenAssetMode.LocalInventory }
        };

        private void AddSettingsTab(IGUI gui)
        {
            Panel panel = new Panel();
            panel.Dock = DockStyle.Fill;

            var checkbox = new CoolProxy.Controls.CheckBox();
            checkbox.AutoSize = true;
            checkbox.Location = new Point(14, 12);
            checkbox.Setting = "AutomaticallyOpenKeyTool";
            checkbox.Text = "Automatically Open Key for KeyTool";

            panel.Controls.Add(checkbox);

            checkbox = new CoolProxy.Controls.CheckBox();
            checkbox.AutoSize = true;
            checkbox.Location = new Point(36, 35);
            checkbox.EnabledSetting = "AutomaticallyOpenKeyTool";
            checkbox.Setting = "AutomaticallyCloseKeyTool";
            checkbox.Text = "Automatically close KeyTool";

            panel.Controls.Add(checkbox);

            var label = new Label();
            label.AutoSize = true;
            label.Location = new Point(240, 12);
            label.Text = "Open Asset Action:";

            panel.Controls.Add(label);

            var combo = new ComboBox();
            combo.Size = new Size(170, 22);
            combo.Location = new Point(240, 30);
            combo.DropDownStyle = ComboBoxStyle.DropDownList;

            var first = ModeNames.First(x => x.Value == Mode);

            combo.Items.Add("Nothing");
            combo.Items.Add("Local Inventory Inject");
            if (NotecardMagic != null) combo.Items.Add("Notecard Magic");
            if (ROBUST != null) combo.Items.Add("SuperSuitcase Add");

            combo.SelectedItem = first.Key;

            combo.SelectedValueChanged += (x, y) =>
            {
                Mode = ModeNames[(string)combo.SelectedItem];
                Proxy.Settings.setInteger("KeyToolOpenAssetMode", (int)Mode);
            };

            panel.Controls.Add(combo);

            checkbox = new CoolProxy.Controls.CheckBox();
            checkbox.AutoSize = true;
            checkbox.Location = new Point(14, 70);
            checkbox.Setting = "MakeKeysInChatSLURLs";
            checkbox.Text = "Make Keys in chat SLURLs";

            panel.Controls.Add(checkbox);

            gui.AddSettingsTab("KeyTool", panel);
        }

        private void handleKeyToolMenuOption(object user_data)
        {
            if (UUID.TryParse(Clipboard.GetText(), out UUID key))
            {
                RunKeyTool(key);
            }
            else Proxy.SayToUser("KeyTool", "Invalid UUID");
        }

        private void RunKeyTool(UUID key)
        {
            Proxy.SayToUser("KeyTool", "Running KeyTool on " + key.ToString());

            Thread keytoolGUIThread = null;
            keytoolGUIThread = new Thread(new ThreadStart(() =>
            {
                var keytoolGUI = new KeyToolForm(Proxy, key);
                keytoolGUI.ShowDialog();
                keytoolGUIThread = null;
                keytoolGUI = null;
            }));
            keytoolGUIThread.SetApartmentState(ApartmentState.STA);
            keytoolGUIThread.Start();
        }

        private Packet HandleChatFromSimulator(Packet packet, RegionManager.RegionProxy sim)
        {
            if (!Proxy.Settings.getBool("MakeKeysInChatSLURLs")) return packet;

            ChatFromSimulatorPacket chat = (ChatFromSimulatorPacket)packet;

            string message = Utils.BytesToString(chat.ChatData.Message);

            var matches = Regex.Matches(message, "[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}");

            if (matches.Count > 0)
            {
                string new_string = string.Empty;
                int last_index = 0;

                foreach (Match match in matches)
                {
                    string sub_str = message.Substring(last_index, match.Index - last_index);

                    new_string += sub_str + "[secondlife:///app/chat/1337/" + match.Value + " " + match.Value + "]";

                    last_index = match.Index + 36;
                }

                if (last_index != message.Length)
                {
                    new_string += message.Substring(last_index);
                }

                chat.ChatData.Message = Utils.StringToBytes(new_string);

                return chat;
            }

            return packet;
        }

        private Packet HandleChatFromViewer(Packet packet, RegionManager.RegionProxy sim)
        {
            ChatFromViewerPacket chat = (ChatFromViewerPacket)packet;

            if (chat.ChatData.Channel == 1337)
            {
                string message = Utils.BytesToString(chat.ChatData.Message);

                if (UUID.TryParse(message, out UUID key))
                {
                    RunKeyTool(key);
                    return null;
                }
            }

            return packet;
        }
    }
}
