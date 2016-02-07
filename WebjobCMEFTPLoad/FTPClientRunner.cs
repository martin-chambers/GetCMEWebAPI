using GetCMEWebAPI.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebjobCMEFTPLoad
{
    public class FTPClientRunner
    {
        public DateSet DateSet { get; set; }
        public InputData InputData { get; set; }
        public string TestId { get; set; }
        public FTPClientRunner(DateSet dateset, InputData inputdata, string testId)
        {
            DateSet = dateset;
            InputData = inputdata;
            TestId = testId;
        }
        private string getUrlForFile(string fileToSearch, string url, string user, string password, FTPClient client)
        {
            string foundUrl = "";
            List<string> fileEntries = client.DirectoryListing(url);
            if (fileEntries.Contains(fileToSearch))
            {
                return url;
            }
            else
            {
                // look in all subdirectories recursively
                foreach (string fileEntry in fileEntries.Where(x => x.Contains(".") == false))
                {
                    if (foundUrl == "")
                    {
                        foundUrl = getUrlForFile(fileToSearch, url + fileEntry + "/", user, password, client);
                    }
                }
            }
            return foundUrl;
        }
        private static int stringToInt(string i)
        {
            return Convert.ToInt32(i);
        }

        private string getTempDirectory()
        {
            string path = Path.GetRandomFileName();
            Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), path));
            return path;
        }

        public async Task RunAsync()
        {
            // get FTPClient config values
            string host = ConfigurationManager.AppSettings["FTPHost"];
            string tempPath = getTempDirectory(); ;
            //string approot = Path.Combine(tempPath, "approot");
            //string workingpath = Path.Combine(tempPath, "working");
            string downloadfolder = Path.Combine(tempPath, "Downloads");
            string datafolder = Path.Combine(tempPath, "Data");
            string datasubfolder = "InputDataFiles";
            string logfolder = Path.Combine(tempPath, "Logs");
            string logfile = ConfigurationManager.AppSettings["LogFileName"];
            string logPath = Path.Combine(logfolder, logfile);
            string user = ConfigurationManager.AppSettings["User"];
            string password = ConfigurationManager.AppSettings["Password"];
            bool deleteZips = Convert.ToBoolean(ConfigurationManager.AppSettings["DeleteZips"]);

            // get details of fileformat. Must have length 2, to give a pre-date and post-date part 
            string[] fileformat = InputData.Pattern.Split(new char[] { ';' });
            if (fileformat.Length != 2)
            {
                throw new InvalidOperationException("Configuration problem: file format is unexpected");
            }
            string firstFilePart = fileformat[0];
            string lastFilePart = fileformat[1];

            // get date search logic values 
            int[] months = Array.ConvertAll(InputData.Months.Split(new char[] { ';' }),
                    new Converter<string, int>(stringToInt));

            int decrementLimit = InputData.DateDecrement;
            List<string> folderlist = ConfigurationManager.AppSettings["NewFolderList"].Split(new char[] { ';' }).ToList();

            // create FTP client
            FTPClient client = new FTPClient(user, password, downloadfolder, logPath);

            string azureConnection = ConfigurationManager.ConnectionStrings["AzureWebJobsStorage"].ToString();
            BlobManager manager = new BlobManager(azureConnection);

            List<string> dates = DateSet.DateList;
            foreach (string line in dates)
            {
                string basedate = line;
                string dataRoot = Path.Combine(datafolder, basedate);
                DateTime startdate = new DateTime(
                    Convert.ToInt32(basedate.Substring(0, 4)),
                    Convert.ToInt32(basedate.Substring(4, 2)),
                    Convert.ToInt32(basedate.Substring(6, 2))
                );
                TDateSet tdates = new TDateSet(startdate, months);
                for (int i = 0; i < tdates.Length; i++)
                {
                    string url = "";
                    string downloadDestination = Path.Combine(downloadfolder, basedate);
                    string dataDestination = Path.Combine(dataRoot, datasubfolder);
                    DateTime searchDate = tdates.Dates[i];
                    string filename = "";
                    int d = 0;
                    while (url == "" && d <= decrementLimit)
                    {
                        filename = firstFilePart + searchDate.ToString("yyyyMMdd") + lastFilePart;
                        await client.LogAsync("Searching for " + filename + " ... ");
                        url = getUrlForFile(filename, host, user, password, client);
                        if (url == "")
                        {
                            await client.LogAsync("Could not find file " + filename + ". Decrementing date ...");
                        }
                        searchDate = searchDate.Decrement();
                        d++;
                    }
                    // still not found ?
                    if (url == "")
                    {
                        await client.LogAsync("Input error: " + filename + " was not found in " + url + " or any of the subfolders");
                    }
                    else
                    {
                        // FTP download
                        try
                        {
                            await client.LogAsync(client.DownloadingSummary(filename, url, downloadDestination));
                            await client.DownloadAsync(downloadDestination, filename, url);
                            await client.LogAsync(client.DownloadedSummary(filename, url, downloadDestination));

                        }
                        catch (Exception ex)
                        {
                            await client.LogAsync(client.DownloadErrorSummary(filename, host, downloadDestination, ex.Message));
                        }
                        // Unzip
                        try
                        {
                            await client.UnzipAsync(filename, downloadDestination, dataRoot, dataDestination, deleteZips, folderlist);
                            Task a = client.LogAsync(client.UnzipSummary(filename, dataDestination));
                            await a;
                        }
                        catch (Exception ex)
                        {
                            Task a = client.LogAsync(client.UnzipErrorSummary(filename, downloadDestination, dataDestination, ex.Message));
                            await a;
                        }
                    }                    
                }
                var success = manager.UploadDirectory(dataRoot, TestId, basedate);
            }
        }
    }
}
