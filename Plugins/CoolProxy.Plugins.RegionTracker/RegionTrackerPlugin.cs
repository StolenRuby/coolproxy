using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CoolProxy.Plugins.RegionTracker
{
    class RegionTrackerPlugin : CoolProxyPlugin
    {
        public RegionTrackerPlugin(CoolProxyFrame frame)
        {
            IGUI gui = frame.RequestModuleInterface<IGUI>();

            var form = new RegionTrackerForm(frame);

            form.FormClosing += (x, y) => { y.Cancel = true; form.Hide(); };

            gui.AddMainMenuOption(new MenuOption("TOGGLE_REGION_TRACKER_FORM", "Region Tracker", true, "World")
            {
                Checked = (x) => form.Visible,
                Clicked = (x) =>
                {
                    if (form.Visible)
                        form.Hide();
                    else form.Show();
                }
            });
        }
    }
}
