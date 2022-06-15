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
using System.IO.Compression;
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

        private CoolProxyFrame Proxy;

        ImportProgressForm importProgressForm;

        public bool ImporterBusy { get; private set; } = false;
        public bool BuildingLinkset { get; private set; } = false;

        ZipArchive Zip;
        int EntryIndex;
        UUID FolderID;
        Dictionary<UUID, UUID> AssetReplacements;

        List<Primitive> ImportPrims;

        Dictionary<uint, List<Primitive>> LinkSets = new Dictionary<uint, List<Primitive>>();
        Dictionary<uint, uint> IDToLocalID = new Dictionary<uint, uint>();

        Dictionary<uint, AttachmentInfo> Attachments = new Dictionary<uint, AttachmentInfo>();
        Dictionary<uint, uint> AttachingItems = new Dictionary<uint, uint>();
        List<AssetWearable> Wearables = new List<AssetWearable>();

        int CurrentPrim = 0;

        int CurrentItem = 0;
        List<ImportableItem> Content;

        Vector3 ImportOffset = new Vector3(0, 0, 3);

        Primitive SeedPrim = null;
        InventoryItem InvSeedItem = null;

        string ExportDir = "./copybot";

        public CopyBotPlugin(CoolProxyFrame frame)
        {
            IGUI gui = frame.RequestModuleInterface<IGUI>();

            Proxy = frame;
            Instance = this;

            //GUI.AddToolButton("CopyBot", "Export Selected Objects", exportSelectedObjects);
            //GUI.AddToolButton("CopyBot", "Import Object from File", importXML);
            gui.AddToolButton("Objects", "Import with Selected Object", importXMLWithSeed);

            gui.AddSingleMenuItem("Save As...", exportAvatar);

            gui.AddInventoryItemOption("Import With...", importWithInv, AssetType.Object);

            gui.AddTrayOption("-", null);
            gui.AddTrayOption("Import Object from File...", importXML);
            gui.AddTrayOption("Export Selected Objects...", exportSelectedObjects);

            Proxy.Objects.ObjectUpdate += Objects_ObjectUpdate;


            AddSettingsTab(gui);


            ExportDir = Proxy.Settings.getString("SOGExportDir");
            if (ExportDir.EndsWith("/") == false) ExportDir += "/";
            ExportDir = Path.GetFullPath(ExportDir);

            if(!Directory.Exists(ExportDir))
            {
                Directory.CreateDirectory(ExportDir);
                File.Create(ExportDir + "exported_objects_go_here").Close();
            }
        }

        private void AddSettingsTab(IGUI gui)
        {
            Panel panel = new Panel();
            panel.Dock = DockStyle.Fill;

            Vector3 import_offset = Proxy.Settings.getVector("ImportOffset");

            var offset_x = new NumericUpDown();
            var offset_y = new NumericUpDown();
            var offset_z = new NumericUpDown();

            EventHandler handle_change = (x, y) =>
            {
                Vector3 offset = new Vector3((float)offset_x.Value, (float)offset_y.Value, (float)offset_z.Value);
                Proxy.Settings.setVector("ImportOffset", offset);
            };

            var label_x = new Label();
            label_x.Location = new Point(6, 21);
            label_x.Size = new Size(17, 13);
            label_x.Text = "X:";

            offset_x.DecimalPlaces = 3;
            offset_x.Location = new Point(29, 19);
            offset_x.Size = new Size(85, 20);
            offset_x.Value = (decimal)import_offset.X;
            offset_x.ValueChanged += handle_change;

            var label_y = new Label();
            label_y.Location = new Point(6, 47);
            label_y.Size = new Size(17, 13);
            label_y.Text = "Y:";

            offset_y.DecimalPlaces = 3;
            offset_y.Location = new Point(29, 45);
            offset_y.Size = new Size(85, 20);
            offset_y.Value = (decimal)import_offset.Y;
            offset_y.ValueChanged += handle_change;

            var label_z = new Label();
            label_z.Location = new Point(6, 73);
            label_z.Size = new Size(17, 13);
            label_z.Text = "Z:";

            offset_z.DecimalPlaces = 3;
            offset_z.Location = new Point(29, 71);
            offset_z.Size = new Size(85, 20);
            offset_z.Value = (decimal)import_offset.Z;
            offset_z.ValueChanged += handle_change;

            var group_box = new GroupBox();
            group_box.Controls.Add(label_x);
            group_box.Controls.Add(offset_x);
            group_box.Controls.Add(label_y);
            group_box.Controls.Add(offset_y);
            group_box.Controls.Add(label_z);
            group_box.Controls.Add(offset_z);
            group_box.Location = new Point(6, 6);
            group_box.Size = new Size(120, 100);
            group_box.Text = "Object Import Offset";

            panel.Controls.Add(group_box);

            var path_label = new Label();
            var path_box = new L33T.GUI.TextBox();
            var path_btn = new Button();

            panel.Controls.Add(path_label);
            panel.Controls.Add(path_box);
            panel.Controls.Add(path_btn);

            path_label.Location = new Point(140, 15);
            path_label.Size = new Size(100, 13);
            path_label.Text = "Export Directory:";

            path_box.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
            path_box.Location = new Point(140, 35);
            path_box.Size = new Size(-10, 20);
            path_box.Setting = "SOGExportDir";
            path_box.Margin = new Padding(10);
            path_box.Enabled = false;

            path_btn.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            path_btn.Location = new Point(140, 35);
            path_btn.Size = new Size(50, 20);
            path_btn.Text = "...";
            path_btn.Click += (s, e) =>
            {
                using (FolderBrowserDialog browser = new FolderBrowserDialog())
                {
                    if (browser.ShowDialog() == DialogResult.OK)
                    {
                        path_box.Text = browser.SelectedPath;
                        ExportDir = browser.SelectedPath;
                    }
                }
            };

            gui.AddSettingsTab("CopyBot", panel);
        }

        private void importWithInv(InventoryItem inventoryItem)
        {
            if (ImporterBusy) return;

            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Filter = "Object Backup|*.sog";
                dialog.InitialDirectory = ExportDir;
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
                    dialog.Filter = "Object Backup|*.sog";
                    dialog.InitialDirectory = ExportDir;
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
                    dialog.Filter = "Object Backup|*.sog";
                    dialog.InitialDirectory = ExportDir;
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

            form.TopMost = Proxy.Settings.getBool("KeepCoolProxyOnTop");
            Proxy.Settings.getSetting("KeepCoolProxyOnTop").OnChanged += (x, y) => { form.TopMost = (bool)y.Value; };

            form.Show();
        }

        private void exportAvatar(UUID target)
        {
            Avatar avatar = Proxy.Network.CurrentSim.ObjectsAvatars.Find(x => x.ID == target);

            if (avatar == null)
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

            ImportOffset = Proxy.Settings.getVector("ImportOffset");

            Zip = options.Archive;

            IDToLocalID.Clear();
            LinkSets.Clear();
            AttachingItems.Clear();
            Attachments.Clear();
            Wearables.Clear();

            SeedPrim = options.SeedPrim;
            InvSeedItem = options.InvItem;

            Content = options.Contents;

            importProgressForm = new ImportProgressForm();
            importProgressForm.Show();

            Wearables = GenerateWearable(options.Wearables);

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
            CurrentItem = 0;

            if(Zip != null)
            {
                if(Zip.Entries.Count > 1)
                {
                    AssetReplacements = new Dictionary<UUID, UUID>();
                    EntryIndex = 0;

                    UUID parent_id = Proxy.Inventory.SuitcaseID == UUID.Zero ? Proxy.Inventory.InventoryRoot : Proxy.Inventory.SuitcaseID;

                    FolderID = UUID.Random();
                    Proxy.OpenSim.XInventory.AddFolder("uploads", FolderID, parent_id, Proxy.Agent.AgentID, FolderType.None, 1, (success) =>
                    {
                        UploadNextAsset();
                    });

                    importProgressForm.Text = "Uploading Assets...";
                    return;
                }
            }
            
            FinishInit();
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
            Zip = null; // ???
        }

        private void FinishImport()
        {
            BuildingLinkset = false;

            if(Zip == null || Content.Count < 1)
            {
                importProgressForm.Text = "Import Complete";
                importProgressForm?.ReportProgress(CurrentPrim, ImportPrims.Count, true);
                Proxy.AlertMessage("Import complete.", false);
                ImporterBusy = false;
            }
            else
            {
                importProgressForm.Text = "Importing Content...";
                importProgressForm?.ReportProgress(CurrentItem, Content.Count, false);

                CurrentItem = 0;
                ImportContent();
            }
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

                foreach(var pair in item.Textures)
                {
                    wearable.Textures[pair.Key] = pair.Value;
                }

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

                IDToLocalID[import_prim.LocalID] = e.Prim.LocalID;

                if (import_prim.ParentID == 0)
                {
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
                        if (pair.Value.Count > 1)
                            Proxy.Objects.LinkPrims(Proxy.Network.CurrentSim, pair.Value.Select(x => x.LocalID).ToList());
                    }

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

                        if(AttachingItems.Count == 0)
                        {
                            FinishImport();
                        }
                    };
                    link_timer.Start();
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

                if(AttachingItems.Count == 0)
                {
                    FinishImport();
                }
            }
        }

        void UploadNextAsset()
        {
            EntryIndex++;

            if(EntryIndex > Zip.Entries.Count)
            {
                importProgressForm.Text = "Importing...";
                importProgressForm?.ReportProgress(CurrentPrim, ImportPrims.Count, false);
                FinishInit();
                return;
            }

            var entry = Zip.Entries[EntryIndex - 1];

            if(entry.Name == "sog")
            {
                UploadNextAsset();
                return;
            }

            int last_index = entry.Name.LastIndexOf(".");
            string type = entry.Name.Substring(last_index + 1);
            string name = entry.Name.Substring(0, last_index);

            if (Enum.TryParse(type, out AssetType at))
            {
                if (UUID.TryParse(name, out UUID id))
                {
                    using(Stream stream = entry.Open())
                    {
                        byte[] data;

                        using(MemoryStream mem = new MemoryStream())
                        {
                            stream.CopyTo(mem);
                            data = mem.ToArray();
                        }

                        Proxy.OpenSim.Assets.UploadAsset(UUID.Random(), at, id.ToString(), "", UUID.Zero, data, (success, new_id) =>
                        {
                            AssetReplacements[id] = new_id;
                            importProgressForm?.ReportProgress(EntryIndex - 1, Zip.Entries.Count - 1, false);
                            UploadNextAsset();
                        });
                    }
                }
            }
        }

        UUID ReplaceAsset(UUID original_id)
        {
            if(AssetReplacements.ContainsKey(original_id))
            {
                return AssetReplacements[original_id];
            }

            return original_id;
        }

        void FinishInit()
        {
            foreach (var item in Wearables)
            {
                if(Zip != null && item.Textures != null)
                {
                    foreach(var tex in item.Textures)
                    {
                        item.Textures[tex.Key] = ReplaceAsset(tex.Value);
                    }
                }

                UploadWearble(item.Name, item.AssetData, item.WearableType, item.AssetType);
            }

            if(Zip != null)
            {
                foreach (var prim in ImportPrims)
                {
                    if (prim.Sculpt != null && prim.Sculpt.SculptTexture != UUID.Zero)
                    {
                        prim.Sculpt.SculptTexture = ReplaceAsset(prim.Sculpt.SculptTexture);
                    }

                    if (prim.Textures != null)
                    {
                        if (prim.Textures.DefaultTexture.TextureID != UUID.Zero)
                            prim.Textures.DefaultTexture.TextureID = ReplaceAsset(prim.Textures.DefaultTexture.TextureID);

                        if (prim.Textures.DefaultTexture.MaterialID != UUID.Zero)
                            prim.Textures.DefaultTexture.MaterialID = ReplaceAsset(prim.Textures.DefaultTexture.MaterialID);

                        foreach (var entry in prim.Textures.FaceTextures)
                        {
                            if (entry != null)
                            {
                                if (entry.TextureID != UUID.Zero)
                                    entry.TextureID = ReplaceAsset(entry.TextureID);

                                if (entry.MaterialID != UUID.Zero)
                                    entry.MaterialID = ReplaceAsset(entry.MaterialID);
                            }
                        }
                    }
                }
            }

            if(ImportPrims.Count > 0)
            {
                BuildingLinkset = true;
                RezSupply();
            }
            else
            {
                FinishImport();
            }
        }

        void ImportContent()
        {
            if(CurrentItem >= Content.Count)
            {
                Proxy.Inventory.RequestFolderContents(FolderID, Proxy.Agent.AgentID, true, true, InventorySortOrder.ByDate, false);
                importProgressForm?.ReportProgress(Content.Count, Content.Count, true);
                importProgressForm.Text = "Import Complete";
                Proxy.AlertMessage("Import complete.", false);
                ImporterBusy = false;
                Zip = null;
                return;
            }

            ImportableItem importable = Content[CurrentItem];
            InventoryItem item = importable.Item;

            item.AssetUUID = ReplaceAsset(item.AssetUUID);
            item.ParentUUID = FolderID;
            item.UUID = UUID.Random();
            item.CreatorData = string.Empty;
            item.OwnerID = Proxy.Agent.AgentID;

            Proxy.OpenSim.XInventory.AddItem(item, (success) =>
            {
                if(success)
                {
                    uint local_id = IDToLocalID[importable.ObjectID];
                    if(item.AssetType == AssetType.LSLText)
                    {
                        Proxy.Inventory.CopyScriptToTask(local_id, item, true);
                    }
                    else
                    {
                        Proxy.Inventory.UpdateTaskInventory(local_id, item);
                    }
                }

                CurrentItem++;
                importProgressForm?.ReportProgress(CurrentItem, Content.Count, false);
                ImportContent();
            });
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
        public Dictionary<AvatarTextureIndex, UUID> Textures;

        public ImportableWearable(string name, WearableType type, Dictionary<int, int> param, Dictionary<AvatarTextureIndex, UUID> textures)
        {
            Name = name;
            Type = type;
            VisualParams = param;
            Textures = textures;
        }
    }

    public class ImportableItem
    {
        public uint ObjectID { get; private set; } = 0;
        public InventoryItem Item { get; private set; } = null;

        public ImportableItem(OSD item, uint object_id)
        {
            Item = InventoryItem.FromOSD(item);
            ObjectID = object_id;
        }
    }

    public class ImportOptions
    {
        public List<ImportableWearable> Wearables { get; private set; } = new List<ImportableWearable>();

        public List<ImportableItem> Contents { get; set; } = new List<ImportableItem>();

        public List<Linkset> Linksets { get; private set; } = new List<Linkset>();

        public bool KeepPositions { get; set; } = false;

        public Primitive SeedPrim { get; set; } = null;

        public InventoryItem InvItem { get; set; } = null;

        public ZipArchive Archive { get; set; } = null;

        public ImportOptions(string filename)
        {
            try
            {
                Archive = ZipFile.OpenRead(filename);
                var entry = Archive.GetEntry("sog");
                var stream = entry.Open();
                OSD osd = OSDParser.DeserializeLLSDXml(stream);
                AddFromOSD(osd);

                if(Archive.Entries.Count < 2)
                {
                    Archive = null;
                }
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

                    Dictionary<AvatarTextureIndex, UUID> textures = new Dictionary<AvatarTextureIndex, UUID>();

                    OSDMap texture_map = (OSDMap)entry["textures"];
                    foreach (KeyValuePair<string, OSD> pair in texture_map)
                    {
                        int index = Convert.ToInt32(pair.Key);
                        textures[(AvatarTextureIndex)index] = pair.Value.AsUUID();
                    }

                    Wearables.Add(new ImportableWearable(name, type, paramvalues, textures));
                }
                else
                {
                    OSDMap prim_osd = (OSDMap)kvp.Value;
                    Primitive prim = Primitive.FromOSD(prim_osd);
                    prim.LocalID = uint.Parse(kvp.Key);
                    prims.Add(prim);

                    if(prim_osd.ContainsKey("inventory"))
                    {
                        OSDArray item_array = (OSDArray)prim_osd["inventory"];
                        foreach(var item in item_array)
                        {
                            Contents.Add(new ImportableItem(item, prim.LocalID));
                        }
                    }
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
