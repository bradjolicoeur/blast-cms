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
using System.Threading;
using System.Threading.Tasks;

namespace blastcms.McpServer.Tests;

/// <summary>
/// Integration tests for write tools on the Blast CMS MCP server.
/// Covers create_blog_article, update_blog_article, create_content_block,
/// and update_content_block — verifying success paths, error propagation,
/// and tool registration.
/// </summary>
[TestFixture]
public class McpServerWriteToolTests
{
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

    // ── Shared handler helpers ────────────────────────────────────────────────

    private static HttpMessageHandler CreateSuccessHandler(string jsonContent, HttpStatusCode status = HttpStatusCode.OK)
    {
        var mock = new Mock<HttpMessageHandler>();
        mock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(status)
            {
                Content = new StringContent(jsonContent, Encoding.UTF8, "application/json")
            });
        return mock.Object;
    }

    private static HttpMessageHandler CreateErrorHandler(HttpStatusCode status, string body = "")
    {
        var mock = new Mock<HttpMessageHandler>();
        mock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(status)
            {
                Content = new StringContent(body, Encoding.UTF8, "text/plain")
            });
        return mock.Object;
    }

    // ── create_blog_article ───────────────────────────────────────────────────

    [Test]
    public async Task ListTools_ReturnsCreateBlogArticleTool()
    {
        var handler = CreateSuccessHandler("{}");
        var (client, serverTask, cts) = await CreateClientServerPair(handler);

        await using (client)
        {
            var tools = await client.ListToolsAsync();

            Assert.That(tools.Any(t => t.Name == "create_blog_article"), Is.True,
                "Expected create_blog_article tool to be registered");
        }

        await cts.CancelAsync();
        await serverTask;
    }

    [Test]
    public async Task CreateBlogArticle_Success_ReturnsCreatedArticle()
    {
        const string responseJson = "{\"id\":\"00000000-0000-0000-0000-000000000010\",\"title\":\"New Article\",\"slug\":\"new-article\"}";
        var handler = CreateSuccessHandler(responseJson, HttpStatusCode.Created);
        var (client, serverTask, cts) = await CreateClientServerPair(handler);

        await using (client)
        {
            var result = await client.CallToolAsync("create_blog_article",
                new Dictionary<string, object?>
                {
                    ["title"] = "New Article",
                    ["slug"] = "new-article",
                    ["content"] = "This is the article body."
                });

            Assert.That(result, Is.Not.Null);
            Assert.That(result.IsError, Is.Not.True, "Expected successful result");
            var textContent = result.Content.OfType<TextContentBlock>().FirstOrDefault();
            Assert.That(textContent, Is.Not.Null, "Expected text content in result");
            Assert.That(textContent!.Text, Does.Contain("New Article"));
        }

        await cts.CancelAsync();
        await serverTask;
    }

    [Test]
    public async Task CreateBlogArticle_Unauthorized_ReturnsError()
    {
        var handler = CreateErrorHandler(HttpStatusCode.Unauthorized, "Unauthorized");
        var (client, serverTask, cts) = await CreateClientServerPair(handler);

        await using (client)
        {
            var result = await client.CallToolAsync("create_blog_article",
                new Dictionary<string, object?>
                {
                    ["title"] = "New Article",
                    ["slug"] = "new-article"
                });

            Assert.That(result, Is.Not.Null);
            Assert.That(result.IsError, Is.True, "Expected error result when API key is invalid");
        }

        await cts.CancelAsync();
        await serverTask;
    }

    [Test]
    public async Task CreateBlogArticle_BadRequest_ReturnsError()
    {
        var handler = CreateErrorHandler(HttpStatusCode.BadRequest, "title is required");
        var (client, serverTask, cts) = await CreateClientServerPair(handler);

        await using (client)
        {
            // Simulate missing required field by omitting title
            var result = await client.CallToolAsync("create_blog_article",
                new Dictionary<string, object?>
                {
                    ["slug"] = "new-article"
                });

            Assert.That(result, Is.Not.Null);
            Assert.That(result.IsError, Is.True, "Expected error result on bad request");
        }

        await cts.CancelAsync();
        await serverTask;
    }

    // ── update_blog_article ───────────────────────────────────────────────────

    [Test]
    public async Task UpdateBlogArticle_Success_ReturnsUpdatedArticle()
    {
        const string responseJson = "{\"id\":\"00000000-0000-0000-0000-000000000010\",\"title\":\"Updated Article\",\"slug\":\"updated-article\"}";
        var handler = CreateSuccessHandler(responseJson);
        var (client, serverTask, cts) = await CreateClientServerPair(handler);

        await using (client)
        {
            var result = await client.CallToolAsync("update_blog_article",
                new Dictionary<string, object?>
                {
                    ["id"] = "00000000-0000-0000-0000-000000000010",
                    ["title"] = "Updated Article",
                    ["slug"] = "updated-article",
                    ["content"] = "Updated body."
                });

            Assert.That(result, Is.Not.Null);
            Assert.That(result.IsError, Is.Not.True, "Expected successful result");
            var textContent = result.Content.OfType<TextContentBlock>().FirstOrDefault();
            Assert.That(textContent, Is.Not.Null, "Expected text content in result");
            Assert.That(textContent!.Text, Does.Contain("Updated Article"));
        }

        await cts.CancelAsync();
        await serverTask;
    }

    [Test]
    public async Task UpdateBlogArticle_NotFound_ReturnsError()
    {
        var handler = CreateErrorHandler(HttpStatusCode.NotFound, "Not Found");
        var (client, serverTask, cts) = await CreateClientServerPair(handler);

        await using (client)
        {
            var result = await client.CallToolAsync("update_blog_article",
                new Dictionary<string, object?>
                {
                    ["id"] = "00000000-0000-0000-0000-000000000099",
                    ["title"] = "Ghost Article"
                });

            Assert.That(result, Is.Not.Null);
            Assert.That(result.IsError, Is.True, "Expected error result when article is not found");
        }

        await cts.CancelAsync();
        await serverTask;
    }

    [Test]
    public async Task UpdateBlogArticle_Unauthorized_ReturnsError()
    {
        var handler = CreateErrorHandler(HttpStatusCode.Unauthorized, "Unauthorized");
        var (client, serverTask, cts) = await CreateClientServerPair(handler);

        await using (client)
        {
            var result = await client.CallToolAsync("update_blog_article",
                new Dictionary<string, object?>
                {
                    ["id"] = "00000000-0000-0000-0000-000000000010",
                    ["title"] = "Updated Article"
                });

            Assert.That(result, Is.Not.Null);
            Assert.That(result.IsError, Is.True, "Expected error result when API key is invalid");
        }

        await cts.CancelAsync();
        await serverTask;
    }

    // ── create_content_block ─────────────────────────────────────────────────

    [Test]
    public async Task ListTools_ReturnsCreateContentBlockTool()
    {
        var handler = CreateSuccessHandler("{}");
        var (client, serverTask, cts) = await CreateClientServerPair(handler);

        await using (client)
        {
            var tools = await client.ListToolsAsync();

            Assert.That(tools.Any(t => t.Name == "create_content_block"), Is.True,
                "Expected create_content_block tool to be registered");
        }

        await cts.CancelAsync();
        await serverTask;
    }

    [Test]
    public async Task CreateContentBlock_Success_ReturnsCreatedBlock()
    {
        const string responseJson = "{\"id\":\"00000000-0000-0000-0000-000000000020\",\"title\":\"Hero Banner\",\"slug\":\"hero-banner\"}";
        var handler = CreateSuccessHandler(responseJson, HttpStatusCode.Created);
        var (client, serverTask, cts) = await CreateClientServerPair(handler);

        await using (client)
        {
            var result = await client.CallToolAsync("create_content_block",
                new Dictionary<string, object?>
                {
                    ["title"] = "Hero Banner",
                    ["slug"] = "hero-banner",
                    ["content"] = "<h1>Welcome</h1>",
                    ["group"] = "homepage"
                });

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
    public async Task CreateContentBlock_Unauthorized_ReturnsError()
    {
        var handler = CreateErrorHandler(HttpStatusCode.Unauthorized, "Unauthorized");
        var (client, serverTask, cts) = await CreateClientServerPair(handler);

        await using (client)
        {
            var result = await client.CallToolAsync("create_content_block",
                new Dictionary<string, object?>
                {
                    ["title"] = "Hero Banner",
                    ["slug"] = "hero-banner"
                });

            Assert.That(result, Is.Not.Null);
            Assert.That(result.IsError, Is.True, "Expected error result when API key is invalid");
        }

        await cts.CancelAsync();
        await serverTask;
    }

    // ── update_content_block ─────────────────────────────────────────────────

    [Test]
    public async Task UpdateContentBlock_Success_ReturnsUpdatedBlock()
    {
        const string responseJson = "{\"id\":\"00000000-0000-0000-0000-000000000020\",\"title\":\"Updated Banner\",\"slug\":\"updated-banner\"}";
        var handler = CreateSuccessHandler(responseJson);
        var (client, serverTask, cts) = await CreateClientServerPair(handler);

        await using (client)
        {
            var result = await client.CallToolAsync("update_content_block",
                new Dictionary<string, object?>
                {
                    ["id"] = "00000000-0000-0000-0000-000000000020",
                    ["title"] = "Updated Banner",
                    ["slug"] = "updated-banner",
                    ["content"] = "<h1>New content</h1>",
                    ["group"] = "homepage"
                });

            Assert.That(result, Is.Not.Null);
            Assert.That(result.IsError, Is.Not.True, "Expected successful result");
            var textContent = result.Content.OfType<TextContentBlock>().FirstOrDefault();
            Assert.That(textContent, Is.Not.Null, "Expected text content in result");
            Assert.That(textContent!.Text, Does.Contain("Updated Banner"));
        }

        await cts.CancelAsync();
        await serverTask;
    }

    [Test]
    public async Task UpdateContentBlock_NotFound_ReturnsError()
    {
        var handler = CreateErrorHandler(HttpStatusCode.NotFound, "Not Found");
        var (client, serverTask, cts) = await CreateClientServerPair(handler);

        await using (client)
        {
            var result = await client.CallToolAsync("update_content_block",
                new Dictionary<string, object?>
                {
                    ["id"] = "00000000-0000-0000-0000-000000000099",
                    ["title"] = "Ghost Block"
                });

            Assert.That(result, Is.Not.Null);
            Assert.That(result.IsError, Is.True, "Expected error result when content block is not found");
        }

        await cts.CancelAsync();
        await serverTask;
    }

    [Test]
    public async Task UpdateContentBlock_Unauthorized_ReturnsError()
    {
        var handler = CreateErrorHandler(HttpStatusCode.Unauthorized, "Unauthorized");
        var (client, serverTask, cts) = await CreateClientServerPair(handler);

        await using (client)
        {
            var result = await client.CallToolAsync("update_content_block",
                new Dictionary<string, object?>
                {
                    ["id"] = "00000000-0000-0000-0000-000000000020",
                    ["title"] = "Updated Banner"
                });

            Assert.That(result, Is.Not.Null);
            Assert.That(result.IsError, Is.True, "Expected error result when API key is invalid");
        }

        await cts.CancelAsync();
        await serverTask;
    }
}
