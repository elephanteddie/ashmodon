using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;
using BridgePublic;
using System.ServiceModel;
using Microsoft.ServiceBus;
using System.ServiceModel.Description;
using Microsoft.ServiceBus.Description;
using Microsoft.ServiceBus.Messaging;
using System.Configuration;
using Newtonsoft.Json;

namespace BusRole
{
    public class WorkerRole : RoleEntryPoint
    {
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private readonly ManualResetEvent runCompleteEvent = new ManualResetEvent(false);
        public string instanceID;
        public log log;
        public bool suspendBridge = false;
        private CloudStorageAccount storageAccount = null;
        private static PublicStore ps;

        public override void Run()
        {
            l("BusRole is running");

            try
            {
                this.RunAsync(this.cancellationTokenSource.Token).Wait();
            }
            finally
            {
                this.runCompleteEvent.Set();
            }
        }

        public override bool OnStart()
        {
            // Set the maximum number of concurrent connections
            ServicePointManager.DefaultConnectionLimit = 12;

            bool result = base.OnStart();

            string id = RoleEnvironment.CurrentRoleInstance.Id;
            instanceID = "i";

            int pos1 = id.IndexOf("BusRole");
            if (pos1 >= 0)
            {
                instanceID = id.Substring(pos1 + "BusRole".Length + 1);
            }
            else
            {
                instanceID = id;
            }

            gvars.iid = instanceID.ToString();

            storageAccount = Microsoft.WindowsAzure.Storage.CloudStorageAccount.Parse(Properties.Settings.Default.storage);

            ps = new PublicStore(storageAccount, "base");

            log = new log("base", "LogsServicebusWorker", storageAccount);

            l("BusRole has been started");

            return result;
        }

        public override void OnStop()
        {
            el("BusRole is stopping");

            this.cancellationTokenSource.Cancel();
            this.runCompleteEvent.WaitOne();

            base.OnStop();

            el("BusRole has stopped");
        }

        private async Task RunAsync(CancellationToken cancellationToken)
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
                            suspendBridge = true;
                        else if (body == "resumeBridge")
                            suspendBridge = false;
                        else el("AdminTopic message unhandled: " + body);

                        // Remove message from subscription.
                        message.Complete();
                    }
                    catch (Exception)
                    {
                        el("Message abandoned: ");
                        el(message.GetBody<string>());
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

        public void l(string s)
        {
            log.l(instanceID + " " + s);
        }

        public void el(string s)
        {
            log.el(instanceID + " " + s);
        }

        public void ex(string s)
        {
            log.ex(instanceID + " " + s);
        }

        public void g(string s)
        {
            log.g(instanceID + " " + s);
        }
    }
}
