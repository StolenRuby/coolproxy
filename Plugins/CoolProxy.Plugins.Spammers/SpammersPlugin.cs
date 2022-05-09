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

            IGUI gui = frame.RequestModuleInterface<IGUI>();
            gui.AddToggleFormQuick("Avatar", "Instant Message Spammer", new IMSpamForm(Proxy));
            gui.AddToggleFormQuick("Objects", "Touch Spammer", new TouchSpammerForm(Proxy));
        }
    }
}
