using CoolProxy.Plugins.InventoryBrowser;
using CoolProxy.Plugins.OpenSim;
using CoolProxy.Plugins.ToolBox;
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


            var ast = new AssetServiceTool(frame);
            var gimt = new GridIMTool(frame);

            gui.RegisterForm("asset_service_uploader", ast);
            gui.RegisterForm("grid_im_tool", gimt);


            IToolBox toolbox = frame.RequestModuleInterface<IToolBox>();

            toolbox.AddTool(new SimpleToggleFormButton("Asset Service Upload", ast)
            {
                ID = "TOGGLE_ROBUST_UPLOAD"
            });

            toolbox.AddTool(new SimpleToggleFormButton("Grid Instant Message Tool", gimt)
            {
                ID = "TOGGLE_ROBUST_IM"
            });

            //gui.AddToggleFormQuick("Assets", "Asset Service Upload", new AssetServiceTool(frame));
            //gui.AddToggleFormQuick("Avatar", "Grid Instant Message Tool", new GridIMTool(frame));

            IInventoryBrowser inv = Proxy.RequestModuleInterface<IInventoryBrowser>();
            if (inv != null)
            {
                inv.AddInventoryItemOption("Edit Item...", (x) => new XInventoryServiceForm(frame, x).Show(), frame.Inventory.IsItemWithinSuitcase);
                inv.AddInventoryFolderOption("Add Item...", (x) => new XInventoryServiceForm(frame, x).Show(), frame.Inventory.IsWithinSuitcase);

                inv.AddInventoryItemOption("Fetch Asset ID", x =>
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
                    if (e.AssetUUID == UUID.Zero)
                    {
                        return frame.Inventory.IsItemWithinSuitcase(e);
                    }

                    return false;
                });
            }
        }
    }
}
