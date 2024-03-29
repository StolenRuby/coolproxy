﻿using OpenMetaverse.StructuredData;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
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
                fetchGridList();
                saveGrids();
            }

            var openSimGridInfo = Program.Frame.Settings.getSetting("EnableGridInfo");

            bool enable_grid_info = (bool)openSimGridInfo.Value;

            openSimGridInfo.OnChanged += (x, y) =>
            {
                bool enabled = (bool)y.Value;

                if(enabled != enable_grid_info)
                {
                    enable_grid_info = enabled;

                    if (enabled)
                    {
                        Program.Frame.HTTP.AddHttpHandler("/get_grid_info", HandleGetGridInfo);
                    }
                    else
                    {
                        Program.Frame.HTTP.RemoveHttpHandler("/get_grid_info");
                    }
                }
            };

            if(enable_grid_info)
            {
                Program.Frame.HTTP.AddHttpHandler("/get_grid_info", HandleGetGridInfo);
            }

            Program.Frame.HTTP.AddHttpHandler("/splash", HandleSplashScreen);
        }

        private void fetchGridList()
        {
            try
            {
                string list_url = "http://phoenixviewer.com/app/fsdata/grids.xml"; // todo: setting?

                WebClient webClient = new WebClient();
                string xml = webClient.DownloadString(list_url);

                OSDMap map = (OSDMap)OSDParser.DeserializeLLSDXml(xml);
                
                foreach(var key in map.Keys)
                {
                    try
                    {
                        OSDMap info = (OSDMap)map[key];

                        if (info.ContainsKey("DEPRECATED")) continue;
                        
                        string name = info["gridname"].AsString();
                        string nick = info["gridnick"].AsString();

                        OSDArray uris = (OSDArray)info["loginuri"];
                        string uri = uris.First().AsString();

                        string splash = info["loginpage"].AsString();

                        info.TryGetValue("system_grid", out OSD is_sl_osd);

                        bool is_sl = is_sl_osd?.AsBoolean() ?? false;

                        mGrids.Add(new GridInfo(name, nick, uri, splash, is_sl));
                    }
                    catch
                    {
                        OpenMetaverse.Logger.Log("[Grid Manager] Failed to parse `" + key + "`", OpenMetaverse.Helpers.LogLevel.Debug);
                    }
                }
            }
            catch
            {
                OpenMetaverse.Logger.Log("[Grid Manager] Failed to download grid list!", OpenMetaverse.Helpers.LogLevel.Debug);

                // Add some default grids...
                mGrids.Add(new GridInfo("Second Life", "Agni", "https://login.agni.lindenlab.com/cgi-bin/login.cgi", "https://viewer-splash.secondlife.com", true));
                mGrids.Add(new GridInfo("Second Life Beta", "Aditi", "https://login.aditi.lindenlab.com/cgi-bin/login.cgi", "https://viewer-splash.secondlife.com", true));
            }
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
            Program.Frame.Settings.setString("LastGridUsed", name);
            Program.Frame.Settings.setBool("LindenGridSelected", info.IsLindenGrid);
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

                string address = Program.Frame.Settings.getString("GridProxyListenAddress");
                int port = Program.Frame.Settings.getInteger("GridProxyListenPort");

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
