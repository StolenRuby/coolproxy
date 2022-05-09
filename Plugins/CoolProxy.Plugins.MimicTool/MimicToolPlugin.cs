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
            gui.AddToggleFormQuick("Avatar", "Avatar Mimicry Tool", new MimicToolForm(frame));
        }
    }
}
