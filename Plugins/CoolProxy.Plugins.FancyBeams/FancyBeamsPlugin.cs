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
        CoolProxyFrame Proxy;

        Timer beamTimer;

        public FancyBeamsPlugin(SettingsManager settings, GUIManager gui, CoolProxyFrame frame)
        {
            Proxy = frame;
            frame.Network.AddDelegate(PacketType.ViewerEffect, Direction.Outgoing, HandleViewerEffect);

            settings.getSetting("RainbowSelectionBeam").OnChanged += (x, y) => EnableRainbowBeam = (bool)y.Value;
            EnableRainbowBeam = settings.getBool("RainbowSelectionBeam");

            settings.getSetting("QuadSelectionBeam").OnChanged += (x, y) => EnableQuadBeam = (bool)y.Value;
            EnableQuadBeam = settings.getBool("QuadSelectionBeam");

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

        bool EnableQuadBeam = true;
        bool EnableRainbowBeam = true;
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

                    if (EnableQuadBeam)
                    {
                        Vector3d position = new Vector3d(effect.TypeData, 32);

                        for (int x = 0; x < 2; x++)
                        {
                            for (int y = 0; y < 2; y++)
                            {
                                for (int z = 0; z < 2; z++)
                                {
                                    ViewerEffectPacket beam = new ViewerEffectPacket();
                                    beam.AgentData.AgentID = Proxy.Agent.AgentID;
                                    beam.AgentData.SessionID = Proxy.Agent.SessionID;

                                    beam.Effect = new ViewerEffectPacket.EffectBlock[1];

                                    beam.Effect[0] = new ViewerEffectPacket.EffectBlock();
                                    beam.Effect[0].ID = UUID.Random();
                                    beam.Effect[0].AgentID = effect.AgentID;
                                    beam.Effect[0].Type = effect.Type;
                                    beam.Effect[0].Duration = effect.Duration;
                                    beam.Effect[0].Color = effect.Color;
                                    beam.Effect[0].TypeData = effect.TypeData.ToArray(); // ToArray to clone it...

                                    double ox = ((double)x - 0.5f);
                                    double oy = ((double)y - 0.5f);
                                    double oz = ((double)z - 0.5f);
                                    Vector3d pos = new Vector3d(ox + position.X, oy + position.Y, oz + position.Z);
                                    Buffer.BlockCopy(pos.GetBytes(), 0, beam.Effect[0].TypeData, 32, 24);

                                    Proxy.Network.InjectPacket(beam, Direction.Outgoing);
                                }
                            }
                        }
                    }

                    return ve;
                }
            }

            return packet;
        }
    }
}
