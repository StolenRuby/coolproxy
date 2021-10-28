using Nwc.XmlRpc;
using OpenMetaverse;
using OpenMetaverse.Packets;
using OpenMetaverse.StructuredData;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using static GridProxy.RegionManager;

namespace GridProxy
{
    public enum Direction
    {
        Incoming,
        Outgoing
    }

    public class RegionManager
    {
        public delegate Packet PacketDelegate(Packet packet, RegionProxy sim);

        //public delegate void GenericRegionDelegate(RegionProxy region);

        public event GenericRegionDelegate OnRegionChanged;
        public event GenericRegionDelegate OnHandshake;

        private Socket simFacingSocket;
        //public IPEndPoint activeCircuit = null;
        //private Dictionary<IPEndPoint, IPEndPoint> proxyEndPoints = new Dictionary<IPEndPoint, IPEndPoint>();
        private Dictionary<IPEndPoint, RegionProxy> simProxies = new Dictionary<IPEndPoint, RegionProxy>();
        private Dictionary<EndPoint, RegionProxy> proxyHandlers = new Dictionary<EndPoint, RegionProxy>();

        public void AddDelegate(PacketType logoutReply, object onLogoutReply)
        {
            throw new NotImplementedException();
        }

        private Dictionary<PacketType, List<PacketDelegate>> incomingDelegates { get; set; } = new Dictionary<PacketType, List<PacketDelegate>>();
        private Dictionary<PacketType, List<PacketDelegate>> outgoingDelegates { get; set; } = new Dictionary<PacketType, List<PacketDelegate>>();
        private List<Packet> queuedIncomingInjections { get; set; } = new List<Packet>();
        private List<Packet> queuedOutgoingInjections { get; set; } = new List<Packet>();


        public List<RegionProxy> Regions { get; private set; } = new List<RegionProxy>();

        
        private ObservableDictionary<string, CapInfo> KnownCaps = new ObservableDictionary<string, CapInfo>();

        private Dictionary<string, List<CapsDelegate>> KnownCapsDelegates = new Dictionary<string, List<CapsDelegate>>();

        private string CapProxyURL = string.Empty;

        public RegionProxy CurrentSim { get; private set; }

        public GenericRegionDelegate OnNewRegion;

        private ProxyFrame Frame;

        public RegionManager(ProxyFrame frame)
        {
            Frame = frame;

            Frame.Login.AddLoginResponseDelegate(FixupLoginResponse);
            InitializeSimProxy();

            //AddDelegate(PacketType.ChatFromViewer, Direction.Outgoing, HandleChatFromViewer);

            AddDelegate(PacketType.RegionHandshake, Direction.Incoming, HandleHandshake);

            Frame.HTTP.AddHttpHandler("/capproxy", ProxyCaps);

            AddCapsDelegate("EventQueueGet", FixupEventQueueGet);

            OnNewRegion += RegionAdded;
            //OnHandshake += RegionManager_OnHandshake;
        }

        //private void RegionManager_OnHandshake(RegionProxy region)
        //{
        //    Frame.SayToUser(string.Format("The owner of {0} is {1}", region.RegionName, region.RegionOwner));
        //}

        private void RegionAdded(RegionProxy region)
        {
            OpenMetaverse.Logger.Log("Creating proxy for " + region.RemoteEndPoint + " at " + region.LocalEndPoint, Helpers.LogLevel.Info);
            //OpenMetaverse.Logger.Log(string.Format("Region added: {0}", region.RemoteEndPoint), Helpers.LogLevel.Info);
        }

        private Packet HandleHandshake(Packet packet, RegionProxy sim)
        {
            RegionHandshakePacket handshake = (RegionHandshakePacket)packet;
            sim.Name = Utils.BytesToString(handshake.RegionInfo.SimName);

            sim.ID = handshake.RegionInfo2.RegionID;

            sim.Owner = handshake.RegionInfo.SimOwner;

            //Frame.SayToUser(sim.RegionID.ToString());

            OnHandshake?.Invoke(sim);

            //Frame.SayToUser(string.Format("Region name set to {0} for {1}", sim.RegionName, sim.RemoteEndPoint));

            return packet;
        }

        public void Start()
        {
            RunSimProxy();

            CapProxyURL = Frame.HTTP.ProxyURI + "capproxy/";
        }

        private void FixupLoginResponse(XmlRpcResponse response)
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

            OpenMetaverse.Logger.Log("Fixup login for simproxy", Helpers.LogLevel.Info);

            if (responseData.Contains("sim_ip") && responseData.Contains("sim_port"))
            {
                IPEndPoint realSim = new IPEndPoint(IPAddress.Parse((string)responseData["sim_ip"]), Convert.ToUInt16(responseData["sim_port"]));
                RegionProxy fakeSim = ProxySim(realSim);

                if(responseData.Contains("region_x") && responseData.Contains("region_y"))
                {
                    fakeSim.Handle = Utils.UIntsToLong(uint.Parse(responseData["region_x"].ToString()), uint.Parse(responseData["region_y"].ToString()));
                }

                IPEndPoint fakeAddress = fakeSim.LocalEndPoint;
                responseData["sim_ip"] = fakeAddress.Address.ToString();
                responseData["sim_port"] = fakeAddress.Port;
                CurrentSim = fakeSim;

                if (responseData.Contains("seed_capability"))
                {
                    CapInfo info = new CapInfo((string)responseData["seed_capability"], CurrentSim, "SeedCapability");
                    info.AddDelegate(new CapsDelegate(FixupSeedCapsResponse));
                    info.AddDelegate(new CapsDelegate(KnownCapDelegate));
                    KnownCaps[(string)responseData["seed_capability"]] = info;
                    responseData["seed_capability"] = CapProxyURL + responseData["seed_capability"];
                    OpenMetaverse.Logger.Log("Seed cap URL replaced with: " + responseData["seed_capability"], Helpers.LogLevel.Info);
                }
            }
        }

        // InitializeSimProxy: initialize the sim proxy
        private void InitializeSimProxy()
        {
            InitializeAddressCheckers();

            simFacingSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            simFacingSocket.Bind(new IPEndPoint(Frame.Config.remoteFacingAddress, 0));
            Reset();
        }

        // Reset: start a new session
        private void Reset()
        {
            foreach (RegionProxy simProxy in simProxies.Values)
                simProxy.Reset();
        }

        private byte[] receiveBuffer = new byte[8192];
        private byte[] zeroBuffer = new byte[8192];
        private EndPoint remoteEndPoint = (EndPoint)new IPEndPoint(IPAddress.Any, 0);

        // RunSimProxy: start listening for packets from remote sims
        private void RunSimProxy()
        {
            simFacingSocket.BeginReceiveFrom(receiveBuffer, 0, receiveBuffer.Length, SocketFlags.None, ref remoteEndPoint, new AsyncCallback(ReceiveFromSim), null);
        }

        // ReceiveFromSim: packet received from a remote sim
        private void ReceiveFromSim(IAsyncResult ar)
        {
            lock (this)
                try
                {
                    //if (!simFacingSocket.Connected) return;
                    // pause listening and fetch the packet
                    bool needsZero = false;
                    bool needsCopy = true;
                    int length;
                    length = simFacingSocket.EndReceiveFrom(ar, ref remoteEndPoint);

                    if (proxyHandlers.ContainsKey(remoteEndPoint))
                    {
                        // find the proxy responsible for forwarding this packet
                        RegionProxy simProxy = (RegionProxy)proxyHandlers[remoteEndPoint];

                        // interpret the packet according to the SL protocol
                        Packet packet;
                        int end = length - 1;

                        packet = Packet.BuildPacket(receiveBuffer, ref end, zeroBuffer);

                        // check for ACKs we're waiting for
                        packet = simProxy.CheckAcks(packet, Direction.Incoming, ref length, ref needsCopy);

                        // modify sequence numbers to account for injections
                        uint oldSequence = packet.Header.Sequence;
                        packet = simProxy.ModifySequence(packet, Direction.Incoming, ref length, ref needsCopy);

                        // keep track of sequence numbers
                        if (packet.Header.Sequence > simProxy.incomingSequence)
                            simProxy.incomingSequence = packet.Header.Sequence;

                        // check the packet for addresses that need proxying
                        if (incomingCheckers.ContainsKey(packet.Type))
                        {
                            /* if (needsZero) {
                                length = Helpers.ZeroDecode(packet.Header.Data, length, zeroBuffer);
                                packet.Header.Data = zeroBuffer;
                                needsZero = false;
                            } */

                            Packet newPacket = ((AddressChecker)incomingCheckers[packet.Type])(packet);
                            SwapPacket(packet, newPacket);
                            packet = newPacket;
                            needsCopy = false;
                        }

                        // pass the packet to any callback delegates
                        if (incomingDelegates.ContainsKey(packet.Type))
                        {
                            /* if (needsZero) {
                                length = Helpers.ZeroDecode(packet.Header.Data, length, zeroBuffer);
                                packet.Header.Data = zeroBuffer;
                                needsCopy = true;
                            } */

                            if (packet.Header.AckList != null && needsCopy)
                            {
                                uint[] newAcks = new uint[packet.Header.AckList.Length];
                                Array.Copy(packet.Header.AckList, 0, newAcks, 0, newAcks.Length);
                                packet.Header.AckList = newAcks; // FIXME
                            }

                            try
                            {
                                Packet newPacket = callDelegates(incomingDelegates, packet, simProxy);
                                if (newPacket == null)
                                {
                                    if (packet.Header.Reliable)
                                        simProxy.Inject(SpoofAck(oldSequence), Direction.Outgoing);

                                    if (packet.Header.AppendedAcks)
                                        packet = SeparateAck(packet);
                                    else
                                        packet = null;
                                }
                                else
                                {
                                    bool oldReliable = packet.Header.Reliable;
                                    bool newReliable = newPacket.Header.Reliable;
                                    if (oldReliable && !newReliable)
                                        simProxy.Inject(SpoofAck(oldSequence), Direction.Outgoing);
                                    else if (!oldReliable && newReliable)
                                        simProxy.WaitForAck(packet, Direction.Incoming);

                                    SwapPacket(packet, newPacket);
                                    packet = newPacket;
                                }
                            }
                            catch (Exception e)
                            {
                                OpenMetaverse.Logger.Log("Exception in incoming delegate", Helpers.LogLevel.Error, e);
                            }

                            if (packet != null)
                                simProxy.SendPacket(packet, false);
                        }
                        else
                            simProxy.SendPacket(packet, needsZero);
                    }
                    else
                        // ignore packets from unknown peers
                        OpenMetaverse.Logger.Log("Dropping packet from unknown peer " + remoteEndPoint, Helpers.LogLevel.Warning);
                }
                catch (Exception e)
                {
                    OpenMetaverse.Logger.Log("Error processing incoming packet from simulator", Helpers.LogLevel.Error, e);
                }
                finally
                {
                    // resume listening
                    try
                    {
                        simFacingSocket.BeginReceiveFrom(receiveBuffer, 0, receiveBuffer.Length, SocketFlags.None,
                            ref remoteEndPoint, new AsyncCallback(ReceiveFromSim), null);
                    }
                    catch (Exception e)
                    {
                        OpenMetaverse.Logger.Log("Listener Socket Exception", Helpers.LogLevel.Error, e);
                    }
                }
        }

        // SendPacket: send a packet to a sim from our fake client endpoint
        public void SendPacket(Packet packet, IPEndPoint endPoint, bool skipZero)
        {

            byte[] buffer = packet.ToBytes();
            if (skipZero || !packet.Header.Zerocoded)
                simFacingSocket.SendTo(buffer, buffer.Length, SocketFlags.None, endPoint);
            else
            {
                int zeroLength = Helpers.ZeroEncode(buffer, buffer.Length, zeroBuffer);
                simFacingSocket.SendTo(zeroBuffer, zeroLength, SocketFlags.None, endPoint);
            }
        }

        // SpoofAck: create an ACK for the given packet
        public Packet SpoofAck(uint sequence)
        {
            PacketAckPacket spoof = new PacketAckPacket();
            spoof.Packets = new PacketAckPacket.PacketsBlock[1];
            spoof.Packets[0] = new PacketAckPacket.PacketsBlock();
            spoof.Packets[0].ID = sequence;
            return (Packet)spoof;
        }

        // SeparateAck: create a standalone PacketAck for packet's appended ACKs
        public Packet SeparateAck(Packet packet)
        {
            PacketAckPacket seperate = new PacketAckPacket();
            seperate.Packets = new PacketAckPacket.PacketsBlock[packet.Header.AckList.Length];

            for (int i = 0; i < packet.Header.AckList.Length; ++i)
            {
                seperate.Packets[i] = new PacketAckPacket.PacketsBlock();
                seperate.Packets[i].ID = packet.Header.AckList[i];
            }

            Packet ack = seperate;
            ack.Header.Sequence = packet.Header.Sequence;
            return ack;
        }

        // SwapPacket: copy the sequence number and appended ACKs from one packet to another
        public static void SwapPacket(Packet oldPacket, Packet newPacket)
        {
            newPacket.Header.Sequence = oldPacket.Header.Sequence;

            int oldAcks = oldPacket.Header.AppendedAcks ? oldPacket.Header.AckList.Length : 0;
            int newAcks = newPacket.Header.AppendedAcks ? newPacket.Header.AckList.Length : 0;

            if (oldAcks != 0 || newAcks != 0)
            {
                uint[] newAckList = new uint[oldAcks];
                Array.Copy(oldPacket.Header.AckList, 0, newAckList, 0, oldAcks);

                newPacket.Header.AckList = newAckList;
                newPacket.Header.AppendedAcks = oldPacket.Header.AppendedAcks;
            }
        }

        // ProxySim: return the proxy for the specified sim, creating it if it doesn't exist
        public RegionProxy ProxySim(IPEndPoint simEndPoint)
        {
            int index = Regions.FindIndex(x => x.RemoteEndPoint.Equals(simEndPoint));
            if (index != -1)
                return Regions[index];
            else
            {
                RegionProxy simProxy = new RegionProxy(Frame, simEndPoint);
                IPEndPoint fakeSim = simProxy.LocalEndPoint;

                OpenMetaverse.Logger.Log("====================================================", Helpers.LogLevel.Info);
                OpenMetaverse.Logger.Log("Created proxy for " + simEndPoint + " at " + fakeSim, Helpers.LogLevel.Info);
                OpenMetaverse.Logger.Log("====================================================", Helpers.LogLevel.Info);

                OnNewRegion?.Invoke(simProxy);

                Regions.Add(simProxy);

                simProxy.Run();

                return simProxy;
            }
        }
        // AddHandler: remember which sim proxy corresponds to a given sim
        public void AddHandler(EndPoint endPoint, RegionProxy proxy)
        {
            proxyHandlers.Add(endPoint, proxy);
        }

        // Checkers swap proxy addresses in for real addresses.  A few constraints:
        //   - Checkers must not alter the incoming packet.
        //   - Checkers must return a freshly built packet, even if nothing's changed.
        //   - The incoming packet's buffer may be longer than the length of the data it contains.
        //   - The incoming packet's buffer must not be used after the checker returns.
        // This is all because checkers may be operating on data that's still in a scratch buffer.
        public delegate Packet AddressChecker(Packet packet);

        public Dictionary<PacketType, AddressChecker> incomingCheckers { get; private set; } = new Dictionary<PacketType, AddressChecker>();
        public Dictionary<PacketType, AddressChecker> outgoingCheckers { get; private set; } = new Dictionary<PacketType, AddressChecker>();

        // InitializeAddressCheckers: initialize delegates that check packets for addresses that need proxying
        private void InitializeAddressCheckers()
        {
            // TODO: what do we do with mysteries and empty IPs?
            AddMystery(PacketType.OpenCircuit);
            //AddMystery(PacketType.AgentPresenceResponse);

            incomingCheckers.Add(PacketType.TeleportFinish, new AddressChecker(CheckTeleportFinish));
            incomingCheckers.Add(PacketType.CrossedRegion, new AddressChecker(CheckCrossedRegion));
            incomingCheckers.Add(PacketType.EnableSimulator, new AddressChecker(CheckEnableSimulator));
            //incomingCheckers.Add("UserLoginLocationReply", new AddressChecker(CheckUserLoginLocationReply));
        }

        // AddMystery: add a checker delegate that logs packets we're watching for development purposes
        private void AddMystery(PacketType type)
        {
            incomingCheckers.Add(type, new AddressChecker(LogIncomingMysteryPacket));
            outgoingCheckers.Add(type, new AddressChecker(LogOutgoingMysteryPacket));
        }

        // GenericCheck: replace the sim address in a packet with our proxy address
        private void GenericCheck(ref uint simIP, ref ushort simPort, ref string simCaps, bool active, ulong handle)
        {
            IPAddress sim_ip = new IPAddress((long)simIP);

            IPEndPoint realSim = new IPEndPoint(sim_ip, Convert.ToInt32(simPort));
            RegionProxy fakeSim = ProxySim(realSim);

            if (handle != 0) fakeSim.Handle = handle;

            IPEndPoint fakeAddress = fakeSim.LocalEndPoint;

            simPort = (ushort)fakeAddress.Port;
            byte[] bytes = fakeAddress.Address.GetAddressBytes();
            simIP = Utils.BytesToUInt(bytes);
            if (simCaps != null && simCaps.Length > 0)
            {
                CapInfo info = new CapInfo(simCaps, fakeSim, "SeedCapability");
                info.AddDelegate(new CapsDelegate(FixupSeedCapsResponse));
                info.AddDelegate(new CapsDelegate(KnownCapDelegate));

                lock (this)
                {
                    KnownCaps[simCaps] = info;
                }
                simCaps = CapProxyURL + simCaps;
            }

            if (active)
            {
                CurrentSim = fakeSim;

                OpenMetaverse.Logger.Log("Changed active region to: " + CurrentSim.RemoteEndPoint.ToString(), Helpers.LogLevel.Info);
                //Frame.SayToUser("Region changed to " + CurrentRegion.RemoteEndPoint.ToString());

                OnRegionChanged?.Invoke(CurrentSim);
            }
        }

        // CheckTeleportFinish: check TeleportFinish packets
        private Packet CheckTeleportFinish(Packet packet)
        {
            TeleportFinishPacket tfp = (TeleportFinishPacket)packet;
            string simCaps = Encoding.UTF8.GetString(tfp.Info.SeedCapability).Replace("\0", "");
            GenericCheck(ref tfp.Info.SimIP, ref tfp.Info.SimPort, ref simCaps, true, tfp.Info.RegionHandle);
            tfp.Info.SeedCapability = Utils.StringToBytes(simCaps);
            return (Packet)tfp;
        }

        // CheckEnableSimulator: check EnableSimulator packets
        private Packet CheckEnableSimulator(Packet packet)
        {
            EnableSimulatorPacket esp = (EnableSimulatorPacket)packet;
            string simCaps = null;
            GenericCheck(ref esp.SimulatorInfo.IP, ref esp.SimulatorInfo.Port, ref simCaps, false, esp.SimulatorInfo.Handle);
            return (Packet)esp;
        }

        // CheckCrossedRegion: check CrossedRegion packets
        private Packet CheckCrossedRegion(Packet packet)
        {
            CrossedRegionPacket crp = (CrossedRegionPacket)packet;
            string simCaps = Encoding.UTF8.GetString(crp.RegionData.SeedCapability).Replace("\0", "");
            GenericCheck(ref crp.RegionData.SimIP, ref crp.RegionData.SimPort, ref simCaps, true, crp.RegionData.RegionHandle);
            crp.RegionData.SeedCapability = Utils.StringToBytes(simCaps);
            return (Packet)crp;
        }

        // LogPacket: log a packet dump
        private Packet LogPacket(Packet packet, string type)
        {
            OpenMetaverse.Logger.Log(type + " packet:\n" + packet, Helpers.LogLevel.Info);
            return packet;
        }

        // LogIncomingMysteryPacket: log an incoming packet we're watching for development purposes
        private Packet LogIncomingMysteryPacket(Packet packet)
        {
            return LogPacket(packet, "incoming mystery");
        }

        // LogOutgoingMysteryPacket: log an outgoing packet we're watching for development purposes
        private Packet LogOutgoingMysteryPacket(Packet packet)
        {
            return LogPacket(packet, "outgoing mystery");
        }



        // AddDelegate: add callback packetDelegate for packets of type packetName going direction
        public void AddDelegate(PacketType packetType, Direction direction, PacketDelegate packetDelegate)
        {
            lock (this)
            {
                Dictionary<PacketType, List<PacketDelegate>> delegates = (direction == Direction.Incoming ? incomingDelegates : outgoingDelegates);
                if (!delegates.ContainsKey(packetType))
                {
                    delegates[packetType] = new List<PacketDelegate>();
                }
                List<PacketDelegate> delegateArray = delegates[packetType];
                if (!delegateArray.Contains(packetDelegate))
                {
                    delegateArray.Add(packetDelegate);
                }
            }
        }

        // RemoveDelegate: remove callback for packets of type packetName going direction
        public void RemoveDelegate(PacketType packetType, Direction direction, PacketDelegate packetDelegate)
        {
            lock (this)
            {
                Dictionary<PacketType, List<PacketDelegate>> delegates = (direction == Direction.Incoming ? incomingDelegates : outgoingDelegates);
                if (!delegates.ContainsKey(packetType))
                {
                    return;
                }
                List<PacketDelegate> delegateArray = delegates[packetType];
                if (delegateArray.Contains(packetDelegate))
                {
                    delegateArray.Remove(packetDelegate);
                }
            }
        }

        public Packet callDelegates(Dictionary<PacketType, List<PacketDelegate>> delegates, Packet packet, RegionProxy sim)
        {
            PacketType origType = packet.Type;
            foreach (PacketDelegate del in delegates[origType])
            {
                try { packet = del(packet, sim); }
                catch (Exception ex) { OpenMetaverse.Logger.Log("Error in packet delegate", Helpers.LogLevel.Warning, ex); }

                // FIXME: how should we handle the packet type changing?
                if (packet == null || packet.Type != origType) break;
            }
            return packet;
        }


        // InjectPacket: send packet to the client or server when direction is Incoming or Outgoing, respectively
        public void InjectPacket(Packet packet, Direction direction)
        {
            lock (this)
            {
                if (CurrentSim == null)
                {
                    // no active circuit; queue the packet for injection once we have one
                    List<Packet> queue = direction == Direction.Incoming ? queuedIncomingInjections : queuedOutgoingInjections;
                    queue.Add(packet);
                }
                else
                    // tell the active sim proxy to inject the packet
                    CurrentSim.Inject(packet, direction);
            }
        }



        //////////////////////////////////////////////// cap shit

        static List<string> BinaryResponseCaps = new List<string>()
        {
            "ViewerAsset",
            "GetTexture",
            "GetMesh",
            "GetMesh2"
        };

        private void ProxyCaps(string uri, string method, NetworkStream netStream, Dictionary<string, string> headers, byte[] content)
        {

            Match match = new Regex(@"^(/capproxy/https?)://([^:/]+)(:\d+)?(/.*)$").Match(uri);
            if (!match.Success)
            {
                OpenMetaverse.Logger.Log("Malformed proxy URI: " + uri, Helpers.LogLevel.Error);
                byte[] wr = Encoding.ASCII.GetBytes("HTTP/1.0 404 Not Found\r\nContent-Length: 0\r\n\r\n");
                netStream.Write(wr, 0, wr.Length);
                return;
            }

            uri = uri.Substring(10);

            CapInfo cap = null;
            lock (this)
            {
                string capuri = Regex.Replace(uri, @"/?\?.*$", string.Empty);

                //OpenMetaverse.Logger.Log(capuri, Helpers.LogLevel.Info);

                if (KnownCaps.ContainsKey(capuri))
                {
                    cap = KnownCaps[capuri];
                }
            }

            CapsRequest capReq = null; bool shortCircuit = false; bool requestFailed = false;
            if (cap != null)
            {
                capReq = new CapsRequest(cap);

                capReq.Method = method;

                if (cap.ReqFmt == CapsDataFormat.OSD)
                {
                    capReq.Request = OSDParser.DeserializeLLSDXml(content);
                }
                else
                {
                    capReq.Request = OSDParser.DeserializeLLSDXml(content);
                }

                capReq.RawRequest = content;
                capReq.FullUri = uri;

                foreach (CapsDelegate d in cap.GetDelegates())
                {
                    if (d(capReq, CapsStage.Request)) { shortCircuit = true; break; }
                }
            }

            byte[] respBuf = null;
            string consoleMsg = String.Empty;

            bool reqrange = false;
            bool resprange = false;

            if (shortCircuit)
            {
                byte[] wr = Encoding.UTF8.GetBytes("HTTP/1.0 200 OK\r\n");
                netStream.Write(wr, 0, wr.Length);
            }
            else
            {
                HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(uri);
                req.KeepAlive = false;

                foreach (string header in headers.Keys)
                {
                    if (header == "connection" ||
                       header == "content-length" || header == "date" || header == "expect" ||
                       header == "host" || header == "if-modified-since" || header == "referer" ||
                       header == "transfer-encoding" || header == "user-agent" ||
                       header == "proxy-connection" || header == "accept-encoding")
                    {
                        // can't touch these!
                    }
                    else if (header == "accept")
                    {
                        req.Accept = headers["accept"];
                    }
                    else if (header == "content-type")
                    {
                        req.ContentType = headers["content-type"];
                    }
                    else if (header == "range")
                    {
                        string rangeHeader = headers[header];
                        string[] parts = rangeHeader.Split('=');

                        if (parts.Length == 2)
                        {
                            string[] range = parts[1].Split('-');
                            int from;
                            int to;

                            if (range.Length == 2)
                            {
                                if (!int.TryParse(range[0], out from))
                                    from = 0;
                                if (int.TryParse(range[1], out to))
                                    req.AddRange(parts[0], from, to);
                                else
                                    req.AddRange(parts[0], from);

                                reqrange = true;
                            }
                            else if (range.Length == 1 && int.TryParse(range[0], out to))
                            {
                                req.AddRange(parts[0], to);
                                reqrange = true;
                            }
                        }
                    }
                    else
                    {
                        req.Headers[header] = headers[header];
                    }
                }

                if (capReq != null)
                {
                    capReq.RequestHeaders = req.Headers;
                }

                req.Method = method;

                // can't do gets on requests with a content body
                // without throwing a protocol exception. So force it to post 
                // incase our parser stupidly set it to GET due to the viewer 
                // doing something stupid like sending an empty request
                if (content.Length > 0 && req.Method.ToLower() == "get")
                    req.Method = "POST";

                req.ContentLength = content.Length;

                HttpWebResponse resp;
                try
                {
                    if (content.Length > 0)
                    {
                        Stream reqStream = req.GetRequestStream();
                        reqStream.Write(content, 0, content.Length);
                        reqStream.Close();
                    }
                    else if (cap == null)
                    {
                        OpenMetaverse.Logger.Log(string.Format("{0} {1}", req.Method, req.Address.ToString()), Helpers.LogLevel.Info);
                    }
                    resp = (HttpWebResponse)req.GetResponse();
                }

                catch (WebException e)
                {
                    if (e.Status == WebExceptionStatus.Timeout || e.Status == WebExceptionStatus.SendFailure)
                    {
                        OpenMetaverse.Logger.Log("Request timeout", Helpers.LogLevel.Warning, e);
                        byte[] wr = Encoding.ASCII.GetBytes("HTTP/1.0 504 Proxy Request Timeout\r\nContent-Length: 0\r\n\r\n");
                        netStream.Write(wr, 0, wr.Length);
                        return;
                    }
                    else if (e.Status == WebExceptionStatus.ProtocolError && e.Response != null)
                    {
                        resp = (HttpWebResponse)e.Response; requestFailed = true;
                    }
                    else
                    {
                        OpenMetaverse.Logger.Log("Request error", Helpers.LogLevel.Error, e);
                        byte[] wr = Encoding.ASCII.GetBytes("HTTP/1.0 502 Gateway Error\r\nContent-Length: 0\r\n\r\n"); // FIXME
                        netStream.Write(wr, 0, wr.Length);
                        return;
                    }
                }

                try
                {
                    Stream respStream = resp.GetResponseStream();
                    int read;
                    int length = 0;
                    respBuf = new byte[256];

                    do
                    {
                        read = respStream.Read(respBuf, length, 256);
                        if (read > 0)
                        {
                            length += read;
                            Array.Resize(ref respBuf, length + 256);
                        }
                    } while (read > 0);

                    Array.Resize(ref respBuf, length);

                    if (capReq != null && !requestFailed)
                    {
                        if (cap.RespFmt == CapsDataFormat.OSD)
                        {
                            capReq.Response = OSDParser.DeserializeLLSDXml(respBuf);
                        }
                        else
                        {
                            capReq.Response = OSDParser.DeserializeLLSDXml(respBuf);
                        }
                        capReq.RawResponse = respBuf;

                    }

                    consoleMsg += "Response from " + uri + "\nStatus: " + (int)resp.StatusCode + " " + resp.StatusDescription + "\n";

                    {
                        byte[] wr = Encoding.UTF8.GetBytes("HTTP/1.1 " + (int)resp.StatusCode + " " + resp.StatusDescription + "\r\n");
                        netStream.Write(wr, 0, wr.Length);
                    }

                    if (capReq != null)
                        capReq.ResponseHeaders = resp.Headers;

                    for (int i = 0; i < resp.Headers.Count; i++)
                    {
                        string key = resp.Headers.Keys[i];
                        string val = resp.Headers[i];
                        string lkey = key.ToLower();
                        //                        if (lkey != "content-length" && lkey != "transfer-encoding" && lkey != "connection")
                        if (lkey != "content-length" && lkey != "transfer-encoding")
                        {
                            consoleMsg += key + ": " + val + "\n";
                            byte[] wr = Encoding.UTF8.GetBytes(key + ": " + val + "\r\n");
                            netStream.Write(wr, 0, wr.Length);
                        }
                        if (lkey == "content-range")
                        {
                            resprange = true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    // TODO: Should we handle this somehow?
                    OpenMetaverse.Logger.DebugLog("Failed writing output: " + ex.Message);
                    return;
                }
            }

            if (reqrange && !resprange)
            {

            }

            if (cap != null && !requestFailed && (!capReq.Response.ToString().Equals("undef") || respBuf.Length > 0))
            {
                foreach (CapsDelegate d in cap.GetDelegates())
                {
                    try
                    {
                        if (d(capReq, CapsStage.Response)) { break; }
                    }
                    catch (InvalidCastException ex)
                    {
                        OpenMetaverse.Logger.Log("Invalid Cast thrown trying to cast OSD to OSDMap: \n'" + capReq.Response.AsString() + "' Length=" + capReq.RawResponse.Length.ToString() + "\n",
                            Helpers.LogLevel.Error, ex);
                    }
                    catch (Exception ex)
                    {
                        OpenMetaverse.Logger.Log("Error firing delegate", Helpers.LogLevel.Error, ex);
                    }
                }
                if (!capReq.Response.ToString().Equals("undef"))
                {
                    if (cap.RespFmt == CapsDataFormat.OSD)
                    {
                        respBuf = OSDParser.SerializeLLSDXmlBytes((OSD)capReq.Response);
                    }
                    else
                    {
                        respBuf = OSDParser.SerializeLLSDXmlBytes(capReq.Response);
                    }
                }
            }


            string respString;
            if (cap == null || cap.RespFmt == CapsDataFormat.Binary)
            {
                respString = "<data>";
            }
            else
            {
                respString = Encoding.UTF8.GetString(respBuf);
            }

            consoleMsg += "\n" + respString + "\n--------";
            OpenMetaverse.Logger.Log(consoleMsg, Helpers.LogLevel.Debug);
            OpenMetaverse.Logger.Log("Fixed-up response:\n" + respString + "\n--------", Helpers.LogLevel.Debug);

            try
            {
                byte[] wr2 = Encoding.UTF8.GetBytes("Content-Length: " + respBuf.Length + "\r\n\r\n");
                netStream.Write(wr2, 0, wr2.Length);

                netStream.Write(respBuf, 0, respBuf.Length);
            }
            catch (SocketException) { }
            catch (IOException) { }
            catch (Exception e)
            {
                OpenMetaverse.Logger.Log("Exception: Error writing to stream " + e, Helpers.LogLevel.Error, e);
            }

            return;
        }

        private bool FixupSeedCapsResponse(CapsRequest capReq, CapsStage stage)
        {
            if (stage != CapsStage.Response) return false;

            capReq.Info.Sim.Caps.Clear();

            OSDMap nm = new OSDMap();

            if (capReq.Response.Type == OSDType.Map)
            {
                OSDMap m = (OSDMap)capReq.Response;

                foreach (string key in m.Keys)
                {
                    string val = m[key].AsString();

                    if (!string.IsNullOrEmpty(val))
                    {
                        if(KnownCapsDelegates.ContainsKey(key))
                        {
                            if (!KnownCaps.ContainsKey(val))
                            {
                                CapsDataFormat resFmt = BinaryResponseCaps.Contains(key) ? CapsDataFormat.Binary : CapsDataFormat.OSD;
                                CapsDataFormat reqFmt = CapsDataFormat.OSD;
                                CapInfo newCap = new CapInfo(val, capReq.Info.Sim, key, reqFmt, resFmt);
                                newCap.AddDelegate(new CapsDelegate(KnownCapDelegate));
                                lock (this) { KnownCaps[val] = newCap; }
                            }
                            nm[key] = OSD.FromString(CapProxyURL + val);

                            OpenMetaverse.Logger.Log(string.Format("{0} replaced with {1}", key, nm[key]), Helpers.LogLevel.Info);
                        }
                        else
                        {
                            nm[key] = OSD.FromString(val);
                        }

                        capReq.Info.Sim.Caps.Add(key, val);

                    }
                    else
                    {
                        nm[key] = OSD.FromString(val);
                    }
                }
            }

            capReq.Response = nm;
            return false;
        }
        private bool KnownCapDelegate(CapsRequest capReq, CapsStage stage)
        {
            lock (this)
            {
                if (!KnownCapsDelegates.ContainsKey(capReq.Info.CapType))
                    return false;

                if (stage == CapsStage.Response)
                {
                    if (capReq.Response != null && capReq.Response is OSDMap)
                    {
                        OSDMap map = (OSDMap)capReq.Response;

                        if (map.ContainsKey("uploader"))
                        {
                            string val = map["uploader"].AsString();

                            if (!KnownCaps.ContainsKey(val))
                            {
                                CapInfo newCap = new CapInfo(val, capReq.Info.Sim, capReq.Info.CapType, CapsDataFormat.Binary, CapsDataFormat.OSD);
                                newCap.AddDelegate(new CapsDelegate(KnownCapDelegate));
                                lock (this) { KnownCaps[val] = newCap; }
                            }

                            map["uploader"] = OSD.FromString(CapProxyURL + val);
                        }
                    }
                }

                List<CapsDelegate> delegates = KnownCapsDelegates[capReq.Info.CapType];

                foreach (CapsDelegate d in delegates)
                {
                    if (d(capReq, stage)) { return true; }
                }
            }

            return false;
        }


        private bool FixupEventQueueGet(CapsRequest capReq, CapsStage stage)
        {
            if (stage != CapsStage.Response) return false;

            OSDMap map = null;
            if (capReq.Response is OSDMap)
                map = (OSDMap)capReq.Response;
            else return false;

            OSDArray array = null;
            if (map.ContainsKey("events") && map["events"] is OSDArray)
                array = (OSDArray)map["events"];
            else
                return false;

            for (int i = 0; i < array.Count; i++)
            {
                OSDMap evt = (OSDMap)array[i];

                string message = evt["message"].AsString();
                OSDMap body = (OSDMap)evt["body"];

                if (message == "TeleportFinish" || message == "CrossedRegion")
                {
                    OSDMap info = null;
                    if (message == "TeleportFinish")
                        info = (OSDMap)(((OSDArray)body["Info"])[0]);
                    else
                        info = (OSDMap)(((OSDArray)body["RegionData"])[0]);
                    byte[] bytes = info["SimIP"].AsBinary();
                    uint simIP = Utils.BytesToUInt(bytes);
                    ushort simPort = (ushort)info["SimPort"].AsInteger();
                    string capsURL = info["SeedCapability"].AsString();

                    ulong handle = info["RegionHandle"].AsULong();

                    GenericCheck(ref simIP, ref simPort, ref capsURL, capReq.Info.Sim.RemoteEndPoint == CurrentSim.RemoteEndPoint, handle);

                    info["SeedCapability"] = OSD.FromString(capsURL);
                    bytes[0] = (byte)(simIP % 256);
                    bytes[1] = (byte)((simIP >> 8) % 256);
                    bytes[2] = (byte)((simIP >> 16) % 256);
                    bytes[3] = (byte)((simIP >> 24) % 256);
                    info["SimIP"] = OSD.FromBinary(bytes);
                    info["SimPort"] = OSD.FromInteger(simPort);
                }
                else if (message == "EnableSimulator")
                {
                    OSDMap info = null;
                    info = (OSDMap)(((OSDArray)body["SimulatorInfo"])[0]);
                    byte[] bytes = info["IP"].AsBinary();
                    uint IP = Utils.BytesToUInt(bytes);
                    ushort Port = (ushort)info["Port"].AsInteger();
                    string capsURL = null;
                    ulong handle = info["RegionHandle"].AsULong();

                    //GenericCheck(ref IP, ref Port, ref capsURL, capReq.Info.Sim.RemoteEndPoint == CurrentRegion.RemoteEndPoint);
                    GenericCheck(ref IP, ref Port, ref capsURL, false, handle);

                    bytes[0] = (byte)(IP % 256);
                    bytes[1] = (byte)((IP >> 8) % 256);
                    bytes[2] = (byte)((IP >> 16) % 256);
                    bytes[3] = (byte)((IP >> 24) % 256);
                    info["IP"] = OSD.FromBinary(bytes);
                    info["Port"] = OSD.FromInteger(Port);
                }
                else if (message == "EstablishAgentCommunication")
                {
                    string ipAndPort = body["sim-ip-and-port"].AsString();
                    string[] pieces = ipAndPort.Split(':');
                    byte[] bytes = IPAddress.Parse(pieces[0]).GetAddressBytes();
                    uint simIP = Utils.BytesToUInt(bytes);
                    ushort simPort = (ushort)Convert.ToInt32(pieces[1]);

                    string capsURL = body["seed-capability"].AsString();

                    OpenMetaverse.Logger.Log("DEBUG: Got EstablishAgentCommunication for " + ipAndPort + " with seed cap " + capsURL, Helpers.LogLevel.Debug);

                    GenericCheck(ref simIP, ref simPort, ref capsURL, false, 0);
                    body["seed-capability"] = OSD.FromString(capsURL);
                    string ipport = String.Format("{0}:{1}", new IPAddress(simIP), simPort);
                    body["sim-ip-and-port"] = OSD.FromString(ipport);

                    OpenMetaverse.Logger.Log("DEBUG: Modified EstablishAgentCommunication to " + body["sim-ip-and-port"].AsString() + " with seed cap " + capsURL, Helpers.LogLevel.Debug);
                }
            }
            return false;
        }


        public void AddCapsDelegate(string CapName, CapsDelegate capsDelegate)
        {
            lock (this)
            {
                if (!KnownCapsDelegates.ContainsKey(CapName))
                {
                    KnownCapsDelegates[CapName] = new List<CapsDelegate>();
                }
                List<CapsDelegate> delegateArray = KnownCapsDelegates[CapName];
                if (!delegateArray.Contains(capsDelegate))
                {
                    delegateArray.Add(capsDelegate);
                }
            }
        }

        public void RemoveCapRequestDelegate(string CapName, CapsDelegate capsDelegate)
        {
            lock (this)
            {

                if (!KnownCapsDelegates.ContainsKey(CapName))
                {
                    return;
                }
                List<CapsDelegate> delegateArray = KnownCapsDelegates[CapName];
                if (delegateArray.Contains(capsDelegate))
                {
                    delegateArray.Remove(capsDelegate);
                }
            }
        }


        // SimProxy: proxy for a single simulator
        public class RegionProxy
        {
            public Dictionary<uint, Avatar> ObjectsAvatars = new Dictionary<uint, Avatar>();

            public InternalDictionary<uint, Primitive> ObjectsPrimitives = new InternalDictionary<uint, Primitive>();

            internal InternalDictionary<UUID, Vector3> avatarPositions = new InternalDictionary<UUID, Vector3>();

            public InternalDictionary<UUID, Vector3> AvatarPositions { get { return avatarPositions; } }

            /// <summary>AvatarPositions key representing TrackAgent target</summary>
            internal UUID preyID = UUID.Zero;

            //private ProxyConfig proxyConfig;
            public IPEndPoint RemoteEndPoint { get; private set; }

            // LocalEndPoint: return the endpoint that the client should communicate with
            public IPEndPoint LocalEndPoint { get { return (IPEndPoint)socket.LocalEndPoint; } }


            public string GridURI { get; set; } = string.Empty;

            public string ProfileServerURI { get; set; } = string.Empty;
            public string AssetServerURI { get; set; } = string.Empty;
            public string IMServerURI { get; set; } = string.Empty;
            public string InvetoryServerURI { get; set; } = string.Empty;
            public string FriendsServerURI { get; set; } = string.Empty;
            public string GatekeeperURI { get; set; } = string.Empty;
            public string HomeURI { get; set; } = string.Empty;


            public string HostUri { get; set; } = string.Empty;


            private Socket socket;
            public uint incomingSequence;
            public uint outgoingSequence;
            private List<uint> incomingInjections;
            private List<uint> outgoingInjections;
            private uint incomingOffset = 0;
            private uint outgoingOffset = 0;
            private Dictionary<uint, Packet> incomingAcks;
            private Dictionary<uint, Packet> outgoingAcks;
            private List<uint> incomingSeenAcks;
            private List<uint> outgoingSeenAcks;
            public Dictionary<string, string> Caps { get; private set; } = new Dictionary<string, string>();

            public string Name { get; set; } = string.Empty;
            public UUID ID { get; set; } = UUID.Zero;
            public UUID Owner { get; set; } = UUID.Zero;
            public ulong Handle { get; internal set; } = 0;

            public string SeedCap { get; set; } = string.Empty;

            private ProxyFrame Frame;

            // SimProxy: construct a proxy for a single simulator
            public RegionProxy(ProxyFrame frame, IPEndPoint simEndPoint)
            {
                Frame = frame;
                //this.proxyConfig = proxyConfig;
                RemoteEndPoint = simEndPoint;
                //RemoteEndPoint = new IPEndPoint(simEndPoint.Address, simEndPoint.Port);

                socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                socket.Bind(new IPEndPoint(Frame.Config.clientFacingAddress, 0));
                Frame.Network.AddHandler(RemoteEndPoint, this);
                Reset();
            }

            // Reset: start a new session
            public void Reset()
            {
                // Packet bs
                incomingSequence = 0;
                outgoingSequence = 0;
                incomingInjections = new List<uint>();
                outgoingInjections = new List<uint>();
                incomingAcks = new Dictionary<uint, Packet>();
                outgoingAcks = new Dictionary<uint, Packet>();
                incomingSeenAcks = new List<uint>();
                outgoingSeenAcks = new List<uint>();

                // Reset of the region stuff
                //Caps.Clear();
            }

            // BackgroundTasks: resend unacknowledged packets and keep data structures clean
            private void BackgroundTasks()
            {
                try
                {
                    int tick = 1;
                    int incomingInjectionsPoint = 0;
                    int outgoingInjectionsPoint = 0;
                    int incomingSeenAcksPoint = 0;
                    int outgoingSeenAcksPoint = 0;

                    for (; ; Thread.Sleep(1000)) lock (Frame.Network)
                        {
                            if ((tick = (tick + 1) % 60) == 0)
                            {
                                for (int i = 0; i < incomingInjectionsPoint; ++i)
                                {
                                    incomingInjections.RemoveAt(0);
                                    ++incomingOffset;
                                }
                                incomingInjectionsPoint = incomingInjections.Count;

                                for (int i = 0; i < outgoingInjectionsPoint; ++i)
                                {
                                    outgoingInjections.RemoveAt(0);
                                    ++outgoingOffset;
                                }
                                outgoingInjectionsPoint = outgoingInjections.Count;

                                for (int i = 0; i < incomingSeenAcksPoint; ++i)
                                {
                                    incomingAcks.Remove(incomingSeenAcks[0]);
                                    incomingSeenAcks.RemoveAt(0);
                                }
                                incomingSeenAcksPoint = incomingSeenAcks.Count;

                                for (int i = 0; i < outgoingSeenAcksPoint; ++i)
                                {
                                    outgoingAcks.Remove(outgoingSeenAcks[0]);
                                    outgoingSeenAcks.RemoveAt(0);
                                }
                                outgoingSeenAcksPoint = outgoingSeenAcks.Count;
                            }

                            foreach (uint id in incomingAcks.Keys)
                                if (!incomingSeenAcks.Contains(id))
                                {
                                    Packet packet = (Packet)incomingAcks[id];
                                    packet.Header.Resent = true;
                                    SendPacket(packet, false);
                                }

                            foreach (uint id in outgoingAcks.Keys)
                                if (!outgoingSeenAcks.Contains(id))
                                {
                                    Packet packet = (Packet)outgoingAcks[id];
                                    packet.Header.Resent = true;
                                    Frame.Network.SendPacket(packet, RemoteEndPoint, false);
                                }
                        }
                }
                catch (Exception e)
                {
                    OpenMetaverse.Logger.Log("Exception running BackgroundTasks", Helpers.LogLevel.Error, e);
                }
            }

            private byte[] receiveBuffer = new byte[8192];
            private byte[] zeroBuffer = new byte[8192];
            private EndPoint clientEndPoint = new IPEndPoint(IPAddress.Any, 0);
            bool firstReceive = true;

            // Run: forward packets from the client to the sim
            public void Run()
            {
                Thread backgroundTasks = new Thread(new ThreadStart(BackgroundTasks));
                backgroundTasks.IsBackground = true;
                backgroundTasks.Priority = ThreadPriority.Highest;
                backgroundTasks.Start();
                socket.BeginReceiveFrom(receiveBuffer, 0, receiveBuffer.Length, SocketFlags.None, ref clientEndPoint, new AsyncCallback(ReceiveFromClient), null);
            }

            // ReceiveFromClient: packet received from the client
            private void ReceiveFromClient(IAsyncResult ar)
            {
                lock (Frame.Network)
                {
                    try
                    {
                        // pause listening and fetch the packet
                        bool needsZero = false;
                        bool needsCopy = true;
                        int length = 0;

                        try { length = socket.EndReceiveFrom(ar, ref clientEndPoint); }
                        catch (SocketException) { }

                        if (length != 0)
                        {
                            // interpret the packet according to the SL protocol
                            int end = length - 1;
                            Packet packet = OpenMetaverse.Packets.Packet.BuildPacket(receiveBuffer, ref end, zeroBuffer);

                            //OpenMetaverse.Logger.Log("-> " + packet.Type + " #" + packet.Header.Sequence, Helpers.LogLevel.Debug);

                            // check for ACKs we're waiting for
                            packet = CheckAcks(packet, Direction.Outgoing, ref length, ref needsCopy);

                            // modify sequence numbers to account for injections
                            uint oldSequence = packet.Header.Sequence;
                            packet = ModifySequence(packet, Direction.Outgoing, ref length, ref needsCopy);

                            // keep track of sequence numbers
                            if (packet.Header.Sequence > outgoingSequence)
                                outgoingSequence = packet.Header.Sequence;

                            // check the packet for addresses that need proxying
                            if (Frame.Network.outgoingCheckers.ContainsKey(packet.Type))
                            {
                                /* if (packet.Header.Zerocoded) {
                                    length = Helpers.ZeroDecode(packet.Header.Data, length, zeroBuffer);
                                    packet.Header.Data = zeroBuffer;
                                    needsZero = false;
                                } */

                                Packet newPacket = ((AddressChecker)Frame.Network.outgoingCheckers[packet.Type])(packet);
                                RegionManager.SwapPacket(packet, newPacket);
                                packet = newPacket;
                                needsCopy = false;
                            }

                            // pass the packet to any callback delegates
                            if (Frame.Network.outgoingDelegates.ContainsKey(packet.Type))
                            {
                                if (packet.Header.AckList != null && needsCopy)
                                {
                                    uint[] newAcks = new uint[packet.Header.AckList.Length];
                                    Array.Copy(packet.Header.AckList, 0, newAcks, 0, newAcks.Length);
                                    packet.Header.AckList = newAcks; // FIXME
                                }

                                try
                                {
                                    Packet newPacket = Frame.Network.callDelegates(Frame.Network.outgoingDelegates, packet, this);
                                    if (newPacket == null)
                                    {
                                        if (packet.Header.Reliable)
                                            Inject(Frame.Network.SpoofAck(oldSequence), Direction.Incoming);

                                        if (packet.Header.AppendedAcks)
                                            packet = Frame.Network.SeparateAck(packet);
                                        else
                                            packet = null;
                                    }
                                    else
                                    {
                                        bool oldReliable = packet.Header.Reliable;
                                        bool newReliable = newPacket.Header.Reliable;
                                        if (oldReliable && !newReliable)
                                            Inject(Frame.Network.SpoofAck(oldSequence), Direction.Incoming);
                                        else if (!oldReliable && newReliable)
                                            WaitForAck(packet, Direction.Outgoing);

                                        RegionManager.SwapPacket(packet, newPacket);
                                        packet = newPacket;
                                    }
                                }
                                catch (Exception e)
                                {
                                    OpenMetaverse.Logger.Log("exception in outgoing delegate", Helpers.LogLevel.Error, e);
                                }

                                if (packet != null)
                                    Frame.Network.SendPacket(packet, RemoteEndPoint, false);
                            }
                            else
                                Frame.Network.SendPacket(packet, RemoteEndPoint, needsZero);

                            // send any packets queued for injection
                            if (firstReceive)
                            {
                                firstReceive = false;
                                foreach (Packet queuedPacket in Frame.Network.queuedIncomingInjections)
                                    Inject(queuedPacket, Direction.Incoming);
                                Frame.Network.queuedIncomingInjections = new List<Packet>();
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        OpenMetaverse.Logger.Log("Proxy error sending packet", Helpers.LogLevel.Error, e);
                    }
                    finally
                    {
                        // resume listening
                        try
                        {
                            socket.BeginReceiveFrom(receiveBuffer, 0, receiveBuffer.Length, SocketFlags.None,
                                ref clientEndPoint, new AsyncCallback(ReceiveFromClient), null);
                        }
                        catch (SocketException e)
                        {
                            OpenMetaverse.Logger.Log("Socket Shutdown: " + e.SocketErrorCode, Helpers.LogLevel.Warning);
                        }
                    }
                }
            }

            // SendPacket: send a packet from the sim to the client via our fake sim endpoint
            public void SendPacket(Packet packet, bool skipZero)
            {
                byte[] buffer = packet.ToBytes();
                if (skipZero || !packet.Header.Zerocoded)
                    socket.SendTo(buffer, buffer.Length, SocketFlags.None, clientEndPoint);
                else
                {
                    int zeroLength = Helpers.ZeroEncode(buffer, buffer.Length, zeroBuffer);
                    socket.SendTo(zeroBuffer, zeroLength, SocketFlags.None, clientEndPoint);
                }
            }

            // Inject: inject a packet
            public void Inject(Packet packet, Direction direction)
            {
                if (direction == Direction.Incoming)
                {
                    if (firstReceive)
                    {
                        Frame.Network.queuedIncomingInjections.Add(packet);
                        return;
                    }

                    incomingInjections.Add(++incomingSequence);
                    packet.Header.Sequence = incomingSequence;
                }
                else
                {
                    outgoingInjections.Add(++outgoingSequence);
                    packet.Header.Sequence = outgoingSequence;
                }

                if (packet.Header.Reliable)
                    WaitForAck(packet, direction);

                if (direction == Direction.Incoming)
                {
                    byte[] buffer = packet.ToBytes();
                    if (!packet.Header.Zerocoded)
                        socket.SendTo(buffer, buffer.Length, SocketFlags.None, clientEndPoint);
                    else
                    {
                        int zeroLength = Helpers.ZeroEncode(buffer, buffer.Length, zeroBuffer);
                        socket.SendTo(zeroBuffer, zeroLength, SocketFlags.None, clientEndPoint);
                    }
                }
                else
                    Frame.Network.SendPacket(packet, RemoteEndPoint, false);
            }

            // WaitForAck: take care of resending a packet until it's ACKed
            public void WaitForAck(Packet packet, Direction direction)
            {
                Dictionary<uint, Packet> table = direction == Direction.Incoming ? incomingAcks : outgoingAcks;
                lock (table)
                {
                    if (!table.ContainsKey(packet.Header.Sequence))
                        table.Add(packet.Header.Sequence, packet);
                }
            }

            // CheckAcks: check for and remove ACKs of packets we've injected
            public Packet CheckAcks(Packet packet, Direction direction, ref int length, ref bool needsCopy)
            {
                Dictionary<uint, Packet> acks = direction == Direction.Incoming ? outgoingAcks : incomingAcks;
                List<uint> seenAcks = direction == Direction.Incoming ? outgoingSeenAcks : incomingSeenAcks;

                if (acks.Count == 0)
                    return packet;

                // check for embedded ACKs
                if (packet.Type == PacketType.PacketAck)
                {
                    bool changed = false;
                    List<PacketAckPacket.PacketsBlock> newPacketBlocks = new List<PacketAckPacket.PacketsBlock>();
                    foreach (PacketAckPacket.PacketsBlock pb in ((PacketAckPacket)packet).Packets)
                    {
                        uint id = pb.ID;
                        if (acks.ContainsKey(id))
                        {
                            acks.Remove(id);
                            seenAcks.Add(id);
                            changed = true;
                        }
                        else
                        {
                            newPacketBlocks.Add(pb);
                        }
                    }
                    if (changed)
                    {
                        PacketAckPacket newPacket = new PacketAckPacket();
                        newPacket.Packets = new PacketAckPacket.PacketsBlock[newPacketBlocks.Count];

                        int a = 0;
                        foreach (PacketAckPacket.PacketsBlock pb in newPacketBlocks)
                        {
                            newPacket.Packets[a++] = pb;
                        }

                        RegionManager.SwapPacket(packet, (Packet)newPacket);
                        packet = newPacket;
                        needsCopy = false;
                    }
                }

                // check for appended ACKs
                if (packet.Header.AppendedAcks)
                {
                    int ackCount = packet.Header.AckList.Length;
                    for (int i = 0; i < ackCount;)
                    {
                        uint ackID = packet.Header.AckList[i]; // FIXME FIXME FIXME

                        if (acks.ContainsKey(ackID))
                        {
                            uint[] newAcks = new uint[ackCount - 1];
                            Array.Copy(packet.Header.AckList, 0, newAcks, 0, i);
                            Array.Copy(packet.Header.AckList, i + 1, newAcks, i, ackCount - i - 1);
                            packet.Header.AckList = newAcks;
                            --ackCount;
                            acks.Remove(ackID);
                            seenAcks.Add(ackID);
                            needsCopy = false;
                        }
                        else
                            ++i;
                    }
                    if (ackCount == 0)
                    {
                        packet.Header.AppendedAcks = false;
                        packet.Header.AckList = new uint[0];
                    }
                }

                return packet;
            }

            // ModifySequence: modify a packet's sequence number and ACK IDs to account for injections
            public Packet ModifySequence(Packet packet, Direction direction, ref int length, ref bool needsCopy)
            {
                List<uint> ourInjections = direction == Direction.Outgoing ? outgoingInjections : incomingInjections;
                List<uint> theirInjections = direction == Direction.Incoming ? outgoingInjections : incomingInjections;
                uint ourOffset = direction == Direction.Outgoing ? outgoingOffset : incomingOffset;
                uint theirOffset = direction == Direction.Incoming ? outgoingOffset : incomingOffset;

                uint newSequence = (uint)(packet.Header.Sequence + ourOffset);
                foreach (uint injection in ourInjections)
                    if (newSequence >= injection)
                        ++newSequence;

                packet.Header.Sequence = newSequence;

                if (packet.Header.AppendedAcks)
                {
                    int ackCount = packet.Header.AckList.Length;
                    for (int i = 0; i < ackCount; ++i)
                    {
                        //int offset = length - (ackCount - i) * 4 - 1;
                        uint ackID = packet.Header.AckList[i] - theirOffset;

                        for (int j = theirInjections.Count - 1; j >= 0; --j)
                            if (ackID >= (uint)theirInjections[j])
                                --ackID;

                        packet.Header.AckList[i] = ackID;
                    }
                }

                if (packet.Type == PacketType.PacketAck)
                {
                    PacketAckPacket pap = (PacketAckPacket)packet;
                    foreach (PacketAckPacket.PacketsBlock pb in pap.Packets)
                    {
                        uint ackID = (uint)pb.ID - theirOffset;

                        for (int i = theirInjections.Count - 1; i >= 0; --i)
                            if (ackID >= (uint)theirInjections[i])
                                --ackID;

                        pb.ID = ackID;

                    }

                    switch (packet.Header.Frequency)
                    {
                        case PacketFrequency.High: length = 7; break;
                        case PacketFrequency.Medium: length = 8; break;
                        case PacketFrequency.Low: length = 10; break;
                    }

                    needsCopy = false;
                }

                return packet;
            }
        }




        public class CapInfo
        {
            private string uri;
            private RegionProxy sim;
            private string type;
            private CapsDataFormat reqFmt;
            private CapsDataFormat respFmt;

            private List<CapsDelegate> Delegates = new List<CapsDelegate>();


            public CapInfo(string URI, RegionProxy Sim, string CapType)
                :
                this(URI, Sim, CapType, CapsDataFormat.OSD, CapsDataFormat.OSD)
            { }
            public CapInfo(string URI, RegionProxy Sim, string CapType, CapsDataFormat ReqFmt, CapsDataFormat RespFmt)
            {
                uri = URI; sim = Sim; type = CapType; reqFmt = ReqFmt; respFmt = RespFmt;
            }

            public string URI
            {
                get { return uri; }
            }
            public string CapType
            {
                get { return type; } /* EventQueueGet, etc */
            }
            public RegionProxy Sim
            {
                get { return sim; }
            }
            public CapsDataFormat ReqFmt
            {
                get { return reqFmt; } /* expected request format */
            }
            public CapsDataFormat RespFmt
            {
                get { return respFmt; } /* expected response format */
            }

            public void AddDelegate(CapsDelegate deleg)
            {
                lock (this)
                {
                    if (!Delegates.Contains(deleg))
                    {
                        Delegates.Add(deleg);
                    }
                }
            }
            public void RemoveDelegate(CapsDelegate deleg)
            {
                lock (this)
                {
                    if (Delegates.Contains(deleg))
                    {
                        Delegates.Remove(deleg);
                    }
                }
            }

            // inefficient, but avoids potential deadlocks.
            public List<CapsDelegate> GetDelegates()
            {
                lock (this)
                {
                    return new List<CapsDelegate>(Delegates);
                }
            }
        }

        // Information associated with a caps request/response
        public class CapsRequest
        {
            public CapsRequest(CapInfo info)
            {
                Info = info;
            }

            //public CapsRequest(RegionProxy region)
            //{
            //    Region = region;
            //}

            public readonly CapInfo Info;

            //public readonly RegionProxy Region;

            // The request
            public OSD Request = null;

            // The corresponding response
            public OSD Response = null;

            public byte[] RawRequest = null;
            public byte[] RawResponse = null;

            public WebHeaderCollection RequestHeaders = new WebHeaderCollection();
            public WebHeaderCollection ResponseHeaders = new WebHeaderCollection();

            public string FullUri = string.Empty;
            public string Method = string.Empty;
        }

        public delegate bool CapsDelegate(CapsRequest req, CapsStage stage);

        public enum CapsDataFormat
        {
            Binary = 0,
            OSD = 1
        }

        public enum CapsStage
        {
            Request,
            Response
        }
    }

    public delegate void GenericRegionDelegate(RegionProxy proxy);
}
