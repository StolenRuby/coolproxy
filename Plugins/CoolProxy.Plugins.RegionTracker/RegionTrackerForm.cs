using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static GridProxy.RegionManager;

namespace CoolProxy.Plugins.RegionTracker
{
    public partial class RegionTrackerForm : Form
    {
        private CoolProxyFrame Proxy;

        public RegionTrackerForm(CoolProxyFrame frame)
        {
            Proxy = frame;
            InitializeComponent();

            Proxy.Network.OnHandshake += onHandshake;
            Proxy.Network.OnNewRegion += onNewRegion;

            regionsDataGridView.DoubleBuffered(true);

            var region_tracker_timer = new System.Windows.Forms.Timer();
            region_tracker_timer.Tick += Region_tracker_timer_Tick;
            region_tracker_timer.Interval = 1000;
            region_tracker_timer.Start();

        }

        private void Region_tracker_timer_Tick(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in regionsDataGridView.Rows)
            {
                RegionProxy region = (RegionProxy)row.Tag;
                row.Cells[0].Style.ForeColor = region.Connected ? region == Proxy.Network.CurrentSim ? Color.Green : Color.Black : Color.Maroon;
                row.Cells[2].Value = region.Connected ? region.AvatarPositions.Count.ToString() : "?";
            }
        }


        private void onHandshake(RegionProxy proxy)
        {
            if (regionsDataGridView.InvokeRequired) regionsDataGridView.BeginInvoke(new Action(() => onHandshake(proxy)));
            else
            {
                foreach (DataGridViewRow row in regionsDataGridView.Rows)
                {
                    //if((string)row.Cells[1].Value == proxy.RemoteEndPoint.ToString())
                    if ((RegionProxy)row.Tag == proxy)
                    {
                        row.Cells[0].Value = proxy.Name;
                        return;
                    }
                }
            }
        }

        private void onNewRegion(RegionProxy proxy)
        {
            AddRegion(proxy);
        }

        void AddRegion(RegionProxy proxy)
        {
            if (regionsDataGridView.InvokeRequired) regionsDataGridView.BeginInvoke(new Action(() => AddRegion(proxy)));
            else
            {
                int r = regionsDataGridView.Rows.Add("???", proxy.RemoteEndPoint.ToString(), "?");
                regionsDataGridView.Rows[r].Tag = proxy;
            }
        }





        private void copyRegionIPToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (regionsDataGridView.SelectedRows.Count == 1)
            {
                RegionProxy region = (RegionProxy)regionsDataGridView.SelectedRows[0].Tag;
                Clipboard.SetText(region.RemoteEndPoint.ToString());
            }
        }

        private void copyRegionIDToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (regionsDataGridView.SelectedRows.Count == 1)
            {
                RegionProxy region = (RegionProxy)regionsDataGridView.SelectedRows[0].Tag;
                Clipboard.SetText(region.ID.ToString());
            }
        }

        private void regionsContextMenu_Opening(object sender, CancelEventArgs e)
        {
            if (regionsDataGridView.SelectedRows.Count == 0)
            {
                e.Cancel = true;
                return;
            }

            var region = (RegionProxy)regionsDataGridView.SelectedRows[0].Tag;
        }

    }
}
