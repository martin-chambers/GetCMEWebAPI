using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace GetCMEWebAPI.Models
{
    public class TestService : ITestService
    {
        private readonly IMongoService mongoService;
        /// <summary>
        /// default controller
        /// </summary>
        public TestService(IMongoService mongo)
        {
            mongoService = mongo;
        }
        /// <summary>
        /// Controller takes new Models.Repository by default (will be amended to use DI)
        /// </summary>
        public TestService() : this(new MongoService()) { }


        public async Task RunFTPDownloadAsync(string inputdataId, string datesetId)
        {
            // insert service bus initiation logic here
            // this will cue a continuously running webjob which will resemble the GETCME console app created earlier
            // however, instead of using config files and the file system, it needs to get its config from 
            // MongoDB via rest api calls.
            //await new Task<int>(() => 0); // replace this
        }
        public async Task RunFTPDownloadAsync(string testId)
        {
            //Test t = await 
            // insert service bus initiation logic here
            // this will cue a continuously running webjob which will resemble the GETCME console app created earlier
            // however, instead of using config files and the file system, it needs to get its config from 
            // MongoDB via rest api calls.
            //await new Task<int>(() => 0); // replace this
        }
        public async Task RunTestAsync()
        {
            // insert test run logic here
            // this needs to start a webjob which will resemble the GETCME console app created earlier
            // however, instead of using config files and the file system, it needs to get its config from 
            // MongoDB via rest api calls.
            //await new Task<int>(() => 0); // replace this
        }
        public async Task<Test> GetAsync(string Id)
        {
            Test t = null;
            try
            {
                t = await mongoService.GetTestAsync(Id);
                return t;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return t;
            }
        }

        /// <summary>
        /// Get the test objects
        /// </summary>
        public async Task<IEnumerable<Test>> GetAsync()
        {
            IEnumerable<Test> testObjects = null;
            try
            {
                testObjects = await mongoService.GetTestCollectionAsync();
                return testObjects;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return testObjects;
            }
        }
        /// <summary>
        /// Get a test object for a specified set of inputs
        /// </summary>
        /// <param name="inputdataId"></param>
        /// <param name="datesetId"></param>
        public async Task<Test> GetTestFromInputsAsync(string inputdataId, string datesetId)
        {
            Test rv = null;
            try
            {
                rv = await mongoService.GetTestByInputs(inputdataId, datesetId);
                return rv;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return rv;
            }
        }


    }
}