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



namespace Abot.Function
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

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            name = name ?? data?.name;

            var config = new CrawlConfiguration
                {
                    MaxPagesToCrawl = 25,
                    MinCrawlDelayPerDomainMilliSeconds = 3000
                };
            var crawler = new PoliteWebCrawler(config);

            var crawlResult = await crawler.CrawlAsync(new Uri("https://lagimik.github.io/PartnerCrucible/PracticeBuilding"));


            string responseMessage = string.IsNullOrEmpty(name)
                ? "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response."
                : $"Hello, {name}. This HTTP triggered function executed successfully.";
            
            responseMessage += crawlResult;


            return new OkObjectResult(responseMessage);
        }
    }
}
