using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace GetCMEWebAPI.Models
{
    public class BlobService : IBlobService
    {
        // much of the BlobService implementation is based on the following blog:
        // http://arcware.net/upload-and-download-files-with-web-api-and-azure-blob-storage/

        public async Task<BlobDownloadModel> DownloadBlob(string blobReference)
        {
            var blobName = blobReference + BlobHelper.BlobSuffix();

            try
            {

                if (!String.IsNullOrEmpty(blobName))
                {
                    CloudBlobContainer container = BlobHelper.GetBlobContainer();
                    var blob = container.GetBlockBlobReference(blobName);

                    // Download the blob into a memory stream. Notice that we're not putting the memory
                    // stream in a using statement. This is because we need the stream to be open for the
                    // API controller in order for the file to actually be downloadable. The closing and
                    // disposing of the stream is handled by the Web API framework.
                    var ms = new MemoryStream();
                    await blob.DownloadToStreamAsync(ms);

                    // Strip off any folder structure so the file name is just the file name
                    var lastPos = blob.Name.LastIndexOf('/');
                    var fileName = blob.Name.Substring(lastPos + 1, blob.Name.Length - lastPos - 1);

                    // Build and return the download model with the blob stream and its relevant info
                    var download = new BlobDownloadModel
                    {
                        BlobStream = ms,
                        BlobFileName = fileName,
                        BlobLength = blob.Properties.Length,
                        BlobContentType = blob.Properties.ContentType
                    };

                    return download;
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
            // Otherwise
            return null;
        }
        public async Task<List<BlobUploadModel>> UploadBlobs(HttpContent httpContent)
        {
            var blobUploadProvider = new BlobStorageUploadProvider();

            var list = await httpContent.ReadAsMultipartAsync(blobUploadProvider)
                .ContinueWith(task =>
                {
                    if (task.IsFaulted || task.IsCanceled)
                    {
                        throw task.Exception;
                    }

                    var provider = task.Result;
                    return provider.Uploads.ToList();
                });

            // TODO: Use data in the list to store blob info in your
            // database so that you can always retrieve it later.

            return list;
        }
    }
}