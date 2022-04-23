using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CoolProxy.Plugins.FancyBeams
{
    public class BeamSettingsPanel : Panel
    {
        private SettingsManager Settings;

        public BeamSettingsPanel(SettingsManager settings) : base()
        {
            Settings = settings;

            InitGUI();

            ReloadBeams();
        }

        public void ReloadBeams()
        {
            comboBox.SelectedIndexChanged -= ComboBox_SelectedIndexChanged;

            comboBox.Items.Clear();

            comboBox.Items.Add("=== Off ===");
            string[] files = Directory.GetFiles(FancyBeamsPlugin.BeamsFolderDir, "*.xml");
            foreach (string file in files)
            {
                string name = Path.GetFileNameWithoutExtension(file);
                comboBox.Items.Add(name);
            }

            string selected = Settings.getString("BeamShape");

            if (selected == string.Empty)
                comboBox.SelectedIndex = 0;
            else
                comboBox.SelectedItem = selected;

            if (comboBox.SelectedItem == null)
                comboBox.SelectedIndex = 0;

            delete_button.Enabled = comboBox.SelectedIndex > 0;

            comboBox.SelectedIndexChanged += ComboBox_SelectedIndexChanged;
        }

        void ComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            string save = string.Empty;
            if (comboBox.SelectedIndex != 0)
                save = (string)comboBox.SelectedItem;
            Settings.setString("BeamShape", save);

            delete_button.Enabled = comboBox.SelectedIndex > 0;
        }

        ComboBox comboBox;
        Button delete_button;

        private void InitGUI()
        {
            this.Dock = DockStyle.Fill;

            comboBox = new ComboBox();
            comboBox.Location = new Point(15, 15);
            comboBox.Size = new Size(150, 22);
            comboBox.DropDownStyle = ComboBoxStyle.DropDownList;

            Settings.getSetting("BeamShape").OnChanged += (x, y) =>
            {
                string change = (string)y.Value;
                if (change == string.Empty)
                    comboBox.SelectedIndex = 0;
                else
                    comboBox.SelectedItem = change;
            };

            comboBox.SelectedIndexChanged += ComboBox_SelectedIndexChanged;

            this.Controls.Add(comboBox);

            delete_button = new Button();
            delete_button.Text = "Delete";
            delete_button.Location = new Point(175, 15);
            delete_button.Size = new Size(80, 22);
            delete_button.Click += Delete_button_Click;

            this.Controls.Add(delete_button);

            Button editor_button = new Button();
            editor_button.Text = "Create New";
            editor_button.Location = new Point(175, 45);
            editor_button.Size = new Size(80, 22);
            editor_button.Click += (s, e) =>
            {
                var editor = new BeamEditor();
                editor.TopMost = Settings.getBool("KeepCoolProxyOnTop");
                Settings.getSetting("KeepCoolProxyOnTop").OnChanged += (x, y) => { editor.TopMost = (bool)y.Value; };
                editor.Show();
            };

            this.Controls.Add(editor_button);

            Button refresh_button = new Button();
            refresh_button.Text = "Refresh List";
            refresh_button.Location = new Point(175, 75);
            refresh_button.Size = new Size(80, 22);
            refresh_button.Click += Refresh_button_Click;

            this.Controls.Add(refresh_button);

            Label label = new Label();
            label.Text = "Scale:";
            label.AutoSize = true;
            label.Location = new Point(15, 48);
            this.Controls.Add(label);

            TrackBar trackBar = new TrackBar();
            trackBar.Location = new Point(50, 45);
            trackBar.Size = new Size(90, 22);
            trackBar.Minimum = 1;
            trackBar.Maximum = 30;
            trackBar.Value = 12;
            trackBar.TickStyle = TickStyle.None;
            this.Controls.Add(trackBar);


            Label scale = new Label();
            scale.Text = "?x";
            scale.AutoSize = true;
            scale.Location = new Point(140, 48);
            this.Controls.Add(scale);

            trackBar.ValueChanged += (x, y) =>
            {
                double val = (0.1 * trackBar.Value);
                Settings.setDouble("BeamScale", val);
                scale.Text = (val).ToString() + "x";
            };

            trackBar.Value = (int)(10.0f * Settings.getDouble("BeamScale"));

            var checkbox = new CoolGUI.Controls.CheckBox();
            checkbox.AutoSize = true;
            checkbox.Location = new Point(20, 105);
            checkbox.Setting = "RainbowSelectionBeam";
            checkbox.Text = "Rainbow Selection Beam";
            this.Controls.Add(checkbox);
            checkbox.BringToFront();

            checkbox = new CoolGUI.Controls.CheckBox();
            checkbox.AutoSize = true;
            checkbox.Location = new Point(20, 80);
            checkbox.Setting = "RotateShapedBeam";
            checkbox.Text = "Rotate Selection Beam";
            this.Controls.Add(checkbox);
            checkbox.BringToFront();
        }

        private void Delete_button_Click(object sender, EventArgs e)
        {
            string file = FancyBeamsPlugin.BeamsFolderDir + (string)comboBox.SelectedItem + ".xml";
            if(File.Exists(file))
            {
                File.Delete(file);
                ReloadBeams();
                Settings.setString("BeamShape", string.Empty);
            }
        }

        private void Refresh_button_Click(object sender, EventArgs e)
        {
            ReloadBeams();
        }
    }
}
