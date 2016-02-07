using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GetCMEWebAPI.Models;
using System.Net.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.ServiceBus.Messaging;
using Newtonsoft.Json.Linq;
using Microsoft.ServiceBus;
using System.Configuration;
using Microsoft.Azure.WebJobs.ServiceBus;

namespace WebjobCMEFTPLoad
{
    public class Program
    {
        private static string sbConnectionString = 
            ConfigurationManager.ConnectionStrings["AzureWebJobsServiceBus"].ToString();

        public static void Main(string[] args)
        {
            JobHostConfiguration config = new JobHostConfiguration();
            ServiceBusConfiguration servicebusConfig = new ServiceBusConfiguration
            {
                ConnectionString = sbConnectionString
            };
            config.UseServiceBus(servicebusConfig);
            JobHost host = new JobHost(config);
            host.RunAndBlock();
        }


        static async Task<Test> getTestAsync(HttpClient client, string TestId)
        {
            string testString = await client.GetStringAsync("/api/v1/test/" + TestId);
            Test test = JObject.Parse(testString).ToObject<Test>();
            return test;
        }

        static async Task<DateSet> getDatesetAsync(HttpClient client, string datesetId)
        {
            string datesetString = await client.GetStringAsync("/api/v1/dateset/" + datesetId);
            DateSet dateset = JObject.Parse(datesetString).ToObject<DateSet>();
            return dateset;
        }
        static async Task<InputData> getInputdataAsync(HttpClient client, string inputdataId)
        {
            string datesetString = await client.GetStringAsync("/api/v1/inputdata/" + inputdataId);
            InputData inputdata = JObject.Parse(datesetString).ToObject<InputData>();
            return inputdata;
        }

        public static async Task GetMessageAndRunAsync([ServiceBusTrigger("FTPRunQueue")] BrokeredMessage message)
        {
            string testId = message.Properties["TestId"].ToString(); ;
            HttpClient client = new HttpClient();
            client.Timeout = new TimeSpan(0, 1, 0);
            client.BaseAddress = new Uri(ConfigurationManager.AppSettings["BaseAddress"]);
            // we can't proceed till both inputdata and dateset IDs are available - so await
            string testJsonString = client.GetStringAsync("api/v1/Test/" + testId).Result;
            // we need the test now ...
            //var testJsonString = await testTask;
            Test test = JObject.Parse(testJsonString).ToObject<Test>();
            string inputdataId = test.InputDataId;
            string datesetId = test.DateSetId;
            // each task will proceed without blocking the other
            Task<DateSet> dateSetTask = getDatesetAsync(client, datesetId);
            Task<InputData> inputdataTask = getInputdataAsync(client, inputdataId);
            InputData data = await inputdataTask;
            DateSet workingdates = await dateSetTask;
            FTPClientRunner runner = new FTPClientRunner(workingdates, data, testId);
            await runner.RunAsync();
        }
    }
}
