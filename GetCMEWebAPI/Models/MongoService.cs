using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Bson;
using System.Configuration;
using System.Web.Http;
using GetCMEWebAPI.Models;
using System.Net.Http;

namespace GetCMEWebAPI.Models
{
    /// <summary>
    /// MongoDB repository
    /// </summary>
    public class MongoService : IMongoService
    {
        MongoClient client;
        IMongoDatabase database;
        IMongoCollection<InputData> inputdata;
        IMongoCollection<DateSet> datesets;
        IMongoCollection<Test> tests;

        // useful resources:
        // http://mongodb.github.io/mongo-csharp-driver/2.0/getting_started/quick_tour/
        // http://dotnetcodr.com/data-storage/


        /// <summary>
        /// Default constructor for the repository
        /// </summary>
        public MongoService()
        {
            string connection = ConfigurationManager.ConnectionStrings["MongoDB"].ConnectionString;
            client = new MongoClient(connection);
            database = client.GetDatabase("CMEdata");
            inputdata = database.GetCollection<InputData>("inputdata");
            datesets = database.GetCollection<DateSet>("datesets");
            tests = database.GetCollection<Test>("tests");
        }

        // InputData object /////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Insert an InputData object
        /// </summary>
        /// <param name="item"></param>
        public async Task<InputData> AddInputDataAsync(InputData item)
        {
            item.Id = MongoDB.Bson.ObjectId.GenerateNewId().ToString();
            await inputdata.InsertOneAsync(item);
            return item;
        }
 
        /// <summary>
        /// Get an InputData object for an Id
        /// </summary>
        /// <param name="Id"></param>
        public async Task<InputData> GetInputDataAsync(string Id)
        {
            var filter = Builders<InputData>.Filter.Eq("Id", Id);
            InputData rv = await inputdata.Find(filter).FirstAsync();
            return rv;
        }
        /// <summary>
        /// Get all InputData objects
        /// </summary>
        public async Task<IEnumerable<InputData>> GetInputDataCollectionAsync()
        {
            IEnumerable<InputData> valueList = await inputdata.Find(new BsonDocument()).ToListAsync();
            return valueList;
        }

        /// <summary>
        /// Remove an InputData object specified by an Id
        /// </summary>
        /// <param name="Id">
        /// <Description>A SHA-1 string id</Description>
        /// </param>
        public async Task<long> RemoveInputDataAsync(string Id)
        {
            var filter = Builders<InputData>.Filter.Eq("Id", Id);
            Task<DeleteResult> deleteResult = inputdata.DeleteOneAsync(filter);
            await deleteResult;
            long matchedCount = deleteResult.Result.DeletedCount;
            return matchedCount;
        }
        /// <summary>
        /// Updates an InputData object specified by an Id
        /// </summary>
        /// <param name="item">
        /// <Description>A SHA-1 string id</Description>
        /// </param>
        public async Task<long> UpdateInputDataAsync(InputData item)
        {
            var filter = Builders<InputData>.Filter.Eq("Id", item.Id);
            var update = Builders<InputData>.Update
                .Set("Months", item.Months)
                .Set("Pattern", item.Pattern)
                .Set("DateDecrement", item.DateDecrement);
            Task<UpdateResult> updateResult = inputdata.UpdateOneAsync(filter, update);
            await updateResult;
            long matchedCount = updateResult.Result.MatchedCount;
            return matchedCount;
        }

        // DateSet object ///////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// insert a DateSet object
        /// </summary>
        /// <param name="item"></param>
        public async Task<DateSet> AddDateSetAsync(DateSet item)
        {
            item.Id = MongoDB.Bson.ObjectId.GenerateNewId().ToString();
            await datesets.InsertOneAsync(item);
            return item;
        }

        /// <summary>
        /// Removes a DateSet object specified by an Id
        /// </summary>
        /// <param name="Id">
        /// <Description>A SHA-1 string id</Description>
        /// </param>
        public async Task<long> RemoveDateSetAsync(string Id)
        {
            var filter = Builders<DateSet>.Filter.Eq("Id", Id);
            Task<DeleteResult> deleteResult = datesets.DeleteOneAsync(filter);
            await deleteResult;
            long matchedCount = deleteResult.Result.DeletedCount;
            return matchedCount;
        }

        /// <summary>
        /// Gets a DateSet object for an Id
        /// </summary>
        /// <param name="Id"></param>
        public async Task<DateSet> GetDateSetAsync(string Id)
        {
            var filter = Builders<DateSet>.Filter.Eq("Id", Id);
            DateSet rv = await datesets.Find(filter).FirstAsync();
            return rv;
        }

        /// <summary>
        /// Gets all DateSet objects
        /// </summary>
        public async Task<IEnumerable<DateSet>> GetDateSetCollectionAsync()
        {
            IEnumerable<DateSet> valueList = await datesets.Find(new BsonDocument()).ToListAsync(); // there isn't a ToEnumerableAsync()
            return valueList;
        }

        // Test object //////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Insert a test object and run a test
        /// </summary>
        /// <param name="input"></param>
        /// <param name="dateset"></param>
        /// <returns></returns>
        public async Task<Test> InsertTestAsync(string input, string dateset)
        {

            Test item = new Test();
            item.Id = MongoDB.Bson.ObjectId.GenerateNewId().ToString();
            item.InputDataId = input;
            item.DateSetId = dateset;
            await tests.InsertOneAsync(item);
            return item;
        }

        /// <summary>
        /// Get the test result file
        /// </summary>
        /// <param name="inputId"></param>
        /// <param name="datesetId"></param>
        public IHttpActionResult Download(string inputId, string datesetId)
        {
            string fileId = inputId + "-" + datesetId;
            return new FileActionResult(fileId);
        }

        public async Task<Test> GetTestAsync(string Id)
        {
            var filter = Builders<Test>.Filter.Eq("Id", Id);
            Test rv = await tests.Find(filter).FirstAsync();
            return rv;
        }

        public async Task<Test> GetTestByInputs(string inputdataId, string datesetId)
        {
            var filter = Builders<Test>.Filter.Eq("InputDataId", inputdataId) & Builders<Test>.Filter.Eq("DateSetId", datesetId);
            Test rv = await tests.Find(filter).FirstAsync();
            return rv;
        }

        public async Task<IEnumerable<Test>> GetTestCollectionAsync()
        {
            IEnumerable<Test> valueList = await tests.Find(new BsonDocument()).ToListAsync();
            return valueList;
        }

        

    }
}