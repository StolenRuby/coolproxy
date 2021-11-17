using GridProxy;
using OpenMetaverse;
using OpenMetaverse.Packets;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CoolProxy.Plugins.FancyBeams
{
    public class FancyBeamsPlugin : CoolProxyPlugin
    {
        List<Vector3> BeamOffsets = new List<Vector3>()
        {
            new Vector3(0.0f, -0.5f,    0.5f),
            new Vector3(0.0f, -0.25f,   0.5f),
            new Vector3(0.0f,  0.0f,    0.5f),
            new Vector3(0.0f,  0.25f,   0.5f),

            new Vector3(0.0f,  0.5f,    0.5f),
            new Vector3(0.0f,  0.5f,   0.25f),
            new Vector3(0.0f,  0.5f,    0.0f),
            new Vector3(0.0f,  0.5f,  -0.25f),

            new Vector3(0.0f,  0.5f,   -0.5f),
            new Vector3(0.0f,  0.25f,  -0.5f),
            new Vector3(0.0f,  0.0f,   -0.5f),
            new Vector3(0.0f, -0.25f,  -0.5f),

            new Vector3(0.0f, -0.5f,   -0.5f),
            new Vector3(0.0f, -0.5f,  -0.25f),
            new Vector3(0.0f, -0.5f,    0.0f),
            new Vector3(0.0f, -0.5f,   0.25f),
        };

        CoolProxyFrame Proxy;

        Timer beamTimer;

        public FancyBeamsPlugin(SettingsManager settings, GUIManager gui, CoolProxyFrame frame)
        {
            Proxy = frame;
            frame.Network.AddDelegate(PacketType.ViewerEffect, Direction.Outgoing, HandleViewerEffect);

            settings.getSetting("RainbowSelectionBeam").OnChanged += (x, y) => EnableRainbowBeam = (bool)y.Value;
            EnableRainbowBeam = settings.getBool("RainbowSelectionBeam");

            settings.getSetting("ShapedSelectionBeam").OnChanged += (x, y) => EnableShapedBeam = (bool)y.Value;
            EnableShapedBeam = settings.getBool("ShapedSelectionBeam");

            settings.getSetting("RotateShapedBeam").OnChanged += (x, y) => RotateBeamShape = (bool)y.Value;
            RotateBeamShape = settings.getBool("RotateShapedBeam");

            beamTimer = new Timer();
            beamTimer.Tick += BeamTick;

            gui.AddToolCheckbox("Avatar", "Rainbow Beam Trail", toggleBeamTrail);
        }

        private void BeamTick(object sender, EventArgs e)
        {
            ViewerEffectPacket ve = new ViewerEffectPacket();
            ve.AgentData.AgentID = Proxy.Agent.AgentID;
            ve.AgentData.SessionID = Proxy.Agent.SessionID;

            ve.Effect = new ViewerEffectPacket.EffectBlock[1];

            byte[] type_data = new byte[56];

            Proxy.Agent.AgentID.ToBytes(type_data, 0);

            var agent_pos = Proxy.Agent.SimPosition;
            Util.RegionHandleToWorldLoc(Proxy.Network.CurrentSim.Handle, out uint x, out uint y);

            Vector3d pos = new Vector3d(agent_pos.X + x, agent_pos.Y + y, agent_pos.Z);
            pos.ToBytes(type_data, 32);

            ve.Effect[0] = new ViewerEffectPacket.EffectBlock();
            ve.Effect[0].AgentID = Proxy.Agent.AgentID;
            ve.Effect[0].Color = GetNextColor();
            ve.Effect[0].Duration = 3.0f;
            ve.Effect[0].ID = UUID.Random();
            ve.Effect[0].Type = (byte)EffectType.Beam;
            ve.Effect[0].TypeData = type_data;

            Proxy.Network.InjectPacket(ve, Direction.Outgoing);
        }

        private void toggleBeamTrail(object sender, EventArgs e)
        {
            beamTimer.Enabled = (sender as CheckBox).Checked;
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

        bool EnableShapedBeam = true;
        bool EnableRainbowBeam = true;
        bool RotateBeamShape = true;
        float RainbowProgress = 0.0f;

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

                    if (EnableShapedBeam)
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

                        bool is_opensim = true; // todo: grid detection somewhere
                        List<ViewerEffectPacket.EffectBlock> blocks = null;

                        if(is_opensim)
                        {
                            blocks = new List<ViewerEffectPacket.EffectBlock>();
                        }

                        foreach (var offset in BeamOffsets)
                        {
                            var beam = new ViewerEffectPacket.EffectBlock();
                            beam.ID = UUID.Random();
                            beam.AgentID = effect.AgentID;
                            beam.Type = effect.Type;
                            beam.Duration = effect.Duration;
                            beam.Color = effect.Color;
                            beam.TypeData = effect.TypeData.ToArray(); // ToArray to clone it...

                            var rotated_offset = offset;
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
