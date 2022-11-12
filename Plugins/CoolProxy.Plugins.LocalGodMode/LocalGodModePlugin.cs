using GridProxy;
using OpenMetaverse;
using OpenMetaverse.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CoolProxy.Plugins.LocalGodMode
{
    public enum GodModeLevel
    {
        GOD_NOT = 0,
        GOD_LIKE = 1,
        GOD_CUSTOMER_SERVICE = 100,
        GOD_LIAISON = 150,
        GOD_FULL = 200,
        GOD_MAINTENANCE = 250
    }

    public class LocalGodModePlugin : CoolProxyPlugin
    {
        private CoolProxyFrame Proxy;

        public LocalGodModePlugin(CoolProxyFrame frame)
        {
            Proxy = frame;

            IGUI gui = frame.RequestModuleInterface<IGUI>();

            var blarg = Enum.GetValues(typeof(GodModeLevel)).Cast<object>().ToArray();

            foreach(var l in blarg)
            {
                MenuOption opt = new MenuOption("SET_GM_LEVEL_" + l.ToString(), l.ToString(), true, "Local GodMode Level")
                {
                    Clicked = handleChangeGodLevel,
                    Checked = handleCheckGodLevel,
                    Tag = l
                };
                gui.AddMainMenuOption(opt);
            }
        }


        int LocalGodLevel = 0;

        private void handleChangeGodLevel(object user_data)
        {
            LocalGodLevel = (int)user_data;

            GodModeLevel level = (GodModeLevel)LocalGodLevel;

            GrantGodlikePowersPacket grantGodlikePowersPacket = new GrantGodlikePowersPacket();
            grantGodlikePowersPacket.AgentData = new GrantGodlikePowersPacket.AgentDataBlock();
            grantGodlikePowersPacket.AgentData.AgentID = Proxy.Agent.AgentID;
            grantGodlikePowersPacket.AgentData.SessionID = Proxy.Agent.SessionID;
            grantGodlikePowersPacket.GrantData = new GrantGodlikePowersPacket.GrantDataBlock();
            grantGodlikePowersPacket.GrantData.GodLevel = (byte)level;
            grantGodlikePowersPacket.GrantData.Token = UUID.Zero;

            Proxy.Network.InjectPacket(grantGodlikePowersPacket, Direction.Incoming);
        }

        private bool handleCheckGodLevel(object user_data)
        {
            return (int)user_data == LocalGodLevel;
        }
    }
}
