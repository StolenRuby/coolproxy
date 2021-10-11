using Nwc.XmlRpc;
using OpenMetaverse;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CoolProxy
{
    public partial class TestRegionForm : Form
    {
        public TestRegionForm()
        {
            InitializeComponent();

            this.TopMost = CoolProxy.Settings.getBool("KeepCoolProxyOnTop");
            CoolProxy.Settings.getSetting("KeepCoolProxyOnTop").OnChanged += (x, y) => { this.TopMost = (bool)y.Value; };
        }

        private void button1_Click(object sender, EventArgs e)
        {
            RegionPickerForm regionPickerForm = new RegionPickerForm();
            regionPickerForm.StartPosition = FormStartPosition.Manual;
            regionPickerForm.Location = new Point(this.Location.X + this.Width, this.Location.Y);

            if (regionPickerForm.ShowDialog() == DialogResult.OK)
            {
                textBox1.Text = regionPickerForm.RegionName;
                textBox2.Text = regionPickerForm.RegionX.ToString();
                textBox3.Text = regionPickerForm.RegionY.ToString();
                textBox4.Text = regionPickerForm.RegionHandle.ToString();
                textBox5.Text = regionPickerForm.RegionUUID.ToString();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {

            Hashtable data = new Hashtable();
            data["region_name"] = textBox1.Text;

            //proxyFrame.SayToUser("Trying " + id.ToString() + "...");
            //OpenMetaverse.Logger.Log("Trying " + id.ToString() + "...", Helpers.LogLevel.Info);

            ArrayList SendParams = new ArrayList();
            SendParams.Add(data);

            XmlRpcRequest GridReq = new XmlRpcRequest("link_region", SendParams);

            try
            {

                XmlRpcResponse GridResp = GridReq.Send(CoolProxy.Frame.Network.CurrentSim.GatekeeperURI, 10000);

                Hashtable responseData = (Hashtable)GridResp.Value;

                if (responseData.ContainsKey("result"))
                {
                    if ((string)responseData["result"] != "True")
                    {
                        MessageBox.Show("Ohhhh dear");
                        return;
                    }
                }

                if(responseData.ContainsKey("uuid"))
                {
                    UUID uuid;
                    if(UUID.TryParse((string)responseData["uuid"], out uuid))
                    {
                        textBox5.Text = uuid.ToString();
                    }
                }
            }
            catch
            {

            }
        }
    }
}
