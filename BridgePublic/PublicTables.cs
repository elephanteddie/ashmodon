using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using BridgePublic;

namespace BridgePublic
{
    public static class PublicTables
    {
        private static CloudStorageAccount storageAccount = null;
        private static CloudTableClient tableClient = null; //bridgebase

        private static log blog = null;

        static PublicTables()
        {
        }

        public static bool seed(string usr, string constr)
        {
            try
            {
                storageAccount = Microsoft.WindowsAzure.Storage.CloudStorageAccount.Parse(constr);
                CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

                //blog = new log(usr, "TableStorageUserLog", constr);
                //blog.g("seeded");
                return true;
            }
            catch (Exception ex1)
            {
                blog.ex(System.Reflection.MethodBase.GetCurrentMethod().Name + " " + ex1.ToString());
                return false;
            }
        }

        private static CloudTable gtS(string name)
        {
            try
            {
                var table = tableClient.GetTableReference(name);
                table.CreateIfNotExists();

                return table;
            }
            catch (Exception ex1)
            {
                blog.ex(System.Reflection.MethodBase.GetCurrentMethod().Name + " " + ex1.ToString());
                return null;
            }
        }

        private static async Task<CloudTable> gt(string name)
        {
            try
            {
                var table = tableClient.GetTableReference(name);
                await table.CreateIfNotExistsAsync();

                return table;
            }
            catch (Exception ex1)
            {
                blog.ex(System.Reflection.MethodBase.GetCurrentMethod().Name + " " + ex1.ToString());
                return null;
            }
        }

        public static async Task<T> GetI<T>(string tableName, string partitionKey, string rowKey) where T : TableEntity, new()
        {
            try
            {
                CloudTable table = await gt(tableName);
                TableOperation retrieveOperation = TableOperation.Retrieve<T>(partitionKey, rowKey);

                TableResult retrievedResult = await table.ExecuteAsync(retrieveOperation);

                return (T)retrievedResult.Result;
            }
            catch (Exception ex1)
            {
                blog.ex(System.Reflection.MethodBase.GetCurrentMethod().Name + " " + ex1.ToString());
                return null;
            }
        }

        public static async Task<List<T>> GetP<T>(string tableName, string partitionKey) where T : TableEntity, new()
        {
            try
            {
                CloudTable table = await gt(tableName);

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
                blog.ex(System.Reflection.MethodBase.GetCurrentMethod().Name + " " + ex1.ToString());
                return null;
            }
        }

        public static async Task<List<T>> GetAll<T>(string tableName) where T : TableEntity, new()
        {
            try
            {
                var table = await gt(tableName);

                List<T> entities = new List<T>();

                TableQuery<T> query = new TableQuery<T>();
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
            catch (Exception ex)
            {
                blog.ex("GetAll " + ex.ToString());
                return null;
            }
        }

        public static bool UpsertIS<T>(string tableName, T item) where T : TableEntity, new()
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
                    blog.el(item.PartitionKey + " " + item.RowKey + " UpsertI returned null");
                    return false;
                }
            }
            catch (Exception ex1)
            {
                blog.ex(System.Reflection.MethodBase.GetCurrentMethod().Name + " " + ex1.ToString());
                return false;
            }
        }

        public static async Task<bool> UpsertI<T>(string tableName, T item) where T : TableEntity, new()
        {
            try
            {
                CloudTable table = await gt(tableName);

                TableOperation upsertOperation = TableOperation.InsertOrReplace(item);
                TableResult res1 = await table.ExecuteAsync(upsertOperation);

                if (res1 != null)
                    return true;
                else
                {
                    blog.el(item.PartitionKey + " " + item.RowKey + " UpsertI returned null");
                    return false;
                }
            }
            catch (Exception ex1)
            {
                blog.ex(System.Reflection.MethodBase.GetCurrentMethod().Name + " " + ex1.ToString());
                return false;
            }
        }

        public static async Task<bool> UpsertP<T>(string tableName, List<T> parts) where T : TableEntity, new()
        {
            try
            {
                CloudTable table = await gt(tableName);

                var batchOperation = new TableBatchOperation();
                int itemsInBatch = 0;

                foreach (T si in parts)
                {
                    batchOperation.InsertOrReplace(si);
                    itemsInBatch++;

                    if (itemsInBatch == 100)
                    {
                        await table.ExecuteBatchAsync(batchOperation);
                        itemsInBatch = 0;
                        batchOperation = new TableBatchOperation();
                    }
                }
                if (itemsInBatch > 0)
                {
                    await table.ExecuteBatchAsync(batchOperation);
                }

                return true;
            }
            catch (Exception ex1)
            {
                blog.ex("UpsertP " + ex1.ToString());
                return false;
            }
        }

        public static async Task<bool> UpsertAll<T>(string tableName, List<T> partsIn) where T : TableEntity, new()
        {
            try
            {
                CloudTable table = await gt(tableName);

                List<string> pkeys = new List<string>();

                foreach (T item in partsIn)
                {
                    if (!pkeys.Contains(item.PartitionKey))
                    {
                        pkeys.Add(item.PartitionKey);
                        await UpsertP<T>(tableName, partsIn.Where(p => p.PartitionKey == item.PartitionKey).ToList());
                    }
                }

                return true;
            }
            catch (Exception ex1)
            {
                blog.ex("UpsertAll " + ex1.ToString());
                return false;
            }
        }

        public static async Task<bool> deleteI<T>(string tableName, T item) where T : TableEntity, new()
        {
            try
            {
                CloudTable tbl = await gt(tableName);

                var entity = new DynamicTableEntity(item.PartitionKey, item.RowKey);
                entity.ETag = "*";

                await tbl.ExecuteAsync(TableOperation.Delete(entity));

                return true;
            }
            catch (Exception ex)
            {
                blog.ex("deleteI " + ex.ToString());
                return false;
            }
        }

        public static async Task<bool> deleteAll<T>(string tableName, List<T> items) where T : TableEntity, new()
        {
            try
            {
                CloudTable tbl = await gt(tableName);

                foreach (T item in items)
                {
                    var entity = new DynamicTableEntity(item.PartitionKey, item.RowKey);
                    entity.ETag = "*";

                    await tbl.ExecuteAsync(TableOperation.Delete(entity));
                }

                return true;
            }
            catch (Exception ex)
            {
                blog.ex("deleteI " + ex.ToString());
                return false;
            }
        }
    }
}
