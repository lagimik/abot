using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Abot2.Core;
using Abot2.Crawler;
using Abot2.Poco;
using CrucibleCrawler.Blob;


namespace CrucibleCrawler.Function
{
    public static class HttpTrigger
    {
        [FunctionName("HttpTrigger")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string name = req.Query["name"];
            string url = req.Query["url"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            name = name ?? data?.name;

       
            string responseMessage = string.IsNullOrEmpty(name)
                ? "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response."
                : $"Hello, {name}. This HTTP triggered function executed successfully.";
            
            if (! string.IsNullOrEmpty(url)) {
                log.LogInformation("Crawling: " + url);
                var pageRequester = new PageRequester(new CrawlConfiguration(), new WebContentExtractor());
                var crawledPage = await pageRequester.MakeRequestAsync(new Uri(url));

                 // Write to blob storage using blob storage writer and applications settings
                log.LogInformation("Writing to: " + Environment.GetEnvironmentVariable("BlobSASURL") + Environment.GetEnvironmentVariable("BlobSASToken") + Environment.GetEnvironmentVariable("BlobContainerName"));    
                var blobStorageWriter = new BlobStorageWriter(Environment.GetEnvironmentVariable("BlobSASURL"), Environment.GetEnvironmentVariable("BlobSASToken")); 
                
                //try to write to blobStorageWriter.WriteStringToBlobAsync and catch any exceptions
                try {
                    await blobStorageWriter.WriteStringToBlobAsync(Environment.GetEnvironmentVariable("BlobContainerName"), url, crawledPage.Content.Text);
                }
                catch (Exception e) {
                    log.LogInformation("Error writing to blob storage: " + e.Message);
                    responseMessage = "Error writing to blob storage: " + e.Message;
                }	

            }
            
           


            return new OkObjectResult(responseMessage);
        }
    }
}
