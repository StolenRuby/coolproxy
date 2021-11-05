using OpenMetaverse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoolProxy.Plugins.ChatCommands
{
    class PlatformCommand : Command
    {
        public PlatformCommand(SettingsManager settings, CoolProxyFrame frame)
        {
            Proxy = frame;
            CMD = ".plat";
            Name = "Rez a Platform";
            Description = "Create a platform beneath your feet.";
            Category = CommandCategory.Movement;
        }

        public override string Execute(string[] args)
        {
            Vector3 pos = Proxy.Agent.SimPosition;
            pos.Z -= 3.0f;

            Proxy.Objects.AddPrim(Proxy.Network.CurrentSim, Primitive.ConstructionData.DefaultCube, UUID.Zero,
                pos, new Vector3(30.0f, 30.0f, 0.25f), Quaternion.Identity);

            return string.Empty;
        }
    }
}
