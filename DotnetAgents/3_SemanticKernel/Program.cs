using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

var endpoint = Environment.GetEnvironmentVariable("AZUREAI_ENDPOINT");
var deploymentName = Environment.GetEnvironmentVariable("AZUREAI_ENDPOINT");
var apiKey = Environment.GetEnvironmentVariable("AZUREAI_APIKEY");


var kernel = Kernel.CreateBuilder()
            .AddAzureOpenAIChatCompletion(
                deploymentName,
                endpoint,
                apiKey)
            .Build();

var executionSettings = new OpenAIPromptExecutionSettings()
{
    Temperature = 0,
    ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions,
    ChatSystemPrompt = "",
};

var chat = kernel.GetRequiredService<IChatCompletionService>();

var chatHistory = new ChatHistory();

chatHistory.AddUserMessage($@"Give me the most important information about Prague.");

var response = await chat.GetChatMessageContentAsync(chatHistory, executionSettings, kernel);

var content = response.Content?.ToString() ?? string.Empty;

Console.WriteLine(content);



