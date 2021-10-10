using Nwc.XmlRpc;
using OpenMetaverse;
using OpenMetaverse.Packets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace GridProxy
{
    public class ProxyFrame
    {
        public ProxyConfig Config { get; private set; }
        public HttpManager HTTP { get; private set; }
        public LoginProxy Login { get; private set; }
        public RegionManager Network { get; private set; }
        public AgentManager Agent { get; private set; }
        public ObjectManager Objects { get; private set; }
        public InventoryManager Inventory { get; private set; }
        public GridManager Grid { get; private set; }
        public AssetManager Assets { get; private set; }

        public string[] Args { get; private set; }

        public bool Started { get; private set; } = false;
        public bool LoggedIn { get; private set; } = false;


        public event DisconnectedEvent Disconnected;


        public ProxyFrame(string[] args)
        {
            Init(args, null);
        }

        public ProxyFrame(string[] args, ProxyConfig proxyConfig)
        {
            Init(args, proxyConfig);
        }

        private void Init(string[] args, ProxyConfig proxyConfig)
        {
            this.Args = args;

            if (proxyConfig == null)
            {
                proxyConfig = new ProxyConfig(args, true);
            }

            Config = proxyConfig;
            HTTP = new HttpManager(this);
            Login = new LoginProxy(this);
            Network = new RegionManager(this);
            Agent = new AgentManager(this);
            Objects = new ObjectManager(this);
            Inventory = new InventoryManager(this);
            Grid = new GridManager(this);
            Assets = new AssetManager(this);

            Login.AddLoginResponseDelegate(onLoginResponse);
            Network.AddDelegate(PacketType.LogoutReply, Direction.Incoming, onLogoutReply);
        }

        private Packet onLogoutReply(Packet packet, RegionManager.RegionProxy sim)
        {
            LoggedIn = false;

            Disconnected?.Invoke("User logged out");

            return packet;
        }

        public void Start()
        {
            HTTP.Start();
            Login.Start();
            Network.Start();

            Started = true;
        }

        public void Stop()
        {
            HTTP.Stop();
            Started = false;
        }

        private void onLoginResponse(XmlRpcResponse response)
        {
            System.Collections.Hashtable responseData;
            try
            {
                responseData = (System.Collections.Hashtable)response.Value;
            }
            catch (Exception e)
            {
                OpenMetaverse.Logger.Log(e.Message, Helpers.LogLevel.Error);
                return;
            }

            if(responseData.ContainsKey("login"))
            {
                if((string)responseData["login"] == "true")
                {
                    LoggedIn = true;
                    //responseData["message"] = "New and improved GridProxy!";
                }
            }
        }

        private UUID messageFromID = UUID.Zero;

        private UUID MessageFromID
        {
            get
            {
                if (messageFromID == UUID.Zero)
                    messageFromID = UUID.Random();
                return messageFromID;
            }
        }

        public void SayToUser(string message)
        {
            SayToUser("Cool Proxy", message);
        }

        public void SayToUser(string fromName, string message)
        {
            ChatFromSimulatorPacket packet = new ChatFromSimulatorPacket();
            packet.ChatData.SourceID = MessageFromID;
            packet.ChatData.FromName = Utils.StringToBytes(fromName);
            packet.ChatData.OwnerID = Agent.AgentID;
            packet.ChatData.SourceType = (byte)2;
            packet.ChatData.ChatType = (byte)1;
            packet.ChatData.Audible = (byte)1;
            packet.ChatData.Position = new Vector3(0, 0, 0);
            packet.ChatData.Message = Utils.StringToBytes(message);
            Network.InjectPacket(packet, Direction.Incoming);
        }

        public void AlertMessage(string message, bool modal)
        {
            AgentAlertMessagePacket alertPacket = new AgentAlertMessagePacket();
            alertPacket.AgentData.AgentID = Agent.AgentID;
            alertPacket.AlertData.Message = Utils.StringToBytes(message);
            alertPacket.AlertData.Modal = modal;
            Network.InjectPacket(alertPacket, Direction.Incoming);
        }
    }

    public delegate void DisconnectedEvent(string reason);
}
