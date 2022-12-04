using CoolProxy.Plugins.ToolBox;
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

            var log = new MessageLogForm(frame);
            var builder = new MessageBuilderForm(frame);


            IToolBox toolbox = frame.RequestModuleInterface<IToolBox>();

            toolbox.AddTool(new SimpleToggleFormButton("Message Log", log)
            {
                ID = "TOGGLE_MESSAGE_LOG",
                Default = false
            });

            toolbox.AddTool(new SimpleToggleFormButton("Message Builder", builder)
            {
                ID = "TOGGLE_MESSAGE_BUILDER",
                Default = false
            });


            //gui.AddToggleFormQuick("Messages", "Message Log", log);
            //gui.AddToggleFormQuick("Messages", "Message Builder", builder);

            gui.AddMainMenuOption(new MenuOption("TOGGLE_MESSAGE_LOG", "Message Log", true, "Tools")
            {
                Clicked = (x) =>
                {
                    if (log.Visible)
                        log.Hide();
                    else
                        log.Show();
                },
                Checked = (x) => log.Visible
            });

            gui.AddMainMenuOption(new MenuOption("TOGGLE_MESSAGE_BUILDER", "Message Builder", true, "Tools")
            {
                Clicked = (x) =>
                {
                    if (builder.Visible)
                        builder.Hide();
                    else
                        builder.Show();
                },
                Checked = (x) => builder.Visible
            });
        }
    }
}
