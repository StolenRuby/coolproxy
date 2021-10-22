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

        private InventoryItem currentItem;
        private bool IsUpdating = false;

        public XInventoryServiceForm(CoolProxyFrame frame, InventoryFolder folder) : this(frame)
        {
            folderIDTextBox.Text = folder.UUID.ToString();
        }

        public XInventoryServiceForm(CoolProxyFrame frame) : this(frame, default(InventoryItem))
        {
            this.Text = "New Inventory Item...";
            button1.Text = "Add Item";
            button2.Enabled = true;
            itemIDTextbox.Enabled = true;
            invTypeCombo.Enabled = true;
            assetTypeCombo.Enabled = true;
        }

        public XInventoryServiceForm(CoolProxyFrame frame, InventoryItem item)
        {
            if (item is default(InventoryItem))
            {
                currentItem = new InventoryItem(UUID.Zero);
                currentItem.OwnerID = frame.Agent.AgentID;
                currentItem.Permissions = new Permissions((uint)PermissionMask.All, 0, 0, 0, (uint)PermissionMask.All);
                currentItem.CreationDate = DateTime.UtcNow;
            }
            else
            {
                // gross, but it needs to be a clone
                currentItem = InventoryItem.FromOSD(item.GetOSD());
                currentItem.UUID = item.UUID;
                IsUpdating = true;

                // another gross hack
                if (currentItem.OwnerID == UUID.Zero)
                    currentItem.OwnerID = frame.Agent.AgentID;
            }

            Proxy = frame;
            InitializeComponent();

            assetTypeCombo.DataSource = Enum.GetNames(typeof(AssetType));
            assetTypeCombo.SelectedIndex = 0;

            invTypeCombo.DataSource = Enum.GetNames(typeof(InventoryType));
            invTypeCombo.SelectedIndex = 0;

            nameTextbox.Text = currentItem?.Name ?? string.Empty;
            descTextBox.Text = currentItem?.Description ?? string.Empty;
            assetIDTextBox.Text = currentItem.AssetUUID.ToString();
            itemIDTextbox.Text = currentItem.UUID.ToString();
            assetTypeCombo.SelectedIndex = assetTypeCombo.Items.IndexOf(currentItem.AssetType.ToString());
            invTypeCombo.SelectedIndex = invTypeCombo.Items.IndexOf(currentItem.InventoryType.ToString());
            folderIDTextBox.Text = currentItem.ParentUUID.ToString();
            numericUpDown1.Value = currentItem.Flags;
            creatorIDTextBox.Text = currentItem.CreatorID.ToString();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            currentItem.Name = nameTextbox.Text;
            currentItem.Description = descTextBox.Text;
            currentItem.CreatorData = creatorDataTextBox.Text;

            if (UUID.TryParse(creatorIDTextBox.Text, out UUID creator_id))
                currentItem.CreatorID = creator_id;

            if (UUID.TryParse(itemIDTextbox.Text, out UUID item_id))
                currentItem.UUID = item_id;

            if (UUID.TryParse(assetIDTextBox.Text, out UUID asset_id))
                currentItem.AssetUUID = asset_id;

            if (UUID.TryParse(folderIDTextBox.Text, out UUID folder_id))
                currentItem.ParentUUID = folder_id;

            currentItem.Flags = (uint)numericUpDown1.Value;

            currentItem.AssetType = (AssetType)Enum.Parse(typeof(AssetType), assetTypeCombo.SelectedValue.ToString());
            currentItem.InventoryType = (InventoryType)Enum.Parse(typeof(InventoryType), invTypeCombo.SelectedValue.ToString());

            GenericSuccessResult handle = (success) =>
            {
                if (success)
                {
                    Proxy.Inventory.InjectFetchInventoryReply(currentItem);
                }
                else Proxy.AlertMessage("Failed to store inventory item!", false);
            };

            if(IsUpdating)
                Proxy.OpenSim.XInventory.UpdateItem(currentItem, handle);
            else
                Proxy.OpenSim.XInventory.AddItem(currentItem, handle);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            itemIDTextbox.Text = UUID.Random().ToString();
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
