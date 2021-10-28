using GridProxy;
using OpenMetaverse;
using OpenMetaverse.Assets;
using OpenMetaverse.Packets;
using OpenMetaverse.StructuredData;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CoolProxy.Plugins.CopyBot
{
    public class CopyBotPlugin : CoolProxyPlugin
    {
        internal static CopyBotPlugin Instance;

        private SettingsManager Settings;
        private GUIManager GUI;
        private CoolProxyFrame Proxy;

        public bool ImporterBusy { get; private set; } = false;

        Primitive currentPrim;
        Vector3 currentPosition;
        AutoResetEvent primDone = new AutoResetEvent(false);
        List<Primitive> primsCreated;
        List<uint> linkQueue;
        uint rootLocalID;
        ImporterState state = ImporterState.Idle;

        BackgroundWorker importWorker;
        List<Linkset> Linksets;

        private bool CancellingImport = false;


        public CopyBotPlugin(SettingsManager settings, GUIManager gui, CoolProxyFrame frame)
        {
            Settings = settings;
            GUI = gui;
            Proxy = frame;
            Instance = this;

            //GUI.AddToolButton("Objects", "Clear Selection", (x, y) => { Proxy.Agent.ClearSelection(); });
            GUI.AddToolButton("Objects", "Export Selected Objects", exportSelectedObjects);
            GUI.AddToolButton("Objects", "Import Object from File", importXML);

            //GUI.AddSingleMenuItem("-", (a) => { });
            GUI.AddSingleMenuItem("Save As...", exportAvatar);

            Proxy.Objects.ObjectUpdate += Objects_ObjectUpdate;
        }

        private void importXML(object sender, EventArgs e)
        {
            if (!ImporterBusy)
            {
                using (OpenFileDialog dialog = new OpenFileDialog())
                {
                    dialog.Filter = "Object File|*.xml";
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        new ImportForm(Proxy, new ImportOptions(dialog.FileName), this).ShowDialog();
                    }
                }
            }
        }

        void OpenExportForm(Avatar avatar)
        {
            var form = new ExportForm(Proxy, avatar);

            form.TopMost = Settings.getBool("KeepCoolProxyOnTop");
            Settings.getSetting("KeepCoolProxyOnTop").OnChanged += (x, y) => { form.TopMost = (bool)y.Value; };

            form.Show();
        }

        private void exportAvatar(UUID target)
        {
            Avatar avatar = Proxy.Network.CurrentSim.ObjectsAvatars.Values.FirstOrDefault(x => x.ID == target);

            if (avatar == default(Avatar))
            {
                return;
            }

            OpenExportForm(avatar);
        }

        private void exportSelectedObjects(object sender, EventArgs e)
        {
            OpenExportForm(null);
        }

        public void UploadWearble(string name, byte[] data, WearableType wearabletype, AssetType assettype)
        {
            AssetUploadRequestPacket request = new AssetUploadRequestPacket();
            request.AssetBlock.TransactionID = UUID.Random();
            request.AssetBlock.Type = (sbyte)assettype;
            request.AssetBlock.Tempfile = false;
            request.AssetBlock.StoreLocal = false;
            request.AssetBlock.AssetData = data;

            Proxy.Network.InjectPacket(request, Direction.Outgoing);

            CreateInventoryItemPacket item = new CreateInventoryItemPacket();
            item.AgentData.AgentID = Proxy.Agent.AgentID;
            item.AgentData.SessionID = Proxy.Agent.SessionID;

            item.InventoryBlock.CallbackID = 0;
            item.InventoryBlock.FolderID = Proxy.Inventory.FindFolderForType(assettype);
            item.InventoryBlock.TransactionID = request.AssetBlock.TransactionID;
            item.InventoryBlock.NextOwnerMask = ((uint)PermissionMask.Transfer | (uint)PermissionMask.Move);
            item.InventoryBlock.Type = (sbyte)assettype;
            item.InventoryBlock.InvType = (sbyte)InventoryType.Wearable;
            item.InventoryBlock.WearableType = (byte)wearabletype;
            item.InventoryBlock.Name = Utils.StringToBytes(name);
            item.InventoryBlock.Description = new byte[0];

            Proxy.Network.InjectPacket(item, Direction.Outgoing);
        }

        internal void ForgeLinkset(ImportOptions options)
        {
            List<OSAssetPrim> assetPrims = new List<OSAssetPrim>();
            foreach(var linkset in options.Linksets)
            {
                OSAssetPrim asset = new OSAssetPrim(UUID.Random(), new byte[0]);
                asset.Children = new List<OSPrimObject>();
                asset.Parent = OSPrimObject.FromPrimitive(linkset.RootPrim);
                asset.Parent.ID = UUID.Random();

                foreach(var child in linkset.Children)
                {
                    var c = OSPrimObject.FromPrimitive(child);
                    c.ID = UUID.Random();
                    asset.Children.Add(c);
                }

                assetPrims.Add(asset);
            }

            foreach(var asset in assetPrims)
            {
                string xml = asset.EncodeXml();
                byte[] bytes = Encoding.UTF8.GetBytes(xml);

                UUID asset_id = UUID.Random();

                Proxy.OpenSim.Assets.UploadAsset(asset_id, AssetType.Object, asset.Parent.Name, asset.Parent.Description, asset.Parent.CreatorID, bytes, (success, new_id) =>
                {
                    if(success)
                    {
                        UUID folder_id = Proxy.Inventory.SuitcaseID != UUID.Zero ?
                            Proxy.Inventory.FindSuitcaseFolderForType(FolderType.Object) :
                            Proxy.Inventory.FindFolderForType(FolderType.Object);

                        UUID item_id = UUID.Random();

                        Proxy.OpenSim.XInventory.AddItem(
                            folder_id, item_id, asset_id, Proxy.Agent.AgentID,
                            AssetType.Object, InventoryType.Object, 0, asset.Parent.Name, asset.Parent.Description, DateTime.UtcNow, s =>
                            {
                                if (s)
                                {
                                    Proxy.Inventory.RequestFetchInventory(item_id, Proxy.Agent.AgentID, false);
                                }
                                else Proxy.SayToUser("Error adding item to suitcase!");
                            });
                    }
                });
            }
        }

        internal void ImportLinkset(ImportOptions options)
        {
            if (ImporterBusy) return;

            Proxy.SayToUser(string.Format("Importing {0} linksets", options.Linksets.Count));

            Linksets = options.Linksets;
            ImporterBusy = true;
            CancellingImport = false;

            importProgressForm = new ImportProgressForm();
            importProgressForm.Show();

            var wearables = GenerateWearable(options.Wearables);
            foreach (var item in wearables)
            {
                UploadWearble(item.Name, item.AssetData, item.WearableType, AssetType.Bodypart);
            }

            importWorker = new BackgroundWorker();
            importWorker.DoWork += Worker_DoWork;
            importWorker.RunWorkerCompleted += ImportWorker_RunWorkerCompleted;
            importWorker.RunWorkerAsync();
        }

        public void CancelImport()
        {
            CancellingImport = true;
        }

        private void ImportWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            ImporterBusy = false;
            Proxy.AlertMessage(CancellingImport ? "Import cancelled." : "Import complete.", false);
        }


        ImportProgressForm importProgressForm;

        int CountPrims(List<Linkset> linksets)
        {
            int count = linksets.Count;
            foreach (var set in linksets)
                count += set.Children.Count;
            return count;
        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            primsCreated = new List<Primitive>();
            linkQueue = new List<uint>();

            //Proxy.SayToUser("Importing " + Linksets.Count + " structures.");

            int linkset_count = CountPrims(Linksets);
            int count = 0;

            importProgressForm?.ReportProgress(count, linkset_count);

            foreach (Linkset linkset in Linksets)
            {
                if (CancellingImport) return;
                if (linkset.RootPrim.LocalID != 0)
                {
                    state = ImporterState.RezzingParent;
                    currentPrim = linkset.RootPrim;
                    // HACK: Import the structure just above our head
                    // We need a more elaborate solution for importing with relative or absolute offsets
                    linkset.RootPrim.Position = Proxy.Agent.SimPosition;
                    linkset.RootPrim.Position.Z += 3.0f;
                    currentPosition = linkset.RootPrim.Position;

                    // Rez the root prim with no rotation
                    Quaternion rootRotation = linkset.RootPrim.Rotation;
                    linkset.RootPrim.Rotation = Quaternion.Identity;

                    Proxy.Objects.AddPrim(Proxy.Network.CurrentSim, linkset.RootPrim.PrimData, UUID.Zero,
                        linkset.RootPrim.Position, linkset.RootPrim.Scale, linkset.RootPrim.Rotation);

                    if (!primDone.WaitOne(10000, false))
                    {
                        Proxy.SayToUser("Rez failed, timed out while creating the root prim.");
                        return;
                    }

                    Proxy.Objects.SetPosition(Proxy.Network.CurrentSim, primsCreated[primsCreated.Count - 1].LocalID, linkset.RootPrim.Position);

                    importProgressForm?.ReportProgress(++count, linkset_count);

                    state = ImporterState.RezzingChildren;

                    // Rez the child prims
                    foreach (Primitive prim in linkset.Children)
                    {
                        if (CancellingImport) return;

                        currentPrim = prim;
                        currentPosition = prim.Position + linkset.RootPrim.Position;

                        Proxy.Objects.AddPrim(Proxy.Network.CurrentSim, prim.PrimData, UUID.Zero, currentPosition, prim.Scale, prim.Rotation);

                        if (!primDone.WaitOne(10000, false))
                        {
                            Proxy.SayToUser("Rez failed, timed out while creating child prim.");
                            return;
                        }

                        Proxy.Objects.SetPosition(Proxy.Network.CurrentSim, primsCreated[primsCreated.Count - 1].LocalID, currentPosition);
                        importProgressForm?.ReportProgress(++count, linkset_count);
                    }

                    // Create a list of the local IDs of the newly created prims
                    List<uint> primIDs = new List<uint>(primsCreated.Count);
                    primIDs.Add(rootLocalID); // Root prim is first in list.

                    if (linkset.Children.Count != 0)
                    {
                        // Add the rest of the prims to the list of local IDs
                        foreach (Primitive prim in primsCreated)
                        {
                            if (prim.LocalID != rootLocalID)
                                primIDs.Add(prim.LocalID);
                        }
                        linkQueue = new List<uint>(primIDs.Count);
                        linkQueue.AddRange(primIDs);

                        // Link and set the permissions + rotation
                        state = ImporterState.Linking;
                        Proxy.Objects.LinkPrims(Proxy.Network.CurrentSim, linkQueue);

                        if (primDone.WaitOne(1000 * linkset.Children.Count, false))
                            Proxy.Objects.SetRotation(Proxy.Network.CurrentSim, rootLocalID, rootRotation);
                        else
                            Proxy.SayToUser(string.Format("Warning: Failed to link {0} prims", linkQueue.Count));

                    }
                    else
                    {
                        Proxy.Objects.SetRotation(Proxy.Network.CurrentSim, rootLocalID, rootRotation);
                    }

                    // Set permissions on newly created prims
                    Proxy.Objects.SetPermissions(Proxy.Network.CurrentSim, primIDs,
                        PermissionWho.Everyone | PermissionWho.Group | PermissionWho.NextOwner,
                        PermissionMask.All, true);

                    state = ImporterState.Idle;
                }
                else
                {
                    // Skip linksets with a missing root prim
                    Proxy.SayToUser("WARNING: Skipping a linkset with a missing root prim");
                }

                // Reset everything for the next linkset
                primsCreated.Clear();
            }

            importProgressForm?.ReportProgress(count, linkset_count, true);
        }

        private void Objects_ObjectUpdate(object sender, GridProxy.PrimEventArgs e)
        {
            Primitive prim = e.Prim;

            if(state != ImporterState.Linking)
            {
                if ((prim.Flags & PrimFlags.CreateSelected) == 0)
                    return; // We received an update for an object we didn't create
            }

            switch (state)
            {
                case ImporterState.RezzingParent:
                    rootLocalID = prim.LocalID;
                    goto case ImporterState.RezzingChildren;
                case ImporterState.RezzingChildren:
                    if (!primsCreated.Contains(prim))
                    {
                        //Proxy.SayToUser("Setting properties for " + prim.LocalID);
                        // TODO: Is there a way to set all of this at once, and update more ObjectProperties stuff?
                        Proxy.Objects.SetPosition(e.Region, prim.LocalID, currentPosition);

                        if (currentPrim.Sculpt != null && currentPrim.Sculpt.SculptTexture != UUID.Zero)
                        {
                            Proxy.Objects.SetSculpt(e.Region, prim.LocalID, currentPrim.Sculpt);
                        }

                        if (currentPrim.Light != null)
                        {
                            Proxy.Objects.SetLight(e.Region, prim.LocalID, currentPrim.Light);
                        }

                        if (currentPrim.Flexible != null)
                        {
                            Proxy.Objects.SetFlexible(e.Region, prim.LocalID, currentPrim.Flexible);
                        }

                        Proxy.Objects.SetTextures(e.Region, prim.LocalID, currentPrim.Textures);

                        if (currentPrim.Properties != null && !string.IsNullOrEmpty(currentPrim.Properties.Name))
                        {
                            Proxy.Objects.SetName(e.Region, prim.LocalID, currentPrim.Properties.Name);
                        }

                        if (currentPrim.Properties != null && !string.IsNullOrEmpty(currentPrim.Properties.Description))
                        {
                            Proxy.Objects.SetDescription(e.Region, prim.LocalID, currentPrim.Properties.Description);
                        }

                        primsCreated.Add(prim);
                        primDone.Set();
                    }
                    break;
                case ImporterState.Linking:
                    lock (linkQueue)
                    {
                        int index = linkQueue.IndexOf(prim.LocalID);
                        if (index != -1)
                        {
                            linkQueue.RemoveAt(index);
                            if (linkQueue.Count == 0)
                                primDone.Set();
                        }

                    }
                    break;
            }
        }

        public static List<Linkset> PrimListToLinksetList(List<Primitive> prims)
        {
            // Build an organized structure from the imported prims
            Dictionary<uint, Linkset> linksets = new Dictionary<uint, Linkset>();
            for (int i = 0; i < prims.Count; i++)
            {
                Primitive prim = prims[i];

                if (prim.ParentID == 0)
                {
                    if (linksets.ContainsKey(prim.LocalID))
                        linksets[prim.LocalID].RootPrim = prim;
                    else
                        linksets[prim.LocalID] = new Linkset(prim);
                }
                else
                {
                    if (!linksets.ContainsKey(prim.ParentID))
                        linksets[prim.ParentID] = new Linkset();

                    linksets[prim.ParentID].Children.Add(prim);
                }
            }

            return linksets.Values.ToList();
        }

        public static Bitmap WearableTypeToIcon(WearableType type)
        {
            switch (type)
            {
                case WearableType.Skin:
                    return Properties.Resources.Skin;
                case WearableType.Shape:
                    return Properties.Resources.BodyShape;
                case WearableType.Eyes:
                    return Properties.Resources.Inv_Eye;
                case WearableType.Hair:
                    return Properties.Resources.Hair;
                default:
                    return null;
            }
        }

        public List<AssetWearable> GenerateWearable(List<ImportableWearable> importable)
        {
            List<AssetWearable> assetWearables = new List<AssetWearable>();

            foreach (var item in importable)
            {
                AssetWearable wearable = new AssetBodypart();

                wearable.Name = item.Name;
                wearable.Description = string.Empty;
                wearable.Creator = UUID.Zero;
                wearable.ForSale = SaleType.Not;
                wearable.Group = UUID.Zero;
                wearable.GroupOwned = false;
                wearable.LastOwner = UUID.Zero;
                wearable.WearableType = item.Type;

                var perms = new Permissions();
                perms.BaseMask = PermissionMask.All;
                perms.EveryoneMask = PermissionMask.None;
                perms.GroupMask = PermissionMask.None;
                perms.NextOwnerMask = PermissionMask.All;
                perms.OwnerMask = PermissionMask.All;

                wearable.Permissions = perms;

                wearable.Params = new Dictionary<int, float>();

                foreach (var pair in item.VisualParams)
                {
                    wearable.Params.Add(pair.Key, pair.Value);
                }

                // todo: textures...

                wearable.Encode();

                assetWearables.Add(wearable);
            }

            return assetWearables;
        }
    }

    public enum ImporterState
    {
        RezzingParent,
        RezzingChildren,
        Linking,
        Idle
    }

    public class Linkset
    {
        public Primitive RootPrim;
        public List<Primitive> Children = new List<Primitive>();

        public Linkset()
        {
            RootPrim = new Primitive();
        }

        public Linkset(Primitive rootPrim)
        {
            RootPrim = rootPrim;
        }
    }

    public class ImportableWearable
    {
        public string Name;
        public WearableType Type;
        public Dictionary<int, int> VisualParams;
        public List<UUID> Textures;

        public ImportableWearable(string name, WearableType type, Dictionary<int, int> param, List<UUID> textures)
        {
            Name = name;
            Type = type;
            VisualParams = param;
            Textures = textures;
        }
    }

    public class ImportOptions
    {
        public List<ImportableWearable> Wearables { get; private set; } = new List<ImportableWearable>();

        public List<Linkset> Linksets { get; private set; } = new List<Linkset>();

        public bool KeepPositions { get; set; } = false;

        public ImportOptions(string filename)
        {
            try
            {
                string xml = File.ReadAllText(filename);
                OSD osd = OSDParser.DeserializeLLSDXml(xml);
                AddFromOSD(osd);
            }
            catch
            {
            }
        }

        public ImportOptions(OSDMap osd)
        {
            AddFromOSD(osd);
        }

        public ImportOptions()
        {
        }

        private void AddFromOSD(OSD osd)
        {
            List<Primitive> prims = new List<Primitive>();

            OSDMap map = (OSDMap)osd;
            foreach (KeyValuePair<string, OSD> kvp in map)
            {
                OSDMap entry = (OSDMap)kvp.Value;

                if (entry.ContainsKey("type") && entry["type"] == "wearable")
                {
                    WearableType type = (WearableType)entry["flag"].AsInteger();
                    string name = entry["name"].ToString();

                    Dictionary<int, int> paramvalues = new Dictionary<int, int>();

                    OSDMap visual_params = (OSDMap)entry["params"];
                    foreach (KeyValuePair<string, OSD> pair in visual_params)
                    {
                        int param_id = Convert.ToInt32(pair.Key);
                        int value = pair.Value.AsInteger();
                        paramvalues[param_id] = value;
                    }

                    // todo... textures
                    List<UUID> textures = new List<UUID>();

                    Wearables.Add(new ImportableWearable(name, type, paramvalues, textures));
                }
                else
                {
                    Primitive prim = Primitive.FromOSD(kvp.Value);
                    prim.LocalID = UInt32.Parse(kvp.Key);
                    prims.Add(prim);
                }
            }

            // Build an organized structure from the imported prims
            Dictionary<uint, Linkset> linksets = new Dictionary<uint, Linkset>();
            for (int i = 0; i < prims.Count; i++)
            {
                Primitive prim = prims[i];

                if (prim.ParentID == 0)
                {
                    if (linksets.ContainsKey(prim.LocalID))
                        linksets[prim.LocalID].RootPrim = prim;
                    else
                        linksets[prim.LocalID] = new Linkset(prim);
                }
                else
                {
                    if (!linksets.ContainsKey(prim.ParentID))
                        linksets[prim.ParentID] = new Linkset();

                    linksets[prim.ParentID].Children.Add(prim);
                }
            }

            Linksets = linksets.Values.ToList();
        }
    }
}
