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

            gui.AddToggleFormQuick("Objects", "New MegaPrim", new Plugins.MegaPrimMaker.NewMegaprimForm(frame));
        }
    }
}
