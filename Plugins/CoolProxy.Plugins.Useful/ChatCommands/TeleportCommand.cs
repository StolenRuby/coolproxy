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
    class TeleportCommand : Command
    {
        public TeleportCommand(CoolProxyFrame frame)
        {
            Proxy = frame;
            CMD = "tp";
            Name = "Teleport to Position";
            Description = "Teleport to a specified position.\n\nUsage: tp <x> <y> <z>";
            Category = CommandCategory.Movement;
        }

        public override string Execute(string[] args)
        {
            if (args.Length < 4)
                return "Usage: tp <x> <y> <z>";

            float x, y, z;
            if (float.TryParse(args[1], out x) && float.TryParse(args[2], out y) && float.TryParse(args[3], out z))
            {
                Vector3 pos = new Vector3(x, y, z);

                TeleportLocationRequestPacket teleport = new TeleportLocationRequestPacket();
                teleport.AgentData.AgentID = Proxy.Agent.AgentID;
                teleport.AgentData.SessionID = Proxy.Agent.SessionID;
                teleport.Info.LookAt = Vector3.Zero;
                teleport.Info.Position = pos;
                teleport.Info.RegionHandle = Proxy.Network.CurrentSim.Handle;

                Proxy.Network.CurrentSim.Inject(teleport, Direction.Outgoing);

                return string.Empty;
            }
            else return "Invalid float!";
        }
    }
}
