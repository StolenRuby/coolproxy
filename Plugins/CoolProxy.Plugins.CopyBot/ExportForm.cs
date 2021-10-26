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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CoolProxy.Plugins.CopyBot
{
    public partial class ExportForm : Form
    {
        CoolProxyFrame Frame = null;

        Dictionary<int, uint> indexToLocalID= new Dictionary<int, uint>();
        Dictionary<UUID, int> uuidToIndex = new Dictionary<UUID, int>();

        List<Primitive> primsToExport = new List<Primitive>();
        List<UUID> primsWaiting = new List<UUID>();

        Avatar TargetAvatar;

        enum OutputMode
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

        private void Objects_ObjectProperties(object sender, ObjectPropertiesEventArgs e)
        {
            lock(primsWaiting)
            {
                if (primsWaiting.Contains(e.Properties.ObjectID))
                {
                    primsWaiting.Remove(e.Properties.ObjectID);
                    updateCount();

                    if(uuidToIndex.ContainsKey(e.Properties.ObjectID))
                    {
                        int index = uuidToIndex[e.Properties.ObjectID];
                        dataGridView.Invoke(new Action(() =>
                        {
                            dataGridView.Rows[index].Cells[2].Value = e.Properties.Name;
                        }));
                    }
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
                    dataGridView.Rows.Add(true, CopyBot.Properties.Resources.BodyShape, "Shape", 0, (int)WearableType.Shape);
                    //dataGridView.Rows.Add(true, CopyBot.Properties.Resources.Skin, "Baked Skin", 0, (int)WearableType.Skin);
                }

                selectedRoots = Frame.Network.CurrentSim.ObjectsPrimitives.Values.Where((x) => { return x.ParentID == TargetAvatar.LocalID; }).ToList();
            }
            else
            {
                selectedRoots = Frame.Network.CurrentSim.ObjectsPrimitives.Values.Where((x) =>
                    { return selection.Contains(x.LocalID) && x.ParentID == 0; }).ToList();
            }
            
            foreach (Primitive root in selectedRoots)
            {
                string text = root.LocalID.ToString();
                if ((root.Properties?.Name ?? string.Empty) != string.Empty)
                    text = root.Properties.Name;

                int index = dataGridView.Rows.Add(true, CopyBot.Properties.Resources.Object, text, 1, root.LocalID);

                uuidToIndex.Add(root.ID, index);
                indexToLocalID.Add(index, root.LocalID);
            }
            
            List<uint> rootLocalIDs = selectedRoots.Select(x => x.LocalID).ToList();

            primsToExport = Frame.Network.CurrentSim.ObjectsPrimitives.Values.Where(x => rootLocalIDs.Contains(x.LocalID) || rootLocalIDs.Contains(x.ParentID)).ToList();
            primsWaiting = primsToExport.Select(x => x.ID).ToList();

            if (primsToExport.Count > 0)
                Frame.Objects.SelectObjects(Frame.Network.CurrentSim, primsToExport.Select(x => x.LocalID).ToArray());
            else
                countLabel.Text = "Names loaded!";

            this.Focus();
        }

        static int[] SHAPE_PARAM_LIST = new int[] { 1, 2, 4, 5, 6, 7, 8, 10, 11, 12, 13, 14, 15, 17, 18, 19, 20, 21, 22, 23, 24, 25, 27, 33, 34, 35, 36, 37, 38, 80, 105, 155, 157, 185, 193, 196, 505, 506, 507, 515, 517, 518, 629, 637, 646, 647, 649, 650, 652, 653, 656, 659, 662, 663, 664, 665, 675, 676, 678, 682, 683, 684, 685, 690, 692, 693, 753, 756, 758, 759, 760, 764, 765, 769, 773, 795, 796, 799, 841, 842, 879, 880 };

        private void saveButton_Click(object sender, EventArgs e)
        {
            List<uint> selected = new List<uint>();
            List<WearableType> wearables = new List<WearableType>();

            for (int i = 0; i < dataGridView.Rows.Count; i++)
            {
                if ((bool)dataGridView.Rows[i].Cells[0].Value)
                {
                    int type = (int)dataGridView.Rows[i].Cells[3].Value;

                    if (type == 0) // wearable
                    {
                        WearableType wearable = (WearableType)(int)dataGridView.Rows[i].Cells[4].Value;
                        wearables.Add(wearable);
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

            if (Mode == OutputMode.Save)
            {
                using (SaveFileDialog dialog = new SaveFileDialog())
                {
                    if(TargetAvatar != null)
                    {
                        dialog.FileName = TargetAvatar.Name;
                    }
                    else if (selected.Count == 1)
                    {
                        var prim = Frame.Network.CurrentSim.ObjectsPrimitives[selected[0]];
                        if (prim.Properties != null)
                        {
                            dialog.FileName = prim.Properties.Name;
                        }
                    }
                    else
                    {
                        dialog.FileName = "Multiple Objects";
                    }

                    dialog.Filter = "Object XML|*.xml";
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        OSDMap map = (OSDMap)Helpers.PrimListToOSD(final_selection);

                        string output = OSDParser.SerializeLLSDXmlString(map);
                        try { File.WriteAllText(dialog.FileName, output); }
                        catch (Exception ex) { Frame.SayToUser(ex.Message); }

                        Frame.SayToUser("Exported " + final_selection.Count + " prims to " + dialog.FileName);

                        this.Close();
                    }
                }
            }
            else
            {
                List<Primitive> updated_prim_list = new List<Primitive>();
                foreach(var p in final_selection)
                {
                    var n = new Primitive(p);
                    if(p.IsAttachment)
                    {
                        n.PrimData.State = (byte)p.PrimData.AttachmentPoint;
                        n.ParentID = 0;
                    }

                    n.OwnerID = Frame.Agent.AgentID;

                    if (n.Properties != null)
                    {
                        n.Properties.OwnerID = Frame.Agent.AgentID;

                        if(n.Properties.Permissions != null)
                        {
                            n.Properties.Permissions.EveryoneMask = PermissionMask.None;
                            n.Properties.Permissions.GroupMask = PermissionMask.None;
                        }
                    }

                    updated_prim_list.Add(n);
                }

                var visual_params = TargetAvatar.VisualParameters;

                //if (JustATestPlugin.Instance.Appearances.TryGetValue(TargetAvatar, out appearancePacket))
                {
                    // load visual parameters onto dictionary
                    Dictionary<int, int> paramvalues = new Dictionary<int, int>();
                    int pcount = 0;
                    foreach (KeyValuePair<int, VisualParam> kvp in VisualParams.Params)
                    {
                        if (kvp.Value.Group == 0)
                        {
                            paramvalues.Add(kvp.Value.ParamID, visual_params[pcount]);
                            pcount++;
                        }
                    }

                    foreach (WearableType type in wearables)
                    {
                        AssetBodypart shape = new AssetBodypart();
                        shape.Name = "Shape";
                        shape.Description = string.Empty;
                        shape.Creator = UUID.Zero;
                        shape.ForSale = SaleType.Not;
                        shape.Group = UUID.Zero;
                        shape.GroupOwned = false;
                        shape.LastOwner = UUID.Zero;
                        shape.WearableType = type;

                        var perms = new Permissions();
                        perms.BaseMask = PermissionMask.All;
                        perms.EveryoneMask = PermissionMask.None;
                        perms.GroupMask = PermissionMask.None;
                        perms.NextOwnerMask = PermissionMask.All;
                        perms.OwnerMask = PermissionMask.All;

                        shape.Permissions = perms;

                        shape.Params = new Dictionary<int, float>();

                        if(type == WearableType.Shape)
                        {
                            foreach (int param in SHAPE_PARAM_LIST)
                            {
                                shape.Params.Add(param, Utils.ByteToFloat((byte)paramvalues[param], VisualParams.Params[param].MinValue, VisualParams.Params[param].MaxValue));
                            }
                        }
                        // todo: the rest...

                        shape.Encode();

                        CopyBotPlugin.Instance.UploadWearble(TargetAvatar.Name + "'s " + type.ToString(), shape.AssetData, type, AssetType.Bodypart);

                        //Frame.OpenSim.Assets.UploadAsset(UUID.Random(), AssetType.Bodypart, "Shape", "", UUID.Zero, shape.AssetData, (success, new_asset_id) =>
                        //{
                        //    if(success)
                        //    {

                        //        UUID folder_id = Frame.Inventory.SuitcaseID != UUID.Zero ?
                        //            Frame.Inventory.FindSuitcaseFolderForType(FolderType.BodyPart) :
                        //            Frame.Inventory.FindFolderForType(FolderType.BodyPart);

                        //        UUID item_id = UUID.Random();

                        //        Frame.OpenSim.XInventory.AddItem(folder_id, item_id, new_asset_id, AssetType.Bodypart, InventoryType.Wearable, (uint)type, "Shape", "", DateTime.UtcNow, (item_succes) =>
                        //        {
                        //            if (item_succes)
                        //            {
                        //                Frame.SayToUser("Success!");
                        //                Frame.Inventory.RequestFetchInventory(item_id, Frame.Agent.AgentID, false);
                        //            }
                        //            else Frame.SayToUser("Failed!");
                        //        });
                        //    }
                        //});
                    }
                }

                List<Linkset> linksets = CopyBotPlugin.PrimListToLinksetList(updated_prim_list);
                if (Mode == OutputMode.Forge)
                {
                    CopyBotPlugin.Instance.ForgeLinkset(linksets);
                }
                else if(Mode == OutputMode.Import)
                {
                    CopyBotPlugin.Instance.ImportLinkset(linksets);
                }
                this.Close();
            }
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
            checkBox1.Visible = true;
            checkBox1.Checked = false;
        }

        private void importToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Mode = OutputMode.Import;
            splitButton1.Text = "Import";
            checkBox1.Visible = false;
        }

        private void forgeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Mode = OutputMode.Forge;
            splitButton1.Text = "Forge";
            checkBox1.Text = "Preserve Properties";
            checkBox1.Visible = true;
            checkBox1.Checked = true;
        }

        private void selectAllButton_Click(object sender, EventArgs e)
        {
            updateSelection(true, true);
        }

        private void selectObjectsButton_Click(object sender, EventArgs e)
        {
            updateSelection(true, false);
        }

        private void selectWearablesButton_Click(object sender, EventArgs e)
        {
            updateSelection(false, true);
        }

        void updateSelection(bool objects, bool wearables)
        {
            for (int i = 0; i < dataGridView.Rows.Count; i++)
            {
                int type = (int)dataGridView.Rows[i].Cells[3].Value;

                if (type == 0)
                {
                    dataGridView.Rows[i].Cells[0].Value = wearables;
                }
                else
                {
                    dataGridView.Rows[i].Cells[0].Value = objects;
                }
            }
        }
    }
}
