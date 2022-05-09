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

        public UsefulPlugin(CoolProxyFrame frame)
        {
            Proxy = frame;

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

            var uploader_form = new UploaderForm();

            if(Util.IsDebugMode)
            {
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
            }

            gui.AddInventoryItemOption("Copy Item ID", x => Clipboard.SetText(x.UUID.ToString()));
            gui.AddInventoryItemOption("Copy Asset ID", x => Clipboard.SetText(x.AssetUUID.ToString()), x => x.AssetUUID != UUID.Zero);

            gui.AddInventoryFolderOption("Copy Folder ID", x => Clipboard.SetText(x.UUID.ToString()));

            gui.AddInventoryItemOption("Save As...", handleSaveItemAs, x => x.AssetUUID != UUID.Zero);
            gui.AddInventoryFolderOption("Save As...", handleSaveFolderAs);

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

        private void handleSaveFolderAs(InventoryFolder folder)
        {
            InventoryBackupSettingsForm inventoryBackupSettingsForm = new InventoryBackupSettingsForm();
            inventoryBackupSettingsForm.TopMost = true;
            if (inventoryBackupSettingsForm.ShowDialog() == DialogResult.OK)
            {
                using(SaveFileDialog dialog = new SaveFileDialog())
                {
                    dialog.Filter = "Inventory Backup|*.inv";
                    dialog.FileName = folder.Name;
                    
                    if(dialog.ShowDialog() == DialogResult.OK)
                    {
                        bool assets = MessageBox.Show("Would you like to download the assets for these items?", "", MessageBoxButtons.YesNo) == DialogResult.Yes;

                        var exporter = new FolderExporter(Proxy, folder, dialog.FileName, inventoryBackupSettingsForm.SelectedTypes);
                        new Task(() =>
                        {
                            exporter.Begin();
                        }).Start();
                    }
                }
            }
        }

        class FolderExporter
        {
            private CoolProxyFrame Proxy;
            private InventoryFolder Folder;
            private string Filename;
            //private Regex Regex;
            private List<AssetType> Filter;

            private int Nulls = 0;
            private int Skipped = 0;

            OSDArray InvArray = new OSDArray();

            Dictionary<UUID, AssetType> AssetsToExport = new Dictionary<UUID, AssetType>();


            ZipArchive Archive;

            bool UseRobust = false;


            public FolderExporter(CoolProxyFrame proxy, InventoryFolder folder, string filename, List<AssetType> filter)
            {
                Filename = filename;
                Proxy = proxy;
                Folder = folder;
                Filter = filter;

                UseRobust = proxy.Network.CurrentSim.AssetServerURI != string.Empty;

                //string regexSearch = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
                //Regex = new Regex(string.Format("[{0}]", Regex.Escape(regexSearch)));

                if (File.Exists(filename)) File.Delete(filename);
                Archive = ZipFile.Open(filename, ZipArchiveMode.Create);
            }

            public void Begin()
            {
                Dig(Folder);

                Proxy.AlertMessage(string.Format("Downloading {0} assets...", AssetsToExport.Count), false);

                OSDMap map = new OSDMap();
                map["items"] = InvArray;
                map["name"] = Folder.Name;
                map["uuid"] = Folder.UUID;
                map["date"] = (int)Utils.GetUnixTime();

                AddToArchive("inv", OSDParser.SerializeLLSDXmlToBytes(map));

                NextAsset();
            }

            private void Dig(InventoryFolder folder)
            {
                var content = Proxy.Inventory.FolderContents(folder.UUID, Proxy.Agent.AgentID, true, true, InventorySortOrder.ByName, 10000);
                foreach (var item_base in content)
                {
                    if (item_base is InventoryFolder)
                    {
                        InvArray.Add(((InventoryFolder)item_base).GetOSD());
                        Dig(item_base as InventoryFolder);
                    }
                    else
                    {
                        var item = item_base as InventoryItem;
                        if (!Filter.Contains(item.AssetType))
                        {
                            Skipped++;
                        }
                        else if (item.AssetUUID != UUID.Zero)
                        {
                            InvArray.Add(item.GetOSD());

                            AssetsToExport[item.AssetUUID] = item.AssetType;
                        }
                        else
                        {
                            Nulls++;
                        }
                    }
                }
            }

            private void HandleAsset(UUID id, AssetType type, bool success, byte[] data)
            {
                if (success)
                {
                    AddToArchive(id.ToString() + "." + type.ToString(), data);
                }

                NextAsset();
            }

            void NextAsset()
            {
                if (AssetsToExport.Count > 0)
                {
                    var first = AssetsToExport.First();
                    AssetsToExport.Remove(first.Key);

                    if (UseRobust)
                        Proxy.OpenSim.Assets.DownloadAsset(first.Key, (x, y) => HandleAsset(first.Key, first.Value, x, y));
                    else
                        Proxy.Assets.EasyDownloadAsset(first.Key, first.Value, (x, y) => HandleAsset(first.Key, first.Value, x, y));
                }
                else
                {
                    Archive.Dispose();
                    Proxy.AlertMessage("Saved to " + Filename, false);
                }
            }

            void AddToArchive(string name, byte[] data)
            {
                try
                {
                    var zipEntry = Archive.CreateEntry(name);

                    using (var stream = new MemoryStream(data))
                    using (var entry = zipEntry.Open())
                    {
                        stream.CopyTo(entry);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log(ex.Message, Helpers.LogLevel.Debug);
                }
            }
        }
    }
}
