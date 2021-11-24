using CoolProxy.Plugins.NotecardMagic;
using OpenMetaverse;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CoolProxy.Plugins.KeyTool
{
    public enum OpenAssetMode
    {
        Nothing,
        SuperSuitcase,
        NotecardMagic,
        LocalInventory
    }

    public class KeyToolPlugin : CoolProxyPlugin
    {
        public static SettingsManager Settings { get; private set; }
        public static CoolProxyFrame Proxy { get; private set; }

        internal static INotecardMagic NotecardMagic = null;

        internal static OpenAssetMode Mode = OpenAssetMode.Nothing;

        public KeyToolPlugin(SettingsManager settings, GUIManager gui, CoolProxyFrame frame)
        {
            Settings = settings;
            Proxy = frame;

            gui.AddToolButton("UUID", "KeyTool from Clipboard", handleKeyToolButton);
            gui.AddTrayOption("KeyTool from Clipboard", handleKeyToolButton);

            Mode = (OpenAssetMode)settings.getInteger("KeyToolOpenAssetMode");

            AddSettingsTab(gui);

            NotecardMagic = Proxy.RequestModuleInterface<INotecardMagic>();
        }

        private void AddSettingsTab(GUIManager gui)
        {
            Panel panel = new Panel();
            panel.Dock = DockStyle.Fill;

            var checkbox = new CoolGUI.Controls.CheckBox();
            checkbox.AutoSize = true;
            checkbox.Location = new Point(14, 12);
            checkbox.Setting = "AutomaticallyOpenKeyTool";
            checkbox.Text = "Automatically Open Key for KeyTool";

            panel.Controls.Add(checkbox);

            checkbox = new CoolGUI.Controls.CheckBox();
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

            combo.Items.Add("Nothing");
            combo.Items.Add("Super Suitcase Add");
            combo.Items.Add("Notecard Magic");
            combo.Items.Add("Local Inventory Inject");

            combo.SelectedIndex = (int)Mode;

            combo.SelectedIndexChanged += (x, y) =>
            {
                Mode = (OpenAssetMode)combo.SelectedIndex;
                Settings.setInteger("KeyToolOpenAssetMode", (int)Mode);
            };

            panel.Controls.Add(combo);

            gui.AddSettingsTab("KeyTool", panel);
        }

        private void handleKeyToolButton(object sender, EventArgs e)
        {
            UUID key = UUID.Zero;
            if (UUID.TryParse(Clipboard.GetText(), out key))
            {
                Proxy.SayToUser("KeyTool", "Running KeyTool on " + key.ToString());

                Thread keytoolGUIThread = null;
                keytoolGUIThread = new Thread(new ThreadStart(() =>
                {
                    var keytoolGUI = new KeyToolForm(Proxy, key);
                    //keytoolGUI.FormClosed += (x, y) => { keytoolGUIThread = null; keytoolGUI = null; };
                    //Application.Run(keytoolGUI);
                    keytoolGUI.ShowDialog();
                    keytoolGUIThread = null;
                    keytoolGUI = null;
                }));
                keytoolGUIThread.SetApartmentState(ApartmentState.STA);
                keytoolGUIThread.Start();
            }
            else Proxy.SayToUser("KeyTool", "Invalid UUID");
        }
    }
}
