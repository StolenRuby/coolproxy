using GridProxy;
using Nwc.XmlRpc;
using OpenMetaverse;
using OpenMetaverse.StructuredData;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace CoolProxy
{
    public class OpenSimManager
    {
        private CoolProxyFrame proxyFrame;

        public AssetService Assets { get; private set; } = null;
        public XInventoryServie XInventory { get; private set; } = null;
        public GridIMService GridIM { get; private set; } = null;

        public HypergridService Hypergrid { get; private set; } = null;



        public event GenericRegionDelegate OnURLsRetrieved;



        bool HasHandshake = false;
        bool HasFeatures = false;
        bool MadeRequest = false;


        public OpenSimManager(CoolProxyFrame coolProxy)
        {
            proxyFrame = coolProxy;

            Assets = new AssetService(coolProxy);
            XInventory = new XInventoryServie(coolProxy);
            GridIM = new GridIMService(coolProxy);
            Hypergrid = new HypergridService(coolProxy);

            proxyFrame.Network.AddCapsDelegate("SimulatorFeatures", OnSimulatorFeatures);

            coolProxy.Network.OnHandshake += Regions_OnHandshake;

            coolProxy.Network.OnRegionChanged += Regions_OnRegionChanged;
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
            OpenMetaverse.Logger.Log("Attempting to get service urls...", Helpers.LogLevel.Info);

            MadeRequest = true;

            string target_uri = region.GridURI;

            Task.Run(() =>
            {
                List<UUID> keys_to_try = new List<UUID>();
                keys_to_try.Add(proxyFrame.Agent.AgentID);

                if (proxyFrame.Network.CurrentSim.Owner != UUID.Zero)
                    keys_to_try.Add(proxyFrame.Network.CurrentSim.Owner);

                bool success = false;

                foreach (UUID id in keys_to_try)
                {
                    Hashtable data = new Hashtable();
                    data["userID"] = id.ToString();

                    //proxyFrame.SayToUser("Trying " + id.ToString() + "...");
                    OpenMetaverse.Logger.Log("Trying " + id.ToString() + "...", Helpers.LogLevel.Info);

                    ArrayList SendParams = new ArrayList();
                    SendParams.Add(data);

                    XmlRpcRequest GridReq = new XmlRpcRequest("get_server_urls", SendParams);

                    try
                    {

                        XmlRpcResponse GridResp = GridReq.Send(target_uri, 10000);

                        Hashtable responseData = (Hashtable)GridResp.Value;

                        if(responseData.ContainsKey("result"))
                        {
                            if((string)responseData["result"] == "No Service URLs")
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
                        OpenMetaverse.Logger.Log("Successfully got service URLs!", Helpers.LogLevel.Info);

                        success = true;
                        break;
                    }
                    catch (WebException err)
                    {

                    }
                }

                if(success)
                {
                    string image_url;
                    UUID uuid;
                    proxyFrame.OpenSim.Hypergrid.LinkRegion(region.Name, out uuid, out image_url);

                    if (image_url != string.Empty)
                    {
                        Uri imguri = new Uri(image_url);
                        region.HostUri = string.Format("{0}://{1}", imguri.Scheme, imguri.Authority);

                        OpenMetaverse.Logger.Log("Successfully got host URI!", Helpers.LogLevel.Info);
                    }

                    OnURLsRetrieved?.Invoke(region);
                }

                //OpenMetaverse.Logger.Log("Failed to get service URLs!", Helpers.LogLevel.Info);
                //CoolProxy.Frame.AlertMessage("Failed to get service URLs!", false);
            });
        }

        private void Regions_OnHandshake(RegionManager.RegionProxy region)
        {
            if (region != CoolProxy.Frame.Network.CurrentSim)
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
                if(req.Info.Sim == CoolProxy.Frame.Network.CurrentSim)
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

    public class HypergridService
    {
        private CoolProxyFrame coolProxy;
        public HypergridService(CoolProxyFrame proxy)
        {
            coolProxy = proxy;
        }

        public void LinkRegion(string region_name, out UUID region_uuid, out string image_url)
        {
            Hashtable data = new Hashtable();
            data["region_name"] = region_name;

            region_uuid = UUID.Zero;
            image_url = string.Empty;

            //proxyFrame.SayToUser("Trying " + id.ToString() + "...");
            //OpenMetaverse.Logger.Log("Trying " + id.ToString() + "...", Helpers.LogLevel.Info);

            ArrayList SendParams = new ArrayList();
            SendParams.Add(data);

            XmlRpcRequest GridReq = new XmlRpcRequest("link_region", SendParams);

            try
            {

                XmlRpcResponse GridResp = GridReq.Send(CoolProxy.Frame.Network.CurrentSim.GatekeeperURI, 10000);

                Hashtable responseData = (Hashtable)GridResp.Value;

                if (responseData.ContainsKey("result"))
                {
                    if ((string)responseData["result"] != "True")
                    {
                        //MessageBox.Show("Ohhhh dear");
                        return;
                    }
                }

                if (responseData.ContainsKey("uuid"))
                {
                    if (!UUID.TryParse((string)responseData["uuid"], out region_uuid))
                    {
                        OpenMetaverse.Logger.Log("Invalid UUID in link_region response!", Helpers.LogLevel.Debug);
                    }
                }

                if (responseData.ContainsKey("region_image"))
                {
                    image_url = (string)responseData["region_image"];
                }
            }
            catch (Exception ex)
            {
                coolProxy.SayToUser(ex.Message);
            }
        }
    }

    public delegate void HandleUploadAssetResult(bool success, UUID new_id);

    public class AssetService
    {
        private CoolProxyFrame coolProxy;

        public AssetService(CoolProxyFrame proxy)
        {
            coolProxy = proxy;
        }

        public void UploadAsset(UUID asset_id, AssetType asset_type, string name, string desc, UUID creator_id, byte[] data, HandleUploadAssetResult handler = null)
        {
            int upload_attempts = 0;

            string base64_data = Convert.ToBase64String(data);

            string format = "<?xml version=\"1.0\" encoding=\"utf-8\"?><AssetBase xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"><Data>{0}</Data><FullID><Guid>{1}</Guid></FullID><ID>{1}</ID><Name>{2}</Name><Description>{3}</Description><Type>{4}</Type><UploadAttempts>{5}</UploadAttempts><Local>false</Local><Temporary>false</Temporary><CreatorID>{6}</CreatorID><Flags>{7}</Flags></AssetBase>";
            string request = string.Format(format, base64_data, asset_id, name, desc, (int)asset_type, upload_attempts, creator_id, "Normal");


            WebClient webClient = new WebClient();
            webClient.UploadStringCompleted += (x, y) =>
            {
                XmlSerializer deserializer = new XmlSerializer(typeof(string));

                string reply_str = (string)deserializer.Deserialize(GenerateStreamFromString(y.Result));

                UUID new_id;
                if(UUID.TryParse(reply_str, out new_id))
                {
                    handler?.Invoke(true, new_id);
                    return;
                }

                handler?.Invoke(false, UUID.Zero);
            };

            string target_uri = coolProxy.Network.CurrentSim.AssetServerURI;

            if (target_uri == string.Empty)
                target_uri = coolProxy.Network.CurrentSim.GridURI;

            target_uri += "assets";

            webClient.UploadStringAsync(new Uri(target_uri), "POST", request);
        }

        public static Stream GenerateStreamFromString(string s)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        public void DownloadAsset(UUID asset_id, AssetServiceDownloadComplete handler)
        {
            string target_uri = coolProxy.Network.CurrentSim.AssetServerURI;

            if (target_uri == string.Empty)
                target_uri = coolProxy.Network.CurrentSim.GridURI;


            // I'm doing it this way instead of /data because some grids dont allow /data for some reason??

            string url = string.Format("{0}assets/{1}", target_uri, asset_id.ToString());

            WebClient webClient = new WebClient();
            webClient.DownloadStringCompleted += (x, y) =>
            {
                try
                {
                    Dictionary<string, string> replyData = parseResponse(y.Result);

                    if (replyData.ContainsKey("Data"))
                    {
                        byte[] data = Convert.FromBase64String((string)replyData["Data"]);
                        handler?.Invoke(true, data);
                    }
                    else
                        handler?.Invoke(false, null);
                }
                catch
                {
                    handler?.Invoke(false, null);
                }
            };
            webClient.DownloadStringAsync(new Uri(url));
        }

        public void GetAssetMetadata(UUID asset_id, AssetMetadataCallback handler)
        {
            string target_uri = coolProxy.Network.CurrentSim.AssetServerURI;

            if (target_uri == string.Empty)
                target_uri = coolProxy.Network.CurrentSim.GridURI;

            string url = string.Format("{0}assets/{1}/metadata", target_uri, asset_id.ToString());

            WebClient webClient = new WebClient();
            webClient.DownloadStringCompleted += (x, y) =>
            {
                try
                {
                    Dictionary<string, string> replyData = parseResponse(y.Result);

                    var meta = AssetMetadata.FromDictionary(replyData);

                    handler?.Invoke(meta);
                }
                catch
                {
                    handler?.Invoke(null);
                }
            };
            webClient.DownloadStringAsync(new Uri(url));
        }

        Dictionary<string, string> parseResponse(string data)
        {
            XDocument doc = XDocument.Parse(data);
            Dictionary<string, string> dataDictionary = new Dictionary<string, string>();

            foreach (XElement element in doc.Descendants().Where(p => p.HasElements == false))
            {
                int keyInt = 0;
                string keyName = element.Name.LocalName;

                while (dataDictionary.ContainsKey(keyName))
                {
                    keyName = element.Name.LocalName + "_" + keyInt++;
                }

                dataDictionary.Add(keyName, element.Value);
            }

            return dataDictionary;
        }
    }

    public class AssetMetadata
    {
        public AssetType Type { get; private set; }

        public AssetMetadata()
        {

        }

        public static AssetMetadata FromDictionary(Dictionary<string, string> dict)
        {
            var meta = new AssetMetadata();

            string str;
            if(dict.TryGetValue("Type", out str))
            {
                meta.Type = (AssetType)Convert.ToInt32(str);
            }


            // todo: add the rest

            return meta;
        }
    }

    public delegate void AssetMetadataCallback(AssetMetadata metadata);

    public delegate void AssetServiceDownloadComplete(bool success, byte[] data);

    public delegate void HandleAddItemResult(bool result);
    public delegate void HandleGetItem(InventoryItem item);

    public class XInventoryServie
    {
        private CoolProxyFrame coolProxy;
        public XInventoryServie(CoolProxyFrame frame)
        {
            coolProxy = frame;
        }
        public void AddItem(UUID folderID, UUID itemID, UUID assetID, AssetType assetType, InventoryType invType, uint flags, string item_name, string item_desc, int creationDate, HandleAddItemResult handler = null)
        {
            AddItem(folderID, itemID, assetID, CoolProxy.Frame.Agent.AgentID, assetType, invType, flags, item_name, item_desc, creationDate, 532480, 581635, 581635, 0, 0, UUID.Zero, false, 0, SaleType.Not, "", "", handler);
        }

        public void AddItem(UUID folderID, UUID itemID, UUID assetID, UUID ownerID, AssetType assetType, InventoryType invType, uint flags, string item_name, string item_desc, int creationDate, HandleAddItemResult handler = null)
        {
            AddItem(folderID, itemID, assetID, ownerID, assetType, invType, flags, item_name, item_desc, creationDate, 532480, 581635, 581635, 0, 0, UUID.Zero, false, 0, SaleType.Not, "", "", handler);
        }

        public void AddItem(InventoryItem item, HandleAddItemResult handler = null)
        {
            AddItem(item.ParentUUID, item.UUID, item.AssetUUID, item.OwnerID, item.AssetType, item.InventoryType, item.Flags, item.Name, item.Description,
                (int)Utils.DateTimeToUnixTime(item.CreationDate), (uint)item.Permissions.NextOwnerMask, (uint)item.Permissions.OwnerMask, (uint)item.Permissions.BaseMask, (uint)item.Permissions.EveryoneMask, (uint)item.Permissions.GroupMask,
                item.GroupID, item.GroupOwned, (uint)item.SalePrice, item.SaleType, string.Empty, string.Empty, handler);
        }

        public void AddItem(UUID folderID, UUID itemID, UUID assetID, UUID ownerID, AssetType assetType, InventoryType invType, uint flags, string item_name, string item_desc, int creationDate, uint nextPermissions, uint currentPermissions, uint basePermissions, uint everyonePermissions, uint groupPermissions, UUID groupID, bool group_owned, uint sale_price, SaleType saleType, string creatorID, string creatorData, HandleAddItemResult handler = null)
        {

            Dictionary<string, object> sendData = new Dictionary<string, object> {
                        { "METHOD", "ADDITEM"},
                        { "AssetID", assetID.ToString() },
                        { "AssetType", (uint)assetType },
                        { "Name", item_name },
                        { "Owner", ownerID.ToString() },
                        { "ID", itemID.ToString() },
                        { "InvType", (uint)invType },
                        { "Folder", folderID.ToString() },
                        { "CreatorId", creatorID },
                        { "CreatorData", creatorData },
                        { "Description", item_desc },
                        { "NextPermissions", nextPermissions.ToString() },
                        { "CurrentPermissions", currentPermissions.ToString() },
                        { "BasePermissions", basePermissions.ToString() },
                        { "EveryOnePermissions", everyonePermissions.ToString() },
                        { "GroupPermissions", groupPermissions.ToString() },
                        { "GroupID", groupID.ToString() },
                        { "GroupOwned", group_owned.ToString() },
                        { "SalePrice", sale_price.ToString() },
                        { "SaleType", (uint)saleType },
                        { "Flags", flags.ToString() },
                        { "CreationDate", creationDate.ToString() }
                    };

            string request_str = ServerUtils.BuildQueryString(sendData);

            WebClient webClient = new WebClient();
            webClient.UploadStringCompleted += (x, y) =>
            {
                Dictionary<string, object> replyData = ServerUtils.ParseXmlResponse(y.Result);

                object res;
                if (replyData.TryGetValue("RESULT", out res))
                {
                    if ((string)res == "True")
                    {
                        handler?.Invoke(true);
                        return;
                    }
                }

                handler?.Invoke(false);
            };

            string target_uri = coolProxy.Network.CurrentSim.InvetoryServerURI;

            if (target_uri == string.Empty)
                target_uri = coolProxy.Network.CurrentSim.GridURI;

            target_uri += "xinventory";

            webClient.UploadStringAsync(new Uri(target_uri), "POST", request_str);
        }


        public void GetItem(UUID item_id, UUID agent_id, HandleGetItem handler)
        {
            Dictionary<string, object> sendData = new Dictionary<string, object> {
                { "METHOD", "GETITEM"},
                { "ID", item_id.ToString() },
                {"PRINCIPAL", agent_id.ToString() }
            };

            string request_str = ServerUtils.BuildQueryString(sendData);

            WebClient webClient = new WebClient();
            webClient.UploadStringCompleted += (x, y) =>
            {
                Dictionary<string, object> replyData = ServerUtils.ParseXmlResponse(y.Result);

                //object res;
                //if (replyData.TryGetValue("RESULT", out res))
                //{
                //    if ((string)res == "True")
                //    {
                //        handler?.Invoke(true);
                //        return;
                //    }
                //}

                InventoryItem item = null;

                if(replyData.ContainsKey("item"))
                {
                    item = BuildItem((Dictionary<string, object>)replyData["item"]);
                }

                handler?.Invoke(item);
            };

            string target_uri = coolProxy.Network.CurrentSim.InvetoryServerURI;

            if (target_uri == string.Empty)
                target_uri = coolProxy.Network.CurrentSim.GridURI;

            target_uri += "xinventory";

            webClient.UploadStringAsync(new Uri(target_uri), "POST", request_str);
        }

        private InventoryItem BuildItem(Dictionary<string, object> data)
        {
            try
            {
                UUID uuid = new UUID(data["ID"].ToString());
                InventoryType inv_type = (InventoryType)int.Parse(data["InvType"].ToString());

                //item.UUID = uuid;
                //item.InventoryType = inv_type;

                InventoryItem item = new InventoryItem(inv_type, uuid);

                item.AssetUUID = new UUID(data["AssetID"].ToString());
                item.AssetType = (AssetType)int.Parse(data["AssetType"].ToString());
                item.Name = data["Name"].ToString();
                item.OwnerID = new UUID(data["Owner"].ToString());
                item.ParentUUID = new UUID(data["Folder"].ToString());
                item.CreatorID = new UUID(data["CreatorId"].ToString());
                //if (data.ContainsKey("CreatorData"))
                //    item.CreatorData = data["CreatorData"].ToString();
                item.Description = data["Description"].ToString();
                item.Permissions.NextOwnerMask = (PermissionMask)uint.Parse(data["NextPermissions"].ToString());
                item.Permissions.OwnerMask = (PermissionMask)uint.Parse(data["CurrentPermissions"].ToString());
                item.Permissions.BaseMask = (PermissionMask)uint.Parse(data["BasePermissions"].ToString());
                item.Permissions.EveryoneMask = (PermissionMask)uint.Parse(data["EveryOnePermissions"].ToString());
                item.Permissions.GroupMask = (PermissionMask)uint.Parse(data["GroupPermissions"].ToString());
                item.GroupID = new UUID(data["GroupID"].ToString());
                item.GroupOwned = bool.Parse(data["GroupOwned"].ToString());
                item.SalePrice = int.Parse(data["SalePrice"].ToString());
                item.SaleType = (SaleType)byte.Parse(data["SaleType"].ToString());
                item.Flags = uint.Parse(data["Flags"].ToString());
                item.CreationDate = Utils.UnixTimeToDateTime(int.Parse(data["CreationDate"].ToString()));

                return item;
            }
            catch (Exception e)
            {
                OpenMetaverse.Logger.Log("[XINVENTORY CONNECTOR]: Exception building item: ", Helpers.LogLevel.Debug, e);
            }

            return null;
        }
        
        
        public void AddFolder(string name, UUID folder_id, UUID parent_id, UUID owner_id, FolderType folder_type, int version, HandleAddItemResult handler)
        {
            Dictionary<string, object> sendData = new Dictionary<string, object> {
                        { "METHOD", "ADDFOLDER"},
                        { "ParentID", parent_id.ToString() },
                        { "Type", ((short)folder_type).ToString() },
                        { "Version", ((short)version).ToString() },
                        { "Name", name },
                        { "Owner", owner_id.ToString() },
                        { "ID", folder_id.ToString() }
                    };

            string request_str = ServerUtils.BuildQueryString(sendData);

            WebClient webClient = new WebClient();
            webClient.UploadStringCompleted += (x, y) =>
            {
                Dictionary<string, object> replyData = ServerUtils.ParseXmlResponse(y.Result);

                object res;
                if (replyData.TryGetValue("RESULT", out res))
                {
                    if ((string)res == "True")
                    {
                        handler?.Invoke(true);
                        return;
                    }
                }

                handler?.Invoke(false);
            };

            string target_uri = coolProxy.Network.CurrentSim.InvetoryServerURI;

            if (target_uri == string.Empty)
                target_uri = coolProxy.Network.CurrentSim.GridURI;

            target_uri += "xinventory";

            webClient.UploadStringAsync(new Uri(target_uri), "POST", request_str);
        }
    }

    public class GridService
    {
        private CoolProxyFrame coolProxy;

        public GridService(CoolProxyFrame frame)
        {
            coolProxy = frame;
        }
    }

    public class GridIMService
    {
        private CoolProxyFrame coolProxy;

        public GridIMService(CoolProxyFrame proxy)
        {
            coolProxy = proxy;
        }

        public void SendGridIM(UUID fromAgentUUID, string fromAgentName, UUID toAgentUUID, InstantMessageDialog dialogType, bool from_group, string message, UUID sessionUUID, bool offline, Vector3 position, UUID regionUUID, uint parent_estate, byte[] bucket, uint timestamp, string target_uri = "")
        {
            //GridInstantMessage gridInstantMessage = new GridInstantMessage(
            //    fromAgentUUID, fromAgentName, toAgentUUID,
            //    (byte)dialogType, from_group, message, sessionUUID,
            //    offline, position, regionUUID,
            //    parent_estate, bucket, timestamp);


            //var data = ConvertGridInstantMessageToXMLRPC(gridInstantMessage);

            Hashtable gim = new Hashtable();
            gim["from_agent_id"] = fromAgentUUID.ToString();
            // Kept for compatibility
            gim["from_agent_session"] = UUID.Zero.ToString();
            gim["to_agent_id"] = toAgentUUID.ToString();
            gim["im_session_id"] = sessionUUID.ToString();
            gim["timestamp"] = timestamp.ToString();
            gim["from_agent_name"] = fromAgentName;
            gim["message"] = message;
            byte[] dialogdata = new byte[1] { (byte)dialogType };
            gim["dialog"] = Convert.ToBase64String(dialogdata, Base64FormattingOptions.None);

            if (from_group)
                gim["from_group"] = "TRUE";
            else
                gim["from_group"] = "FALSE";

            byte[] offlinedata = new byte[1] { offline ? (byte)0x01 : (byte)0x00 };
            gim["offline"] = Convert.ToBase64String(offlinedata, Base64FormattingOptions.None);
            gim["parent_estate_id"] = parent_estate.ToString();
            gim["position_x"] = position.X.ToString();
            gim["position_y"] = position.Y.ToString();
            gim["position_z"] = position.Z.ToString();
            gim["region_id"] = regionUUID.ToString();
            gim["binary_bucket"] = Convert.ToBase64String(bucket, Base64FormattingOptions.None);
            gim["region_id"] = new UUID(regionUUID).ToString();

            //if (messageKey != string.Empty)
            //    gim["message_key"] = messageKey;



            ArrayList SendParams = new ArrayList();
            SendParams.Add(gim);

            if (target_uri == string.Empty)
                target_uri = coolProxy.Network.CurrentSim.IMServerURI;

            if (target_uri == string.Empty)
                target_uri = coolProxy.Network.CurrentSim.GridURI;

            Task.Run(() =>
            {
                XmlRpcRequest GridReq = new XmlRpcRequest("grid_instant_message", SendParams);

                try
                {
                    XmlRpcResponse GridResp = GridReq.Send(target_uri, 10000);

                    Hashtable responseData = (Hashtable)GridResp.Value;

                    if (responseData.ContainsKey("success"))
                    {
                        if ((string)responseData["success"] == "TRUE")
                        {
                            //CoolProxy.Frame.SayToUser("Message sent successfully!");
                            return;
                        }
                        else
                        {
                            //CoolProxy.Frame.SayToUser("Failed to send message!");
                            CoolProxy.Frame.AlertMessage("Failed to send message!", false);
                            return;
                        }
                    }
                    else
                    {
                        CoolProxy.Frame.SayToUser(string.Format("[GRID INSTANT MESSAGE]: No response from {0}", target_uri));
                        return;
                    }
                }
                catch (WebException err)
                {
                    CoolProxy.Frame.SayToUser(string.Format("[GRID INSTANT MESSAGE]: Error sending message to {0} the host didn't respond " + err.ToString(), target_uri));
                }
            });
        }

        //protected static Hashtable ConvertGridInstantMessageToXMLRPC(GridInstantMessage msg, string messageKey = "")
        //{
        //    Hashtable gim = new Hashtable();
        //    gim["from_agent_id"] = msg.fromAgentID.ToString();
        //    // Kept for compatibility
        //    gim["from_agent_session"] = UUID.Zero.ToString();
        //    gim["to_agent_id"] = msg.toAgentID.ToString();
        //    gim["im_session_id"] = msg.imSessionID.ToString();
        //    gim["timestamp"] = msg.timestamp.ToString();
        //    gim["from_agent_name"] = msg.fromAgentName;
        //    gim["message"] = msg.message;
        //    byte[] dialogdata = new byte[1]; dialogdata[0] = msg.dialog;
        //    gim["dialog"] = Convert.ToBase64String(dialogdata, Base64FormattingOptions.None);

        //    if (msg.fromGroup)
        //        gim["from_group"] = "TRUE";
        //    else
        //        gim["from_group"] = "FALSE";

        //    byte[] offlinedata = new byte[1]; offlinedata[0] = msg.offline;
        //    gim["offline"] = Convert.ToBase64String(offlinedata, Base64FormattingOptions.None);
        //    gim["parent_estate_id"] = msg.ParentEstateID.ToString();
        //    gim["position_x"] = msg.Position.X.ToString();
        //    gim["position_y"] = msg.Position.Y.ToString();
        //    gim["position_z"] = msg.Position.Z.ToString();
        //    gim["region_id"] = msg.RegionID.ToString();
        //    gim["binary_bucket"] = Convert.ToBase64String(msg.binaryBucket, Base64FormattingOptions.None);
        //    gim["region_id"] = new UUID(msg.RegionID).ToString();

        //    if (messageKey != string.Empty)
        //        gim["message_key"] = messageKey;

        //    return gim;
        //}
    }
}
