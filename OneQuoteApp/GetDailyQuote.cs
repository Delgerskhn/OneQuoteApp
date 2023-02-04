using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Azure.Data.Tables;
using OneQuoteApp.Models;
using System.Linq;

namespace OneQuoteApp
{
    public static class GetDailyQuote
    {
        [FunctionName("GetDailyQuote")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            [Table("Quotes", Connection = "AzureWebJobsStorage")] TableClient quoteClient,
            ILogger log)
        {

            var quote = quoteClient.Query<Quote>(r => r.PartitionKey == DateTime.UtcNow.Date.ToString("yyyy-MM-dd")).First();


            return new OkObjectResult(quote);
        }
    }
}
