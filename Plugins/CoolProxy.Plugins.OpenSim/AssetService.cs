using OpenMetaverse;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace CoolProxy.Plugins.OpenSim
{

    public delegate void HandleUploadAssetResult(bool success, UUID new_id);

    public delegate void AssetMetadataCallback(AssetMetadata metadata);

    public delegate void AssetServiceDownloadComplete(bool success, byte[] data);


    public class AssetService
    {
        private CoolProxyFrame Proxy;

        public AssetService(CoolProxyFrame proxy)
        {
            Proxy = proxy;
        }

        public void UploadAsset(UUID asset_id, AssetType asset_type, string name, string desc, UUID creator_id, byte[] data, HandleUploadAssetResult handler = null)
        {
            int upload_attempts = 0;

            string base64_data = Convert.ToBase64String(data);

            string format = "<?xml version=\"1.0\" encoding=\"utf-8\"?><AssetBase xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"><Data>{0}</Data><FullID><Guid>{1}</Guid></FullID><ID>{1}</ID><Name>{2}</Name><Description>{3}</Description><Type>{4}</Type><UploadAttempts>{5}</UploadAttempts><Local>false</Local><Temporary>false</Temporary><CreatorID>{6}</CreatorID><Flags>{7}</Flags></AssetBase>";
            string request = string.Format(format, base64_data, asset_id, name, desc, (int)asset_type, upload_attempts, creator_id, "Normal");


            WebClient webClient = new WebClient();
            webClient.UploadStringCompleted += (x, y) =>
            {
                XmlSerializer deserializer = new XmlSerializer(typeof(string));

                string reply_str = (string)deserializer.Deserialize(GenerateStreamFromString(y.Result));

                UUID new_id;
                if (UUID.TryParse(reply_str, out new_id))
                {
                    handler?.Invoke(true, new_id);
                    return;
                }

                handler?.Invoke(false, UUID.Zero);
            };

            string target_uri = Proxy.Network.CurrentSim.AssetServerURI;

            if (target_uri == string.Empty)
                target_uri = Proxy.Network.CurrentSim.GridURI;

            target_uri += "assets";

            webClient.UploadStringAsync(new Uri(target_uri), "POST", request);
        }

        public static Stream GenerateStreamFromString(string s)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        public void DownloadAsset(UUID asset_id, AssetServiceDownloadComplete handler)
        {
            string target_uri = Proxy.Network.CurrentSim.AssetServerURI;

            if (target_uri == string.Empty)
                target_uri = Proxy.Network.CurrentSim.GridURI;


            // I'm doing it this way instead of /data because some grids dont allow /data for some reason??

            string url = string.Format("{0}assets/{1}", target_uri, asset_id.ToString());

            WebClient webClient = new WebClient();
            webClient.DownloadStringCompleted += (x, y) =>
            {
                try
                {
                    Dictionary<string, string> replyData = parseResponse(y.Result);

                    if (replyData.ContainsKey("Data"))
                    {
                        byte[] data = Convert.FromBase64String((string)replyData["Data"]);
                        handler?.Invoke(true, data);
                    }
                    else
                        handler?.Invoke(false, null);
                }
                catch
                {
                    handler?.Invoke(false, null);
                }
            };
            webClient.DownloadStringAsync(new Uri(url));
        }

        public void GetAssetMetadata(UUID asset_id, AssetMetadataCallback handler)
        {
            string target_uri = Proxy.Network.CurrentSim.AssetServerURI;

            if (target_uri == string.Empty)
                target_uri = Proxy.Network.CurrentSim.GridURI;

            string url = string.Format("{0}assets/{1}/metadata", target_uri, asset_id.ToString());

            WebClient webClient = new WebClient();
            webClient.DownloadStringCompleted += (x, y) =>
            {
                try
                {
                    Dictionary<string, string> replyData = parseResponse(y.Result);

                    var meta = AssetMetadata.FromDictionary(replyData);

                    handler?.Invoke(meta);
                }
                catch
                {
                    handler?.Invoke(null);
                }
            };
            webClient.DownloadStringAsync(new Uri(url));
        }

        Dictionary<string, string> parseResponse(string data)
        {
            XDocument doc = XDocument.Parse(data);
            Dictionary<string, string> dataDictionary = new Dictionary<string, string>();

            foreach (XElement element in doc.Descendants().Where(p => p.HasElements == false))
            {
                int keyInt = 0;
                string keyName = element.Name.LocalName;

                while (dataDictionary.ContainsKey(keyName))
                {
                    keyName = element.Name.LocalName + "_" + keyInt++;
                }

                dataDictionary.Add(keyName, element.Value);
            }

            return dataDictionary;
        }
    }

    public class AssetMetadata
    {
        public AssetType Type { get; private set; }

        public AssetMetadata()
        {

        }

        public static AssetMetadata FromDictionary(Dictionary<string, string> dict)
        {
            var meta = new AssetMetadata();

            string str;
            if (dict.TryGetValue("Type", out str))
            {
                meta.Type = (AssetType)Convert.ToInt32(str);
            }


            // todo: add the rest

            return meta;
        }
    }
}
