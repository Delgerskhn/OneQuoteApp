using System;
using System.Collections.Generic;
using Azure;
using Azure.Data.Tables;

namespace OneQuoteApp.Models;

public class User : ITableEntity
{
    //user
    public string PartitionKey { get; set; }
    //email of user
    public string RowKey { get; set; }
    public string Email { get; set; }
    public DateTime RegisteredAt { get; set; }
    public DateTime LastInteractedAt { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
}

