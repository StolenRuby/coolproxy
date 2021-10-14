namespace CoolProxy.Plugins.Textures
{
    public class ObjectTexturesPlugin : CoolProxyPlugin
    {
        public ObjectTexturesPlugin(SettingsManager settings, GUIManager gui, CoolProxyFrame frame)
        {
            gui.AddToolButton("Objects", "View Textures", (x, y) => new ObjectTexturesForm(frame, settings).Show());
        }
    }
}