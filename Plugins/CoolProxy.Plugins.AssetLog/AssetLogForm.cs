using GridProxy;
using OpenMetaverse;
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

namespace CoolProxy.Plugins.AssetLog
{
    public partial class AssetLogForm : Form
    {
        private CoolProxyFrame Proxy;

        private Dictionary<UUID, DateTime> AssetBlacklist = new Dictionary<UUID, DateTime>();


        public AssetLogForm(CoolProxyFrame frame)
        {
            InitializeComponent();
            Proxy = frame;


            // Asset Logging
            soundsDataGridView.DoubleBuffered(true);
            animsDataGridView.DoubleBuffered(true);

            Proxy.Network.AddDelegate(PacketType.SoundTrigger, Direction.Incoming, onSoundTrigger);
            Proxy.Network.AddDelegate(PacketType.AttachedSound, Direction.Incoming, onAttachedSound);
            Proxy.Network.AddDelegate(PacketType.PreloadSound, Direction.Incoming, onPreloadSound);

            Proxy.Objects.ObjectDataBlockUpdate += Objects_ObjectDataBlockUpdate;

            Proxy.Avatars.AvatarAnimation += Avatars_AvatarAnimation;


            LoadBlacklist();
        }


        private void LoadBlacklist()
        {
            string app_settings_dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "CoolProxy\\");
            string filename = Path.Combine(app_settings_dir, "blacklist.xml");

            if (File.Exists(filename))
            {
                byte[] xml = File.ReadAllBytes(filename);
                OSD osd = OSDParser.DeserializeLLSDXml(xml);
                OSDMap map = (OSDMap)osd;

                foreach (KeyValuePair<string, OSD> pair in map)
                {
                    UUID asset_id = UUID.Parse(pair.Key);
                    DateTime date = ((OSDDate)pair.Value).AsDate();
                    blacklistDataGridView.Rows.Add(asset_id, date.ToString());
                    AssetBlacklist.Add(asset_id, date);
                }
            }

            //AssetBlacklist.Add(UUID.Parse("89556747-24cb-43ed-920b-47caed15465f"), DateTime.Now);
        }

        private void SaveBlacklist()
        {
            OSDMap blacklist = new OSDMap();

            foreach (var pair in AssetBlacklist)
            {
                blacklist.Add(pair.Key.ToString(), pair.Value);
            }


            string app_settings_dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "CoolProxy\\");
            string filename = Path.Combine(app_settings_dir, "blacklist.xml");

            byte[] xml = OSDParser.SerializeLLSDXmlToBytes((OSD)blacklist);
            File.WriteAllBytes(filename, xml);
        }

        private void BlacklistAsset(UUID asset_id)
        {
            if (!AssetBlacklist.ContainsKey(asset_id))
            {
                var now = DateTime.Now;
                AssetBlacklist.Add(asset_id, now);
                blacklistDataGridView.Rows.Add(asset_id, now.ToString());

                SaveBlacklist();
            }
        }




        private void Avatars_AvatarAnimation(object sender, AvatarAnimationEventArgs e)
        {
            UUID[] anims = e.Animations.Select(x => x.AnimationID).ToArray();
            LogAnimation(anims, e.AvatarID);
        }

        List<UUID> default_animations = Animations.ToDictionary().Keys.ToList();

        Dictionary<UUID, Tuple<int, Dictionary<UUID, DateTime>>> AvatarAnimations = new Dictionary<UUID, Tuple<int, Dictionary<UUID, DateTime>>>();

        string quick_format_name(AgentDisplayName name)
        {
            if (name.IsDefaultDisplayName)
            {
                if (name.LegacyLastName.ToLower() == "resident")
                {
                    return name.LegacyFirstName;
                }
                else
                {
                    return name.LegacyFirstName + " " + name.LegacyLastName;
                }
            }
            else
            {
                return name.DisplayName + " (" + name.UserName + ")";
            }
        }

        void LogAnimation(UUID[] anims, UUID avatar)
        {
            if (animsDataGridView.InvokeRequired) animsDataGridView.BeginInvoke(new Action(() => LogAnimation(anims, avatar)));
            else
            {
                Dictionary<UUID, DateTime> logged_anims = null;

                if (!AvatarAnimations.TryGetValue(avatar, out var tuple))
                {
                    int index = animsDataGridView.Rows.Add(avatar.ToString(), avatar.ToString(), anims.Length, 0);
                    logged_anims = new Dictionary<UUID, DateTime>();
                    tuple = new Tuple<int, Dictionary<UUID, DateTime>>(index, logged_anims);
                    AvatarAnimations[avatar] = tuple;

                    Proxy.Avatars.GetDisplayNames(new List<UUID>() { avatar }, (success, names, z) =>
                    {
                        if (success)
                        {
                            foreach (var name in names)
                            {
                                if (name.ID == avatar)
                                {
                                    UpdateLogName(name, index);
                                    break;
                                }
                            }
                        }
                    });
                }
                else
                {
                    logged_anims = tuple.Item2;
                }

                lock (logged_anims)
                {
                    foreach (UUID anim in anims)
                    {
                        if (!default_animations.Contains(anim))
                        {
                            logged_anims[anim] = DateTime.UtcNow;
                        }
                    }
                }

                animsDataGridView.Rows[tuple.Item1].Cells[2].Value = anims.Length.ToString();
                animsDataGridView.Rows[tuple.Item1].Cells[3].Value = logged_anims.Count.ToString();
            }
        }

        private void UpdateLogName(AgentDisplayName name, int index)
        {
            if (animsDataGridView.InvokeRequired) animsDataGridView.BeginInvoke(new Action(() => UpdateLogName(name, index)));
            else
            {
                animsDataGridView.Rows[index].Tag = name;
                animsDataGridView.Rows[index].Cells[1].Value = quick_format_name(name);
            }
        }

        List<UUID> loggedSounds = new List<UUID>();

        Font boldFont = null;
        Font BoldFont
        {
            get
            {
                if (boldFont == null)
                    boldFont = new Font(soundsDataGridView.DefaultCellStyle.Font, FontStyle.Bold);
                return boldFont;
            }
        }

        private void LogSound(string type, UUID id, UUID owner, bool blacklisted)
        {
            if (soundsDataGridView.InvokeRequired) soundsDataGridView.BeginInvoke(new Action(() => LogSound(type, id, owner, blacklisted)));
            else
            {
                lock (loggedSounds)
                {
                    if (!loggedSounds.Contains(id))
                    {
                        int row = soundsDataGridView.Rows.Add(type, id.ToString(), DateTime.Now.ToString());
                        if (blacklisted)
                            soundsDataGridView.Rows[row].Cells[1].Style.Font = BoldFont;
                        loggedSounds.Add(id);
                    }
                }
            }
        }

        private Packet onSoundTrigger(Packet packet, RegionManager.RegionProxy sim)
        {
            SoundTriggerPacket soundTriggerPacket = (SoundTriggerPacket)packet;
            bool blacklisted = AssetBlacklist.ContainsKey(soundTriggerPacket.SoundData.SoundID);
            LogSound("Trigger", soundTriggerPacket.SoundData.SoundID, soundTriggerPacket.SoundData.OwnerID, blacklisted);
            return blacklisted ? null : packet;
        }

        private Packet onAttachedSound(Packet packet, RegionManager.RegionProxy sim)
        {
            AttachedSoundPacket attachedSoundPacket = (AttachedSoundPacket)packet;
            bool blacklisted = AssetBlacklist.ContainsKey(attachedSoundPacket.DataBlock.SoundID);
            LogSound("Attached", attachedSoundPacket.DataBlock.SoundID, attachedSoundPacket.DataBlock.OwnerID, blacklisted);
            return blacklisted ? null : packet;
        }

        private Packet onPreloadSound(Packet packet, RegionManager.RegionProxy sim)
        {
            PreloadSoundPacket preloadSoundPacket = (PreloadSoundPacket)packet;

            foreach (var block in preloadSoundPacket.DataBlock)
            {
                bool blacklisted = AssetBlacklist.ContainsKey(block.SoundID);
                LogSound("Preload", block.SoundID, block.OwnerID, blacklisted);

                if (blacklisted)
                {
                    block.SoundID = UUID.Zero;
                }
            }

            return preloadSoundPacket;
        }

        private void Objects_ObjectDataBlockUpdate(object sender, ObjectDataBlockUpdateEventArgs e)
        {
            bool blacklisted_sound = AssetBlacklist.ContainsKey(e.Block.Sound);
            LogSound("Loop", e.Block.Sound, e.Prim.OwnerID, blacklisted_sound);

            if (blacklisted_sound)
            {
                e.Block.Sound = UUID.Zero;
            }

            bool blacklisted_textures = false;

            // todo: materials etc
            if (AssetBlacklist.ContainsKey(e.Update.Textures.DefaultTexture?.TextureID ?? UUID.Zero))
            {
                e.Update.Textures.DefaultTexture.TextureID = UUID.Zero;
                blacklisted_textures = true;
            }

            foreach (var face in e.Update.Textures.FaceTextures)
            {
                if (face == null) continue;
                if (AssetBlacklist.ContainsKey(face.TextureID))
                {
                    face.TextureID = UUID.Zero;
                    blacklisted_textures = true;
                }
            }

            if (blacklisted_textures)
            {
                e.Block.TextureEntry = e.Update.Textures.GetBytes();
            }
        }







        private void soundsListContextMenu_Opening(object sender, CancelEventArgs e)
        {
            int count = soundsDataGridView.SelectedRows.Count;
            if (count == 0)
            {
                e.Cancel = true;
                return;
            }

            bool single = count == 1;
            playLocallyToolStripMenuItem.Visible = single;
            playInworldToolStripMenuItem.Visible = single;
            toolStripSeparator9.Visible = single;
        }

        private void playLocallyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UUID sound_id = UUID.Parse((string)soundsDataGridView.SelectedRows[0].Cells[1].Value);
            TriggerSound(sound_id, true);
        }

        private void playInworldToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UUID sound_id = UUID.Parse((string)soundsDataGridView.SelectedRows[0].Cells[1].Value);
            TriggerSound(sound_id, false);
        }

        void TriggerSound(UUID sound_id, bool local)
        {
            SoundTriggerPacket packet = new SoundTriggerPacket();
            packet.SoundData.SoundID = sound_id;
            packet.SoundData.Position = Proxy.Agent.SimPosition;
            packet.SoundData.Handle = Proxy.Network.CurrentSim.Handle;
            packet.SoundData.Gain = 1.0f;

            Proxy.Network.CurrentSim.Inject(packet, local ? GridProxy.Direction.Incoming : GridProxy.Direction.Outgoing);
        }

        private void soundsDataGridView_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            int count = soundsDataGridView.SelectedRows.Count;
            if (count == 1)
            {
                UUID sound_id = UUID.Parse((string)soundsDataGridView.SelectedRows[0].Cells[1].Value);
                TriggerSound(sound_id, true);
            }
        }

        private void blacklistToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in soundsDataGridView.SelectedRows)
            {
                UUID asset_id = UUID.Parse((string)row.Cells[1].Value);
                BlacklistAsset(asset_id);
            }
        }


        private void animsDataGridView_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            var row = animsDataGridView.Rows[e.RowIndex];
            UUID agent_id = UUID.Parse((string)row.Cells[0].Value);
            AgentDisplayName name = (AgentDisplayName)row.Tag;
            var form = new AvatarAnimationsForm(Proxy, name, AvatarAnimations[agent_id].Item2);

            form.TopMost = Proxy.Settings.getBool("KeepCoolProxyOnTop");
            Proxy.Settings.getSetting("KeepCoolProxyOnTop").OnChanged += (x, y) => { form.TopMost = (bool)y.Value; };

            form.Show();
        }
    }
}
