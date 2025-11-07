using OpenAI.Chat;
using Azure;
using Azure.AI.OpenAI;


var endpoint = Environment.GetEnvironmentVariable("AZUREAI_ENDPOINT");
var deploymentName = Environment.GetEnvironmentVariable("AZUREAI_ENDPOINT");
var apiKey = Environment.GetEnvironmentVariable("AZUREAI_APIKEY");

AzureOpenAIClient azureClient = new(
    new Uri(endpoint),
    new AzureKeyCredential(apiKey));
ChatClient chatClient = azureClient.GetChatClient(deploymentName);

var requestOptions = new ChatCompletionOptions()
{
    //MaxOutputTokenCount = 100,
    Temperature = 1.0f,
    TopP = 1.0f,
    FrequencyPenalty = 0.0f,
    PresencePenalty = 0.0f,

};

List<ChatMessage> messages = new List<ChatMessage>()
{
    new SystemChatMessage("You are a helpful assistant."),
    new UserChatMessage("I am going to Prague, what should I see? Limit your answer to 100 tokens."),
};

var response = chatClient.CompleteChat(messages, requestOptions);

System.Console.WriteLine(response.Value.Content[0].Text);