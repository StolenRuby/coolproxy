using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CoolProxy
{
    public partial class ListDialogBox : Form
    {
        public string SelectedOption { get; set; }

        public ListDialogBox(string[] options)
        {
            InitializeComponent();
            listBox1.Items.AddRange(options);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            SelectedOption = listBox1.SelectedItem.ToString();
            DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
