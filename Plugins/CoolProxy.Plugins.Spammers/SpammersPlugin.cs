using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoolProxy.Plugins.Spammers
{
    public class SpammersPlugin : CoolProxyPlugin
    {
        private SettingsManager Settings;
        private GUIManager GUI;
        private CoolProxyFrame Proxy;

        public SpammersPlugin(SettingsManager settings, GUIManager gui, CoolProxyFrame frame)
        {
            Settings = settings;
            GUI = gui;
            Proxy = frame;


            GUI.AddToggleFormQuick("Avatar", "Instant Message Spammer", new IMSpamForm(Proxy));
            GUI.AddToggleFormQuick("Objects", "Touch Spammer", new TouchSpammerForm(Proxy));
        }
    }
}
