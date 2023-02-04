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

namespace OneQuoteApp
{
    public static class UnsubscribeTrigger
    {
        [FunctionName("UnsubscribeTrigger")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            [Table("Users", Connection = "AzureWebJobsStorage")] TableClient userClient,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string name = req.Query["email"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            name = name ?? data?.email;

            userClient.DeleteEntity("User", name);

            string responseMessage = string.IsNullOrEmpty(name)
                ? "<html><body><h1>Welcome to One Quote App</h1><p>Your email address is not provided in the request, please try again with your email address.</p></body></html>"
                : $"<html><body><h1>Welcome to One Quote App</h1><p>Hello, {name}! You have been successfully unsubscribed from One Quote App's daily quotes email service.</p></body></html>";

            return new ContentResult
            {
                Content = responseMessage,
                ContentType = "text/html",
                StatusCode = 200
            };
        }
    }
}
