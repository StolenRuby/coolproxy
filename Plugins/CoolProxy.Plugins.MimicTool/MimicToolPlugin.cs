using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoolProxy.Plugins.MimicTool
{
    public class MimicToolPlugin : CoolProxyPlugin
    {
        public MimicToolPlugin(SettingsManager settings, GUIManager gui, CoolProxyFrame frame)
        {
            gui.AddToggleFormQuick("Avatar", "Avatar Mimicry Tool", new MimicToolForm(frame));
        }
    }
}
