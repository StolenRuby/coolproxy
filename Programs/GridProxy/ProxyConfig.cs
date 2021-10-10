using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GridProxy
{
    /// <summary>
    /// Proxy Configuration Class
    /// </summary>
    public class ProxyConfig
    {

        #region Timeouts and Intervals

        /// <summary>Number of milliseconds before an asset transfer will time
        /// out</summary>
        public int TRANSFER_TIMEOUT = 90 * 1000;

        /// <summary>Number of milliseconds before a CAPS call will time out</summary>
        /// <remarks>Setting this too low will cause web requests time out and
        /// possibly retry repeatedly</remarks>
        public int CAPS_TIMEOUT = 60 * 1000;

        /// <summary>Milliseconds to wait for a simulator info request through
        /// the grid interface</summary>
        public int MAP_REQUEST_TIMEOUT = 5 * 1000;

        #endregion


        public bool HTTP_INVENTORY = true;


        public int UPLOAD_COST = 0;




        /// <summary>
        /// If true, images, and other assets downloaded from the server 
        /// will be cached in a local directory
        /// </summary>
        public bool USE_ASSET_CACHE = true;

        /// <summary>Path to store cached texture data</summary>
        public string ASSET_CACHE_DIR = "asset_cache";

        /// <summary>Maximum size cached files are allowed to take on disk (bytes)</summary>
        public long ASSET_CACHE_MAX_SIZE = 1024 * 1024 * 1024; // 1GB

        public bool USE_HTTP_ASSETS = true;


        /// <summary>
        /// The port the proxy server will listen on
        /// </summary>
        public ushort loginPort = 8080;
        /// <summary>
        /// The IP Address the proxy server will communication with the client on
        /// </summary>
        public IPAddress clientFacingAddress = IPAddress.Loopback;
        /// <summary>
        /// The IP Address the proxy server will communicate with the server on
        /// </summary>
        public IPAddress remoteFacingAddress = IPAddress.Any;
        /// <summary>
        /// The URI of the login server
        /// </summary>
        public Uri remoteLoginUri = new Uri("https://login.agni.lindenlab.com/cgi-bin/login.cgi");

        /// <summary>
        /// construct a default proxy configuration, parsing command line arguments (try --help)
        /// </summary>
        /// <param name="userAgent">The user agent reported to the remote server</param>
        /// <param name="author">Email address of the proxy application's author</param>
        /// <param name="args">An array containing the parameters to use to override the proxy
        /// servers default settings</param>
        public ProxyConfig(string[] args, bool exitOnError)
        {
            Dictionary<string, ArgumentParser> argumentParsers = new Dictionary<string, ArgumentParser>();
            argumentParsers["help"] = new ArgumentParser(ParseHelp);
            argumentParsers["proxy-help"] = new ArgumentParser(ParseHelp);
            argumentParsers["proxy-login-port"] = new ArgumentParser(ParseLoginPort);
            argumentParsers["proxy-client-facing-address"] = new ArgumentParser(ParseClientFacingAddress);
            argumentParsers["proxy-remote-facing-address"] = new ArgumentParser(ParseRemoteFacingAddress);
            argumentParsers["proxy-remote-login-uri"] = new ArgumentParser(ParseRemoteLoginUri);

            foreach (string arg in args)
            {
                foreach (string argument in argumentParsers.Keys)
                {
                    Match match = (new Regex("^--" + argument + "(?:=(.*))?$")).Match(arg);
                    if (match.Success)
                    {
                        string value;
                        if (match.Groups[1].Captures.Count == 1)
                            value = match.Groups[1].Captures[0].ToString();
                        else
                            value = null;
                        try
                        {
                            ((ArgumentParser)argumentParsers[argument])(value);
                        }
                        catch
                        {
                            Console.WriteLine("invalid value for --" + argument);
                            if (exitOnError)
                            {
                                ParseHelp(null);
                            }
                            else
                            {
                                throw;
                            }
                        }
                    }
                }
            }
        }

        private delegate void ArgumentParser(string value);

        private void ParseHelp(string value)
        {
            Console.WriteLine("Proxy command-line arguments:");
            Console.WriteLine("  --help                              display this help");
            Console.WriteLine("  --proxy-login-port=<port>           listen for logins on <port>");
            Console.WriteLine("  --proxy-client-facing-address=<IP>  communicate with client via <IP>");
            Console.WriteLine("  --proxy-remote-facing-address=<IP>  communicate with server via <IP>");
            Console.WriteLine("  --proxy-remote-login-uri=<URI>      use SL login server at <URI>");
            Console.WriteLine("  --log-all                           log all packets by default in Analyst");
            Console.WriteLine("  --log-whitelist=<file>              log packets listed in file, one name per line");
            Console.WriteLine("  --no-log-blacklist=<file>           don't log packets in file, one name per line");
            Console.WriteLine("  --output=<logfile>                  log Analyst output to a file");

            Environment.Exit(1);
        }

        private void ParseLoginPort(string value)
        {
            loginPort = Convert.ToUInt16(value);
        }

        private void ParseClientFacingAddress(string value)
        {
            clientFacingAddress = IPAddress.Parse(value);
        }

        private void ParseRemoteFacingAddress(string value)
        {
            remoteFacingAddress = IPAddress.Parse(value);
        }

        private void ParseRemoteLoginUri(string value)
        {
            remoteLoginUri = new Uri(value);
        }
    }
}
