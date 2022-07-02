using OpenMetaverse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace GridProxy
{
    public delegate void BasicHTTPHandler(string url, string method, NetworkStream netStream, Dictionary<string, string> headers, byte[] content);

    public class HttpManager
    {
        private int capsReqCount = 0;

        object keepAliveLock = new object();

        public string ProxyURI { get; private set; }

        private Socket loginServer;

        private Dictionary<string, BasicHTTPHandler> HttpHandlers = new Dictionary<string, BasicHTTPHandler>();

        public DownloadManager Downloads { get; set; } = null;

        private ProxyFrame Frame;
        public HttpManager(ProxyFrame frame)
        {
            Frame = frame;
            Downloads = new DownloadManager();
        }

        // Start: begin accepting clients
        public void Start()
        {
            InitializeLoginProxy();

            lock (this)
            {
                System.Threading.Monitor.Enter(keepAliveLock);
                (new Thread(new ThreadStart(KeepAlive))).Start();

                //RunSimProxy();

                Thread runLoginProxy = new Thread(new ThreadStart(RunHttpHandler));
                runLoginProxy.IsBackground = true;
                runLoginProxy.Name = "HTTP Handler";
                runLoginProxy.Start();

                IPEndPoint endPoint = (IPEndPoint)loginServer.LocalEndPoint;
                IPAddress displayAddress;
                if (endPoint.Address == IPAddress.Any)
                    displayAddress = IPAddress.Loopback;
                else
                    displayAddress = endPoint.Address;
                ProxyURI = "http://" + displayAddress + ":" + endPoint.Port + "/";

                OpenMetaverse.Logger.Log("[HTTP] Proxy ready at " + ProxyURI, Helpers.LogLevel.Info);
            }
        }

        // Stop: allow foreground threads to die
        public void Stop()
        {
            lock (this)
            {
                System.Threading.Monitor.Exit(keepAliveLock);
            }

            HttpHandlers.Clear();
        }

        public void KeepAlive()
        {
            OpenMetaverse.Logger.Log(">T> KeepAlive", Helpers.LogLevel.Debug);

            lock (keepAliveLock) { };

            if (loginServer.Connected)
            {
                loginServer.Disconnect(false);
                loginServer.Shutdown(SocketShutdown.Both);
            }

            loginServer.Close();

            OpenMetaverse.Logger.Log("<T< KeepAlive", Helpers.LogLevel.Debug);
        }

        private void RunHttpHandler()
        {
            OpenMetaverse.Logger.Log(">T> RunLoginProxy", Helpers.LogLevel.Debug);

            try
            {
                for (; ; )
                {
                    try
                    {
                        Socket client = loginServer.Accept();

                        Thread connThread = new Thread((ThreadStart)delegate
                        {
                            OpenMetaverse.Logger.Log(">T> HTTPHandler", Helpers.LogLevel.Debug);
                            ProxyHTTP(client);
                            OpenMetaverse.Logger.Log("<T< HTTPHandler", Helpers.LogLevel.Debug);
                        });

                        connThread.IsBackground = true;
                        connThread.Name = "HTTP Handler";
                        connThread.Start();
                    }
                    catch (SocketException e)
                    {
                        // indicates we've told the listener to shutdown
                        if (e.SocketErrorCode == SocketError.Interrupted)
                            break;

                        OpenMetaverse.Logger.Log("Http Exception: ", Helpers.LogLevel.Error, e);
                        break;
                    }
                    catch (ObjectDisposedException)
                    {
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                OpenMetaverse.Logger.Log("Exception in RunHttpHandler", Helpers.LogLevel.Error, e);
            }

            OpenMetaverse.Logger.Log("<T< RunHttpHandler", Helpers.LogLevel.Debug);
        }

        private void InitializeLoginProxy()
        {
            try
            {
                loginServer = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                loginServer.Bind(new IPEndPoint(Frame.Config.clientFacingAddress, Frame.Config.loginPort));
                loginServer.Listen(1);
            }
            catch (SocketException e)
            {
                OpenMetaverse.Logger.Log("Socket Exception", Helpers.LogLevel.Error, e);
            }
            catch (ObjectDisposedException e)
            {
                OpenMetaverse.Logger.Log("Socket Object is disposed Exception", Helpers.LogLevel.Error, e);
            }
        }


        private class HandyNetReader
        {
            private NetworkStream netStream;
            private const int BUF_SIZE = 8192;
            private byte[] buf = new byte[BUF_SIZE];
            private int bufFill = 0;

            public HandyNetReader(NetworkStream s)
            {
                netStream = s;
            }

            public byte[] ReadLine()
            {
                int i = -1;
                while (true)
                {
                    i = Array.IndexOf(buf, (byte)'\n', 0, bufFill);
                    if (i >= 0) break;
                    if (bufFill >= BUF_SIZE) return null;
                    if (!ReadMore()) return null;
                }
                if (bufFill < (i + 1)) return null;
                byte[] ret = new byte[i];
                Array.Copy(buf, ret, i);
                Array.Copy(buf, i + 1, buf, 0, bufFill - (i + 1));
                bufFill -= i + 1;
                return ret;
            }

            private bool ReadMore()
            {
                try
                {
                    int n = netStream.Read(buf, bufFill, BUF_SIZE - bufFill);
                    bufFill += n;
                    return n > 0;
                }
                catch
                {
                    return false;
                }
            }

            public int Read(byte[] rbuf, int start, int len)
            {
                int read = 0;
                while (len > bufFill)
                {
                    Array.Copy(buf, 0, rbuf, start, bufFill);
                    start += bufFill; len -= bufFill;
                    read += bufFill; bufFill = 0;
                    if (!ReadMore()) break;
                }
                if (bufFill < len) return 0;
                Array.Copy(buf, 0, rbuf, start, len);
                Array.Copy(buf, len, buf, 0, bufFill - len);
                bufFill -= len; read += len;
                return read;
            }
        }


        private void ProxyHTTP(Socket client)
        {
            NetworkStream netStream = new NetworkStream(client);
            HandyNetReader reader = new HandyNetReader(netStream);

            string line = null;
            int reqNo;
            int contentLength = 0;
            Match match;
            string uri;
            string meth;
            Dictionary<string, string> headers = new Dictionary<string, string>();

            lock (this)
            {
                capsReqCount++; reqNo = capsReqCount;
            }

            byte[] byteLine = reader.ReadLine();
            if (byteLine == null)
            {
                //This dirty hack is part of the LIBOMV-457 workaround
                //The connecting libomv client being proxied can manage to trigger a null from the ReadLine()
                //The happens just after the seed request and is not seen again. TODO find this bug in the library.
                netStream.Close(); client.Close();
                return;
            }

            if (byteLine != null) line = Encoding.UTF8.GetString(byteLine).Replace("\r", "");

            if (line == null)
                throw new Exception("EOF in client HTTP header");

            match = new Regex(@"^(\S+)\s+(\S+)\s+(HTTP/\d\.\d)$").Match(line);

            if (!match.Success)
            {
                OpenMetaverse.Logger.Log("[" + reqNo + "] Bad request!", Helpers.LogLevel.Warning);
                byte[] wr = Encoding.ASCII.GetBytes("HTTP/1.0 400 Bad Request\r\nContent-Length: 0\r\n\r\n");
                netStream.Write(wr, 0, wr.Length);
                netStream.Close(); client.Close();
                return;
            }

            meth = match.Groups[1].Captures[0].ToString();
            uri = match.Groups[2].Captures[0].ToString();

            OpenMetaverse.Logger.Log(String.Format("[{0}] {1}:{2}", reqNo, meth, uri), Helpers.LogLevel.Debug);

            // read HTTP header
            do
            {
                // read one line of the header
                line = Encoding.UTF8.GetString(reader.ReadLine()).Replace("\r", "");

                // check for premature EOF
                if (line == null)
                    throw new Exception("EOF in client HTTP header");

                if (line == "") break;

                match = new Regex(@"^([^:]+):\s*(.*)$").Match(line);

                if (!match.Success)
                {
                    OpenMetaverse.Logger.Log(String.Format("[{0}] Bad Header: '{1}'", reqNo, line), Helpers.LogLevel.Warning);
                    byte[] wr = Encoding.ASCII.GetBytes("HTTP/1.0 400 Bad Request\r\nContent-Length: 0\r\n\r\n");
                    netStream.Write(wr, 0, wr.Length);
                    netStream.Close(); client.Close();
                    return;
                }

                string key = match.Groups[1].Captures[0].ToString();
                string val = match.Groups[2].Captures[0].ToString();
                headers[key.ToLower()] = val;
            } while (line != "");

            if (headers.ContainsKey("content-length"))
            {
                contentLength = Convert.ToInt32(headers["content-length"]);
            }

            // read the HTTP body into a buffer
            byte[] content = new byte[contentLength];
            reader.Read(content, 0, contentLength);

            //if (contentLength < 8192)
            //    OpenMetaverse.Logger.Log(String.Format("[{0}] request length={1}:\n{2}", reqNo, contentLength, Utils.BytesToString(content)), Helpers.LogLevel.Debug);

            string found_uri = string.Empty;

            foreach(string u in HttpHandlers.Keys)
            {
                if(uri.StartsWith(u))
                {
                    found_uri = u;
                    break;
                }
            }

            if(found_uri != string.Empty)
            {
                var handler = HttpHandlers[found_uri];
                handler?.Invoke(uri, meth, netStream, headers, content);
            }
            else
            {
                OpenMetaverse.Logger.Log("404 not found: " + uri, Helpers.LogLevel.Error);
                byte[] wr = Encoding.ASCII.GetBytes("HTTP/1.0 404 Not Found\r\nContent-Length: 0\r\n\r\n");
                netStream.Write(wr, 0, wr.Length);
                netStream.Close(); client.Close();
                return;
            }

            netStream.Close();
            client.Close();
        }

        public void AddHttpHandler(string url, BasicHTTPHandler handler)
        {
            if (HttpHandlers.ContainsKey(url) == false)
            {
                HttpHandlers.Add(url, handler);


                OpenMetaverse.Logger.Log("[HTTP] Added HTTP handler at: " + url, Helpers.LogLevel.Info);
            }
            else throw new Exception("URL already has a handler");
        }

        public void RemoveHttpHandler(string url)
        {
            if(HttpHandlers.ContainsKey(url))
            {
                HttpHandlers.Remove(url);

                OpenMetaverse.Logger.Log("[HTTP] Removed HTTP handler at: " + url, Helpers.LogLevel.Info);
            }
        }
    }
}
