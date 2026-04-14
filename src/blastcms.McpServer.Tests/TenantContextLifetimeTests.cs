using blastcms.McpServer.Tools;
using Microsoft.AspNetCore.Http;
using Moq;
using Moq.Protected;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace blastcms.McpServer.Tests;

[TestFixture]
public class TenantContextLifetimeTests
{
    [Test]
    public async Task ListBlogArticles_UsesTenantFromCurrentHttpContext_WhenToolWasCreatedBeforeRequest()
    {
        HttpRequestMessage? capturedRequest = null;
        var httpClientFactory = CreateHttpClientFactory(request =>
        {
            capturedRequest = request;
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"data\":[]}", Encoding.UTF8, "application/json")
            };
        });

        var httpContextAccessor = new HttpContextAccessor();
        var tenantContext = new TenantContext(httpContextAccessor);
        var tool = new BlogArticleTools(httpClientFactory.Object, tenantContext);

        httpContextAccessor.HttpContext = new DefaultHttpContext();
        tenantContext.TenantId = "tenant-a";

        await tool.ListBlogArticles();

        Assert.That(capturedRequest, Is.Not.Null);
        Assert.That(capturedRequest!.RequestUri!.ToString(),
            Is.EqualTo("http://localhost/tenant-a/api/blogarticle/all?currentPage=1&take=10&skip=0"));
    }

    [Test]
    public async Task ListBlogArticles_ReusesSameToolInstanceWithoutKeepingStaleTenant()
    {
        var requests = new List<string>();
        var httpClientFactory = CreateHttpClientFactory(request =>
        {
            requests.Add(request.RequestUri!.ToString());
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"data\":[]}", Encoding.UTF8, "application/json")
            };
        });

        var httpContextAccessor = new HttpContextAccessor();
        var tenantContext = new TenantContext(httpContextAccessor);
        var tool = new BlogArticleTools(httpClientFactory.Object, tenantContext);

        httpContextAccessor.HttpContext = new DefaultHttpContext();
        tenantContext.TenantId = "tenant-a";
        await tool.ListBlogArticles();

        httpContextAccessor.HttpContext = new DefaultHttpContext();
        tenantContext.TenantId = "tenant-b";
        await tool.ListBlogArticles();

        Assert.That(requests, Is.EqualTo(new[]
        {
            "http://localhost/tenant-a/api/blogarticle/all?currentPage=1&take=10&skip=0",
            "http://localhost/tenant-b/api/blogarticle/all?currentPage=1&take=10&skip=0"
        }));
    }

    private static Mock<IHttpClientFactory> CreateHttpClientFactory(Func<HttpRequestMessage, HttpResponseMessage> responder)
    {
        var handler = new Mock<HttpMessageHandler>();
        handler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync((HttpRequestMessage request, CancellationToken _) => responder(request));

        var httpClient = new HttpClient(handler.Object)
        {
            BaseAddress = new Uri("http://localhost/")
        };

        var factory = new Mock<IHttpClientFactory>();
        factory.Setup(f => f.CreateClient(BlastCmsClientConstants.HttpClientName)).Returns(httpClient);
        return factory;
    }
}
