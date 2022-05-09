using OpenMetaverse;
using OpenMetaverse.Http;
using OpenMetaverse.StructuredData;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CoolProxy
{
    public partial class AvatarPickerSearchForm : Form
    {
        private readonly CoolProxyFrame Proxy;

        int PageSize = 100;

        public UUID SelectedID { get; set; }
        public string SelectedName { get; set; }

        public AvatarPickerSearchForm()
        {
            this.Proxy = CoolProxy.Frame;
            InitializeComponent();

            Vector3 my_pos = Proxy.Agent.SimPosition;

            Proxy.Network.CurrentSim?.ObjectsAvatars.ForEach(av =>
            {
                float dist = Vector3.Distance(av.Position, my_pos);

                string dist_str = av.ID == Proxy.Agent.AgentID ? "n/a" : Math.Round(dist, 2).ToString() + "m";

                dataGridView2.Rows.Add(av.Name, dist_str, av.ID);
            });

            this.TopMost = CoolProxy.Frame.Settings.getBool("KeepCoolProxyOnTop");
            CoolProxy.Frame.Settings.getSetting("KeepCoolProxyOnTop").OnChanged += (x, y) => { this.TopMost = (bool)y.Value; };

            dataGridView2.Sort(dataGridView2.Columns[1], ListSortDirection.Descending);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Search(textBox1.Text);
        }

        void Search(string search)
        {
            if (!Proxy.LoggedIn) return;

            if (string.IsNullOrWhiteSpace(search))
                return;

            button1.Enabled = false;
            button2.Enabled = false;

            string aps_url;
            if (Proxy.Network.CurrentSim.Caps.TryGetValue("AvatarPickerSearch", out aps_url))
            {
                if (!aps_url.EndsWith("/"))
                    aps_url += "/";

                string search_string = WebUtility.UrlEncode(search);

                Uri url = new Uri(string.Format("{0}?page_size={1}&names={2}", aps_url, PageSize, search_string));

                CapsClient capsClient = new CapsClient(url);
                capsClient.OnComplete += CapsClient_OnComplete;
                capsClient.BeginGetResponse(10000);
            }
        }

        private void CapsClient_OnComplete(CapsClient client, OSD result, Exception error)
        {
            this.BeginInvoke(new Action(() =>
            {
                dataGridView1.Rows.Clear();

                OSDMap map = (OSDMap)result;

                OSDArray agents = (OSDArray)map["agents"];
                if (agents.Count > 0)
                {
                    foreach (OSDMap agent in agents)
                    {
                        string username = agent["username"].AsString();
                        string display_name = agent["display_name"].AsString();
                        UUID uuid = agent["id"].AsUUID();

                        dataGridView1.Rows.Add(display_name, username, uuid);
                    }
                }

                button1.Enabled = true;
            }));
        }

        private void dataGridView_SelectionChanged(object sender, EventArgs e)
        {
            DataGridView dgv = sender as DataGridView;
            if (dgv.SelectedRows.Count > 0)
            {
                button2.Enabled = true;
                var row = dgv.SelectedRows[0];
                SelectedID = (UUID)row.Cells[2].Value;
                SelectedName = (string)row.Cells[0].Value;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            this.Close();
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.KeyCode == Keys.Return)
            {
                Search(textBox1.Text);
            }
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            int index = tabControl1.SelectedIndex;
            if(index == 0 || index == 1)
            {
                var dgv = index == 0 ? dataGridView1 : dataGridView2;
                bool has_row = dgv.SelectedRows.Count > 0;
                button2.Enabled = has_row;
                if(has_row)
                {
                    var row = dgv.SelectedRows[0];
                    SelectedID = (UUID)row.Cells[2].Value;
                    SelectedName = (string)row.Cells[0].Value;
                }
            }
        }
    }
}
