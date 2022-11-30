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

            gui.AddMainMenuOption(new MenuOption("TOGGLE_HACKED_GODMODE", "Hacked GodMode", false)
            {
                Checked = (x) => LocalGodLevel > 0,
                Clicked = (x) =>
                {
                    handleChangeGodLevel(LocalGodLevel > 0 ? 0 : 250);

                    if(LocalGodLevel > 0)
                    {
                        Proxy.Network.AddDelegate(PacketType.ObjectUpdate, Direction.Incoming, HandleObjectUpdate);
                        Proxy.Network.AddDelegate(PacketType.ObjectUpdateCompressed, Direction.Incoming, HandleObjectUpdateCompressed);
                    }
                    else
                    {
                        Proxy.Network.RemoveDelegate(PacketType.ObjectUpdate, Direction.Incoming, HandleObjectUpdate);
                        Proxy.Network.RemoveDelegate(PacketType.ObjectUpdateCompressed, Direction.Incoming, HandleObjectUpdateCompressed);
                    }
                }
            });
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

        private Packet HandleObjectUpdate(Packet packet, RegionManager.RegionProxy sim)
        {
            var update = packet as ObjectUpdatePacket;

            foreach (var block in update.ObjectData)
            {
                PrimFlags flags = (PrimFlags)block.UpdateFlags;
                flags |= PrimFlags.ObjectModify | PrimFlags.ObjectMove;
                //flags |= PrimFlags.ObjectModify | PrimFlags.ObjectCopy | PrimFlags.ObjectTransfer | PrimFlags.ObjectYouOwner | PrimFlags.ObjectMove | PrimFlags.ObjectOwnerModify | PrimFlags.ObjectYouOfficer;
                block.UpdateFlags = (uint)flags;
            }

            return update;
        }

        private Packet HandleObjectUpdateCompressed(Packet packet, RegionManager.RegionProxy sim)
        {
            var ouc = packet as ObjectUpdateCompressedPacket;
            foreach (var block in ouc.ObjectData)
            {
                PrimFlags flags = (PrimFlags)block.UpdateFlags;
                flags |= PrimFlags.ObjectModify | PrimFlags.ObjectMove;
                //flags |= PrimFlags.ObjectModify | PrimFlags.ObjectCopy | PrimFlags.ObjectTransfer | PrimFlags.ObjectYouOwner | PrimFlags.ObjectMove | PrimFlags.ObjectOwnerModify | PrimFlags.ObjectYouOfficer;
                block.UpdateFlags = (uint)flags;
            }

            return ouc;
        }
    }
}
