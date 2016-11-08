using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System.ComponentModel.DataAnnotations;

namespace BridgePrivate
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
}
