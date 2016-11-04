using Microsoft.AspNet.SignalR;

/*
 * https://www.asp.net/signalr
 * 
 * ASP.NET SignalR is a library for ASP.NET developers that simplifies the process of adding 
 * real-time web functionality to applications. Real-time web functionality is the ability to 
 * have server code push content to connected clients instantly as it becomes available, rather 
 * than having the server wait for a client to request new data.
 * 
 * How Hubs work
 * When server-side code calls a method on the client, a packet is sent across the active 
 * transport that contains the name and parameters of the method to be called (when an object 
 * is sent as a method parameter, it is serialized using JSON). The client then matches the 
 * method name to methods defined in client-side code. If there is a match, the client method 
 * will be executed using the deserialized parameter data.
 * 
 */

namespace RealTimeProgressBar
{
    [Microsoft.AspNet.SignalR.Hubs.HubName("progressHub")]
    public class ProgressHub : Hub
    {
        //public string msg = "";
        //public int count = 0;

        public static void SendMessage(string msg, int count, bool complete)
        {
            var hubContext = GlobalHost.ConnectionManager.GetHubContext<ProgressHub>();
            hubContext.Clients.All.sendMessage(msg, count, complete);
        }

        public void GetCountAndMessage()
        {
            //Clients.Caller.sendMessage(string.Format(msg), count);
        }
    }
}