using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using BridgePrivate;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace AdminApp
{
    public partial class Form1 : Form
    {
        public static List<Thread> threads = new List<Thread>();
        public log dlog;

        public Form1()
        {
            InitializeComponent();
            this.toolStrip1.Visible = false;
            this.ControlBox = false;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            l("Loading");

            new Thread(() => 
            {
                dlog = new log("base", "AdminDebugLog");

                this.Invoke((MethodInvoker)delegate()
                {
                    this.toolStrip1.Visible = true;
                });

                g("Load successful");

            }).Start();       
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            l("closer");
            this.toolStripButton1.Visible = false;
            this.ControlBox = true;
        }

        private void logTestToolStripMenuItem_Click(object sender, EventArgs e)
        {
            dlog.l("hi there");
            l("hi");
        }

        private void logTest2ToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        public void g(string message)
        {
            new Thread(() => gl(message)).Start();
            Thread.Sleep(10);
        }
        public void gl(string message)
        {
            this.Invoke((MethodInvoker)delegate()
            {
                DateTime mytime = DateTime.Now;

                this.richTextBox1.SelectionBackColor = Color.Black;
                this.richTextBox1.SelectionColor = Color.ForestGreen;
                this.richTextBox1.AppendText("\n[" + mytime + "] " + message);
            });
        }
        public void l(string message)
        {
            new Thread(() => lt(message)).Start();
            Thread.Sleep(10);
        }
        public void lt(string message)
        {
            this.Invoke((MethodInvoker)delegate()
            {
                DateTime mytime = DateTime.Now;

                this.richTextBox1.SelectionBackColor = Color.Black;
                this.richTextBox1.SelectionColor = Color.White;
                this.richTextBox1.AppendText("\n[" + mytime + "] " + message);
            });
        }
        public void el(string message)
        {
            new Thread(() => elt(message)).Start();
            Thread.Sleep(10);
        }
        public void elt(string message)
        {
            this.Invoke((MethodInvoker)delegate()
            {
                DateTime mytime = DateTime.Now;

                this.richTextBox1.SelectionBackColor = Color.Black;
                this.richTextBox1.SelectionColor = Color.Firebrick;
                this.richTextBox1.AppendText("\n[" + mytime + "] " + message);
            });
        }
        public void ex(string message)
        {
            new Thread(() => ext(message)).Start();
            Thread.Sleep(10);
        }
        public void ext(string message)
        {
            this.Invoke((MethodInvoker)delegate()
            {
                DateTime mytime = DateTime.Now;

                this.richTextBox1.SelectionBackColor = Color.Black;
                this.richTextBox1.SelectionColor = Color.Goldenrod;
                this.richTextBox1.AppendText("\n[" + mytime + "] " + message);
            });
        }
        public void n(string message)
        {
            new Thread(() => nt(message)).Start();
        }
        public void nt(string message)
        {
            this.Invoke((MethodInvoker)delegate()
            {
                this.richTextBox1.SelectionBackColor = Color.Black;
                this.richTextBox1.SelectionColor = Color.White;
                this.richTextBox1.AppendText(message);
            });
        }
    }
}
