using Nwc.XmlRpc;
using OpenMetaverse;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoolProxy.Plugins.OpenSim
{
    public class HypergridService
    {
        private CoolProxyFrame Proxy;

        public HypergridService(CoolProxyFrame proxy)
        {
            Proxy = proxy;
        }

        public void LinkRegion(string region_name, out UUID region_uuid, out string image_url)
        {
            Hashtable data = new Hashtable();
            data["region_name"] = region_name;

            region_uuid = UUID.Zero;
            image_url = string.Empty;

            //proxyFrame.SayToUser("Trying " + id.ToString() + "...");
            //OpenMetaverse.Logger.Log("Trying " + id.ToString() + "...", Helpers.LogLevel.Info);

            ArrayList SendParams = new ArrayList();
            SendParams.Add(data);

            XmlRpcRequest GridReq = new XmlRpcRequest("link_region", SendParams);

            try
            {

                XmlRpcResponse GridResp = GridReq.Send(Proxy.Network.CurrentSim.GatekeeperURI, 10000);

                Hashtable responseData = (Hashtable)GridResp.Value;

                if (responseData.ContainsKey("result"))
                {
                    if ((string)responseData["result"] != "True")
                    {
                        //MessageBox.Show("Ohhhh dear");
                        return;
                    }
                }

                if (responseData.ContainsKey("uuid"))
                {
                    if (!UUID.TryParse((string)responseData["uuid"], out region_uuid))
                    {
                        OpenMetaverse.Logger.Log("Invalid UUID in link_region response!", Helpers.LogLevel.Debug);
                    }
                }

                if (responseData.ContainsKey("region_image"))
                {
                    image_url = (string)responseData["region_image"];
                }
            }
            catch (Exception ex)
            {
                Proxy.SayToUser(ex.Message);
            }
        }
    }

}
