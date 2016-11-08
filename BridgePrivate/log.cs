﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace BridgePrivate
{
    public class log
    {
        private string lognameIn;
        private string userIn;

        public async Task<log> makelog(string usin, string logname1)
        {
            lognameIn = logname1;
            userIn = usin;

            if (await tableserver.UpsertI<TableEntity>(userIn, "Logs", new TableEntity("Log", lognameIn)))
                return this;

            return null;
        }

        public async Task<bool> l(string message1)
        {
            var sendLog = new Logs
            {
                PartitionKey = "Log",
                RowKey = DateTime.Now.Ticks.ToString(),
                message = message1,
                batch = "1"
            };

            try
            {
                return await tableserver.UpsertI<Logs>(userIn, lognameIn, sendLog);
            }
            catch { return false; }
        }

        public async Task<bool> el(string message1)
        {
            var sendLog = new Logs
            {
                PartitionKey = "Log",
                RowKey = DateTime.Now.Ticks.ToString(),
                message = message1,
                batch = "2"
            };

            try
            {
                return await tableserver.UpsertI<Logs>(userIn, lognameIn, sendLog);
            }
            catch { return false; }
        }

        public async Task<bool> g(string message1)
        {
            var sendLog = new Logs
            {
                PartitionKey = "Log",
                RowKey = DateTime.Now.Ticks.ToString(),
                message = message1,
                batch = "3"
            };

            try
            {
                return await tableserver.UpsertI<Logs>(userIn, lognameIn, sendLog);
            }
            catch { return false; }
        }

        public async Task<bool> ex(string message1)
        {
            var sendLog = new Logs
            {
                PartitionKey = "Log",
                RowKey = DateTime.Now.Ticks.ToString(),
                message = message1,
                batch = "4"
            };

            try
            {
                return await tableserver.UpsertI<Logs>(userIn, lognameIn, sendLog);
            }
            catch { return false; }
        }
    }
}