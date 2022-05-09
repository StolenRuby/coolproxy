using OpenMetaverse;
using OpenMetaverse.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoolProxy.Plugins.ChatCommands
{
    class SimInfoCommand : Command
    {
        public SimInfoCommand(CoolProxyFrame frame)
        {
            Proxy = frame;
            CMD = ".siminfo";
            Name = "Display Sim Info";
            Description = "A command to print information about the region to chat.";
            Category = CommandCategory.Simulator;
        }

        public override string Execute(string[] args)
        {
            var sim = Proxy.Network.CurrentSim;

            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("Region Name: " + sim.Name);
            stringBuilder.AppendLine("EndPoint: " + sim.RemoteEndPoint.ToString());
            //stringBuilder.AppendLine("Seed Cap: " + sim.SeedCap);
            stringBuilder.AppendLine("Number of Caps: " + sim.Caps.Count.ToString());
            stringBuilder.AppendLine("Host URI: " + sim.HostUri);
            stringBuilder.AppendLine("Asset Server URI: " + sim.AssetServerURI);
            stringBuilder.AppendLine("IM Server URI: " + sim.IMServerURI);
            stringBuilder.AppendLine("Inventory Server URI: " + sim.InvetoryServerURI);
            stringBuilder.AppendLine("Profile Server URI: " + sim.ProfileServerURI);
            stringBuilder.AppendLine("Friends Server URI: " + sim.FriendsServerURI);
            stringBuilder.AppendLine("Gatekeeper URI: " + sim.GatekeeperURI);

            return stringBuilder.ToString();
        }
    }
}
