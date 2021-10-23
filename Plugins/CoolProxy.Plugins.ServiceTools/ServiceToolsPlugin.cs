using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoolProxy.Plugins.ServiceTools
{
    public class ServiceToolsPlugin : CoolProxyPlugin
    {
        //private SettingsManager Settings;
        //private GUIManager GUI;
        //private CoolProxyFrame Proxy;

        public ServiceToolsPlugin(SettingsManager settings, GUIManager gui, CoolProxyFrame frame)
        {
            // todo: maybe pass the forms all 3?

            gui.AddToggleFormQuick("Assets", "Asset Service Upload", new AssetServiceTool(frame));
            gui.AddToggleFormQuick("Avatar", "Grid Instant Message Tool", new GridIMTool(frame));

            gui.AddInventoryItemOption("Edit Item...", (x) => new XInventoryServiceForm(frame, settings, x).Show());
            gui.AddInventoryFolderOption("Add Item...", (x) => new XInventoryServiceForm(frame, settings, x).Show());
        }
    }
}
