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
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.WindowsAzure.Storage.Blob;
using BridgePrivate;
using BridgePublic;
using System.Diagnostics;
using System.ServiceModel;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using System.ServiceModel.Description;
using Microsoft.ServiceBus.Description;
using System.Configuration;
using Newtonsoft.Json;

namespace AdminApp
{
    public partial class Form1 : Form
    {
        // housekeeping
        public static List<Thread> threads = new List<Thread>();
        public tokens tokens = new tokens();
        private KeyValuePair<string, CancellationToken> maintoken;

        // debug log
        public log dlog;

        // stuff to emulate bridgeservice
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        public bool suspendBridge = false;

        public Form1()
        {
            InitializeComponent();
            this.toolStrip1.Visible = false;
            this.ControlBox = false;
            maintoken = tokens.add("main");
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

        private void logTestToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new Thread( async () => 
            {

                string cbc = await blobserver.GetLock("base", "testcon3", "eddie3");
                if (cbc == null)
                    el("null");
                else g(cbc);

                bool ret = await blobserver.ReleaseLock("base", "testcon3", "eddie3", cbc);
                if (ret)
                    g("good");
                else el("still locked");

            }).Start();
        }

        private void logTest2ToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            new Thread(() => Closer()).Start();
        }

        private void Closer()
        {
            try
            {
                tokens.cancelAll();
            }
            catch (Exception ex1)
            {
                ex(System.Reflection.MethodBase.GetCurrentMethod().Name + " " + ex1.ToString());
            }

            try
            {
                this.cancellationTokenSource.Cancel();
            }
            catch (Exception ex1)
            {
                ex(System.Reflection.MethodBase.GetCurrentMethod().Name + " " + ex1.ToString());
            }

            this.Invoke((MethodInvoker)delegate()
            {
                toolStripDropDownButton1.Enabled = false;
                toolStripDropDownButton1.Visible = false;

                toolStrip1.Enabled = false;
                toolStrip1.Hide();
                panel1.Hide();
                richTextBox1.Dock = DockStyle.Fill;
            });

            Stopwatch watch = new Stopwatch();
            watch.Start();

            while (true)
            {
                bool done = true;
                foreach (Thread thr in threads)
                {
                    if (thr.IsAlive)
                    {
                        done = false;

                        if (watch.Elapsed.TotalSeconds > 5)
                            l("waiting for thread: " + thr.Name);
                    }
                }

                if (watch.Elapsed.TotalSeconds > 5)
                    watch.Restart();

                if (done)
                    break;
                else
                {
                    this.Invoke((MethodInvoker)delegate()
                    {
                        this.Refresh();
                    });
                }

                Thread.Sleep(500);
            }

            watch.Stop();

            g("Ready to exit");

            this.Invoke((MethodInvoker)delegate()
            {
                this.ControlBox = true;
            });
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

        private void startToolStripMenuItem_Click(object sender, EventArgs e)
        {
            l("Starting service");
            threads.Add(new Thread(() => RunAsync(this.cancellationTokenSource.Token)));
            threads.Last().Name = "RunAsync";
            threads.Last().Start();
        }

        private void RunAsync(CancellationToken cancellationToken)
        {
            try
            {                
                while (!cancellationToken.IsCancellationRequested)
                {
                    l("Setting up service host");

                    ServiceBusEnvironment.SystemConnectivity.Mode = ConnectivityMode.AutoDetect;

                    TransportClientEndpointBehavior sasCredential = new TransportClientEndpointBehavior();
                    sasCredential.TokenProvider = TokenProvider.CreateSharedAccessSignatureTokenProvider("RootManageSharedAccessKey", ConfigurationManager.AppSettings["sasKey"]);

                    Uri address = ServiceBusEnvironment.CreateServiceUri("sb", ConfigurationManager.AppSettings["serviceNamespace"], "BridgeService");

                    ServiceHost host = new ServiceHost(typeof(BridgeService), address);

                    IEndpointBehavior serviceRegistrySettings = new ServiceRegistrySettings(DiscoveryType.Private);

                    foreach (ServiceEndpoint endpoint in host.Description.Endpoints)
                    {
                        endpoint.Behaviors.Add(serviceRegistrySettings);
                        endpoint.Behaviors.Add(sasCredential);
                    }

                    host.Open();

                    g("Running service host");
                    while (!cancellationToken.IsCancellationRequested && !suspendBridge)
                    {
                        Thread.Sleep(1000);
                    }

                    host.Close();

                    if (suspendBridge)
                    {
                        el("BridgeService suspended");

                        while (suspendBridge && !cancellationToken.IsCancellationRequested)
                        {
                            Thread.Sleep(1000);
                        }

                        if (!suspendBridge)
                            l("BridgeService resuming");
                    }
                }

                el("BusRole cancellation requested");
            }
            catch (Exception ex1)
            {
                ex(ex1.Message + " " + ex1.ToString());
            }
        }

        private void stopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.cancellationTokenSource.Cancel();
        }
    }
}
