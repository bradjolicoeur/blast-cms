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

[TestFixture]
public class ImageFileToolTests
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
            .WithTools<FeedArticleTools>()
            .WithTools<ImageFileTools>();

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
    public async Task ListTools_ReturnsExpectedImageFileTools()
    {
        var handler = CreateSuccessHandler("{\"data\":[],\"count\":0,\"page\":1}");
        var (client, serverTask, cts) = await CreateClientServerPair(handler);

        await using (client)
        {
            var tools = await client.ListToolsAsync();

            Assert.That(tools, Is.Not.Null);
            Assert.That(tools.Any(t => t.Name == "list_image_files"), Is.True,
                "Expected list_image_files tool to be registered");
            Assert.That(tools.Any(t => t.Name == "get_image_file_by_id"), Is.True,
                "Expected get_image_file_by_id tool to be registered");
            Assert.That(tools.Any(t => t.Name == "get_image_file_by_title"), Is.True,
                "Expected get_image_file_by_title tool to be registered");
        }

        await cts.CancelAsync();
        await serverTask;
    }

    [Test]
    public async Task ListImageFiles_WithValidApiKey_ReturnsImageFileList()
    {
        const string expectedJson = "{\"data\":[{\"id\":\"00000000-0000-0000-0000-000000000001\",\"title\":\"Logo\",\"url\":\"https://example.com/logo.png\"}],\"count\":1,\"page\":1}";
        var handler = CreateSuccessHandler(expectedJson);
        var (client, serverTask, cts) = await CreateClientServerPair(handler);

        await using (client)
        {
            var result = await client.CallToolAsync("list_image_files",
                new Dictionary<string, object?> { ["page"] = 1, ["pageSize"] = 10 });

            Assert.That(result, Is.Not.Null);
            Assert.That(result.IsError, Is.Not.True, "Expected successful result");
            var textContent = result.Content.OfType<TextContentBlock>().FirstOrDefault();
            Assert.That(textContent, Is.Not.Null, "Expected text content in result");
            Assert.That(textContent!.Text, Does.Contain("Logo"));
        }

        await cts.CancelAsync();
        await serverTask;
    }

    [Test]
    public async Task GetImageFileById_WithValidApiKey_ReturnsImageFile()
    {
        const string expectedJson = "{\"id\":\"00000000-0000-0000-0000-000000000001\",\"title\":\"Logo\",\"url\":\"https://example.com/logo.png\"}";
        var handler = CreateSuccessHandler(expectedJson);
        var (client, serverTask, cts) = await CreateClientServerPair(handler);

        await using (client)
        {
            var result = await client.CallToolAsync("get_image_file_by_id",
                new Dictionary<string, object?> { ["id"] = "00000000-0000-0000-0000-000000000001" });

            Assert.That(result, Is.Not.Null);
            Assert.That(result.IsError, Is.Not.True, "Expected successful result");
            var textContent = result.Content.OfType<TextContentBlock>().FirstOrDefault();
            Assert.That(textContent, Is.Not.Null, "Expected text content in result");
            Assert.That(textContent!.Text, Does.Contain("Logo"));
        }

        await cts.CancelAsync();
        await serverTask;
    }

    [Test]
    public async Task GetImageFileByTitle_WithValidApiKey_ReturnsImageFile()
    {
        const string expectedJson = "{\"id\":\"00000000-0000-0000-0000-000000000001\",\"title\":\"Logo\",\"url\":\"https://example.com/logo.png\"}";
        var handler = CreateSuccessHandler(expectedJson);
        var (client, serverTask, cts) = await CreateClientServerPair(handler);

        await using (client)
        {
            var result = await client.CallToolAsync("get_image_file_by_title",
                new Dictionary<string, object?> { ["title"] = "Logo" });

            Assert.That(result, Is.Not.Null);
            Assert.That(result.IsError, Is.Not.True, "Expected successful result");
            var textContent = result.Content.OfType<TextContentBlock>().FirstOrDefault();
            Assert.That(textContent, Is.Not.Null, "Expected text content in result");
            Assert.That(textContent!.Text, Does.Contain("Logo"));
        }

        await cts.CancelAsync();
        await serverTask;
    }

    [Test]
    public async Task ListImageFiles_WithInvalidApiKey_ReturnsError()
    {
        var handler = CreateUnauthorizedHandler();
        var (client, serverTask, cts) = await CreateClientServerPair(handler);

        await using (client)
        {
            var result = await client.CallToolAsync("list_image_files",
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
