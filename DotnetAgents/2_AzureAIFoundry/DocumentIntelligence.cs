using Azure;
using Azure.AI.DocumentIntelligence;
using Azure.Core;
using System.Diagnostics;

public class DocumentIntelligenceService
{
    DocumentIntelligenceClient _client;
    string _model;
    private const string pageBreak = "<!-- PageBreak -->";

    public DocumentIntelligenceService(string endpoint, string key, string model)
    {
        var clientOptions = new DocumentIntelligenceClientOptions
        {
            Retry =
            {
                Mode = RetryMode.Exponential,
                MaxRetries = 3,
                Delay = TimeSpan.FromSeconds(10),
                MaxDelay = TimeSpan.FromSeconds(30),
                NetworkTimeout = TimeSpan.FromSeconds(60)
            }
        };

        var credential = new AzureKeyCredential(key);
        _model = model;
        _client = new DocumentIntelligenceClient(new Uri(endpoint), credential, clientOptions);
    }

    public async Task<DocumentData> Extract(Stream stream)
    {
        if (stream == null)
        {
            throw new ArgumentNullException(nameof(stream), "Stream can't be null.");
        }

        var extractedData = new DocumentData();
        extractedData.Pages = new List<PageOfDocument>();

        BinaryData binaryData = BinaryData.FromStream(stream);
        var options = new AnalyzeDocumentOptions(_model, binaryData)
        {
            OutputContentFormat = DocumentContentFormat.Markdown,
        };

        options.Output.Add(AnalyzeOutputOption.Figures);
        //options.Features.Add(DocumentAnalysisFeature.Formulas);
        //options.Features.Add(DocumentAnalysisFeature.KeyValuePairs);
        //options.Features.Add(DocumentAnalysisFeature.QueryFields);
        //options.QueryFields.Add("PaperTitle");
        //options.QueryFields.Add("Publisher");


        var timer = Stopwatch.StartNew();

        try
        {
            Operation<AnalyzeResult> operation = await _client.AnalyzeDocumentAsync(WaitUntil.Completed, options);
            AnalyzeResult result = operation.Value;

            var extractedPages = result.Pages.Count();

            timer.Stop();
            var duration = Math.Round(timer.Elapsed.TotalSeconds, 1);

            GetTables(result);
            GetKeyValuePairs(operation, result);
            await GetFigures(operation, result);

            var documentMarkdown = result.Content;

            var markdownPages = documentMarkdown.Split(new[] { pageBreak }, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < markdownPages.Count(); i++)
            {
                var documentPage = new PageOfDocument()
                {
                    // NOTE: start numbering from 1
                    PageNumber = i + 1,
                    Markdown = markdownPages[i],
                };

                extractedData.Pages.Add(documentPage);
            }
        }
        catch (Exception e)
        {
            timer.Stop();
            var duration = Math.Round(timer.Elapsed.TotalSeconds, 1);

            throw;
        }

        return extractedData;
    }

    private static void GetTables(AnalyzeResult result)
    {
        foreach (var table in result.Tables)
        {
            var rows = table.RowCount;
            var columns = table.ColumnCount;
            var cells = table.Cells;

            foreach (var cell in cells)
            {
            }
        }
    }

    private async Task GetKeyValuePairs(Operation<AnalyzeResult> operation, AnalyzeResult result)
    {
        foreach (var kvp in result.KeyValuePairs)
        {
            var key = kvp.Key;
            var value = kvp.Value;
            var confidence = kvp.Confidence;
        }
    }
    private async Task GetFigures(Operation<AnalyzeResult> operation, AnalyzeResult result)
    {
        var timer = Stopwatch.StartNew();

        foreach (var figure in result.Figures)
        {
            try
            {
                var figureData = await _client.GetAnalyzeResultFigureAsync(
                    result.ModelId,
                    operation.Id,
                    figure.Id
                );

                var figureBytes = figureData.Value.ToArray();

                string fileName = $"{figure.Id}.png";

                await File.WriteAllBytesAsync(fileName, figureBytes);

                timer.Stop();
                var duration = Math.Round(timer.Elapsed.TotalSeconds, 1);
            }
            catch (Exception e)
            {
                timer.Stop();
                var duration = Math.Round(timer.Elapsed.TotalSeconds, 1);

                throw;
            }
        }
    }
}

public class PageOfDocument
{
    public int PageNumber { get; set; }
    public string Markdown { get; set; }
}

public class DocumentData
{
    public List<PageOfDocument> Pages;
    public DocumentData()
    {
    }
}