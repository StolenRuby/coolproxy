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
    class GotoCommand : Command
    {
        public GotoCommand(CoolProxyFrame frame)
        {
            Proxy = frame;
            CMD = ".goto";
            Name = "Teleport to Region";
            Description = "Teleport to another region by name.\n\nUsage: .goto <region name>";
            Category = CommandCategory.Movement;
        }

        public override string Execute(string[] args)
        {
            if (args.Length < 2)
                return "Usage: .goto <region name>";

            string name = args[1];
            for(int i = 2; i < args.Length; i++)
            {
                name += " " + args[i];
            }

            new Task(() => Proxy.Agent.Teleport(name, new Vector3(128.0f, 128.0f, 30.0f))).Start();

            return string.Empty;
        }
    }
}
