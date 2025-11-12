using Azure;
using Azure.AI.Language.QuestionAnswering;


// QNA example:    
var question = "Oldest part of Prague";

var endpointUrl = "";
var key = "";
var projectName = "";
var deploymentName = "";

Uri endpoint = new Uri(endpointUrl);
AzureKeyCredential credential = new AzureKeyCredential(key);

var client = new QuestionAnsweringClient(endpoint, credential);
var project = new QuestionAnsweringProject(projectName, deploymentName);
var answersOptions = new AnswersOptions
{
    ConfidenceThreshold = 0.5,
    RankerKind = RankerKind.QuestionOnly,
    Size = 60
};

Response<AnswersResult> response = client.GetAnswers(question, project, answersOptions);
var topAnswer = response.Value.Answers.OrderByDescending(x => x.Confidence).First().Answer;



//using Azure.AI.Language.QuestionAnswering;
//using Azure;
//using Microsoft.SemanticKernel;
//using System.ComponentModel;

//namespace Nsure.ConversationalVirtualAssistant.Plugins.Native.FAQ;

//[Description("Plugin to retrieve information from QnA knowledge base. Should be used as a first try to answer every user question.")]
//public class FAQPlugin
//{
//    private readonly ILogger<FAQPlugin> _logger;
//    QuestionAnsweringClient _client;
//    QuestionAnsweringProject _project;
//    AnswersOptions _answersOptions;

//    public FAQPlugin(FaqPluginConfig config, ILogger<FAQPlugin> logger)
//    {
//        _logger = logger;

//        Uri endpoint = new Uri(config.Endpoint);
//        AzureKeyCredential credential = new AzureKeyCredential(config.Key);
//        string projectName = config.ProjectName;
//        string deploymentName = config.DeploymentName;

//        _client = new QuestionAnsweringClient(endpoint, credential);
//        _project = new QuestionAnsweringProject(projectName, deploymentName);
//        _answersOptions = new AnswersOptions
//        {
//            ConfidenceThreshold = 0.44,
//            RankerKind = RankerKind.QuestionOnly,
//            Size = 60,
//        };
//    }

//    [KernelFunction, Description("Retrieves top answer to a given question from QnA knowledge base.")]
//    public string GetDataFromQnA([Description("the question to QnA")] string question, List<string> carriers)
//    {
//        _answersOptions.Filters.SourceFilter.Clear();
//        foreach (var carrier in carriers)
//        {
//            _answersOptions.Filters.SourceFilter.Add(carrier);
//        }

//        Response<AnswersResult> response = _client.GetAnswers(question, _project, _answersOptions);
//        var topAnswer = response.Value.Answers.OrderByDescending(x => x.Confidence).First().Answer;

//        return topAnswer;
//    }
//}
