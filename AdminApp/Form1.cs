﻿using System;
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
        private CloudStorageAccount storageAccount = null;
        private static PublicStore ps;
        public static List<Thread> threads = new List<Thread>();
        public tokens tokens = new tokens();
        private KeyValuePair<string, CancellationToken> maintoken;
        public static SubscriptionClient Client = SubscriptionClient.CreateFromConnectionString(ConfigurationManager.AppSettings["Microsoft.ServiceBus.ConnectionString"], "AdminTopic", "AllMessages");
        public static OnMessageOptions options = new OnMessageOptions();

        // debug log
        public static log dlog;

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
            options.AutoComplete = false;
            options.AutoRenewTimeout = TimeSpan.FromMinutes(1);

            Client.OnMessage((message) =>
            {
                try
                {
                    // Process message from subscription.
                    l("Echo: " + message.GetBody<string>());

                    // Remove message from subscription.
                    message.Complete();
                }
                catch (Exception)
                {
                    // Indicates a problem, unlock message in subscription.
                    message.Abandon();
                }
            }, options);

            new Thread(() => 
            {
                try
                {
                    l("Loading");
                    storageAccount = Microsoft.WindowsAzure.Storage.CloudStorageAccount.Parse(Properties.Settings.Default.storage);

                    ps = new PublicStore(storageAccount, "base");
                    
                    dlog = new log("base", "LogsAdminDebug", storageAccount);

                    this.Invoke((MethodInvoker)delegate()
                    {
                        this.toolStrip1.Visible = true;
                    });

                    g("Load successful");
                }
                catch (Exception ex1)
                {
                    ex(System.Reflection.MethodBase.GetCurrentMethod().Name + " " + ex1.ToString());
                }

            }).Start();       
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

        private void stopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.cancellationTokenSource.Cancel();
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
                l("Setting up AdminTopic subscription");

                TopicDescription td = new TopicDescription("AdminTopic");
                td.MaxSizeInMegabytes = 5120;
                td.DefaultMessageTimeToLive = new TimeSpan(0, 1, 0);

                var namespaceManager = NamespaceManager.CreateFromConnectionString(ConfigurationManager.AppSettings["Microsoft.ServiceBus.ConnectionString"]);
                if (!namespaceManager.TopicExists("AdminTopic"))
                    namespaceManager.CreateTopic(td);

                // change the subscription to instanceID!
                if (!namespaceManager.SubscriptionExists("AdminTopic", gvars.iid))
                    namespaceManager.CreateSubscription("AdminTopic", gvars.iid);

                // change the subscription to instanceID!          
                SubscriptionClient Client = SubscriptionClient.CreateFromConnectionString(ConfigurationManager.AppSettings["Microsoft.ServiceBus.ConnectionString"], "AdminTopic", gvars.iid);
                OnMessageOptions options = new OnMessageOptions();
                options.AutoComplete = false;
                options.AutoRenewTimeout = TimeSpan.FromMinutes(1);

                Client.OnMessage((message) =>
                {
                    try
                    {
                        // Process message from subscription.
                        string body = message.GetBody<string>();
                        if (body == "suspendBridge")
                            l("received: suspendBridge = true");
                        else if (body == "resumeBridge")
                            l("received: suspendBridge = false");
                        else el("AdminTopic message unhandled: " + body);

                        // Remove message from subscription.
                        message.Complete();
                    }
                    catch (Exception)
                    {
                        // Indicates a problem, unlock message in subscription.
                        message.Abandon();
                    }
                }, options);

                g("Subscripted to AdminTopic");

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

                el("BusRole stopped - cancellation requested");
            }
            catch (Exception ex1)
            {
                ex(System.Reflection.MethodBase.GetCurrentMethod().Name + " " + ex1.ToString());
            }
        }

        private void suspendServiceBusToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TopicClient Client = TopicClient.CreateFromConnectionString(ConfigurationManager.AppSettings["Microsoft.ServiceBus.ConnectionString"], "AdminTopic");

            Client.Send(new BrokeredMessage("suspendBridge"));
            l("Sent");
        }

        private void resumeServiceBusToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TopicClient Client = TopicClient.CreateFromConnectionString(ConfigurationManager.AppSettings["Microsoft.ServiceBus.ConnectionString"], "AdminTopic");

            Client.Send(new BrokeredMessage("resumeBridge"));
            l("Sent");
        }

        private void encodeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string s = rehpis.Encrypt("constr", "webus");
            l(s);
            l(rehpis.Decrypt(s, "webus"));
        }      
    }
}
