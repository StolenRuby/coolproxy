using GridProxy;
using OpenMetaverse;
using OpenMetaverse.Packets;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CoolProxy.Plugins.KeyTool
{
    public partial class KeyToolForm : Form
    {
        public static List<KeyToolForm> mInstances = new List<KeyToolForm>();

        Dictionary<string, Label> typeLabels = new Dictionary<string, Label>();

        private KeyTool mKeyTool;
        private UUID mKey = UUID.Zero;

        private CoolProxyFrame Frame = null;

        public KeyToolForm(CoolProxyFrame frame, UUID key)
        {
            this.Frame = frame;
            this.mKey = key;

            InitializeComponent();

            showType(KT_TYPE.KT_AGENT, AssetType.Unknown, RESULT.MAYBE);
            showType(KT_TYPE.KT_TASK, AssetType.Unknown, RESULT.MAYBE);
            showType(KT_TYPE.KT_GROUP, AssetType.Unknown, RESULT.MAYBE);
            showType(KT_TYPE.KT_REGION, AssetType.Unknown, RESULT.MAYBE);
            showType(KT_TYPE.KT_PARCEL, AssetType.Unknown, RESULT.MAYBE);
            showType(KT_TYPE.KT_ITEM, AssetType.Unknown, RESULT.MAYBE);
            showType(KT_TYPE.KT_ASSET, AssetType.Texture, RESULT.MAYBE);
            showType(KT_TYPE.KT_ASSET, AssetType.Sound, RESULT.MAYBE);
            showType(KT_TYPE.KT_ASSET, AssetType.Animation, RESULT.MAYBE);
            showType(KT_TYPE.KT_ASSET, AssetType.Landmark, RESULT.MAYBE);
            showType(KT_TYPE.KT_ASSET, AssetType.Gesture, RESULT.MAYBE);
            showType(KT_TYPE.KT_ASSET, AssetType.Clothing, RESULT.MAYBE);
            showType(KT_TYPE.KT_ASSET, AssetType.Bodypart, RESULT.MAYBE);
            showType(KT_TYPE.KT_ASSET, AssetType.Mesh, RESULT.MAYBE);
            showType(KT_TYPE.KT_ASSET, AssetType.Object, RESULT.MAYBE);
            showType(KT_TYPE.KT_ASSET, AssetType.Settings, RESULT.MAYBE);

            mInstances.Add(this);

            this.Load += KeyToolForm_Load;
        }

        private void KeyToolForm_Load(object sender, EventArgs e)
        {
            mKeyTool = new KeyTool(Frame, mKey, keyToolCallback, this);

            this.BringToFront();


            this.TopMost = KeyToolPlugin.Settings.getBool("KeepCoolProxyOnTop");
            KeyToolPlugin.Settings.getSetting("KeepCoolProxyOnTop").OnChanged += (x, y) => { this.TopMost = (bool)y.Value; };
        }

        ~KeyToolForm()
        {
            mInstances.Remove(this);
        }

        void keyToolCallback(UUID asset_id, KT_TYPE keyType, AssetType assetType, bool is_type, KeyToolForm form)
        {
            if(!form.IsDisposed)
                form.showType(keyType, assetType, is_type ? RESULT.YES : RESULT.NO);
        }

        void showType(KT_TYPE ktype, AssetType atype, RESULT result)
        {
            string type = KeyTool.aWhat(ktype, atype);

            Label label = null;
            
            if (!typeLabels.ContainsKey(type))
            {
                label = new Label();

                label.AutoSize = true;
                label.Location = new System.Drawing.Point(10, 5 + (17 * typeLabels.Count));
                label.Size = new System.Drawing.Size(38, 13);
                label.TabIndex = 0;
                label.Text = type;
                label.DoubleClick += (x, y) => KeyTool.openKey(ktype, atype, mKey);

                typeLabels[type] = label;

                base.Controls.Add(label);
            }
            else label = typeLabels[type];

            switch(result)
            {
                case RESULT.YES:
                    //Frame.SayToUser("KeyTool", mKey.ToString() + " is a " + type);
                    label.ForeColor = Color.Green;
                    if (KeyToolPlugin.Settings.getBool("AutomaticallyOpenKeyTool"))
                    {
                        new Task(() => KeyTool.openKey(ktype, atype, mKey)).Start();

                        if (KeyToolPlugin.Settings.getBool("AutomaticallyCloseKeyTool"))
                        {
                            //this.Invoke(new MethodInvoker(() => { this.Dispose(); this.Close(); }));
                            this.Close();
                        }
                    }
                    break;
                case RESULT.NO:
                    label.ForeColor = Color.Red;
                    break;
                case RESULT.MAYBE:
                    label.ForeColor = Color.Gray;
                    break;
                default:
                    label.ForeColor = Color.White;
                    break;
            }
        }
    }
}
