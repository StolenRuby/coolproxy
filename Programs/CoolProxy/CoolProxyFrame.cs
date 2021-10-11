using GridProxy;
using OpenMetaverse.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static GridProxy.RegionManager;

namespace CoolProxy
{
    public class CoolProxyFrame : ProxyFrame
    {
        public OpenSimManager OpenSim { get; private set; }

        public CoolProxyFrame(string[] args) : this(args, null) { }

        internal event NewChatCommandAdded OnNewChatCommand;

        Dictionary<string, Command> newChatcommands = new Dictionary<string, Command>();

        public CoolProxyFrame(string[] args, ProxyConfig proxyConfig) : base(args, proxyConfig)
        {
            Init(args, proxyConfig);
        }

        private void Init(string[] args, ProxyConfig proxyConfig)
        {
            OpenSim = new OpenSimManager(this);
        }

        public new void Start()
        {
            base.Start();

            this.Network.AddDelegate(PacketType.ChatFromViewer, Direction.Outgoing, ChatFromViewerOut);
        }

        public new void Stop()
        {
            base.Stop();
        }

        private Packet ChatFromViewerOut(Packet packet, RegionProxy sim)
        {
            ChatFromViewerPacket cpacket = (ChatFromViewerPacket)packet;
            string message = Encoding.UTF8.GetString(cpacket.ChatData.Message).Replace("\0", "");

            if (message.Length > 1 && message[0] == '.')
            {
                string[] words = message.Split(' ');
                if (newChatcommands.ContainsKey(words[0]))
                {
                    string result = (newChatcommands[words[0]]).Execute(words);

                    if(result != string.Empty)
                    {
                        SayToUser(result);
                    }

                    return null;
                }
            }

            return packet;
        }

        internal void AddChatCommand(Command command)
        {
            newChatcommands[command.CMD] = command;
            OnNewChatCommand.Invoke(command.CMD, command.Name, command.Description);
        }

        ////////////////////////////////////////////////////

        protected Dictionary<Type, List<object>> ModuleInterfaces = new Dictionary<Type, List<object>>();

        public T RequestModuleInterface<T>()
        {
            if (ModuleInterfaces.ContainsKey(typeof(T)) &&
                    (ModuleInterfaces[typeof(T)].Count > 0))
                return (T)ModuleInterfaces[typeof(T)][0];
            else
                return default(T);
        }

        public void RegisterModuleInterface<M>(M mod)
        {
            OpenMetaverse.Logger.Log(string.Format( "[FRAME]: Registering interface {0}", typeof(M)), OpenMetaverse.Helpers.LogLevel.Info);

            List<Object> l = null;
            if (!ModuleInterfaces.TryGetValue(typeof(M), out l))
            {
                l = new List<Object>();
                ModuleInterfaces.Add(typeof(M), l);
            }
        }

        public void UnregisterModuleInterface<M>(M mod)
        {
            List<Object> l;
            if (ModuleInterfaces.TryGetValue(typeof(M), out l))
            {
                if (l.Remove(mod))
                {
                }
            }
        }
    }

    public delegate void NewChatCommandAdded(string command, string label, string info = "");
}
