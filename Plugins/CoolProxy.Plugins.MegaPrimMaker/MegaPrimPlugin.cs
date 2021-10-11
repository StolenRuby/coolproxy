using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoolProxy.Plugin.MegaPrimMaker
{
    public class MegaPrimPlugin : CoolProxyPlugin
    {
        private SettingsManager Settings;
        private GUIManager GUI;
        private CoolProxyFrame Proxy;

        public MegaPrimPlugin(SettingsManager settings, GUIManager gui, CoolProxyFrame frame)
        {
            Settings = settings;
            GUI = gui;
            Proxy = frame;

            GUI.AddToggleFormQuick("Objects", "New MegaPrim", new Plugins.MegaPrimMaker.NewMegaprimForm(Proxy));
        }
    }
}
