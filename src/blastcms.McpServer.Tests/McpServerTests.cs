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
/// Integration tests for the Blast CMS MCP server. Tests connect an McpClient
/// to the configured McpServer in-process to validate tool behaviour, including
/// proper rejection of requests with invalid or missing API keys.
/// </summary>
[TestFixture]
public class McpServerTests
{
    /// <summary>
    /// Builds an in-process MCP server using the given HttpMessageHandler and 
    /// returns a connected McpClient. The caller is responsible for disposing 
    /// the client and cancelling the server task.
    /// </summary>
    private static async Task<(McpClient client, Task serverTask, CancellationTokenSource cts)> CreateClientServerPair(
        HttpMessageHandler httpMessageHandler)
    {
        var clientToServer = new Pipe();
        var serverToClient = new Pipe();

        var cts = new CancellationTokenSource();

        var services = new ServiceCollection();
        services.AddLogging(b => b.SetMinimumLevel(LogLevel.None));

        // Register the mocked HTTP client factory
        var mockHttpClient = new HttpClient(httpMessageHandler)
        {
            BaseAddress = new Uri("http://localhost/")
        };
        var mockFactory = new Mock<IHttpClientFactory>();
        mockFactory
            .Setup(f => f.CreateClient(BlastCmsClientConstants.HttpClientName))
            .Returns(mockHttpClient);
        services.AddSingleton(mockFactory.Object);

        // Configure the MCP server using in-process stream transport
        // Explicitly register each tool type so the test project does not need to scan the MCP server assembly
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

    [Test]
    public async Task ListTools_ReturnsExpectedBlogArticleTools()
    {
        var handler = CreateSuccessHandler("{\"data\":[],\"count\":0,\"page\":1}");
        var (client, serverTask, cts) = await CreateClientServerPair(handler);

        await using (client)
        {
            var tools = await client.ListToolsAsync();

            Assert.That(tools, Is.Not.Null);
            Assert.That(tools.Any(t => t.Name == "list_blog_articles"), Is.True,
                "Expected list_blog_articles tool to be registered");
            Assert.That(tools.Any(t => t.Name == "get_blog_article_by_slug"), Is.True,
                "Expected get_blog_article_by_slug tool to be registered");
            Assert.That(tools.Any(t => t.Name == "get_blog_article_by_id"), Is.True,
                "Expected get_blog_article_by_id tool to be registered");
        }

        await cts.CancelAsync();
        await serverTask;
    }

    [Test]
    public async Task ListTools_ReturnsExpectedContentBlockTools()
    {
        var handler = CreateSuccessHandler("{\"data\":[],\"count\":0,\"page\":1}");
        var (client, serverTask, cts) = await CreateClientServerPair(handler);

        await using (client)
        {
            var tools = await client.ListToolsAsync();

            Assert.That(tools.Any(t => t.Name == "list_content_blocks"), Is.True,
                "Expected list_content_blocks tool to be registered");
            Assert.That(tools.Any(t => t.Name == "get_content_block_by_slug"), Is.True,
                "Expected get_content_block_by_slug tool to be registered");
            Assert.That(tools.Any(t => t.Name == "get_content_blocks_by_group"), Is.True,
                "Expected get_content_blocks_by_group tool to be registered");
            Assert.That(tools.Any(t => t.Name == "get_content_block_by_id"), Is.True,
                "Expected get_content_block_by_id tool to be registered");
        }

        await cts.CancelAsync();
        await serverTask;
    }

    [Test]
    public async Task ListTools_ReturnsExpectedFeedArticleTools()
    {
        var handler = CreateSuccessHandler("{\"data\":[],\"count\":0,\"page\":1}");
        var (client, serverTask, cts) = await CreateClientServerPair(handler);

        await using (client)
        {
            var tools = await client.ListToolsAsync();

            Assert.That(tools.Any(t => t.Name == "list_feed_articles"), Is.True,
                "Expected list_feed_articles tool to be registered");
            Assert.That(tools.Any(t => t.Name == "get_feed_article_by_id"), Is.True,
                "Expected get_feed_article_by_id tool to be registered");
        }

        await cts.CancelAsync();
        await serverTask;
    }

    [Test]
    public async Task ListBlogArticles_WithValidApiKey_ReturnsArticleList()
    {
        const string expectedJson = "{\"data\":[{\"id\":\"00000000-0000-0000-0000-000000000001\",\"title\":\"Test Article\",\"slug\":\"test-article\"}],\"count\":1,\"page\":1}";
        var handler = CreateSuccessHandler(expectedJson);
        var (client, serverTask, cts) = await CreateClientServerPair(handler);

        await using (client)
        {
            var result = await client.CallToolAsync("list_blog_articles",
                new Dictionary<string, object?> { ["page"] = 1, ["pageSize"] = 10 });

            Assert.That(result, Is.Not.Null);
            Assert.That(result.IsError, Is.Not.True, "Expected successful result");
            var textContent = result.Content.OfType<TextContentBlock>().FirstOrDefault();
            Assert.That(textContent, Is.Not.Null, "Expected text content in result");
            Assert.That(textContent!.Text, Does.Contain("Test Article"));
        }

        await cts.CancelAsync();
        await serverTask;
    }

    [Test]
    public async Task GetBlogArticleBySlug_WithValidApiKey_ReturnsArticle()
    {
        const string expectedJson = "{\"id\":\"00000000-0000-0000-0000-000000000001\",\"title\":\"Test Article\",\"slug\":\"test-article\"}";
        var handler = CreateSuccessHandler(expectedJson);
        var (client, serverTask, cts) = await CreateClientServerPair(handler);

        await using (client)
        {
            var result = await client.CallToolAsync("get_blog_article_by_slug",
                new Dictionary<string, object?> { ["slug"] = "test-article" });

            Assert.That(result, Is.Not.Null);
            Assert.That(result.IsError, Is.Not.True, "Expected successful result");
            var textContent = result.Content.OfType<TextContentBlock>().FirstOrDefault();
            Assert.That(textContent, Is.Not.Null, "Expected text content in result");
            Assert.That(textContent!.Text, Does.Contain("test-article"));
        }

        await cts.CancelAsync();
        await serverTask;
    }

    [Test]
    public async Task ListBlogArticles_WithInvalidApiKey_ReturnsError()
    {
        var handler = CreateUnauthorizedHandler();
        var (client, serverTask, cts) = await CreateClientServerPair(handler);

        await using (client)
        {
            var result = await client.CallToolAsync("list_blog_articles",
                new Dictionary<string, object?> { ["page"] = 1, ["pageSize"] = 10 });

            Assert.That(result, Is.Not.Null);
            Assert.That(result.IsError, Is.True, "Expected error result when API key is invalid");
        }

        await cts.CancelAsync();
        await serverTask;
    }

    [Test]
    public async Task ListContentBlocks_WithValidApiKey_ReturnsContentBlockList()
    {
        const string expectedJson = "{\"data\":[{\"id\":\"00000000-0000-0000-0000-000000000002\",\"title\":\"Hero Banner\",\"slug\":\"hero-banner\"}],\"count\":1,\"page\":1}";
        var handler = CreateSuccessHandler(expectedJson);
        var (client, serverTask, cts) = await CreateClientServerPair(handler);

        await using (client)
        {
            var result = await client.CallToolAsync("list_content_blocks",
                new Dictionary<string, object?> { ["page"] = 1, ["pageSize"] = 10 });

            Assert.That(result, Is.Not.Null);
            Assert.That(result.IsError, Is.Not.True, "Expected successful result");
            var textContent = result.Content.OfType<TextContentBlock>().FirstOrDefault();
            Assert.That(textContent, Is.Not.Null, "Expected text content in result");
            Assert.That(textContent!.Text, Does.Contain("Hero Banner"));
        }

        await cts.CancelAsync();
        await serverTask;
    }

    [Test]
    public async Task ListContentBlocks_WithInvalidApiKey_ReturnsError()
    {
        var handler = CreateUnauthorizedHandler();
        var (client, serverTask, cts) = await CreateClientServerPair(handler);

        await using (client)
        {
            var result = await client.CallToolAsync("list_content_blocks",
                new Dictionary<string, object?> { ["page"] = 1, ["pageSize"] = 10 });

            Assert.That(result, Is.Not.Null);
            Assert.That(result.IsError, Is.True, "Expected error result when API key is invalid");
        }

        await cts.CancelAsync();
        await serverTask;
    }

    [Test]
    public async Task ListFeedArticles_WithValidApiKey_ReturnsFeedArticleList()
    {
        const string expectedJson = "{\"data\":[{\"id\":\"00000000-0000-0000-0000-000000000003\",\"title\":\"News Article\",\"articleUrl\":\"https://example.com/news\"}],\"count\":1,\"page\":1}";
        var handler = CreateSuccessHandler(expectedJson);
        var (client, serverTask, cts) = await CreateClientServerPair(handler);

        await using (client)
        {
            var result = await client.CallToolAsync("list_feed_articles",
                new Dictionary<string, object?> { ["page"] = 1, ["pageSize"] = 10 });

            Assert.That(result, Is.Not.Null);
            Assert.That(result.IsError, Is.Not.True, "Expected successful result");
            var textContent = result.Content.OfType<TextContentBlock>().FirstOrDefault();
            Assert.That(textContent, Is.Not.Null, "Expected text content in result");
            Assert.That(textContent!.Text, Does.Contain("News Article"));
        }

        await cts.CancelAsync();
        await serverTask;
    }

    [Test]
    public async Task ListFeedArticles_WithInvalidApiKey_ReturnsError()
    {
        var handler = CreateUnauthorizedHandler();
        var (client, serverTask, cts) = await CreateClientServerPair(handler);

        await using (client)
        {
            var result = await client.CallToolAsync("list_feed_articles",
                new Dictionary<string, object?> { ["page"] = 1, ["pageSize"] = 10 });

            Assert.That(result, Is.Not.Null);
            Assert.That(result.IsError, Is.True, "Expected error result when API key is invalid");
        }

        await cts.CancelAsync();
        await serverTask;
    }

    private static HttpMessageHandler CreateSuccessHandler(string jsonContent)
    {
        var mock = new Mock<HttpMessageHandler>();
        mock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(jsonContent, Encoding.UTF8, "application/json")
            });
        return mock.Object;
    }

    private static HttpMessageHandler CreateUnauthorizedHandler()
    {
        var mock = new Mock<HttpMessageHandler>();
        mock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.Unauthorized)
            {
                Content = new StringContent("Unauthorized", Encoding.UTF8, "text/plain")
            });
        return mock.Object;
    }
}
