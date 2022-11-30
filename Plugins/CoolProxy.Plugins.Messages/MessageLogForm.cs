using GridProxy;
using OpenMetaverse;
using OpenMetaverse.Packets;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CoolProxy.Plugins.Messages
{
    public partial class MessageLogForm : Form
    {
        public class MessageLogEntry
        {
            public UUID ID { get; } = UUID.Random();

            public Packet Packet { get; private set; }
            public RegionManager.RegionProxy Region { get; private set; }

            public string Summary { get; private set; }

            public bool Incoming { get; private set; }

            public MessageLogEntry(Packet packet, RegionManager.RegionProxy region, bool outgoing)
            {
                this.Packet = packet;
                this.Region = region;

                this.Incoming = outgoing;

                string summary = "[ ";

                if (packet.Header.Zerocoded)
                    summary += "Zer ";
                if (packet.Header.Reliable)
                    summary += "Rel ";
                if (packet.Header.Resent)
                    summary += "Rsd ";
                if (packet.Header.AppendedAcks)
                    summary += "Ack ";

                summary += "] ";

                this.Summary = summary;
            }

            public ListViewItem AsRow()
            {
                var row = new ListViewItem(new string[]
                {
                    Packet.Header.Sequence.ToString(),
                    //Incoming ? "↑" : "↓",
                    Incoming ? "from" : "to",
                    Region == null ? "(first)" : string.IsNullOrEmpty(Region.Name) ? Region.RemoteEndPoint.ToString() : Region.Name,
                    Packet.Type.ToString(),
                    Summary
                });

                row.Tag = this;
                return row;
            }
        }

        List<PacketType> PositiveNames = new List<PacketType>();
        List<PacketType> NegativeNames = new List<PacketType>();

        List<MessageLogEntry> Entries = new List<MessageLogEntry>();


        private static string LessSpamFilter = "!StartPingCheck !CompletePingCheck !PacketAck !SimulatorViewerTimeMessage !SimStats !AgentUpdate !AgentAnimation !AvatarAnimation !ViewerEffect !CoarseLocationUpdate !LayerData !CameraConstraint !ObjectUpdateCached !RequestMultipleObjects !ObjectUpdate !ObjectUpdateCompressed !ImprovedTerseObjectUpdate !KillObject !RequestObjectPropertiesFamily !ObjectPropertiesFamily !ImagePacket !SendXferPacket !ConfirmXferPacket !TransferPacket !ObjectAnimation";
        private static string LessSpamNoSoundsFilter = "!StartPingCheck !CompletePingCheck !PacketAck !SimulatorViewerTimeMessage !SimStats !AgentUpdate !AgentAnimation !AvatarAnimation !ViewerEffect !CoarseLocationUpdate !LayerData !CameraConstraint !ObjectUpdateCached !RequestMultipleObjects !ObjectUpdate !ObjectUpdateCompressed !ImprovedTerseObjectUpdate !KillObject !RequestObjectPropertiesFamily !ObjectPropertiesFamily !ImagePacket !SendXferPacket !ConfirmXferPacket !TransferPacket !ObjectAnimation !SoundTrigger !AttachedSound !PreloadSound";
        private static string ObjectUpdatesFilter = "ObjectUpdateCached ObjectUpdate ObjectUpdateCompressed ImprovedTerseObjectUpdate KillObject RequestMultipleObjects";

        CoolProxyFrame Proxy;

        private static MessageLogForm Instance;

        public MessageLogForm(CoolProxyFrame frame)
        {
            Proxy = frame;
            Instance = this;
            InitializeComponent();

            var nf = contextMenuStrip1.Items.Add("No filter");
            nf.Click += noFilterToolStripMenuItem_Click;

            var fsm = contextMenuStrip1.Items.Add("Fewer spammy messages");
            fsm.Tag = LessSpamFilter;
            fsm.Click += noFilterToolStripMenuItem_Click;

            var fsmns = contextMenuStrip1.Items.Add("Fewer spammy messages (no sounds)");
            fsmns.Tag = LessSpamNoSoundsFilter;
            fsmns.Click += noFilterToolStripMenuItem_Click;

            var ou = contextMenuStrip1.Items.Add("Object updates");
            ou.Tag = ObjectUpdatesFilter;
            ou.Click += noFilterToolStripMenuItem_Click;

            textBox1.Text = LessSpamNoSoundsFilter;

            //PacketType.RequestObjectPropertiesFamily
            //PacketType.ObjectPropertiesFamily
            //PacketType.ObjectProperties

            ApplyFilter(textBox1.Text);

            foreach(PacketType type in Enum.GetValues(typeof(PacketType)))
            {
                Proxy.Network.AddDelegate(type, Direction.Outgoing, onPacketOut);
                Proxy.Network.AddDelegate(type, Direction.Incoming, onPacketIn);
            }
        }

        void ApplyFilter(string filter)
        {
            NegativeNames.Clear();
            PositiveNames.Clear();

            string[] list = filter?.Split(' ') ?? new string[0];
            foreach(string p in list)
            {
                bool negative = p.StartsWith("!");
                
                if (Enum.TryParse<PacketType>(negative ? p.Substring(1) : p, true, out var pt))
                {
                    if(negative)
                    {
                        NegativeNames.Add(pt);
                    }
                    else
                    {
                        PositiveNames.Add(pt);
                    }
                }
            }


            listViewSessions.BeginUpdate();
            listViewSessions.Items.Clear();
            foreach (var entry in Entries.ToArray())
            {
                ConditionalLog(entry);
            }
            listViewSessions.EndUpdate();
        }

        private Packet onPacketIn(Packet packet, RegionManager.RegionProxy sim)
        {
            LogPacket(packet, sim, true);
            return packet;
        }

        private Packet onPacketOut(Packet packet, RegionManager.RegionProxy sim)
        {
            LogPacket(packet, sim, false);
            return packet;
        }

        private void LogPacket(Packet packet, RegionManager.RegionProxy sim, bool incoming)
        {
            var item = new MessageLogEntry(packet, sim, incoming);
            Entries.Add(item);

            ConditionalLog(item);
        }

        private void ConditionalLog(MessageLogEntry entry)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action(() => ConditionalLog(entry)));
                return;
            }

            bool log = true;

            if(PositiveNames.Count > 0)
            {
                if(!PositiveNames.Contains(entry.Packet.Type))
                {
                    log = false;
                }
            }
            else
            {
                if(NegativeNames.Contains(entry.Packet.Type))
                {
                    log = false;
                }
            }

            if(log)
            {
                this.listViewSessions.Items.Add(entry.AsRow()).EnsureVisible();
            }

            label1.Text = string.Format("Showing {0} messages from {1}", listViewSessions.Items.Count, Entries.Count);
        }

        public static void LogPacket(Packet packet, RegionManager.RegionProxy region, Direction direction)
        {
            Instance.LogPacket(packet, region, direction == Direction.Incoming);
        }

        private void listViewSessions_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listViewSessions.SelectedItems.Count != 1) return;

            button4.Enabled = true;

            var row = listViewSessions.SelectedItems[0];
            var entry = (MessageLogEntry)row.Tag;
            //textBox2.Text = PlainPacketDecoder.PacketToString(entry.Packet);
            textBox2.Text = (entry.Incoming ? "in " : "out ") + entry.Packet.Type.ToString() + Environment.NewLine + PlainPacketDecoder.PacketToString(entry.Packet);
        }

        private void noFilterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var item = sender as ToolStripMenuItem;
            string tag = item.Tag as string;

            textBox1.Text = tag;
            ApplyFilter(tag);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            ApplyFilter(textBox1.Text);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Entries.Clear();
            listViewSessions.Items.Clear();
            textBox2.Text = string.Empty;
            button4.Enabled = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            contextMenuStrip1.Show(Cursor.Position);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            var row = listViewSessions.SelectedItems[0];
            var entry = (MessageLogEntry)row.Tag;
            MessageBuilderForm.Open(entry.Packet, entry.Incoming ? Direction.Incoming : Direction.Outgoing);
        }
    }
}
