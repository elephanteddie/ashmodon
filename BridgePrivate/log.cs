using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using BridgePublic;

namespace BridgePrivate
{
    public class log
    {
        private string lognameIn;
        private string userIn;
        private bool failed = false;

        public log(string usin, string logname1)
        {
            lognameIn = logname1;
            userIn = usin;

            if (!tableserver.UpsertIS<TableEntity>(userIn, "Logs", new TableEntity("Log", lognameIn)))
                failed = true;
        }

        public void l(string message1)
        {
            try
            {
                new Thread(() =>
                {
                    try
                    {
                        var sendLog = new Logs
                        {
                            PartitionKey = "Log",
                            RowKey = DateTime.Now.Ticks.ToString(),
                            message = message1,
                            batch = "1"
                        };

                        tableserver.UpsertIS<Logs>(userIn, lognameIn, sendLog);
                    }
                    catch { }

                }).Start();
            }
            catch { }
        }

        public void el(string message1)
        {
            try 
            { 
                new Thread(() =>
                {
                    try
                    {
                        var sendLog = new Logs
                        {
                            PartitionKey = "Log",
                            RowKey = DateTime.Now.Ticks.ToString(),
                            message = message1,
                            batch = "2"
                        };

                        tableserver.UpsertIS<Logs>(userIn, lognameIn, sendLog);
                    }
                    catch { }

                }).Start();
            }
            catch { }
        }

        public void g(string message1)
        {
            try
            { 
                new Thread(() =>
                {
                    try
                    {
                        var sendLog = new Logs
                        {
                            PartitionKey = "Log",
                            RowKey = DateTime.Now.Ticks.ToString(),
                            message = message1,
                            batch = "3"
                        };

                        tableserver.UpsertIS<Logs>(userIn, lognameIn, sendLog);
                    }
                    catch { }

                }).Start();
            }
            catch { }
        }

        public void ex(string message1)
        {
            try 
            { 
                new Thread(() =>
                {
                    try
                    {
                        var sendLog = new Logs
                        {
                            PartitionKey = "Log",
                            RowKey = DateTime.Now.Ticks.ToString(),
                            message = message1,
                            batch = "4"
                        };

                        tableserver.UpsertIS<Logs>(userIn, lognameIn, sendLog);
                    }
                    catch { }

                }).Start();
            }
            catch { }
        }
    }
}
