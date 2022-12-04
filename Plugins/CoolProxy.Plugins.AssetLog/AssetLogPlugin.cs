using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CoolProxy.Plugins.AssetLog
{
    class AssetLogPlugin : CoolProxyPlugin
    {
        public AssetLogPlugin(CoolProxyFrame frame)
        {
            AssetLogForm form = new AssetLogForm(frame);

            form.FormClosing += (x, y) =>
            {
                y.Cancel = true;
                form.Hide();
            };

            IGUI tray = frame.RequestModuleInterface<IGUI>();
            if(tray != null)
            {
                tray.AddMainMenuOption(new MenuOption("TOGGLE_ASSET_LOG_FORM", "Asset Log", true, "World")
                {
                    Checked = (x) => form.Visible,
                    Clicked = (x) =>
                    {
                        if (form.Visible)
                            form.Hide();
                        else form.Show();
                    }
                });
            }
        }
    }
}
