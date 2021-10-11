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
    public partial class AccountManagerForm : Form
    {
        public AccountManagerForm()
        {
            InitializeComponent();

            dataGridView1.Rows.Add("Second Life", "Stolen Ruby");
            dataGridView1.Rows.Add("Second Life Beta", "Stolen Ruby");
            dataGridView1.Rows.Add("Mobius Grid", "Ruby Mobian");
            dataGridView1.Rows.Add("Mobius Grid", "Stolen Ruby");
            dataGridView1.Rows.Add("Local Grid", "ribbon Dover");
            dataGridView1.Rows.Add("Local Grid", "Loser User");
        }

        private void dataGridView1_UserDeletingRow(object sender, DataGridViewRowCancelEventArgs e)
        {
            if (MessageBox.Show("Are you sure?", "Quest Chin", MessageBoxButtons.YesNo) == DialogResult.No)
            {
                e.Cancel = true;
            }
        }
    }
}
