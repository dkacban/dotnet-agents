using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using OpenAI.Chat;
using System.Text.Json;
using System.Text.Json.Serialization;

public class DocumentPage
{
    [JsonPropertyName("Paragraphs")]
    public List<Paragraph> Paragraphs { get; set; } = new();
}

public class Paragraph
{
    [JsonPropertyName("Text")]
    public string Text { get; set; }

    [JsonPropertyName("ClassificationClass")]
    public string ClassificationClass { get; set; }
}


public class VisionService
{
    public VisionService()
    {
    }

    static string classificationPrompt =
@$"You are given a text of scientific paper.
The text is an array of paragraphs of text that are in order as in the original document.
Task for you:
- classify each paragraph of the original text based on the rules specified below and the entire document context.
The result must contain data for each and paragraph of the input text with unchanged order. 

RULES:
1. The list of available classes that you should use to classify each element:
- title 
- header
- paragraph
- toExclude

2. Descriptions of specific classes:
- title - this is a title of entire document - appears only once in the document at the beginning.
- header - this is a header of the following paragraph - appears multiple times in the document.
- paragraph - this is a typical block of text - 1 or more sentence or sentence equivalent(s). It has similar font size like all other paragraphs. And visualy it looks like paragraph. May include index terms or descriptions of figures, tables, etc.
- figureDescription - figure or graph foot note - the text that is present after the image and describes the figure or image. Typically starts with prefix 'Fig.'
- tableDescription - table description - the text that describes the table and is located above or below the table. Typically starts with prefix 'Table'. If the text doesn't start with table indication prefix and is formated as standard paragraph, then class paragraph class should be used.
- mathsFormula - mathematical formula or equation - the text that is a mathematical formula or equation and doesn't contain any verbs and nouns or sentence equivalent. It is typically formatted as a single line of text with special characters like '=', '+', '-', etc. If the text is not a formula, then class paragraph should be used. Example: (c) vmp Y \u2212\u2192 oB, if \u2203acrVMS (vmp, B) . action = pass, vmp \u2208 (TID \u2227 TSNeti \u2227 VMSi), B \u2209 VMSs, acrp \u2208 ACRi.
- toExclude - everyting that does not belong to any of the above categories. Specific description of this class below.

3. ToExclude class signify every line of text that is not part of main document content: 
- author names
- page number
- references to other papers
- logo
- tables
- diagrams
- publishing information like date, journal name, etc.
- article info, keywords etc.
- data that is visually part of the table - normally it is a line that contains one word or sentence.
- data that is visually part of the diagram - normally it is a line that contains one word or sentence.
- footer of the page
";

    public async static Task<DocumentPage> ExtractDataFromImage(string path)
    {
        var model = "";
        var azureEndpoint = "";
        var apiKey = "";

        var kernel = Kernel.CreateBuilder()
            .AddAzureOpenAIChatCompletion(model, azureEndpoint, apiKey)
            .Build();

        var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

        int maxRetries = 3;
        int retryDelayMs = 20_000;

        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                byte[] bytes = File.ReadAllBytes(path);
                string base64 = Convert.ToBase64String(bytes);
                string imageUri = $"data:image/png;base64,{base64}";

                var chatHistory = new ChatHistory("You can extract text from image of document.");
                chatHistory.AddUserMessage(
                [
                    new TextContent(classificationPrompt),
                    new ImageContent(imageUri),
                ]);

                var format = ChatResponseFormat.CreateJsonSchemaFormat(
                    jsonSchemaFormatName: "markdown_result",
                    jsonSchema: BinaryData.FromString("""
                        {
                        "type": "object",
                        "properties": {
                            "Paragraphs": {
                                "type": "array",
                                "items": {
                                    "type": "object",
                                    "properties": {
                                        "Text": { "type": "string" },
                                        "ClassificationClass": { "type": "string" }
                                    },
                                    "required": ["Text", "ClassificationClass"],
                                    "additionalProperties": false
                                }
                            }
                        },
                        "required": ["Paragraphs"],
                        "additionalProperties": false
                    }
                    """),
                            jsonSchemaIsStrict: true);

#pragma warning disable SKEXP0010
                var executionSettings = new OpenAIPromptExecutionSettings()
                {
                    ResponseFormat = format,
                    Temperature = 0
                };
#pragma warning restore SKEXP0010
                var chatResponse = await chatCompletionService.GetChatMessageContentAsync(chatHistory, executionSettings);

                var tokens = GetTokens(chatResponse);

                var jsonResponse = chatResponse.Content;


                DocumentPage page = JsonSerializer.Deserialize<DocumentPage>(jsonResponse);

                return page;
            }
            catch (Exception e)
            {
                if (attempt == maxRetries)
                    throw;
                await Task.Delay(retryDelayMs);
            }
        }

        return null;
    }

    private static TokensUsage GetTokens(Microsoft.SemanticKernel.ChatMessageContent chatResponse)
    {
        if (chatResponse.Metadata != null && chatResponse.Metadata.TryGetValue("usage", out var usageObj))
        {
            var usage = usageObj as JsonElement?;
            if (usage != null && usage.Value.ValueKind == JsonValueKind.Object)
            {
                int promptTokens = usage.Value.GetProperty("prompt_tokens").GetInt32();
                int completionTokens = usage.Value.GetProperty("completion_tokens").GetInt32();
                int totalTokens = usage.Value.GetProperty("total_tokens").GetInt32();

                return new TokensUsage()
                {
                    PromptTokens = promptTokens,
                    CompletionTokens = completionTokens,
                    TotalTokens = totalTokens
                };
            }
        }

        return null;
    }


}


public class TokensUsage
{
    public int PromptTokens { get; set; }
    public int CompletionTokens { get; set; }
    public int TotalTokens { get; set; }
}

class Program
{
    static async Task Main()
    {
        var page = await VisionService.ExtractDataFromImage("images/image.png");
        Console.WriteLine(page.Paragraphs.Count);

        Console.WriteLine("PARAGRAPHS:");
        foreach(var paragraph in page.Paragraphs)
        {
            Console.WriteLine(paragraph.Text);
        }
    }
}