using OpenMetaverse;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace CoolProxy.Plugins.Useful
{
    public partial class UploaderForm : Form
    {
        private CoolProxyFrame Proxy;

        private UUID CurrentTransaction = UUID.Zero;
        private UUID FolderID = UUID.Zero;
        private string AssetName = string.Empty;
        private string FileName = string.Empty;
        private InventoryType InvType = InventoryType.Unknown;
        private WearableType WearType = WearableType.Shape;

        public UploaderForm(CoolProxyFrame frame)
        {
            Proxy = frame;
            InitializeComponent();
            
            assetTypeCombo.DataSource = Enum.GetValues(typeof(AssetType));
            assetTypeCombo.SelectedItem = AssetType.Unknown;

            inventoryTypeCombo.DataSource = Enum.GetValues(typeof(InventoryType));
            inventoryTypeCombo.SelectedItem = InventoryType.Unknown;

            wearableTypeCombo.Visible = false;
            flagsLabel.Visible = false;

            inventoryTypeCombo.SelectedValueChanged += InventoryTypeCombo_SelectedValueChanged;

            wearableTypeCombo.DataSource = Enum.GetValues(typeof(WearableType));
            wearableTypeCombo.SelectedItem = WearableType.Invalid;

            frame.Network.OnRegionChanged += Network_OnRegionChanged;

            frame.Assets.AssetUploaded += Assets_AssetUploaded;
            frame.Assets.UploadProgress += Assets_UploadProgress;
        }

        private void Assets_UploadProgress(object sender, GridProxy.AssetUploadEventArgs e)
        {
            if(e.Upload.ID == CurrentTransaction)
            {
                int percent = (int)(((float)e.Upload.Transferred / (float)e.Upload.Size) * 100.0f);
                UpdateProgress(percent);
            }
        }

        private void UpdateProgress(int percent, bool done = false)
        {
            if (this.InvokeRequired) this.BeginInvoke(new Action(() =>
            {
                UpdateProgress(percent, done);
                return;
            }));

            uploadProgressBar.Value = percent;
            uploadButton.Enabled = done;
        }

        private void Assets_AssetUploaded(object sender, GridProxy.AssetUploadEventArgs e)
        {
            if(CurrentTransaction == e.Upload.ID)
            {
                CurrentTransaction = UUID.Zero;
                if (e.Upload.Success)
                {
                    Proxy.Inventory.RequestCreateItem(FolderID, AssetName, string.Empty, e.Upload.AssetType, e.Upload.ID, InvType, WearType, PermissionMask.All, HandleCreateItem);
                }
                else
                {
                    MessageBox.Show("Upload failed!");
                    UpdateProgress(0, true);
                }
            }
        }

        private void HandleCreateItem(bool success, InventoryItem item)
        {
            UpdateProgress(100, true);
            Proxy.Inventory.RequestFetchInventory(item.UUID, item.OwnerID, false);
            FinishUpload();
        }

        private void Network_OnRegionChanged(GridProxy.RegionManager.RegionProxy proxy)
        {
            UpdateOptions();
        }

        private void InventoryTypeCombo_SelectedValueChanged(object sender, EventArgs e)
        {
            InventoryType inventoryType;
            if(Enum.TryParse<InventoryType>(inventoryTypeCombo.SelectedValue.ToString(), out inventoryType))
            {
                if(inventoryType == InventoryType.Wearable)
                {
                    flagsLabel.Text = "Wearable Type:";
                    flagsLabel.Visible = true;
                    wearableTypeCombo.Visible = true;

                    wearableTypeCombo.DataSource = Enum.GetValues(typeof(WearableType));
                    wearableTypeCombo.SelectedItem = WearableType.Invalid;
                }
                else if(inventoryType == InventoryType.Settings)
                {
                    flagsLabel.Text = "Settings Type:";
                    flagsLabel.Visible = true;
                    wearableTypeCombo.Visible = true;

                    wearableTypeCombo.DataSource = Enum.GetValues(typeof(SettingType));
                    wearableTypeCombo.SelectedItem = SettingType.Sky;
                }
                else
                {
                    wearableTypeCombo.Visible = false;
                    flagsLabel.Visible = false;
                }
            }
        }

        public void SetFile(string filename)
        {
            textBox1.Text = filename;
            textBox1.SelectionStart = textBox1.Text.Length;
            textBox1.ScrollToCaret();
            textBox1.Refresh();

            InventoryType inv_type;
            AssetType ass_type;
            Enum flags;

            TypesFromExt(Path.GetExtension(filename).ToLower(), out inv_type, out ass_type, out flags);

            inventoryTypeCombo.SelectedItem = inv_type;
            assetTypeCombo.SelectedItem = ass_type;

            if (flags != null)
                wearableTypeCombo.SelectedItem = flags;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = Util.GetCombinedFilter();
            if(openFileDialog.ShowDialog() == DialogResult.OK)
            {
                SetFile(openFileDialog.FileName);
            }
        }

        private void TypesFromExt(string ext, out InventoryType inv, out AssetType ass, out Enum type)
        {
            inv = InventoryType.Unknown;
            ass = AssetType.Unknown;
            type = null;

            if(ext == ".object")
            {
                inv = InventoryType.Object;
                ass = AssetType.Object;
            }
            else if(ext == ".mesh")
            {
                inv = InventoryType.Mesh;
                ass = AssetType.Mesh;
            }
            else if(ext == ".anim")
            {
                inv = InventoryType.Animation;
                ass = AssetType.Animation;
            }
            else if(ext == ".sound" || ext == ".ogg")
            {
                inv = InventoryType.Sound;
                ass = AssetType.Sound;
            }
            else if(ext == ".notecard")
            {
                inv = InventoryType.Notecard;
                ass = AssetType.Notecard;
            }
            else if(ext == ".lsl")
            {
                inv = InventoryType.LSL;
                ass = AssetType.LSLText;
            }
            else if(ext == ".gesture")
            {
                inv = InventoryType.Gesture;
                ass = AssetType.Gesture;
            }
            else if(ext == ".landmark")
            {
                inv = InventoryType.Landmark;
                ass = AssetType.Landmark;
            }
            else if(ext == ".texture" || ext == ".tga")
            {
                inv = InventoryType.Texture;
                ass = AssetType.Texture;
            }
            // bodyparts
            else if (ext == ".eyes")
            {
                inv = InventoryType.Wearable;
                ass = AssetType.Bodypart;
                type = WearableType.Eyes;
            }
            else if (ext == ".shape")
            {
                inv = InventoryType.Wearable;
                ass = AssetType.Bodypart;
                type = WearableType.Shape;
            }
            else if (ext == ".skin")
            {
                inv = InventoryType.Wearable;
                ass = AssetType.Bodypart;
                type = WearableType.Skin;
            }
            else if (ext == ".hair")
            {
                inv = InventoryType.Wearable;
                ass = AssetType.Bodypart;
                type = WearableType.Hair;
            }
            // settings
            else if(ext == ".sky")
            {
                inv = InventoryType.Settings;
                ass = AssetType.Settings;
                type = SettingType.Sky;
            }
            else if (ext == ".daycycle")
            {
                inv = InventoryType.Settings;
                ass = AssetType.Settings;
                type = SettingType.DayCycle;
            }
            else if (ext == ".water")
            {
                inv = InventoryType.Settings;
                ass = AssetType.Settings;
                type = SettingType.Water;
            }
            // clothing...
        }

        private void UpdateOptions()
        {
            if (Proxy.IsLindenGrid)
            {
                if (radioButton3.Checked)
                {
                    radioButton2.Checked = true;
                    radioButton3.Enabled = false;
                }
            }
            else
            {
                radioButton3.Enabled = Proxy.Network.CurrentSim?.AssetServerURI != string.Empty;
            }
        }

        private void UploaderForm_Activated(object sender, EventArgs e)
        {
            UpdateOptions();
        }

        private void uploadButton_Click(object sender, EventArgs e)
        {
            byte[] data = File.ReadAllBytes(textBox1.Text);

            if (!Enum.TryParse(assetTypeCombo.SelectedValue.ToString(), out AssetType assetType)) return;
            if (!Enum.TryParse(inventoryTypeCombo.SelectedValue.ToString(), out InvType)) return;

            WearType = WearableType.Shape;

            if (InvType == InventoryType.Wearable)
            {
                Enum.TryParse(wearableTypeCombo.SelectedValue.ToString(), out WearType);
            }
            else if (InvType == InventoryType.Settings)
            {
                if (Enum.TryParse(wearableTypeCombo.SelectedValue.ToString(), out SettingType settingType))
                {
                    WearType = (WearableType)settingType;
                }
            }

            var x = wearableTypeCombo.SelectedValue;

            FileName = textBox1.Text;
            AssetName = Path.GetFileNameWithoutExtension(FileName);

            FolderID = Proxy.Inventory.FindFolderForType(assetType);

            UpdateProgress(0);

            if (radioButton3.Checked)
            {
                FolderID = Proxy.Inventory.FindSuitcaseFolderForType(assetType);

                UsefulPlugin.ROBUST.Assets.UploadAsset(UUID.Random(), assetType, AssetName, string.Empty, Proxy.Agent.AgentID, data, (success, new_id) =>
                {
                    if (success)
                    {
                        UpdateProgress(90);

                        UUID itemID = UUID.Random();
                        UsefulPlugin.ROBUST.Inventory.AddItem(FolderID, itemID, new_id, assetType, InvType, (uint)WearType, AssetName, string.Empty, DateTime.UtcNow, (created) =>
                        {
                            if(created)
                            {
                                Proxy.Inventory.RequestFetchInventory(itemID, Proxy.Agent.AgentID, false);
                                UpdateProgress(100, true);
                                FinishUpload();
                            }
                            else
                            {
                                MessageBox.Show("Failed to create inventory item!");
                                UpdateProgress(0, true);
                            }
                        });
                    }
                    else
                    {
                        UpdateProgress(0, true);
                    }
                });

                return;
            }


            bool use_caps = radioButton2.Checked;

            switch(assetType)
            {
                case AssetType.Animation:
                case AssetType.Sound:
                case AssetType.Texture:
                    if (!use_caps) goto default;
                    Proxy.Inventory.RequestCreateItemFromAsset(data, AssetName, string.Empty, assetType, InvType, FolderID, HandleUploadComplete);
                    break;
                case AssetType.Gesture:
                case AssetType.LSLText:
                case AssetType.Notecard:
                //case AssetType.Settings:
                    {
                        if (!use_caps) goto default;
                        Proxy.Inventory.RequestCreateItem(FolderID, AssetName, string.Empty, assetType, UUID.Zero, InvType, PermissionMask.All, (success, new_item) =>
                        {
                            if(!success)
                            {
                                UpdateProgress(0, true);
                                MessageBox.Show("Error creating item!");
                                return;
                            }

                            UpdateProgress(90);

                            if (assetType == AssetType.Gesture)
                            {
                                Proxy.Inventory.RequestUploadGestureAsset(data, new_item.UUID, HandleUploadAsset);
                            }
                            else if(assetType == AssetType.Notecard)
                            {
                                Proxy.Inventory.RequestUploadNotecardAsset(data, new_item.UUID, HandleUploadAsset);
                            }
                            else if(assetType == AssetType.LSLText)
                            {
                                Proxy.Inventory.RequestUpdateScriptAgentInventory(data, new_item.UUID, true, HandleScriptUpdated);
                            }
                        });
                    }
                    break;
                default:
                    {
                        CurrentTransaction = Proxy.Assets.RequestUpload(assetType, data, false);
                    }
                    break;
            }
        }

        private void FinishUpload()
        {
            Proxy.AlertMessage("Uploaded " + FileName, false);
            this.Close();
        }

        private void HandleScriptUpdated(bool uploadSuccess, string uploadStatus, bool compileSuccess, List<string> compileMessages, UUID itemID, UUID assetID)
        {
            HandleUploadFinished(uploadSuccess, uploadStatus);
        }

        private void HandleUploadAsset(bool success, string status, UUID itemID, UUID assetID)
        {
            HandleUploadFinished(success, status);
        }

        private void HandleUploadComplete(bool success, string status, UUID itemID, UUID assetID)
        {
            HandleUploadFinished(success, status);
        }

        void HandleUploadFinished(bool success, string status)
        {
            if(!success)
            {
                UpdateProgress(0, true);
                MessageBox.Show("Error", status, MessageBoxButtons.OK);
                return;
            }

            UpdateProgress(100, true);
            FinishUpload();
        }

        private void radioButton_MouseHover(object sender, EventArgs e)
        {
            if (sender == radioButton1) toolTip1.Show("Upload the file via Xfer", radioButton1);
            else if (sender == radioButton2) toolTip1.Show("Upload via caps if possible", radioButton2);
            else if (sender == radioButton3) toolTip1.Show("Upload the asset using the ROBUST", radioButton3);
        }
    }
}
