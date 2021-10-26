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
        CoolProxyFrame Frame = null;

        Dictionary<int, Linkset> indexToLinkset = new Dictionary<int, Linkset>();

        CopyBotPlugin ThePlugin;

        bool ViaCopybot = false;

        public ImportForm(CoolProxyFrame frame, string filename, CopyBotPlugin plugin)
        {
            this.Frame = frame;
            InitializeComponent();

            if(!decodeXML(filename, out string error))
            {
                this.Load += (x, y) => Close();
                Frame.AlertMessage(error, false);
                return;
            }

            ThePlugin = plugin;

            Focus();
        }

        bool decodeXML(string filename, out string error)
        {
            string xml;
            List<Primitive> prims;

            try { xml = File.ReadAllText(filename); }
            catch (Exception e)
            {
                error = e.Message;
                return false;
            }

            try { prims = Helpers.OSDToPrimList(OSDParser.DeserializeLLSDXml(xml)); }
            catch (Exception e)
            {
                error = "Failed to deserialize " + filename + ": " + e.Message;
                return false;
            }

            List<Linkset> linksets = CopyBotPlugin.PrimListToLinksetList(prims);

            foreach(var linkset in linksets)
            {
                int index = checkedListBox1.Items.Add(linkset.RootPrim.Properties?.Name ?? "Object", true);
                indexToLinkset.Add(index, linkset);
            }

            error = string.Empty;
            return true;
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
            if (ViaCopybot)
                ThePlugin.ImportLinkset(indexToLinkset.Values.ToList());
            else
                ThePlugin.ForgeLinkset(indexToLinkset.Values.ToList());

            this.Close();
        }
    }
}
