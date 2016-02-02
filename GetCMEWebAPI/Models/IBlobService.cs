using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Http;
using Microsoft.WindowsAzure.Storage.Blob;

namespace GetCMEWebAPI.Models
{
    public interface IBlobService
    {
        Task<List<BlobUploadModel>> UploadBlobs(HttpContent httpContent);
        Task<BlobDownloadModel> DownloadBlob(string blobId);
    }
}
