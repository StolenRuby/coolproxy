using CoolProxy.Plugins.ToolBox;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CoolProxy.Plugins.Spammers
{
    public class TouchSpammerTool : ToolBoxControl
    {
        private CoolProxyFrame Proxy;

        private uint TargetLocalID = 0;

        public TouchSpammerTool(CoolProxyFrame proxy)
        {
            ID = "TOUCH_SPAMMER_TOOL";

            Proxy = proxy;

            this.Size = new Size(230, 72);

            Button button = new Button();
            button.Location = new Point(5, 1);
            button.Size = new Size(40, 40);
            button.Image = Properties.Resources.Inv_Object;

            Label label = new Label();
            label.Location = new Point(50, 5);
            label.Size = new Size(100, 18);
            label.Text = "Touch Spammer";

            Label tlabel = new Label();
            tlabel.Location = new Point(50, 23);
            tlabel.Size = new Size(100, 18);
            tlabel.Text = "No Target";

            Timer timer = new Timer();
            timer.Interval = 100;
            timer.Tick += (x, y) =>
            {
                Proxy.Agent.Touch(TargetLocalID);
            };

            CheckBox ebutton = new CheckBox();
            ebutton.Location = new Point(5, 50);
            ebutton.Size = new Size(220, 22);
            ebutton.Text = "Enable Touch Spam";
            ebutton.Appearance = Appearance.Button;
            ebutton.TextAlign = ContentAlignment.MiddleCenter;
            ebutton.CheckedChanged += (x, y) =>
            {
                timer.Enabled = ebutton.Checked;
            };


            button.Click += (x, y) =>
            {
                if (Proxy.Agent.Selection.Length == 1)
                {
                    uint id = Proxy.Agent.Selection[0];
                    TargetLocalID = id;
                    tlabel.Text = TargetLocalID.ToString();
                    ebutton.Enabled = true;
                }
            };


            this.Controls.AddRange(new Control[]
            {
                button,
                label,
                tlabel,
                ebutton
            });
        }
    }
}
