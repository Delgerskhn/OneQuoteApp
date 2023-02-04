using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.WebJobs.Extensions;
using Newtonsoft.Json;
using Azure.Data.Tables;
using Azure;
using OneQuoteApp.Models;

namespace OneQuoteApp
{

    public class CreateUserTrigger
    {
        [FunctionName("CreateUserTrigger")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            [Table("Users", Connection = "AzureWebJobsStorage")] IAsyncCollector<User> collector,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string name = req.Query["email"];

            string requestBody = new StreamReader(req.Body).ReadToEndAsync().Result;
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            name = name ?? data?.email;

            var entity = new User
            {
                PartitionKey = "User",
                RowKey = name,
                Email = name,
                RegisteredAt = DateTime.UtcNow,
                LastInteractedAt = DateTime.UtcNow
            };
            await collector.AddAsync(entity);

            string responseMessage = $"User entity with email '{name}' was created in Azure Table Storage.";

            return new CreatedResult("", entity);
        }
    }
}
