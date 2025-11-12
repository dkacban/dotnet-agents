using System.Net.Http;
using System.Threading;
using Microsoft.SemanticKernel;
using ModelContextProtocol.Client;

static async Task Main(string[] args)
{
    var transport = new HttpClientTransport(
        new HttpClientTransportOptions
        {
            Endpoint = new Uri("https://localhost:7089"),
            TransportMode = HttpTransportMode.StreamableHttp
        }
    );
    var client = await McpClient.CreateAsync(
    transport,
    new McpClientOptions
    {
        ClientInfo = new() { Name = "MyAgent", Version = "1.0.0" }
    });
    var tools = await client.ListToolsAsync();
    var functions = tools.Select(t => t.AsKernelFunction()).ToList();
    var plugin = KernelPluginFactory.CreateFromFunctions("MyMcp", functions);
}

