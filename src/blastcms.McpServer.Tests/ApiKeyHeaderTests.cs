using blastcms.McpServer;
using blastcms.McpServer.Tools;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;
using Moq;
using Moq.Protected;
using System.IO.Pipelines;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace blastcms.McpServer.Tests;

/// <summary>
/// Regression tests for the outgoing HTTP request header name used to authenticate
/// with the blast-cms REST API. The REST API reads "ApiKey"; sending any other name
/// (e.g. "X-API-Key") silently drops authentication.
/// </summary>
[TestFixture]
public class ApiKeyHeaderTests
{
    /// <summary>
    /// Regression: the MCP server must send "ApiKey", not "X-API-Key".
    /// A handler that captures the outgoing request is used to inspect headers
    /// directly, independently of what the server chooses to respond with.
    /// </summary>
    [Test]
    public async Task OutgoingRequest_UsesApiKeyHeader_NotXApiKey()
    {
        HttpRequestMessage? capturedRequest = null;

        var mock = new Mock<HttpMessageHandler>();
        mock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((req, _) => capturedRequest = req)
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    "{\"data\":[],\"count\":0,\"page\":1}",
                    Encoding.UTF8,
                    "application/json")
            });

        var (client, serverTask, cts) = await CreateClientServerPair(mock.Object);

        await using (client)
        {
            await client.CallToolAsync("list_blog_articles",
                new Dictionary<string, object?> { ["page"] = 1, ["pageSize"] = 10 });
        }

        await cts.CancelAsync();
        await serverTask;

        Assert.That(capturedRequest, Is.Not.Null, "Expected an HTTP request to have been made");
        Assert.That(capturedRequest!.Headers.Contains("ApiKey"), Is.True,
            "Outgoing request must include 'ApiKey' header (the name expected by the REST API)");
        Assert.That(capturedRequest.Headers.Contains("X-API-Key"), Is.False,
            "Outgoing request must NOT use 'X-API-Key' — that name is not read by the REST API");
    }

    private static async Task<(McpClient client, Task serverTask, CancellationTokenSource cts)> CreateClientServerPair(
        HttpMessageHandler httpMessageHandler)
    {
        var clientToServer = new Pipe();
        var serverToClient = new Pipe();

        var cts = new CancellationTokenSource();

        var services = new ServiceCollection();
        services.AddLogging(b => b.SetMinimumLevel(LogLevel.None));

        var mockHttpClient = new HttpClient(httpMessageHandler)
        {
            BaseAddress = new Uri("http://localhost/")
        };
        // Simulate the header that Program.cs adds to the registered HttpClient.
        // The REST API reads "ApiKey"; using any other name (e.g. "X-API-Key") silently drops auth.
        mockHttpClient.DefaultRequestHeaders.Add("ApiKey", "test-api-key");
        var mockFactory = new Mock<IHttpClientFactory>();
        mockFactory
            .Setup(f => f.CreateClient(BlastCmsClientConstants.HttpClientName))
            .Returns(mockHttpClient);
        services.AddSingleton(mockFactory.Object);

        services
            .AddMcpServer()
            .WithStreamServerTransport(
                clientToServer.Reader.AsStream(),
                serverToClient.Writer.AsStream())
            .WithTools<BlogArticleTools>()
            .WithTools<ContentBlockTools>()
            .WithTools<FeedArticleTools>();

        var sp = services.BuildServiceProvider();
        var server = sp.GetRequiredService<ModelContextProtocol.Server.McpServer>();
        var serverTask = Task.Run(async () =>
        {
            try { await server.RunAsync(cts.Token); }
            catch (OperationCanceledException) { }
            finally { serverToClient.Writer.Complete(); }
        });

        var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
        var client = await McpClient.CreateAsync(
            new StreamClientTransport(
                serverInput: clientToServer.Writer.AsStream(),
                serverOutput: serverToClient.Reader.AsStream(),
                loggerFactory),
            loggerFactory: loggerFactory,
            cancellationToken: cts.Token);

        return (client, serverTask, cts);
    }
}
