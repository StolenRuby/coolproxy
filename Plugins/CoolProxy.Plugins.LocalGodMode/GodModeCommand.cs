using GridProxy;
using OpenMetaverse;
using OpenMetaverse.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoolProxy.Plugins.LocalGodMode
{
    class GodModeCommand : Command
    {
        public GodModeCommand(CoolProxyFrame frame)
        {
            Proxy = frame;
            CMD = "gm";
            Name = "Set Local GodMode Level";
            Description = "Set your local GodMode level.\n\nUsage: gm <level>";
            Category = CommandCategory.Other;
        }

        public override string Execute(string[] args)
        {
            if (args.Length < 2)
                return "Usage: gm <level>";

            int level;
            if(int.TryParse(args[1], out level))
            {
                GrantGodlikePowersPacket grantGodlikePowersPacket = new GrantGodlikePowersPacket();
                //grantGodlikePowersPacket.AgentData = new GrantGodlikePowersPacket.AgentDataBlock();
                grantGodlikePowersPacket.AgentData.AgentID = Proxy.Agent.AgentID;
                grantGodlikePowersPacket.AgentData.SessionID = Proxy.Agent.SessionID;
                //grantGodlikePowersPacket.GrantData = new GrantGodlikePowersPacket.GrantDataBlock();
                grantGodlikePowersPacket.GrantData.GodLevel = (byte)level;
                grantGodlikePowersPacket.GrantData.Token = UUID.Zero;

                Proxy.Network.InjectPacket(grantGodlikePowersPacket, Direction.Incoming);
            }

            return string.Empty;
        }
    }
}
