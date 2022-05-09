namespace CoolProxy.Plugins.Textures
{
    public class ObjectTexturesPlugin : CoolProxyPlugin
    {
        public ObjectTexturesPlugin(CoolProxyFrame frame)
        {
            IGUI gui = frame.RequestModuleInterface<IGUI>();
            gui.AddToolButton("Objects", "View Textures", (x, y) => new ObjectTexturesForm(frame).Show());
        }
    }
}