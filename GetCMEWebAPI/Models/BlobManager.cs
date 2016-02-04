using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;

namespace GetCMEWebAPI.Models
{
    /// <summary>
    /// Blob storage manager class
    /// (Thanks to: http://dotnetspeak.com/2012/08/uploading-directory-to-azure-blob-storage )
    /// </summary>
    public class BlobManager
    {
        private readonly CloudStorageAccount _account;
        private readonly CloudBlobClient _blobClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="BlobManager" /> class.
        /// </summary>
        /// <param name="connectionString">Name of the connection string in app.config or web.config file.</param>
        public BlobManager(string connectionString)
        {
            _account = CloudStorageAccount.Parse(connectionString);

            _blobClient = _account.CreateCloudBlobClient();
            _blobClient.RetryPolicy = RetryPolicies.Retry(4, TimeSpan.Zero);
        }

        /// <summary>
        /// Updates or created a blob in Azure blobl storage
        /// </summary>
        /// <param name="containerName">Name of the container.</param>
        /// <param name="blobName">Name of the blob.</param>
        /// <param name="content">The content of the blob.</param>
        /// <returns></returns>
        public bool PutBlob(string containerName, string blobName, byte[] content)
        {
            return ExecuteWithExceptionHandlingAndReturnValue(
                    () =>
                    {
                        CloudBlobContainer container = _blobClient.GetContainerReference(containerName);
                        CloudBlob blob = container.GetBlobReference(blobName);
                        blob.UploadByteArray(content);
                    });
        }

        /// <summary>
        /// Creates the container in Azure blobl storage
        /// </summary>
        /// <param name="containerName">Name of the container.</param>
        /// <returns>True if contianer was created successfully</returns>
        public bool CreateContainer(string containerName)
        {
            return ExecuteWithExceptionHandlingAndReturnValue(
                    () =>
                    {
                        CloudBlobContainer container = _blobClient.GetContainerReference(containerName);
                        container.Create();
                    });
        }

        /// <summary>
        /// Checks if a container exist.
        /// </summary>
        /// <param name="containerName">Name of the container.</param>
        /// <returns>True if conainer exists</returns>
        public bool DoesContainerExist(string containerName)
        {
            bool returnValue = false;
            ExecuteWithExceptionHandling(
                    () =>
                    {
                        IEnumerable<CloudBlobContainer> containers = _blobClient.ListContainers();
                        returnValue = containers.Any(one => one.Name == containerName);
                    });
            return returnValue;
        }

        /// <summary>
        /// Uploads the directory to blobl storage
        /// </summary>
        /// <param name="sourceDirectory">The source directory name.</param>
        /// <param name="containerName">Name of the container to upload to.</param>
        /// <returns>True if successfully uploaded</returns>
        public bool UploadDirectory(string sourceDirectory, string containerName)
        {
            return UploadDirectory(sourceDirectory, containerName, string.Empty);
        }

        private bool UploadDirectory(string sourceDirectory, string containerName, string prefixAzureFolderName)
        {
            return ExecuteWithExceptionHandlingAndReturnValue(
                () =>
                {
                    if (!DoesContainerExist(containerName))
                    {
                        CreateContainer(containerName);
                    }
                    var folder = new DirectoryInfo(sourceDirectory);
                    var files = folder.GetFiles();
                    foreach (var fileInfo in files)
                    {
                        string blobName = fileInfo.Name;
                        if (!string.IsNullOrEmpty(prefixAzureFolderName))
                        {
                            blobName = prefixAzureFolderName + "/" + blobName;
                        }
                        PutBlob(containerName, blobName, File.ReadAllBytes(fileInfo.FullName));
                    }
                    var subFolders = folder.GetDirectories();
                    foreach (var directoryInfo in subFolders)
                    {
                        var prefix = directoryInfo.Name;
                        if (!string.IsNullOrEmpty(prefixAzureFolderName))
                        {
                            prefix = prefixAzureFolderName + "/" + prefix;
                        }
                        UploadDirectory(directoryInfo.FullName, containerName, prefix);
                    }
                });
        }

        private void ExecuteWithExceptionHandling(Action action)
        {
            try
            {
                action();
            }
            catch (StorageClientException ex)
            {
                if ((int)ex.StatusCode != 409)
                {
                    throw;
                }
            }
        }

        private bool ExecuteWithExceptionHandlingAndReturnValue(Action action)
        {
            try
            {
                action();
                return true;
            }
            catch (StorageClientException ex)
            {
                if ((int)ex.StatusCode == 409)
                {
                    return false;
                }
                throw;
            }
        }
    }
}