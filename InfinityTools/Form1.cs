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
using System.Configuration;
using BridgePublic;
using System.Diagnostics;
using Microsoft.ServiceBus;
using System.ServiceModel;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.WindowsAzure.Storage.RetryPolicies;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Interactions.Internal;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System.Net;

namespace InfinityTools
{
    [ServiceContract(Name = "IBridgeContract", Namespace = "lolwtfbbq")]
    public interface IBridgeContract
    {
        [OperationContract]
        string Echo(string text);

        [OperationContract]
        string Rev(string text);

        [OperationContract]
        string Cred(string usr, string tok, string ip);
    }

    public interface IBridgeChannel : IBridgeContract, IClientChannel { }

    public partial class Form1 : Form
    {
        // servicebus stuff
        public static IBridgeChannel channel;
        public static ChannelFactory<IBridgeChannel> channelFactory;

        // housekeeping
        public static List<Thread> threads = new List<Thread>();
        public tokens tokens = new tokens();
        private KeyValuePair<string, CancellationToken> maintoken;
        public string EXEPath = "";
        private Dictionary<string, ChromeDriver> cdrivers;
        public static bool ready2try = false;
        public static Agility agility = new Agility();
        private static bool noip { get; set; }
        private CloudStorageAccount storageAccount = null;
        private static PublicStore ps;
        private static log log = null;

        public Form1()
        {
            InitializeComponent();
            this.toolStrip1.Visible = false;
            this.ControlBox = false;
            maintoken = tokens.add("main");
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.Text = "Infinity Tools - Not signed in";
            this.signInToolStripMenuItem.Text = "Sign In";
            this.signInToolStripMenuItem.BackColor = Color.IndianRed;
            l("Loading");

            ServicePointManager.DefaultConnectionLimit = 100;
            cdrivers = new Dictionary<string, ChromeDriver>();

            new Thread(() =>
            {               
                gvars.lip = agility.getIP();

                if (gvars.lip == null || gvars.lip == "" || gvars.lip.Contains("error"))
                {
                    noip = true;
                    el("Internet connection required to continue");
                    el("IP test returned: " + gvars.lip);
                    el("Please establish an internet connection and then restart this application");
                    new Thread(() => Closer()).Start();
                    return;
                }
                else noip = false;

                if (!startRelay1())
                {
                    ex("Host relay required to continue");
                    new Thread(() => Closer()).Start();
                    return;
                }

                this.Invoke((MethodInvoker)delegate()
                {
                    this.toolStrip1.Visible = true;
                });

                if (Properties.Settings.Default.webus == "none" || Properties.Settings.Default.webtok == "none")
                {
                    el("Update your web username and token under 'Options' to continue");

                    ready2try = false;
                }
                else
                {
                    ready2try = true;
                }

                l("Load complete");
                l("Please update user and token if needed and 'Sign In' under 'Options' to continue");
                l("Note that updating user or token will sign you in automatically");

            }).Start();   
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            l("Preparing to exit...");
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

            try
            {
                channel.Close();
                channelFactory.Close();
            }
            catch { }

            foreach (KeyValuePair<string, ChromeDriver> kvp in cdrivers)
                try
                {
                    this.Invoke((MethodInvoker)delegate()
                    {
                        try
                        {
                            kvp.Value.Quit();
                        }
                        catch { }
                    });
                }
                catch { }

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

        private void gS(string s)
        {
            if (log != null)
                log.g(s);
        }

        private void elS(string s)
        {
            if (log != null)
                log.el(s);
        }

        private void exS(string s)
        {
            if (log != null)
                log.ex(s);
        }

        private void lS(string s)
        {
            if (log != null)
                log.l(s);
        }

        private void startRelayChannel1ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            l("Connecting...");
            threads.Add(new Thread(() => startRelay1()));
            threads.Last().Name = "startRelay1";
            threads.Last().Start();
        }

        private bool startRelay1()
        {
            try
            {
                ServiceBusEnvironment.SystemConnectivity.Mode = ConnectivityMode.AutoDetect;

                Uri serviceUri = ServiceBusEnvironment.CreateServiceUri("sb", ConfigurationManager.AppSettings["serviceNamespace"], "BridgeService");

                TransportClientEndpointBehavior sasCredential = new TransportClientEndpointBehavior();
                sasCredential.TokenProvider = TokenProvider.CreateSharedAccessSignatureTokenProvider("RootManageSharedAccessKey", ConfigurationManager.AppSettings["sasKey"]);

                channelFactory = new ChannelFactory<IBridgeChannel>("RelayEndpoint", new EndpointAddress(serviceUri));
                channelFactory.Endpoint.Behaviors.Add(sasCredential);

                channel = channelFactory.CreateChannel();
                channel.Open();
                g("BridgeService Channel opened");
                return true;
            }
            catch (System.ServiceModel.EndpointNotFoundException)
            {
                ex("The BridgeService endpoint was not found");
                return false;
            }
            catch (Exception ex1)
            {
                ex(System.Reflection.MethodBase.GetCurrentMethod().Name + " " + ex1.ToString());
                return false;
            }
        }

        private void stopRelayChannel1ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            threads.Add(new Thread(() => stopRelay1()));
            threads.Last().Name = "stopRelay1";
            threads.Last().Start();
        }

        private bool stopRelay1()
        {
            try
            {
                channel.Close();
                channelFactory.Close();
            }
            catch { }

            el("BridgeService Channel closed");
            return true;
        }

        private void channel1TestToolStripMenuItem_Click(object sender, EventArgs e)
        {
            l(channel.Echo("echo"));
        }

        private void resetCredentialsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.webus = "none";
            Properties.Settings.Default.webtok = "none";
            Properties.Settings.Default.refpath = "none";
            Properties.Settings.Default.Save();
        }

        private void updateUserTokenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Updater testDialog = new Updater();
            try
            {
                // Show testDialog as a modal dialog and determine if DialogResult = OK.
                if (testDialog.ShowDialog(this) == DialogResult.OK)
                {
                    threads.Add(new Thread(() => Updater()));
                    threads.Last().Name = "Updater";
                    threads.Last().Start();
                }
                else
                {
                    el("Update cancelled");
                }
            }
            catch (Exception ex1)
            {
                ex(System.Reflection.MethodBase.GetCurrentMethod().Name + " " + ex1.ToString());
            }

            testDialog.Dispose();
        }

        private void Updater()
        {
            // Read the contents of testDialog's TextBox.
            l("Validating web credentials...");
            string s = "";

            this.Invoke((MethodInvoker)delegate()
            {
                s = channel.Cred(Properties.Settings.Default.webus, Properties.Settings.Default.webtok, gvars.lip);
            });

            if (s.Contains("error"))
            {
                ps = null;
                log = null;

                el(s);
                el("Please update with valid web user and web token to continue.");

                this.Invoke((MethodInvoker)delegate()
                {
                    this.Text = "Infinity Tools - Not signed in";
                    this.signInToolStripMenuItem.Text = "Sign In";
                    this.signInToolStripMenuItem.BackColor = Color.IndianRed;
                });               
            }
            else
            {
                storageAccount = Microsoft.WindowsAzure.Storage.CloudStorageAccount.Parse(rehpis.Decrypt(s, Properties.Settings.Default.webus));

                ps = new PublicStore(storageAccount, Properties.Settings.Default.webus);

                log = new log(Properties.Settings.Default.webus, "LogsLocalTools", storageAccount);
                lS(Properties.Settings.Default.webus + " authenticated");
                g("Credentials validated.");
                this.Invoke((MethodInvoker)delegate()
                {
                    this.Text = "Infinity Tools - Signed in";
                    this.signInToolStripMenuItem.Text = "Signed In";
                    this.signInToolStripMenuItem.BackColor = Color.Green;
                });
            }        
        }

        private void signInToolStripMenuItem_Click(object sender, EventArgs e)
        {
            threads.Add(new Thread(() => Updater()));
            threads.Last().Name = "Updater";
            threads.Last().Start();
        }
    }
}
