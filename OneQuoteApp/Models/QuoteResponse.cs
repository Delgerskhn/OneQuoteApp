using System.Collections.Generic;
namespace OneQuoteApp.Models;


public class QuoteResponse
{
    public string Id { get; set; }
    public List<Choice> Choices { get; set; }

}

public class Choice
{
    public string Text { get; set; }
}