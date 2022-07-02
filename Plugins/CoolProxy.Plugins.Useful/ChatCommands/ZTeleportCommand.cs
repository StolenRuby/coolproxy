using GridProxy;
using OpenMetaverse;
using OpenMetaverse.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoolProxy.Plugins.ChatCommands
{
    class ZTeleportCommand : Command
    {
        public ZTeleportCommand(CoolProxyFrame frame)
        {
            Proxy = frame;
            CMD = "gth";
            Name = "Teleport to Height";
            Description = "Teleport to a specified height.\n\nUsage: gth <height>";
            Category = CommandCategory.Movement;
        }

        public override string Execute(string[] args)
        {
            if (args.Length < 2)
                return "Usage: gth <height>";

            float height;

            if (float.TryParse(args[1], out height))
            {
                Vector3 pos = Proxy.Agent.SimPosition;
                pos.Z = height;

                TeleportLocationRequestPacket teleport = new TeleportLocationRequestPacket();
                teleport.AgentData.AgentID = Proxy.Agent.AgentID;
                teleport.AgentData.SessionID = Proxy.Agent.SessionID;
                teleport.Info.LookAt = Vector3.Zero;
                teleport.Info.Position = pos;
                teleport.Info.RegionHandle = Proxy.Network.CurrentSim.Handle;

                Proxy.Network.CurrentSim.Inject(teleport, Direction.Outgoing);

                return string.Empty;
            }
            else return "Invalid float";
        }
    }
}
