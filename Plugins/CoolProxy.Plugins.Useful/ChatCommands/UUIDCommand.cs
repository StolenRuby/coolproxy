using OpenMetaverse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoolProxy.Plugins.Useful.ChatCommands
{
    public class UUIDCommand : Command
    {
        public UUIDCommand(CoolProxyFrame frame)
        {
            Proxy = frame;
            CMD = ".uuid";
            Name = "Generate UUID";
            Description = "Generates a random UUID.";
            Category = CommandCategory.Other;
        }

        public override string Execute(string[] args)
        {
            return UUID.Random().ToString();
        }
    }
}
