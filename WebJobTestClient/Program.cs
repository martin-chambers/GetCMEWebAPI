using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GetCMEWebAPI.Models;
using System.Net.Http;
using Newtonsoft.Json.Linq;

namespace WebJobTestClient
{
    public class Program
    {
        private static string ServiceNamespace;
        private static string sasKeyName = "RootManageSharedAccessKey";
        private static string sasKeyValue;
        private const string QUEUE = "FTPRunQueue";

        public static void Main(string[] args)
        {
            try
            {
                Task t = MainSync();
                t.Wait();
            }
            catch(Exception ex)
            {

                Console.WriteLine(ex.Message + ": - " + ex.InnerException.Message);
                Console.ReadKey();
            }
        }

        public async static Task MainSync()
        {
            ServiceNamespace = ConfigurationManager.AppSettings["ServiceNamespace"];
            // Issuer key
            sasKeyValue = ConfigurationManager.AppSettings["SASKey"];

            // Get test async
            string testId = "56b35007fdfad839d47e0c63";

            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("http://getcmewebapi.azurewebsites.net/");
            Task<string> testTask = client.GetStringAsync("api/v1/Test/" + testId);

            // do other stuff while we're waiting for the test ...

            // Create management credentials
            TokenProvider credentials = TokenProvider.CreateSharedAccessSignatureTokenProvider(sasKeyName, sasKeyValue);
            NamespaceManager namespaceClient =
                new NamespaceManager(
                    ServiceBusEnvironment.CreateServiceUri("sb", ServiceNamespace, string.Empty),
                    credentials);
            QueueDescription myQueue;
            if (!namespaceClient.QueueExists(QUEUE))
            {
                myQueue = namespaceClient.CreateQueue(QUEUE);
            }
            MessagingFactory factory = MessagingFactory.Create(ServiceBusEnvironment.CreateServiceUri("sb", ServiceNamespace, string.Empty), credentials);
            QueueClient myQueueClient = factory.CreateQueueClient(QUEUE);

            // Send message
            BrokeredMessage message = new BrokeredMessage();

            // we need the test now ...
            var testJsonString = await testTask;
            Test test = JObject.Parse(testJsonString).ToObject<Test>();

            message.Properties.Add("Id", test.Id);
            message.Properties.Add("TestId", test.InputDataId);
            message.Properties.Add("DatesetId", test.DateSetId);
            myQueueClient.Send(message);
        }
    }
}
