using System.Threading.Tasks;

namespace GetCMEWebAPI.Models
{
    public interface ITestService
    {
        Task RunFTPDownloadAsync(string inputdataId, string datesetId);
        Task RunFTPDownloadAsync(string testId);
        Task RunTestAsync();
    }
}