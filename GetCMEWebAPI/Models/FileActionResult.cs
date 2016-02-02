using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Configuration;
using System.Web.Http;

namespace GetCMEWebAPI.Models
{
    /// <summary>
    /// An HttpActionResult with file data (Used by MongoService Download method)
    /// </summary>
    public class FileActionResult : IHttpActionResult
    {
        public FileActionResult(string fileId)
        {
            ExpectedFileExtension = ConfigurationManager.AppSettings["FileExtension"];
            this.FileId = fileId + "." + ExpectedFileExtension; ;
            this.FileLocation = ConfigurationManager.AppSettings["ResultsFolder"];
        }

        public string FileId { get; private set; }
        public string FileLocation { get; set; }
        public string ExpectedFileExtension { get; set; }

        public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            HttpResponseMessage response = new HttpResponseMessage();
            response.Content = new StreamContent(File.OpenRead(Path.Combine(FileLocation, FileId)));
            response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment") { FileName = FileId };
            return Task.FromResult(response);
        }
    }
}