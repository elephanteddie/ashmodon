using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace BridgePublic
{
    public class log
    {
        private string lognameIn;
        private string userIn;
        private CloudStorageAccount storageAccount = null;
        private CloudTableClient tableClient = null;

        public log(string usin, string logname1, CloudStorageAccount con)
        {
            lognameIn = logname1;
            userIn = usin;
            storageAccount = con;
            tableClient = storageAccount.CreateCloudTableClient();

            if (!UpsertIS<TableEntity>("Logs", new TableEntity("Log", lognameIn)))
                throw new System.InvalidOperationException("failed to create log reference");
        }

        private CloudTable gtS(string name)
        {
            try
            {
                var table = tableClient.GetTableReference(name);
                table.CreateIfNotExists();

                return table;
            }
            catch
            {
                return null;
            }
        }

        private bool UpsertIS<T>(string tableName, T item) where T : TableEntity, new()
        {
            try
            {
                CloudTable table = gtS(tableName);

                TableOperation upsertOperation = TableOperation.InsertOrReplace(item);
                TableResult res1 = table.Execute(upsertOperation);

                if (res1 != null)
                    return true;
                else
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
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

                        UpsertIS<Logs>(lognameIn, sendLog);
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

                        UpsertIS<Logs>(lognameIn, sendLog);
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

                        UpsertIS<Logs>(lognameIn, sendLog);
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

                        UpsertIS<Logs>(lognameIn, sendLog);
                    }
                    catch { }

                }).Start();
            }
            catch { }
        }
    }
}
