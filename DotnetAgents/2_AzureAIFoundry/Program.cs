

var endpoint = Environment.GetEnvironmentVariable("DI_ENDPOINT");
var key = Environment.GetEnvironmentVariable("DI_KEY");
var model = "prebuilt-layout";
var di = new DocumentIntelligenceService(endpoint, key, model);

string filePath = "2020-Scrum-Guide-US.pdf";

using (FileStream pdfStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
{
    using (var memoryStream = new MemoryStream())
    {
        pdfStream.CopyTo(memoryStream);
        memoryStream.Position = 0;

        var result = await di.Extract(memoryStream);
    }
}

