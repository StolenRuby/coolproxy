using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoolProxy.Plugins.Messages
{
    class MessagesPlugin : CoolProxyPlugin
    {
        public MessagesPlugin(CoolProxyFrame frame)
        {
            IGUI gui = frame.RequestModuleInterface<IGUI>();
            gui.AddToggleFormQuick("Messages", "Message Log", new MessageLogForm(frame));
            gui.AddToggleFormQuick("Messages", "Message Builder", new MessageBuilderForm(frame));
        }
    }
}
