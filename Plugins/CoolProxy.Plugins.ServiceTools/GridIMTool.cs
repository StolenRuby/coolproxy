using OpenMetaverse;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace CoolProxy.Plugins.ServiceTools
{
    public partial class GridIMTool : Form
    {
        Point ButtonPos = new Point(12, 542);
        Size FormSize = new Size(340, 630);

        Point ButtonPosOpen = new Point(12, 586);
        Size FormSizeOpen = new Size(340, 676);

        private CoolProxyFrame Proxy;

        public GridIMTool(CoolProxyFrame frame)
        {
            Proxy = frame;
            InitializeComponent();

            hexBoxRequest.ByteProvider = new Be.Windows.Forms.DynamicByteProvider(new byte[0]);
            hexBoxRequest.BytesPerLine = 11;

            dialogTypeDrop.DataSource = Enum.GetNames(typeof(InstantMessageDialog));
            dialogTypeDrop.SelectedIndex = 0;
        }
        

        private void postMessageBtn_Click(object sender, EventArgs e)
        {
            UUID fromAgentUUID = UUID.Parse(fromAgentID.Text);
            UUID toAgentUUID = UUID.Parse(toAgentID.Text);

            UUID sessionUUID = UUID.Parse(sessionID.Text);

            InstantMessageDialog im_type = (InstantMessageDialog)Enum.Parse(typeof(InstantMessageDialog), dialogTypeDrop.SelectedValue.ToString());

            Vector3 position = new Vector3((float)numericUpDown1.Value, (float)numericUpDown2.Value, (float)numericUpDown3.Value);

            UUID regionUUID = UUID.Parse(regionID.Text);

            //hexBoxRequest.ByteProvider.ApplyChanges();

            byte[] bucket = new byte[hexBoxRequest.ByteProvider.Length];
            for(int i = 0; i < hexBoxRequest.ByteProvider.Length; i++)
            {
                bucket[i] = hexBoxRequest.ByteProvider.ReadByte(i);
            }

            Proxy.OpenSim.GridIM.SendGridIM(
                fromAgentUUID, fromAgentName.Text, toAgentUUID,
                im_type, fromGroupCheck.Checked, messageTextbox.Text, sessionUUID,
                offlineCheck.Checked, position, regionUUID,
                (uint)parentEstateID.Value, bucket, (uint)timestampSpinner.Value, checkBox1.Checked ? targetURI.Text : string.Empty);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            timestampSpinner.Value = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }

        private void button1_Click(object sender, EventArgs e)
        {

            UUID fromAgentUUID = UUID.Parse(fromAgentID.Text);
            UUID toAgentUUID = UUID.Parse(toAgentID.Text);
            sessionID.Text = (fromAgentUUID ^ toAgentUUID).ToString();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            UUID uuid;
            if(UUID.TryParse(Clipboard.GetText(), out uuid))
            {
                MessageBox.Show(uuid.ToString());
                hexBoxRequest.ByteProvider.InsertBytes(hexBoxRequest.ByteProvider.Length, uuid.GetBytes());
                hexBoxRequest.ByteProvider.ApplyChanges();

            }
        }

        private void GridIMTool_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }

        private void GridIMTool_Shown(object sender, EventArgs e)
        {
            if(Proxy.Network.CurrentSim != null)
            {
                targetURI.Text = Proxy.Network.CurrentSim.IMServerURI;
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            AvatarPickerSearchForm avatarPickerSearch = new AvatarPickerSearchForm();
            avatarPickerSearch.StartPosition = FormStartPosition.Manual;
            avatarPickerSearch.Location = new Point(this.Location.X + this.Width, this.Location.Y);

            if (avatarPickerSearch.ShowDialog() == DialogResult.OK)
            {
                fromAgentID.Text = avatarPickerSearch.SelectedID.ToString();
                fromAgentName.Text = avatarPickerSearch.SelectedName;
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            AvatarPickerSearchForm avatarPickerSearch = new AvatarPickerSearchForm();
            avatarPickerSearch.StartPosition = FormStartPosition.Manual;
            avatarPickerSearch.Location = new Point(this.Location.X + this.Width, this.Location.Y);

            if (avatarPickerSearch.ShowDialog() == DialogResult.OK)
            {
                toAgentID.Text = avatarPickerSearch.SelectedID.ToString();
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            postMessageBtn.Location = checkBox1.Checked ? ButtonPosOpen : ButtonPos;
            this.Size = checkBox1.Checked ? FormSizeOpen : FormSize;
            label1.Visible = checkBox1.Checked;
            targetURI.Visible = checkBox1.Checked;
        }

        private void button3_Click_1(object sender, EventArgs e)
        {
            RegionPickerForm regionPickerForm = new RegionPickerForm();
            regionPickerForm.StartPosition = FormStartPosition.Manual;
            regionPickerForm.Location = new Point(this.Location.X + this.Width, this.Location.Y);

            if (regionPickerForm.ShowDialog() == DialogResult.OK)
            {
                regionID.Text = regionPickerForm.RegionUUID.ToString();
            }
        }
    }
}
