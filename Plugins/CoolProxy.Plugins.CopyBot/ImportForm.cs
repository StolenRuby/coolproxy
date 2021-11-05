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
    public partial class ImportForm : Form
    {
        CopyBotPlugin ThePlugin;

        bool ViaCopybot = true;

        ImportOptions Options;

        public ImportForm(CoolProxyFrame frame, ImportOptions options, CopyBotPlugin plugin)
        {
            Options = options;

            InitializeComponent();

            ThePlugin = plugin;
            Focus();
        }

        Dictionary<UUID, object> importMap = new Dictionary<UUID, object>();

        private void PostBuild(object sender, EventArgs e)
        {
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
            this.Close();
        }

        private void importToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ViaCopybot = true;
            splitButton1.Text = "Import";
        }

        private void forgeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ViaCopybot = false;
            splitButton1.Text = "Forge";
        }

        private void splitButton1_Click(object sender, EventArgs e)
        {
            ImportOptions importOptions = new ImportOptions();

            importOptions.KeepPositions = checkBox1.Checked;
            importOptions.SeedPrim = Options.SeedPrim;
            importOptions.InvItem = Options.InvItem;

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

            if (ViaCopybot)
                ThePlugin.ImportLinkset(importOptions);
            else
                ThePlugin.ForgeLinkset(importOptions);

            this.Close();
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
