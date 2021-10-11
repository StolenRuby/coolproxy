using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoolProxy
{
    public enum CommandCategory : int
    {
        Parcel,
        Appearance,
        Movement,
        Simulator,
        Communication,
        Inventory,
        Objects,
        Voice,
        CoolProxy,
        Friends,
        Groups,
        Other,
        Unknown,
        Search
    }

    public abstract class Command : IComparable
    {
        public string CMD;
        public string Name;
        public string Description;
        public CommandCategory Category;

        public CoolProxyFrame Proxy;

        public abstract string Execute(string[] args);

        public int CompareTo(object obj)
        {
            if (obj is Command)
            {
                Command c2 = (Command)obj;
                return Category.CompareTo(c2.Category);
            }
            else
                throw new ArgumentException("Object is not of type Command.");
        }

    }
}
