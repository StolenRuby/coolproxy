using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoolProxy.Plugins.DynamicGroupTitle
{
    class DynamicGroupTitlePlugin : CoolProxyPlugin
    {
        public DynamicGroupTitlePlugin(CoolProxyFrame frame)
        {
            IGUI gui = frame.RequestModuleInterface<IGUI>();

            var form = new DynamicGroupTitleEditor(frame);

            gui.AddMainMenuOption(new MenuOption("TOGGLE_DYNAMIC_GROUP_TITLE_EDITOR", "Dynamic Group Title", true, "Tools")
            {
                Clicked = (x) =>
                {
                    if (form.Visible) form.Hide();
                    else form.Show();
                },
                Checked = (x) => form.Visible
            });

            gui.AddToggleFormQuick("Avatar", "Dynamic Group Title", form);
        }
    }
}
