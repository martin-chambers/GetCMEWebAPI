using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Configuration;

namespace GetCMEWebAPI.Models
{
    public static class BlobHelper
    {
        public static CloudBlobContainer GetBlobContainer()
        {
            // Pull these from config
            var blobStorageConnectionString = 
                ConfigurationManager.ConnectionStrings["AzureStorageConnectionString"].ToString();
            var blobStorageContainerName = ConfigurationManager.AppSettings["AzureContainerName"];

            // Create blob client and return reference to the container
            var blobStorageAccount = CloudStorageAccount.Parse(blobStorageConnectionString);
            var blobClient = blobStorageAccount.CreateCloudBlobClient();
            return blobClient.GetContainerReference(blobStorageContainerName);
        }

        public static string BlobSuffix()
        {
            return ConfigurationManager.AppSettings["FileSuffix"];
        }

    }
}