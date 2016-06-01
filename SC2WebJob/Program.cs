using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using SC2MiM.Common.Entities;
using SC2MiM.Common;
using SC2MiM.Common.Services;
using System.Net;
using System.Diagnostics;
using System.Configuration;

namespace SC2WebJob
{
    // To learn more about Microsoft Azure WebJobs SDK, please see http://go.microsoft.com/fwlink/?LinkID=320976
    class Program
    {
        // Please set the following connection strings in app.config for this WebJob to run:
        // AzureWebJobsDashboard and AzureWebJobsStorage
        static void Main()
        {
            JobHost host = new JobHost();

            var method = typeof(Functions).GetMethod("ProcessArgs");

            Task callTask = host.CallAsync(method, new { arg = "/region=eu /target=F" });

            Console.WriteLine("Waiting for async operation...");
            callTask.Wait();
            Console.WriteLine("Task completed: " + callTask.Status);
            Console.ReadLine();
        }
  
    }
}
