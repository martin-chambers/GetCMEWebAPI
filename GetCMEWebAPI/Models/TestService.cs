using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace GetCMEWebAPI.Models
{
    public class TestService : ITestService
    {
        public async Task RunFTPDownloadAsync(string inputdataId, string datesetId)
        {
            // insert service bus initiation logic here
            // this will cue a continuously running webjob which will resemble the GETCME console app created earlier
            // however, instead of using config files and the file system, it needs to get its config from 
            // MongoDB via rest api calls.
            //await new Task<int>(() => 0); // replace this
        }
        public async Task RunFTPDownloadAsync(string testId)
        {
            // insert service bus initiation logic here
            // this will cue a continuously running webjob which will resemble the GETCME console app created earlier
            // however, instead of using config files and the file system, it needs to get its config from 
            // MongoDB via rest api calls.
            //await new Task<int>(() => 0); // replace this
        }
        public async Task RunTestAsync()
        {
            // insert test run logic here
            // this needs to start a webjob which will resemble the GETCME console app created earlier
            // however, instead of using config files and the file system, it needs to get its config from 
            // MongoDB via rest api calls.
            //await new Task<int>(() => 0); // replace this
        }
    }
}