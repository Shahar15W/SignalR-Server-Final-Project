using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignalR_Server.Hubs
{
    public class ChatHub : Hub
    {
        public override Task OnConnected()
        {
            // Send a welcome message to the connected client
            Console.WriteLine($"User {Context.ConnectionId} connected."); // Display in server console

            return base.OnConnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            // Log or handle the disconnect event
            Console.WriteLine($"User {Context.ConnectionId} has disconnected."); // Display in server console

            return base.OnDisconnected(stopCalled);
        }

        public void Send(string name, string message)
        {
            // Your existing Send method
            Clients.All.broadcastMessage(name, message);
        }
    }
}
