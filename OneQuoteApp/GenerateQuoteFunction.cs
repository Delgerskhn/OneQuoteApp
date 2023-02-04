using System;
using System.Net.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.Extensions.Logging;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure.Data.Tables;
using Azure;
using OneQuoteApp.Models;
using Microsoft.Extensions.Configuration;

namespace OneQuoteApp
{

    public class GenerateQuoteFunction
    {
        private readonly IConfiguration _config;

        public GenerateQuoteFunction(IConfiguration config)
        {
            _config = config;
        }

        [FunctionName("GenerateQuoteFunction")]
        public async Task Run(
            [TimerTrigger("0 0 6 * * *")] TimerInfo myTimer,
            ILogger log,
            [Table("Quotes", Connection = "AzureWebJobsStorage")] IAsyncCollector<Quote> collector
            )
        {
            var promptCommandText = "Quote of the day separate author by a dash";

            HttpClient client = new HttpClient();
            var token = _config.GetValue<string>("OPENAI_TOKEN");
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

            string generateObject = $"{{\"model\":\"text-davinci-003\",\"prompt\":\"{promptCommandText}\",\"temperature\":0.6,\"max_tokens\":150,\"top_p\":1,\"frequency_penalty\":1,\"presence_penalty\":1}}";

            var content = new StringContent(generateObject, System.Text.Encoding.UTF8, "application/json");

            var response = await client.PostAsync($"{_config.GetValue<string>("OPENAI_HOST")}/v1/completions", content);
            var responseString = await response.Content.ReadAsStringAsync();

            var quoteResponse = Newtonsoft.Json.JsonConvert.DeserializeObject<QuoteResponse>(responseString);
            var quoteTexts = quoteResponse.Choices.First().Text.Split("-");

            var quoteText = quoteTexts[0];
            var author = quoteTexts[1];
            var date = DateTime.UtcNow.Date;
            var quote = new Quote
            {
                PartitionKey = date.ToString("yyyy-MM-dd"),
                //save time part of date as row key
                RowKey = date.ToString("HH:mm:ss"),
                QuoteText = quoteText,
                Author = author,
                CreatedAt = DateTime.Now
            };

            await collector.AddAsync(quote);

        }
    }
}
