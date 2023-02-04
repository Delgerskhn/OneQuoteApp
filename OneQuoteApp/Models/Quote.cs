
using System;
using Azure;
using Azure.Data.Tables;

namespace OneQuoteApp.Models;

public class Quote : ITableEntity
{
    public string PartitionKey { get; set; }
    public string RowKey { get; set; }
    public string QuoteText { get; set; }
    public string Author { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
}