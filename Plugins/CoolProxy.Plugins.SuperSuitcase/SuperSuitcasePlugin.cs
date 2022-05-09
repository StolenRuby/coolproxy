using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoolProxy.Plugins.SuperSuitcase
{
    public class SuperSuitcasePlugin : CoolProxyPlugin
    {
        public SuperSuitcasePlugin(CoolProxyFrame frame)
        {
            IGUI gui = frame.RequestModuleInterface<IGUI>();
            gui.AddToolButton("Hacks", "Browse Target Suitcase", (x, y) =>
            {
                AvatarPickerSearchForm avatarPickerSearchForm = new AvatarPickerSearchForm();

                if(avatarPickerSearchForm.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    frame.OpenSim.XInventory.GetRootFolder(avatarPickerSearchForm.SelectedID, (root) =>
                    {
                        if(root != null)
                        {
                            var form = new SuperSuitcaseForm(frame, root);

                            form.TopMost = frame.Settings.getBool("KeepCoolProxyOnTop");
                            frame.Settings.getSetting("KeepCoolProxyOnTop").OnChanged += (s, a) => { form.TopMost = (bool)a.Value; };

                            form.Show();
                        }
                        else
                        {
                            frame.SayToUser("No inventory found for " + avatarPickerSearchForm.SelectedName);
                        }
                    });
                }
            });
        }
    }
}
