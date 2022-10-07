using CoolProxy.Plugins.OpenSim;
using OpenMetaverse;
using OpenMetaverse.Packets;
using OpenMetaverse.StructuredData;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CoolProxy.Plugins.Useful
{
    public class UsefulPlugin : CoolProxyPlugin
    {
        private CoolProxyFrame Proxy;

        internal static IROBUST ROBUST;

        public UsefulPlugin(CoolProxyFrame frame)
        {
            Proxy = frame;

            ROBUST = frame.RequestModuleInterface<IROBUST>();

            IGUI gui = frame.RequestModuleInterface<IGUI>();
            gui.AddSingleMenuItem("Teleport To", (avatar_id) =>
            {
                var avatars = Proxy.Network.CurrentSim.ObjectsAvatars;
                Avatar avatar = avatars.Find(x => x.ID == avatar_id);

                if (avatar != null)
                {
                    Proxy.Agent.Teleport(Proxy.Network.CurrentSim.Handle, avatar.Position);
                }
            });

            gui.AddSingleMenuItem("Offer Teleport", (avatar_id) =>
            {
                Proxy.Agent.SendTeleportLure(avatar_id);
            });

            gui.AddMultipleMenuItem("Offer Teleport", (avatars) =>
            {
                foreach (UUID avatar in avatars)
                {
                    Proxy.Agent.SendTeleportLure(avatar);
                }
            });

            gui.AddSingleMenuItem("Copy Key", copyAvatarKey);
            gui.AddMultipleMenuItem("Copy Keys", copyAvatarKeys);

            gui.AddToolButton("UUID", "Avatar Picker to Clipboard", avatarPickerToClipboard);
            gui.AddToolButton("UUID", "Group Picker to Clipboard", groupPickerToClipboard);

            var uploader_form = new UploaderForm(frame);

            gui.AddToggleFormQuick("Assets", "Upload Asset", uploader_form);
            gui.AddTrayOption("Upload Asset...", (x, y) =>
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = Util.GetCombinedFilter();
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    uploader_form.SetFile(openFileDialog.FileName);
                    uploader_form.Show();
                }
            });

            gui.AddInventoryItemOption("Copy Item ID", x => Clipboard.SetText(x.UUID.ToString()));
            gui.AddInventoryItemOption("Copy Asset ID", x => Clipboard.SetText(x.AssetUUID.ToString()), x => x.AssetUUID != UUID.Zero);

            gui.AddInventoryFolderOption("Copy Folder ID", x => Clipboard.SetText(x.UUID.ToString()));

            gui.AddInventoryItemOption("Play Locally", handlePlaySoundLocally, AssetType.Sound);
            gui.AddInventoryItemOption("Play Inworld", handlePlaySoundInworld, AssetType.Sound);
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
    }
}
