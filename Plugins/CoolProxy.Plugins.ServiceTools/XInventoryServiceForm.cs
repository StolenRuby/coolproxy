using OpenMetaverse;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace CoolProxy.Plugins.ServiceTools
{
    public partial class XInventoryServiceForm : Form
    {
        Point ButtonPos = new Point(12, 365);
        Point ButtonPosOpen = new Point(12, 415);

        Size FormSize = new Size(311, 467);
        Size FormSizeOpen = new Size(311, 517);

        private CoolProxyFrame Proxy;

        public XInventoryServiceForm(CoolProxyFrame frame)
        {
            Proxy = frame;
            InitializeComponent();


            assetTypeCombo.DataSource = Enum.GetNames(typeof(AssetType));
            assetTypeCombo.SelectedIndex = 0;

            invTypeCombo.DataSource = Enum.GetNames(typeof(InventoryType));
            invTypeCombo.SelectedIndex = 0;

            this.Shown += XInventoryServiceForm_Shown;
        }

        private void XInventoryServiceForm_Shown(object sender, EventArgs e)
        {
            //string url = CoolProxy.Frame.OpenSim.InvetoryServerURI;
            //if (url == string.Empty)
            //    url = CoolProxy.Frame.OpenSim.CurrentGridURI;

            //uriTextBox.Text = url + "xinventory";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string target_uri = Proxy.Network.CurrentSim.InvetoryServerURI;
            if (target_uri == string.Empty)
                target_uri = Proxy.Network.CurrentSim.GridURI;

            target_uri += "xinventory";

            UUID assetID = UUID.Parse(assetIDTextBox.Text);
            UUID ownerID = Proxy.Agent.AgentID;

            UUID itemID = UUID.Parse(itemIDTextbox.Text);
            UUID folderID = UUID.Parse(folderIDTextBox.Text);

            bool group_owned = false;
            UUID groupID = UUID.Zero;


            AssetType assetType = (AssetType)Enum.Parse(typeof(AssetType), assetTypeCombo.SelectedValue.ToString());
            InventoryType invType = (InventoryType)Enum.Parse(typeof(InventoryType), invTypeCombo.SelectedValue.ToString());

            string item_name = nameTextbox.Text;
            string item_desc = descTextBox.Text;

            string creatorData = creatorDataTextBox.Text;
            string creatorID = creatorIDTextBox.Text;

            uint nextPermissions = 532480;
            uint currentPermissions = 581635;
            uint basePermissions = 581635;
            uint everyonePermissions = 0;
            uint groupPermissions = 0;

            uint sale_price = 0;

            SaleType saleType = SaleType.Not;

            uint flags = (uint)numericUpDown1.Value;

            int creationDate = 0;

            Proxy.OpenSim.XInventory.AddItem(
                folderID, itemID, assetID, ownerID, 
                assetType, invType, flags, item_name, item_desc, creationDate,
                nextPermissions, currentPermissions, basePermissions, everyonePermissions, groupPermissions,
                groupID, group_owned, sale_price, saleType,
                creatorID, creatorData, success =>
                {
                    if (success)
                    {
                        Proxy.Inventory.RequestFetchInventory(itemID, ownerID, false);
                    }
                    else Proxy.AlertMessage("Error adding item to suitcase!", false);
                });
        }

        private void button2_Click(object sender, EventArgs e)
        {
            itemIDTextbox.Text = UUID.Random().ToString();
        }

        private void assetTypeCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            AssetType assetType = (AssetType)Enum.Parse(typeof(AssetType), assetTypeCombo.SelectedValue.ToString());


            UUID folder_id = Proxy.Inventory.SuitcaseID != UUID.Zero ?
                Proxy.Inventory.FindSuitcaseFolderForType((FolderType)assetType) :
                Proxy.Inventory.FindFolderForType((FolderType)assetType);

            folderIDTextBox.Text = folder_id.ToString();
        }

        private void XInventoryServiceForm_FormClosing(object sender, FormClosingEventArgs e)
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
                creatorIDTextBox.Text = avatarPickerSearch.SelectedID.ToString();
            }
        }

        private void XInventoryServiceForm_HelpButtonClicked(object sender, CancelEventArgs e)
        {
            MessageBox.Show("never gonna give you up~");
            e.Cancel = true;
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            button1.Location = checkBox1.Checked ? ButtonPosOpen : ButtonPos;
            this.Size = checkBox1.Checked ? FormSizeOpen : FormSize;
            uriLabel.Visible = checkBox1.Checked;
            targetUriTextbox.Visible = checkBox1.Checked;
        }
    }
}
