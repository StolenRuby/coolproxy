using GridProxy;
using Nwc.XmlRpc;
using OpenMetaverse;
using OpenMetaverse.Packets;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static GridProxy.RegionManager;

namespace CoolProxy
{
    public partial class RegionPickerForm : Form
    {
        private CoolProxyFrame Proxy;

        byte[] CurrentSearch;

        public ushort RegionX { get; private set; }
        public ushort RegionY { get; private set; }
        public string RegionName { get; private set; }
        public byte RegionAccess { get; private set; }
        public ulong RegionHandle { get; private set; } = 0;

        //public UUID RegionUUID { get; private set; } = UUID.Zero;
        //public string ImageURL { get; private set; } = string.Empty;

        public RegionPickerForm()
        {
            InitializeComponent();
            Proxy = Program.Frame;

            this.TopMost = Program.Frame.Settings.getBool("KeepCoolProxyOnTop");
            Program.Frame.Settings.getSetting("KeepCoolProxyOnTop").OnChanged += (x, y) => { this.TopMost = (bool)y.Value; };

            dataGridView1.DoubleBuffered(true);

            Proxy.Network.AddDelegate(PacketType.MapBlockReply, Direction.Incoming, OnMapBlockReply);
        }

        private Packet OnMapBlockReply(Packet packet, RegionProxy sim)
        {
            MapBlockReplyPacket reply = (MapBlockReplyPacket)packet;

            this.BeginInvoke(new Action(() =>
            {
                int count = reply.Data.Length;
                if (count > 0)
                {
                    if(reply.Data[count - 1].Name.SequenceEqual(CurrentSearch))
                    {
                        for(int i = 0; i < count - 1; i++)
                        {
                            var block = reply.Data[i];

                            Image icon;

                            switch(block.Access)
                            {
                                // Missing enum SimAccess
                                case 13:
                                    icon = Properties.Resources.Parcel_PG_Light;
                                    break;
                                case 21:
                                    icon = Properties.Resources.Parcel_M_Light;
                                    break;
                                case 42:
                                default:
                                    icon = Properties.Resources.Parcel_R_Light;
                                    break;
                            }

                            dataGridView1.Rows.Add(icon, Utils.BytesToString(block.Name), block.X, block.Y);
                        }
                    }
                }
            }));

            return packet;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Search(textBox1.Text);
        }

        private void Search(string search)
        {
            dataGridView1.Rows.Clear();
            button2.Enabled = false;

            MapNameRequestPacket request = new MapNameRequestPacket();
            request.AgentData = new MapNameRequestPacket.AgentDataBlock();
            request.AgentData.AgentID = Proxy.Agent.AgentID;
            request.AgentData.SessionID = Proxy.Agent.SessionID;
            request.AgentData.Godlike = false;
            request.AgentData.Flags = 2;
            request.AgentData.EstateID = 0; // Proxy.Regions.CurrentRegion.EstateID;

            CurrentSearch = Utils.StringToBytes(search);

            request.NameData = new MapNameRequestPacket.NameDataBlock();
            request.NameData.Name = CurrentSearch;

            Proxy.Network.CurrentSim.Inject(request, Direction.Outgoing);
        }

        private void RegionPickerForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Proxy.Network.RemoveDelegate(PacketType.MapBlockReply, Direction.Incoming, OnMapBlockReply);
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
                Search(textBox1.Text);
        }

        private void dataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            if(dataGridView1.SelectedRows.Count > 0)
            {
                var row = dataGridView1.SelectedRows[0];
                RegionName = (string)row.Cells[1].Value;
                RegionX = (ushort)row.Cells[2].Value;
                RegionY = (ushort)row.Cells[3].Value;

                RegionHandle = Util.RegionGridLocToHandle(RegionX, RegionY);

                button2.Enabled = true;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            button2.Enabled = false;
            button2.Text = "Fetching...";

            new Task(() =>
            {
                UUID uuid;
                string image;
                //Proxy.OpenSim.Hypergrid.LinkRegion(RegionName, out uuid, out image);
                //RegionUUID = uuid;
                //ImageURL = image;


                this.DialogResult = DialogResult.OK;
                this.Close();
            }).Start();
        }
    }
}
