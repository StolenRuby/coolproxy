using GridProxy;
using OpenMetaverse;
using OpenMetaverse.Packets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static GridProxy.RegionManager;

namespace CoolProxy
{
    public class CoolProxyFrame : ProxyFrame
    {
        public bool IsLindenGrid { get; internal set; } = false;

        public CoolProxyFrame(string[] args) : this(args, null) { }

        public Dictionary<string, Command> Commands = new Dictionary<string, Command>();

        private char ChatCmdPrefix = (char)0;

        public CoolProxyFrame(string[] args, ProxyConfig proxyConfig) : base(args, proxyConfig)
        {
            Init(args, proxyConfig);
        }

        public SettingsManager SettingsPerAccount { get; private set; } = null;

        private void Init(string[] args, ProxyConfig proxyConfig)
        {
            Settings.addSetting("ChatCommandPrefix", "string", ".", "What character starts chat commands.");


            string prefix = Settings.getString("ChatCommandPrefix");
            if (prefix.Length > 0)
            {
                ChatCmdPrefix = prefix[0];
            }

            Settings.getSetting("ChatCommandPrefix").OnChanged += (x, y) =>
            {
                string new_prefix = (string)y.Value;
                ChatCmdPrefix = new_prefix.Length > 0 ? new_prefix[0] : (char)0;
            };

            SettingsPerAccount = new SettingsManager();
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

            if (cpacket.ChatData.Type != 1) return packet;

            string message = Encoding.UTF8.GetString(cpacket.ChatData.Message).Replace("\0", "");

            if ((ChatCmdPrefix != 0 && message.Length > 1 && message[0] == ChatCmdPrefix) || ChatCmdPrefix == (char)0)
            {
                string[] words = message.Substring(ChatCmdPrefix == (char)0 ? 0 : 1).Split(' ');
                if (Commands.ContainsKey(words[0]))
                {
                    string result = (Commands[words[0]]).Execute(words);

                    if(result != string.Empty)
                    {
                        SayToUser(result);
                    }

                    return null;
                }
            }

            return packet;
        }

        public string RunCommand(string cmd)
        {
            string[] words = cmd.Split(' ');
            if (Program.Frame.Commands.TryGetValue(words[0], out Command command))
            {
                return command.Execute(words);
            }
            else return "Command not found";
        }

        internal void AddChatCommand(Command command)
        {
            Commands[command.CMD] = command;
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

            List<object> l = null;
            if (!ModuleInterfaces.TryGetValue(typeof(M), out l))
            {
                l = new List<object>();
                ModuleInterfaces.Add(typeof(M), l);
            }

            l.Add(mod);
        }

        public void UnregisterModuleInterface<M>(M mod)
        {
            List<object> l;
            if (ModuleInterfaces.TryGetValue(typeof(M), out l))
            {
                if (l.Remove(mod))
                {
                }
            }
        }
    }
}
