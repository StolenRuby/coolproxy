using OpenMetaverse;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace CoolProxy.Plugins.OpenSim
{
    public static class ServerUtils
    {
        public static Dictionary<string, object> ParseXmlResponse(string data)
        {
            //m_log.DebugFormat("[XXX]: received xml string: {0}", data);

            try
            {
                XmlReaderSettings xset = new XmlReaderSettings() { IgnoreWhitespace = true, IgnoreComments = true, ConformanceLevel = ConformanceLevel.Fragment, CloseInput = true };
                XmlParserContext xpc = new XmlParserContext(null, null, null, XmlSpace.None);
                xpc.Encoding = Util.UTF8NoBomEncoding;
                using (XmlReader xr = XmlReader.Create(new StringReader(data), xset, xpc))
                {
                    if (!xr.ReadToFollowing("ServerResponse"))
                        return new Dictionary<string, object>();
                    return ScanXmlResponse(xr);
                }
            }
            catch// (Exception e)
            {
                //m_log.DebugFormat("[serverUtils.ParseXmlResponse]: failed error: {0}\n --string:\n{1}\n", e.Message, data);
            }
            return new Dictionary<string, object>();
        }

        private static Dictionary<string, object> ScanXmlResponse(XmlReader xr)
        {
            Dictionary<string, object> ret = new Dictionary<string, object>();
            xr.Read();
            while (!xr.EOF && xr.NodeType != XmlNodeType.EndElement)
            {
                if (xr.IsStartElement())
                {
                    string type = xr.GetAttribute("type");
                    if (type != "List")
                    {
                        if (xr.IsEmptyElement)
                        {
                            ret[XmlConvert.DecodeName(xr.Name)] = "";
                            xr.Read();
                        }
                        else
                            ret[XmlConvert.DecodeName(xr.Name)] = xr.ReadElementContentAsString();
                    }
                    else
                    {
                        string name = XmlConvert.DecodeName(xr.Name);
                        if (xr.IsEmptyElement)
                            ret[name] = new Dictionary<string, object>();
                        else
                            ret[name] = ScanXmlResponse(xr);
                        xr.Read();
                    }
                }
                else
                    xr.Read();
            }
            return ret;
        }


        public static string BuildQueryString(Dictionary<string, object> data)
        {
            // this is not conform to html url encoding
            // can only be used on Body of POST or PUT
            StringBuilder sb = new StringBuilder(4096);

            string pvalue;

            foreach (KeyValuePair<string, object> kvp in data)
            {
                if (kvp.Value is List<string>)
                {
                    List<string> l = (List<String>)kvp.Value;
                    int llen = l.Count;
                    string nkey = System.Web.HttpUtility.UrlEncode(kvp.Key);
                    for (int i = 0; i < llen; ++i)
                    {
                        if (sb.Length != 0)
                            sb.Append("&");
                        sb.Append(nkey);
                        sb.Append("[]=");
                        sb.Append(System.Web.HttpUtility.UrlEncode(l[i]));
                    }
                }
                else if (kvp.Value is Dictionary<string, object>)
                {
                    // encode complex structures as JSON
                    // needed for estate bans with the encoding used on xml
                    // encode can be here because object does contain the structure information
                    // but decode needs to be on estateSettings (or other user)
                    string js;
                    try
                    {
                        // bypass libovm, we dont need even more useless high level maps
                        // this should only be called once.. but no problem, i hope
                        // (other uses may need more..)
                        LitJson.JsonMapper.RegisterExporter<UUID>((uuid, writer) => writer.Write(uuid.ToString()));
                        js = LitJson.JsonMapper.ToJson(kvp.Value);
                    }
                    //                   catch(Exception e)
                    catch
                    {
                        continue;
                    }
                    if (sb.Length != 0)
                        sb.Append("&");
                    sb.Append(System.Web.HttpUtility.UrlEncode(kvp.Key));
                    sb.Append("=");
                    sb.Append(System.Web.HttpUtility.UrlEncode(js));
                }
                else
                {
                    if (sb.Length != 0)
                        sb.Append("&");
                    sb.Append(System.Web.HttpUtility.UrlEncode(kvp.Key));

                    pvalue = kvp.Value?.ToString() ?? string.Empty;
                    if (!String.IsNullOrEmpty(pvalue))
                    {
                        sb.Append("=");
                        sb.Append(System.Web.HttpUtility.UrlEncode(pvalue));
                    }
                }
            }

            return sb.ToString();
        }
    }
}
