using CoolProxy.Plugins.OpenSim;

namespace CoolProxy.Plugins.Textures
{
    public class ObjectTexturesPlugin : CoolProxyPlugin
    {
        internal static IROBUST ROBUST;

        public ObjectTexturesPlugin(CoolProxyFrame frame)
        {
            IGUI gui = frame.RequestModuleInterface<IGUI>();
            ROBUST = frame.RequestModuleInterface<IROBUST>();
            gui.AddToolButton("Objects", "View Textures", (x, y) => new ObjectTexturesForm(frame).Show());
        }
    }
}