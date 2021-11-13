using OpenMetaverse;
using OpenMetaverse.Assets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoolProxy.Plugins.ClientAO
{
    public class ClientAOPlugin : CoolProxyPlugin
    {
        public class AOAnim
        {
            public UUID ItemID { get; private set; } = UUID.Zero;
            public UUID AssetID { get; private set; } = UUID.Zero;
            public string Name { get; private set; } = string.Empty;

            public AOAnim(UUID item_id, UUID asset_id, string name)
            {
                ItemID = item_id;
                AssetID = asset_id;
                Name = name;
            }
        }

        public class AOState
        {
            public List<AOAnim> Entries = new List<AOAnim>();

            public bool Cycle { get; set; } = true;
            public bool Randomise { get; set; } = true;

            private int Current = 0;

            public AOAnim GetNext()
            {
                if (Entries.Count < 2 || !Cycle)
                {
                    return Entries[0];
                }
                else
                {
                    if (Randomise)
                    {
                        var random = new Random();
                        int index = random.Next(Entries.Count);
                        return Entries[index];
                    }
                    else
                    {
                        if (++Current >= Entries.Count)
                        {
                            Current = 0;
                        }

                        return Entries[Current];
                    }
                }
            }

            public AOState()
            {

            }
        }

        public static readonly Dictionary<UUID, string> DefaultAnimToState = new Dictionary<UUID, string>()
        {
            {Animations.WALK, "Walking" },
            {Animations.RUN, "Running" },
            {Animations.TURNLEFT, "Turning Left" },
            {Animations.TURNRIGHT, "Turning Right" },
            {Animations.STAND, "Standing" },
            {Animations.STAND_1, "Standing" },
            {Animations.STAND_2, "Standing" },
            {Animations.STAND_3, "Standing" },
            {Animations.STAND_4, "Standing" },
            {Animations.CROUCH, "Crouching" },
            {Animations.CROUCHWALK, "Crouch Walking" },
            {Animations.JUMP, "Jumping" },
            {Animations.PRE_JUMP, "Prejumping" },
            {Animations.FLY, "Flying" },
            {Animations.FLYSLOW, "Flying Slowly" },
            {Animations.HOVER, "Hovering" },
            {Animations.HOVER_UP, "Hovering Up" },
            {Animations.HOVER_DOWN, "Hovering Down" },
            {Animations.LAND, "Landing" },
            {Animations.MEDIUM_LAND, "Landing" },
            {Animations.SIT, "Sitting" },
            {Animations.SIT_FEMALE, "Sitting" },
            {Animations.SIT_GENERIC, "Sitting" },
            {Animations.SIT_GROUND, "Sitting on Ground" },
            {Animations.SIT_GROUND_staticRAINED, "Sitting on Ground" },
            {Animations.SIT_TO_STAND, "Standing Up" },
            {Animations.TYPE, "Typing" },
            {Animations.FALLDOWN, "Falling" },
            {Animations.STANDUP, "Standing Up" },
        };

        public static readonly Dictionary<string, string> NotecardNameToStateName = new Dictionary<string, string>()
        {
            {"Stand.1", "Standing"},
            {"Stand.2", "Standing"},
            {"Stand.3", "Standing"},
            {"Sit.N", "Sitting"},
            {"Sit.G", "Sitting on Ground"},
            {"Turn.R", "Turning Right"},
            {"Turn.L", "Turning Left"},
            {"Walk.N", "Walking"},
            {"Walk.C", "Crouch Walking"},
            {"Jump.P", "Prejumping"},
            {"Jump.N", "Jumping"},
            {"Land.N", "Landing"},
            {"Fly.N", "Flying"},
            {"Hover.N", "Hovering" },
            {"Hover.U", "Hovering Up" },
            {"Hover.D", "Hovering Down" },
            {"Crouch", "Crouching" },
            {"Stand.U", "Standing Up" },
            {"Jump.F", "Jumping" },
            {"Jump.B", "Jumping" },
            {"Jump.D", "Jumping" },
        };

        Dictionary<string, AOState> Overrides = new Dictionary<string, AOState>();

        public static ClientAOPlugin Instance;

        private CoolProxyFrame Proxy;

        private bool Enabled = false;

        public ClientAOPlugin(SettingsManager settings, GUIManager gui, CoolProxyFrame frame)
        {
            Proxy = frame;
            Instance = this;

            frame.Avatars.AvatarAnimation += Avatars_AvatarAnimation;
        }

        Dictionary<UUID, AOAnim> CurrentOverrides = new Dictionary<UUID, AOAnim>();

        private void Avatars_AvatarAnimation(object sender, AvatarAnimationEventArgs e)
        {
            if(e.AvatarID == Proxy.Agent.AgentID && Enabled)
            {
                var signaled_anims = e.Animations.Select(x => x.AnimationID).ToArray();

                Dictionary<UUID, bool> anims = new Dictionary<UUID, bool>();

                foreach(var pair in CurrentOverrides.ToArray())
                {
                    if(!signaled_anims.Contains(pair.Key))
                    {
                        var anim = pair.Value;
                        anims.Add(anim.AssetID, false);
                        CurrentOverrides.Remove(pair.Key);
                    }
                }

                foreach(var signaled_anim in signaled_anims)
                {
                    if(!CurrentOverrides.ContainsKey(signaled_anim))
                    {
                        if (DefaultAnimToState.TryGetValue(signaled_anim, out string state_name))
                        {
                            var state = Overrides[state_name];

                            var anim = state.GetNext();
                            anims.Add(anim.AssetID, true);
                            CurrentOverrides.Add(signaled_anim, anim);
                        }
                    }
                }

                if(anims.Count > 0)
                {
                    Proxy.Agent.Animate(anims, false);
                }
            }
        }

        public void LoadNotecard(InventoryItem notecard)
        {
            // todo: download via caps...
            Proxy.OpenSim.Assets.DownloadAsset(notecard.AssetUUID, (success, data) =>
            {
                if(success)
                {
                    AssetNotecard asset = new AssetNotecard(notecard.AssetUUID, data);
                    asset.Decode();

                    Dictionary<string, List<string>> states_and_anims = new Dictionary<string, List<string>>();
                    Overrides.Clear();

                    string[] lines = asset.BodyText.Split('\n');
                    foreach(var line in lines)
                    {
                        if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
                            continue;

                        if(line.StartsWith("["))
                        {
                            int index = line.IndexOf("]", 1);
                            string state = line.Substring(1, index - 1);

                            if (NotecardNameToStateName.ContainsKey(state))
                                state = NotecardNameToStateName[state];

                            string[] split = line.Substring(index + 1).Split(',');
                            if (!states_and_anims.ContainsKey(state))
                                states_and_anims[state] = split.ToList();
                            else states_and_anims[state].AddRange(split);
                        }
                        else
                        {
                            string[] split = line.Split('|');
                            if (split.Length > 1)
                            {
                                string state = split[0];
                                string[] anims = new string[split.Length - 1];
                                Array.Copy(split, 1, anims, 0, split.Length - 1);
                                states_and_anims[state] = anims.ToList();
                            }
                        }
                    }

                    var names_to_items = Proxy.Inventory.Store.GetContents(notecard.ParentUUID).Where(x => x is InventoryAnimation).ToDictionary(x => x.Name, x => x);

                    foreach(var pair in states_and_anims)
                    {
                        if (!DefaultAnimToState.ContainsValue(pair.Key))
                        {
                            Proxy.SayToUser("Invalid animation state `" + pair.Key + "`");
                            continue;
                        }

                        AOState state = new AOState();

                        foreach(var anim_name in pair.Value)
                        {
                            if(names_to_items.TryGetValue(anim_name, out InventoryBase item))
                            {
                                InventoryAnimation anim = item as InventoryAnimation;
                                AOAnim entry = new AOAnim(anim.UUID, anim.AssetUUID, anim_name);
                                state.Entries.Add(entry);
                            }
                            else
                            {
                                Proxy.SayToUser("Animation `" + anim_name + "` was not found!");
                            }
                        }

                        Overrides[pair.Key] = state;
                    }

                    Proxy.SayToUser("Finished Loading!");
                    Enabled = true;
                }
                else
                {
                    Proxy.SayToUser("Failed to download notecard!");
                }
            });
        }
    }
}
