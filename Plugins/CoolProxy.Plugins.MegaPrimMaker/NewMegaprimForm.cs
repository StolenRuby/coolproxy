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

namespace CoolProxy.Plugins.MegaPrimMaker
{
    public partial class NewMegaprimForm : Form
    {
        private CoolProxyFrame Proxy;
        public NewMegaprimForm(CoolProxyFrame frame)
        {
            Proxy = frame;
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Vector3 size = new Vector3((float)numericUpDown1.Value, (float)numericUpDown2.Value, (float)numericUpDown3.Value);

            string name = string.Format("{0}x{1}x{2}", size.X, size.Y, size.Z);

            OSAssetPrim asset = new OSAssetPrim(UUID.Random(), new byte[0]);
            asset.Children = new List<OSPrimObject>();

            OSPrimObject prim = new OSPrimObject();

            UUID object_id = UUID.Random();

            prim.CreatorID = Proxy.Agent.AgentID;
            prim.ID = object_id;
            prim.FolderID = object_id;
            prim.Name = name;
            prim.Description = "";
            prim.OwnerID = Proxy.Agent.AgentID;
            prim.CreationDate = DateTime.UtcNow;
            prim.Scale = size;
            prim.PCode = (int)PCode.Prim;
            prim.ExtraParams = new byte[0];

            #region make shape
            prim.Shape = new OSPrimObject.ShapeBlock();
            prim.Shape.PathBegin = 0;
            prim.Shape.PathCurve = (int)PathCurve.Line;
            prim.Shape.PathEnd = 1.0f;


            prim.Shape.ProfileBegin = 0.0f;
            prim.Shape.ProfileEnd = Primitive.PackEndCut(1.0f);
            prim.Shape.ProfileCurve = (int)ProfileCurve.Square;
            prim.Shape.ProfileHollow = (int)HoleType.Same;

            prim.Shape.PathTaperX = 0.0f;
            prim.Shape.PathTaperY = 0.0f;

            prim.Shape.PathScaleX = 1.0f;
            prim.Shape.PathScaleY = 1.0f;
            #endregion

            #region make textures
            prim.Textures = new Primitive.TextureEntry(UUID.Parse("89556747-24cb-43ed-920b-47caed15465f"));
            prim.Textures.FaceTextures = new Primitive.TextureEntryFace[1];

            prim.Textures.DefaultTexture = prim.Textures.CreateFace(0);

            prim.Textures.FaceTextures[0].Bump = Bumpiness.None;
            prim.Textures.FaceTextures[0].Fullbright = false;
            prim.Textures.FaceTextures[0].Glow = 0.0f;

            prim.Textures.FaceTextures[0].MaterialID = UUID.Zero;
            prim.Textures.FaceTextures[0].MediaFlags = false;
            prim.Textures.FaceTextures[0].RGBA = Color4.White;
            prim.Textures.FaceTextures[0].TexMapType = OpenMetaverse.MappingType.Default;
            #endregion

            prim.PermsBase = (uint)PermissionMask.All;
            prim.PermsEveryone = (uint)PermissionMask.All;
            prim.PermsGroup = 0;
            prim.PermsNextOwner = (uint)PermissionMask.All;
            prim.PermsOwner = (uint)PermissionMask.All;

            asset.Parent = prim;


            string object_xml = asset.EncodeXml();

            byte[] data = Utils.StringToBytes(object_xml);

            UUID asset_id = UUID.Random();

            Proxy.OpenSim.Assets.UploadAsset(asset_id, AssetType.Object, name, "", Proxy.Agent.AgentID, data, (success, new_id) =>
            {
                if (success)
                {
                    UUID folder_id = Proxy.Inventory.SuitcaseID;

                    UUID item_id = UUID.Random();

                    Proxy.OpenSim.XInventory.AddItem(folder_id, item_id, new_id, AssetType.Object, InventoryType.Object, 0, name, "", (int)Utils.DateTimeToUnixTime(DateTime.UtcNow), (created) =>
                    {
                        if (created)
                        {
                            Proxy.Inventory.RequestFetchInventory(item_id, Proxy.Agent.AgentID, false);
                        }
                        else Proxy.AlertMessage("Failed to upload item!", false);
                    });
                }
                else Proxy.AlertMessage("Failed to upload asset!", false);
            });
        }
    }
}
