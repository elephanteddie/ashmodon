using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System.ComponentModel.DataAnnotations;

namespace BridgePublic
{
    public class Logs : TableEntity
    {
        [Display(Name = "[Active]")]
        public bool logging { get; set; }

        [Display(Name = "[Message]")]
        public string message { get; set; }

        public string batch { get; set; }
    }

    public class sacstr : TableEntity
    {
        public string sac { get; set; }
    }

    public class tokens
    {
        public Dictionary<string, CancellationTokenSource> sources;
        //public Dictionary<string, CancellationToken> tokens;

        public tokens()
        {
            sources = new Dictionary<string, CancellationTokenSource>();
            //tokens = new Dictionary<string, CancellationToken>();
        }

        public KeyValuePair<string, CancellationToken> add(string tokstr)
        {
            if (sources.ContainsKey(tokstr))
            {
                return new KeyValuePair<string, CancellationToken>("error", new CancellationToken());
            }

            sources.Add(tokstr, new CancellationTokenSource());
            //tokens.Add(tokstr, sources[tokstr].Token);

            return new KeyValuePair<string, CancellationToken>(tokstr, sources[tokstr].Token);
        }

        public bool remove(string tokstr)
        {
            if (!sources.ContainsKey(tokstr))
                return false;

            sources.Remove(tokstr);
            return true;
        }

        public bool cancel(string tokstr)
        {
            if (!sources.ContainsKey(tokstr))
                return false;

            sources[tokstr].Cancel();
            return true;
        }

        public bool cancelAll()
        {
            foreach (KeyValuePair<string, CancellationTokenSource> kvp in sources)
            {
                kvp.Value.Cancel();
            }

            return true;
        }
    }
}
