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

            gui.AddToggleFormQuick("Hacks", "Hacked Profile Editor", new HackedProfileEditor(frame));
        }
    }
}
