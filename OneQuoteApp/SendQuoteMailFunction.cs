using System;
using System.Net.Mail;
using System.Net;
using System.Xml.Linq;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Azure.Data.Tables;
using OneQuoteApp.Models;
using Azure;

namespace OneQuoteApp
{
    public class SendQuoteMailFunction
    {


        private readonly IConfiguration _config;

        public SendQuoteMailFunction(IConfiguration config)
        {
            _config = config;
        }

        [FunctionName("SendQuoteMailFunction")]
        public void Run(
          [QueueTrigger("pending-mail-request-queue", Connection = "AzureWebJobsStorage")] string myQueueItem,
          [Table("Users", Connection = "AzureWebJobsStorage")] TableClient userClient,
          ILogger log)
        {
            //get email and quote from queue
            string[] queueItem = myQueueItem.Split("<SPLITTER/>");
            string emailAddress = queueItem[0];
            string quote = queueItem[1];
            string author = queueItem[2];

            var user = userClient.GetEntity<User>("User", emailAddress);
            if (user == null)
            {
                log.LogInformation($"User not found: {emailAddress}");
                return;
            }
            //update interacted at
            user.Value.LastInteractedAt = DateTime.UtcNow;

            userClient.UpdateEntity(user.Value, ETag.All, TableUpdateMode.Merge);


            //send email
            string appName = _config.GetValue<string>("APP_NAME");
            var fromAddress = new MailAddress(_config.GetValue<string>("EMAIL"), appName);
            var toAddress = new MailAddress(emailAddress, emailAddress);
            string fromPassword = _config.GetValue<string>("EMAIL_PASSWORD");
            //https://stackoverflow.com/questions/32260/sending-email-in-net-through-gmail
            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
            };
            using (var message = new MailMessage(fromAddress, toAddress)
            {
                Subject = appName,
                Body = EmailTemplate.Replace("{{quote}}", quote).Replace("{{author}}", author).Replace("{{unsubscribe}}", $"{_config.GetValue<string>("APP_HOST")}/api/UnsubscribeTrigger?email={emailAddress}"),
                IsBodyHtml = true
            })
            {
                smtp.Send(message);
            }

            log.LogInformation($"C# Queue trigger function processed: {myQueueItem}");
        }


        private string EmailTemplate = @"
          <head>
            <meta charset=""UTF-8"" />
            <title>HTML CSS blockquote</title>
            <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"" />
            <meta https-equiv=""X-UA-Compatible"" content=""ie=edge"" />
            <link rel=""stylesheet"" href=""style.css"" />
            <link href=""https://fonts.googleapis.com/css2?family=IBM+Plex+Sans&display=swap"" rel=""stylesheet"">
          </head>
          <style>
          blockquote {
            margin: 0;
          }
          blockquote p {
              padding: 20px;
              font-size: 30px;
              background: #eee;
              border-radius: 5px;
          }
          blockquote p::before {
              content: '\201C';
          }
          blockquote p::after {
              content: '\201D';
          }
          .footer {
            display: flex;
            justify-content: space-between;
          }
          </style>
          <body>
            <figure>
            <blockquote cite=""https://www.w3schools.com/html/html_intro.asp"">
                <p>{{quote}}</p>
            </blockquote>
            <div class=""footer"">
            <figcaption>Author, <cite>{{author}}</cite></figcaption>
              <a href=""{{unsubscribe}}"">Click here to unsubscribe</a>
            </div>
        </figure>
          </body>";
    }
}
