using OpenMetaverse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoolProxy.Plugins.ServiceTools
{
    public class ServiceToolsPlugin : CoolProxyPlugin
    {
        private CoolProxyFrame Proxy;

        public ServiceToolsPlugin(CoolProxyFrame frame)
        {
            Proxy = frame;

            IGUI gui = frame.RequestModuleInterface<IGUI>();

            gui.AddToggleFormQuick("Assets", "Asset Service Upload", new AssetServiceTool(frame));
            gui.AddToggleFormQuick("Avatar", "Grid Instant Message Tool", new GridIMTool(frame));

            gui.AddInventoryItemOption("Edit Item...", (x) => new XInventoryServiceForm(frame, x).Show(), IsItemWithinSuitcase);
            gui.AddInventoryFolderOption("Add Item...", (x) => new XInventoryServiceForm(frame, x).Show(), IsWithinSuitcase);
        }

        private bool IsWithinSuitcase(InventoryFolder folder)
        {
            if (folder.UUID == Proxy.Inventory.SuitcaseID)
            {
                return true;
            }

            UUID parent_id = folder.ParentUUID;

            while (parent_id != UUID.Zero)
            {
                if (parent_id == Proxy.Inventory.SuitcaseID) return true;

                var parent = Proxy.Inventory.Store[parent_id] ?? null;
                if (parent != null)
                {
                    parent_id = parent.ParentUUID;
                }
            }

            return false;
        }

        private bool IsItemWithinSuitcase(InventoryItem item)
        {
            if (item.ParentUUID == Proxy.Inventory.SuitcaseID) return true;

            InventoryFolder parent = (InventoryFolder)Proxy.Inventory.Store[item.ParentUUID] ?? null;

            if(parent != null)
            {
                return IsWithinSuitcase(parent);
            }

            return false;
        }
    }
}
