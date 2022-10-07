using CoolProxy;
using OpenMetaverse;
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
    enum ImportMode
    {
        Import,
        Forge,
        Upload
    }

    public partial class ImportForm : Form
    {
        CopyBotPlugin ThePlugin;
        CoolProxyFrame Proxy;

        ImportMode Mode = ImportMode.Import;

        ImportOptions Options;

        public ImportForm(CoolProxyFrame frame, ImportOptions options, CopyBotPlugin plugin)
        {
            Options = options;

            InitializeComponent();

            ThePlugin = plugin;
            Proxy = frame;
            Focus();
        }

        Dictionary<UUID, object> importMap = new Dictionary<UUID, object>();

        private void PostBuild(object sender, EventArgs e)
        {
            if(Options.Archive == null)
            {
                checkBox2.Enabled = false;
            }

            foreach(var wearable in Options.Wearables)
            {
                UUID id = UUID.Random();
                dataGridView.Rows.Add(true, CopyBotPlugin.WearableTypeToIcon(wearable.Type), wearable.Name, 0, wearable.Type, id);
                importMap[id] = wearable;
            }

            foreach(var linkset in Options.Linksets)
            {
                dataGridView.Rows.Add(true, Properties.Resources.Object, linkset.RootPrim.Properties?.Name ?? "Object", 1, linkset.RootPrim.LocalID, linkset.RootPrim.ID);
                importMap[linkset.RootPrim.ID] = linkset;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Options.Archive = null; // ???
            this.Close();
        }

        private void importToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Mode = ImportMode.Import;
            splitButton1.Text = "Import";
        }

        private void forgeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Mode = ImportMode.Forge;
            splitButton1.Text = "Forge";
        }

        private void uploadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Mode = ImportMode.Upload;
            splitButton1.Text = "Upload";
        }

        private void splitButton1_Click(object sender, EventArgs e)
        {
            if (Mode == ImportMode.Forge)
            {
                if(Options.InvItem == null)
                {
                    if (Proxy.Agent.Selection.Length == 1)
                    {
                        uint local_id = Proxy.Agent.Selection[0];
                        Options.SeedPrim = Proxy.Network.CurrentSim.ObjectsPrimitives.Find(x => x.LocalID == local_id);
                    }
                    else
                    {
                        MessageBox.Show("You need to be selecting exactly one prim to forge an object!\n(Or select an object via the inventory)");
                        return;
                    }
                }
            }

            ImportOptions importOptions = new ImportOptions();

            importOptions.KeepPositions = checkBox1.Checked;
            importOptions.SeedPrim = Options.SeedPrim;
            importOptions.InvItem = Options.InvItem;

            if(checkBox2.Checked)
            {
                importOptions.Archive = Options.Archive;
                importOptions.Contents = Options.Contents;
            }

            foreach(DataGridViewRow row in dataGridView.Rows)
            {
                if((bool)row.Cells[0].Value)
                {
                    int type = (int)row.Cells[3].Value;
                    UUID id = (UUID)row.Cells[5].Value;
                    object obj = importMap[id];
                    if(type == 0)
                    {
                        ImportableWearable wearable = (ImportableWearable)obj;
                        importOptions.Wearables.Add(wearable);
                    }
                    else
                    {
                        Linkset linkset = (Linkset)obj;
                        importOptions.Linksets.Add(linkset);
                    }
                }
            }

            if (Mode == ImportMode.Forge || Mode == ImportMode.Import)
                ThePlugin.ImportLinkset(importOptions);
            else
                ThePlugin.ForgeLinkset(importOptions);

            this.Close();
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
    }
}
