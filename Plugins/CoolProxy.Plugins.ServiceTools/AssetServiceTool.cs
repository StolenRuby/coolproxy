using OpenMetaverse;
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

namespace CoolProxy.Plugins.ServiceTools
{
    public partial class AssetServiceTool : Form
    {
        Point ButtonPos = new Point(12, 319);
        Size FormSize = new Size(310, 414);

        Point ButtonPosOpen = new Point(12, 367);
        Size FormSizeOpen = new Size(310, 462);

        private CoolProxyFrame Proxy;

        public AssetServiceTool(CoolProxyFrame frame)
        {
            Proxy = frame;
            InitializeComponent();


            comboBox1.DataSource = Enum.GetNames(typeof(AssetType));
            comboBox1.SelectedIndex = 0;

            this.Shown += AssetServiceTool_Shown;
        }

        private void AssetServiceTool_Shown(object sender, EventArgs e)
        {
            //string url = CoolProxy.Frame.OpenSim.AssetServerURI;
            //if (url == string.Empty)
            //    url = CoolProxy.Frame.OpenSim.CurrentGridURI;

            //textBox6.Text = url + "assets";
        }

        private void uploadAssetButton_Click(object sender, EventArgs e)
        {
            int upload_attempts = (int)numericUpDown1.Value;
            int flags = 0;
            if (checkBox1.Checked) flags |= 1;
            if (checkBox2.Checked) flags |= 2;
            if (checkBox3.Checked) flags |= 4;

            AssetType asset_type = (AssetType)Enum.Parse(typeof(AssetType), comboBox1.SelectedValue.ToString());

            byte[] data = Encoding.ASCII.GetBytes("Listen, Nick...");

            if(Filename != string.Empty)
            {
                data = File.ReadAllBytes(Filename);
            }

            UUID asset_id = UUID.Parse(textBox1.Text);

            string name = textBox2.Text;
            string desc = textBox3.Text;

            UUID creator_id = UUID.Parse(textBox4.Text);

            Proxy.OpenSim.Assets.UploadAsset(asset_id, asset_type, name, desc, creator_id, data, (success, new_id) =>
            {
                if (success)
                {
                    MessageBox.Show("Success!");
                }
                else MessageBox.Show("Failed!");
            });
        }

        string Filename = string.Empty;

        private void button1_Click(object sender, EventArgs e)
        {
            using(OpenFileDialog dialog = new OpenFileDialog())
            {
                if(dialog.ShowDialog() == DialogResult.OK)
                {
                    Filename = dialog.FileName;
                    textBox5.Text = Path.GetFileName(Filename);
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            textBox1.Text = UUID.Random().ToString();
        }

        private void AssetServiceTool_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            AvatarPickerSearchForm avatarPickerSearch = new AvatarPickerSearchForm();
            avatarPickerSearch.StartPosition = FormStartPosition.Manual;
            avatarPickerSearch.Location = new Point(this.Location.X + this.Width, this.Location.Y);

            if (avatarPickerSearch.ShowDialog() == DialogResult.OK)
            {
                textBox4.Text = avatarPickerSearch.SelectedID.ToString();
            }
        }

        private void checkBox4_CheckedChanged(object sender, EventArgs e)
        {
            uploadAssetButton.Location = checkBox4.Checked ? ButtonPosOpen : ButtonPos;
            this.Size = checkBox4.Checked ? FormSizeOpen : FormSize;
            label8.Visible = checkBox4.Checked;
            textBox6.Visible = checkBox4.Checked;
        }
    }
}
