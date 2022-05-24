using OpenMetaverse.StructuredData;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace CoolProxy
{
    public class GridListManager
    {
        private List<GridInfo> mGrids = new List<GridInfo>();

        public GridInfo SelectedGrid { get; private set; }

        public GridListManager()
        {
            string grids_file = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "CoolProxy\\grid_list.xml");

            if(File.Exists(grids_file))
            {
                loadGrids(grids_file);
            }

            if(mGrids.Count == 0)
            {
                mGrids.Add(new GridInfo("Second Life", "Agni", "https://login.agni.lindenlab.com/cgi-bin/login.cgi", "https://login.aditi.lindenlab.com/cgi-bin/login.cgi", true));
                mGrids.Add(new GridInfo("Second Life Beta", "Aditi", "https://login.aditi.lindenlab.com/cgi-bin/login.cgi", "https://login.aditi.lindenlab.com/cgi-bin/login.cgi", true));
                saveGrids();
            }

            var openSimGridInfo = CoolProxy.Frame.Settings.getSetting("EnableGridInfo");
            openSimGridInfo.OnChanged += (x, y) =>
            {
                bool enabled = (bool)y.Value;

                if(enabled)
                {
                    CoolProxy.Frame.HTTP.AddHttpHandler("/get_grid_info", HandleGetGridInfo);
                }
                else
                {
                    CoolProxy.Frame.HTTP.RemoveHttpHandler("/get_grid_info");
                }
            };

            if((bool)openSimGridInfo.Value)
            {
                CoolProxy.Frame.HTTP.AddHttpHandler("/get_grid_info", HandleGetGridInfo);
            }

            CoolProxy.Frame.HTTP.AddHttpHandler("/splash", HandleSplashScreen);
        }

        public void loadGrids(string filename)
        {
            byte[] data = File.ReadAllBytes(filename);
            OSDMap map = (OSDMap)OSDParser.DeserializeLLSDXml(data);

            foreach (string key in map.Keys)
            {
                OSDMap grid_osd = (OSDMap)map[key];

                if (grid_osd.ContainsKey("Name") && grid_osd.ContainsKey("LoginURI"))
                {
                    string nick = "OpenSim";
                    if (grid_osd.ContainsKey("Nick"))
                        nick = grid_osd["Nick"];

                    string splash = string.Empty;
                    if (grid_osd.ContainsKey("Splash"))
                        splash = grid_osd["Splash"];

                    bool is_linden = false;
                    if (grid_osd.ContainsKey("Linden"))
                        is_linden = grid_osd["Linden"];

                    GridInfo gridInfo = new GridInfo(grid_osd["Name"], nick, grid_osd["LoginURI"], splash, is_linden);

                    mGrids.Add(gridInfo);
                }
            }
        }

        public void saveGrids()
        {
            string grids_file = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "CoolProxy\\grid_list.xml");

            OSDMap gridMap = new OSDMap();
            foreach(var info in mGrids)
            {
                OSDMap gridInfo = new OSDMap();
                gridInfo["Name"] = info.Name;
                gridInfo["LoginURI"] = info.LoginURI;
                gridInfo["Linden"] = info.IsLindenGrid;
                gridInfo["Splash"] = info.Splash;
                gridInfo["Nick"] = info.Nick;

                gridMap[info.Name] = gridInfo;
            }

            byte[] xml = OSDParser.SerializeLLSDXmlBytes(gridMap);

            File.WriteAllBytes(grids_file, xml);
        }

        public GridInfo[] getGrids()
        {
            return mGrids.ToArray();
        }

        public string[] getGridNames()
        {
            return mGrids.Select(x => x.Name).ToArray();
        }

        public GridInfo getInfoFromName(string name)
        {
            int i = mGrids.FindIndex(x => x.Name == name);
            return i > -1 ? mGrids[i] : null;
        }

        public void removeGrid(string name)
        {
            int i = mGrids.FindIndex(x => x.Name == name);
            if(i > -1)
            {
                mGrids.RemoveAt(i);
            }

            saveGrids();
        }

        public delegate void LoginGridChangedDelegate(GridInfo gridInfo);

        public event LoginGridChangedDelegate OnGridChanged;

        public delegate void GridAddedDelegate(GridInfo gridInfo);

        public event GridAddedDelegate OnGridAdded;

        public void selectGrid(string name)
        {
            var info = getInfoFromName(name);
            CoolProxy.Frame.Settings.setString("LastGridUsed", name);
            CoolProxy.Frame.Settings.setBool("LindenGridSelected", info.IsLindenGrid);
            SelectedGrid = info;
            OnGridChanged?.Invoke(SelectedGrid);
        }

        public bool addGridFromDictionary(Dictionary<string, string> info, out string error)
        {
            if (info.ContainsKey("gridname") && info.ContainsKey("login"))
            {
                string gridName = info["gridname"];
                string gridLogin = info["login"];

                if(getInfoFromName(gridName) != null)
                {
                    error = "Grid already added!";
                    return false;
                }

                string welcome = string.Empty;
                if (info.ContainsKey("welcome"))
                    welcome = info["welcome"];

                string platform = "OpenSim";
                if (info.ContainsKey("platform"))
                    platform = info["platform"];

                var grid_info = new GridInfo(gridName, platform, gridLogin, welcome);

                mGrids.Add(grid_info);

                saveGrids();

                OnGridAdded?.Invoke(grid_info);

                selectGrid(gridName);

                error = "";
                return true;
            }
            else
            {
                error = "Invalid GridInfo!";
                return false;
            }
        }



        private void HandleSplashScreen(string url, string method, NetworkStream netStream, Dictionary<string, string> headers, byte[] content)
        {
            lock (this)
            {
                StreamWriter writer = new StreamWriter(netStream);
                writer.Write("HTTP/1.0 307 Temporary Redirect\r\n");
                writer.Write("Location: " + SelectedGrid.Splash + "\r\n");
                writer.Close();
            }
        }

        private void HandleGetGridInfo(string url, string method, NetworkStream netStream, Dictionary<string, string> headers, byte[] content)
        {
            lock (this)
            {
                StringBuilder sb = new StringBuilder();

                string address = CoolProxy.Frame.Settings.getString("GridProxyListenAddress");
                int port = CoolProxy.Frame.Settings.getInteger("GridProxyListenPort");

                sb.Append("<gridinfo>\n");
                sb.AppendFormat("<{0}>{1}</{0}>\n", "gridname", "Cool Proxy" + (port != 8080 ? " (" + port.ToString() + ")" : string.Empty));
                sb.AppendFormat("<{0}>{1}</{0}>\n", "platform", "Cool Proxy");
                sb.AppendFormat("<{0}>{1}</{0}>\n", "login", string.Format("http://{0}:{1}/login", address, port.ToString()));
                sb.AppendFormat("<{0}>{1}</{0}>\n", "welcome", string.Format("http://{0}:{1}/splash", address, port.ToString()));
                sb.Append("</gridinfo>\n");

                StreamWriter writer = new StreamWriter(netStream);
                writer.Write("HTTP/1.0 200 OK\r\n");
                writer.Write("Content-type: application/llsd+xml\r\n");
                writer.Write("\r\n");
                writer.Write(sb.ToString());
                writer.Close();
            }
        }
    }

    public class GridInfo
    {
        public string Name { get; private set; }
        public string Nick { get; private set; }
        public string LoginURI { get; private set; }
        public string Splash { get; private set; }
        public bool IsLindenGrid { get; private set; }

        public GridInfo(string name, string nick, string uri, string splash, bool linden = false)
        {
            Name = name;
            Nick = nick;
            LoginURI = uri;
            Splash = splash;
            IsLindenGrid = linden;
        }
    }
}
