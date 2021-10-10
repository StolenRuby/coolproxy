using Nwc.XmlRpc;
using OpenMetaverse;
using OpenMetaverse.Http;
using OpenMetaverse.StructuredData;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace GridProxy
{
    public class XmlRpcRequestEventArgs : EventArgs
    {
        public XmlRpcRequest m_Request;
        public int m_originalSize;
        public string m_host;
        public Dictionary<string, string> m_headers;

        public XmlRpcRequestEventArgs(XmlRpcRequest request, int originalSize, Dictionary<string, string> headers, string host)
        {
            m_Request = request;
            m_originalSize = originalSize;
            m_headers = headers;
            m_host = host;
        }
    }

    public delegate void XmlRpcRequestDelegate(object sender, XmlRpcRequestEventArgs e);

    // XmlRpcResponseDelegate: specifies a delegate to be called for XML-RPC responses
    public delegate void XmlRpcResponseDelegate(XmlRpcResponse response);

    public class LoginProxy
    {
        private List<XmlRpcRequestDelegate> loginRequestDelegates = new List<XmlRpcRequestDelegate>();
        private List<XmlRpcResponseDelegate> loginResponseDelegates = new List<XmlRpcResponseDelegate>();

        private ProxyFrame Frame;

        public LoginProxy(ProxyFrame frame)
        {
            Frame = frame;
        }

        public void Start()
        {
            Frame.HTTP.AddHttpHandler("/login", HandleProxyLogin);
        }

        public void AddLoginRequestDelegate(XmlRpcRequestDelegate xmlRpcRequestDelegate)
        {
            lock (loginRequestDelegates)
                if (!loginRequestDelegates.Contains(xmlRpcRequestDelegate))
                    loginRequestDelegates.Add(xmlRpcRequestDelegate);

        }

        public void AddLoginResponseDelegate(XmlRpcResponseDelegate xmlRpcResponseDelegate)
        {
            lock (loginResponseDelegates)
                if (!loginResponseDelegates.Contains(xmlRpcResponseDelegate))
                    loginResponseDelegates.Add(xmlRpcResponseDelegate);
        }

        private void HandleProxyLogin(string url, string meth, NetworkStream netStream, Dictionary<string, string> headers, byte[] content)
        {
            string content_type = headers["content-type"];
            if (content_type == "application/xml+llsd" || content_type == "application/xml")
            {
                ProxyLoginSD(netStream, content, headers, url);
            }
            else
            {
                ProxyLogin(netStream, content, headers, url);
            }
        }


        private void ProxyLogin(NetworkStream netStream, byte[] content, Dictionary<string, string> headers, string url)
        {
            lock (this)
            {
                // incase some silly person tries to access with their web browser
                if (content.Length <= 0)
                    return;

                // convert the body into an XML-RPC request
                XmlRpcRequest request = (XmlRpcRequest)(new XmlRpcRequestDeserializer()).Deserialize(Encoding.UTF8.GetString(content));

                // call the loginRequestDelegate
                lock (loginRequestDelegates)
                {
                    foreach (XmlRpcRequestDelegate d in loginRequestDelegates)
                    {
                        try { d(this, new XmlRpcRequestEventArgs(request, content.Length, headers, url)); }
                        //try { d(request); }
                        catch (Exception e) { OpenMetaverse.Logger.Log("Exception in login request delegate" + e, Helpers.LogLevel.Error, e); }
                    }
                }
                XmlRpcResponse response;
                try
                {
                    // forward the XML-RPC request to the server
                    response = (XmlRpcResponse)request.Send(Frame.Config.remoteLoginUri.ToString(),
                        30 * 1000); // 30 second timeout
                }
                catch (Exception e)
                {
                    OpenMetaverse.Logger.Log("Error during login response", Helpers.LogLevel.Error, e);
                    return;
                }

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

                lock (loginResponseDelegates)
                {
                    foreach (XmlRpcResponseDelegate d in loginResponseDelegates)
                    {
                        try { d(response); }
                        catch (Exception e) { OpenMetaverse.Logger.Log("Exception in login response delegate" + e, Helpers.LogLevel.Error, e); }
                    }
                }

                // forward the XML-RPC response to the client
                StreamWriter writer = new StreamWriter(netStream);
                writer.Write("HTTP/1.0 200 OK\r\n");
                writer.Write("Content-type: text/xml\r\n");
                writer.Write("\r\n");

                XmlTextWriter responseWriter = new XmlTextWriter(writer);
                XmlRpcResponseSerializer.Singleton.Serialize(responseWriter, response);
                responseWriter.Close(); writer.Close();
            }
        }

        private void ProxyLoginSD(NetworkStream netStream, byte[] content, Dictionary<string, string> headers, string url)
        {
            lock (this)
            {
                AutoResetEvent remoteComplete = new AutoResetEvent(false);
                CapsClient loginRequest = new CapsClient(Frame.Config.remoteLoginUri);
                OSD response = null;
                loginRequest.OnComplete += new CapsClient.CompleteCallback(
                    delegate (CapsClient client, OSD result, Exception error)
                    {
                        if (error == null)
                        {
                            if (result != null && result.Type == OSDType.Map)
                            {
                                response = result;
                            }
                        }
                        remoteComplete.Set();
                    }
                    );
                loginRequest.BeginGetResponse(content, "application/llsd+xml", 1000 * 100);
                remoteComplete.WaitOne(1000 * 100, false);

                if (response == null)
                {
                    byte[] wr = Encoding.ASCII.GetBytes("HTTP/1.0 500 Internal Server Error\r\nContent-Length: 0\r\n\r\n");
                    netStream.Write(wr, 0, wr.Length);
                    return;
                }

                //OSDMap map = (OSDMap)response;

                //OSD llsd;
                //string sim_port = null, sim_ip = null, seed_capability = null;
                //map.TryGetValue("sim_port", out llsd);
                //if (llsd != null) sim_port = llsd.AsString();
                //map.TryGetValue("sim_ip", out llsd);
                //if (llsd != null) sim_ip = llsd.AsString();
                //map.TryGetValue("seed_capability", out llsd);
                //if (llsd != null) seed_capability = llsd.AsString();

                //if (sim_port == null || sim_ip == null || seed_capability == null)
                //{
                //    if (map != null)
                //    {
                //        OpenMetaverse.Logger.Log("Connection to server failed, returned LLSD error follows:\n" + map.ToString(), Helpers.LogLevel.Error);
                //    }
                //    byte[] wr = Encoding.ASCII.GetBytes("HTTP/1.0 500 Internal Server Error\r\nContent-Length: 0\r\n\r\n");
                //    netStream.Write(wr, 0, wr.Length);
                //    return;
                //}

                //IPEndPoint realSim = new IPEndPoint(IPAddress.Parse(sim_ip), Convert.ToUInt16(sim_port));
                //IPEndPoint fakeSim = ProxySim(realSim);
                //map["sim_ip"] = OSD.FromString(fakeSim.Address.ToString());
                //map["sim_port"] = OSD.FromInteger(fakeSim.Port);
                //activeCircuit = realSim;

                //// start a new proxy session
                //Reset();

                //CapInfo info = new CapInfo(seed_capability, activeCircuit, "SeedCapability");
                //info.AddDelegate(new CapsDelegate(FixupSeedCapsResponse));
                //info.AddDelegate(new CapsDelegate(KnownCapDelegate));

                //KnownCaps[seed_capability] = info;
                //map["seed_capability"] = OSD.FromString(loginURI + seed_capability);

                StreamWriter writer = new StreamWriter(netStream);
                writer.Write("HTTP/1.0 200 OK\r\n");
                writer.Write("Content-type: application/xml+llsd\r\n");
                writer.Write("\r\n");
                writer.Write(OSDParser.SerializeLLSDXmlString(response));
                writer.Close();
            }
        }
    }
}
