﻿using OpenMetaverse;
using OpenMetaverse.Packets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CoolProxy.Plugins.Useful
{
    public class UsefulPlugin : CoolProxyPlugin
    {
        private SettingsManager Settings;
        private GUIManager GUI;
        private CoolProxyFrame Proxy;

        public UsefulPlugin(SettingsManager settings, GUIManager gui, CoolProxyFrame frame)
        {
            Settings = settings;
            GUI = gui;
            Proxy = frame;

            GUI.AddSingleMenuItem("Teleport To", (avatar_id) =>
            {
                var avatars = Proxy.Network.CurrentSim.ObjectsAvatars;
                Avatar avatar = avatars.Find(x => x.ID == avatar_id);

                if (avatar != null)
                {
                    Proxy.Agent.Teleport(Proxy.Network.CurrentSim.Handle, avatar.Position);
                }
            });

            GUI.AddSingleMenuItem("Offer Teleport", (avatar_id) =>
            {
                Proxy.Agent.SendTeleportLure(avatar_id);
            });

            GUI.AddMultipleMenuItem("Offer Teleport", (avatars) =>
            {
                foreach (UUID avatar in avatars)
                {
                    Proxy.Agent.SendTeleportLure(avatar);
                }
            });

            GUI.AddSingleMenuItem("Copy Key", copyAvatarKey);
            GUI.AddMultipleMenuItem("Copy Keys", copyAvatarKeys);

            GUI.AddToolButton("UUID", "Avatar Picker to Clipboard", avatarPickerToClipboard);
            GUI.AddToolButton("UUID", "Group Picker to Clipboard", groupPickerToClipboard);

            var uploader_form = new UploaderForm();

            if(Util.IsDebugMode)
            {
                GUI.AddToggleFormQuick("Assets", "Upload Asset", uploader_form);
                GUI.AddTrayOption("Upload Asset...", (x, y) =>
                {
                    OpenFileDialog openFileDialog = new OpenFileDialog();
                    openFileDialog.Filter = Util.GetCombinedFilter();
                    if (openFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        uploader_form.SetFile(openFileDialog.FileName);
                        uploader_form.Show();
                    }
                });
            }

            GUI.AddInventoryItemOption("Copy Item ID", x => Clipboard.SetText(x.UUID.ToString()));
            GUI.AddInventoryItemOption("Copy Asset ID", x => Clipboard.SetText(x.AssetUUID.ToString()), x => x.AssetUUID != UUID.Zero);

            GUI.AddInventoryFolderOption("Copy Folder ID", x => Clipboard.SetText(x.UUID.ToString()));

            GUI.AddInventoryItemOption("Save As...", handleSaveItemAs, x => x.AssetUUID != UUID.Zero);

            GUI.AddInventoryItemOption("Play Locally", handlePlaySoundLocally, AssetType.Sound);
            GUI.AddInventoryItemOption("Play Inworld", handlePlaySoundInworld, AssetType.Sound);
        }

        private void groupPickerToClipboard(object sender, EventArgs e)
        {
            GroupPickerForm groupPickerForm = new GroupPickerForm(GroupPowers.None);
            groupPickerForm.StartPosition = FormStartPosition.CenterScreen;

            if(groupPickerForm.ShowDialog() == DialogResult.OK)
            {
                Clipboard.SetText(groupPickerForm.GroupID.ToString());
            }
        }

        private void copyAvatarKey(UUID avatar)
        {
            Clipboard.SetText(avatar.ToString());
        }

        private void copyAvatarKeys(List<UUID> targets)
        {
            Clipboard.SetText(string.Join(", ", targets));
        }

        private void avatarPickerToClipboard(object sender, EventArgs e)
        {
            AvatarPickerSearchForm avatarPickerSearchForm = new AvatarPickerSearchForm();
            avatarPickerSearchForm.StartPosition = FormStartPosition.CenterScreen;

            if (avatarPickerSearchForm.ShowDialog() == DialogResult.OK)
            {
                Clipboard.SetText(avatarPickerSearchForm.SelectedID.ToString());
            }
        }

        private void handlePlaySoundInworld(InventoryItem inventoryItem)
        {
            TriggerSound(inventoryItem.AssetUUID, false);
        }

        private void handlePlaySoundLocally(InventoryItem inventoryItem)
        {
            TriggerSound(inventoryItem.AssetUUID, true);
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

        private void handleSaveItemAs(InventoryItem inventoryItem)
        {
            using (SaveFileDialog dialog = new SaveFileDialog())
            {
                string suggested_name;

                if (inventoryItem.Name != string.Empty)
                    suggested_name = inventoryItem.Name;
                else
                    suggested_name = inventoryItem.AssetUUID.ToString();

                string ext = Util.GetExtensionForInventoryType(inventoryItem.InventoryType, inventoryItem.Flags);

                //suggested_name += "." + ext;

                dialog.AddExtension = true;
                dialog.DefaultExt = ext;
                dialog.Filter = Util.GetFileFilterFromAssetType(inventoryItem.AssetType);
                dialog.FileName = suggested_name;

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    Proxy.Assets.RequestAsset(inventoryItem.AssetUUID, inventoryItem.AssetType, (x, y) =>
                    {
                        if (x.Success)
                        {
                            File.WriteAllBytes(dialog.FileName, y.AssetData);
                            //coolProxyFrame.SayToUser("Saved to " + dialog.FileName);

                            Proxy.AlertMessage("Saved to " + dialog.FileName, false);
                        }
                        else
                        {
                            //coolProxyFrame.SayToUser("Failed to download " + suggested_name + "!");
                            Proxy.AlertMessage("Failed to download " + suggested_name + "!", false);
                        }
                    });
                }
            }
        }
    }
}
