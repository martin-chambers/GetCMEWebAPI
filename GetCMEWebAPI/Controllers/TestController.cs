using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using GetCMEWebAPI.Models;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using System.Web;

namespace GetCMEWebAPI.Controllers
{
    public class TestController : ApiController
    {
        private const bool USING_BLOB_STORAGE = true;
        private readonly IMongoService mongoService;
        private readonly IBlobService blobService;
        private readonly ITestService testservice;
        /// <summary>
        /// default controller
        /// </summary>
        public TestController(IMongoService mongo, IBlobService blob, ITestService test)
        {
            mongoService = mongo;
            blobService = blob;
            testservice = test;
        }
        /// <summary>
        /// Controller takes new Models.Repository by default (will be amended to use DI)
        /// </summary>
        public TestController() : this(new MongoService(), new BlobService(), new TestService()) { }

        //  GET: api/v1/test/568b9cbefbfd383c642a6dde/568b9cbefbfd383c642d4abb
        /// <summary>
        /// Get a Test file from file storage for a specified inputdata and dateset Id
        /// </summary>
        /// <remarks></remarks>
        /// <param name="inputid">
        /// <Description>A SHA-1 string id</Description>
        /// </param>
        /// <param name="datesetid">
        /// <Description>A SHA-1 string id</Description>
        /// </param>
        [NonAction]
        public IHttpActionResult GetFile(string inputid, string datesetid)
        {
            return mongoService.Download(inputid, datesetid);
        }

        //  GET: api/v1/test/568b9cbefbfd383c642a6dde
        /// <summary>
        /// Get a Test file from Azure blob storage for a specified Test Id
        /// </summary>
        /// <param name="Id"></param>
        [Route("api/v1/Test/Download/{Id}")]
        [HttpGet]
        public async Task<HttpResponseMessage> GetBlobFromStorageAsync(string Id)
        {
            string blobReference = Id;
            try
            {
                var result = await blobService.DownloadBlob(blobReference);
                if (result == null)
                {
                    return new HttpResponseMessage(HttpStatusCode.NotFound);
                }
                // Reset the stream position; otherwise, download will not work
                result.BlobStream.Position = 0;

                // Create response message with blob stream as its content
                var message = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StreamContent(result.BlobStream)
                };

                // Set content headers
                message.Content.Headers.ContentLength = result.BlobLength;
                message.Content.Headers.ContentType = new MediaTypeHeaderValue(result.BlobContentType);
                message.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                {
                    FileName = HttpUtility.UrlDecode(result.BlobFileName),
                    Size = result.BlobLength
                };

                return message;
            }
            catch (Exception ex)
            {
                return new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Content = new StringContent(ex.Message)
                };
            }
        }

        /// <summary>
        /// Get a test object for a specified set of inputs
        /// </summary>
        /// <param name="inputdataId"></param>
        /// <param name="datesetId"></param>
        /// <returns></returns>
        [Route("api/v1/Test/inputdata/{inputdataId}/dateset/{datesetId}")]
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

        [NonAction]
        public async Task RunTestAsync(string inputdataid, string datesetId)
        {
            await testservice.RunFTPDownloadAsync(inputdataid, datesetId);
        }
        [NonAction]
        public async Task RunTestAsync(string testId)
        {
            await testservice.RunFTPDownloadAsync(testId);
        }

        // GET: api/v1/test/568b9cbefbfd383c642a6dde/
        /// <summary>
        /// Get a test object for a specified Id
        /// </summary>
        /// <remarks></remarks>
        /// <param name="Id">
        /// <Description>A SHA-1 string id</Description>
        /// </param>
        public async Task<Test> GetAsync(string Id)
        {
            Test rv = null;
            try
            {
                rv = await mongoService.GetTestAsync(Id);
                return rv;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return rv;
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
        /// Records and runs a test  (running test is not yet implemented)
        /// </summary>
        /// <param name="inputdataId"></param>
        /// <param name="datesetId"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/v1/Test/inputdata/{inputdataId}/dateset/{datesetId}")]
        public async Task<HttpResponseMessage> InsertAsync(string inputdataId, string datesetId)
        {
            //await RunTestAsync(inputdataId, datesetId);
            if (string.IsNullOrEmpty(inputdataId) || string.IsNullOrEmpty(datesetId))
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }
            Task<Test> task = mongoService.InsertTestAsync(inputdataId, datesetId);
            try
            {
                await task;                
                return Request.CreateResponse(HttpStatusCode.Created, task.Result);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return Request.CreateResponse(HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Runs a test for a previously stored test configuration (not yet implemented)
        /// </summary>
        /// <param name="testId"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/v1/Test/{testId}")]
        public async Task<HttpResponseMessage> RunExistingAsync(string testId)
        {
            Test test = await mongoService.GetTestAsync(testId);
            if (test != null)
            {
                try
                {
                    //await RunTestAsync(test.InputDataId, test.DateSetId);
                    return Request.CreateResponse(HttpStatusCode.OK);
                }
                catch(Exception ex)
                {
                    return Request.CreateResponse(HttpStatusCode.InternalServerError);
                }
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.NotFound);
            }
        }

        /// <summary>
        /// Create a Test object
        /// </summary>
        /// <remarks>This is for admin use. The normal method for creating a test object in the data store will be by running a test.</remarks>
        /// <param name="test"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<HttpResponseMessage> InsertAsync([FromBody] Test test)
        {
            if (test == null)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }
            Task<Test> task = mongoService.InsertTestAsync(test.InputDataId, test.DateSetId);
            try
            {
                await task;
                return Request.CreateResponse(HttpStatusCode.Created, task.Result);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return Request.CreateResponse(HttpStatusCode.InternalServerError);
            }
        }




    }
}
