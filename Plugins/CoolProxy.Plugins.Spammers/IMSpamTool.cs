using CoolProxy.Plugins.ToolBox;
using OpenMetaverse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CoolProxy.Plugins.Spammers
{
    public class IMSpamTool : ToolBoxControl
    {
        private CoolProxyFrame Proxy;

        private UUID Target = UUID.Zero;
        private string SpamMessage;

        public IMSpamTool(CoolProxyFrame frame)
        {
            ID = "IM_SPAMER";

            Proxy = frame;

            Timer timer = new Timer();
            timer.Interval = 1000;
            timer.Tick += (x, y) =>
            {
                Proxy.Agent.InstantMessage(Target, SpamMessage, UUID.Random());
            };

            Label tlabel = new Label();
            tlabel.Location = new System.Drawing.Point(5, 1);
            //tlabel.Size = new System.Drawing.Size(60, 18);
            tlabel.AutoSize = true;
            tlabel.Text = "Instant Message Spammer";

            TextBox username_textbox = new TextBox();
            username_textbox.Size = new System.Drawing.Size(160, 42);
            username_textbox.Location = new System.Drawing.Point(5, 21);
            username_textbox.Text = "(nobody)";
            username_textbox.ReadOnly = true;


            Button button = new Button();
            button.Size = new System.Drawing.Size(55, 22);
            button.Location = new System.Drawing.Point(170, 20);
            button.Text = "Set";

            TextBox uuid_textbox = new TextBox();
            uuid_textbox.Size = new System.Drawing.Size(220, 22);
            uuid_textbox.Location = new System.Drawing.Point(5, 48);
            uuid_textbox.Text = "o.o";
            //uuid_textbox.ReadOnly = true;


            Label label = new Label();
            label.Location = new System.Drawing.Point(10, 77);
            //label.Size = new System.Drawing.Size(60, 18);
            label.AutoSize = true;
            label.Text = "Rate ms:";

            NumericUpDown spinner = new NumericUpDown();
            spinner.Size = new System.Drawing.Size(65, 22);
            spinner.Location = new System.Drawing.Point(60, 72);
            spinner.Value = 100;

            CheckBox enable = new CheckBox();
            enable.Location = new System.Drawing.Point(130, 72);
            enable.Text = "Enable";
            enable.Appearance = Appearance.Button;
            enable.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            enable.Size = new System.Drawing.Size(95, 20);
            enable.Enabled = false;

            enable.CheckedChanged += (x, y) =>
            {
                if(enable.Checked)
                {
                    SpamMessage = uuid_textbox.Text;
                    timer.Interval = (int)spinner.Value;
                }

                button.Enabled = !enable.Checked;
                uuid_textbox.Enabled = !enable.Checked;
                spinner.Enabled = !enable.Checked;
                timer.Enabled = enable.Checked;
            };

            button.Click += (x, y) =>
            {
                AvatarPickerSearchForm aps = new AvatarPickerSearchForm();
                if (aps.ShowDialog() == DialogResult.OK)
                {
                    Target = aps.SelectedID;
                    username_textbox.Text = aps.SelectedName;
                    enable.Enabled = true;
                }
            };


            this.Size = new System.Drawing.Size(230, 95);

            this.Controls.AddRange(new Control[]
            {
                tlabel,
                username_textbox,
                button,

                uuid_textbox,

                label,
                spinner,

                enable
            });
        }
    }
}
