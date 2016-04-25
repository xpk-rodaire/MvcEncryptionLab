using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FipsCheckerUtility
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        public string DisplayText
        {
            get
            {
                return this.txtDisplayText.Text;
            }

            set
            {
                this.txtDisplayText.Text = value;
            }
        }

        private void txtDisplayText_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
