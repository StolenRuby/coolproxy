﻿using CoolProxy.Plugins.InventoryBrowser;
using CoolProxy.Plugins.ToolBox;
using OpenMetaverse;
using OpenMetaverse.Assets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CoolProxy.Plugins.ClientAO
{
    public class ClientAOPlugin : CoolProxyPlugin
    {
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

        public static ClientAOPlugin Instance;

        internal CoolProxyFrame Proxy;

        private bool OverrideSits = false;

        private bool mEnabled = false;
        public bool Enabled
        {
            get
            {
                return mEnabled;
            }
            set
            {
                mEnabled = value;
                if(!mEnabled)
                {
                    Dictionary<UUID, bool> stop = new Dictionary<UUID, bool>();
                    foreach(var ao in Current)
                    {
                        stop.Add(ao.CurrentAnim.AssetID, false);
                    }
                    Current.Clear();

                    if (stop.Count > 0)
                    {
                        Proxy.Agent.Animate(stop, false);
                    }
                    EditorForm.UpdatePlaying();
                }
                else
                {
                    UpdateAO();
                }

                Proxy.SettingsPerAccount.setBool("AOEnabled", mEnabled);
            }
        }

        private ClientAOEditor EditorForm;

        internal string AOName = string.Empty;

        internal Dictionary<string, AOState> Overrides = new Dictionary<string, AOState>();

        public ClientAOPlugin(CoolProxyFrame frame)
        {
            Proxy = frame;
            Instance = this;

            IGUI gui = frame.RequestModuleInterface<IGUI>();

            EditorForm = new ClientAOEditor(this);

            gui.RegisterForm("ao_editor", EditorForm);

            IToolBox toolbox = frame.RequestModuleInterface<IToolBox>();

            toolbox.AddTool(new SimpleToggleFormButton("Animation Override", EditorForm)
            {
                ID = "TOGGLE_AO_FORM"
            });
            //gui.AddToggleFormQuick("Avatar", "Animation Override", EditorForm);

            frame.Avatars.AvatarAnimation += Avatars_AvatarAnimation;

            frame.Settings.getSetting("EnableAO").OnChanged += (x, y) =>
            {
                Enabled = (bool)y.Value;
                Proxy.SayToUser("AO " + (Enabled ? "enabled" : "disabled"));
            };
            Enabled = frame.Settings.getBool("EnableAO");

            frame.Settings.getSetting("OverrideSits").OnChanged += (x, y) =>
            {
                OverrideSits = (bool)y.Value;

                foreach(var state in Current)
                {
                    if(state.StateName == "Sitting")
                    {
                        Dictionary<UUID, bool> anims = new Dictionary<UUID, bool>();

                        if (OverrideSits)
                            Proxy.Agent.AnimationStart(state.CurrentAnim.AssetID, false);
                        else
                            Proxy.Agent.AnimationStop(state.CurrentAnim.AssetID, false);

                        break;
                    }
                }
            };
            OverrideSits = frame.Settings.getBool("OverrideSits");

            gui.AddMainMenuOption(new MenuOption("TOGGLE_AO_EDITOR", "Animation Override", true)
            {
                Clicked = (x) =>
                {
                    if (EditorForm.Visible)
                        EditorForm.Hide();
                    else
                        EditorForm.Show();
                },
                Checked = (x) => EditorForm.Visible
            });

            gui.AddMainMenuOption(new MenuOption("TOGGLE_AO", "Enable Client AO", true)
            {
                Clicked = (x) => frame.Settings.setBool("EnableAO", !Enabled),
                Checked = (x) => Enabled
            });

            IInventoryBrowser inv = Proxy.RequestModuleInterface<IInventoryBrowser>();

            inv.AddInventoryItemOption("Load as AO", x =>
            {
                LoadNotecard(x);
            }, AssetType.Notecard);

            Proxy.SettingsPerAccount.addSetting("AONotecardItemID", "string", UUID.Zero.ToString(), "The item ID of the notecard");
            Proxy.SettingsPerAccount.addSetting("AOEnabled", "bool", false, "Enable the Animation Override");

            Proxy.Connected += Proxy_Connected;
        }

        UUID ItemBeingFetched = UUID.Zero;

        private void Proxy_Connected()
        {
            string id = Proxy.SettingsPerAccount.getString("AONotecardItemID");

            UUID.TryParse(id, out UUID item_id);

            if (item_id != UUID.Zero)
            {
                ItemBeingFetched = item_id;
                Proxy.Inventory.ItemReceived += Inventory_ItemReceived;
                Proxy.Inventory.RequestFetchInventory(item_id, Proxy.Agent.AgentID);
            }
        }

        private void Inventory_ItemReceived(object sender, GridProxy.ItemReceivedEventArgs e)
        {
            if (e.Item.UUID != ItemBeingFetched)
                return;

            LoadNotecard(e.Item);

            Enabled = Proxy.SettingsPerAccount.getBool("AOEnabled");

            Proxy.Inventory.ItemReceived -= Inventory_ItemReceived;
        }

        public List<AOState> Current = new List<AOState>();

        public UUID[] CurrentAnims = new UUID[0];

        private void Avatars_AvatarAnimation(object sender, AvatarAnimationEventArgs e)
        {
            if(e.AvatarID == Proxy.Agent.AgentID)
            {
                CurrentAnims = e.Animations.Select(x => x.AnimationID).ToArray();
                if (Enabled) UpdateAO();
            }
        }

        public void UpdateAO()
        {
            List<string> current_states = new List<string>();

            Dictionary<UUID, bool> anims = new Dictionary<UUID, bool>();

            foreach (var anim in CurrentAnims)
            {
                if (DefaultAnimToState.TryGetValue(anim, out string state_name))
                {
                    current_states.Add(state_name);

                    int index = Current.FindIndex(x => x.StateName == state_name);
                    if (index == -1)
                    {
                        if(Overrides.TryGetValue(state_name, out var state))
                        {
                            Current.Add(state);
                            if (state.Cycle)
                            {
                                if (state.Randomise)
                                    state.Random();
                                else
                                    state.Next();
                            }

                            if (state.StateName != "Sitting" || (state.StateName == "Sitting" && OverrideSits))
                                anims.Add(state.CurrentAnim.AssetID, true);
                        }
                    }
                }
            }

            foreach (var state in Current.ToArray())
            {
                if (!current_states.Contains(state.StateName))
                {
                    var anim = state.CurrentAnim;
                    anims.Add(anim.AssetID, false);
                    Current.Remove(state);
                }
            }

            if (anims.Count > 0)
            {
                Proxy.Agent.Animate(anims, false);
                EditorForm.UpdatePlaying();
            }
        }

        public void LoadNotecard(InventoryItem notecard)
        {
            Proxy.SayToUser("Loading notecard `" + notecard.Name + "`");

            Proxy.Assets.RequestInventoryAsset(notecard, true, (transfer, download) =>
            {
                if (transfer.Success)
                {
                    AOName = notecard.Name;

                    Proxy.SettingsPerAccount.setString("AONotecardItemID", notecard.UUID.ToString());

                    AssetNotecard asset = new AssetNotecard(notecard.AssetUUID, download.AssetData);
                    asset.Decode();

                    Dictionary<string, List<string>> states_and_anims = new Dictionary<string, List<string>>();
                    Overrides.Clear();

                    string[] lines = asset.BodyText.Split('\n');
                    foreach (var line in lines)
                    {
                        if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
                            continue;

                        if (line.StartsWith("["))
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

                    foreach (var pair in states_and_anims)
                    {
                        if (!DefaultAnimToState.ContainsValue(pair.Key))
                        {
                            Proxy.SayToUser("Invalid animation state `" + pair.Key + "`");
                            continue;
                        }

                        AOState state = new AOState(pair.Key);

                        foreach (var anim_name in pair.Value)
                        {
                            if (names_to_items.TryGetValue(anim_name, out InventoryBase item))
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
                    EditorForm.UpdateUI();

                    if (Enabled) UpdateAO();
                }
                else
                {
                    Proxy.SayToUser("Failed to download notecard!");
                }
            });
        }
    }

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

        public AOAnim CurrentAnim { get; private set; } = null;

        public string StateName { get; private set; }

        public bool Random()
        {
            var current = CurrentAnim;
            Current = new Random().Next(Entries.Count);
            CurrentAnim = Entries[Current];
            return current != CurrentAnim;
        }

        public bool Next()
        {
            var current = CurrentAnim;

            if (Entries.Count < 2)
            {
                CurrentAnim = Entries[0];
            }
            else
            {
                if (++Current >= Entries.Count)
                {
                    Current = 0;
                }

                CurrentAnim = Entries[Current];
            }

            return current != CurrentAnim;
        }

        public bool Previous()
        {
            var current = CurrentAnim;

            if (Entries.Count < 2)
            {
                CurrentAnim = Entries[0];
            }
            else
            {
                if (--Current < 0)
                {
                    Current = Entries.Count - 1;
                }

                CurrentAnim = Entries[Current];
            }

            return current != CurrentAnim;
        }

        public AOState(string state_name)
        {
            StateName = state_name;
        }
    }
}
