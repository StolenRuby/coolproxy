using GridProxy;
using OpenMetaverse.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CoolProxy.Plugins.Movement
{
    public class MovementPlugin : CoolProxyPlugin
    {
        private CoolProxyFrame Proxy;

        private bool DisableDelays = false;

        public MovementPlugin(CoolProxyFrame frame)
        {
            Proxy = frame;

            IGUI gui = frame.RequestModuleInterface<IGUI>();
            gui.AddToolCheckbox("Avatar", "Disable Pre-jump/Landing Delay", handleCheckbox);

            Proxy.Network.AddDelegate(PacketType.AgentUpdate, Direction.Outgoing, HandleAgentUpdate);
        }

        private void handleCheckbox(object sender, EventArgs e)
        {
            DisableDelays = (sender as CheckBox).Checked;
        }

        private Packet HandleAgentUpdate(Packet packet, RegionManager.RegionProxy sim)
        {
            AgentUpdatePacket update = (AgentUpdatePacket)packet;

            if (DisableDelays)
            {
                update.AgentData.ControlFlags |= (uint)OpenMetaverse.AgentManager.ControlFlags.AGENT_CONTROL_FINISH_ANIM;
            }

            return packet;
        }
    }
}
