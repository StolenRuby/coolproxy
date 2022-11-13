using CoolProxy.Plugins.OpenSim;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoolProxy.Plugins.SuperSuitcase
{
    public class SuperSuitcasePlugin : CoolProxyPlugin
    {
        internal static IROBUST ROBUST;

        private CoolProxyFrame Proxy;

        public SuperSuitcasePlugin(CoolProxyFrame frame)
        {
            Proxy = frame;
            ROBUST = frame.RequestModuleInterface<IROBUST>();
            IGUI gui = frame.RequestModuleInterface<IGUI>();

            gui.AddMainMenuOption(new MenuOption("BROWSE_TARGET_SUITCASE", "Browse Target Suitcase...", true, "Hacks")
            {
                Clicked = OpenSuitcase
            });

            gui.AddToolButton("Hacks", "Browse Target Suitcase", (x, y) => OpenSuitcase(null));
        }

        private void OpenSuitcase(object user_data)
        {

            AvatarPickerSearchForm avatarPickerSearchForm = new AvatarPickerSearchForm();

            if (avatarPickerSearchForm.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                ROBUST.Inventory.GetRootFolder(avatarPickerSearchForm.SelectedID, (root) =>
                {
                    if (root != null)
                    {
                        var form = new SuperSuitcaseForm(Proxy, root, avatarPickerSearchForm.SelectedName);

                        form.TopMost = Proxy.Settings.getBool("KeepCoolProxyOnTop");
                        Proxy.Settings.getSetting("KeepCoolProxyOnTop").OnChanged += (s, a) => { form.TopMost = (bool)a.Value; };

                        form.Show();
                    }
                    else
                    {
                        Proxy.SayToUser("No inventory found for " + avatarPickerSearchForm.SelectedName);
                    }
                });
            }
        }
    }
}
