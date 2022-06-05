using OpenMetaverse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoolProxy.Plugins.Useful.ChatCommands
{
    public class MD5Command : Command
    {
        public MD5Command(CoolProxyFrame frame)
        {
            Proxy = frame;
            CMD = ".md5";
            Name = "MD5 String";
            Description = "MD5 a given string.\n\nUsage: .md5 <string>";
            Category = CommandCategory.Other;
        }

        public override string Execute(string[] args)
        {
            if(args .Length < 2)
            {
                return "Usage: .md5 <string>";
            }

            args = args.Skip(1).ToArray();

            return Utils.MD5String(string.Join(" ", args));
        }
    }
}
