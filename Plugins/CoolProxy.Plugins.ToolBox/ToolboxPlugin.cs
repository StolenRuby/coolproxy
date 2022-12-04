using CoolProxy.Plugins.ToolBox.Components;
using OpenMetaverse;
using OpenMetaverse.StructuredData;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CoolProxy.Plugins.ToolBox
{
    public interface IToolBox
    {
        void AddTool(ToolBoxControl control);
        ToolBoxControl[] GetToolbx();
    };

    public class ToolboxPlugin : CoolProxyPlugin, IToolBox
    {
        private CoolProxyFrame Proxy;

        private ToolBoxForm ToolBox;

        public ToolboxPlugin(CoolProxyFrame frame)
        {
            Proxy = frame;

            var gui = Proxy.RequestModuleInterface<IGUI>();

            ToolBox = new ToolBoxForm(this);

            MenuOption menuOption = new MenuOption("TOGGLE_TOOLBOX", "ToolBox", true)
            {
                Checked = (x) => ToolBox.Visible,
                Clicked = (x) =>
                {
                    if (ToolBox.Visible)
                        ToolBox.Hide();
                    else
                        ToolBox.Show();
                }
            };

            gui.AddMainMenuOption(menuOption);

            Proxy.RegisterModuleInterface<IToolBox>(this);
        }

        internal void ApplyToolBox(ToolBoxControl[] tools)
        {
            ToolsInUse = tools.ToList();
            ToolBox.ClearTools();
            PopulateToolbox();
            SaveTools();
        }

        internal void UpdateToolbox()
        {
            LoadTools();
            AppendNewTools();
            PopulateToolbox();
            SaveTools();
        }

        private void PopulateToolbox()
        {
            foreach(var tool in ToolsInUse)
            {
                ToolBox.AddToolboxItem(tool);
            }
        }

        List<string> KnownOptions = new List<string>();

        Dictionary<string, ToolBoxControl> Tools = new Dictionary<string, ToolBoxControl>();

        public ToolBoxControl[] GetAvailable()
        {
            return Tools.Values.ToArray();
        }

        public void AddTool(ToolBoxControl control)
        {
            Tools.Add(control.ID, control);
        }

        List<ToolBoxControl> ToolsInUse = new List<ToolBoxControl>();

        public ToolBoxControl[] GetToolbx()
        {
            return ToolsInUse.ToArray();
        }

        void LoadTools()
        {
            ToolsInUse.Clear();

            string app_settings_dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "CoolProxy\\");
            string filename = Path.Combine(app_settings_dir, "toolbox.xml");

            if (File.Exists(filename))
            {
                byte[] xml = File.ReadAllBytes(filename);
                OSD osd = OSDParser.DeserializeLLSDXml(xml);
                OSDMap map = (OSDMap)osd;

                OSDArray known = (OSDArray)map["known"];
                OSDArray tools = (OSDArray)map["tools"];
                //OSDMap root = (OSDMap)map["root"];

                KnownOptions = known.Select(x => x.AsString()).ToList();
                

                foreach(OSD tool_osd in tools)
                {
                    OSDMap tool = tool_osd as OSDMap;
                    string type = tool["type"];

                    if(type == "separator")
                    {
                        ToolsInUse.Add(new SimpleSeparator());
                    }
                    else if(type == "label")
                    {
                        string text = tool["text"];
                        ToolsInUse.Add(new SimpleLabel(text));
                    }
                    else if(type == "option")
                    {
                        string name = tool["name"];
                        if(Tools.TryGetValue(name, out var o))
                        {
                            ToolsInUse.Add(o);
                        }
                    }
                }

            }
        }

        void SaveTools()
        {
            OSDArray known = new OSDArray();
            foreach (var opt in KnownOptions)
            {
                known.Add(opt);
            }

            OSDMap map = new OSDMap();
            map["known"] = known;
            map["tools"] = ToolsToArray();

            string app_settings_dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "CoolProxy\\");
            string filename = Path.Combine(app_settings_dir, "toolbox.xml");

            byte[] xml = OSDParser.SerializeLLSDXmlToBytes((OSD)map);
            File.WriteAllBytes(filename, xml);
        }

        private OSDArray ToolsToArray()
        {
            OSDArray array = new OSDArray();

            foreach(var tool in ToolsInUse)
            {
                OSDMap map = new OSDMap();
                if(tool is SimpleSeparator)
                {
                    map["type"] = "separator";
                }
                else if(tool is SimpleLabel)
                {
                    map["type"] = "label";
                    map["text"] = (tool as SimpleLabel).Label;
                }
                else
                {
                    map["type"] = "option";
                    map["name"] = tool.ID;
                }

                array.Add(map);
            }

            return array;
        }

        void AppendNewTools()
        {
            foreach(var tool in Tools.Values)
            {
                if(tool.Default && !KnownOptions.Contains(tool.ID))
                {
                    KnownOptions.Add(tool.ID);
                    ToolsInUse.Add(tool);
                }
            }
        }
    }
}
