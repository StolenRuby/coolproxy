using OpenMetaverse;
using OpenMetaverse.Packets;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CoolProxy.Plugins.Useful
{
    public partial class UploaderForm : Form
    {
        public UploaderForm()
        {
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

            TypesFromExt(Path.GetExtension(filename), out inv_type, out ass_type, out flags);

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
        }
    }
}
