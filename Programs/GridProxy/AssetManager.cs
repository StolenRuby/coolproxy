/*
 * Copyright (c) 2006-2016, openmetaverse.co
 * All rights reserved.
 *
 * - Redistribution and use in source and binary forms, with or without 
 *   modification, are permitted provided that the following conditions are met:
 *
 * - Redistributions of source code must retain the above copyright notice, this
 *   list of conditions and the following disclaimer.
 * - Neither the name of the openmetaverse.co nor the names 
 *   of its contributors may be used to endorse or promote products derived from
 *   this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" 
 * AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE 
 * IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE 
 * ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE 
 * LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR 
 * CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF 
 * SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS 
 * INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN 
 * CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) 
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE 
 * POSSIBILITY OF SUCH DAMAGE.
 */

using System;
using System.Collections.Generic;
using System.Threading;
using System.IO;
using System.Net;
using OpenMetaverse;
using OpenMetaverse.Packets;
using OpenMetaverse.Assets;
using OpenMetaverse.Http;
using OpenMetaverse.StructuredData;
using OpenMetaverse.Messages.Linden;
using static GridProxy.RegionManager;

namespace GridProxy
{
    #region Transfer Classes

    /// <summary>
    /// 
    /// </summary>
    public class Transfer
    {
        public UUID ID;
        public int Size;
        public byte[] AssetData = Utils.EmptyBytes;
        public int Transferred;
        public bool Success;
        public AssetType AssetType;

        private int transferStart;

        /// <summary>Number of milliseconds passed since the last transfer
        /// packet was received</summary>
        public int TimeSinceLastPacket
        {
            get { return Environment.TickCount - transferStart; }
            internal set { transferStart = Environment.TickCount + value; }
        }

        public Transfer()
        {
            AssetData = Utils.EmptyBytes;
            transferStart = Environment.TickCount;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class AssetDownload : Transfer
    {
        public UUID AssetID;
        public ChannelType Channel;
        public SourceType Source;
        public TargetType Target;
        public StatusCode Status;
        public float Priority;
        public RegionProxy Simulator;
        public AssetManager.AssetReceivedCallback Callback;

        public int nextPacket;
        public InternalDictionary<int, byte[]> outOfOrderPackets;
        internal ManualResetEvent HeaderReceivedEvent = new ManualResetEvent(false);

        public AssetDownload()
            : base()
        {
            nextPacket = 0;
            outOfOrderPackets = new InternalDictionary<int, byte[]>();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class XferDownload : Transfer
    {
        public ulong XferID;
        public UUID VFileID;
        public uint PacketNum;
        public string Filename = String.Empty;
        public TransferError Error = TransferError.None;

        public XferDownload()
            : base()
        {
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class ImageDownload : Transfer
    {
        public ushort PacketCount;
        public ImageCodec Codec;
        public Simulator Simulator;
        public SortedList<ushort, ushort> PacketsSeen;
        public ImageType ImageType;
        public int DiscardLevel;
        public float Priority;
        internal int InitialDataSize;
        internal ManualResetEvent HeaderReceivedEvent = new ManualResetEvent(false);

        public ImageDownload()
            : base()
        {
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class AssetUpload : Transfer
    {
        public UUID AssetID;
        public AssetType Type;
        public ulong XferID;
        public uint PacketNum;

        public AssetUpload()
            : base()
        {
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class ImageRequest
    {
        public UUID ImageID;
        public ImageType Type;
        public float Priority;
        public int DiscardLevel;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="imageid"></param>
        /// <param name="type"></param>
        /// <param name="priority"></param>
        /// <param name="discardLevel"></param>
        public ImageRequest(UUID imageid, ImageType type, float priority, int discardLevel)
        {
            ImageID = imageid;
            Type = type;
            Priority = priority;
            DiscardLevel = discardLevel;
        }

    }
    #endregion Transfer Classes

    /// <summary>
    /// 
    /// </summary>
    public class AssetManager
    {
        /// <summary>Number of milliseconds to wait for a transfer header packet if out of order data was received</summary>
        const int TRANSFER_HEADER_TIMEOUT = 1000 * 15;

        #region Delegates
        /// <summary>
        /// Callback used for various asset download requests
        /// </summary>
        /// <param name="transfer">Transfer information</param>
        /// <param name="asset">Downloaded asset, null on fail</param>
        public delegate void AssetReceivedCallback(AssetDownload transfer, Asset asset);
        /// <summary>
        /// Callback used upon competition of baked texture upload
        /// </summary>
        /// <param name="newAssetID">Asset UUID of the newly uploaded baked texture</param>
        public delegate void BakedTextureUploadedCallback(UUID newAssetID);
        /// <summary>
        /// A callback that fires upon the completition of the RequestMesh call
        /// </summary>
        /// <param name="success">Was the download successfull</param>
        /// <param name="assetMesh">Resulting mesh or null on problems</param>
        #endregion Delegates

        #region Events

        #region XferReceived
        /// <summary>The event subscribers. null if no subcribers</summary>
        private EventHandler<XferReceivedEventArgs> m_XferReceivedEvent;

        /// <summary>Raises the XferReceived event</summary>
        /// <param name="e">A XferReceivedEventArgs object containing the
        /// data returned from the simulator</param>
        protected virtual void OnXferReceived(XferReceivedEventArgs e)
        {
            EventHandler<XferReceivedEventArgs> handler = m_XferReceivedEvent;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>Thread sync lock object</summary>
        private readonly object m_XferReceivedLock = new object();

        /// <summary>Raised when the simulator responds sends </summary>
        public event EventHandler<XferReceivedEventArgs> XferReceived
        {
            add { lock (m_XferReceivedLock) { m_XferReceivedEvent += value; } }
            remove { lock (m_XferReceivedLock) { m_XferReceivedEvent -= value; } }
        }
        #endregion

        #region AssetUploaded
        /// <summary>The event subscribers. null if no subcribers</summary>
        private EventHandler<AssetUploadEventArgs> m_AssetUploadedEvent;

        /// <summary>Raises the AssetUploaded event</summary>
        /// <param name="e">A AssetUploadedEventArgs object containing the
        /// data returned from the simulator</param>
        protected virtual void OnAssetUploaded(AssetUploadEventArgs e)
        {
            EventHandler<AssetUploadEventArgs> handler = m_AssetUploadedEvent;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>Thread sync lock object</summary>
        private readonly object m_AssetUploadedLock = new object();

        /// <summary>Raised during upload completes</summary>
        public event EventHandler<AssetUploadEventArgs> AssetUploaded
        {
            add { lock (m_AssetUploadedLock) { m_AssetUploadedEvent += value; } }
            remove { lock (m_AssetUploadedLock) { m_AssetUploadedEvent -= value; } }
        }
        #endregion

        #region UploadProgress
        /// <summary>The event subscribers. null if no subcribers</summary>
        private EventHandler<AssetUploadEventArgs> m_UploadProgressEvent;

        /// <summary>Raises the UploadProgress event</summary>
        /// <param name="e">A UploadProgressEventArgs object containing the
        /// data returned from the simulator</param>
        protected virtual void OnUploadProgress(AssetUploadEventArgs e)
        {
            EventHandler<AssetUploadEventArgs> handler = m_UploadProgressEvent;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>Thread sync lock object</summary>
        private readonly object m_UploadProgressLock = new object();

        /// <summary>Raised during upload with progres update</summary>
        public event EventHandler<AssetUploadEventArgs> UploadProgress
        {
            add { lock (m_UploadProgressLock) { m_UploadProgressEvent += value; } }
            remove { lock (m_UploadProgressLock) { m_UploadProgressEvent -= value; } }
        }
        #endregion UploadProgress

        #region InitiateDownload
        /// <summary>The event subscribers. null if no subcribers</summary>
        private EventHandler<InitiateDownloadEventArgs> m_InitiateDownloadEvent;

        /// <summary>Raises the InitiateDownload event</summary>
        /// <param name="e">A InitiateDownloadEventArgs object containing the
        /// data returned from the simulator</param>
        protected virtual void OnInitiateDownload(InitiateDownloadEventArgs e)
        {
            EventHandler<InitiateDownloadEventArgs> handler = m_InitiateDownloadEvent;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>Thread sync lock object</summary>
        private readonly object m_InitiateDownloadLock = new object();

        /// <summary>Fired when the simulator sends an InitiateDownloadPacket, used to download terrain .raw files</summary>
        public event EventHandler<InitiateDownloadEventArgs> InitiateDownload
        {
            add { lock (m_InitiateDownloadLock) { m_InitiateDownloadEvent += value; } }
            remove { lock (m_InitiateDownloadLock) { m_InitiateDownloadEvent -= value; } }
        }
        #endregion InitiateDownload

        #region ImageReceiveProgress
        /// <summary>The event subscribers. null if no subcribers</summary>
        private EventHandler<ImageReceiveProgressEventArgs> m_ImageReceiveProgressEvent;

        /// <summary>Raises the ImageReceiveProgress event</summary>
        /// <param name="e">A ImageReceiveProgressEventArgs object containing the
        /// data returned from the simulator</param>
        protected virtual void OnImageReceiveProgress(ImageReceiveProgressEventArgs e)
        {
            EventHandler<ImageReceiveProgressEventArgs> handler = m_ImageReceiveProgressEvent;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>Thread sync lock object</summary>
        private readonly object m_ImageReceiveProgressLock = new object();

        /// <summary>Fired when a texture is in the process of being downloaded by the TexturePipeline class</summary>
        public event EventHandler<ImageReceiveProgressEventArgs> ImageReceiveProgress
        {
            add { lock (m_ImageReceiveProgressLock) { m_ImageReceiveProgressEvent += value; } }
            remove { lock (m_ImageReceiveProgressLock) { m_ImageReceiveProgressEvent -= value; } }
        }
        #endregion ImageReceiveProgress

        #region Asset Downloads
        public event AssetDownloadProgress DownloadProgress;
        #endregion

        #endregion Events

        /// <summary>Texture download cache</summary>
        public AssetCache Cache;

        //private TexturePipeline Texture;

        //private DownloadManager HttpDownloads;

        private ProxyFrame Frame;

        private Dictionary<UUID, Transfer> Transfers = new Dictionary<UUID, Transfer>();

        private AssetUpload PendingUpload;
        private object PendingUploadLock = new object();
        private volatile bool WaitingForUploadConfirm = false;



        private static readonly Dictionary<AssetType, string> assetRequestStrings = new Dictionary<AssetType, string>()
        {
            {AssetType.Texture, "texture_id" },
            {AssetType.Sound, "sound_id" },
            {AssetType.CallingCard, "callcard_id" },
            {AssetType.Landmark, "landmark_id" },
            {AssetType.LSLText, "script_id" },
            {AssetType.Clothing, "clothing_id" },
            {AssetType.Object, "object_id" },
            {AssetType.Notecard, "notecard_id" },
            //{AssetType.LSLText, "lsltext_id" },
            {AssetType.LSLBytecode, "lslbyte_id" },
            {AssetType.TextureTGA, "txtr_tga_id" },
            {AssetType.Bodypart, "bodypart_id" },
            {AssetType.SoundWAV, "snd_wav_id" },
            {AssetType.ImageTGA, "img_tga_id" },
            {AssetType.ImageJPEG, "jpeg_id" },
            {AssetType.Animation, "animatn_id" },
            {AssetType.Gesture, "gesture_id" },
            {AssetType.Mesh, "mesh_id" },
            //{AssetType.Settings, "settings_id" },
        };


        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="frame">A reference to the GridClient object</param>
        public AssetManager(ProxyFrame frame)
        {
            Frame = frame;
            Cache = new AssetCache(frame);
            //Texture = new TexturePipeline(frame);
            //HttpDownloads = new DownloadManager();

            // Transfer packets for downloading large assets
            Frame.Network.AddDelegate(PacketType.TransferInfo, Direction.Incoming, TransferInfoHandler);
            Frame.Network.AddDelegate(PacketType.TransferPacket, Direction.Incoming, TransferPacketHandler);

            // Xfer packets for uploading large assets
            Frame.Network.AddDelegate(PacketType.RequestXfer, Direction.Incoming, RequestXferHandler);
            Frame.Network.AddDelegate(PacketType.ConfirmXferPacket, Direction.Incoming, ConfirmXferPacketHandler);
            Frame.Network.AddDelegate(PacketType.AssetUploadComplete, Direction.Incoming, AssetUploadCompleteHandler);

            // Xfer packets for downloading misc assets
            Frame.Network.AddDelegate(PacketType.SendXferPacket, Direction.Incoming, SendXferPacketHandler);
            Frame.Network.AddDelegate(PacketType.AbortXfer, Direction.Incoming, AbortXferHandler);

            // Simulator is responding to a request to download a file
            Frame.Network.AddDelegate(PacketType.InitiateDownload, Direction.Incoming, InitiateDownloadPacketHandler);
        }


        public void RequestAsset(UUID assetID, AssetType type, AssetReceivedCallback callback)
        {
            RequestAsset(assetID, type, true, callback);
        }

        /// <summary>
        /// Request an asset download
        /// </summary>
        /// <param name="assetID">Asset UUID</param>
        /// <param name="type">Asset type, must be correct for the transfer to succeed</param>
        /// <param name="priority">Whether to give this transfer an elevated priority</param>
        /// <param name="callback">The callback to fire when the simulator responds with the asset data</param>
        public void RequestAsset(UUID assetID, AssetType type, bool priority, AssetReceivedCallback callback)
        {
            RequestAsset(assetID, type, priority, SourceType.Asset, UUID.Random(), callback);
        }

        /// <summary>
        /// Request an asset download
        /// </summary>
        /// <param name="assetID">Asset UUID</param>
        /// <param name="type">Asset type, must be correct for the transfer to succeed</param>
        /// <param name="priority">Whether to give this transfer an elevated priority</param>
        /// <param name="sourceType">Source location of the requested asset</param>
        /// <param name="callback">The callback to fire when the simulator responds with the asset data</param>
        public void RequestAsset(UUID assetID, AssetType type, bool priority, SourceType sourceType, AssetReceivedCallback callback)
        {
            RequestAsset(assetID, type, priority, sourceType, UUID.Random(), callback);
        }

        /// <summary>
        /// Request an asset download
        /// </summary>
        /// <param name="assetID">Asset UUID</param>
        /// <param name="type">Asset type, must be correct for the transfer to succeed</param>
        /// <param name="priority">Whether to give this transfer an elevated priority</param>
        /// <param name="sourceType">Source location of the requested asset</param>
        /// <param name="transactionID">UUID of the transaction</param>
        /// <param name="callback">The callback to fire when the simulator responds with the asset data</param>
        public void RequestAsset(UUID assetID, AssetType type, bool priority, SourceType sourceType, UUID transactionID, AssetReceivedCallback callback)
        {
            RequestAsset(assetID, UUID.Zero, UUID.Zero, type, priority, sourceType, transactionID, callback);
        }

        /// <summary>
        /// Request an asset download
        /// </summary>
        /// <param name="assetID">Asset UUID</param>
        /// <param name="type">Asset type, must be correct for the transfer to succeed</param>
        /// <param name="priority">Whether to give this transfer an elevated priority</param>
        /// <param name="sourceType">Source location of the requested asset</param>
        /// <param name="transactionID">UUID of the transaction</param>
        /// <param name="callback">The callback to fire when the simulator responds with the asset data</param>
        public void RequestAsset(UUID assetID, UUID itemID, UUID taskID, AssetType asset_type, bool priority, SourceType sourceType, UUID transactionID, AssetReceivedCallback callback, bool allow_fallback = false)
        {
            if (assetID == UUID.Zero || callback == null)
                return;

            byte[] assetData;
            // Do we have this image in the cache?
            if (Frame.Assets.Cache.HasAsset(assetID)
                && (assetData = Frame.Assets.Cache.GetCachedAssetBytes(assetID)) != null)
            {
                AssetDownload image = new AssetDownload();
                image.ID = assetID;
                image.AssetData = assetData;
                image.Size = image.AssetData.Length;
                image.Transferred = image.AssetData.Length;
                //image.ImageType = imageType;
                image.AssetType = asset_type;
                image.Success = true;

                callback(image, new AssetTexture(image.ID, image.AssetData));

                DownloadProgress?.Invoke(new AssetDownloadProgressEventArgs(AssetDownloadState.Finished, assetID, asset_type, image.Size, image.Transferred));

                //FireImageProgressEvent(image.ID, image.Transferred, image.Size);
                return;
            }

            if (!Frame.Config.USE_HTTP_ASSETS)
            {
                RequestAssetUDP(assetID, itemID, taskID, asset_type, priority, sourceType, transactionID, callback);
                return;
            }

            CapsBase.DownloadProgressEventHandler progressHandler = null;

            //if (progress)
            {
                progressHandler = (HttpWebRequest request, HttpWebResponse response, int bytesReceived, int totalBytesToReceive) =>
                {
                    DownloadProgress?.Invoke(new AssetDownloadProgressEventArgs(AssetDownloadState.Downloading, assetID, asset_type, bytesReceived, totalBytesToReceive));
                    //FireImageProgressEvent(assetID, bytesReceived, totalBytesToReceive);
                };
            }

            string cap_url = string.Empty;

            if (!Frame.Network.CurrentSim.Caps.TryGetValue("ViewerAsset", out cap_url))
            {
                if (asset_type == AssetType.Texture)
                    Frame.Network.CurrentSim.Caps.TryGetValue("GetTexture", out cap_url);
                else if (asset_type == AssetType.Mesh)
                    Frame.Network.CurrentSim.Caps.TryGetValue("GetMesh", out cap_url);
            }

            Uri url = new Uri(cap_url);

            string str_type = assetRequestStrings[asset_type];

            DownloadRequest req = new DownloadRequest(
                new Uri(string.Format("{0}/?{1}={2}", url.ToString(), str_type, assetID.ToString())),
                Frame.Config.CAPS_TIMEOUT,
                "", // todo: this! //"image/x-j2c",
                progressHandler,
                (HttpWebRequest request, HttpWebResponse response, byte[] responseData, Exception error) =>
                {
                    if (error == null && responseData != null) // success
                    {
                        AssetDownload image = new AssetDownload();
                        image.ID = assetID;
                        image.AssetData = responseData;
                        image.Size = image.AssetData.Length;
                        image.Transferred = image.AssetData.Length;
                        //image.ImageType = imageType;
                        image.AssetType = asset_type;
                        image.Success = true;

                        var asset = CreateAssetWrapper(asset_type);
                        asset.AssetData = responseData;
                        asset.AssetID = assetID;
                        //asset.Decode();

                        DownloadProgress?.Invoke(new AssetDownloadProgressEventArgs(AssetDownloadState.Finished, assetID, asset_type, image.Size, image.Transferred));

                        callback(image, asset);
                        //FireImageProgressEvent(image.ID, image.Transferred, image.Size);

                        Frame.Assets.Cache.SaveAssetToCache(assetID, responseData);
                    }
                    else if(allow_fallback) // download failed
                    {
                        Logger.Log(
                            string.Format("Failed to fetch asset {0} over HTTP, falling back to UDP: {1}",
                                assetID,
                                (error == null) ? "" : error.Message
                            ),
                            Helpers.LogLevel.Warning);

                        RequestAssetUDP(assetID, itemID, taskID, asset_type, priority, sourceType, transactionID, callback);
                    }
                    else
                    {
                        AssetDownload image = new AssetDownload();
                        image.ID = assetID;
                        image.AssetData = null;
                        image.Size = 0;
                        image.Transferred = 0;
                        image.AssetType = asset_type;
                        image.Success = false;

                        DownloadProgress?.Invoke(new AssetDownloadProgressEventArgs(AssetDownloadState.Failed, assetID, asset_type, image.Size, image.Transferred));
                        callback(image, null);
                    }
                }
            );

            Frame.HTTP.Downloads.QueueDownload(req);
            DownloadProgress?.Invoke(new AssetDownloadProgressEventArgs(AssetDownloadState.Queued, assetID, asset_type, 0, 0));
        }

        public delegate void AssetDataCallback(bool success, byte[] data);
        public void EasyDownloadAsset(UUID asset_id, AssetType asset_type, AssetDataCallback callback)
        {

            string cap_url = string.Empty;

            if (!Frame.Network.CurrentSim.Caps.TryGetValue("ViewerAsset", out cap_url))
            {
                if (asset_type == AssetType.Texture)
                    Frame.Network.CurrentSim.Caps.TryGetValue("GetTexture", out cap_url);
                else if (asset_type == AssetType.Mesh)
                    Frame.Network.CurrentSim.Caps.TryGetValue("GetMesh", out cap_url);
            }

            if (cap_url == string.Empty)
            {
                callback(false, null);
                return;
            }

            string str_type = assetRequestStrings[asset_type];

            string url = string.Format("{0}/?{1}={2}", cap_url, str_type, asset_id.ToString());

            WebClient webClient = new WebClient();
            webClient.DownloadDataCompleted += (x, y) =>
            {
                try
                {
                    if(y.Error == null)
                    {
                        callback(true, y.Result);
                    }
                    else
                    {
                        callback(false, null);
                    }
                }
                catch
                {
                    callback(false, null);
                }
            };
            webClient.DownloadDataAsync(new Uri(url));
        }

        private void RequestAssetUDP(UUID assetID, UUID itemID, UUID taskID, AssetType type, bool priority, SourceType sourceType, UUID transactionID, AssetReceivedCallback callback)
        {
            AssetDownload transfer = new AssetDownload();
            transfer.ID = transactionID;
            transfer.AssetID = assetID;
            //transfer.AssetType = type; // Set in TransferInfoHandler.
            transfer.Priority = 100.0f + (priority ? 1.0f : 0.0f);
            transfer.Channel = ChannelType.Asset;
            transfer.Source = sourceType;
            transfer.Simulator = Frame.Network.CurrentSim;
            transfer.Callback = callback;

            DownloadProgress?.Invoke(new AssetDownloadProgressEventArgs(AssetDownloadState.Started, transfer.AssetID, type, 0, 0));

            // Add this transfer to the dictionary
            lock (Transfers) Transfers[transfer.ID] = transfer;

            // Build the request packet and send it
            TransferRequestPacket request = new TransferRequestPacket();
            request.TransferInfo.ChannelType = (int)transfer.Channel;
            request.TransferInfo.Priority = transfer.Priority;
            request.TransferInfo.SourceType = (int)transfer.Source;
            request.TransferInfo.TransferID = transfer.ID;

            byte[] paramField = taskID == UUID.Zero ? new byte[20] : new byte[96];
            Buffer.BlockCopy(assetID.GetBytes(), 0, paramField, 0, 16);
            Buffer.BlockCopy(Utils.IntToBytes((int)type), 0, paramField, 16, 4);

            if (taskID != UUID.Zero)
            {
                Buffer.BlockCopy(taskID.GetBytes(), 0, paramField, 48, 16);
                Buffer.BlockCopy(itemID.GetBytes(), 0, paramField, 64, 16);
                Buffer.BlockCopy(assetID.GetBytes(), 0, paramField, 80, 16);
            }
            request.TransferInfo.Params = paramField;

            Frame.Network.CurrentSim.Inject(request, Direction.Outgoing);
        }

        /// <summary>
        /// Request an asset download through the almost deprecated Xfer system
        /// </summary>
        /// <param name="filename">Filename of the asset to request</param>
        /// <param name="deleteOnCompletion">Whether or not to delete the asset
        /// off the server after it is retrieved</param>
        /// <param name="useBigPackets">Use large transfer packets or not</param>
        /// <param name="vFileID">UUID of the file to request, if filename is
        /// left empty</param>
        /// <param name="vFileType">Asset type of <code>vFileID</code>, or
        /// <code>AssetType.Unknown</code> if filename is not empty</param>
        /// <param name="fromCache">Sets the FilePath in the request to Cache
        /// (4) if true, otherwise Unknown (0) is used</param>
        /// <returns></returns>
        public ulong RequestAssetXfer(string filename, bool deleteOnCompletion, bool useBigPackets, UUID vFileID, AssetType vFileType, bool fromCache)
        {
            UUID uuid = UUID.Random();
            ulong id = uuid.GetULong();

            XferDownload transfer = new XferDownload();
            transfer.XferID = id;
            transfer.ID = new UUID(id); // Our dictionary tracks transfers with UUIDs, so convert the ulong back
            transfer.Filename = filename;
            transfer.VFileID = vFileID;
            transfer.AssetType = vFileType;

            // Add this transfer to the dictionary
            lock (Transfers) Transfers[transfer.ID] = transfer;

            RequestXferPacket request = new RequestXferPacket();
            request.XferID.ID = id;
            request.XferID.Filename = Utils.StringToBytes(filename);
            request.XferID.FilePath = fromCache ? (byte)4 : (byte)0;
            request.XferID.DeleteOnCompletion = deleteOnCompletion;
            request.XferID.UseBigPackets = useBigPackets;
            request.XferID.VFileID = vFileID;
            request.XferID.VFileType = (short)vFileType;

            Frame.Network.CurrentSim.Inject(request, Direction.Outgoing);

            return id;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="assetID">Use UUID.Zero if you do not have the 
        /// asset ID but have all the necessary permissions</param>
        /// <param name="itemID">The item ID of this asset in the inventory</param>
        /// <param name="taskID">Use UUID.Zero if you are not requesting an 
        /// asset from an object inventory</param>
        /// <param name="ownerID">The owner of this asset</param>
        /// <param name="type">Asset type</param>
        /// <param name="priority">Whether to prioritize this asset download or not</param>
        /// <param name="callback"></param>
        public void RequestInventoryAsset(UUID assetID, UUID itemID, UUID taskID, UUID ownerID, AssetType type, bool priority, AssetReceivedCallback callback)
        {
            AssetDownload transfer = new AssetDownload();
            transfer.ID = UUID.Random();
            transfer.AssetID = assetID;
            //transfer.AssetType = type; // Set in TransferInfoHandler.
            transfer.Priority = 100.0f + (priority ? 1.0f : 0.0f);
            transfer.Channel = ChannelType.Asset;
            transfer.Source = SourceType.SimInventoryItem;
            transfer.Simulator = Frame.Network.CurrentSim;
            transfer.Callback = callback;

            // Check asset cache first
            if (callback != null && Cache.HasAsset(assetID))
            {
                byte[] data = Cache.GetCachedAssetBytes(assetID);
                transfer.AssetData = data;
                transfer.Success = true;
                transfer.Status = StatusCode.OK;

                Asset asset = CreateAssetWrapper(type);
                asset.AssetData = data;
                asset.AssetID = assetID;

                try { callback(transfer, asset); }
                catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, e); }

                return;
            }

            // Add this transfer to the dictionary
            lock (Transfers) Transfers[transfer.ID] = transfer;

            // Build the request packet and send it
            TransferRequestPacket request = new TransferRequestPacket();
            request.TransferInfo.ChannelType = (int)transfer.Channel;
            request.TransferInfo.Priority = transfer.Priority;
            request.TransferInfo.SourceType = (int)transfer.Source;
            request.TransferInfo.TransferID = transfer.ID;

            byte[] paramField = new byte[100];
            Buffer.BlockCopy(Frame.Agent.AgentID.GetBytes(), 0, paramField, 0, 16);
            Buffer.BlockCopy(Frame.Agent.SessionID.GetBytes(), 0, paramField, 16, 16);
            Buffer.BlockCopy(ownerID.GetBytes(), 0, paramField, 32, 16);
            Buffer.BlockCopy(taskID.GetBytes(), 0, paramField, 48, 16);
            Buffer.BlockCopy(itemID.GetBytes(), 0, paramField, 64, 16);
            Buffer.BlockCopy(assetID.GetBytes(), 0, paramField, 80, 16);
            Buffer.BlockCopy(Utils.IntToBytes((int)type), 0, paramField, 96, 4);
            request.TransferInfo.Params = paramField;

            Frame.Network.CurrentSim.Inject(request, Direction.Outgoing);
        }

        public void RequestInventoryAsset(InventoryItem item, bool priority, AssetReceivedCallback callback)
        {
            RequestInventoryAsset(item.AssetUUID, item.UUID, UUID.Zero, item.OwnerID, item.AssetType, priority, callback);
        }

        public void RequestEstateAsset()
        {
            throw new Exception("This function is not implemented yet!");
        }

        /// <summary>
        /// Used to force asset data into the PendingUpload property, ie: for raw terrain uploads
        /// </summary>
        /// <param name="assetData">An AssetUpload object containing the data to upload to the simulator</param>
        internal void SetPendingAssetUploadData(AssetUpload assetData)
        {
            lock (PendingUploadLock)
                PendingUpload = assetData;
        }

        /// <summary>
        /// Request an asset be uploaded to the simulator
        /// </summary>
        /// <param name="asset">The <seealso cref="Asset"/> Object containing the asset data</param>
        /// <param name="storeLocal">If True, the asset once uploaded will be stored on the simulator
        /// in which the client was connected in addition to being stored on the asset server</param>
        /// <returns>The <seealso cref="UUID"/> of the transfer, can be used to correlate the upload with
        /// events being fired</returns>
        public UUID RequestUpload(Asset asset, bool storeLocal)
        {
            if (asset.AssetData == null)
                throw new ArgumentException("Can't upload an asset with no data (did you forget to call Encode?)");

            UUID assetID;
            UUID transferID = RequestUpload(out assetID, asset.AssetType, asset.AssetData, storeLocal);
            asset.AssetID = assetID;
            return transferID;
        }

        /// <summary>
        /// Request an asset be uploaded to the simulator
        /// </summary>
        /// <param name="type">The <seealso cref="AssetType"/> of the asset being uploaded</param>
        /// <param name="data">A byte array containing the encoded asset data</param>
        /// <param name="storeLocal">If True, the asset once uploaded will be stored on the simulator
        /// in which the client was connected in addition to being stored on the asset server</param>
        /// <returns>The <seealso cref="UUID"/> of the transfer, can be used to correlate the upload with
        /// events being fired</returns>
        public UUID RequestUpload(AssetType type, byte[] data, bool storeLocal)
        {
            UUID assetID;
            return RequestUpload(out assetID, type, data, storeLocal);
        }

        /// <summary>
        /// Request an asset be uploaded to the simulator
        /// </summary>
        /// <param name="assetID"></param>
        /// <param name="type">Asset type to upload this data as</param>
        /// <param name="data">A byte array containing the encoded asset data</param>
        /// <param name="storeLocal">If True, the asset once uploaded will be stored on the simulator
        /// in which the client was connected in addition to being stored on the asset server</param>
        /// <returns>The <seealso cref="UUID"/> of the transfer, can be used to correlate the upload with
        /// events being fired</returns>
        public UUID RequestUpload(out UUID assetID, AssetType type, byte[] data, bool storeLocal)
        {
            return RequestUpload(out assetID, type, data, storeLocal, UUID.Random());
        }

        /// <summary>
        /// Initiate an asset upload
        /// </summary>
        /// <param name="assetID">The ID this asset will have if the
        /// upload succeeds</param>
        /// <param name="type">Asset type to upload this data as</param>
        /// <param name="data">Raw asset data to upload</param>
        /// <param name="storeLocal">Whether to store this asset on the local
        /// simulator or the grid-wide asset server</param>
        /// <param name="transactionID">The tranaction id for the upload <see cref="RequestCreateItem"/></param>
        /// <returns>The transaction ID of this transfer</returns>
        public UUID RequestUpload(out UUID assetID, AssetType type, byte[] data, bool storeLocal, UUID transactionID)
        {
            AssetUpload upload = new AssetUpload();
            upload.AssetData = data;
            upload.AssetType = type;
            assetID = UUID.Combine(transactionID, Frame.Agent.SecureSessionID);
            upload.AssetID = assetID;
            upload.Size = data.Length;
            upload.XferID = 0;
            upload.ID = transactionID;

            // Build and send the upload packet
            AssetUploadRequestPacket request = new AssetUploadRequestPacket();
            request.AssetBlock.StoreLocal = storeLocal;
            request.AssetBlock.Tempfile = false; // This field is deprecated
            request.AssetBlock.TransactionID = transactionID;
            request.AssetBlock.Type = (sbyte)type;

            bool isMultiPacketUpload;
            if (data.Length + 100 < Settings.MAX_PACKET_SIZE)
            {
                isMultiPacketUpload = false;
                Logger.Log(
                    String.Format("Beginning asset upload [Single Packet], ID: {0}, AssetID: {1}, Size: {2}",
                    upload.ID.ToString(), upload.AssetID.ToString(), upload.Size), Helpers.LogLevel.Info);

                Transfers[upload.ID] = upload;

                // The whole asset will fit in this packet, makes things easy
                request.AssetBlock.AssetData = data;
                upload.Transferred = data.Length;
            }
            else
            {
                isMultiPacketUpload = true;
                Logger.Log(
                    String.Format("Beginning asset upload [Multiple Packets], ID: {0}, AssetID: {1}, Size: {2}",
                    upload.ID.ToString(), upload.AssetID.ToString(), upload.Size), Helpers.LogLevel.Info);

                // Asset is too big, send in multiple packets
                request.AssetBlock.AssetData = Utils.EmptyBytes;
            }

            // Wait for the previous upload to receive a RequestXferPacket
            lock (PendingUploadLock)
            {
                const int UPLOAD_CONFIRM_TIMEOUT = 20 * 1000;
                const int SLEEP_INTERVAL = 50;
                int t = 0;
                while (WaitingForUploadConfirm && t < UPLOAD_CONFIRM_TIMEOUT)
                {
                    System.Threading.Thread.Sleep(SLEEP_INTERVAL);
                    t += SLEEP_INTERVAL;
                }

                if (t < UPLOAD_CONFIRM_TIMEOUT)
                {
                    if (isMultiPacketUpload)
                    {
                        WaitingForUploadConfirm = true;
                    }
                    PendingUpload = upload;
                    Frame.Network.CurrentSim.Inject(request, Direction.Outgoing);

                    return upload.ID;
                }
                else
                {
                    throw new Exception("Timeout waiting for previous asset upload to begin");
                }
            }
        }

        public void RequestUploadBakedTexture(byte[] textureData, BakedTextureUploadedCallback callback)
        {
            Uri url = null;

            string cap_url;
            if(Frame.Network.CurrentSim.Caps.TryGetValue("UploadBakedTexture", out cap_url))
            {
                url = new Uri(cap_url);
            }

            if (url != null)
            {
                // Fetch the uploader capability
                CapsClient request = new CapsClient(url);
                request.OnComplete +=
                    delegate (CapsClient client, OSD result, Exception error)
                    {
                        if (error == null && result is OSDMap)
                        {
                            UploadBakedTextureMessage message = new UploadBakedTextureMessage();
                            message.Deserialize((OSDMap)result);

                            if (message.Request.State == "upload")
                            {
                                Uri uploadUrl = ((UploaderRequestUpload)message.Request).Url;

                                if (uploadUrl != null)
                                {
                                    // POST the asset data
                                    CapsClient upload = new CapsClient(uploadUrl);
                                    upload.OnComplete +=
                                        delegate (CapsClient client2, OSD result2, Exception error2)
                                        {
                                            if (error2 == null && result2 is OSDMap)
                                            {
                                                UploadBakedTextureMessage message2 = new UploadBakedTextureMessage();
                                                message2.Deserialize((OSDMap)result2);

                                                if (message2.Request.State == "complete")
                                                {
                                                    callback(((UploaderRequestComplete)message2.Request).AssetID);
                                                    return;
                                                }
                                            }

                                            Logger.Log("Bake upload failed during asset upload", Helpers.LogLevel.Warning);
                                            callback(UUID.Zero);
                                        };
                                    upload.BeginGetResponse(textureData, "application/octet-stream", Frame.Config.CAPS_TIMEOUT);
                                    return;
                                }
                            }
                        }

                        Logger.Log("Bake upload failed during uploader retrieval", Helpers.LogLevel.Warning);
                        callback(UUID.Zero);
                    };
                request.BeginGetResponse(new OSDMap(), OSDFormat.Xml, Frame.Config.CAPS_TIMEOUT);
            }
            else
            {
                Logger.Log("UploadBakedTexture not available, falling back to UDP method", Helpers.LogLevel.Info);

                WorkPool.QueueUserWorkItem(
                    delegate (object o)
                    {
                        UUID transactionID = UUID.Random();
                        BakedTextureUploadedCallback uploadCallback = (BakedTextureUploadedCallback)o;
                        AutoResetEvent uploadEvent = new AutoResetEvent(false);
                        EventHandler<AssetUploadEventArgs> udpCallback =
                            delegate (object sender, AssetUploadEventArgs e)
                            {
                                if (e.Upload.ID == transactionID)
                                {
                                    uploadEvent.Set();
                                    uploadCallback(e.Upload.Success ? e.Upload.AssetID : UUID.Zero);
                                }
                            };

                        AssetUploaded += udpCallback;

                        UUID assetID;
                        bool success;

                        try
                        {
                            RequestUpload(out assetID, AssetType.Texture, textureData, true, transactionID);
                            success = uploadEvent.WaitOne(Frame.Config.TRANSFER_TIMEOUT, false);
                        }
                        catch (Exception)
                        {
                            success = false;
                        }

                        AssetUploaded -= udpCallback;

                        if (!success)
                            uploadCallback(UUID.Zero);
                    }, callback
                );
            }
        }



        


        #region Texture Downloads

        /// <summary>
        /// Request a texture asset from the simulator using the <see cref="TexturePipeline"/> system to 
        /// manage the requests and re-assemble the image from the packets received from the simulator
        /// </summary>
        /// <param name="textureID">The <see cref="UUID"/> of the texture asset to download</param>
        /// <param name="imageType">The <see cref="ImageType"/> of the texture asset. 
        /// Use <see cref="ImageType.Normal"/> for most textures, or <see cref="ImageType.Baked"/> for baked layer texture assets</param>
        /// <param name="priority">A float indicating the requested priority for the transfer. Higher priority values tell the simulator
        /// to prioritize the request before lower valued requests. An image already being transferred using the <see cref="TexturePipeline"/> can have
        /// its priority changed by resending the request with the new priority value</param>
        /// <param name="discardLevel">Number of quality layers to discard.
        /// This controls the end marker of the data sent. Sending with value -1 combined with priority of 0 cancels an in-progress
        /// transfer.</param>
        /// <remarks>A bug exists in the Linden Simulator where a -1 will occasionally be sent with a non-zero priority
        /// indicating an off-by-one error.</remarks>
        /// <param name="packetStart">The packet number to begin the request at. A value of 0 begins the request
        /// from the start of the asset texture</param>
        /// <param name="callback">The <see cref="TextureDownloadCallback"/> callback to fire when the image is retrieved. The callback
        /// will contain the result of the request and the texture asset data</param>
        /// <param name="progress">If true, the callback will be fired for each chunk of the downloaded image. 
        /// The callback asset parameter will contain all previously received chunks of the texture asset starting 
        /// from the beginning of the request</param>
        /// <example>
        /// Request an image and fire a callback when the request is complete
        /// <code>
        /// Client.Assets.RequestImage(UUID.Parse("c307629f-e3a1-4487-5e88-0d96ac9d4965"), ImageType.Normal, TextureDownloader_OnDownloadFinished);
        /// 
        /// private void TextureDownloader_OnDownloadFinished(TextureRequestState state, AssetTexture asset)
        /// {
        ///     if(state == TextureRequestState.Finished)
        ///     {
        ///       Console.WriteLine("Texture {0} ({1} bytes) has been successfully downloaded", 
        ///         asset.AssetID,
        ///         asset.AssetData.Length); 
        ///     }
        /// }
        /// </code>
        /// Request an image and use an inline anonymous method to handle the downloaded texture data
        /// <code>
        /// Client.Assets.RequestImage(UUID.Parse("c307629f-e3a1-4487-5e88-0d96ac9d4965"), ImageType.Normal, delegate(TextureRequestState state, AssetTexture asset) 
        ///                                         {
        ///                                             if(state == TextureRequestState.Finished)
        ///                                             {
        ///                                                 Console.WriteLine("Texture {0} ({1} bytes) has been successfully downloaded", 
        ///                                                 asset.AssetID,
        ///                                                 asset.AssetData.Length); 
        ///                                             }
        ///                                         }
        /// );
        /// </code>
        /// Request a texture, decode the texture to a bitmap image and apply it to a imagebox 
        /// <code>
        /// Client.Assets.RequestImage(UUID.Parse("c307629f-e3a1-4487-5e88-0d96ac9d4965"), ImageType.Normal, TextureDownloader_OnDownloadFinished);
        /// 
        /// private void TextureDownloader_OnDownloadFinished(TextureRequestState state, AssetTexture asset)
        /// {
        ///     if(state == TextureRequestState.Finished)
        ///     {
        ///         ManagedImage imgData;
        ///         Image bitmap;
        ///
        ///         if (state == TextureRequestState.Finished)
        ///         {
        ///             OpenJPEG.DecodeToImage(assetTexture.AssetData, out imgData, out bitmap);
        ///             picInsignia.Image = bitmap;
        ///         }               
        ///     }
        /// }
        /// </code>
        /// </example>
        public void RequestImage(UUID textureID, ImageType imageType, float priority, int discardLevel,
            uint packetStart, TextureDownloadCallback callback, bool progress)
        {
            string cap_url = string.Empty;
            if (Frame.Config.USE_HTTP_ASSETS)
            {
                if (!Frame.Network.CurrentSim.Caps.TryGetValue("ViewerAsset", out cap_url))
                {
                    if (!Frame.Network.CurrentSim.Caps.TryGetValue("GetTexture", out cap_url))
                    {
                        Logger.Log("No cap for downloading assets", Helpers.LogLevel.Warning);
                    }
                }
            }


            if (cap_url != string.Empty)
            {
                HttpRequestTexture(textureID, imageType, priority, discardLevel, packetStart, callback, progress);
            }
            else
            {
                callback(TextureRequestState.Aborted, null);
                //Texture.RequestTexture(textureID, imageType, priority, discardLevel, packetStart, callback, progress);
            }
        }

        /// <summary>
        /// Overload: Request a texture asset from the simulator using the <see cref="TexturePipeline"/> system to 
        /// manage the requests and re-assemble the image from the packets received from the simulator
        /// </summary>
        /// <param name="textureID">The <see cref="UUID"/> of the texture asset to download</param>
        /// <param name="callback">The <see cref="TextureDownloadCallback"/> callback to fire when the image is retrieved. The callback
        /// will contain the result of the request and the texture asset data</param>
        public void RequestImage(UUID textureID, TextureDownloadCallback callback)
        {
            RequestImage(textureID, ImageType.Normal, 101300.0f, 0, 0, callback, false);
        }

        /// <summary>
        /// Overload: Request a texture asset from the simulator using the <see cref="TexturePipeline"/> system to 
        /// manage the requests and re-assemble the image from the packets received from the simulator
        /// </summary>
        /// <param name="textureID">The <see cref="UUID"/> of the texture asset to download</param>
        /// <param name="imageType">The <see cref="ImageType"/> of the texture asset. 
        /// Use <see cref="ImageType.Normal"/> for most textures, or <see cref="ImageType.Baked"/> for baked layer texture assets</param>
        /// <param name="callback">The <see cref="TextureDownloadCallback"/> callback to fire when the image is retrieved. The callback
        /// will contain the result of the request and the texture asset data</param>
        public void RequestImage(UUID textureID, ImageType imageType, TextureDownloadCallback callback)
        {
            RequestImage(textureID, imageType, 101300.0f, 0, 0, callback, false);
        }

        /// <summary>
        /// Overload: Request a texture asset from the simulator using the <see cref="TexturePipeline"/> system to 
        /// manage the requests and re-assemble the image from the packets received from the simulator
        /// </summary>
        /// <param name="textureID">The <see cref="UUID"/> of the texture asset to download</param>
        /// <param name="imageType">The <see cref="ImageType"/> of the texture asset. 
        /// Use <see cref="ImageType.Normal"/> for most textures, or <see cref="ImageType.Baked"/> for baked layer texture assets</param>
        /// <param name="callback">The <see cref="TextureDownloadCallback"/> callback to fire when the image is retrieved. The callback
        /// will contain the result of the request and the texture asset data</param>
        /// <param name="progress">If true, the callback will be fired for each chunk of the downloaded image. 
        /// The callback asset parameter will contain all previously received chunks of the texture asset starting 
        /// from the beginning of the request</param>
        public void RequestImage(UUID textureID, ImageType imageType, TextureDownloadCallback callback, bool progress)
        {
            RequestImage(textureID, imageType, 101300.0f, 0, 0, callback, progress);
        }

        /// <summary>
        /// Cancel a texture request
        /// </summary>
        /// <param name="textureID">The texture assets <see cref="UUID"/></param>
        public void RequestImageCancel(UUID textureID)
        {
            //Texture.AbortTextureRequest(textureID);
        }

        /// <summary>
        /// Fetach avatar texture on a grid capable of server side baking
        /// </summary>
        /// <param name="avatarID">ID of the avatar</param>
        /// <param name="textureID">ID of the texture</param>
        /// <param name="bakeName">Name of the part of the avatar texture applies to</param>
        /// <param name="callback">Callback invoked on operation completion</param>
        //public void RequestServerBakedImage(UUID avatarID, UUID textureID, string bakeName, TextureDownloadCallback callback)
        //{
        //    if (avatarID == UUID.Zero || textureID == UUID.Zero || callback == null)
        //        return;

        //    if (string.IsNullOrEmpty(Frame.Network.AgentAppearanceServiceURL))
        //    {
        //        callback(TextureRequestState.NotFound, null);
        //        return;
        //    }

        //    byte[] assetData;
        //    // Do we have this image in the cache?
        //    if (Frame.Assets.Cache.HasAsset(textureID)
        //        && (assetData = Frame.Assets.Cache.GetCachedAssetBytes(textureID)) != null)
        //    {
        //        ImageDownload image = new ImageDownload();
        //        image.ID = textureID;
        //        image.AssetData = assetData;
        //        image.Size = image.AssetData.Length;
        //        image.Transferred = image.AssetData.Length;
        //        image.ImageType = ImageType.ServerBaked;
        //        image.AssetType = AssetType.Texture;
        //        image.Success = true;

        //        callback(TextureRequestState.Finished, new AssetTexture(image.ID, image.AssetData));
        //        FireImageProgressEvent(image.ID, image.Transferred, image.Size);
        //        return;
        //    }

        //    CapsBase.DownloadProgressEventHandler progressHandler = null;

        //    Uri url = new Uri(string.Format("{0}texture/{1}/{2}/{3}", Frame.Network.AgentAppearanceServiceURL, avatarID, bakeName, textureID));

        //    DownloadRequest req = new DownloadRequest(
        //        url,
        //        Frame.Config.CAPS_TIMEOUT,
        //        "image/x-j2c",
        //        progressHandler,
        //        (HttpWebRequest request, HttpWebResponse response, byte[] responseData, Exception error) =>
        //        {
        //            if (error == null && responseData != null) // success
        //            {
        //                ImageDownload image = new ImageDownload();
        //                image.ID = textureID;
        //                image.AssetData = responseData;
        //                image.Size = image.AssetData.Length;
        //                image.Transferred = image.AssetData.Length;
        //                image.ImageType = ImageType.ServerBaked;
        //                image.AssetType = AssetType.Texture;
        //                image.Success = true;

        //                callback(TextureRequestState.Finished, new AssetTexture(image.ID, image.AssetData));

        //                Frame.Assets.Cache.SaveAssetToCache(textureID, responseData);
        //            }
        //            else // download failed
        //            {
        //                Logger.Log(
        //                    string.Format("Failed to fetch server bake {0}: {1}",
        //                        textureID,
        //                        (error == null) ? "" : error.Message
        //                    ),
        //                    Helpers.LogLevel.Warning);

        //                callback(TextureRequestState.Timeout, null);
        //            }
        //        }
        //    );

        //    HttpDownloads.QueueDownload(req);

        //}

        /// <summary>
        /// Lets TexturePipeline class fire the progress event
        /// </summary>
        /// <param name="texureID">The texture ID currently being downloaded</param>
        /// <param name="transferredBytes">the number of bytes transferred</param>
        /// <param name="totalBytes">the total number of bytes expected</param>
        internal void FireImageProgressEvent(UUID texureID, int transferredBytes, int totalBytes)
        {
            try { OnImageReceiveProgress(new ImageReceiveProgressEventArgs(texureID, transferredBytes, totalBytes)); }
            catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, e); }
        }

        // Helper method for downloading textures via GetTexture cap
        // Same signature as the UDP variant since we need all the params to
        // pass to the UDP TexturePipeline in case we need to fall back to it
        // (Linden servers currently (1.42) don't support bakes downloads via HTTP)
        private void HttpRequestTexture(UUID textureID, ImageType imageType, float priority, int discardLevel,
            uint packetStart, TextureDownloadCallback callback, bool progress)
        {
            if (textureID == UUID.Zero || callback == null)
                return;

            byte[] assetData;
            // Do we have this image in the cache?
            if (Frame.Assets.Cache.HasAsset(textureID)
                && (assetData = Frame.Assets.Cache.GetCachedAssetBytes(textureID)) != null)
            {
                ImageDownload image = new ImageDownload();
                image.ID = textureID;
                image.AssetData = assetData;
                image.Size = image.AssetData.Length;
                image.Transferred = image.AssetData.Length;
                image.ImageType = imageType;
                image.AssetType = AssetType.Texture;
                image.Success = true;

                callback(TextureRequestState.Finished, new AssetTexture(image.ID, image.AssetData));
                FireImageProgressEvent(image.ID, image.Transferred, image.Size);
                return;
            }

            CapsBase.DownloadProgressEventHandler progressHandler = null;

            if (progress)
            {
                progressHandler = (HttpWebRequest request, HttpWebResponse response, int bytesReceived, int totalBytesToReceive) =>
                {
                    FireImageProgressEvent(textureID, bytesReceived, totalBytesToReceive);
                };
            }

            string cap_url;
            if (!Frame.Network.CurrentSim.Caps.TryGetValue("GetTexture", out cap_url))
            {
                callback(TextureRequestState.Aborted, null);
                return;
            }
            Uri url = new Uri(cap_url);

            DownloadRequest req = new DownloadRequest(
                new Uri(string.Format("{0}/?texture_id={1}", url.ToString(), textureID.ToString())),
                Frame.Config.CAPS_TIMEOUT,
                "image/x-j2c",
                progressHandler,
                (HttpWebRequest request, HttpWebResponse response, byte[] responseData, Exception error) =>
                {
                    if (error == null && responseData != null) // success
                    {
                        ImageDownload image = new ImageDownload();
                        image.ID = textureID;
                        image.AssetData = responseData;
                        image.Size = image.AssetData.Length;
                        image.Transferred = image.AssetData.Length;
                        image.ImageType = imageType;
                        image.AssetType = AssetType.Texture;
                        image.Success = true;

                        callback(TextureRequestState.Finished, new AssetTexture(image.ID, image.AssetData));
                        FireImageProgressEvent(image.ID, image.Transferred, image.Size);

                        Frame.Assets.Cache.SaveAssetToCache(textureID, responseData);
                    }
                    else // download failed
                    {
                        Logger.Log(
                            string.Format("Failed to fetch texture {0} over HTTP", //, falling back to UDP: {1}",
                                textureID,
                                (error == null) ? "" : error.Message
                            ),
                            Helpers.LogLevel.Warning);


                        callback(TextureRequestState.Aborted, null);
                        return;

                        //Texture.RequestTexture(textureID, imageType, priority, discardLevel, packetStart, callback, progress);
                    }
                }
            );

            Frame.HTTP.Downloads.QueueDownload(req);
        }

        #endregion Texture Downloads

        #region Helpers

        public Asset CreateAssetWrapper(AssetType type)
        {
            Asset asset;

            switch (type)
            {
                case AssetType.Notecard:
                    asset = new AssetNotecard();
                    break;
                case AssetType.LSLText:
                    asset = new AssetScriptText();
                    break;
                case AssetType.LSLBytecode:
                    asset = new AssetScriptBinary();
                    break;
                case AssetType.Texture:
                    asset = new AssetTexture();
                    break;
                case AssetType.Object:
                    asset = new AssetPrim();
                    break;
                case AssetType.Clothing:
                    asset = new AssetClothing();
                    break;
                case AssetType.Bodypart:
                    asset = new AssetBodypart();
                    break;
                case AssetType.Animation:
                    asset = new AssetAnimation();
                    break;
                case AssetType.Sound:
                    asset = new AssetSound();
                    break;
                case AssetType.Landmark:
                    asset = new AssetLandmark();
                    break;
                case AssetType.Gesture:
                    asset = new AssetGesture();
                    break;
                case AssetType.CallingCard:
                    asset = new AssetCallingCard();
                    break;
                default:
                    asset = new AssetMutable(type);
                    Logger.Log("Unimplemented asset type: " + type, Helpers.LogLevel.Error);
                    break;
            }

            return asset;
        }

        private Asset WrapAsset(AssetDownload download)
        {
            Asset asset = CreateAssetWrapper(download.AssetType);
            if (asset != null)
            {
                asset.AssetID = download.AssetID;
                asset.AssetData = download.AssetData;
                return asset;
            }
            else
            {
                return null;
            }
        }

        private void SendNextUploadPacket(AssetUpload upload)
        {
            SendXferPacketPacket send = new SendXferPacketPacket();

            send.XferID.ID = upload.XferID;
            send.XferID.Packet = upload.PacketNum++;

            if (send.XferID.Packet == 0)
            {
                // The first packet reserves the first four bytes of the data for the
                // total length of the asset and appends 1000 bytes of data after that
                send.DataPacket.Data = new byte[1004];
                Buffer.BlockCopy(Utils.IntToBytes(upload.Size), 0, send.DataPacket.Data, 0, 4);
                Buffer.BlockCopy(upload.AssetData, 0, send.DataPacket.Data, 4, 1000);
                upload.Transferred += 1000;

                lock (Transfers)
                {
                    Transfers.Remove(upload.AssetID);
                    Transfers[upload.ID] = upload;
                }
            }
            else if ((send.XferID.Packet + 1) * 1000 < upload.Size)
            {
                // This packet is somewhere in the middle of the transfer, or a perfectly
                // aligned packet at the end of the transfer
                send.DataPacket.Data = new byte[1000];
                Buffer.BlockCopy(upload.AssetData, upload.Transferred, send.DataPacket.Data, 0, 1000);
                upload.Transferred += 1000;
            }
            else
            {
                // Special handler for the last packet which will be less than 1000 bytes
                int lastlen = upload.Size - ((int)send.XferID.Packet * 1000);
                send.DataPacket.Data = new byte[lastlen];
                Buffer.BlockCopy(upload.AssetData, (int)send.XferID.Packet * 1000, send.DataPacket.Data, 0, lastlen);
                send.XferID.Packet |= (uint)0x80000000; // This signals the final packet
                upload.Transferred += lastlen;
            }

            Frame.Network.InjectPacket(send, Direction.Outgoing);
        }

        private void SendConfirmXferPacket(ulong xferID, uint packetNum)
        {
            ConfirmXferPacketPacket confirm = new ConfirmXferPacketPacket();
            confirm.XferID.ID = xferID;
            confirm.XferID.Packet = packetNum;

            Frame.Network.InjectPacket(confirm, Direction.Outgoing);
        }

        #endregion Helpers

        #region Transfer Callbacks

        /// <summary>Process an incoming packet and raise the appropriate events</summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">The EventArgs object containing the packet data</param>
        protected Packet TransferInfoHandler(Packet packet, RegionProxy region)
        {
            TransferInfoPacket info = (TransferInfoPacket)packet;
            Transfer transfer;
            AssetDownload download;

            if (Transfers.TryGetValue(info.TransferInfo.TransferID, out transfer))
            {
                download = (AssetDownload)transfer;

                if (download.Callback == null) return null;

                download.Channel = (ChannelType)info.TransferInfo.ChannelType;
                download.Status = (StatusCode)info.TransferInfo.Status;
                download.Target = (TargetType)info.TransferInfo.TargetType;
                download.Size = info.TransferInfo.Size;

                // TODO: Once we support mid-transfer status checking and aborting this
                // will need to become smarter
                if (download.Status != StatusCode.OK)
                {
                    Logger.Log("Transfer failed with status code " + download.Status, Helpers.LogLevel.Warning);

                    lock (Transfers) Transfers.Remove(download.ID);

                    // No data could have been received before the TransferInfo packet
                    download.AssetData = null;


                    DownloadProgress?.Invoke(new AssetDownloadProgressEventArgs(AssetDownloadState.Finished, download.AssetID, download.AssetType, 0, 0));

                    // Fire the event with our transfer that contains Success = false;
                    try { download.Callback(download, null); }
                    catch (Exception ex) { Logger.Log(ex.Message, Helpers.LogLevel.Error, ex); }
                }
                else
                {
                    download.AssetData = new byte[download.Size];

                    if (download.Source == SourceType.Asset && info.TransferInfo.Params.Length == 20)
                    {
                        download.AssetID = new UUID(info.TransferInfo.Params, 0);
                        download.AssetType = (AssetType)(sbyte)info.TransferInfo.Params[16];

                        //Client.DebugLog(String.Format("TransferInfo packet received. AssetID: {0} Type: {1}",
                        //    transfer.AssetID, type));
                    }
                    else if (download.Source == SourceType.SimInventoryItem && info.TransferInfo.Params.Length == 100)
                    {
                        // TODO: Can we use these?
                        //UUID agentID = new UUID(info.TransferInfo.Params, 0);
                        //UUID sessionID = new UUID(info.TransferInfo.Params, 16);
                        //UUID ownerID = new UUID(info.TransferInfo.Params, 32);
                        //UUID taskID = new UUID(info.TransferInfo.Params, 48);
                        //UUID itemID = new UUID(info.TransferInfo.Params, 64);
                        download.AssetID = new UUID(info.TransferInfo.Params, 80);
                        download.AssetType = (AssetType)(sbyte)info.TransferInfo.Params[96];

                        //Client.DebugLog(String.Format("TransferInfo packet received. AgentID: {0} SessionID: {1} " + 
                        //    "OwnerID: {2} TaskID: {3} ItemID: {4} AssetID: {5} Type: {6}", agentID, sessionID, 
                        //    ownerID, taskID, itemID, transfer.AssetID, type));
                    }
                    else
                    {
                        Logger.Log("Received a TransferInfo packet with a SourceType of " + download.Source.ToString() +
                            " and a Params field length of " + info.TransferInfo.Params.Length,
                            Helpers.LogLevel.Warning);
                    }
                }

                download.HeaderReceivedEvent.Set();

                return null;
            }
            else
            {
                return packet;
                //Logger.Log("Received a TransferInfo packet for an asset we didn't request, TransferID: " +
                //    info.TransferInfo.TransferID, Helpers.LogLevel.Warning);
            }
        }

        /// <summary>Process an incoming packet and raise the appropriate events</summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">The EventArgs object containing the packet data</param>
        protected Packet TransferPacketHandler(Packet packet, RegionProxy region)
        {
            TransferPacketPacket asset = (TransferPacketPacket)packet;
            Transfer transfer;

            if (Transfers.TryGetValue(asset.TransferData.TransferID, out transfer))
            {
                AssetDownload download = (AssetDownload)transfer;

                if (download.Size == 0)
                {
                    Logger.DebugLog("TransferPacket received ahead of the transfer header, blocking...");

                    // We haven't received the header yet, block until it's received or times out
                    download.HeaderReceivedEvent.WaitOne(TRANSFER_HEADER_TIMEOUT, false);

                    if (download.Size == 0)
                    {
                        Logger.Log("Timed out while waiting for the asset header to download for " +
                            download.ID.ToString(), Helpers.LogLevel.Warning);

                        // Abort the transfer
                        TransferAbortPacket abort = new TransferAbortPacket();
                        abort.TransferInfo.ChannelType = (int)download.Channel;
                        abort.TransferInfo.TransferID = download.ID;
                        download.Simulator.Inject(abort, Direction.Outgoing);

                        download.Success = false;
                        lock (Transfers) Transfers.Remove(download.ID);


                        DownloadProgress?.Invoke(new AssetDownloadProgressEventArgs(AssetDownloadState.Failed, download.AssetID, download.AssetType, 0, 0));

                        // Fire the event with our transfer that contains Success = false
                        if (download.Callback != null)
                        {
                            try { download.Callback(download, null); }
                            catch (Exception ex) { Logger.Log(ex.Message, Helpers.LogLevel.Error, ex); }
                        }

                        return null;
                    }
                }

                // If packets arrive out of order, we add them to the out of order packet directory
                // until all previous packets have arrived
                try
                {
                    if (download.nextPacket == asset.TransferData.Packet)
                    {
                        byte[] data = asset.TransferData.Data;
                        do
                        {
                            Buffer.BlockCopy(data, 0, download.AssetData, download.Transferred, data.Length);
                            download.Transferred += data.Length;
                            download.nextPacket++;
                        } while (download.outOfOrderPackets.TryGetValue(download.nextPacket, out data));
                    }
                    else
                    {
                        //Logger.Log(string.Format("Fixing out of order packet {0} when expecting {1}!", asset.TransferData.Packet, download.nextPacket), Helpers.LogLevel.Debug);
                        download.outOfOrderPackets.Add(asset.TransferData.Packet, asset.TransferData.Data);
                    }
                }
                catch (ArgumentException)
                {
                    Logger.Log(String.Format("TransferPacket handling failed. TransferData.Data.Length={0}, AssetData.Length={1}, TransferData.Packet={2}",
                        asset.TransferData.Data.Length, download.AssetData.Length, asset.TransferData.Packet), Helpers.LogLevel.Error);
                    return null;
                }

                //Client.DebugLog(String.Format("Transfer packet {0}, received {1}/{2}/{3} bytes for asset {4}",
                //    asset.TransferData.Packet, asset.TransferData.Data.Length, transfer.Transferred, transfer.Size,
                //    transfer.AssetID.ToString()));


                DownloadProgress?.Invoke(new AssetDownloadProgressEventArgs(AssetDownloadState.Downloading, download.AssetID, download.AssetType, transfer.Transferred, transfer.Size));

                // Check if we downloaded the full asset
                if (download.Transferred >= download.Size)
                {
                    Logger.DebugLog("Transfer for asset " + download.AssetID.ToString() + " completed");

                    download.Success = true;
                    lock (Transfers) Transfers.Remove(download.ID);

                    DownloadProgress?.Invoke(new AssetDownloadProgressEventArgs(AssetDownloadState.Finished, download.AssetID, download.AssetType, transfer.Size, transfer.Size));

                    // Cache successful asset download
                    Cache.SaveAssetToCache(download.AssetID, download.AssetData);

                    if (download.Callback != null)
                    {
                        try { download.Callback(download, WrapAsset(download)); }
                        catch (Exception ex) { Logger.Log(ex.Message, Helpers.LogLevel.Error, ex); }
                    }
                }
            }

            return packet;
        }

        #endregion Transfer Callbacks

        #region Xfer Callbacks

        /// <summary>Process an incoming packet and raise the appropriate events</summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">The EventArgs object containing the packet data</param>
        protected Packet InitiateDownloadPacketHandler(Packet packet, RegionProxy region)
        {
            InitiateDownloadPacket request = (InitiateDownloadPacket)packet;
            try
            {
                OnInitiateDownload(new InitiateDownloadEventArgs(Utils.BytesToString(request.FileData.SimFilename),
                    Utils.BytesToString(request.FileData.ViewerFilename)));
            }
            catch (Exception ex) { Logger.Log(ex.Message, Helpers.LogLevel.Error, ex); }

            return packet; /// ????
        }

        /// <summary>Process an incoming packet and raise the appropriate events</summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">The EventArgs object containing the packet data</param>
        protected Packet RequestXferHandler(Packet packet, RegionProxy region)
        {
            if (PendingUpload == null)
                Logger.Log("Received a RequestXferPacket for an unknown asset upload", Helpers.LogLevel.Warning);
            else
            {
                AssetUpload upload = PendingUpload;
                PendingUpload = null;
                WaitingForUploadConfirm = false;
                RequestXferPacket request = (RequestXferPacket)packet;

                upload.XferID = request.XferID.ID;
                upload.Type = (AssetType)request.XferID.VFileType;

                UUID transferID = new UUID(upload.XferID);
                Transfers[transferID] = upload;

                // Send the first packet containing actual asset data
                SendNextUploadPacket(upload);

                return null;
            }

            return packet;
        }

        /// <summary>Process an incoming packet and raise the appropriate events</summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">The EventArgs object containing the packet data</param>
        protected Packet ConfirmXferPacketHandler(Packet packet, RegionProxy region)
        {
            ConfirmXferPacketPacket confirm = (ConfirmXferPacketPacket)packet;

            // Building a new UUID every time an ACK is received for an upload is a horrible
            // thing, but this whole Xfer system is horrible
            UUID transferID = new UUID(confirm.XferID.ID);
            Transfer transfer;
            AssetUpload upload = null;

            if (Transfers.TryGetValue(transferID, out transfer))
            {
                upload = (AssetUpload)transfer;

                //Client.DebugLog(String.Format("ACK for upload {0} of asset type {1} ({2}/{3})",
                //    upload.AssetID.ToString(), upload.Type, upload.Transferred, upload.Size));

                try { OnUploadProgress(new AssetUploadEventArgs(upload)); }
                catch (Exception ex) { Logger.Log(ex.Message, Helpers.LogLevel.Error, ex); }

                if (upload.Transferred < upload.Size)
                    SendNextUploadPacket(upload);

                return null;
            }

            return packet;
        }

        /// <summary>Process an incoming packet and raise the appropriate events</summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">The EventArgs object containing the packet data</param>
        protected Packet AssetUploadCompleteHandler(Packet packet, RegionProxy region)
        {
            AssetUploadCompletePacket complete = (AssetUploadCompletePacket)packet;

            // If we uploaded an asset in a single packet, RequestXferHandler()
            // will never be called so we need to set this here as well
            WaitingForUploadConfirm = false;

            if (m_AssetUploadedEvent != null)
            {
                bool found = false;
                KeyValuePair<UUID, Transfer> foundTransfer = new KeyValuePair<UUID, Transfer>();

                // Xfer system sucks really really bad. Where is the damn XferID?
                lock (Transfers)
                {
                    foreach (KeyValuePair<UUID, Transfer> transfer in Transfers)
                    {
                        if (transfer.Value.GetType() == typeof(AssetUpload))
                        {
                            AssetUpload upload = (AssetUpload)transfer.Value;

                            if ((upload).AssetID == complete.AssetBlock.UUID)
                            {
                                found = true;
                                foundTransfer = transfer;
                                upload.Success = complete.AssetBlock.Success;
                                upload.Type = (AssetType)complete.AssetBlock.Type;
                                break;
                            }
                        }
                    }
                }

                if (found)
                {
                    lock (Transfers) Transfers.Remove(foundTransfer.Key);

                    try { OnAssetUploaded(new AssetUploadEventArgs((AssetUpload)foundTransfer.Value)); }
                    catch (Exception ex) { Logger.Log(ex.Message, Helpers.LogLevel.Error, ex); }
                }
                else
                {
                    Logger.Log(String.Format(
                        "Got an AssetUploadComplete on an unrecognized asset, AssetID: {0}, Type: {1}, Success: {2}",
                        complete.AssetBlock.UUID, (AssetType)complete.AssetBlock.Type, complete.AssetBlock.Success),
                        Helpers.LogLevel.Warning);
                }
            }

            return packet;
        }

        /// <summary>Process an incoming packet and raise the appropriate events</summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">The EventArgs object containing the packet data</param>
        protected Packet SendXferPacketHandler(Packet packet, RegionProxy region)
        {
            SendXferPacketPacket xfer = (SendXferPacketPacket)packet;

            // Lame ulong to UUID conversion, please go away Xfer system
            UUID transferID = new UUID(xfer.XferID.ID);
            Transfer transfer;
            XferDownload download = null;

            if (Transfers.TryGetValue(transferID, out transfer))
            {
                download = (XferDownload)transfer;

                // Apply a mask to get rid of the "end of transfer" bit
                uint packetNum = xfer.XferID.Packet & 0x0FFFFFFF;

                // Check for out of order packets, possibly indicating a resend
                if (packetNum != download.PacketNum)
                {
                    if (packetNum == download.PacketNum - 1)
                    {
                        Logger.DebugLog("Resending Xfer download confirmation for packet " + packetNum);
                        SendConfirmXferPacket(download.XferID, packetNum);
                    }
                    else
                    {
                        Logger.Log("Out of order Xfer packet in a download, got " + packetNum + " expecting " + download.PacketNum,
                            Helpers.LogLevel.Warning);
                        // Re-confirm the last packet we actually received
                        SendConfirmXferPacket(download.XferID, download.PacketNum - 1);
                    }

                    return null; // maybe not null?
                }

                if (packetNum == 0)
                {
                    // This is the first packet received in the download, the first four bytes are a size integer
                    // in little endian ordering
                    byte[] bytes = xfer.DataPacket.Data;
                    download.Size = (bytes[0] + (bytes[1] << 8) + (bytes[2] << 16) + (bytes[3] << 24));
                    download.AssetData = new byte[download.Size];

                    Logger.DebugLog("Received first packet in an Xfer download of size " + download.Size);

                    Buffer.BlockCopy(xfer.DataPacket.Data, 4, download.AssetData, 0, xfer.DataPacket.Data.Length - 4);
                    download.Transferred += xfer.DataPacket.Data.Length - 4;
                }
                else
                {
                    Buffer.BlockCopy(xfer.DataPacket.Data, 0, download.AssetData, download.Transferred /*1000 * (int)packetNum*/, xfer.DataPacket.Data.Length); // opensim sends 1024?
                    download.Transferred += xfer.DataPacket.Data.Length;
                }

                // Increment the packet number to the packet we are expecting next
                download.PacketNum++;

                // Confirm receiving this packet
                SendConfirmXferPacket(download.XferID, packetNum);

                if ((xfer.XferID.Packet & 0x80000000) != 0)
                {
                    // This is the last packet in the transfer
                    if (!String.IsNullOrEmpty(download.Filename))
                        Logger.DebugLog("Xfer download for asset " + download.Filename + " completed");
                    else
                        Logger.DebugLog("Xfer download for asset " + download.VFileID.ToString() + " completed");

                    download.Success = true;
                    lock (Transfers) Transfers.Remove(download.ID);

                    try { OnXferReceived(new XferReceivedEventArgs(download)); }
                    catch (Exception ex) { Logger.Log(ex.Message, Helpers.LogLevel.Error, ex); }
                }

                return null;
            }

            return packet;
        }

        /// <summary>Process an incoming packet and raise the appropriate events</summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">The EventArgs object containing the packet data</param>
        protected Packet AbortXferHandler(Packet packet, RegionProxy region)
        {
            AbortXferPacket abort = (AbortXferPacket)packet;
            XferDownload download = null;

            // Lame ulong to UUID conversion, please go away Xfer system
            UUID transferID = new UUID(abort.XferID.ID);

            lock (Transfers)
            {
                Transfer transfer;
                if (Transfers.TryGetValue(transferID, out transfer))
                {
                    download = (XferDownload)transfer;
                    Transfers.Remove(transferID);
                }
            }

            if (download != null && m_XferReceivedEvent != null)
            {
                download.Success = false;
                download.Error = (TransferError)abort.XferID.Result;

                try { OnXferReceived(new XferReceivedEventArgs(download)); }
                catch (Exception ex) { Logger.Log(ex.Message, Helpers.LogLevel.Error, ex); }

                return null;
            }

            return packet;
        }

        #endregion Xfer Callbacks
    }
    #region EventArg classes
    // <summary>Provides data for XferReceived event</summary>
    public class XferReceivedEventArgs : EventArgs
    {
        private readonly XferDownload m_Xfer;

        /// <summary>Xfer data</summary>
        public XferDownload Xfer { get { return m_Xfer; } }

        public XferReceivedEventArgs(XferDownload xfer)
        {
            this.m_Xfer = xfer;
        }
    }

    // <summary>Provides data for AssetUploaded event</summary>
    public class AssetUploadEventArgs : EventArgs
    {
        private readonly AssetUpload m_Upload;

        /// <summary>Upload data</summary>
        public AssetUpload Upload { get { return m_Upload; } }

        public AssetUploadEventArgs(AssetUpload upload)
        {
            this.m_Upload = upload;
        }
    }

    // <summary>Provides data for InitiateDownloaded event</summary>
    public class InitiateDownloadEventArgs : EventArgs
    {
        private readonly string m_SimFileName;
        private readonly string m_ViewerFileName;

        /// <summary>Filename used on the simulator</summary>
        public string SimFileName { get { return m_SimFileName; } }

        /// <summary>Filename used by the client</summary>
        public string ViewerFileName { get { return m_ViewerFileName; } }

        public InitiateDownloadEventArgs(string simFilename, string viewerFilename)
        {
            this.m_SimFileName = simFilename;
            this.m_ViewerFileName = viewerFilename;
        }
    }

    // <summary>Provides data for ImageReceiveProgress event</summary>
    public class ImageReceiveProgressEventArgs : EventArgs
    {
        private readonly UUID m_ImageID;
        private readonly int m_Received;
        private readonly int m_Total;

        /// <summary>UUID of the image that is in progress</summary>
        public UUID ImageID { get { return m_ImageID; } }

        /// <summary>Number of bytes received so far</summary>
        public int Received { get { return m_Received; } }

        /// <summary>Image size in bytes</summary>
        public int Total { get { return m_Total; } }

        public ImageReceiveProgressEventArgs(UUID imageID, int received, int total)
        {
            this.m_ImageID = imageID;
            this.m_Received = received;
            this.m_Total = total;
        }
    }

    public enum AssetDownloadState
    {
        Queued,
        Started,
        Downloading,
        Finished,
        Failed
    }

    public class AssetDownloadProgressEventArgs : EventArgs
    {
        public UUID AssetID { get; private set; }
        public AssetType Type { get; private set; }
        public int Received { get; private set; }
        public int Total { get; private set; }
        public AssetDownloadState Status { get; private set; }

        public AssetDownloadProgressEventArgs(AssetDownloadState status, UUID assetID, AssetType assetType, int received, int total)
        {
            Status = status;
            AssetID = assetID;
            Type = assetType;
            Received = received;
            Total = total;
        }
    }

    public delegate void AssetDownloadProgress(AssetDownloadProgressEventArgs event_args);

    #endregion
}
