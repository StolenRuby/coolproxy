using CoolProxy.Plugins.AvatarTracker;
using CoolProxy.Plugins.OpenSim;
using CoolProxy.Plugins.ToolBox;
using OpenMetaverse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CoolProxy.Plugins.GridIMHacks
{
    public class GridIMHacksPlugin : CoolProxyPlugin
    {
        private readonly UUID MrOpenSim = new UUID("6571e388-6218-4574-87db-f9379718315e");

        private CoolProxyFrame Proxy;

        public static IROBUST ROBUST { get; private set; }

        public GridIMHacksPlugin(CoolProxyFrame frame)
        {
            Proxy = frame;

            ROBUST = Proxy.RequestModuleInterface<IROBUST>();

            if(ROBUST == null)
            {
                MessageBox.Show("GridIMHacks require the OpenSim interface to be loaded first!");
                return;
            }

            IGUI gui = Proxy.RequestModuleInterface<IGUI>();
            IToolBox toolbox = frame.RequestModuleInterface<IToolBox>();

            var hg_form = new GodHacksForm(frame);

            gui.AddMainMenuOption(new MenuOption("TOGGLE_HACKED_GODTOOLS", "Hacked God Tools", true, "Hacks")
            {
                Clicked = (x) =>
                {
                    if (hg_form.Visible) hg_form.Hide();
                    else hg_form.Show();
                },
                Checked = (x) => hg_form.Visible
            });

            toolbox.AddTool(new SimpleToggleFormButton("Hacked God Tools", hg_form)
            {
                ID = "TOGGLE_HACKED_GODTOOLS"
            });

            var stp_form = new SpecialTeleportForm(frame);

            gui.AddMainMenuOption(new MenuOption("TOGGLE_SPECIAL_TELEPORT", "Special Teleport", true, "Hacks")
            {
                Clicked = (x) =>
                {
                    if (stp_form.Visible) stp_form.Hide();
                    else stp_form.Show();
                },
                Checked = (x) => stp_form.Visible
            });

            toolbox.AddTool(new SimpleToggleFormButton("Special Teleport", stp_form)
            {
                ID = "TOGGLE_SPECIAL_TELEPORT"
            });

            var ezim_form = new EasyIMSpoofer(frame);

            gui.AddMainMenuOption(new MenuOption("TOGGLE_EASY_IM_SPOOFER", "Easy IM Spoofer", true, "Hacks")
            {
                Clicked = (x) =>
                {
                    if (ezim_form.Visible) ezim_form.Hide();
                    else ezim_form.Show();
                },
                Checked = (x) => ezim_form.Visible
            });

            toolbox.AddTool(new SimpleToggleFormButton("Easy IM Spoofer", ezim_form)
            {
                ID = "TOGGLE_EASY_IM_SPOOFER"
            });

            //gui.AddToggleFormQuick("Hacks", "Hacked God Tools", hg_form);
            //gui.AddToggleFormQuick("Hacks", "Special Teleport", stp_form);
            //gui.AddToggleFormQuick("Hacks", "Easy IM Spoofer", ezim_form);


            IAvatarTracker tracker = frame.RequestModuleInterface<IAvatarTracker>();
            if (tracker != null)
            {
                tracker.AddSingleMenuItem("-", null);
                tracker.AddSingleMenuItem("Force Teleport", handleForceTeleportSingle);
                tracker.AddMultipleMenuItem("Force Teleport", handleForceTeleportMultiple);
                tracker.AddSingleMenuItem("Kick User", handleKickUser);
                tracker.AddMultipleMenuItem("Kick Users", handleKickUsers);
            }
        }

        private void handleKickUsers(List<UUID> targets)
        {
            foreach (var uuid in targets)
                handleKickUser(uuid);
        }

        private void handleForceTeleportMultiple(List<UUID> targets)
        {
            foreach (var uuid in targets)
                handleForceTeleportSingle(uuid);
        }

        private void handleForceTeleportSingle(UUID target)
        {
            ROBUST.IM.SendGridIM(target, string.Empty, target, InstantMessageDialog.GodLikeRequestTeleport, false, "@" + Proxy.Network.CurrentSim.GridURI, target, false, Proxy.Agent.SimPosition, Proxy.Network.CurrentSim.ID, 0, new byte[0], Utils.GetUnixTime());
        }

        private void handleKickUser(UUID target)
        {
            ROBUST.IM.SendGridIM(MrOpenSim, string.Empty, target, InstantMessageDialog.OpenSimKickUser, false, "Kicked by an admin", target, false, Proxy.Agent.SimPosition, Proxy.Network.CurrentSim.ID, 0, new byte[1] { 0 }, Utils.GetUnixTime());
        }
    }
}
