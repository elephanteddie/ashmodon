using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System.ServiceModel;
using Microsoft.ServiceBus;
using System.ServiceModel.Description;
using Microsoft.ServiceBus.Description;
using System.Configuration;
using Newtonsoft.Json;
using BridgePublic;

namespace AdminApp
{
    [ServiceContract(Name = "IBridgeContract", Namespace = "lolwtfbbq")]
    public interface IBridgeContract
    {
        [OperationContract]
        string Echo(string text);

        [OperationContract]
        string Rev(string text);

        [OperationContract]
        Task<string> getI(string user, string token, string tableName, string typ, string pk, string rk);
    }

    public interface IBridgeChannel : IBridgeContract, IClientChannel { }

    [ServiceBehavior(Name = "BridgeService", Namespace = "lolwtfbbq")]
    class BridgeService : IBridgeContract
    {
        private static CloudStorageAccount storageAccount = Microsoft.WindowsAzure.Storage.CloudStorageAccount.Parse(Properties.Settings.Default.storage);

        private log slog = new log("base", "LogServicebusTablerelay", storageAccount);

        public string Echo(string text)
        {
            try
            {
                l("Echoing:" + text);
                return gvars.iid + " " + text;
            }
            catch (Exception ex1)
            {
                ex(System.Reflection.MethodBase.GetCurrentMethod().Name + " " + ex1.Message + " " + ex1.ToString());
                return "Echo error";
            }
        }

        public string Rev(string text)
        {
            try
            {
                l("Reversing:" + text);
                char[] charArray = text.ToCharArray();
                Array.Reverse(charArray);
                return gvars.iid + " " + (new string(charArray));
            }
            catch (Exception ex1)
            {
                ex(System.Reflection.MethodBase.GetCurrentMethod().Name + " " + ex1.Message + " " + ex1.ToString());
                return "Rev error";
            }
        }

        public async Task<string> getI(string user, string token, string tableName, string typ, string pk, string rk)
        {
            return "";
        }
        
        private void l(string s)
        {
            slog.l(gvars.iid + " " + s);
        }

        private void el(string s)
        {
            slog.el(gvars.iid + " " + s);
        }

        private void ex(string s)
        {
            slog.ex(gvars.iid + " " + s);
        }

        private void g(string s)
        {
            slog.g(gvars.iid + " " + s);
        }
    }
}
