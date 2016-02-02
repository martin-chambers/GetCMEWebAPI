using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Threading.Tasks;
using GetCMEWebAPI.Models;

namespace GetCMEWebAPI.Controllers
{
    public class DateSetController : ApiController
    {
        private readonly IMongoService repository;
        /// <summary>
        /// default controller
        /// </summary>
        public DateSetController(MongoService _repository)
        {
            repository = _repository;
        }
        /// <summary>
        /// Controller takes new Models.Repository by default (will be amended to use DI)
        /// </summary>
        public DateSetController() : this(new MongoService()) { }

        // GET: api/v1/datesets
        /// <summary>
        /// Get the DateSet objects
        /// </summary>
        /// <remarks>
        /// The endpoint returns a Json representation of a list of DateSet objects
        /// </remarks>
        public async Task<IEnumerable<DateSet>> Get()
        {
            IEnumerable<DateSet> dateSetObjects = null;
            try
            {
                dateSetObjects = await repository.GetDateSetCollectionAsync(); // does this need explicit cast? (Has one in WebAPIDemo)
                return dateSetObjects;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return dateSetObjects;
            }
        }
        // GET: api/v1/datesets/568b9cbefbfd383c642a6dde/
        /// <summary>
        /// Get a DateSet object for a specified Id
        /// </summary>
        /// <remarks></remarks>
        /// <param name="Id">
        /// <Description>A SHA-1 string id</Description>
        /// </param>
        public async Task<DateSet> Get(string Id)
        {
            DateSet rv = null;
            try
            {
                rv = await repository.GetDateSetAsync(Id);
                return rv;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return rv;
            }
        }
        // POST: api/v1/datesets
        /// <summary>
        /// Create a DateSet object
        /// </summary>
        /// <remarks>The endpoint returns a Json representation of the object created in the data source</remarks>
        /// <param name="dateset"></param>
        [System.Web.Http.HttpPost]
        public async Task<HttpResponseMessage> Insert([FromBody] DateSet dateset)
        {
            if (dateset == null)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }
            Task<DateSet> task = repository.AddDateSetAsync(dateset);
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
        // DELETE: api/v1/datesets
        /// <summary>
        /// Delete a DateSet object
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
                Task<long> deleteResult = repository.RemoveDateSetAsync(Id);
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
