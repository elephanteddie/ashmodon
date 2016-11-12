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

namespace BusRole
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

    [ServiceBehavior(Name = "BridgeService", Namespace = "lolwtfbbq")]
    class BridgeService : IBridgeContract
    {
        private static CloudStorageAccount storageAccount = Microsoft.WindowsAzure.Storage.CloudStorageAccount.Parse(Properties.Settings.Default.storage);
        private static PublicStore ps = new PublicStore(storageAccount, "base");

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
                
        public string Cred(string usr, string tok, string ip)
        {
            // was there a bad attempt in last 5 minutes?
            // if so, reject
            stamp stmp = ps.GetI<stamp>("attemptIp", "IP", ip).Result;
            if (stmp != null)
            {
                if (stmp.retMinutes() < 5.0)
                {
                    el(usr + " " + tok + " " + ip + " Cred error - too many requests from IP.");
                    return "error - too many requests from IP. Please wait " + Math.Round(5.0 - stmp.retMinutes(), 2) + " minutes";
                }
            }

            stamp stmpr2 = ps.GetI<stamp>("attemptUser", "USER", usr).Result;
            if(stmpr2 != null)
            {
                if (stmpr2.retMinutes() < 5.0)
                {
                    el(usr + " " + tok + " " + ip + " Cred error - too many requests for user.");
                    return "error - too many requests for user. Please wait " + Math.Round(5.0 - stmpr2.retMinutes(), 2) + " minutes";
                }
            }

            // if not, check this attempt
            // if good continue
            // if bad, reject and mark new bad attempt

            webtoken token = ps.GetI<webtoken>("webtoks", usr, usr).Result;
            if (token == null)
            {
                stmpr2 = new stamp("USER", usr);
                if (!ps.UpsertI<stamp>("attemptUser", stmpr2).Result)
                {
                    el(usr + " " + tok + " " + ip + " Cred error - no token and error upsert attemptUser");
                    return "error - stamp upsert attemptUser";
                }

                var stmpr = new stamp("IP", ip);
                if (!ps.UpsertI<stamp>("attemptIp", stmpr).Result)
                {
                    el(usr + " " + tok + " " + ip + " Cred error - no token and error upsert attemptIp");
                    return "error - stamp upsert attemptIp";
                }

                el(usr + " " + tok + " " + ip + " Cred error - no token");
                return "error - no token found.  Please ensure you are using the correct username, or log into the Rosenlink website and generate your first token by navigating to Options -> Web Token";
            }

            if (token.tok != tok)
            {
                stmpr2 = new stamp("USER", usr);
                if (!ps.UpsertI<stamp>("attemptUser", stmpr2).Result)
                {
                    el(usr + " " + tok + " " + ip + " Cred error - token mismatch and error upsert attemptUser");
                    return "error - stamp upsert attemptUser";
                }

                var stmpr = new stamp("IP", ip);
                if (!ps.UpsertI<stamp>("attemptIp", stmpr).Result)
                {
                    el(usr + " " + tok + " " + ip + " Cred error - token mismatch and error upsert attemptIp");
                    return "error - stamp upsert attemptIp";
                }

                el(usr + " " + tok + " " + ip + " Cred error - token mismatch");
                return "error - token mismatch. Please ensure you are using the correct username, or log into the Rosenlink website and copy paste the value found by navigating to Options -> Web Token";
            }

            sacstr sac = ps.GetI<sacstr>("sacinfo", "sacstr", usr).Result;
            if (sac == null)
            {
                stmpr2 = new stamp("USER", usr);
                if (!ps.UpsertI<stamp>("attemptUser", stmpr2).Result)
                {
                    el(usr + " " + tok + " " + ip + " Cred error - no sacstr and error upsert attemptUser");
                    return "error - stamp upsert attemptUser";
                }

                var stmpr = new stamp("IP", ip);
                if (!ps.UpsertI<stamp>("attemptIp", stmpr).Result)
                {
                    el(usr + " " + tok + " " + ip + " Cred error - no sacstr and error upsert attemptIp");
                    return "error - stamp upsert attemptIp";
                }

                el(usr + " " + tok + " " + ip + " Cred error - no sacstr");
                return "error - no cstr found.  Please ensure you are using the correct username, or log into the Rosenlink website and generate your first token by navigating to Options -> Web Token";
            }

            g(usr + " " + tok + " " + ip + " credentials verified.");
            return rehpis.Encrypt(sac.sac, usr);
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
