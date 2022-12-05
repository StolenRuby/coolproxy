using CoolProxy.Plugins.ToolBox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoolProxy.Plugins.MimicTool
{
    public class MimicToolPlugin : CoolProxyPlugin
    {
        public MimicToolPlugin(CoolProxyFrame frame)
        {
            IGUI gui = frame.RequestModuleInterface<IGUI>();

            var form = new MimicToolForm(frame);

            gui.RegisterForm("mimic_tool", form);

            gui.AddMainMenuOption(new MenuOption("TOGGLE_MIMIC_FORM", "Avatar Mimicry Tool", true, "Tools")
            {
                Clicked = (x) =>
                {
                    if (form.Visible) form.Hide();
                    else form.Show();
                },
                Checked = (x) => form.Visible
            });


            IToolBox toolbox = frame.RequestModuleInterface<IToolBox>();
            toolbox.AddTool(new SimpleToggleFormButton("Avatar Mimicry Tool", form)
            {
                ID = "TOGGLE_MIMIC_FORM",
                Default = false
            });

            //gui.AddToggleFormQuick("Avatar", "Avatar Mimicry Tool", form);
        }
    }
}
