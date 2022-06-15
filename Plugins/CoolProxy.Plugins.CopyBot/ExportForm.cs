using OpenMetaverse;
using OpenMetaverse.Assets;
using OpenMetaverse.Packets;
using OpenMetaverse.StructuredData;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CoolProxy.Plugins.CopyBot
{
    public partial class ExportForm : Form
    {
        CoolProxyFrame Frame = null;

        List<Primitive> primsToExport = new List<Primitive>();
        List<UUID> primsWaiting = new List<UUID>();

        Avatar TargetAvatar;

        public enum OutputMode
        {
            Save,
            Forge,
            Import
        };

        OutputMode Mode = OutputMode.Save;

        public ExportForm(CoolProxyFrame frame) : this(frame, null)
        {
        }

        public ExportForm(CoolProxyFrame frame, Avatar avatar)
        {
            this.Frame = frame;
            this.TargetAvatar = avatar;

            InitializeComponent();

            Frame.Objects.ObjectProperties += Objects_ObjectProperties;

            this.FormClosing += (x, y) => Frame.Objects.ObjectProperties -= Objects_ObjectProperties;
        }

        private void Objects_ObjectProperties(object sender, GridProxy.ObjectPropertiesEventArgs e)
        {
            lock(primsWaiting)
            {
                if (primsWaiting.Contains(e.Properties.ObjectID))
                {
                    primsWaiting.Remove(e.Properties.ObjectID);

                    dataGridView.BeginInvoke(new Action(() =>
                    {
                        foreach(DataGridViewRow row in dataGridView.Rows)
                        {
                            if((UUID)row.Cells[5].Value == e.Properties.ObjectID)
                            {
                                string name = e.Properties.Name;

                                if(e.Prim?.IsAttachment ?? false)
                                {
                                    name += " (worn on " + Utils.EnumToText(e.Prim.PrimData.AttachmentPoint) + ")";
                                }

                                row.Cells[2].Value = name;
                                break;
                            }
                        }
                    }));

                    updateCount();
                }
            }
        }

        private void updateCount()
        {
            if(countLabel.InvokeRequired) countLabel.BeginInvoke(new Action(() => { updateCount(); }));
            else
            {
                int count = primsToExport.Count;
                int waiting = count - primsWaiting.Count;

                if(waiting == count)
                {
                    countLabel.Text = "Names loaded!";
                }
                else
                {
                    countLabel.Text = string.Format("{0}/{1}", waiting, count);
                }
            }
        }

        private void OtherExportForm_Load(object sender, EventArgs e)
        {
            List<uint> selection = Frame.Agent.Selection.ToList();

            List<Primitive> selectedRoots;
            
            if(TargetAvatar != null)
            {
                if(TargetAvatar.VisualParameters != null)
                {
                    dataGridView.Rows.Add(true, CopyBot.Properties.Resources.Skin, TargetAvatar.Name + "'s Baked Skin", 0, (int)WearableType.Skin, UUID.Zero);
                    dataGridView.Rows.Add(true, CopyBot.Properties.Resources.BodyShape, TargetAvatar.Name + "'s Baked Shape", 0, (int)WearableType.Shape, UUID.Zero);
                    dataGridView.Rows.Add(true, CopyBot.Properties.Resources.Hair, TargetAvatar.Name + "'s Baked Hair", 0, (int)WearableType.Hair, UUID.Zero);
                    dataGridView.Rows.Add(true, CopyBot.Properties.Resources.Inv_Eye, TargetAvatar.Name + "'s Baked Eyes", 0, (int)WearableType.Eyes, UUID.Zero);
                }

                selectedRoots = Frame.Network.CurrentSim.ObjectsPrimitives.FindAll((x) => { return x.ParentID == TargetAvatar.LocalID; }).ToList();
            }
            else
            {
                selectedRoots = Frame.Network.CurrentSim.ObjectsPrimitives.FindAll(x => { return selection.Contains(x.LocalID) && x.ParentID == 0; }).ToList();
            }
            
            foreach (Primitive root in selectedRoots)
            {
                string text = root.LocalID.ToString();
                if ((root.Properties?.Name ?? string.Empty) != string.Empty)
                    text = root.Properties.Name;

                if(root.IsAttachment)
                {
                    text += " (worn on " + Utils.EnumToText(root.PrimData.AttachmentPoint) + ")";
                }
                
                dataGridView.Rows.Add(true, CopyBot.Properties.Resources.Object, text, 1, root.LocalID, root.ID);
            }
            
            List<uint> rootLocalIDs = selectedRoots.Select(x => x.LocalID).ToList();

            primsToExport = Frame.Network.CurrentSim.ObjectsPrimitives.FindAll(x => rootLocalIDs.Contains(x.LocalID) || rootLocalIDs.Contains(x.ParentID)).ToList();
            primsWaiting = primsToExport.Select(x => x.ID).ToList();

            if (primsToExport.Count > 0) // todo: seperate into packets of 250 blocks (or less)
                Frame.Objects.SelectObjects(Frame.Network.CurrentSim, primsToExport.Select(x => x.LocalID).ToArray());
            else
                countLabel.Text = "Names loaded!";

            this.Focus();
        }

        private void saveButton_Click(object sender, EventArgs e)
        {
            List<uint> selected = new List<uint>();
            Dictionary<WearableType, string> wearables = new Dictionary<WearableType, string>();

            for (int i = 0; i < dataGridView.Rows.Count; i++)
            {
                if ((bool)dataGridView.Rows[i].Cells[0].Value)
                {
                    int type = (int)dataGridView.Rows[i].Cells[3].Value;

                    if (type == 0) // wearable
                    {
                        WearableType wearable = (WearableType)(int)dataGridView.Rows[i].Cells[4].Value;
                        wearables[wearable] = (string)dataGridView.Rows[i].Cells[2].Value;
                    }
                    else
                    {
                        uint local_id = (uint)dataGridView.Rows[i].Cells[4].Value;
                        selected.Add(local_id);
                    }
                }
            }

            if (selected.Count == 0 && wearables.Count == 0)
                return;

            var final_selection = primsToExport.Where(x => selected.Contains(x.LocalID) || selected.Contains(x.ParentID)).ToList();

            OSDMap export = (OSDMap)Helpers.PrimListToOSD(final_selection);

            if (TargetAvatar?.VisualParameters != null)
            {
                var textures = (Primitive.TextureEntryFace[])TargetAvatar.Textures.FaceTextures.Clone();

                // swap in the baked textures...
                if(TargetAvatar.ID != Frame.Agent.AgentID)
                {
                    textures[0] = textures[8];   // head
                    textures[5] = textures[9];   // upper
                    textures[6] = textures[10];  // lower
                    textures[3] = textures[11];  // eyes
                }

                var visual_params = TargetAvatar.VisualParameters;
                foreach (var type in wearables)
                {
                    string str_type = type.Key.ToString().ToLower();
                    Dictionary<int, int> paramvalues = new Dictionary<int, int>();
                    int pcount = 0;
                    OSDMap visuals_osd = new OSDMap();
                    foreach (KeyValuePair<int, VisualParam> kvp in VisualParams.Params)
                    {
                        if (kvp.Value.Group == 0 && kvp.Value.Wearable == str_type)
                        {
                            paramvalues.Add(kvp.Value.ParamID, visual_params[pcount]);
                            visuals_osd[kvp.Value.ParamID.ToString()] = visual_params[pcount];
                            pcount++;
                        }
                    }

                    OSDMap texture_map = new OSDMap();

                    for(int te = 0; te < (int)AvatarTextureIndex.NumberOfEntries; te++)
                    {
                        if (AppearanceDictionary.getWearbleType((AvatarTextureIndex)te) == type.Key)
                        {
                            var tef = textures[te];
                            if (tef != null)
                            {
                                texture_map[string.Format("{0}", te)] = tef.TextureID;
                            }
                        }
                    }

                    OSDMap wearable = new OSDMap();
                    wearable["type"] = "wearable";
                    wearable["params"] = visuals_osd;
                    wearable["textures"] = texture_map;
                    wearable["name"] = type.Value;
                    wearable["flag"] = (int)type.Key;

                    export[UUID.Random().ToString()] = wearable;
                }
            }

            string filename = "untitled";

            if (Mode == OutputMode.Save)
            {
                using (SaveFileDialog dialog = new SaveFileDialog())
                {
                    if (selected.Count == 1 && wearables.Count == 0)
                    {
                        var prim = Frame.Network.CurrentSim.ObjectsPrimitives[selected[0]];
                        if (prim.Properties != null)
                        {
                            filename = prim.Properties.Name;
                        }
                    }
                    else if (selected.Count == 0 && wearables.Count == 1)
                    {
                        filename = wearables.First().Value;
                    }
                    else if (TargetAvatar != null)
                    {
                        filename = TargetAvatar.Name;
                    }
                    else
                    {
                        filename = "Multiple Objects";
                    }

                    dialog.Filter = "Object Backup|*.sog";
                    dialog.InitialDirectory = Path.GetFullPath(Frame.Settings.getString("SOGExportDir"));
                    dialog.FileName = filename;
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        filename = dialog.FileName;
                    }
                    else return;
                }
            }

            // Start the exporter on a task to the form can dispose...
            new Task(() =>
            {
                new ExportWorker(Frame, export, final_selection, checkBox1.Checked, checkBox2.Checked, Mode, filename);
            }).Start();

            this.Close();
        }

        private void closeButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Mode = OutputMode.Save;
            splitButton1.Text = "Save As...";
            checkBox1.Text = "Download Assets";
            checkBox1.Checked = false;
            checkBox1.Enabled = true;
        }

        private void importToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Mode = OutputMode.Import;
            splitButton1.Text = "Import";
            checkBox1.Text = "Download Assets";
            checkBox1.Checked = false;
            checkBox1.Enabled = false;
        }

        private void forgeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Mode = OutputMode.Forge;
            splitButton1.Text = "Forge";
            checkBox1.Text = "Preserve Properties";
            checkBox1.Checked = true;
            checkBox1.Enabled = true;
        }

        private void selectAllButton_Click(object sender, EventArgs e)
        {
            if (dataGridView.Rows.Count < 1) return;

            bool enable = false;

            for (int i = 0; i < dataGridView.Rows.Count; i++)
            {
                enable = !(bool)dataGridView.Rows[i].Cells[0].Value;
                break;
            }

            for (int i = 0; i < dataGridView.Rows.Count; i++)
            {
                dataGridView.Rows[i].Cells[0].Value = enable;
            }
        }

        private void selectObjectsButton_Click(object sender, EventArgs e)
        {
            if (dataGridView.Rows.Count < 1) return;

            bool enable = false;

            for (int i = 0; i < dataGridView.Rows.Count; i++)
            {
                if ((int)dataGridView.Rows[i].Cells[3].Value != 0)
                {
                    enable = !(bool)dataGridView.Rows[i].Cells[0].Value;
                    break;
                }
            }

            for (int i = 0; i < dataGridView.Rows.Count; i++)
            {
                if ((int)dataGridView.Rows[i].Cells[3].Value != 0)
                {
                    dataGridView.Rows[i].Cells[0].Value = enable;
                }
            }
        }

        private void selectWearablesButton_Click(object sender, EventArgs e)
        {
            if (dataGridView.Rows.Count < 1) return;

            bool enable = false;

            for (int i = 0; i < dataGridView.Rows.Count; i++)
            {
                if ((int)dataGridView.Rows[i].Cells[3].Value == 0)
                {
                    enable = !(bool)dataGridView.Rows[i].Cells[0].Value;
                    break;
                }
            }

            for (int i = 0; i < dataGridView.Rows.Count; i++)
            {
                if ((int)dataGridView.Rows[i].Cells[3].Value == 0)
                {
                    dataGridView.Rows[i].Cells[0].Value = enable;
                }
            }
        }

        public class ExportWorker
        {
            CoolProxyFrame Proxy;

            List<Primitive> PrimsToExport;
            bool ExportContents;
            bool ExportAssets;
            OutputMode Mode;
            string Filename;

            OSDMap ExportMap;

            Dictionary<UUID, AssetType> AssetsToExport = new Dictionary<UUID, AssetType>();
            Dictionary<InventoryItem, uint> TaskAssetsToExport = new Dictionary<InventoryItem, uint>();

            ZipArchive Archive;

            bool UseRobust = false;

            public ExportWorker(CoolProxyFrame proxy, OSDMap map, List<Primitive> prims, bool assets, bool inv, OutputMode mode, string filename)
            {
                Proxy = proxy;
                ExportMap = map;
                PrimsToExport = prims;
                ExportContents = inv;
                ExportAssets = assets;
                Mode = mode;
                Filename = filename;
                UseRobust = proxy.Network.CurrentSim.AssetServerURI != string.Empty;

                if(Mode == OutputMode.Import)
                {
                    FinishExport();
                    return;
                }
                else if(Mode == OutputMode.Save)
                {
                    if (File.Exists(filename)) File.Delete(filename);
                    Archive = ZipFile.Open(filename, ZipArchiveMode.Create);
                }

                if(!ExportAssets && !ExportContents)
                {
                    FinishExport();
                }
                else
                {
                    if (ExportAssets)
                    {
                        foreach(var prim in PrimsToExport)
                        {
                            if(prim.Sculpt != null)
                            {
                                UUID id = prim.Sculpt.SculptTexture;
                                if(!AssetsToExport.ContainsKey(id) && id != UUID.Zero)
                                {
                                    AssetsToExport[id] = prim.Sculpt.Type == SculptType.Mesh ? AssetType.Mesh : AssetType.Texture;
                                }
                            }

                            LogTextureEntry(prim.Textures.DefaultTexture);
                            foreach (var texture in prim.Textures.FaceTextures)
                                LogTextureEntry(texture);
                        }

                        // todo: maybe merge these?
                        foreach(OSD entry in map.Values)
                        {
                            OSDMap item = (OSDMap)entry;

                            if(item["type"] == "wearable")
                            {
                                OSDMap textures = (OSDMap)item["textures"];

                                foreach(OSD t in textures.Values)
                                {
                                    AssetsToExport[t.AsUUID()] = AssetType.Texture;
                                }
                            }
                        }
                    }

                    string nouns = string.Empty;
                    if (ExportAssets && ExportContents)
                        nouns = "inventory and assets";
                    else if (ExportAssets)
                        nouns = "assets";
                    else if (ExportContents)
                        nouns = "inventory";

                    Proxy.AlertMessage("Starting export of " + nouns + "...", false);

                    if (ExportContents)
                    {
                        NextTaskInv();
                    }
                    else
                    {
                        NextAsset();
                    }
                }
            }

            void LogTextureEntry(Primitive.TextureEntryFace face)
            {
                if (face != null)
                {
                    UUID texture_id = face.TextureID;
                    UUID material_id = face.MaterialID;

                    if (!AssetsToExport.ContainsKey(texture_id) && texture_id != UUID.Zero)
                        AssetsToExport[texture_id] = AssetType.Texture;

                    if (!AssetsToExport.ContainsKey(material_id) && material_id != UUID.Zero)
                        AssetsToExport[material_id] = AssetType.Texture;
                }
            }

            void HandleInventory(uint object_id, bool success, List<InventoryBase> items)
            {
                if(success)
                {
                    OSDArray inv_items = new OSDArray();
                    foreach(var item in items)
                    {
                        if(item is InventoryItem)
                        {
                            InventoryItem inv_item = (InventoryItem)item;
                            if (inv_item.AssetType == AssetType.Notecard || inv_item.AssetType == AssetType.LSLText)
                                TaskAssetsToExport[inv_item] = object_id;
                            else if (inv_item.AssetUUID != UUID.Zero)
                            {
                                inv_items.Add(item.GetOSD());

                                if(ExportAssets)
                                {
                                    AssetsToExport[inv_item.AssetUUID] = inv_item.AssetType;
                                }
                            }
                        }
                    }

                    if(inv_items.Count > 0)
                    {
                        ((OSDMap)ExportMap[object_id.ToString()])["inventory"] = inv_items;
                    }
                }
                else
                {
                    Logger.Log("[EXPORTER] Failed to download inventory for " + object_id.ToString(), Helpers.LogLevel.Info);
                }

                NextTaskInv();
            }

            void NextTaskInv()
            {
                if (PrimsToExport.Count > 0)
                {
                    Primitive next = PrimsToExport[0];
                    PrimsToExport.Remove(next);
                    Proxy.Inventory.GetTaskInventory(next.ID, next.LocalID, 60000, HandleInventory);
                }
                else
                {
                    Logger.Log("[EXPORTER] Finished downloading contents...", Helpers.LogLevel.Info);
                    NextAsset();
                }
            }

            private void HandleAsset(UUID id, AssetType type, bool success, byte[] data)
            {
                if(success)
                {
                    AddToArchive(id.ToString() + "." + type.ToString(), data);
                }

                NextAsset();
            }

            void NextAsset()
            {
                if(AssetsToExport.Count > 0)
                {
                    var first = AssetsToExport.First();
                    AssetsToExport.Remove(first.Key);

                    if (UseRobust)
                        Proxy.OpenSim.Assets.DownloadAsset(first.Key, (x, y) => HandleAsset(first.Key, first.Value, x, y));
                    else
                        Proxy.Assets.EasyDownloadAsset(first.Key, first.Value, (x, y) => HandleAsset(first.Key, first.Value, x, y));
                }
                else if(TaskAssetsToExport.Count > 0)
                {
                    var first = TaskAssetsToExport.First();
                    TaskAssetsToExport.Remove(first.Key);

                    var item = first.Key;

                    Proxy.Assets.RequestInventoryAsset(UUID.Zero, item.UUID, item.ParentUUID, item.OwnerID, item.AssetType, true, (x, y) =>
                    {
                        if(x.Success)
                        {
                            AddToArchive(y.AssetID.ToString() + "." + y.AssetType.ToString(), y.AssetData);

                            item.AssetUUID = y.AssetID;
                            uint object_id = first.Value;

                            OSDMap obj = (OSDMap)ExportMap[object_id.ToString()];

                            OSD val;
                            OSDArray items;

                            if (obj.TryGetValue("inventory", out val))
                                items = (OSDArray)val;
                            else
                                items = new OSDArray();

                            items.Add(item.GetOSD());

                            obj["inventory"] = items;
                        }
                        else
                        {
                            Logger.Log("[EXPORTER] Failed to download " + item.Name + " (" + item.UUID.ToString() + ")", Helpers.LogLevel.Info);
                        }

                        NextAsset();
                    });
                }
                else
                {
                    Logger.Log("[EXPORTER] Finished downloading assets...", Helpers.LogLevel.Info);
                    FinishExport();
                }
            }

            void AddToArchive(string name, byte[] data)
            {
                try
                {
                    var zipEntry = Archive.CreateEntry(name);

                    using (var stream = new MemoryStream(data))
                    using (var entry = zipEntry.Open())
                    {
                        stream.CopyTo(entry);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log(ex.Message, Helpers.LogLevel.Debug);
                }
            }

            void FinishExport()
            {
                if (Mode == OutputMode.Save)
                {
                    AddToArchive("sog", OSDParser.SerializeLLSDXmlToBytes(ExportMap));
                    Archive.Dispose();
                    Proxy.AlertMessage("Saved to " + Filename, false);
                }
                else
                {
                    ImportOptions options = new ImportOptions(ExportMap);

                    if (Mode == OutputMode.Import)
                        CopyBotPlugin.Instance.ImportLinkset(options);
                    else
                        CopyBotPlugin.Instance.ForgeLinkset(options);
                }
            }
        }
    }
}
