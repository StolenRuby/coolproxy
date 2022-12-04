using CoolProxy.Plugins.InventoryBrowser;
using CoolProxy.Plugins.OpenSim;
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
        internal static IROBUST ROBUST;

        private CoolProxyFrame Proxy;

        public EditorsPlugin(CoolProxyFrame frame)
        {
            Proxy = frame;
            ROBUST = frame.RequestModuleInterface<IROBUST>();
            IGUI gui = frame.RequestModuleInterface<IGUI>();

            IInventoryBrowser inv = Proxy.RequestModuleInterface<IInventoryBrowser>();
            if (inv != null)
            {
                inv.AddInventoryItemOption("Hex Edit", x => new HexEditor(x, frame).Show(), IsEditable);
                inv.AddInventoryItemOption("Text Edit", x => new TextEditor(x, frame).Show(), IsEditable);
            }
        }

        bool IsEditable(InventoryItem item)
        {
            if(item.AssetUUID == UUID.Zero)
            {
                if(Proxy.Inventory.SuitcaseID != UUID.Zero)
                {
                    return Proxy.Inventory.IsItemWithinSuitcase(item);
                }
            }

            return true;
        }
    }
}
