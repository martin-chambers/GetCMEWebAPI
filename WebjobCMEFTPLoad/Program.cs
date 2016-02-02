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

namespace WebjobCMEFTPLoad
{
    class Program
    {
        private static string ServiceNamespace;
        private static string sasKeyName = "RootManageSharedAccessKey";
        private static string sasKeyValue;

        static void Main(string[] args)
        {
            Task t = MainAsync(args);
            t.Wait();        }

        static async Task MainAsync(string[] args)
        {
            string testId = args[0];
            HttpClient client = new HttpClient();
            client.Timeout = new TimeSpan(1, 0, 0);
            client.BaseAddress = new Uri("https://getcmewebapi.azurewebsites.net/");
            // we can't proceed till both inputdata and dateset IDs are available - so await
            Test test = await getTestAsync(client, testId);
            string inputdataId = test.InputDataId;
            string datesetId = test.DateSetId;
            // each task will proceed without blocking the other
            Task<DateSet> dateSetTask = getDatesetAsync(client, datesetId);
            Task<InputData> inputdataTask = getInputdataAsync(client, inputdataId);
            InputData data = await inputdataTask;
            DateSet workingdates = await dateSetTask;

            QueueDescription myQueue;
            Queue();
            

            JobHostConfiguration config = new JobHostConfiguration();
            config.UseServiceBus();
            JobHost host = new JobHost(config);
            host.RunAndBlock();
        }

        static void Queue()
        {
            // Create management credentials
            TokenProvider credentials = TokenProvider.CreateSharedAccessSignatureTokenProvider(sasKeyName, sasKeyValue);
            NamespaceManager namespaceClient = new NamespaceManager(ServiceBusEnvironment.CreateServiceUri("sb", ServiceNamespace, string.Empty), credentials);
            QueueDescription myQueue;
            MessagingFactory factory = MessagingFactory.Create(ServiceBusEnvironment.CreateServiceUri("sb", ServiceNamespace, string.Empty), credentials);
            myQueue = namespaceClient.CreateQueue("Testqueue");

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
    }
}
