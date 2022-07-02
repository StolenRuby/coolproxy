using OpenMetaverse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoolProxy.Plugins.Useful.ChatCommands
{
    public class HelpCommand : Command
    {
        public HelpCommand(CoolProxyFrame frame)
        {
            Proxy = frame;
            CMD = "help";
            Name = "Console Help";
            Description = "Lists available commands. Usage: help <command> to display information on commands";
            Category = CommandCategory.CoolProxy;
        }

        public override string Execute(string[] args)
        {
            if (args.Length > 1)
            {
                if (Proxy.Commands.ContainsKey(args[1]))
                    return Proxy.Commands[args[1]].Description;
                else
                    return "Command " + args[1] + " Does not exist. \"help\" to display all available commands.";
            }

            StringBuilder result = new StringBuilder();
            SortedDictionary<CommandCategory, List<Command>> CommandTree = new SortedDictionary<CommandCategory, List<Command>>();

            CommandCategory cc;
            foreach (Command c in Proxy.Commands.Values)
            {
                if (c.Category.Equals(null))
                    cc = CommandCategory.Unknown;
                else
                    cc = c.Category;

                if (CommandTree.ContainsKey(cc))
                    CommandTree[cc].Add(c);
                else
                {
                    List<Command> l = new List<Command>();
                    l.Add(c);
                    CommandTree.Add(cc, l);
                }
            }

            foreach (KeyValuePair<CommandCategory, List<Command>> kvp in CommandTree)
            {
                result.AppendFormat(System.Environment.NewLine + "* {0} Related Commands:" + System.Environment.NewLine, kvp.Key.ToString());
                int colMax = 0;
                for (int i = 0; i < kvp.Value.Count; i++)
                {
                    if (colMax >= 120)
                    {
                        result.AppendLine();
                        colMax = 0;
                    }

                    result.AppendFormat(" {0,-15}", kvp.Value[i].CMD);
                    colMax += 15;
                }
                result.AppendLine();
            }
            result.AppendLine(System.Environment.NewLine + "help <command> for usage/information");

            return result.ToString();
        }
    }
}
