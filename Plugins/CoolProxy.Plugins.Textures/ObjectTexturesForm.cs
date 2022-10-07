using OpenMetaverse;
using OpenMetaverse.Imaging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CoolProxy.Plugins.Textures
{
    public partial class ObjectTexturesForm : Form
    {
        private CoolProxyFrame Proxy;

        public ObjectTexturesForm(CoolProxyFrame frame)
        {
            Proxy = frame;
            InitializeComponent();

            this.TopMost = frame.Settings.getBool("KeepCoolProxyOnTop");
            frame.Settings.getSetting("KeepCoolProxyOnTop").OnChanged += (x, y) => { this.TopMost = (bool)y.Value; };

            this.Shown += ObjectTexturesForm_Shown;

            if(ObjectTexturesPlugin.ROBUST == null)
            {
                copyToInvButton.Text += " (Local)";
            }
        }

        private void ObjectTexturesForm_Shown(object sender, EventArgs e)
        {
            var selection = Proxy.Agent.Selection.ToList();

            var objects = Proxy.Network.CurrentSim?.ObjectsPrimitives.FindAll(x => selection.Contains(x.LocalID));

            if (objects == null) return;

            List<UUID> textures_ids = new List<UUID>();
            List<UUID> material_ids = new List<UUID>();

            foreach (var prim in objects)
            {
                if(prim.Textures != null)
                {
                    LogFace(textures_ids, material_ids, prim.Textures.DefaultTexture);

                    foreach (var face in prim.Textures.FaceTextures)
                        LogFace(textures_ids, material_ids, face);
                }
            }

            foreach (UUID uuid in textures_ids)
            {
                texturesDataGridView.Rows.Add(uuid, "Diffuse");
            }

            foreach (UUID uuid in material_ids)
            {
                texturesDataGridView.Rows.Add(uuid, "Material");
            }
        }

        void LogFace(List<UUID> textures, List<UUID> materials, Primitive.TextureEntryFace face)
        {
            if (face != null)
            {
                UUID texture_id = face.TextureID;
                UUID material_id = face.MaterialID;

                if (texture_id != UUID.Zero && !textures.Contains(texture_id))
                    textures.Add(texture_id);

                if (material_id != UUID.Zero && !materials.Contains(material_id))
                    materials.Add(material_id);
            }
        }

        private void texturesDataGridView_SelectionChanged(object sender, EventArgs e)
        {
            copyToInvButton.Enabled = texturesDataGridView.SelectedRows.Count > 0;

            if (texturesDataGridView.SelectedRows.Count == 1)
            {
                UUID uuid = (UUID)texturesDataGridView.SelectedRows[0].Cells[0].Value;

                Proxy.Assets.RequestAsset(uuid, AssetType.Texture, (x, y) =>
                {
                    ManagedImage imgData;
                    Image bitmap = null;

                    OpenJPEG.DecodeToImage(y.AssetData, out imgData, out bitmap);
                    pictureBox1.Image = bitmap;
                });
            }
        }

        private void copyToInvButton_Click(object sender, EventArgs e)
        {
            if (texturesDataGridView.SelectedRows.Count == 1)
            {
                UUID item_id = UUID.Random();

                UUID asset_id = (UUID)texturesDataGridView.SelectedRows[0].Cells[0].Value;

                if(ObjectTexturesPlugin.ROBUST != null)
                {
                    UUID folder_id = Proxy.Inventory.SuitcaseID != UUID.Zero ?
                        Proxy.Inventory.FindSuitcaseFolderForType(FolderType.Texture) :
                        Proxy.Inventory.FindFolderForType(FolderType.Texture);

                    ObjectTexturesPlugin.ROBUST.Inventory.AddItem(folder_id, item_id, asset_id, AssetType.Texture, InventoryType.Texture, 0, asset_id.ToString(), "", DateTime.UtcNow, (success) =>
                    {
                        Proxy.Inventory.RequestFetchInventory(item_id, Proxy.Agent.AgentID, false);
                    });
                }
                else
                {
                    InventoryItem temp_item = new InventoryItem(UUID.Random());
                    temp_item.Name = asset_id.ToString();
                    temp_item.AssetType = AssetType.Texture;
                    temp_item.AssetUUID = asset_id;
                    temp_item.CreationDate = DateTime.UtcNow;
                    temp_item.InventoryType = InventoryType.Texture;
                    temp_item.OwnerID = Proxy.Agent.AgentID;
                    temp_item.ParentUUID = Proxy.Inventory.InventoryRoot;
                    temp_item.Permissions = Permissions.FullPermissions;
                    
                    Proxy.Inventory.InjectFetchInventoryReply(temp_item);
                }
            }
        }
    }
}
