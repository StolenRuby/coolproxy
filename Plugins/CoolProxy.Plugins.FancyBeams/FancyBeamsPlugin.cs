using GridProxy;
using OpenMetaverse;
using OpenMetaverse.Packets;
using OpenMetaverse.StructuredData;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CoolProxy.Plugins.FancyBeams
{
    public class FancyBeamsPlugin : CoolProxyPlugin
    {
        List<Color4> Colours = new List<Color4>();
        List<Vector3> Offsets = new List<Vector3>();
        List<UUID> BeamIDs = new List<UUID>();

        float BeamScale = 1.0f;

        bool EnableRainbowBeam = true;
        bool RotateBeamShape = true;
        float RainbowProgress = 0.0f;

        CoolProxyFrame Proxy;

        public static string BeamsFolderDir { get; private set; }

        internal static SettingsManager Settings { get; set; }

        internal static BeamSettingsPanel BeamSettingsPanel { get; set; }

        public FancyBeamsPlugin(SettingsManager settings, GUIManager gui, CoolProxyFrame frame)
        {
            Proxy = frame;
            Settings = settings;

            frame.Network.AddDelegate(PacketType.ViewerEffect, Direction.Outgoing, HandleViewerEffect);

            settings.getSetting("RainbowSelectionBeam").OnChanged += (x, y) => EnableRainbowBeam = (bool)y.Value;
            EnableRainbowBeam = settings.getBool("RainbowSelectionBeam");

            settings.getSetting("RotateShapedBeam").OnChanged += (x, y) => RotateBeamShape = (bool)y.Value;
            RotateBeamShape = settings.getBool("RotateShapedBeam");

            settings.getSetting("BeamScale").OnChanged += (x, y) => BeamScale = (float)(double)y.Value;

            BeamsFolderDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "CoolProxy\\beams\\");

            if(!Directory.Exists(BeamsFolderDir))
            {
                Directory.CreateDirectory(BeamsFolderDir);

                if(Directory.Exists(".\\app_data\\beams\\"))
                {
                    string[] default_beams = Directory.GetFiles(".\\app_data\\beams\\", "*.xml");
                    foreach(string file in default_beams)
                    {
                        string filename = Path.GetFileName(file);
                        File.Copy(file, BeamsFolderDir + filename);
                    }
                }
            }

            settings.getSetting("BeamShape").OnChanged += OnBeamChanged;
            settings.setString("BeamShape", settings.getString("BeamShape")); // hack: re-apply the settings to trigger the load

            BeamSettingsPanel = new BeamSettingsPanel(settings);
            gui.AddSettingsTab("Tractor Beam", BeamSettingsPanel);
        }

        private void OnBeamChanged(object source, SettingChangedEventArgs e)
        {
            string val = (string)e.Value;
            LoadBeam(val == string.Empty ? val : BeamsFolderDir + val + ".xml");
        }

        void LoadBeam(string filename)
        {
            Offsets.Clear();
            Colours.Clear();
            BeamIDs.Clear();

            if (filename == string.Empty)
                return;

            if(!File.Exists(filename))
            {
                Settings.setString("BeamShape", string.Empty);
                return;
            }

            byte[] data = File.ReadAllBytes(filename);
            OSD osd = OSDParser.DeserializeLLSDXml(data);
            OSDArray array = (OSDArray)osd;
            foreach (var entry in array)
            {
                OSDMap map = (OSDMap)entry;

                Color4 colour = map["colour"].AsColor4();
                Vector3 offset = map["offset"].AsVector3();

                offset.Y *= -1.0f;
                offset.Z *= -1.0f;

                Colours.Add(colour);
                Offsets.Add(offset);
                BeamIDs.Add(UUID.Random());
            }
        }

        // https://stackoverflow.com/questions/2288498/how-do-i-get-a-rainbow-color-gradient-in-c
        public static Color Rainbow(float progress)
        {
            float div = (Math.Abs(progress % 1) * 6);
            int ascending = (int)((div % 1) * 255);
            int descending = 255 - ascending;

            switch ((int)div)
            {
                case 0:
                    return Color.FromArgb(255, 255, ascending, 0);
                case 1:
                    return Color.FromArgb(255, descending, 255, 0);
                case 2:
                    return Color.FromArgb(255, 0, 255, ascending);
                case 3:
                    return Color.FromArgb(255, 0, descending, 255);
                case 4:
                    return Color.FromArgb(255, ascending, 0, 255);
                default: // case 5:
                    return Color.FromArgb(255, 255, 0, descending);
            }
        }

        byte[] GetNextColor()
        {
            RainbowProgress += 0.025f;
            if (RainbowProgress > 3)
            {
                RainbowProgress = 0.0f;
            }

            Color next_color = Rainbow(RainbowProgress);
            return new byte[4] { next_color.R, next_color.G, next_color.B, next_color.A };
        }

        private Packet HandleViewerEffect(Packet packet, RegionManager.RegionProxy sim)
        {
            ViewerEffectPacket ve = (ViewerEffectPacket)packet;
            foreach (ViewerEffectPacket.EffectBlock effect in ve.Effect)
            {
                if (effect.Type == (byte)EffectType.Beam)
                {
                    if(EnableRainbowBeam)
                    {
                        Buffer.BlockCopy(GetNextColor(), 0, effect.Color, 0, 4);
                    }

                    if (Offsets.Count > 0)
                    {
                        Vector3d position = new Vector3d(effect.TypeData, 32);

                        var agent_pos = Proxy.Agent.SimPosition;

                        Util.RegionHandleToWorldLoc(Proxy.Network.CurrentSim.Handle, out uint x, out uint y);

                        Vector3 local_pos = new Vector3((float)position.X - x, (float)position.Y - y, (float)position.Z);

                        Vector3 beam_line = local_pos - agent_pos;
                        Vector3 beam_line_flat = new Vector3(beam_line.X, beam_line.Y, 0.0f);

                        beam_line.Normalize();
                        beam_line_flat.Normalize();

                        Vector3 turn = Vector3.UnitX;

                        Quaternion yaw = Vector3.RotationBetween(turn, beam_line_flat);
                        turn *= yaw;
                        Quaternion pitch = Vector3.RotationBetween(turn, beam_line);

                        int ticks = Environment.TickCount;
                        int mod = ticks % 20000;
                        double r = mod / 20000.0d;

                        double DEG_TO_RAD = Math.PI / 180.0d;

                        Quaternion roll = RotateBeamShape ? Quaternion.CreateFromEulers(new Vector3((float)(360.0d * r * DEG_TO_RAD), 0.0f, 0.0f)) : Quaternion.Identity;

                        float distance_multiplier = Vector3.Distance(local_pos, agent_pos) * 0.1f;

                        bool is_opensim = !Proxy.IsLindenGrid;
                        List<ViewerEffectPacket.EffectBlock> blocks = null;

                        if(is_opensim)
                        {
                            blocks = new List<ViewerEffectPacket.EffectBlock>();
                        }

                        for(int i = 0; i < Offsets.Count; i++)
                        {
                            var beam = new ViewerEffectPacket.EffectBlock();
                            beam.ID = BeamIDs[i];
                            beam.AgentID = effect.AgentID;
                            beam.Type = effect.Type;
                            beam.Duration = effect.Duration;
                            beam.Color = EnableRainbowBeam ? effect.Color : Colours[i].GetBytes();
                            beam.TypeData = effect.TypeData.ToArray(); // ToArray to clone it...

                            var rotated_offset = Offsets[i];
                            rotated_offset *= distance_multiplier * BeamScale;
                            rotated_offset *= roll;
                            rotated_offset *= yaw;
                            rotated_offset *= pitch;

                            Vector3d pos = new Vector3d(rotated_offset.X + position.X, rotated_offset.Y + position.Y, rotated_offset.Z + position.Z);
                            Buffer.BlockCopy(pos.GetBytes(), 0, beam.TypeData, 32, 24);

                            if(is_opensim)
                            {
                                blocks.Add(beam);
                            }
                            else
                            {
                                ViewerEffectPacket viewerEffect = new ViewerEffectPacket();
                                viewerEffect.AgentData.AgentID = Proxy.Agent.AgentID;
                                viewerEffect.AgentData.SessionID = Proxy.Agent.SessionID;
                                viewerEffect.Effect = new ViewerEffectPacket.EffectBlock[1];
                                viewerEffect.Effect[0] = beam;
                                Proxy.Network.InjectPacket(viewerEffect, Direction.Outgoing);

                            }
                        }

                        if(is_opensim)
                        {
                            ViewerEffectPacket viewerEffect = new ViewerEffectPacket();
                            viewerEffect.AgentData.AgentID = Proxy.Agent.AgentID;
                            viewerEffect.AgentData.SessionID = Proxy.Agent.SessionID;
                            viewerEffect.Effect = blocks.ToArray();
                            Proxy.Network.InjectPacket(viewerEffect, Direction.Outgoing);
                        }

                        return null;
                    }

                    return ve;
                }
            }

            return packet;
        }
    }
}
