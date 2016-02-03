using System.Collections.Generic;
using System.Threading.Tasks;

namespace GetCMEWebAPI.Models
{
    public interface ITestService
    {
        Task RunFTPDownloadAsync(string inputdataId, string datesetId);
        Task RunFTPDownloadAsync(string testId);
        Task RunTestAsync();
        Task<Test> GetAsync(string Id);
        Task<IEnumerable<Test>> GetAsync();
        Task<Test> GetTestFromInputsAsync(string inputdataId, string datesetId);
    }
}