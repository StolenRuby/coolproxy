using OpenMetaverse;
using OpenMetaverse.StructuredData;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CoolProxy
{
    public delegate void SettingChangedEventHandler(object source, SettingChangedEventArgs e);

    //This is a class which describes the event to the class that recieves it.
    //An EventArgs class must always derive from System.EventArgs.
    public class SettingChangedEventArgs : EventArgs
    {
        public string Setting;
        public string Type;
        public object Value;

        public SettingChangedEventArgs(string setting, string type, object value)
        {
            this.Setting = setting;
            this.Type = type;
            this.Value = value;
        }
    }
    
    public class Setting
    {
        public event SettingChangedEventHandler OnChanged;

        internal string Name
        { get; }

        internal string Comment
        { get; }

        internal string Type
        { get; }

        private object mObject;

        public object Value
        {
            get { return mObject; }
            set
            {
                mObject = value;
                if(OnChanged != null)
                {
                    OnChanged(null, new SettingChangedEventArgs(Name, Type, mObject));
                }
            }
        }

        internal Setting(string name, string type, object value, string comment = "")
        {
            this.Name = name;
            this.Type = type;
            this.Value = value;
            this.Comment = comment;
        }
    }

    public class SettingsManager
    {
        private Dictionary<string, Setting> Settings = new Dictionary<string, Setting>();

        public SettingsManager()
        {
            LoadFile();
        }

        ~SettingsManager()
        {
            SaveFile();
        }

        public void LoadFile()
        {
            Settings.Clear();

            string app_settings_dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "CoolProxy\\");

            if(!Directory.Exists(app_settings_dir))
            {
                Directory.CreateDirectory(app_settings_dir);
            }

            string settings_path = Path.Combine(app_settings_dir, "app_settings.xml");

            if (File.Exists(settings_path) == false)
            {
                settings_path = "./app_data/app_settings.xml";
            }

            byte[] data = File.ReadAllBytes(settings_path);

            OSDMap map = (OSDMap)OSDParser.DeserializeLLSDXml(data);

            foreach (string key in map.Keys)
            {
                OSDMap setting_osd = (OSDMap)map[key];


                if (setting_osd.ContainsKey("type") && setting_osd.ContainsKey("value"))
                {
                    string type = setting_osd["type"];

                    object value = null;

                    switch (type)
                    {
                        case "bool":
                            value = setting_osd["value"].AsBoolean();
                            break;
                        case "double":
                            value = setting_osd["value"].AsReal();
                            break;
                        case "color":
                            value = setting_osd["value"].AsColor4();
                            break;
                        case "string":
                            value = setting_osd["value"].AsString();
                            break;
                        case "integer":
                            value = setting_osd["value"].AsInteger();
                            break;
                        case "vector":
                            value = setting_osd["value"].AsVector3();
                            break;
                        case "quaternion":
                            value = setting_osd["value"].AsQuaternion();
                            break;
                        case "osd":
                            value = setting_osd["value"];
                            break;
                    }

                    string comment = string.Empty;

                    if (setting_osd.ContainsKey("comment"))
                        comment = setting_osd["comment"];

                    Setting setting = new Setting(key, type, value, comment);

                    if (value != null)
                    {
                        Settings.Add(key, setting);
                    }
                }
            }
        }

        public void SaveFile()
        {
            string app_data = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "CoolProxy\\");

            if (Directory.Exists(app_data) == false)
                Directory.CreateDirectory(app_data);

            var fileName = Path.Combine(app_data, "app_settings.xml");

            OSDMap settings = new OSDMap();

            foreach(KeyValuePair<string, Setting> pair in Settings)
            {
                Setting setting = pair.Value;

                OSDMap setting_osd = new OSDMap();
                setting_osd["comment"] = setting.Comment;
                setting_osd["type"] = setting.Type;

                switch(setting.Type)
                {
                    case "bool":
                        setting_osd["value"] = (bool)setting.Value;
                        break;
                    case "double":
                        setting_osd["value"] = (double)setting.Value;
                        break;
                    case "color":
                        setting_osd["value"] = (Color4)setting.Value;
                        break;
                    case "string":
                        setting_osd["value"] = (string)setting.Value;
                        break;
                    case "integer":
                        setting_osd["value"] = (int)setting.Value;
                        break;
                    case "vector":
                        setting_osd["value"] = (Vector3)setting.Value;
                        break;
                    case "quaternion":
                        setting_osd["value"] = (Quaternion)setting.Value;
                        break;
                    case "osd":
                        setting_osd["value"] = (OSD)setting.Value;
                        break;
                }

                settings[pair.Key] = setting_osd;
            }
            
            byte[] xml = OSDParser.SerializeLLSDXmlBytes(settings);
            
            if (File.Exists(fileName) == false)
            {
                FileStream fs = File.Create(fileName);
                fs.Close();
            }

            File.WriteAllBytes(fileName, xml);
        }

        object get(string name, string type)
        {
            var find = Settings.FirstOrDefault((x) => { return x.Key == name && x.Value.Type == type; });
            if (find.Key != null)
            {
                return find.Value?.Value;
            }
            return null;
        }

        public Setting getSetting(string name)
        {
            var find = Settings.FirstOrDefault((x) => { return x.Key == name; });
            if (find.Key != null)
            {
                return find.Value;
            }
            return default(Setting);
        }

        public Setting[] getSettings()
        {
            return Settings.Values.ToArray();
        }

        public bool getBool(string name)
        {
            return (bool)get(name, "bool");
        }

        public double getDouble(string name)
        {
            return (double)get(name, "double");
        }

        public int getInteger(string name)
        {
            return (int)get(name, "integer");
        }

        public Color4 getColor(string name)
        {
            return (Color4)get(name, "color");
        }

        public string getString(string name)
        {
            return (string)get(name, "string");
        }

        public Vector3 getVector(string name)
        {
            return (Vector3)get(name, "vector");
        }

        public Quaternion getQuaternion(string name)
        {
            return (Quaternion)get(name, "quaternion");
        }

        public OSD getOSD(string name)
        {
            return (OSD)get(name, "osd");
        }

        bool set(string name, string type, object value)
        {
            var find = Settings.FirstOrDefault((x) => { return x.Key == name && x.Value.Type == type; });
            if (find.Key != null)
            {
                Setting setting = find.Value;
                setting.Value = value;
                return true;
            }
            else return false;
        }

        public bool setBool(string name, bool value)
        {
            return set(name, "bool", value);
        }

        public bool setString(string name, string value)
        {
            return set(name, "string", value);
        }

        public bool setDouble(string name, double value)
        {
            return set(name, "double", value);
        }

        public bool setInteger(string name, int value)
        {
            return set(name, "integer", value);
        }

        public bool setOSD(string name, OSD value)
        {
            return set(name, "osd", value);
        }

        public bool setColor(string name, Color4 value)
        {
            return set(name, "color", value);
        }

        public bool setVector(string name, Vector3 value)
        {
            return set(name, "vector", value);
        }

        public bool setQuaternion(string name, Quaternion value)
        {
            return set(name, "quaternion", value);
        }
    }
}
