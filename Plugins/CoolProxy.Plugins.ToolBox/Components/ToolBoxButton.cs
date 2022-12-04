using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CoolProxy.Plugins.ToolBox.Components
{
    public partial class ToolBoxButton : ToolBoxControl
    {
        public ToolBoxButton(string label)
        {
            InitializeComponent();

            button1.Text = label;
        }
    }
}
