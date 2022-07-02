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
            OpenMetaverse.Logger.Log("[OPENSIM] Attempting to get service urls...", Helpers.LogLevel.Info);

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
                    OpenMetaverse.Logger.Log("[OPENSIM] Trying " + id.ToString() + "...", Helpers.LogLevel.Debug);

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
                        OpenMetaverse.Logger.Log("[OPENSIM] Successfully got service URLs!", Helpers.LogLevel.Info);

                        success = true;
                        break;
                    }
                    catch
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

    public delegate void GenericSuccessResult(bool result);
    public delegate void HandleGetItem(InventoryItem item);
    public delegate void HandleGetSkeleton(InventoryFolder[] folders);
    public delegate void HandleGetFolder(InventoryFolder folder);
    public delegate void HandleFolderContents(InventoryFolder[] folders, InventoryItem[] items);

    delegate void HandleResponse(Dictionary<string, object> reply_data);

    public class XInventoryServie
    {
        private CoolProxyFrame coolProxy;
        public XInventoryServie(CoolProxyFrame frame)
        {
            coolProxy = frame;
        }

        public void AddItem(UUID folderID, UUID itemID, UUID assetID, AssetType assetType, InventoryType invType, uint flags, string item_name, string item_desc, DateTime creationDate, GenericSuccessResult handler = null)
        {
            AddItem(folderID, itemID, assetID, CoolProxy.Frame.Agent.AgentID, assetType, invType, flags, item_name, item_desc, creationDate, 532480, 581635, 581635, 0, 0, UUID.Zero, false, 0, SaleType.Not, UUID.Zero, "", handler);
        }

        public void AddItem(UUID folderID, UUID itemID, UUID assetID, UUID ownerID, AssetType assetType, InventoryType invType, uint flags, string item_name, string item_desc, DateTime creationDate, GenericSuccessResult handler = null)
        {
            AddItem(folderID, itemID, assetID, ownerID, assetType, invType, flags, item_name, item_desc, creationDate, 532480, 581635, 581635, 0, 0, UUID.Zero, false, 0, SaleType.Not, UUID.Zero, "", handler);
        }

        public void AddItem(UUID folderID, UUID itemID, UUID assetID, UUID ownerID, AssetType assetType, InventoryType invType, uint flags, string item_name, string item_desc, DateTime creationDate, uint nextPermissions, uint currentPermissions, uint basePermissions, uint everyonePermissions, uint groupPermissions, UUID groupID, bool group_owned, int sale_price, SaleType saleType, UUID creatorID, string creatorData, GenericSuccessResult handler = null)
        {
            InventoryItem item = new InventoryItem(invType, itemID);
            item.AssetUUID = assetID;
            item.AssetType = assetType;
            item.Name = item_name;
            item.OwnerID = ownerID;
            item.ParentUUID = folderID;
            item.CreatorID = creatorID;
            item.CreatorData = creatorData;
            item.Description = item_desc;
            item.Permissions = new Permissions(basePermissions, everyonePermissions, groupPermissions, nextPermissions, currentPermissions);
            item.GroupID = groupID;
            item.GroupOwned = group_owned;
            item.SalePrice = sale_price;
            item.SaleType = saleType;
            item.Flags = flags;
            item.CreationDate = creationDate;

            AddItem(item, handler);
        }

        public void AddItem(InventoryItem item, GenericSuccessResult handler = null)
        {
            var sendData = ItemToDictionary(item);
            sendData["METHOD"] = "ADDITEM";
            MakeBoolRequest(sendData, handler);
        }

        public void UpdateItem(InventoryItem item, GenericSuccessResult handler = null)
        {
            var sendData = ItemToDictionary(item);
            sendData["METHOD"] = "UPDATEITEM";
            MakeBoolRequest(sendData, handler);
        }

        public void UpdateItem(UUID folderID, UUID itemID, UUID assetID, UUID ownerID, AssetType assetType, InventoryType invType, uint flags, string item_name, string item_desc, DateTime creationDate, uint nextPermissions, uint currentPermissions, uint basePermissions, uint everyonePermissions, uint groupPermissions, UUID groupID, bool group_owned, int sale_price, SaleType saleType, UUID creatorID, string creatorData, GenericSuccessResult handler = null)
        {
            InventoryItem item = new InventoryItem(invType, itemID);
            item.AssetUUID = assetID;
            item.AssetType = assetType;
            item.Name = item_name;
            item.OwnerID = ownerID;
            item.ParentUUID = folderID;
            item.CreatorID = creatorID;
            item.CreatorData = creatorData;
            item.Description = item_desc;
            item.Permissions = new Permissions(basePermissions, everyonePermissions, groupPermissions, nextPermissions, currentPermissions);
            item.GroupID = groupID;
            item.GroupOwned = group_owned;
            item.SalePrice = sale_price;
            item.SaleType = saleType;
            item.Flags = flags;
            item.CreationDate = creationDate;

            UpdateItem(item, handler);
        }

        void MakeBoolRequest(Dictionary<string, object> request, GenericSuccessResult handler)
        {
            MakeRequest(request, (reply_data) =>
            {
                if (reply_data.TryGetValue("RESULT", out object res))
                {
                    if ((string)res == "True")
                    {
                        handler?.Invoke(true);
                        return;
                    }
                }

                handler?.Invoke(false);
            });
        }

        void MakeRequest(Dictionary<string, object> send_data, HandleResponse handle)
        {
            string request_str = ServerUtils.BuildQueryString(send_data);

            WebClient webClient = new WebClient();
            webClient.UploadStringCompleted += (x, y) =>
            {
                Dictionary<string, object> replyData = ServerUtils.ParseXmlResponse(y.Result);

                handle?.Invoke(replyData);
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

            MakeRequest(sendData, (reply_data) =>
            {
                InventoryItem item = null;

                if (reply_data.ContainsKey("item"))
                {
                    item = BuildItem((Dictionary<string, object>)reply_data["item"]);
                }

                handler?.Invoke(item);
            });
        }

        public void GetSkeleton(UUID agent_id, HandleGetSkeleton handler)
        {
            Dictionary<string, object> sendData = new Dictionary<string, object> {
                { "METHOD", "GETINVENTORYSKELETON"},
                {"PRINCIPAL", agent_id.ToString() }
            };

            MakeRequest(sendData, (reply_data) =>
            {
                Dictionary<string, object> folders = (Dictionary<string, object>)reply_data["FOLDERS"];

                List<InventoryFolder> fldrs = new List<InventoryFolder>();

                try
                {
                    foreach (object o in folders.Values)
                        fldrs.Add(BuildFolder((Dictionary<string, object>)o));
                }
                catch (Exception e)
                {
                }

                handler?.Invoke(fldrs.ToArray());
            });
        }

        public void GetRootFolder(UUID agent_id, HandleGetFolder handler)
        {
            Dictionary<string, object> sendData = new Dictionary<string, object> {
                { "METHOD", "GETROOTFOLDER"},
                {"PRINCIPAL", agent_id.ToString() }
            };

            MakeRequest(sendData, (reply_data) =>
            {
                InventoryFolder folder = BuildFolder((Dictionary<string, object>)reply_data["folder"]);

                handler?.Invoke(folder);
            });
        }

        public void GetFolderContent(UUID agent_id, UUID folder_id, HandleFolderContents handler)
        {
            Dictionary<string, object> sendData = new Dictionary<string, object> {
                { "METHOD", "GETFOLDERCONTENT"},
                {"PRINCIPAL", agent_id.ToString() },
                {"FOLDER", folder_id.ToString() }
            };

            MakeRequest(sendData, (reply_data) =>
            {
                Dictionary<string, object> folders = reply_data.ContainsKey("FOLDERS") ?
                    (Dictionary<string, object>)reply_data["FOLDERS"] : null;
                Dictionary<string, object> items = reply_data.ContainsKey("ITEMS") ?
                    (Dictionary<string, object>)reply_data["ITEMS"] : null;


                List<InventoryFolder> inv_folders = new List<InventoryFolder>();
                List<InventoryItem> inv_items = new List<InventoryItem>();

                if (folders != null)
                    foreach (object o in folders.Values)
                        inv_folders.Add(BuildFolder((Dictionary<string, object>)o));
                if (items != null)
                    foreach (object o in items.Values)
                        inv_items.Add(BuildItem((Dictionary<string, object>)o));

                handler?.Invoke(inv_folders.ToArray(), inv_items.ToArray());
            });
        }

        private InventoryFolder BuildFolder(Dictionary<string, object> data)
        {
            InventoryFolder folder = null;

            try
            {
                UUID id = new UUID(data["ID"].ToString());

                folder = new InventoryFolder(id);
                folder.ParentUUID = new UUID(data["ParentID"].ToString());
                folder.PreferredType = (FolderType)short.Parse(data["Type"].ToString());
                folder.Version = ushort.Parse(data["Version"].ToString());
                folder.Name = data["Name"].ToString();
                folder.OwnerID = new UUID(data["Owner"].ToString());
            }
            catch (Exception e)
            {
                //m_log.Error("[XINVENTORY SERVICES CONNECTOR]: Exception building folder: ", e);
            }

            return folder;
        }

        private InventoryItem BuildItem(Dictionary<string, object> data)
        {
            try
            {
                UUID uuid = new UUID(data["ID"].ToString());
                InventoryType inv_type = (InventoryType)int.Parse(data["InvType"].ToString());

                InventoryItem item = new InventoryItem(inv_type, uuid);

                item.AssetUUID = new UUID(data["AssetID"].ToString());
                item.AssetType = (AssetType)int.Parse(data["AssetType"].ToString());
                item.Name = data["Name"].ToString();
                item.OwnerID = new UUID(data["Owner"].ToString());
                item.ParentUUID = new UUID(data["Folder"].ToString());
                item.CreatorID = new UUID(data["CreatorId"].ToString());
                if (data.ContainsKey("CreatorData"))
                    item.CreatorData = data["CreatorData"].ToString();
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

        Dictionary<string, object> ItemToDictionary(InventoryItem item)
        {
            return new Dictionary<string, object>()
            {
                { "AssetID", item.AssetUUID.ToString() },
                { "AssetType", (uint)item.AssetType },
                { "Name", item.Name },
                { "Owner", item.OwnerID.ToString() },
                { "ID", item.UUID.ToString() },
                { "InvType", (uint)item.InventoryType },
                { "Folder", item.ParentUUID.ToString() },
                { "CreatorId", item.CreatorID.ToString() },
                { "CreatorData", item.CreatorData },
                { "Description", item.Description },
                { "NextPermissions", ((uint)item.Permissions.NextOwnerMask).ToString() },
                { "CurrentPermissions", ((uint)item.Permissions.OwnerMask).ToString() },
                { "BasePermissions", ((uint)item.Permissions.BaseMask).ToString() },
                { "EveryOnePermissions", ((uint)item.Permissions.EveryoneMask).ToString() },
                { "GroupPermissions", ((uint)item.Permissions.GroupMask).ToString() },
                { "GroupID", item.GroupID.ToString() },
                { "GroupOwned", item.GroupOwned.ToString() },
                { "SalePrice", item.SalePrice },
                { "SaleType", (uint)item.SaleType },
                { "Flags", item.Flags },
                { "CreationDate", ((int)Utils.DateTimeToUnixTime(item.CreationDate)).ToString() }
            };
        }

        public void AddFolder(string name, UUID folder_id, UUID parent_id, UUID owner_id, FolderType folder_type, int version, GenericSuccessResult handler)
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

            MakeBoolRequest(sendData, handler);
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
