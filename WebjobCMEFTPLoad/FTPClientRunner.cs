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

        public void Run()
        {
            // get FTPClient config values
            string host = ConfigurationManager.AppSettings["FTPHost"];
            string tempPath = Path.GetTempPath();
            string approot = Path.Combine(tempPath, "approot");
            string workingpath = Path.Combine(tempPath, "working");
            string downloadfolder = Path.Combine(tempPath, "Downloads");
            string datafolder = Path.Combine(tempPath, "Data");
            string datasubfolder = Path.Combine(datafolder, "InputDataFiles");
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

            List<string> dates = DateSet.DateList;
            foreach (string line in dates)
            {
                string basedate = line;
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
                    string dataRoot = Path.Combine(datafolder, basedate);
                    string dataDestination = Path.Combine(dataRoot, datasubfolder);
                    DateTime searchDate = tdates.Dates[i];
                    string filename = "";
                    int d = 0;
                    while (url == "" && d <= decrementLimit)
                    {
                        filename = firstFilePart + searchDate.ToString("yyyyMMdd") + lastFilePart;
                        client.Log("Searching for " + filename + " ... ");
                        url = getUrlForFile(filename, host, user, password, client);
                        if (url == "")
                        {
                            client.Log("Could not find file " + filename + ". Decrementing date ...");
                        }
                        searchDate = searchDate.Decrement();
                        d++;
                    }
                    // still not found ?
                    if (url == "")
                    {
                        client.Log("Input error: " + filename + " was not found in " + url + " or any of the subfolders");
                    }
                    else
                    {
                        // FTP download
                        try
                        {
                            client.Log(client.DownloadingSummary(filename, url, downloadDestination));
                            client.Download(downloadDestination, filename, url);
                            client.Log(client.DownloadedSummary(filename, url, downloadDestination));
                        }
                        catch (Exception ex)
                        {
                            client.Log(client.DownloadErrorSummary(filename, host, downloadDestination, ex.Message));
                        }
                        // Unzip
                        try
                        {
                            client.Unzip(filename, downloadDestination, dataRoot, dataDestination, deleteZips, folderlist);
                            client.Log(client.UnzipSummary(filename, dataDestination));
                        }
                        catch (Exception ex)
                        {
                            client.Log(client.UnzipErrorSummary(filename, downloadDestination, dataDestination, ex.Message));
                        }
                    }
                }
                string azureConnection = ConfigurationManager.AppSettings["AzureWebJobsStorage"];
                BlobManager manager = new BlobManager(azureConnection);
                var success = manager.UploadDirectory(tempPath, TestId);


            }
        }
    }
}
