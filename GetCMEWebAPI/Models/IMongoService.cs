using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;

namespace GetCMEWebAPI.Models
{
    public interface IMongoService
    {
        // (1) inputdata methods

        Task<IEnumerable<InputData>> GetInputDataCollectionAsync();
        Task<InputData> GetInputDataAsync(string Id);
        Task<InputData> AddInputDataAsync(InputData item);
        Task<long> RemoveInputDataAsync(string Id);
        Task<long> UpdateInputDataAsync(InputData item);

        // (2) date set methods

        Task<DateSet> GetDateSetAsync(string Id);
        Task<IEnumerable<DateSet>> GetDateSetCollectionAsync();
        Task<DateSet> AddDateSetAsync(DateSet item);
        Task<long> RemoveDateSetAsync(string Id);

        // (3) test methods
        Task<Test> InsertTestAsync(string input, string dateset);
        IHttpActionResult Download(string input, string dateset);
        Task<Test> GetTestAsync(string Id);
        Task<Test> GetTestByInputs(string inputdataId, string datesetId);
        Task<IEnumerable<Test>> GetTestCollectionAsync();

    }
}