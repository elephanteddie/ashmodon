using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using System.Diagnostics;
using System.Threading;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace BridgePublic
{
    public class Agility
    {
        private static log log = null;

        public Agility()
        {
        }

        public Agility(CloudStorageAccount acct, string usr)
        {
            log = new log(usr, "LogsAgility", acct);
        }

        private void g(string s)
        {
            if (log != null)
                log.g(s);
        }

        private void el(string s)
        {
            if (log != null)
                log.el(s);
        }

        private void ex(string s)
        {
            if (log != null)
                log.ex(s);
        }

        private void l(string s)
        {
            if (log != null)
                log.l(s);
        }

        public string getIP()
        {
            try
            {
                bool good = true;

                HtmlAgilityPack.HtmlDocument htmlDoc = new HtmlAgilityPack.HtmlDocument();

                HtmlWeb webGet = new HtmlWeb();

                htmlDoc = webGet.Load((new Uri("http://bot.whatismyipaddress.com/")).AbsoluteUri);

                if (htmlDoc == null)
                {
                    string err = "htmlDoc null";
                    el(err);
                    good = false;
                }

                if (htmlDoc.DocumentNode == null)
                {
                    string err = "htmlDoc.documentNode null";
                    el(err);
                    good = false;
                }

                if (good)
                {
                    return htmlDoc.DocumentNode.OuterHtml;
                }

                htmlDoc = webGet.Load((new Uri("http://ipinfo.io/ip")).AbsoluteUri);

                if (htmlDoc == null)
                {
                    string err = "htmlDoc null";
                    el(err);
                    return "error - " + err;
                }

                if (htmlDoc.DocumentNode == null)
                {
                    string err = "htmlDoc.documentNode null";
                    el(err);
                    return "error - " + err;
                }
                return htmlDoc.DocumentNode.OuterHtml;
            }
            catch (Exception ex1)
            {
                ex(System.Reflection.MethodBase.GetCurrentMethod().Name + " " + ex1.ToString());
                return "error - " + ex1.Message;
            }
        }
    }
}
