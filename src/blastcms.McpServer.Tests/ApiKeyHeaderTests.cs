using blastcms.McpServer;
using blastcms.McpServer.Tools;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;
using Moq;
using Moq.Protected;
using System.IO.Pipelines;
using System.Linq;
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
    /// Regression: Program.cs configures a BearerPassthroughHandler, so an inbound
    /// Authorization: Bearer header must become the downstream ApiKey header that
    /// the Blast CMS REST API expects.
    /// </summary>
    [Test]
    public async Task BearerPassthroughHandler_ForwardsBearerToken_AsApiKeyHeader()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers.Authorization = "Bearer generated-api-key";

        var innerHandler = new CapturingHandler();
        var handler = new BearerPassthroughHandler(new HttpContextAccessor
        {
            HttpContext = httpContext
        })
        {
            InnerHandler = innerHandler
        };

        using var client = new HttpClient(handler);
        await client.GetAsync("http://localhost/finaltestblog/api/blogarticle/all?skip=0&take=10&currentPage=0");

        Assert.That(innerHandler.CapturedRequest, Is.Not.Null, "Expected an HTTP request to have been captured");
        Assert.That(innerHandler.CapturedRequest!.Headers.Contains("ApiKey"), Is.True,
            "Bearer token from the inbound MCP request must be forwarded as the 'ApiKey' header");
        Assert.That(innerHandler.CapturedRequest.Headers.GetValues("ApiKey").Single(), Is.EqualTo("generated-api-key"));
    }

    /// <summary>
    /// Regression: once the downstream HttpClient has the correct ApiKey header,
    /// tool invocation must preserve that header name and must not substitute
    /// an incorrect header such as X-API-Key.
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

        // This isolates tool invocation from auth conversion. The dedicated passthrough test above
        // covers the real Program.cs behavior that maps Authorization: Bearer -> ApiKey.
        var mockHttpClient = new HttpClient(httpMessageHandler)
        {
            BaseAddress = new Uri("http://localhost/")
        };
        mockHttpClient.DefaultRequestHeaders.Add("ApiKey", "test-api-key");
        var mockFactory = new Mock<IHttpClientFactory>();
        mockFactory
            .Setup(f => f.CreateClient(BlastCmsClientConstants.HttpClientName))
            .Returns(mockHttpClient);
        services.AddSingleton(mockFactory.Object);
        services.AddScoped(_ => new TenantContext { TenantId = "test-tenant" });

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

    private sealed class CapturingHandler : HttpMessageHandler
    {
        public HttpRequestMessage? CapturedRequest { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            CapturedRequest = request;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{}", Encoding.UTF8, "application/json")
            });
        }
    }
}
