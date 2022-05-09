using GridProxy;
using OpenMetaverse;
using System;
using System.Collections;

namespace CoolProxy.Plugins.Masking
{
    class MaskingPlugin : CoolProxyPlugin
    {
        private CoolProxyFrame Proxy;

        public MaskingPlugin(CoolProxyFrame frame)
        {
            Proxy = frame;

            Proxy.Login.AddLoginRequestDelegate(handleLoginRequest);

            IGUI gui = Proxy.RequestModuleInterface<IGUI>();
            gui.AddSettingsTab("Masking", new MaskingSettingsPanel(frame));
        }

        private void handleLoginRequest(object sender, XmlRpcRequestEventArgs e)
        {
            Hashtable requestData;
            try
            {
                requestData = (Hashtable)e.m_Request.Params[0];
            }
            catch (Exception ex)
            {
                OpenMetaverse.Logger.Log(ex.Message, Helpers.LogLevel.Error);
                return;
            }

            bool spoof_mac = Proxy.Settings.getBool("SpoofMac");
            bool spoof_id0 = Proxy.Settings.getBool("SpoofId0");

            if (requestData.ContainsKey("mac") && spoof_mac)
            {
                requestData["mac"] = Proxy.Settings.getSetting("SpecifiedMacAddress");
            }

            if (requestData.ContainsKey("id0") && spoof_id0)
            {
                requestData["id0"] = Proxy.Settings.getSetting("SpecifiedId0Address");
            }
        }
    }
}
