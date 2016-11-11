using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Threading;
using System.Runtime.InteropServices;
using System.Collections;
using System.Security.Cryptography;

namespace BridgePublic
{
    public static class Methods
    {
        public static string getrand256()
        {
            var bytes = new byte[16];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(bytes);

                return BitConverter.ToString(bytes).Replace("-", "");
            }
        }
    }
}
