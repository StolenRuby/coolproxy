using GridProxy;
using OpenMetaverse;
using OpenMetaverse.Packets;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoolProxy.Plugins.FancyBeams
{
    public class FancyBeamsPlugin : CoolProxyPlugin
    {
        CoolProxyFrame Proxy;

        public FancyBeamsPlugin(SettingsManager settings, GUIManager gui, CoolProxyFrame frame)
        {
            Proxy = frame;
            frame.Network.AddDelegate(PacketType.ViewerEffect, Direction.Outgoing, HandleViewerEffect);

            settings.getSetting("RainbowSelectionBeam").OnChanged += (x, y) => EnableRainbowBeam = (bool)y.Value;
            EnableRainbowBeam = settings.getBool("RainbowSelectionBeam");

            settings.getSetting("QuadSelectionBeam").OnChanged += (x, y) => EnableQuadBeam = (bool)y.Value;
            EnableQuadBeam = settings.getBool("QuadSelectionBeam");
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

        private Packet HandleViewerEffect(Packet packet, RegionManager.RegionProxy sim)
        {
            ViewerEffectPacket ve = (ViewerEffectPacket)packet;
            foreach (ViewerEffectPacket.EffectBlock effect in ve.Effect)
            {
                if (effect.Type == (byte)EffectType.Beam)
                {
                    if(EnableRainbowBeam)
                    {
                        RainbowProgress += 0.025f;
                        if(RainbowProgress > 3)
                        {
                            RainbowProgress = 0.0f;
                        }

                        Color next_color = Rainbow(RainbowProgress);
                        byte[] color = new byte[4] { next_color.R, next_color.G, next_color.B, next_color.A };
                        Buffer.BlockCopy(color, 0, effect.Color, 0, 4);
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
