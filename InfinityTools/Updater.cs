using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace InfinityTools
{
    public partial class Updater : Form
    {
        public Updater()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // test strings
            bool good = true;
            if (!textBox1.Text.All(char.IsLetterOrDigit))
            {
                textBox1.Text = "value must be alphanumberic";
                good = false;
            }

            if (!textBox2.Text.All(char.IsLetterOrDigit))
            {
                textBox2.Text = "value must be alphanumberic";
                good = false;
            }

            if (good)
            {
                Properties.Settings.Default.webus = textBox1.Text;
                Properties.Settings.Default.webtok = textBox2.Text;
                Properties.Settings.Default.Save();
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void Updater_Load(object sender, EventArgs e)
        {
            textBox1.Text = Properties.Settings.Default.webus;
            textBox2.Text = Properties.Settings.Default.webtok;
        }
    }
}
