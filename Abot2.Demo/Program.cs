using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Abot2.Core;
using Abot2.Crawler;
using Abot2.Poco;
using AngleSharp;
using AngleSharp.Html.Parser;
using Serilog;
using Serilog.Formatting.Json;
using CrucibleCrawler.Blob;
using Microsoft.Extensions.Logging;
using System.Data.SqlTypes;
using Microsoft.VisualBasic.FileIO;
using AngleSharp.Dom;


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
            //await CrucibleCrawlerTest();

            await CrucibleCrawler();


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

           var url = "https://lagimik.github.io/PartnerCrucible/AzureInfrastructureSolutionArea";
      
            var pageRequester = new PageRequester(new CrawlConfiguration(), new WebContentExtractor());
            var crawledPage = await pageRequester.MakeRequestAsync(new Uri(url));

            // Write to blob storage using blob storage writer and applications settings

            var blobStorageWriter = new BlobStorageWriter("DefaultEndpointsProtocol=https;AccountName=partnercrucibl7572430794;AccountKey=xFeg7oUK1iOpLWBSm9h7+nTqpdCFLIu3cS9XZuH1EmJv+btwkbW1hhspsVOZvbMn6v2q9oaTjft/+ASt4HsPkA==;EndpointSuffix=core.windows.net");

            //try to write to blobStorageWriter.WriteStringToBlobAsync and catch any exceptions
            try {

                // detect if the content is binary or text
                // if text, write to blob as string
                // if binary, write to blob as bytes

                if (crawledPage.Content.Bytes != null) {
                    Log.Information("Content is binary");
                    await blobStorageWriter.WriteBytesToBlobAsync("cruciblecrawl", url, crawledPage.Content.Bytes);
                } else {
                    Log.Information("Content is text");
                    await blobStorageWriter.WriteStringToBlobAsync("cruciblecrawl", url, crawledPage.Content.Text);
                }

            }
            catch (Exception e) {
                Log.Error("Error writing to blob storage: " + e.Message);

            }
        }

        private static async Task CrucibleCrawler()
        {
            var config = new CrawlConfiguration
            {
                MaxPagesToCrawl = 100,
                MinCrawlDelayPerDomainMilliSeconds = 3000
            };
            var crawler = new PoliteWebCrawler(config);

            crawler.PageCrawlCompleted += crawler_PageCrawlCompleted;
            crawler.PageCrawlDisallowed += crawler_PageCrawlDisallowed;
            crawler.PageLinksCrawlDisallowed += crawler_PageLinksCrawlDisallowed;

           var crawlResult = await crawler.CrawlAsync(new Uri("https://lagimik.github.io/PartnerCrucible/AzureInfrastructureSolutionArea"));
                       
        }

        private static void crawler_PageCrawlCompleted(object sender, PageCrawlCompletedArgs e)
        {

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .Enrich.WithThreadId()
                .WriteTo.Console(outputTemplate: Constants.LogFormatTemplate)
                .CreateLogger();

            Log.Information("CrucibleCrawl crawled: " + e.CrawledPage.Uri.AbsoluteUri);

           var blobStorageWriter = new BlobStorageWriter("");

            try {

                // detect if the content-type is text/html 
                // if so, write to blob as string
                // else, write to blob as bytes

                          
                // if the content contains %PDF- then it is a PDF file
                // if the content contains <!DOCTYPE html> then it is a HTML file
                // else it is a text file
                
                if (IsPDF(e.CrawledPage.Content.Text.Substring(0, Math.Min(e.CrawledPage.Content.Text.Length, 1024)))) {
                    blobStorageWriter.WriteBytesToBlobAsync("cruciblecrawl", 
                                                            e.CrawledPage.Uri.AbsoluteUri + ".pdf", 
                                                            e.CrawledPage.Content.Bytes);
                    
                } else if (IsHTML(e.CrawledPage.Content.Text.Substring(0, Math.Min(e.CrawledPage.Content.Text.Length, 1024)))) {
                    var bodyHTML = e.CrawledPage.AngleSharpHtmlDocument.QuerySelector("body");
                  
                    blobStorageWriter.WriteStringToBlobAsync("cruciblecrawl", 
                                                            e.CrawledPage.Uri.AbsoluteUri + ".txt", 
                                                            bodyHTML.TextContent);
                  
                } else {
                    blobStorageWriter.WriteBytesToBlobAsync("cruciblecrawl", 
                                                            e.CrawledPage.Uri.AbsoluteUri, 
                                                            e.CrawledPage.Content.Bytes);
                }

            }
            catch (Exception exception) {
                Log.Error("Error writing to blob storage: " + exception.Message);
                

            }
        }

        //write a private method to detect if a string is PDF file
        private static bool IsPDF(string content) {
            if (content.Contains("%PDF-")) {
                return true;
            } else {
                return false;
            }
        }

        private static bool IsHTML(string content) {
            if (content.Contains("<!DOCTYPE html>")) {
                return true;
            } else {
                return false;
            }
        }

        private static void crawler_PageLinksCrawlDisallowed(object sender, PageLinksCrawlDisallowedArgs e)
            {
                CrawledPage crawledPage = e.CrawledPage;
                Console.WriteLine($"Did not crawl the links on page {crawledPage.Uri.AbsoluteUri} due to {e.DisallowedReason}");
            }

        private static void crawler_PageCrawlDisallowed(object sender, PageCrawlDisallowedArgs e)
            {
                PageToCrawl pageToCrawl = e.PageToCrawl;
                Console.WriteLine($"Did not crawl page {pageToCrawl.Uri.AbsoluteUri} due to {e.DisallowedReason}");
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
