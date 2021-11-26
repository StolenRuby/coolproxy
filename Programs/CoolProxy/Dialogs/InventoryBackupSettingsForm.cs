using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenMetaverse;

namespace CoolProxy
{
    public partial class InventoryBackupSettingsForm : Form
    {
        public List<AssetType> SelectedTypes { get; private set; } = new List<AssetType>();

        public InventoryBackupSettingsForm()
        {
            InitializeComponent();

            this.TopMost = CoolProxy.Settings.getBool("KeepCoolProxyOnTop");
            CoolProxy.Settings.getSetting("KeepCoolProxyOnTop").OnChanged += (x, y) => { this.TopMost = (bool)y.Value; };
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (texturesCheckbox.Checked)
                SelectedTypes.Add(AssetType.Texture);

            if (soundsCheckbox.Checked)
                SelectedTypes.Add(AssetType.Sound);

            if (callingCardsCheckbox.Checked)
                SelectedTypes.Add(AssetType.CallingCard);

            if (landmarksCheckbox.Checked)
                SelectedTypes.Add(AssetType.Landmark);

            if (scriptsCheckbox.Checked)
                SelectedTypes.Add(AssetType.LSLText);

            if (clothingCheckbox.Checked)
                SelectedTypes.Add(AssetType.Clothing);

            if (objectsCheckbox.Checked)
                SelectedTypes.Add(AssetType.Object);

            if (notecardsCheckbox.Checked)
                SelectedTypes.Add(AssetType.Notecard);

            if (animationsCheckbox.Checked)
                SelectedTypes.Add(AssetType.Animation);

            if (gesturesCheckbox.Checked)
                SelectedTypes.Add(AssetType.Gesture);

            if (settingsCheckbox.Checked)
                SelectedTypes.Add(AssetType.Settings);

            if (bodypartsCheckbox.Checked)
                SelectedTypes.Add(AssetType.Bodypart);

            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
