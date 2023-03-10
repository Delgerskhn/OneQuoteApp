# OneQuoteApp

OneQuoteApp is a cloud-based solution that generates and sends daily quotes to its subscribers. The solution is built using Microsoft Azure Functions, Azure Table Storage, and OpenAI's GPT-3 API.

## Classes

### CreateUserTrigger

This class is responsible for handling the creation of a new user in Azure Table Storage. It has a single method Run that triggers on a HTTP request, which can be a GET or POST request. The method logs the request and retrieves the email of the user from either the query string or the request body in JSON format. It creates a new User entity with the email as both the partition and row key, as well as the registered and last interacted dates set to the current UTC time. The method returns a 201 (Created) response with the created user entity as the response body.

### GenerateQuoteFunction

This class generates a new quote every day at 6 am UTC. It has a single method Run that triggers on a timer, set to fire every day at 6 am UTC. The method uses the OpenAI API to generate a quote and splits it into quote text and author using a dash as a separator. It creates a new Quote entity with the date as the partition key and time as the row key, as well as the quote text, author, and creation date. The new quote entity is added to Azure Table Storage.

### SendQuoteMailFunction

This class sends a daily quote to subscribers via email. It has a single method Run that triggers on a queue trigger, which listens to a queue for pending email requests. The method retrieves the email addresses from the email request payload and sends the daily quote to each recipient.

## Models

### User

This class represents a user of the OneQuoteApp. It has properties for the partition key (always set to "User"), row key (email), email, registered date, and last interacted date.

### Quote

This class represents a quote generated by the OneQuoteApp. It has properties for the partition key (date in the format "yyyy-MM-dd"), row key (time in the format "HH:mm:ss"), quote text, author, and creation date.

### QuoteResponse

This class represents the response from the OpenAI API, which includes a list of generated quotes as choices.
