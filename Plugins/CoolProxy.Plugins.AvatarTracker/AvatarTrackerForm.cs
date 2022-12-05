using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CoolProxy.Plugins.AvatarTracker
{
    public partial class AvatarTrackerForm : Form
    {
        private CoolProxyFrame Proxy;

        private AvTrackerTest Tracker;

        public AvatarTrackerForm(CoolProxyFrame frame)
        {
            Proxy = frame;
            InitializeComponent();

            Tracker = new AvTrackerTest(avatarTrackerGridView, Proxy);
        }

        internal void AddSingleMenuItem(string label, HandleAvatarPicker handle)
        {
            Tracker.AddSingleMenuItem(label, handle);
        }

        internal void AddMultipleMenuItem(string label, HandleAvatarPickerList handle)
        {
            Tracker.AddMultipleMenuItem(label, handle);
        }
    }
}
