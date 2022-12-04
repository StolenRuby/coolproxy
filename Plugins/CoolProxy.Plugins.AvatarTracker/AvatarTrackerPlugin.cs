using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoolProxy.Plugins.AvatarTracker
{
    public interface IAvatarTracker
    {
        void AddSingleMenuItem(string label, HandleAvatarPicker handle);
        void AddMultipleMenuItem(string label, HandleAvatarPickerList handle);
    }

    class AvatarTrackerPlugin : CoolProxyPlugin, IAvatarTracker
    {
        private CoolProxyFrame Proxy;

        private AvatarTrackerForm Form;

        public AvatarTrackerPlugin(CoolProxyFrame frame)
        {
            Proxy = frame;
            Proxy.RegisterModuleInterface<IAvatarTracker>(this);

            Form = new AvatarTrackerForm(Proxy);

            IGUI gui = Proxy.RequestModuleInterface<IGUI>();

            gui.AddMainMenuOption(new MenuOption("TOGGLE_NEW_AVATAR_TRACKER", "Avatar Tracker", true)
            {
                Checked = (x) => Form.Visible,
                Clicked = (x) =>
                {
                    if (Form.Visible)
                    {
                        Form.Hide();
                    }
                    else
                    {
                        Form.Show();
                    }
                }
            });
        }

        public void AddMultipleMenuItem(string label, HandleAvatarPickerList handle)
        {
            Form.AddMultipleMenuItem(label, handle);
        }

        public void AddSingleMenuItem(string label, HandleAvatarPicker handle)
        {
            Form.AddSingleMenuItem(label, handle);
        }
    }
}
