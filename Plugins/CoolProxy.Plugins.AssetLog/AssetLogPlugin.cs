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
            IGUI gui = frame.RequestModuleInterface<IGUI>();
            if(gui != null)
            {
                AssetLogForm form = new AssetLogForm(frame);

                gui.RegisterForm("asset_log", form);

                gui.AddMainMenuOption(new MenuOption("TOGGLE_ASSET_LOG_FORM", "Asset Log", true, "World")
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
