using GridProxy;
using Nwc.XmlRpc;
using OpenMetaverse;
using OpenMetaverse.StructuredData;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoolProxy.Plugins.OpenSim
{
    public interface IROBUST
    {
        AssetService Assets { get; }
        XInventoryServie Inventory { get; }
        GridIMService IM { get; }
        HypergridService Gatekeeper { get; }
    }

    public class OpenSimPlugin : CoolProxyPlugin, IROBUST
    {
        private CoolProxyFrame Proxy;

        public AssetService Assets { get; } = null;
        public XInventoryServie Inventory { get; } = null;
        public GridIMService IM { get; } = null;
        public HypergridService Gatekeeper { get; } = null;


        public event GenericRegionDelegate OnURLsRetrieved;


        bool HasHandshake = false;
        bool HasFeatures = false;
        bool MadeRequest = false;


        public OpenSimPlugin(CoolProxyFrame frame)
        {
            Proxy = frame;

            Assets = new AssetService(Proxy);
            Inventory = new XInventoryServie(Proxy);
            IM = new GridIMService(Proxy);
            Gatekeeper = new HypergridService(Proxy);

            Proxy.RegisterModuleInterface<IROBUST>(this);

            frame.Network.AddCapsDelegate("SimulatorFeatures", OnSimulatorFeatures);
            frame.Network.OnHandshake += Regions_OnHandshake;
            frame.Network.OnRegionChanged += Regions_OnRegionChanged;
        }

        private void Regions_OnRegionChanged(RegionManager.RegionProxy region)
        {
            HasHandshake = false;
            HasFeatures = false;
            MadeRequest = false;
        }

        void FetchURLS(RegionManager.RegionProxy region)
        {
            //proxyFrame.SayToUser("Attempting to get service urls...");
            OpenMetaverse.Logger.Log("[OPENSIM] Attempting to get service urls...", Helpers.LogLevel.Info);

            MadeRequest = true;

            string target_uri = region.GridURI;

            Task.Run(() =>
            {
                List<UUID> keys_to_try = new List<UUID>();
                keys_to_try.Add(Proxy.Agent.AgentID);

                if (Proxy.Network.CurrentSim.Owner != UUID.Zero)
                    keys_to_try.Add(Proxy.Network.CurrentSim.Owner);

                bool success = false;

                foreach (UUID id in keys_to_try)
                {
                    Hashtable data = new Hashtable();
                    data["userID"] = id.ToString();

                    //proxyFrame.SayToUser("Trying " + id.ToString() + "...");
                    OpenMetaverse.Logger.Log("[OPENSIM] Trying " + id.ToString() + "...", Helpers.LogLevel.Debug);

                    ArrayList SendParams = new ArrayList();
                    SendParams.Add(data);

                    XmlRpcRequest GridReq = new XmlRpcRequest("get_server_urls", SendParams);

                    try
                    {

                        XmlRpcResponse GridResp = GridReq.Send(target_uri, 10000);

                        Hashtable responseData = (Hashtable)GridResp.Value;

                        if (responseData.ContainsKey("result"))
                        {
                            if ((string)responseData["result"] == "No Service URLs")
                            {
                                continue;
                            }
                        }

                        if (responseData.ContainsKey("SRV_ProfileServerURI"))
                        {
                            region.ProfileServerURI = (string)responseData["SRV_ProfileServerURI"] ?? region.GridURI;
                            if (!region.ProfileServerURI.EndsWith("/")) region.ProfileServerURI += "/";
                        }

                        if (responseData.ContainsKey("SRV_AssetServerURI"))
                        {
                            region.AssetServerURI = (string)responseData["SRV_AssetServerURI"] ?? region.GridURI;
                            if (!region.AssetServerURI.EndsWith("/")) region.AssetServerURI += "/";
                        }

                        if (responseData.ContainsKey("SRV_IMServerURI"))
                        {
                            region.IMServerURI = (string)responseData["SRV_IMServerURI"] ?? region.GridURI;
                            if (!region.IMServerURI.EndsWith("/")) region.IMServerURI += "/";
                        }

                        if (responseData.ContainsKey("SRV_InventoryServerURI"))
                        {
                            region.InvetoryServerURI = (string)responseData["SRV_InventoryServerURI"] ?? region.GridURI;
                            if (!region.InvetoryServerURI.EndsWith("/")) region.InvetoryServerURI += "/";
                        }

                        if (responseData.ContainsKey("SRV_FriendsServerURI"))
                        {
                            region.FriendsServerURI = (string)responseData["SRV_FriendsServerURI"] ?? region.GridURI;
                            if (!region.FriendsServerURI.EndsWith("/")) region.FriendsServerURI += "/";
                        }

                        if (responseData.ContainsKey("SRV_GatekeeperURI"))
                        {
                            region.GatekeeperURI = (string)responseData["SRV_GatekeeperURI"] ?? region.GridURI;
                            if (!region.GatekeeperURI.EndsWith("/")) region.GatekeeperURI += "/";
                        }

                        if (responseData.ContainsKey("SRV_HomeURI"))
                        {
                            region.HomeURI = (string)responseData["SRV_HomeURI"] ?? region.GridURI;
                            if (!region.HomeURI.EndsWith("/")) region.HomeURI += "/";
                        }

                        //proxyFrame.SayToUser("Successfully got service URLs!");
                        OpenMetaverse.Logger.Log("[OPENSIM] Successfully got service URLs!", Helpers.LogLevel.Info);

                        success = true;
                        break;
                    }
                    catch
                    {

                    }
                }

                if (success)
                {
                    string image_url;
                    UUID uuid;
                    Gatekeeper.LinkRegion(region.Name, out uuid, out image_url);

                    if (image_url != string.Empty)
                    {
                        Uri imguri = new Uri(image_url);
                        region.HostUri = string.Format("{0}://{1}", imguri.Scheme, imguri.Authority);

                        OpenMetaverse.Logger.Log("[OPENSIM] Successfully got host URI!", Helpers.LogLevel.Info);
                    }

                    OnURLsRetrieved?.Invoke(region);
                }

                //OpenMetaverse.Logger.Log("Failed to get service URLs!", Helpers.LogLevel.Info);
                //CoolProxy.Frame.AlertMessage("Failed to get service URLs!", false);
            });
        }

        private void Regions_OnHandshake(RegionManager.RegionProxy region)
        {
            if (region != Proxy.Network.CurrentSim)
                return;

            HasHandshake = true;

            if (HasHandshake && HasFeatures && !MadeRequest)
                FetchURLS(region);
        }

        private bool OnSimulatorFeatures(RegionManager.CapsRequest req, RegionManager.CapsStage stage)
        {
            //CoolProxy.Frame.SayToUser(req.FullUri);

            if (stage == RegionManager.CapsStage.Response)
            {
                if (req.Info.Sim == Proxy.Network.CurrentSim)
                {
                    req.Info.Sim.GridURI = string.Empty;
                    OSDMap map = (OSDMap)req.Response;

                    OSD extras;
                    if (map.TryGetValue("OpenSimExtras", out extras))
                    {
                        OSDMap extras_map = (OSDMap)extras;

                        if (extras_map.ContainsKey("GridURL"))
                        {
                            req.Info.Sim.GridURI = extras_map["GridURL"].AsString();
                            if (!req.Info.Sim.GridURI.EndsWith("/")) req.Info.Sim.GridURI += "/";
                            //CoolProxy.Frame.SayToUser(string.Format("Current Region's GridURI: {0}", CurrentGridURI));
                        }
                    }

                    HasFeatures = true;

                    if (HasHandshake && HasFeatures && !MadeRequest)
                        FetchURLS(req.Info.Sim);
                }
            }
            return false;
        }
    }
}
