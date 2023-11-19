using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Abot2.Core;
using Abot2.Crawler;
using Abot2.Poco;
using AngleSharp;
using Serilog;
using Serilog.Formatting.Json;
using CrucibleCrawler.Blob;
using Microsoft.Extensions.Logging;


namespace Abot2.Demo
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .Enrich.WithThreadId()
                .WriteTo.Console(outputTemplate: Constants.LogFormatTemplate)
                .CreateLogger();

            Log.Information("Demo starting up!");

            //await DemoPageRequester();
            //await DemoSimpleCrawler();

            await CrucibleCrawlerTest();


            Log.Information("Demo done!");

        }

        private static async Task CrucibleCrawlerTest()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .Enrich.WithThreadId()
                .WriteTo.Console(outputTemplate: Constants.LogFormatTemplate)
                .CreateLogger();

            Log.Information("CrucibleCrawl Starting!");

           //var url = "https://lagimik.github.io/PartnerCrucible/PracticeBuilding";
            var url = "https://tools.totaleconomicimpact.com/go/microsoft/AzureServicesPartner//docs/MicrosoftAzure_ServicesPartners_CaseStudy.pdf";

            var pageRequester = new PageRequester(new CrawlConfiguration(), new WebContentExtractor());
            var crawledPage = await pageRequester.MakeRequestAsync(new Uri(url));

            // Write to blob storage using blob storage writer and applications settings

            var blobStorageWriter = new BlobStorageWriter("");

            //try to write to blobStorageWriter.WriteStringToBlobAsync and catch any exceptions
            try {
               //await blobStorageWriter.WriteStringToBlobAsync("cruciblecrawl", url, crawledPage.Content.Text);
               await blobStorageWriter.WriteBytesToBlobAsync("cruciblecrawl", url, crawledPage.Content.Bytes);


            }
            catch (Exception e) {
                Log.Error("Error writing to blob storage: " + e.Message);

            }
        }

        private static async Task DemoSimpleCrawler()
        {
            var config = new CrawlConfiguration
            {
                MaxPagesToCrawl = 25,
                MinCrawlDelayPerDomainMilliSeconds = 3000
            };
            var crawler = new PoliteWebCrawler(config);

            crawler.PageCrawlCompleted += Crawler_PageCrawlCompleted;

            var crawlResult = await crawler.CrawlAsync(new Uri("http://wvtesting2.com"));
        }

        private static void Crawler_PageCrawlCompleted(object sender, PageCrawlCompletedArgs e)
        {

        }

        private static async Task DemoPageRequester()
        {
            var pageRequester =
                new PageRequester(new CrawlConfiguration(), new WebContentExtractor());

            //var result = await pageRequester.MakeRequestAsync(new Uri("http://google.com"));
            var result = await pageRequester.MakeRequestAsync(new Uri("http://wvtesting2.com"));
            Log.Information("{result}", new { url = result.Uri, status = Convert.ToInt32(result.HttpResponseMessage.StatusCode) });

        }
    }
}
