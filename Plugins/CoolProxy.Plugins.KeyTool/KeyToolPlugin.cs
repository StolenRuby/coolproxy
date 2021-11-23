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
    public class KeyToolPlugin : CoolProxyPlugin
    {
        public static SettingsManager Settings { get; private set; }
        public static CoolProxyFrame Proxy { get; private set; }

        internal static INotecardMagic NotecardMagic = null;

        public KeyToolPlugin(SettingsManager settings, GUIManager gui, CoolProxyFrame frame)
        {
            Settings = settings;
            Proxy = frame;

            gui.AddToolButton("UUID", "KeyTool from Clipboard", handleKeyToolButton);
            gui.AddTrayOption("KeyTool from Clipboard", handleKeyToolButton);

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
