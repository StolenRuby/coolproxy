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

        ImportProgressForm importProgressForm;

        public bool ImporterBusy { get; private set; } = false;

        List<Primitive> ImportPrims;

        Dictionary<uint, List<Primitive>> LinkSets = new Dictionary<uint, List<Primitive>>();
        Dictionary<uint, uint> IDToLocalID = new Dictionary<uint, uint>();

        Dictionary<uint, AttachmentInfo> Attachments = new Dictionary<uint, AttachmentInfo>();
        Dictionary<uint, uint> AttachingItems = new Dictionary<uint, uint>();

        int CurrentPrim = 0;

        Vector3 ImportOffset = new Vector3(0, 0, 3);

        Primitive SeedPrim = null;
        InventoryItem InvSeedItem = null;

        public CopyBotPlugin(SettingsManager settings, GUIManager gui, CoolProxyFrame frame)
        {
            Settings = settings;
            GUI = gui;
            Proxy = frame;
            Instance = this;

            GUI.AddToolButton("CopyBot", "Export Selected Objects", exportSelectedObjects);
            GUI.AddToolButton("CopyBot", "Import Object from File", importXML);
            GUI.AddToolButton("CopyBot", "Import with Selected Object", importXMLWithSeed);

            GUI.AddSingleMenuItem("Save As...", exportAvatar);

            GUI.AddInventoryItemOption("Import With...", importWithInv, AssetType.Object);

            Proxy.Objects.ObjectUpdate += Objects_ObjectUpdate;
        }

        private void importWithInv(InventoryItem inventoryItem)
        {
            if (ImporterBusy) return;

            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Filter = "Object File|*.xml";
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    var options = new ImportOptions(dialog.FileName);
                    options.InvItem = inventoryItem;
                    new ImportForm(Proxy, options, this).ShowDialog();
                }
            }
        }

        private void importXMLWithSeed(object sender, EventArgs e)
        {
            if (ImporterBusy) return;

            if(Proxy.Agent.Selection.Length == 1)
            {
                uint local_id = Proxy.Agent.Selection[0];
                Primitive prim = Proxy.Network.CurrentSim.ObjectsPrimitives.Find(x => x.LocalID == local_id);

                using (OpenFileDialog dialog = new OpenFileDialog())
                {
                    dialog.Filter = "Object File|*.xml";
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        var options = new ImportOptions(dialog.FileName);
                        options.SeedPrim = prim;
                        new ImportForm(Proxy, options, this).ShowDialog();
                    }
                }
            }
            else
            {
                Proxy.SayToUser("You need to be selecting exactly one object.");
            }
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

        //////////////////////////////////////////////////////////////////////////////

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

            ImporterBusy = true;

            ImportOffset = Settings.getVector("ImportOffset");

            IDToLocalID.Clear();
            LinkSets.Clear();
            AttachingItems.Clear();
            Attachments.Clear();

            SeedPrim = options.SeedPrim;
            InvSeedItem = options.InvItem;

            importProgressForm = new ImportProgressForm();
            importProgressForm.Show();

            var wearables = GenerateWearable(options.Wearables);
            foreach (var item in wearables)
            {
                UploadWearble(item.Name, item.AssetData, item.WearableType, AssetType.Bodypart);
            }

            Vector3 center_pos = FindCenterPos(options.Linksets);
            Vector3 import_pos = Proxy.Agent.SimPosition + ImportOffset;

            ImportPrims = new List<Primitive>();
            foreach(var linkset in options.Linksets)
            {
                linkset.RootPrim.ParentID = 0;

                if (linkset.RootPrim.PrimData.State != 0)
                {
                    Attachments[linkset.RootPrim.LocalID] = new AttachmentInfo(linkset.RootPrim.PrimData.AttachmentPoint, linkset.RootPrim.Position, linkset.RootPrim.Rotation);
                    linkset.RootPrim.PrimData.State = 0;
                    linkset.RootPrim.Position = import_pos;
                    linkset.RootPrim.Rotation = Quaternion.Identity;
                }
                else
                {
                    linkset.RootPrim.Position -= center_pos;
                    linkset.RootPrim.Position += import_pos;
                }

                ImportPrims.Add(linkset.RootPrim);
                foreach (var child in linkset.Children)
                {
                    child.Rotation = linkset.RootPrim.Rotation * child.Rotation;
                    child.Position *= linkset.RootPrim.Rotation;
                    child.Position += linkset.RootPrim.Position;
                    ImportPrims.Add(child);
                }
            }

            CurrentPrim = 0;

            RezSupply();
        }

        Vector3 FindCenterPos(List<Linkset> linksets)
        {
            if (linksets.Count == 0)
                return Vector3.Zero;
            else if (linksets.Count == 1)
                return linksets[0].RootPrim.Position;

            Vector3 south_west = linksets[0].RootPrim.Position;
            Vector3 north_east = south_west;

            foreach(var linkset in linksets)
            {
                Vector3 pos = linkset.RootPrim.Position;

                if (pos.X < south_west.X) south_west.X = pos.X;
                if (pos.Y < south_west.Y) south_west.Y = pos.Y;
                if (pos.Z < south_west.Z) south_west.Z = pos.Z;

                if (pos.X > north_east.X) north_east.X = pos.X;
                if (pos.Y > north_east.Y) north_east.Y = pos.Y;
                if (pos.Z > north_east.Z) north_east.Z = pos.Z;
            }

            Vector3 diff = north_east - south_west;
            diff *= 0.5f;
            diff += south_west;

            return diff;
        }

        public void CancelImport()
        {
            ImporterBusy = false;
        }

        private void FinishImport()
        {
            importProgressForm?.ReportProgress(CurrentPrim, ImportPrims.Count, true);
            Proxy.AlertMessage("Import complete.", false);
            ImporterBusy = false;
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


        public void RezSupply()
        {
            if (InvSeedItem != null)
            {
                Proxy.Objects.RezInventoryItem(Proxy.Network.CurrentSim, InvSeedItem, Proxy.Agent.SimPosition + ImportOffset);
            }
            else if (SeedPrim != null)
            {
                Proxy.Objects.DuplicateObject(Proxy.Network.CurrentSim, SeedPrim.LocalID, Proxy.Agent.SimPosition + ImportOffset - SeedPrim.Position, PrimFlags.CreateSelected);
            }
            else
            {
                Proxy.Objects.AddPrim(Proxy.Network.CurrentSim, ImportPrims[CurrentPrim].PrimData, UUID.Zero, ImportPrims[CurrentPrim].Position, ImportPrims[CurrentPrim].Scale, ImportPrims[CurrentPrim].Rotation);
            }
        }

        private void Objects_ObjectUpdate(object sender, GridProxy.PrimEventArgs e)
        {
            if (!ImporterBusy) return;

            if ((e.IsNew && e.Prim.Flags.HasFlag(PrimFlags.CreateSelected))
                || (SeedPrim != null && e.IsNew && e.Prim.PrimData.Equals(SeedPrim.PrimData))) // opensim doesn't seem to send CreateSelected for duplicated prims??
            {
                Primitive import_prim = ImportPrims[CurrentPrim];
                Primitive prim = e.Prim;

                if (import_prim.ParentID == 0)
                {
                    IDToLocalID[import_prim.LocalID] = e.Prim.LocalID;
                    uint local_id = e.Prim.LocalID;
                    LinkSets[local_id] = new List<Primitive>() { e.Prim };
                }
                else
                {
                    uint parent = IDToLocalID[import_prim.ParentID];
                    LinkSets[parent].Add(e.Prim);
                }

                Proxy.Objects.UpdateObject(Proxy.Network.CurrentSim, e.Prim.LocalID, import_prim.Position, import_prim.Scale, import_prim.Rotation);

                Proxy.Objects.SetShape(e.Region, prim.LocalID, import_prim.PrimData);

                Proxy.Objects.SetExtraParams(Proxy.Network.CurrentSim, prim.LocalID, import_prim.Sculpt, import_prim.Flexible, import_prim.Light);

                Proxy.Objects.SetTextures(e.Region, prim.LocalID, import_prim.Textures);

                Proxy.Objects.SetName(e.Region, prim.LocalID, import_prim?.Properties?.Name ?? "Object");

                Proxy.Objects.SetDescription(e.Region, prim.LocalID, import_prim?.Properties?.Description ?? "(No Description)");

                CurrentPrim++;

                if (CurrentPrim >= ImportPrims.Count)
                {
                    foreach (var pair in LinkSets)
                    {
                        Proxy.Objects.LinkPrims(Proxy.Network.CurrentSim, pair.Value.Select(x => x.LocalID).ToList());
                    }

                    if (Attachments.Count > 0)
                    {
                        importProgressForm?.ReportProgress(CurrentPrim, ImportPrims.Count, false);

                        var link_timer = new LinkTimer(LinkSets, this);
                        link_timer.Complete += () =>
                        {
                            foreach (var pair in Attachments)
                            {
                                uint local_id = IDToLocalID[pair.Key];
                                AttachingItems[local_id] = pair.Key;
                                Proxy.Objects.AttachObject(Proxy.Network.CurrentSim, local_id, pair.Value.Point, pair.Value.Rotation, true);
                            }

                            FinishImport();
                        };
                        link_timer.Start();
                    }
                    else
                    {
                        FinishImport();
                    }
                }
                else
                {
                    importProgressForm?.ReportProgress(CurrentPrim, ImportPrims.Count, false);
                    RezSupply();
                }
            }
            else if(AttachingItems.ContainsKey(e.Prim.LocalID))
            {
                uint local_id = AttachingItems[e.Prim.LocalID];
                Proxy.Objects.SetRotation(Proxy.Network.CurrentSim, e.Prim.LocalID, Attachments[local_id].Rotation);
                Proxy.Objects.SetPosition(Proxy.Network.CurrentSim, e.Prim.LocalID, Attachments[local_id].Position);
                AttachingItems.Remove(e.Prim.LocalID);
            }
        }
    }

    public class AttachmentInfo
    {
        public Quaternion Rotation { get; set; }
        public Vector3 Position { get; set; }
        public AttachmentPoint Point { get; set; }

        public AttachmentInfo(AttachmentPoint point, Vector3 pos, Quaternion rot)
        {
            Point = point;
            Position = pos;
            Rotation = rot;
        }
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

        public Primitive SeedPrim { get; set; } = null;

        public InventoryItem InvItem { get; set; } = null;

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
                    prim.LocalID = uint.Parse(kvp.Key);
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

    public delegate void LinkComplete();

    public class LinkTimer : System.Timers.Timer
    {
        Dictionary<uint, List<Primitive>> PrimsToLink;

        public event LinkComplete Complete;

        private CopyBotPlugin Plugin;

        public LinkTimer(Dictionary<uint, List<Primitive>> linksets, CopyBotPlugin plugin) : base()
        {
            Plugin = plugin;
            PrimsToLink = linksets;

            base.Interval = 500;
            base.Elapsed += LinkTimer_Elapsed;
        }

        private void LinkTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if(Plugin.ImporterBusy == false)
            {
                base.Enabled = false;
                return;
            }

            bool is_done = true;

            foreach(var linkset in PrimsToLink.Values)
            {
                if (linkset.Where(x => x.ParentID == 0).ToList().Count > 1)
                {
                    is_done = false;
                    break;
                }
            }

            if(is_done)
            {
                base.Enabled = false;
                Complete?.Invoke();
            }
        }
    }
}
