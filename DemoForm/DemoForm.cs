using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using GMD.DIO.Ctrl;

namespace DemoForm
{
    public partial class DemoForm : Form
    {
        private delegate void OutText(string line);
        private DIO8_MCP22_Manager _Manager = new DIO8_MCP22_Manager();
        private bool DemoMarch=false, DemoInput = false, DemoOutput = false;

        public DemoForm()
        {
            InitializeComponent();
        }
    }
}
