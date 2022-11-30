using OpenMetaverse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CoolProxy.Plugins.OpenSim
{

    public delegate void GenericSuccessResult(bool result);
    public delegate void HandleGetItem(InventoryItem item);
    public delegate void HandleGetSkeleton(InventoryFolder[] folders);
    public delegate void HandleGetFolder(InventoryFolder folder);
    public delegate void HandleFolderContents(InventoryFolder[] folders, InventoryItem[] items);

    public delegate void HandleResponse(Dictionary<string, object> reply_data);

    public class XInventoryServie
    {
        private CoolProxyFrame Proxy;
        public XInventoryServie(CoolProxyFrame frame)
        {
            Proxy = frame;
        }

        public void AddItem(UUID folderID, UUID itemID, UUID assetID, AssetType assetType, InventoryType invType, uint flags, string item_name, string item_desc, DateTime creationDate, GenericSuccessResult handler = null)
        {
            AddItem(folderID, itemID, assetID, Proxy.Agent.AgentID, assetType, invType, flags, item_name, item_desc, creationDate, 532480, 581635, 581635, 0, 0, UUID.Zero, false, 0, SaleType.Not, UUID.Zero, "", handler);
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

            string target_uri = Proxy.Network.CurrentSim.InvetoryServerURI;

            if (target_uri == string.Empty)
                target_uri = Proxy.Network.CurrentSim.GridURI;

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
}
