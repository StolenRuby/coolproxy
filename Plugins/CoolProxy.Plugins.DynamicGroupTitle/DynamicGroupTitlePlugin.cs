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
            gui.AddToggleFormQuick("Avatar", "Dynamic Group Title", new DynamicGroupTitleEditor(frame));
        }
    }
}
