using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin.Hosting;
using Microsoft.Owin.Cors;
using Owin;
using Microsoft.AspNet.SignalR.Messaging;
using SignalR_Server.Hubs;



namespace SignalR_Server
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "SignalR Server";
            string url = "http://localhost:8081"; // Set the desired URL

            // Create and initialize the ServerManager singleton            
            //ServerManager serverManager = ServerManager.Instance;


            using (WebApp.Start(url))
            {
                Console.WriteLine($"Server running at {url}");
                Console.ReadLine();
            }
        }
    }
    
}
public class Startup
{

    public void Configuration(IAppBuilder app)
    {
        app.UseCors(CorsOptions.AllowAll);

        // Map the SignalR hub
        app.MapSignalR("/Hubs", new HubConfiguration
        {
            // Configuration options
        });

    }
}
