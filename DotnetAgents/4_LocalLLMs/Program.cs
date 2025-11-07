using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

var endpoint = Environment.GetEnvironmentVariable("AZUREAI_ENDPOINT");
var deploymentName = Environment.GetEnvironmentVariable("AZUREAI_ENDPOINT");
var apiKey = Environment.GetEnvironmentVariable("AZUREAI_APIKEY");


byte[] bytes = File.ReadAllBytes("images/soldier1.jpg");
string base64 = Convert.ToBase64String(bytes);
string imageUri = $"data:image/jpg;base64,{base64}";

var chatHistory = new ChatHistory("You answer how many people are in the picture - just use number.");
chatHistory.AddUserMessage(
[
    new TextContent("How many people are in the picture?"),
    new ImageContent(imageUri),
]);

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
var response = await chat.GetChatMessageContentAsync(chatHistory, executionSettings, kernel);
var content = response.Content?.ToString() ?? string.Empty;

Console.WriteLine(content);



