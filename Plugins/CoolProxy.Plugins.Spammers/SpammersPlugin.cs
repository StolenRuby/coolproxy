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


            IToolBox toolbox = frame.RequestModuleInterface<IToolBox>();

            toolbox.AddTool(new SimpleToggleFormButton("Instant Message Spammer", new IMSpamForm(Proxy))
            {
                ID = "TOGGLE_IM_SPAM_FORM"
            });

            toolbox.AddTool(new SimpleToggleFormButton("Touch Spammer", new TouchSpammerForm(Proxy))
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
