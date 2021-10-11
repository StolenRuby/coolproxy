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
        private SettingsManager Settings;
        private GUIManager GUI;
        private CoolProxyFrame Proxy;

        public LocalGodModePlugin(SettingsManager settings, GUIManager gui, CoolProxyFrame frame)
        {
            Proxy = frame;
            Settings = settings;
            GUI = gui;

            var blarg = Enum.GetValues(typeof(GodModeLevel)).Cast<object>().ToArray();
            GUI.AddToolLabel("Avatar", "Local GodMode Level:");
            GUI.AddToolComboBox("Avatar", changeGodModeLevel, blarg, GodModeLevel.GOD_NOT);
        }

        private void changeGodModeLevel(object sender, EventArgs e)
        {
            ComboBox comboBox = sender as ComboBox;
            GodModeLevel level = (GodModeLevel)comboBox.SelectedItem;

            GrantGodlikePowersPacket grantGodlikePowersPacket = new GrantGodlikePowersPacket();
            grantGodlikePowersPacket.AgentData = new GrantGodlikePowersPacket.AgentDataBlock();
            grantGodlikePowersPacket.AgentData.AgentID = Proxy.Agent.AgentID;
            grantGodlikePowersPacket.AgentData.SessionID = Proxy.Agent.SessionID;
            grantGodlikePowersPacket.GrantData = new GrantGodlikePowersPacket.GrantDataBlock();
            grantGodlikePowersPacket.GrantData.GodLevel = (byte)level;
            grantGodlikePowersPacket.GrantData.Token = UUID.Zero;

            Proxy.Network.InjectPacket(grantGodlikePowersPacket, Direction.Incoming);
        }
    }
}
