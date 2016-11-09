using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace BridgePrivate
{
    public static class tableserver
    {
        public static CloudStorageAccount storageAccount = Microsoft.WindowsAzure.Storage.CloudStorageAccount.Parse(Properties.Settings.Default.storage);
        public static CloudTableClient tableClient = storageAccount.CreateCloudTableClient(); //bridgebase

        public static Dictionary<string, CloudStorageAccount> stores = new Dictionary<string, CloudStorageAccount>();
        public static Dictionary<string, CloudTableClient> clients = new Dictionary<string, CloudTableClient>();
        public static Dictionary<string, log> logs = new Dictionary<string, log>();

        public static log blog;

        static tableserver()
        {            
            stores.Add("base", storageAccount);
            clients.Add("base", tableClient);
            logs.Add("base", new log("base", "TableStorageUserLog"));
            blog = new log("base", "TableStorageBaseLog");

            if (!seedDictionariesS())
                blog.el("tableserver directory seed error");
        }

        private static bool seedDictionariesS()
        {
            try
            {
                List<sacstr> sacs = GetPS<sacstr>("base", "sacinfo", "sacstr");

                foreach (sacstr te in sacs)
                {
                    if (!stores.ContainsKey(te.RowKey))
                        stores.Add(te.RowKey, Microsoft.WindowsAzure.Storage.CloudStorageAccount.Parse(te.sac));
                    if (!clients.ContainsKey(te.RowKey))
                        clients.Add(te.RowKey, stores[te.RowKey].CreateCloudTableClient());
                    if (!logs.ContainsKey(te.RowKey))
                        logs.Add(te.RowKey, new log(te.RowKey, "TableStorageUserLog"));
                }

                return true;
            }
            catch (Exception ex1)
            {
                blog.ex(System.Reflection.MethodBase.GetCurrentMethod().Name + " " + ex1.ToString());
                return false;
            }
        }

        private static async Task<bool> seedDictionaries()
        {
            try
            {
                List<sacstr> sacs = await GetP<sacstr>("base", "sacinfo", "sacstr");

                foreach (sacstr te in sacs)
                {
                    if (!stores.ContainsKey(te.RowKey))
                        stores.Add(te.RowKey, Microsoft.WindowsAzure.Storage.CloudStorageAccount.Parse(te.sac));
                    if (!clients.ContainsKey(te.RowKey))
                        clients.Add(te.RowKey, stores[te.RowKey].CreateCloudTableClient());
                    if (!logs.ContainsKey(te.RowKey))
                        logs.Add(te.RowKey, new log(te.RowKey, "TableStorageUserLog"));
                }

                return true;
            }
            catch (Exception ex1)
            {
                blog.ex(System.Reflection.MethodBase.GetCurrentMethod().Name + " " + ex1.ToString());
                return false;
            }
        }

        private static async Task<bool> gooduser(string usr)
        {
            try
            {
                if (!stores.ContainsKey(usr))
                {
                    if (await seedDictionaries())
                    {
                        if (!stores.ContainsKey(usr))
                        {
                            blog.el(System.Reflection.MethodBase.GetCurrentMethod().Name + " invalid user request: " + usr);
                            return false;
                        }
                        else return true;
                    }
                    else
                    {
                        blog.el(System.Reflection.MethodBase.GetCurrentMethod().Name + " invalid seed user: " + usr);
                        return false;
                    }
                }
                else return true;
            }
            catch (Exception ex1)
            {
                blog.ex(usr + " on " + System.Reflection.MethodBase.GetCurrentMethod().Name + " " + ex1.ToString());
                return false;
            }
        }

        private static bool gooduserS(string usr)
        {
            try
            {
                if (!stores.ContainsKey(usr))
                {
                    if (seedDictionariesS())
                    {
                        if (!stores.ContainsKey(usr))
                        {
                            blog.el(System.Reflection.MethodBase.GetCurrentMethod().Name + " invalid user request: " + usr);
                            return false;
                        }
                        else return true;
                    }
                    else
                    {
                        blog.el(System.Reflection.MethodBase.GetCurrentMethod().Name + " invalid seed user: " + usr);
                        return false;
                    }
                }
                else return true;
            }
            catch (Exception ex1)
            {
                blog.ex(usr + " on " + System.Reflection.MethodBase.GetCurrentMethod().Name + " " + ex1.ToString());
                return false;
            }
        }

        private static async Task<CloudTable> gt(string user, string name)
        {
            if (! await gooduser(user)) return null;

            try
            {
                var table = clients[user].GetTableReference(name);
                table.CreateIfNotExists();

                return table;
            }
            catch (Exception ex1)
            {
                logs[user].ex(System.Reflection.MethodBase.GetCurrentMethod().Name + " " + ex1.ToString());               
                return null;
            }
        }

        private static CloudTable gtS(string user, string name)
        {
            if (!gooduserS(user)) return null;

            try
            {
                var table = clients[user].GetTableReference(name);
                table.CreateIfNotExists();

                return table;
            }
            catch (Exception ex1)
            {
                logs[user].ex(System.Reflection.MethodBase.GetCurrentMethod().Name + " " + ex1.ToString());
                return null;
            }
        }

        private static List<T> GetPS<T>(string user, string tableName, string partitionKey) where T : TableEntity, new()
        {
            if (!gooduserS(user)) return null;

            try
            {
                CloudTable table = gtS(user, tableName);

                TableQuery<T> query = new TableQuery<T>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey));

                List<T> list = new List<T>();
                TableContinuationToken tokenn = null;

                do
                {
                    TableQuerySegment<T> seg = table.ExecuteQuerySegmented<T>(query, tokenn);
                    tokenn = seg.ContinuationToken;
                    list.AddRange(seg);

                } while (tokenn != null);

                if (list.Count > 0)
                    return list;
                else
                {
                    return null;
                }
            }
            catch (Exception ex1)
            {
                logs[user].ex(System.Reflection.MethodBase.GetCurrentMethod().Name + " " + ex1.ToString());
                return null;
            }
        }

        private static async Task<List<T>> GetP<T>(string user, string tableName, string partitionKey) where T : TableEntity, new()
        {
            if (!await gooduser(user)) return null;

            try
            {
                CloudTable table = await gt(user, tableName);

                TableQuery<T> query = new TableQuery<T>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey));

                List<T> list = new List<T>();
                TableContinuationToken tokenn = null;

                do
                {
                    TableQuerySegment<T> seg = await table.ExecuteQuerySegmentedAsync<T>(query, tokenn);
                    tokenn = seg.ContinuationToken;
                    list.AddRange(seg);

                } while (tokenn != null);

                if (list.Count > 0)
                    return list;
                else
                {
                    return null;
                }
            }
            catch (Exception ex1)
            {
                logs[user].ex(System.Reflection.MethodBase.GetCurrentMethod().Name + " " + ex1.ToString());
                return null;
            }
        }

        public static async Task<bool> UpsertI<T>(string user, string tableName, T item) where T : TableEntity, new()
        {
            if (!await gooduser(user)) return false;

            try
            {
                CloudTable table = await gt(user, tableName);

                TableOperation upsertOperation = TableOperation.InsertOrReplace(item);
                TableResult res1 = await table.ExecuteAsync(upsertOperation);

                if (res1 != null)
                    return true;
                else
                {
                    logs[user].el(item.PartitionKey + " " + item.RowKey + " UpsertI returned null");
                    return false;
                }
            }
            catch (Exception ex1)
            {
                logs[user].ex(System.Reflection.MethodBase.GetCurrentMethod().Name + " " + ex1.ToString());            
                return false;
            }
        }

        public static bool UpsertIS<T>(string user, string tableName, T item) where T : TableEntity, new()
        {
            if (!gooduserS(user)) return false;

            try
            {
                CloudTable table = gtS(user, tableName);

                TableOperation upsertOperation = TableOperation.InsertOrReplace(item);
                TableResult res1 = table.Execute(upsertOperation);

                if (res1 != null)
                    return true;
                else
                {
                    logs[user].el(item.PartitionKey + " " + item.RowKey + " UpsertI returned null");
                    return false;
                }
            }
            catch (Exception ex1)
            {
                logs[user].ex(System.Reflection.MethodBase.GetCurrentMethod().Name + " " + ex1.ToString());
                return false;
            }
        }
    }
}
