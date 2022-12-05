using CoolProxy.Plugins.ToolBox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoolProxy.Plugins.HackedProfileEditor
{
    public class HackedProfileEditorPlugin : CoolProxyPlugin
    {
        private CoolProxyFrame Proxy;

        public HackedProfileEditorPlugin(CoolProxyFrame frame)
        {
            Proxy = frame;

            IGUI gui = Proxy.RequestModuleInterface<IGUI>();

            var form = new HackedProfileEditor(frame);

            gui.RegisterForm("hacked_profile_editor", form);

            gui.AddMainMenuOption(new MenuOption("TOGGLE_HACKED_PROFILE_EDITOR", "Hacked Profile Editor", true, "Hacks")
            {
                Clicked = (x) =>
                {
                    if (form.Visible) form.Hide();
                    else form.Show();
                },
                Checked = (x) => form.Visible
            });


            IToolBox toolbox = frame.RequestModuleInterface<IToolBox>();
            SimpleToggleFormButton toggleButton = new SimpleToggleFormButton("Hacked Profile Editor", form)
            {
                ID = "TOGGLE_HACKED_PROFILE_EDITOR"
            };

            toolbox.AddTool(toggleButton);
            //gui.AddToggleFormQuick("Hacks", "Hacked Profile Editor", form);
        }
    }
}
