using GridProxy;
using OpenMetaverse;
using OpenMetaverse.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static GridProxy.RegionManager;

namespace CoolProxy.Plugins.MagicRez
{
    public class MagicRezPlugin : CoolProxyPlugin, IMagicRez
    {
        private CoolProxyFrame Proxy;

        public MagicRezPlugin(CoolProxyFrame frame)
        {
            Proxy = frame;

            Proxy.RegisterModuleInterface<IMagicRez>(this);

            IGUI gui = frame.RequestModuleInterface<IGUI>();
            gui.AddToggleFormQuick("Hacks", "Magic Rez", new MagicRezForm(frame, this));
        }

        public static void object_asset_to_payload(string xml_str, UUID owner_id, UUID creator_id, bool random_keys, string description, out string object_data, out string script_data)
        {
            string current_breaker = "<RootPart>";

            int index = xml_str.IndexOf(current_breaker);

            object_data = xml_str.Substring(0, index);

            int index2 = xml_str.IndexOf("</RootPart>");

            int start = index + current_breaker.Length;
            object_data += xml_str.Substring(start, index2 - start);

            current_breaker = "<OtherParts>";
            index = xml_str.IndexOf(current_breaker);

            if (index != -1)
            {
                index2 = xml_str.IndexOf("</OtherParts>");
                start = index + current_breaker.Length;

                string test = xml_str.Substring(start, index2 - start);

                string[] parts = test.Split(new string[] { "<Part>", "</Part>" }, StringSplitOptions.RemoveEmptyEntries);

                object_data += "<OtherParts>";
                object_data += string.Join("", parts);
                object_data += "</OtherParts>";
            }
            else
            {
                object_data += "<OtherParts />";
            }

            object_data += "</SceneObjectGroup>";

            string[] key_replacer_split;

            if (random_keys)
            {
                // regernate uuids
                key_replacer_split = object_data.Split(new string[] { "<UUID><UUID>", "</UUID></UUID>" }, StringSplitOptions.RemoveEmptyEntries);

                object_data = string.Empty;

                for (int i = 0; i < key_replacer_split.Length - 1; i += 2)
                {
                    object_data += key_replacer_split[i];
                    object_data += "<UUID><UUID>" + UUID.Random().ToString() + "</UUID></UUID>";
                }

                object_data += key_replacer_split[key_replacer_split.Length - 1];
            }

            // change owner
            if (owner_id != UUID.Zero)
            {
                key_replacer_split = object_data.Split(new string[] { "<OwnerID><UUID>", "</UUID></OwnerID>" }, StringSplitOptions.RemoveEmptyEntries);

                object_data = string.Empty;

                for (int i = 0; i < key_replacer_split.Length - 1; i += 2)
                {
                    object_data += key_replacer_split[i];
                    object_data += "<OwnerID><UUID>" + owner_id.ToString() + "</UUID></OwnerID>";
                }

                object_data += key_replacer_split[key_replacer_split.Length - 1];
            }

            // change creator
            // if (creator_id != UUID.Zero)
            {
                key_replacer_split = object_data.Split(new string[] { "<CreatorID><UUID>", "</UUID></CreatorID>" }, StringSplitOptions.RemoveEmptyEntries);

                object_data = string.Empty;

                for (int i = 0; i < key_replacer_split.Length - 1; i += 2)
                {
                    object_data += key_replacer_split[i];
                    object_data += "<CreatorID><UUID>" + creator_id.ToString() + "</UUID></CreatorID>";
                }

                object_data += key_replacer_split[key_replacer_split.Length - 1];
            }

            // change descriptions
            if (description != "")
            {
                key_replacer_split = object_data.Split(new string[] { "<Description>", "</Description>" }, StringSplitOptions.RemoveEmptyEntries);

                object_data = string.Empty;

                for (int i = 0; i < key_replacer_split.Length - 1; i += 2)
                {
                    object_data += key_replacer_split[i];
                    object_data += "<Description>" + description + "</Description>";
                }

                object_data += key_replacer_split[key_replacer_split.Length - 1];
            }

            object_data = object_data.Replace("\"", "\\\"");


            script_data = string.Empty;

            current_breaker = "<GroupScriptStates>";
            index = xml_str.IndexOf(current_breaker);
            if (index != -1)
            {
                index2 = xml_str.IndexOf("</GroupScriptStates>");

                start = index + current_breaker.Length;
                string script_states = xml_str.Substring(start, index2 - start);

                string[] states = script_states.Split(new string[] { "<SavedScriptState", "<State ", "</SavedScriptState>" }, StringSplitOptions.RemoveEmptyEntries);

                string rejoined = "";

                for (int i = 1; i < states.Length; i += 2)
                {
                    rejoined += "<State " + states[i];
                }

                script_data = "<?xml version=\"1.0\"?><ScriptData><ScriptStates>" + rejoined + "</ScriptStates></ScriptData>";
                script_data = script_data.Replace("\"", "\\\"");
            }
        }

        public static string payload_to_object_rez(RegionProxy sim, Vector3 pos, string object_data, string script_data)
        {
            string payload = "{\"sog\":\"" + object_data;

            payload += "\",\"extra\":\"<ExtraFromItemID>00000000-0000-0000-0000-000000000000</ExtraFromItemID>\",\"modified\":true,\"new_position\":\"";
            payload += pos.ToString();
            payload += "\",";

            if (script_data.Length > 0)
            {
                payload += "\"state\":\"";
                payload += script_data;
                payload += "\",";
            }

            payload += "\"destination_x\":\"256000\",\"destination_y\":\"256000\",\"destination_name\":\"";
            payload += sim.Name;
            payload += "\",\"destination_uuid\":\"";
            payload += sim.ID.ToString();
            payload += "\"}";

            return payload;
        }

        private string modify_object_data_perms_granter(string object_data, UUID owner_id)
        {
            var key_replacer_split = object_data.Split(new string[] { "<PermsGranter><UUID>", "</UUID></PermsGranter>" }, StringSplitOptions.RemoveEmptyEntries);

            object_data = string.Empty;

            for (int i = 0; i < key_replacer_split.Length - 1; i += 2)
            {
                object_data += key_replacer_split[i];
                object_data += "<PermsGranter><UUID>" + owner_id.ToString() + "</UUID></PermsGranter>";
            }

            object_data += key_replacer_split[key_replacer_split.Length - 1];


            key_replacer_split = object_data.Split(new string[] { "<PermsMask>", "</PermsMask>" }, StringSplitOptions.RemoveEmptyEntries);

            object_data = string.Empty;

            for (int i = 0; i < key_replacer_split.Length - 1; i += 2)
            {
                object_data += key_replacer_split[i];
                object_data += "<PermsMask>128</PermsMask>"; // todo: make full
            }

            object_data += key_replacer_split[key_replacer_split.Length - 1];


            return object_data;
        }

        private string modify_script_data_perms_granter(string script_data, UUID owner_id)
        {
            script_data = script_data.Replace("<Permissions />", string.Format("<Permissions granter=\\\"{0}\\\" mask=\\\"128\\\" />", owner_id));

            // yengine
            var key_replacer_split = script_data.Split(new string[] { "<Permissions granter=", " /><EventQueue" }, StringSplitOptions.RemoveEmptyEntries);

            script_data = string.Empty;

            string perm = string.Format("\\\"{0}\\\" mask=\\\"128\\\"", owner_id);

            for (int i = 0; i < key_replacer_split.Length - 1; i += 2)
            {
                script_data += key_replacer_split[i];
                script_data += "<Permissions granter=" + perm + " /><EventQueue";
            }

            script_data += key_replacer_split[key_replacer_split.Length - 1];

            // xengine
            key_replacer_split = script_data.Split(new string[] { "<Permissions granter=", " /><ScriptState" }, StringSplitOptions.RemoveEmptyEntries);

            script_data = string.Empty;

            for (int i = 0; i < key_replacer_split.Length - 1; i += 2)
            {
                script_data += key_replacer_split[i];
                script_data += "<Permissions granter=" + perm + " /><ScriptState";
            }

            script_data += key_replacer_split[key_replacer_split.Length - 1];

            return script_data;
        }

        public void Rez(UUID asset_id, Vector3 position, UUID owner_id, UUID creator_id, string description = "", bool grant_perms = false)
        {
            Proxy.OpenSim.Assets.DownloadAsset(asset_id, (success, data) =>
            {
                try
                {
                    if (success)
                    {
                        string object_data;
                        string script_data;

                        object_asset_to_payload(Encoding.UTF8.GetString(data), owner_id, creator_id, true, description, out object_data, out script_data);

                        if (grant_perms)
                        {
                            object_data = modify_object_data_perms_granter(object_data, owner_id);
                            script_data = modify_script_data_perms_granter(script_data, owner_id);
                        }

                        WebClient webClient = new WebClient();

                        webClient.UploadStringCompleted += (x, y) =>
                        {
                            if (y.Error != null)
                            {
                                Proxy.SayToUser(y.Error.Message);
                            }
                        };

                        var sim = Proxy.Network.CurrentSim;

                        string payload = payload_to_object_rez(sim, position, object_data, script_data);

                        string url = Proxy.Network.CurrentSim.HostUri;
                        if (url == string.Empty)
                        {
                            url = string.Format("http://{0}:{1}/object/{2}", sim.RemoteEndPoint.Address.ToString(), sim.RemoteEndPoint.Port.ToString(), UUID.Random());
                        }
                        else url = string.Format("{0}/object/{1}", url, UUID.Random());

                        webClient.UploadStringAsync(new Uri(url), "POST", payload);
                    }
                    else
                    {
                        Proxy.SayToUser("Error downloading asset via ROBUST");
                    }
                }
                catch// (Exception ex)
                {
                    Proxy.AlertMessage("Failed to rez!", false);
                }
            });
        }

    }
}
