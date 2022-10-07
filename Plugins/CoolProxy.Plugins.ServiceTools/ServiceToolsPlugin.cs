using CoolProxy.Plugins.OpenSim;
using OpenMetaverse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CoolProxy.Plugins.ServiceTools
{
    public class ServiceToolsPlugin : CoolProxyPlugin
    {
        private CoolProxyFrame Proxy;

        internal static IROBUST ROBUST;

        public ServiceToolsPlugin(CoolProxyFrame frame)
        {
            Proxy = frame;

            IGUI gui = frame.RequestModuleInterface<IGUI>();
            ROBUST = frame.RequestModuleInterface<IROBUST>();

            gui.AddToggleFormQuick("Assets", "Asset Service Upload", new AssetServiceTool(frame));
            gui.AddToggleFormQuick("Avatar", "Grid Instant Message Tool", new GridIMTool(frame));

            gui.AddInventoryItemOption("Edit Item...", (x) => new XInventoryServiceForm(frame, x).Show(), frame.Inventory.IsItemWithinSuitcase);
            gui.AddInventoryFolderOption("Add Item...", (x) => new XInventoryServiceForm(frame, x).Show(), frame.Inventory.IsWithinSuitcase);

            gui.AddInventoryItemOption("Fetch Asset ID", x =>
            {
                ROBUST.Inventory.GetItem(x.UUID, x.OwnerID, item =>
                {
                    if (item != null)
                    {
                        Clipboard.SetText(item.AssetUUID.ToString());
                    }
                    else Clipboard.SetText(UUID.Zero.ToString());
                });
            }, e =>
            {
                if(e.AssetUUID == UUID.Zero)
                {
                    return frame.Inventory.IsItemWithinSuitcase(e);
                }

                return false;
            });
        }
    }
}
