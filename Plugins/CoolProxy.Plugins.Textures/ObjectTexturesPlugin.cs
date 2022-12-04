using CoolProxy.Plugins.OpenSim;
using CoolProxy.Plugins.ToolBox;

namespace CoolProxy.Plugins.Textures
{
    public class ObjectTexturesPlugin : CoolProxyPlugin
    {
        internal static IROBUST ROBUST;

        public ObjectTexturesPlugin(CoolProxyFrame frame)
        {
            IGUI gui = frame.RequestModuleInterface<IGUI>();
            ROBUST = frame.RequestModuleInterface<IROBUST>();


            IToolBox toolbox = frame.RequestModuleInterface<IToolBox>();

            SimpleButton simpleButton = new SimpleButton("Object Textures", (x, y) => new ObjectTexturesForm(frame).Show())
            {
                ID = "VIEW_OBJECT_TEXTURES",
                Default = false
            };

            toolbox.AddTool(simpleButton);
            //gui.AddToolButton("Objects", "View Textures", (x, y) => new ObjectTexturesForm(frame).Show());

            gui.AddMainMenuOption(new MenuOption("VIEW_OBJECT_TEXTURES", "View Textures...", true, "Objects")
            {
                Clicked = (x) => new ObjectTexturesForm(frame).Show()
            });
        }
    }
}