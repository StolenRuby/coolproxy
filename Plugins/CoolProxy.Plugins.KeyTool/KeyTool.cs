using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using GridProxy;
using CoolProxy;
using OpenMetaverse;
using OpenMetaverse.Packets;
using static GridProxy.RegionManager;

namespace CoolProxy.Plugins.KeyTool
{
    public enum KT_TYPE
    {
        KT_AGENT,
        KT_TASK,
        KT_GROUP,
        KT_REGION,
        KT_PARCEL,
        KT_ITEM,
        KT_ASSET,
        KT_COUNT
    }

    public enum RESULT
    {
        YES,
        NO,
        MAYBE
    }

    public class KeyTool
    {
        static uint sObjectPropertiesFamilyRequests = 0;
        static uint sParcelInfoRequests = 0;
        static uint sTransferRequests = 0;
        static uint sImageRequests = 0;
        static uint sRegionHandleRequests = 0;
        static uint sGroupNameRequests = 0;
        static uint sAvatarNameRequests = 0;

        static List<KeyTool> mKeyTools = new List<KeyTool>();


        Dictionary<KT_TYPE, bool> mKeyTypesDone = new Dictionary<KT_TYPE, bool>();
        Dictionary<AssetType, bool> mAssetTypesDone = new Dictionary<AssetType, bool>();


        private uint mObjectPropertiesFamilyRequests = 0;
        private uint mParcelInfoRequests = 0;
        private uint mTransferRequests = 0;
        private uint mImageRequests = 0;
        private uint mRegionHandleRequests = 0;
        private uint mGroupNameRequests = 0;
        private uint mAvatarNameRequests = 0;

        private UUID mKey;
        private Action<UUID, KT_TYPE, AssetType, bool, KeyToolForm> keyToolCallback;
        private KeyToolForm keyToolForm;

        private CoolProxyFrame Frame = null;

        public KeyTool(CoolProxyFrame frame, UUID mKey, Action<UUID, KT_TYPE, AssetType, bool, KeyToolForm> keyToolCallback, KeyToolForm keyToolForm)
        {
            this.Frame = frame;
            this.mKey = mKey;
            this.keyToolCallback = keyToolCallback;
            this.keyToolForm = keyToolForm;

            mKeyTools.Add(this);

            tryAgent();
            tryGroup();
            tryTask();
            tryParcel();
            tryRegion();
            tryItem();


            if(frame.Network.CurrentSim.GridURI != string.Empty)
            {
                frame.OpenSim.Assets.GetAssetMetadata(mKey, (x) =>
                {
                    List<AssetType> assetTypes = new List<AssetType>()
                    {
                        AssetType.Bodypart,
                        AssetType.Clothing,
                        AssetType.Gesture,
                        AssetType.Landmark,
                        AssetType.Animation,
                        AssetType.Sound,
                        AssetType.Texture,
                        AssetType.Object,
                        AssetType.Mesh,
                        AssetType.Settings,
                        AssetType.Notecard,
                        AssetType.LSLText
                    };

                    foreach(var t in assetTypes)
                    {
                        keyToolCallback(mKey, KT_TYPE.KT_ASSET, t, t == (x?.Type ?? AssetType.Unknown), keyToolForm);
                    }
                });
            }
            else
            {
                tryAsset(AssetType.Bodypart);
                tryAsset(AssetType.Clothing);
                tryAsset(AssetType.Gesture);
                tryAsset(AssetType.Landmark);
                tryAsset(AssetType.Animation);
                tryAsset(AssetType.Sound);
                tryAsset(AssetType.Texture);
                tryAsset(AssetType.Notecard);
                tryAsset(AssetType.Object);
                tryAsset(AssetType.Mesh);
                tryAsset(AssetType.Settings);
                tryAsset(AssetType.LSLText);
            }
        }

        ~KeyTool()
        {
            // Does this instance own all of the callbacks?
            if (mObjectPropertiesFamilyRequests >= sObjectPropertiesFamilyRequests)
            {
                sObjectPropertiesFamilyRequests = 0;
                mObjectPropertiesFamilyRequests = 0;
                Frame.Network.RemoveDelegate(PacketType.ObjectPropertiesFamily, Direction.Incoming, onObjectPropertiesFamily);
            }
            if (mParcelInfoRequests >= sParcelInfoRequests)
            {
                sParcelInfoRequests = 0;
                mParcelInfoRequests = 0;
                Frame.Network.RemoveDelegate(PacketType.ParcelInfoReply, Direction.Incoming, onParcelInfoReply);
            }
            if (mImageRequests >= sImageRequests)
            {
                sImageRequests = 0;
                mImageRequests = 0;
                Frame.Network.RemoveDelegate(PacketType.ImageData, Direction.Incoming, onImageData);
                Frame.Network.RemoveDelegate(PacketType.ImageNotInDatabase, Direction.Incoming, onImageNotInDatabase);
            }
            if (mTransferRequests >= sTransferRequests)
            {
                sTransferRequests = 0;
                mTransferRequests = 0;
                Frame.Network.RemoveDelegate(PacketType.TransferInfo, Direction.Incoming, onTransferInfo);
            }
            if (mRegionHandleRequests >= sRegionHandleRequests)
            {
                sRegionHandleRequests = 0;
                mRegionHandleRequests = 0;
                Frame.Network.RemoveDelegate(PacketType.RegionIDAndHandleReply, Direction.Incoming, onRegionIDAndHandleReply);
            }
            if (mGroupNameRequests >= sGroupNameRequests)
            {
                sGroupNameRequests = 0;
                mGroupNameRequests = 0;
                Frame.Network.RemoveDelegate(PacketType.UUIDGroupNameReply, Direction.Incoming, onUUIDGroupNameReply);
            }
            if (mAvatarNameRequests >= sAvatarNameRequests)
            {
                sAvatarNameRequests = 0;
                mAvatarNameRequests = 0;
                Frame.Network.RemoveDelegate(PacketType.UUIDNameReply, Direction.Incoming, onUUIDNameReply);
            }

            mKeyTools.Remove(this);
        }

        void tryAgent()
        {
            Frame.Avatars.GetDisplayNames(new List<UUID>() { mKey }, (success, names, rejects) =>
            {
                if (success)
                {
                    //callback(mKey, KT_TYPE.KT_AGENT, AssetType.Unknown, !rejects.Contains(mKey)); // this should work but doesn't...

                    foreach (var name in names)
                    {
                        if(name.ID == mKey)
                        {
                            callback(mKey, KT_TYPE.KT_AGENT, AssetType.Unknown, true);
                            return;
                        }
                    }

                    callback(mKey, KT_TYPE.KT_AGENT, AssetType.Unknown, false);
                }
                else
                {
                    if (names == null && rejects == null) // no cap
                    {
                        if (sAvatarNameRequests <= 0)
                        {
                            sAvatarNameRequests = 0;
                            Frame.Network.AddDelegate(PacketType.UUIDNameReply, Direction.Incoming, onUUIDNameReply);
                        }

                        UUIDNameRequestPacket request = new UUIDNameRequestPacket();
                        request.UUIDNameBlock = new UUIDNameRequestPacket.UUIDNameBlockBlock[1];
                        request.UUIDNameBlock[0] = new UUIDNameRequestPacket.UUIDNameBlockBlock();
                        request.UUIDNameBlock[0].ID = mKey;
                        Frame.Network.InjectPacket(request, Direction.Outgoing);

                        sAvatarNameRequests++;
                        mAvatarNameRequests++;
                    }
                }
            });
        }

        private Packet onUUIDNameReply(Packet packet, RegionProxy endPoint)
        {
            UUIDNameReplyPacket reply = packet as UUIDNameReplyPacket;

            bool was_proxy = false;

            UUIDNameReplyPacket.UUIDNameBlockBlock[] blocks = reply.UUIDNameBlock;
            foreach(var block in blocks)
            {
                bool wanted = callback(block.ID, KT_TYPE.KT_AGENT, AssetType.Unknown, true);
                if (wanted)
                {
                    sAvatarNameRequests--;
                    if (sAvatarNameRequests <= 0)
                    {
                        sAvatarNameRequests = 0;
                        Frame.Network.RemoveDelegate(PacketType.UUIDNameReply, Direction.Incoming, onUUIDNameReply);
                    }

                    was_proxy = true;
                }
            }

            if (was_proxy)
            {
                if (packet.Header.Reliable)
                    Frame.Network.SpoofAck(packet.Header.ID);
                return null;
            }

            return packet;
        }

        void tryGroup()
        {
            if (sGroupNameRequests <= 0)
            {
                sGroupNameRequests = 0;
                Frame.Network.AddDelegate(PacketType.UUIDGroupNameReply, Direction.Incoming, onUUIDGroupNameReply);
            }

            UUIDGroupNameRequestPacket request = new UUIDGroupNameRequestPacket();
            request.UUIDNameBlock = new UUIDGroupNameRequestPacket.UUIDNameBlockBlock[1];
            request.UUIDNameBlock[0] = new UUIDGroupNameRequestPacket.UUIDNameBlockBlock();
            request.UUIDNameBlock[0].ID = mKey;
            Frame.Network.InjectPacket(request, Direction.Outgoing);

            sGroupNameRequests++;
            mGroupNameRequests++;
        }

        private Packet onUUIDGroupNameReply(Packet packet, RegionProxy endPoint)
        {
            bool was_proxy = false;

            UUIDGroupNameReplyPacket reply = packet as UUIDGroupNameReplyPacket;
            UUIDGroupNameReplyPacket.UUIDNameBlockBlock[] blocks = reply.UUIDNameBlock;
            foreach(var block in blocks)
            {
                bool wanted = callback(block.ID, KT_TYPE.KT_GROUP, AssetType.Unknown, !(Utils.BytesToString(block.GroupName) == "Unknown"));
                if (wanted)
                {
                    sGroupNameRequests--;
                    if (sGroupNameRequests <= 0)
                    {
                        sGroupNameRequests = 0;
                        Frame.Network.RemoveDelegate(PacketType.UUIDGroupNameReply, Direction.Incoming, onUUIDGroupNameReply);
                    }

                    was_proxy = true;
                }

            }

            if(was_proxy)
            {
                if (packet.Header.Reliable)
                    Frame.Network.SpoofAck(packet.Header.ID);
                return null;
            }

            return packet;
        }

        void tryItem()
        {
            //if (gInventory.getItem(mKey))
            //    callback(mKey, KT_TYPE.KT_ITEM, AssetType.Unknown, true);
            //else
            //    callback(mKey, KT_TYPE.KT_ITEM, AssetType.Unknown, false);

            if (Frame.Inventory.Store.Contains(mKey))
                callback(mKey, KT_TYPE.KT_ITEM, AssetType.Unknown, true);
            else
                callback(mKey, KT_TYPE.KT_ITEM, AssetType.Unknown, false);
        }

        void tryTask()
        {
            if(sObjectPropertiesFamilyRequests <= 0)
            {
                // Prepare to receive ObjectPropertiesFamily packets
                // Note: no task = no reply
                sObjectPropertiesFamilyRequests = 0;
                Frame.Network.AddDelegate(PacketType.ObjectPropertiesFamily, Direction.Incoming, onObjectPropertiesFamily);
            }

            RequestObjectPropertiesFamilyPacket request = new RequestObjectPropertiesFamilyPacket();
            request.AgentData.AgentID = Frame.Agent.AgentID;
            request.AgentData.SessionID = Frame.Agent.SessionID;
            request.ObjectData.ObjectID = mKey;
            request.ObjectData.RequestFlags = 0;
            Frame.Network.InjectPacket(request, Direction.Outgoing);

            sObjectPropertiesFamilyRequests++;
            mObjectPropertiesFamilyRequests++;
        }

        private Packet onObjectPropertiesFamily(Packet packet, RegionProxy endPoint)
        {
            ObjectPropertiesFamilyPacket family = packet as ObjectPropertiesFamilyPacket;
            
            bool wanted = callback(family.ObjectData.ObjectID, KT_TYPE.KT_TASK, AssetType.Unknown, true);
            if (wanted)
            {
                sObjectPropertiesFamilyRequests--;
                if (sObjectPropertiesFamilyRequests <= 0)
                {
                    sObjectPropertiesFamilyRequests = 0;
                    Frame.Network.RemoveDelegate(PacketType.ObjectPropertiesFamily, Direction.Incoming, onObjectPropertiesFamily);
                }

                if (packet.Header.Reliable)
                    Frame.Network.SpoofAck(packet.Header.ID);
            }

            return packet;
        }

        void tryParcel()
        {
            if (sParcelInfoRequests <= 0)
            {
                // Prepare to receive ParcelInfoReply packets
                // Note: no parcel = no reply
                sParcelInfoRequests = 0;
                Frame.Network.AddDelegate(PacketType.ParcelInfoReply, Direction.Incoming, onParcelInfoReply);
            }

            ParcelInfoRequestPacket request = new ParcelInfoRequestPacket();
            request.AgentData.AgentID = Frame.Agent.AgentID;
            request.AgentData.SessionID = Frame.Agent.SessionID;
            request.Data.ParcelID = mKey;
            Frame.Network.InjectPacket(request, Direction.Outgoing);

            sParcelInfoRequests++;
            mParcelInfoRequests++;
        }

        private Packet onParcelInfoReply(Packet packet, RegionProxy endPoint)
        {
            ParcelInfoReplyPacket reply = packet as ParcelInfoReplyPacket;
            
            bool wanted = callback(reply.Data.ParcelID, KT_TYPE.KT_PARCEL, AssetType.Unknown, true);
            if (wanted)
            {
                sParcelInfoRequests--;
                if (sParcelInfoRequests <= 0)
                {
                    sParcelInfoRequests = 0;
                    Frame.Network.RemoveDelegate(PacketType.ParcelInfoReply, Direction.Incoming, onParcelInfoReply);
                }

                if (packet.Header.Reliable)
                    Frame.Network.SpoofAck(packet.Header.ID);
            }

            return packet;
        }

        void tryRegion()
        {
            if(sRegionHandleRequests <= 0)
            {
                sRegionHandleRequests = 0;
                Frame.Network.AddDelegate(PacketType.RegionIDAndHandleReply, Direction.Incoming, onRegionIDAndHandleReply);
            }

            RegionHandleRequestPacket request = new RegionHandleRequestPacket();
            request.RequestBlock.RegionID = mKey;
            Frame.Network.InjectPacket(request, Direction.Outgoing);
        }

        private Packet onRegionIDAndHandleReply(Packet packet, RegionProxy endPoint)
        {
            RegionIDAndHandleReplyPacket reply = packet as RegionIDAndHandleReplyPacket;
            bool wanted = callback(reply.ReplyBlock.RegionID, KT_TYPE.KT_REGION, AssetType.Unknown, true);
            if (wanted)
            {
                sRegionHandleRequests--;
                if (sRegionHandleRequests <= 0)
                {
                    sRegionHandleRequests = 0;
                    Frame.Network.RemoveDelegate(PacketType.RegionIDAndHandleReply, Direction.Incoming, onRegionIDAndHandleReply);
                }

                if (packet.Header.Reliable)
                    Frame.Network.SpoofAck(packet.Header.ID);
            }

            return packet;
        }

        void tryAsset(AssetType assetType)
        {
            if(assetType == AssetType.Texture)
            {
                if (sImageRequests <= 0)
                {
                    sTransferRequests = 0;
                    Frame.Network.AddDelegate(PacketType.ImageData, Direction.Incoming, onImageData);
                    Frame.Network.AddDelegate(PacketType.ImageNotInDatabase, Direction.Incoming, onImageNotInDatabase);
                }


                // Build the packet and send it
                RequestImagePacket request = new RequestImagePacket();
                request.AgentData.AgentID = Frame.Agent.AgentID;
                request.AgentData.SessionID = Frame.Agent.SessionID;
                request.RequestImage = new RequestImagePacket.RequestImageBlock[1];
                request.RequestImage[0] = new RequestImagePacket.RequestImageBlock();
                request.RequestImage[0].Image = mKey;
                request.RequestImage[0].DiscardLevel = 0;
                request.RequestImage[0].DownloadPriority = 101300;
                request.RequestImage[0].Packet = 0;
                request.RequestImage[0].Type = 0;
                
                //send packet to SL
                Frame.Network.InjectPacket(request, Direction.Outgoing);

                sImageRequests++;
                mImageRequests++;
            }
            else
            {
                if (sTransferRequests <= 0)
                {
                    sTransferRequests = 0;
                    Frame.Network.AddDelegate(PacketType.TransferInfo, Direction.Incoming, onTransferInfo);
                }
                
                // Build the request packet and send it
                TransferRequestPacket request = new TransferRequestPacket();
                request.TransferInfo.ChannelType = (int)ChannelType.Asset;
                request.TransferInfo.Priority = 101.0f;
                request.TransferInfo.SourceType = (int)SourceType.Asset;
                request.TransferInfo.TransferID = UUID.Random();

                byte[] paramField = new byte[20];
                Buffer.BlockCopy(mKey.GetBytes(), 0, paramField, 0, 16);
                Buffer.BlockCopy(Utils.IntToBytes((int)assetType), 0, paramField, 16, 4);

                request.TransferInfo.Params = paramField;
                
                Frame.Network.InjectPacket(request, Direction.Outgoing);

                sTransferRequests++;
                mTransferRequests++;
            }
        }

        private Packet onImageData(Packet packet, RegionProxy endPoint)
        {
            ImageDataPacket image = packet as ImageDataPacket;

            bool wanted = callback(image.ImageID.ID, KT_TYPE.KT_ASSET, AssetType.Texture, true);
            if (wanted)
            {
                sImageRequests--;
                if(sImageRequests <= 0)
                {
                    sImageRequests = 0;
                    Frame.Network.RemoveDelegate(PacketType.ImageData, Direction.Incoming, onImageData);
                    Frame.Network.RemoveDelegate(PacketType.ImageNotInDatabase, Direction.Incoming, onImageNotInDatabase);
                }

                if (packet.Header.Reliable)
                    Frame.Network.SpoofAck(packet.Header.ID);
            }

            return packet;
        }

        private Packet onImageNotInDatabase(Packet packet, RegionProxy endPoint)
        {
            ImageNotInDatabasePacket image = packet as ImageNotInDatabasePacket;

            bool wanted = callback(image.ImageID.ID, KT_TYPE.KT_ASSET, AssetType.Texture, false);
            if (wanted)
            {
                sImageRequests--;
                if (sImageRequests <= 0)
                {
                    sImageRequests = 0;
                    Frame.Network.RemoveDelegate(PacketType.ImageData, Direction.Incoming, onImageData);
                    Frame.Network.RemoveDelegate(PacketType.ImageNotInDatabase, Direction.Incoming, onImageNotInDatabase);
                }

                if (packet.Header.Reliable)
                    Frame.Network.SpoofAck(packet.Header.ID);
            }

            return packet;
        }

        private Packet onTransferInfo(Packet packet, RegionProxy endPoint)
        {
            TransferInfoPacket info = packet as TransferInfoPacket;
            
            UUID assetID = new UUID(info.TransferInfo.Params, 0);
            AssetType assetType = (AssetType)info.TransferInfo.Params[16];

            bool wanted = callback(assetID, KT_TYPE.KT_ASSET, assetType, info.TransferInfo.Status == 0); // LLTS_OK (e_status_codes)
            if (wanted)
            {
                sTransferRequests--;
                if (sTransferRequests <= 0)
                {
                    sTransferRequests = 0;
                    Frame.Network.RemoveDelegate(PacketType.TransferInfo, Direction.Incoming, onTransferInfo);
                }

                if (packet.Header.Reliable)
                    Frame.Network.SpoofAck(packet.Header.ID);

                return null;
            }

            return packet;
        }

        static bool callback(UUID key, KT_TYPE keyType, AssetType assetType, bool is_type)
        {
            bool wanted = false;

            foreach(KeyTool tool in mKeyTools)
            {
                if(tool.mKey == key)
                {
                    bool call = false;

                    if(keyType != KT_TYPE.KT_ASSET)
                    {
                        if(!tool.mKeyTypesDone.ContainsKey(keyType))
                        {
                            tool.mKeyTypesDone[keyType] = true;
                            call = true;
                        }
                    }
                    else
                    {
                        if (!tool.mAssetTypesDone.ContainsKey(assetType))
                        {
                            tool.mAssetTypesDone[assetType] = true;
                            call = true;
                        }
                    }

                    if (call)
                    {
                        tool.mKeyTypesDone[keyType] = is_type;
                        
                        if (tool.keyToolCallback != null)
                        {
                            tool.keyToolCallback(key, keyType, assetType, is_type, tool.keyToolForm);
                            wanted = true;
                        }
                    }

                    if (keyType == KT_TYPE.KT_TASK)
                        tool.mObjectPropertiesFamilyRequests--;
                    else if (keyType == KT_TYPE.KT_PARCEL)
                        tool.mParcelInfoRequests--;
                    else if (keyType == KT_TYPE.KT_ASSET)
                    {
                        if (assetType == AssetType.Texture)
                            tool.mImageRequests--;
                        else
                            tool.mTransferRequests--;
                    }
                }
            }

            return wanted;
        }
        
        public static string aWhat(KT_TYPE ktype, AssetType atype = AssetType.Unknown)
        {
            string name = "unknown";

            switch (ktype)
            {
                case KT_TYPE.KT_AGENT:
                    name = "agent";
                    break;
                case KT_TYPE.KT_TASK:
                    name = "task";
                    break;
                case KT_TYPE.KT_GROUP:
                    name = "group";
                    break;
                case KT_TYPE.KT_REGION:
                    name = "region";
                    break;
                case KT_TYPE.KT_PARCEL:
                    name = "parcel";
                    break;
                case KT_TYPE.KT_ITEM:
                    name = "item";
                    break;
                case KT_TYPE.KT_ASSET:
                    name = atype.ToString().ToLower() + " asset";
                    break;
                default:
                    break;
            }

            return name;
        }

        public static void openKey(KT_TYPE kt_type, AssetType asset_type, UUID asset_id)
        {
            if (kt_type == KT_TYPE.KT_ASSET)
            {
                if(KeyToolPlugin.Mode == OpenAssetMode.SuperSuitcase)
                {
                    UUID folder_id = KeyToolPlugin.Proxy.Inventory.SuitcaseID != UUID.Zero ?
                        KeyToolPlugin.Proxy.Inventory.FindSuitcaseFolderForType((FolderType)asset_type) :
                        KeyToolPlugin.Proxy.Inventory.FindFolderForType(asset_type);

                    UUID item_id = UUID.Random();

                    KeyToolPlugin.Proxy.OpenSim.XInventory.AddItem(
                        folder_id, item_id, asset_id, KeyToolPlugin.Proxy.Agent.AgentID,
                        asset_type, (InventoryType)asset_type, 0, asset_id.ToString(), "", DateTime.UtcNow, success =>
                        {
                            if (success)
                            {
                                KeyToolPlugin.Proxy.Inventory.RequestFetchInventory(item_id, KeyToolPlugin.Proxy.Agent.AgentID, false);
                            }
                            else KeyToolPlugin.Proxy.AlertMessage("Error adding item to suitcase!", false);
                        });
                }
                else if(KeyToolPlugin.Mode == OpenAssetMode.NotecardMagic)
                {
                    UUID folder_id = KeyToolPlugin.Proxy.Inventory.FindFolderForType(asset_type);
                    if (KeyToolPlugin.NotecardMagic != null)
                    {
                        KeyToolPlugin.NotecardMagic.GetAsset(folder_id, asset_id, asset_type, (InventoryType)asset_type);
                    }
                }
                else
                {
                    KeyToolPlugin.Proxy.SayToUser(string.Format("{0} is a {1}", asset_id, asset_type));
                }
            }
            else if(kt_type == KT_TYPE.KT_AGENT)
            {
                KeyToolPlugin.Proxy.SayToUser(string.Format("{0} is an agent secondlife:///app/agent/{0}/about", asset_id));
            }
            else if(kt_type == KT_TYPE.KT_GROUP)
            {
                KeyToolPlugin.Proxy.SayToUser(string.Format("{0} is a group secondlife:///app/group/{0}/about", asset_id));
            }
            else if(kt_type == KT_TYPE.KT_ITEM)
            {
                KeyToolPlugin.Proxy.SayToUser(string.Format("{0} is an inventory item secondlife:///app/inventory/{0}/select", asset_id));
            }
            else if (kt_type == KT_TYPE.KT_PARCEL)
            {
                KeyToolPlugin.Proxy.SayToUser(string.Format("{0} is a parcel secondlife:///app/parcel/{0}/about", asset_id));
            }
            else if (kt_type == KT_TYPE.KT_REGION)
            {
                KeyToolPlugin.Proxy.SayToUser(string.Format("{0} is a region secondlife:///app/region/{0}/about", asset_id));
            }
        }
    }
}
