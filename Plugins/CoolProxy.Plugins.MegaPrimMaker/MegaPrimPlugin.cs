using CoolProxy.Plugins.MegaPrimMaker;
using CoolProxy.Plugins.ToolBox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoolProxy.Plugin.MegaPrimMaker
{
    public class MegaPrimPlugin : CoolProxyPlugin
    {
        public MegaPrimPlugin(CoolProxyFrame frame)
        {
            IGUI gui = frame.RequestModuleInterface<IGUI>();

            var form = new NewMegaprimForm(frame);

            gui.RegisterForm("megaprim_maker", form);

            gui.AddMainMenuOption(new MenuOption("TOGGLE_MEGAPRIM_MAKER", "New MegaPrim", true, "Tools")
            {
                Clicked = (x) =>
                {
                    if (form.Visible) form.Hide();
                    else form.Show();
                },
                Checked = (x) => form.Visible
            });


            IToolBox toolbox = frame.RequestModuleInterface<IToolBox>();
            SimpleToggleFormButton toggleButton = new SimpleToggleFormButton("New MegaPrim", form)
            {
                ID = "TOGGLE_MEGAPRIM_MAKER",
                Default = false
            };

            toolbox.AddTool(toggleButton);
            //gui.AddToggleFormQuick("Objects", "New MegaPrim", form);
        }
    }
}
