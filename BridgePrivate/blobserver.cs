using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using BridgePublic;
using System.Diagnostics;

namespace BridgePrivate
{
    public static class blobserver
    {
        private static CloudStorageAccount storageAccount = Microsoft.WindowsAzure.Storage.CloudStorageAccount.Parse(Properties.Settings.Default.storage);
        private static CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient(); //bridgebase

        private static Dictionary<string, CloudStorageAccount> stores = new Dictionary<string, CloudStorageAccount>();
        private static Dictionary<string, CloudBlobClient> clients = new Dictionary<string, CloudBlobClient>();
        private static Dictionary<string, log> logs = new Dictionary<string, log>();

        private static log blog;

        static blobserver()
        {            
            stores.Add("base", storageAccount);
            clients.Add("base", blobClient);
            logs.Add("base", new log("base", "BlobStorageUserLog"));

            blog = new log("base", "BlobStorageBaseLog");

            if (!seedDictionariesS())
                blog.el("blobserver directory seed error");
            else blog.g("blobserver constructed");
        }

        private static bool seedDictionariesS()
        {
            try
            {
                List<sacstr> sacs = tableserver.GetPS<sacstr>("base", "sacinfo", "sacstr");

                foreach (sacstr te in sacs)
                {
                    if (!stores.ContainsKey(te.RowKey))
                        stores.Add(te.RowKey, Microsoft.WindowsAzure.Storage.CloudStorageAccount.Parse(te.sac));
                    if (!clients.ContainsKey(te.RowKey))
                        clients.Add(te.RowKey, stores[te.RowKey].CreateCloudBlobClient());
                    if (!logs.ContainsKey(te.RowKey))
                        logs.Add(te.RowKey, new log(te.RowKey, "BlobStorageUserLog"));
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
                List<sacstr> sacs = await tableserver.GetP<sacstr>("base", "sacinfo", "sacstr");

                foreach (sacstr te in sacs)
                {
                    if (!stores.ContainsKey(te.RowKey))
                        stores.Add(te.RowKey, Microsoft.WindowsAzure.Storage.CloudStorageAccount.Parse(te.sac));
                    if (!clients.ContainsKey(te.RowKey))
                        clients.Add(te.RowKey, stores[te.RowKey].CreateCloudBlobClient());
                    if (!logs.ContainsKey(te.RowKey))
                        logs.Add(te.RowKey, new log(te.RowKey, "BlobStorageUserLog"));
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

        private static async Task<CloudBlobContainer> gb(string user, string name)
        {
            if (!await gooduser(user)) return null;

            try
            {
                var blob = clients[user].GetContainerReference(name);
                blob.CreateIfNotExists();

                return blob;
            }
            catch (Exception ex1)
            {
                logs[user].ex(System.Reflection.MethodBase.GetCurrentMethod().Name + " " + ex1.ToString());
                return null;
            }
        }

        private static async Task<CloudBlockBlob> GetLockBlob(string user, string container, string bname)
        {
            if (!await gooduser(user)) return null;

            try
            {
                CloudBlobContainer cont = await gb(user, container);
                if (cont == null)
                {
                    logs[user].el("failed to create blob container: " + container);
                    return null;
                }

                CloudBlockBlob blob = cont.GetBlockBlobReference(bname);
                if (!await blob.ExistsAsync())
                {
                    await blob.UploadTextAsync("");
                }
                return blob;
            }
            catch (Exception ex1)
            {
                logs[user].ex(System.Reflection.MethodBase.GetCurrentMethod().Name + " " + ex1.ToString());
                return null;
            }
        }

        public static async Task<string> GetLock(string user, string container, string bname)
        {
            if (!await gooduser(user)) return "error - username";

            string ret = "error - timeout";

            try
            {
                CloudBlockBlob blob = await GetLockBlob(user, container, bname);
                if (blob == null)
                    return "error - no blob";

                Stopwatch watch = new Stopwatch();

                watch.Start();            

                while (watch.Elapsed.TotalSeconds < 70)
                {
                    try
                    {
                        ret = await blob.AcquireLeaseAsync(TimeSpan.FromSeconds(60), Guid.NewGuid().ToString());
                        break;
                    }
                    catch { }

                    await Task.Delay(100);
                }

                watch.Stop();

                return ret;
            }
            catch (Exception ex1)
            {
                logs[user].ex(System.Reflection.MethodBase.GetCurrentMethod().Name + " " + ex1.ToString());
                return "error - exception";
            }
        }

        public static async Task<bool> ReleaseLock(string user, string container, string bname, string lease)
        {
            if (!await gooduser(user)) return false;

            try
            {
                CloudBlockBlob blob = await GetLockBlob(user, container, bname);
                if (blob == null)
                    return false; 

                await blob.ReleaseLeaseAsync(AccessCondition.GenerateLeaseCondition(lease));

                return true;
            }
            catch (Exception ex1)
            {
                logs[user].ex(System.Reflection.MethodBase.GetCurrentMethod().Name + " " + ex1.ToString());
                return false;
            }
        }
    }
}
