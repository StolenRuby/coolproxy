using OpenMetaverse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoolProxy.Plugins.InventoryBrowser
{
    public class InventoryBrowserPlugin : CoolProxyPlugin, IInventoryBrowser
    {
        private InventoryBrowserForm InventoryBrowser;

        public InventoryBrowserPlugin(CoolProxyFrame frame)
        {
            frame.RegisterModuleInterface<IInventoryBrowser>(this);

            InventoryBrowser = new InventoryBrowserForm(frame);

            IGUI gui = frame.RequestModuleInterface<IGUI>();
            gui.AddMainMenuOption(new MenuOption("TOGGLE_INVENTORY_BROWSER", "Inventory", true)
            {
                Clicked = (x) =>
                {
                    if (InventoryBrowser.Visible)
                        InventoryBrowser.Hide();
                    else
                        InventoryBrowser.Show();
                },
                Checked = (x) => InventoryBrowser.Visible
            });
        }

        public void AddInventoryFolderOption(string label, HandleInventoryFolder handle, EnableInventoryFolder enable = null)
        {
            InventoryBrowser.AddInventoryFolderOption(label, handle, enable);
        }

        public void AddInventoryItemOption(string label, HandleInventory handle, AssetType assetType, EnableInventory enable = null)
        {
            InventoryBrowser.AddInventoryItemOption(label, handle, assetType, enable);
        }

        public void AddInventoryItemOption(string label, HandleInventory handle, InventoryType invType, EnableInventory enable = null)
        {
            InventoryBrowser.AddInventoryItemOption(label, handle, invType, enable);
        }

        public void AddInventoryItemOption(string label, HandleInventory handle, EnableInventory enable = null)
        {
            InventoryBrowser.AddInventoryItemOption(label, handle, enable);
        }
    }

    public interface IInventoryBrowser
    {
        void AddInventoryItemOption(string label, HandleInventory handle, AssetType assetType, EnableInventory enable = null);
        void AddInventoryItemOption(string label, HandleInventory handle, InventoryType invType, EnableInventory enable = null);
        void AddInventoryItemOption(string label, HandleInventory handle, EnableInventory enable = null);
        void AddInventoryFolderOption(string label, HandleInventoryFolder handle, EnableInventoryFolder enable = null);
    };
}
