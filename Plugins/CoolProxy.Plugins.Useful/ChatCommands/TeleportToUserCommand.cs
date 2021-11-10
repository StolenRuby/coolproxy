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
    class TeleportToUserCommand : Command
    {
        public TeleportToUserCommand(SettingsManager settings, CoolProxyFrame frame)
        {
            Proxy = frame;
            CMD = ".tpto";
            Name = "Teleport to User";
            Description = "Teleport to a specified user.\n\nUsage: .tpto <username>";
            Category = CommandCategory.Movement;
        }

        public override string Execute(string[] args)
        {
            if (args.Length < 2)
                return "Usage: .tpto <username>";

            string find = args[1];

            if (args.Length > 3)
            {
                var newArray = args.Skip(1).Take(args.Length - 2).ToArray();
                find = string.Join(" ", newArray);
            }

            var av = Proxy.Network.CurrentSim.ObjectsAvatars.Find(avatar => avatar.Name.ToLower().Contains(find));

            if (av != null)
            {
                TeleportLocationRequestPacket teleport = new TeleportLocationRequestPacket();
                teleport.AgentData.AgentID = Proxy.Agent.AgentID;
                teleport.AgentData.SessionID = Proxy.Agent.SessionID;
                teleport.Info.LookAt = Vector3.Zero;
                teleport.Info.Position = av.Position;
                teleport.Info.RegionHandle = Proxy.Network.CurrentSim.Handle;

                Proxy.Network.CurrentSim.Inject(teleport, Direction.Outgoing);

                return string.Empty;
            }
            else return "Avatar not found!";
        }
    }
}
