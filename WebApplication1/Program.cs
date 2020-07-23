using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;

namespace WebApplication1
{
    public class Program
    {
        //here starts the application as a console programm and runs Main(string[] args)
        public static void Main(string[] args)
        {
            //calls createwebhostbuilder with certain parameters(command line arguements if they exist)
            CreateWebHostBuilder(args).Build().Run();
            //after CreateWebHostBuilder ends and returns IWebHostBuilder object
            // it runs build method on it which builds the webhost
            //and then it it runs run method on the webhost which starts listening on that webhost
            //for http request or https request
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            //we use the /ConfigureLogging(which is a part of the createdefaultbuilder behind the scenes to add custom logging providers like nlog
            WebHost.CreateDefaultBuilder(args).ConfigureLogging((hostingContext, logging) =>
            {
                logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                logging.AddConsole();
                logging.AddDebug();
                logging.AddEventSourceLogger();
                // Enable NLog as one of the Logging Provider
                logging.AddNLog();
            })
                .UseStartup<Startup>();
               

        
        //sets up webhost with preconfigured defaults from multiple configuration sources
        //for example it looks at webapplication1 <AspNetCoreHostingModel> and then decides to run inprocess or
        //out of process with the method UseIIS behind the scenes if inprocess and hosts it in the process
        //w3wp.exe or iisexpress.exe(w3wp.exe if IIS and iisexpress.exe if IIS Express)
        //if outofprocess uses a method behind the scenes and then hosts the webhost in the process
        //dotnet.exe (works with kestrel)
        //and configures logging using CreateDefaultBuilder 
        //then extends the configuration with the startup.cs class , and returns IWebHostBuilder object
        //you can change the name but then you must change the startup.cs

    }
}
