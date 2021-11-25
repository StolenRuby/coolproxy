using OpenMetaverse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoolProxy.Plugins.Editors
{
    public class EditorsPlugin : CoolProxyPlugin
    {
        internal SettingsManager Settings;
        internal GUIManager GUI;
        internal CoolProxyFrame Proxy;

        public EditorsPlugin(SettingsManager settings, GUIManager gui, CoolProxyFrame frame)
        {
            Settings = settings;
            GUI = gui;
            Proxy = frame;


            GUI.AddInventoryItemOption("Hex Edit", x => new HexEditor(x, this).Show(), x => x.AssetUUID != UUID.Zero);
            GUI.AddInventoryItemOption("Text Edit", x => new TextEditor(x, this).Show(), x => x.AssetUUID != UUID.Zero);
        }
    }
}
