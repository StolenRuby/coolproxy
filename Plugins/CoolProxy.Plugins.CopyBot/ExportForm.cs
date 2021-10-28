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
                    dataGridView.Rows.Add(true, CopyBot.Properties.Resources.Skin, TargetAvatar.Name + "'s Skin", 0, (int)WearableType.Skin, UUID.Zero);
                    dataGridView.Rows.Add(true, CopyBot.Properties.Resources.BodyShape, TargetAvatar.Name + "'s Shape", 0, (int)WearableType.Shape, UUID.Zero);
                    dataGridView.Rows.Add(true, CopyBot.Properties.Resources.Hair, TargetAvatar.Name + "'s Hair", 0, (int)WearableType.Hair, UUID.Zero);
                    dataGridView.Rows.Add(true, CopyBot.Properties.Resources.Inv_Eye, TargetAvatar.Name + "'s Eyes", 0, (int)WearableType.Eyes, UUID.Zero);

                    // todo: programatically determine what other wearables the avatar is wearing...
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

            OSDMap export = (OSDMap)Helpers.PrimListToOSD(final_selection);

            if (TargetAvatar?.VisualParameters != null)
            {
                var visual_params = TargetAvatar.VisualParameters;
                foreach (var type in wearables)
                {
                    string str_type = type.ToString().ToLower();
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

                    // todo: textures...

                    OSDMap wearable = new OSDMap();
                    wearable["type"] = "wearable";
                    wearable["params"] = visuals_osd;
                    wearable["name"] = TargetAvatar.Name + "'s " + type.ToString();
                    wearable["flag"] = (int)type;

                    export[UUID.Random().ToString()] = wearable;
                }
            }

            if (Mode == OutputMode.Save)
            {
                using (SaveFileDialog dialog = new SaveFileDialog())
                {
                    if (TargetAvatar != null)
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
                        string output = OSDParser.SerializeLLSDXmlString(export);
                        try { File.WriteAllText(dialog.FileName, output); }
                        catch (Exception ex) { Frame.SayToUser(ex.Message); }

                        Frame.AlertMessage("Saved to " + dialog.FileName, false);

                        this.Close();
                    }
                }
            }
            else
            {
                ImportOptions importOptions = new ImportOptions(export);

                if (Mode == OutputMode.Import)
                    CopyBotPlugin.Instance.ImportLinkset(importOptions);
                else
                    CopyBotPlugin.Instance.ForgeLinkset(importOptions);

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
