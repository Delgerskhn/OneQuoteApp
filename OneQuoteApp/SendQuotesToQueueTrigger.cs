using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure.Data.Tables;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Queue;
using OneQuoteApp.Models;

namespace OneQuoteApp
{
    public class SendQuotesToQueueTrigger
    {
        [FunctionName("SendQuotesToQueueTrigger")]
        public async Task Run(
            [TimerTrigger("0 0 7 * * *")] TimerInfo myTimer,
            [Queue("pending-mail-request-queue"), StorageAccount("AzureWebJobsStorage")] ICollector<string> msg,
            [Table("Users", Connection = "AzureWebJobsStorage")] TableClient userClient,
            [Table("Quotes", Connection = "AzureWebJobsStorage")] TableClient quoteClient,
            ILogger log
            )
        {
            var users = userClient.QueryAsync<User>(r => r.LastInteractedAt > DateTime.UtcNow.Date.AddDays(-1), 10);

            //foreach user in users
            await foreach (var user in users.AsPages())
            {
                foreach (var u in user.Values)
                {
                    var quote = quoteClient.Query<Quote>(r => r.PartitionKey == DateTime.UtcNow.Date.ToString("yyyy-MM-dd")).First();

                    var q = quote;
                    //send emailAddress and quote to queue
                    var message = u.Email + "<SPLITTER/>" + q.QuoteText + "<SPLITTER/>" + q.Author;
                    msg.Add(message);
                    log.LogInformation($"Sent to queue at: {DateTime.Now}. \n Email: {u.Email}");
                }
            }


            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
        }
    }
}
