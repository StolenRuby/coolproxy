using GridProxy;
using OpenMetaverse;
using OpenMetaverse.Packets;
using OpenMetaverse.StructuredData;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
                        new ImportForm(Proxy, dialog.FileName, this).ShowDialog();
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

        internal void ForgeLinkset(List<Linkset> linksets)
        {
            List<OSAssetPrim> assetPrims = new List<OSAssetPrim>();
            foreach(var linkset in linksets)
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

        internal void ImportLinkset(List<Linkset> linksets)
        {
            if (ImporterBusy) return;

            Proxy.SayToUser(string.Format("Importing {0} linksets", linksets.Count));

            Linksets = linksets;
            ImporterBusy = true;

            importWorker = new BackgroundWorker();
            importWorker.DoWork += Worker_DoWork;
            importWorker.RunWorkerCompleted += ImportWorker_RunWorkerCompleted;
            importWorker.RunWorkerAsync();
        }

        private void ImportWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            ImporterBusy = false;
            Proxy.AlertMessage("Import complete.", false);
        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            primsCreated = new List<Primitive>();
            linkQueue = new List<uint>();

            //Proxy.SayToUser("Importing " + Linksets.Count + " structures.");

            foreach (Linkset linkset in Linksets)
            {
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

                    state = ImporterState.RezzingChildren;

                    // Rez the child prims
                    foreach (Primitive prim in linkset.Children)
                    {
                        currentPrim = prim;
                        currentPosition = prim.Position + linkset.RootPrim.Position;

                        Proxy.Objects.AddPrim(Proxy.Network.CurrentSim, prim.PrimData, UUID.Zero, currentPosition, prim.Scale, prim.Rotation);

                        if (!primDone.WaitOne(10000, false))
                        {
                            Proxy.SayToUser("Rez failed, timed out while creating child prim.");
                            return;
                        }

                        Proxy.Objects.SetPosition(Proxy.Network.CurrentSim, primsCreated[primsCreated.Count - 1].LocalID, currentPosition);
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
}
