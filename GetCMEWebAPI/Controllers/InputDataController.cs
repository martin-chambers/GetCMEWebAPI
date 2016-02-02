using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Threading.Tasks;
using GetCMEWebAPI.Models;

namespace GetCMEWebAPI.Controllers
{
    /// <summary>
    /// Controller for InputData endpoints
    /// </summary>
    public class InputDataController : ApiController
    {
        private readonly IMongoService repository;
        /// <summary>
        /// default controller
        /// </summary>
        public InputDataController(MongoService _repository)
        {
            repository = _repository;
        }
        /// <summary>
        /// Controller takes new Models.Repository by default (will be amended to use DI)
        /// </summary>
        public InputDataController() : this(new MongoService()) { }

        // GET: api/v1/inputdata
        /// <summary>
        /// Get the InputData objects
        /// </summary>
        /// <remarks>
        /// The endpoint returns a Json representation of a list of InputData objects
        /// </remarks>
        public async Task<IEnumerable<InputData>> Get()
        {
            IEnumerable<InputData> inputDataObjects = null;
            try
            {
                inputDataObjects = await repository.GetInputDataCollectionAsync(); // does this need explicit cast? (Has one in WebAPIDemo)
                return inputDataObjects;
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                return inputDataObjects;
            }
        }

        // GET: api/v1/inputdata/568b9cbefbfd383c642a6dde/
        /// <summary>
        /// Get an InputData object for a specified Id
        /// </summary>
        /// <remarks></remarks>
        /// <param name="Id">
        /// <Description>A SHA-1 string id</Description>
        /// </param>
        public async Task<InputData> Get(string Id)
        {
            InputData rv = null;
            try
            {
                rv = await repository.GetInputDataAsync(Id);
                return rv;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return rv;
            }
        }
        // POST: api/v1/inputdata
        /// <summary>
        /// Create an InputData object
        /// </summary>
        /// <remarks>The endpoint returns a Json representation of the object created in the data source</remarks>
        /// <param name="inputdata"></param>
        [System.Web.Http.HttpPost]
        public async Task<HttpResponseMessage> Insert([FromBody] InputData inputdata)
        {
            if (inputdata == null)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }
            Task<InputData> task = repository.AddInputDataAsync(inputdata);
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
        // DELETE: api/v1/inputdata
        /// <summary>
        /// Delete an InputData object
        /// </summary>
        /// <param name="Id">
        /// <Description>A SHA-1 string id</Description>
        /// </param>
        [System.Web.Http.HttpDelete]        
        public async Task<HttpResponseMessage> Delete(string Id)
        {
            if (Id == null)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }
            try
            {
                Task<long> deleteResult = repository.RemoveInputDataAsync(Id);
                await deleteResult;
                if (deleteResult.Result < 1)
                {
                    Console.WriteLine("No documents matched delete query");
                    return Request.CreateResponse(HttpStatusCode.NotFound);
                }
                else
                {
                    if (deleteResult.Result > 1)
                    {
                        // should not happen: Ids are unique
                        Console.WriteLine("More than one document matched delete query");
                        return Request.CreateResponse(HttpStatusCode.InternalServerError);
                    }
                }
                return Request.CreateResponse(HttpStatusCode.NoContent);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return Request.CreateResponse(HttpStatusCode.InternalServerError);
            }
        }
    }
}