﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using Microsoft.AspNet.SignalR;

namespace RealTimeProgressBar
{
    [Microsoft.AspNet.SignalR.Hubs.HubName("progressHub")]
    public class ProgressHub : Hub
    {
        public string msg = "Initializing and Preparing...";
        public int count = 1;

        public static void SendMessage(string msg, int count)
        {
            var message = "Process completed for " + msg;
            var hubContext = GlobalHost.ConnectionManager.GetHubContext<ProgressHub>();
            hubContext.Clients.All.sendMessage(string.Format(message), count);
        }

        public void GetCountAndMessage()
        {
            Clients.Caller.sendMessage(string.Format(msg), count);
        }
    }
}