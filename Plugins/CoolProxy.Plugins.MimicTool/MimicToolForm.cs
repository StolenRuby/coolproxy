using OpenMetaverse;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CoolProxy.Plugins.MimicTool
{
    public partial class MimicToolForm : Form
    {
        private const float DISTANCE_BUFFER = 3.0f;

        private CoolProxyFrame Proxy;

        private UUID TargetAvatar = UUID.Zero;
        private uint TargetLocalID = 0;
        private bool MimicAnims = false;

        private Timer FollowTimer;

        public MimicToolForm(CoolProxyFrame frame)
        {
            Proxy = frame;
            InitializeComponent();

            Proxy.Avatars.AvatarAnimation += Avatars_AvatarAnimation;

            FollowTimer = new Timer();
            FollowTimer.Interval = 500;
            FollowTimer.Tick += FollowTimer_Tick;
        }

        private void FollowTimer_Tick(object sender, EventArgs e)
        {
            // Find the target position
            lock (Proxy.Network.Regions)
            {
                for (int i = 0; i < Proxy.Network.Regions.Count; i++)
                {
                    Avatar targetAv;

                    if (Proxy.Network.Regions[i].ObjectsAvatars.TryGetValue(TargetLocalID, out targetAv))
                    {
                        float distance = 0.0f;

                        if (Proxy.Network.Regions[i] == Proxy.Network.CurrentSim)
                        {
                            distance = Vector3.Distance(targetAv.Position, Proxy.Agent.SimPosition);
                        }
                        else
                        {
                            // FIXME: Calculate global distances
                        }

                        if (distance > DISTANCE_BUFFER)
                        {
                            uint regionX, regionY;
                            Utils.LongToUInts(Proxy.Network.Regions[i].Handle, out regionX, out regionY);

                            double xTarget = (double)targetAv.Position.X + (double)regionX;
                            double yTarget = (double)targetAv.Position.Y + (double)regionY;
                            double zTarget = targetAv.Position.Z - 2f;

                            Logger.DebugLog(String.Format("[Autopilot] {0} meters away from the target, starting autopilot to <{1},{2},{3}>",
                                distance, xTarget, yTarget, zTarget));

                            Proxy.Agent.AutoPilot(xTarget, yTarget, zTarget);
                        }
                        else
                        {
                            // We are in range of the target and moving, stop moving
                            Proxy.Agent.AutoPilotCancel();
                        }
                    }
                }
            }
        }

        List<UUID> PlayingAnimations = new List<UUID>();

        private void Avatars_AvatarAnimation(object sender, AvatarAnimationEventArgs e)
        {
            if(e.AvatarID == TargetAvatar && MimicAnims)
            {
                List<UUID> anims = e.Animations.Select(x => x.AnimationID).ToList();

                foreach(var anim in PlayingAnimations)
                {
                    if(!anims.Contains(anim))
                    {
                        Proxy.Agent.AnimationStop(anim, false);
                    }
                }

                foreach(var anim in anims)
                {
                    if(!PlayingAnimations.Contains(anim))
                    {
                        Proxy.Agent.AnimationStart(anim, false);
                    }
                }

                PlayingAnimations = anims;
            }
        }

        private void setTargetButton_Click(object sender, EventArgs e)
        {
            AvatarPickerSearchForm avatarPickerSearch = new AvatarPickerSearchForm();
            avatarPickerSearch.StartPosition = FormStartPosition.Manual;
            avatarPickerSearch.Location = new Point(this.Location.X + this.Width, this.Location.Y);

            if (avatarPickerSearch.ShowDialog() == DialogResult.OK)
            {
                targetAgentName.Text = avatarPickerSearch.SelectedName;
                TargetAvatar = avatarPickerSearch.SelectedID;
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            MimicAnims = checkBox1.Checked;
            if(!MimicAnims)
            {
                foreach (var anim in PlayingAnimations)
                {
                    Proxy.Agent.AnimationStop(anim, false);
                }

                PlayingAnimations.Clear();
            }
        }

        private void checkBox4_CheckedChanged(object sender, EventArgs e)
        {
            if(checkBox4.Checked)
            {
                var avatar = Proxy.Network.CurrentSim.ObjectsAvatars.Find(x => x.ID == TargetAvatar);
                if(avatar == null)
                {
                    checkBox4.Checked = false;
                    return;
                }

                TargetLocalID = avatar.LocalID;
            }

            FollowTimer.Enabled = checkBox4.Checked;
        }
    }
}
