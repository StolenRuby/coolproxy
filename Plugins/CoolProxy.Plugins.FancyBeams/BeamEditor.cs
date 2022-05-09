using OpenMetaverse;
using OpenMetaverse.StructuredData;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CoolProxy.Plugins.FancyBeams
{
    public partial class BeamEditor : Form
    {
        public BeamEditor()
        {
            InitializeComponent();
        }

        List<Tuple<PointF, Color>> Points = new List<Tuple<PointF, Color>>();

        Size DotSize = new Size(10, 10);

        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            PointF center = new PointF(panel1.Width / 2.0f, panel1.Height / 2.0f);

            e.Graphics.DrawEllipse(Pens.White, new RectangleF(center.X - 2, center.Y - 2, 4.0f, 4.0f));
            e.Graphics.DrawEllipse(Pens.White, new RectangleF(center.X - 50, center.Y - 50, 100.0f, 100.0f));
            e.Graphics.DrawEllipse(Pens.White, new RectangleF(center.X - 100, center.Y - 100, 200.0f, 200.0f));

            foreach(var point in Points)
            {
                var p = point.Item1;
                p.X -= 5;
                p.Y -= 5;

                p.X += center.X;
                p.Y += center.Y;

                var rect = new RectangleF(p, DotSize);

                using(Brush brush = new SolidBrush(point.Item2))
                {
                    e.Graphics.FillEllipse(brush, rect);
                    e.Graphics.DrawEllipse(Pens.White, rect);
                }
            }
        }

        private void panel1_MouseClick(object sender, MouseEventArgs e)
        {
            PointF center = new PointF(panel1.Width / 2.0f, panel1.Height / 2.0f);

            if (e.Button == MouseButtons.Left)
            {
                PointF pos = e.Location;
                pos.X -= center.X;
                pos.Y -= center.Y;

                Points.Add(new Tuple<PointF, Color>(pos, pictureBox1.BackColor));
                panel1.Refresh();
            }
            else if(e.Button == MouseButtons.Right)
            {
                for (int iter = Points.Count - 1; iter >= 0; iter--)
                {
                    var point = Points[iter].Item1;
                    point.X += center.X;
                    point.Y += center.Y;
                    point.X -= 5;
                    point.Y -= 5;

                    var rect = new RectangleF(point, DotSize);

                    if (rect.Contains(e.Location))
                    {
                        Points.RemoveAt(iter);
                    }
                }

                panel1.Refresh();
            }
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            if(colorDialog1.ShowDialog(this) == DialogResult.OK)
            {
                pictureBox1.BackColor = colorDialog1.Color;
            }
        }

        private void clearButton_Click(object sender, EventArgs e)
        {
            Points.Clear();
            panel1.Refresh();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            using(SaveFileDialog dialog = new SaveFileDialog())
            {
                dialog.InitialDirectory = FancyBeamsPlugin.BeamsFolderDir;
                dialog.Filter = "XML Files|*.xml";
                if(dialog.ShowDialog(this) == DialogResult.OK)
                {
                    OSDArray array = new OSDArray();

                    foreach (var truple in Points)
                    {
                        OSDMap map = new OSDMap();

                        var point = truple.Item1;

                        float x = 0.005f * point.X;
                        float y = 0.005f * point.Y;

                        Color colour = truple.Item2;

                        Vector3 pos = new Vector3(0, x, y);
                        Color4 colour4 = new Color4(colour.R, colour.G, colour.B, colour.A);

                        map["offset"] = pos;
                        map["colour"] = OSD.FromColor4(colour4);

                        array.Add(map);
                    }

                    byte[] data = OSDParser.SerializeLLSDXmlBytes(array);

                    File.WriteAllBytes(dialog.FileName, data);

                    string name = Path.GetFileNameWithoutExtension(dialog.FileName);
                    FancyBeamsPlugin.BeamSettingsPanel.ReloadBeams();
                    FancyBeamsPlugin.Proxy.Settings.setString("BeamShape", name);
                }
            }
        }

        Color Color4ToColor(Color4 color4)
        {
            return Color.FromArgb((int)(255 * color4.R), (int)(255 * color4.G), (int)(255 * color4.B));
        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void loadButton_Click(object sender, EventArgs e)
        {
            using(OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.InitialDirectory = FancyBeamsPlugin.BeamsFolderDir;
                dialog.Filter = "XML Files|*.xml";

                if(dialog.ShowDialog(this) == DialogResult.OK)
                {
                    Points.Clear();

                    byte[] data = File.ReadAllBytes(dialog.FileName);
                    OSD osd = OSDParser.DeserializeLLSDXml(data);
                    OSDArray array = (OSDArray)osd;
                    foreach(var entry in array)
                    {
                        OSDMap map = (OSDMap)entry;

                        Color4 colour = map["colour"].AsColor4();
                        Vector3 offset = map["offset"].AsVector3();

                        float x = offset.Y * 200;
                        float y = offset.Z * 200;

                        Points.Add(new Tuple<PointF, Color>(new PointF(x, y), Color4ToColor(colour)));
                    }

                    panel1.Refresh();
                }
            }
        }
    }
}
