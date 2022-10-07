using CoolProxy.Plugins.OpenSim;
using OpenMetaverse;
using OpenMetaverse.StructuredData;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CoolProxy.Plugins.InventoryBackup
{
    public class InventoryBackupPlugin : CoolProxyPlugin
    {
        internal static CoolProxyFrame Proxy;
        
        internal static IROBUST ROBUST;

        public InventoryBackupPlugin(CoolProxyFrame frame)
        {
            Proxy = frame;

            ROBUST = frame.RequestModuleInterface<IROBUST>();

            IGUI gui = Proxy.RequestModuleInterface<IGUI>();

            gui.AddInventoryItemOption("Save As...", handleSaveItemAs, x => x.AssetUUID != UUID.Zero);
            gui.AddInventoryFolderOption("Save As...", handleSaveFolderAs);
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
                    Proxy.Assets.RequestAsset(inventoryItem.AssetUUID, inventoryItem.UUID, UUID.Zero, inventoryItem.AssetType, true, SourceType.Asset, UUID.Random(), (x, y) =>
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
                    }, true);
                }
            }
        }

        private void handleSaveFolderAs(InventoryFolder folder)
        {
            InventoryBackupSettingsForm inventoryBackupSettingsForm = new InventoryBackupSettingsForm();
            inventoryBackupSettingsForm.TopMost = true;
            if (inventoryBackupSettingsForm.ShowDialog() == DialogResult.OK)
            {
                using (SaveFileDialog dialog = new SaveFileDialog())
                {
                    dialog.Filter = "Inventory Backup|*.inv";
                    dialog.FileName = folder.Name;

                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
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
            private List<AssetType> Filter;

            private int Nulls = 0;
            private int Skipped = 0;

            OSDArray InvArray = new OSDArray();

            Dictionary<UUID, AssetType> AssetsToExport = new Dictionary<UUID, AssetType>();
            List<UUID> ItemIDsToFetch = new List<UUID>();

            ZipArchive Archive;


            public FolderExporter(CoolProxyFrame proxy, InventoryFolder folder, string filename, List<AssetType> filter)
            {
                Filename = filename;
                Proxy = proxy;
                Folder = folder;
                Filter = filter;

                if (File.Exists(filename)) File.Delete(filename);
                Archive = ZipFile.Open(filename, ZipArchiveMode.Create);
            }

            public void Begin()
            {
                Proxy.AlertMessage("Populating inventory tree...", false);

                Dig(Folder);

                OSDMap map = new OSDMap();
                map["items"] = InvArray;
                map["name"] = Folder.Name;
                map["uuid"] = Folder.UUID;
                map["date"] = (int)Utils.GetUnixTime();

                AddToArchive("inv", OSDParser.SerializeLLSDXmlToBytes(map));

                FetchItems();
            }

            private void FetchItems()
            {
                if(ItemIDsToFetch.Count > 0)
                {
                    UUID first = ItemIDsToFetch.First();
                    ROBUST.Inventory.GetItem(first, Proxy.Agent.AgentID, HandleFetchItem);
                    ItemIDsToFetch.RemoveAt(0);

                }
                else
                {
                    Proxy.AlertMessage(string.Format("Downloading {0} assets...", AssetsToExport.Count), false);
                    NextAsset();
                }
            }

            private void HandleFetchItem(InventoryItem item)
            {
                if(item != null && item.AssetUUID != UUID.Zero)
                {
                    InvArray.Add(item.GetOSD());
                    AssetsToExport[item.AssetUUID] = item.AssetType;
                }

                FetchItems();
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
                        else if(item.AssetType == AssetType.Object && ROBUST != null)
                        {
                            ItemIDsToFetch.Add(item_base.UUID);
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

                    if (ROBUST != null)
                        ROBUST.Assets.DownloadAsset(first.Key, (x, y) => HandleAsset(first.Key, first.Value, x, y));
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
