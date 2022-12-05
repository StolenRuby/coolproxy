using CoolProxy.Plugins.ToolBox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoolProxy.Plugins.Spammers
{
    public class SpammersPlugin : CoolProxyPlugin
    {
        private CoolProxyFrame Proxy;

        public SpammersPlugin(CoolProxyFrame frame)
        {
            Proxy = frame;

            var im_spammer = new IMSpamForm(Proxy);
            var touch_spammer = new TouchSpammerForm(Proxy);


            IGUI gui = frame.RequestModuleInterface<IGUI>();

            gui.RegisterForm("im_spammer", im_spammer);
            gui.RegisterForm("touch_spammer", touch_spammer);


            IToolBox toolbox = frame.RequestModuleInterface<IToolBox>();

            toolbox.AddTool(new SimpleToggleFormButton("Instant Message Spammer", im_spammer)
            {
                ID = "TOGGLE_IM_SPAM_FORM"
            });

            toolbox.AddTool(new SimpleToggleFormButton("Touch Spammer", touch_spammer)
            {
                ID = "TOGGLE_TOUCH_SPAM_FORM"
            });

            toolbox.AddTool(new TouchSpammerTool(frame));
            toolbox.AddTool(new IMSpamTool(frame));

            //IGUI gui = frame.RequestModuleInterface<IGUI>();
            //gui.AddToggleFormQuick("Avatar", "Instant Message Spammer", new IMSpamForm(Proxy));
            //gui.AddToggleFormQuick("Objects", "Touch Spammer", new TouchSpammerForm(Proxy));
        }
    }
}
