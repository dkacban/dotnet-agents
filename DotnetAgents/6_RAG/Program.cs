using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Models;

var azureAISearchUrl = "";
var azureAISearchKey = "";  
var azureAISearchIndex = "";

var indexClient = new SearchIndexClient(new Uri(azureAISearchUrl), new AzureKeyCredential(azureAISearchKey));
var searchClient = indexClient.GetSearchClient(azureAISearchIndex);

var searchOptions = new SearchOptions
{
    Size = 3,
    Select = { "chunk", "title" },
    IncludeTotalCount = true,
    QueryType = SearchQueryType.Semantic,
};

List<SearchResultWithTitle> searchResults = new List<SearchResultWithTitle>();


string message = "scrum authors";

SearchResults<SearchDocument> response = await searchClient.SearchAsync<SearchDocument>(message, searchOptions);
await foreach (SearchResult<SearchDocument> r in response.GetResultsAsync())
{
    var content = r.Document["chunk"];
    var title = r.Document["title"];
    var score = r.Score;

    searchResults.Add(new SearchResultWithTitle(title.ToString(), content.ToString()));
}

foreach(var result in searchResults)
{
    Console.WriteLine($"{result.Content}[Source document: {result.Title}]");
}


public record SearchResultWithTitle(string Title, string Content);
