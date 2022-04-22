using OpenMetaverse;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CoolProxy.Plugins.SuperSuitcase
{
    public partial class SuperSuitcaseForm : Form
    {
        private InventoryFolder Root;

        private CoolProxyFrame Frame;

        private InventoryFolder Current;

        private List<InventoryFolder> Path = new List<InventoryFolder>();

        public SuperSuitcaseForm(CoolProxyFrame frame, InventoryFolder folder)
        {
            Frame = frame;
            Root = folder;
            InitializeComponent();

            frame.Avatars.GetDisplayNames(new List<UUID> { folder.OwnerID }, (success, good, bad) =>
            {
                string name = folder.OwnerID.ToString();

                if(success)
                {
                    var avatar = good.FirstOrDefault(adn => adn.ID == folder.OwnerID);
                    if(avatar != default)
                    {
                        name = avatar.UserName;
                    }
                }

                this.Invoke(new Action(() =>
                {
                    this.Text = "Super Suitcase - " + name;
                }));
            });

            LoadFolder(Root);
        }

        void LoadFolder(InventoryFolder folder)
        {
            Current = folder;
            invDGV.Rows.Clear();

            Frame.OpenSim.XInventory.GetFolderContent(folder.OwnerID, folder.UUID, (folders, items) =>
            {
                invDGV.Invoke(new Action(() =>
                {
                    if (folder.UUID != Root.UUID)
                    {
                        invDGV.Rows.Add("up", "<..>");
                    }

                    var sf = folders.OrderBy(x => x.Name);
                    var si = items.OrderBy(x => x.Name);

                    foreach(var fol in sf)
                    {
                        if(fol != null)
                        {
                            int i = invDGV.Rows.Add("folder", "<" + fol.Name + ">");
                            invDGV.Rows[i].Tag = fol;
                        }
                    }

                    foreach(var inv in si)
                    {
                        if (inv != null)
                        {
                            int i = invDGV.Rows.Add("item", inv.Name + "." + inv.InventoryType.ToString().ToLower());
                            invDGV.Rows[i].Tag = inv;
                        }
                    }
                }));
            });
        }

        private void invDGV_DoubleClick(object sender, EventArgs e)
        {
            if (invDGV.SelectedRows.Count < 1) return;

            var row = invDGV.SelectedRows[0];

            string type = row.Cells[0].Value as string;

            if (type == "up")
            {
                InventoryFolder folder = Path.Last();
                Path.Remove(folder);
                LoadFolder(folder);
            }
            else if (type == "folder")
            {
                var folder = row.Tag as InventoryFolder;
                Path.Add(Current);
                LoadFolder(folder);
            }
        }
    }
}
