using Nwc.XmlRpc;
using OpenMetaverse;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CoolProxy.Plugins.OpenSim
{
    public class GridIMService
    {
        private CoolProxyFrame Proxy;

        public GridIMService(CoolProxyFrame proxy)
        {
            Proxy = proxy;
        }

        public void SendGridIM(UUID fromAgentUUID, string fromAgentName, UUID toAgentUUID, InstantMessageDialog dialogType, bool from_group, string message, UUID sessionUUID, bool offline, Vector3 position, UUID regionUUID, uint parent_estate, byte[] bucket, uint timestamp, string target_uri = "")
        {
            Hashtable gim = new Hashtable();
            gim["from_agent_id"] = fromAgentUUID.ToString();
            // Kept for compatibility
            gim["from_agent_session"] = UUID.Zero.ToString();
            gim["to_agent_id"] = toAgentUUID.ToString();
            gim["im_session_id"] = sessionUUID.ToString();
            gim["timestamp"] = timestamp.ToString();
            gim["from_agent_name"] = fromAgentName;
            gim["message"] = message;
            byte[] dialogdata = new byte[1] { (byte)dialogType };
            gim["dialog"] = Convert.ToBase64String(dialogdata, Base64FormattingOptions.None);

            if (from_group)
                gim["from_group"] = "TRUE";
            else
                gim["from_group"] = "FALSE";

            byte[] offlinedata = new byte[1] { offline ? (byte)0x01 : (byte)0x00 };
            gim["offline"] = Convert.ToBase64String(offlinedata, Base64FormattingOptions.None);
            gim["parent_estate_id"] = parent_estate.ToString();
            gim["position_x"] = position.X.ToString();
            gim["position_y"] = position.Y.ToString();
            gim["position_z"] = position.Z.ToString();
            gim["region_id"] = regionUUID.ToString();
            gim["binary_bucket"] = Convert.ToBase64String(bucket, Base64FormattingOptions.None);
            gim["region_id"] = new UUID(regionUUID).ToString();

            //if (messageKey != string.Empty)
            //    gim["message_key"] = messageKey;

            ArrayList SendParams = new ArrayList();
            SendParams.Add(gim);

            if (target_uri == string.Empty)
                target_uri = Proxy.Network.CurrentSim.IMServerURI;

            if (target_uri == string.Empty)
                target_uri = Proxy.Network.CurrentSim.GridURI;

            Task.Run(() =>
            {
                XmlRpcRequest GridReq = new XmlRpcRequest("grid_instant_message", SendParams);

                try
                {
                    XmlRpcResponse GridResp = GridReq.Send(target_uri, 10000);

                    Hashtable responseData = (Hashtable)GridResp.Value;

                    if (responseData.ContainsKey("success"))
                    {
                        if ((string)responseData["success"] == "TRUE")
                        {
                            //CoolProxy.Frame.SayToUser("Message sent successfully!");
                            return;
                        }
                        else
                        {
                            //CoolProxy.Frame.SayToUser("Failed to send message!");
                            Proxy.AlertMessage("Failed to send message!", false);
                            return;
                        }
                    }
                    else
                    {
                        Proxy.SayToUser(string.Format("[GRID INSTANT MESSAGE]: No response from {0}", target_uri));
                        return;
                    }
                }
                catch (WebException err)
                {
                    Proxy.SayToUser(string.Format("[GRID INSTANT MESSAGE]: Error sending message to {0} the host didn't respond " + err.ToString(), target_uri));
                }
            });
        }
    }
}
