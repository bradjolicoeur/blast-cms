extern alias mcpserver;

using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using mcpserver::blastcms.McpServer;
using NUnit.Framework;

namespace blastcms.web.tests.McpServer;

#nullable enable

[TestFixture]
public class TenantMiddlewareTests
{
    private static TenantMiddleware CreateMiddleware(RequestDelegate? next = null)
    {
        return new TenantMiddleware(next ?? (ctx => Task.CompletedTask));
    }

    [Test]
    public async Task InvokeAsync_WithSimpleTenantPath_SetsTenantIdAndRewritesPath()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Path = "/mytenant/mcp";
        var tenantContext = new TenantContext();
        var middleware = CreateMiddleware();

        // Act
        await middleware.InvokeAsync(context, tenantContext);

        // Assert
        Assert.That(tenantContext.TenantId, Is.EqualTo("mytenant"));
        Assert.That(context.Request.Path.Value, Is.EqualTo("/mcp"));
    }

    [Test]
    public async Task InvokeAsync_WithTenantAndSubPath_SetsTenantIdAndRewritesPath()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Path = "/customer2/mcp/sse";
        var tenantContext = new TenantContext();
        var middleware = CreateMiddleware();

        // Act
        await middleware.InvokeAsync(context, tenantContext);

        // Assert
        Assert.That(tenantContext.TenantId, Is.EqualTo("customer2"));
        Assert.That(context.Request.Path.Value, Is.EqualTo("/mcp/sse"));
    }

    [Test]
    public async Task InvokeAsync_WithBareMcp_Returns400WithErrorMessage()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Path = "/mcp";
        context.Response.Body = new MemoryStream();
        var tenantContext = new TenantContext();
        var middleware = CreateMiddleware();

        // Act
        await middleware.InvokeAsync(context, tenantContext);

        // Assert
        Assert.That(context.Response.StatusCode, Is.EqualTo(400));
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var body = await new StreamReader(context.Response.Body).ReadToEndAsync();
        Assert.That(body, Does.Contain("Tenant identifier is required"));
        Assert.That(body, Does.Contain("/{tenant}/mcp"));
    }

    [Test]
    public async Task InvokeAsync_WithBareMcpAndSubPath_Returns400WithErrorMessage()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Path = "/mcp/message";
        context.Response.Body = new MemoryStream();
        var tenantContext = new TenantContext();
        var middleware = CreateMiddleware();

        // Act
        await middleware.InvokeAsync(context, tenantContext);

        // Assert
        Assert.That(context.Response.StatusCode, Is.EqualTo(400));
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var body = await new StreamReader(context.Response.Body).ReadToEndAsync();
        Assert.That(body, Does.Contain("Tenant identifier is required"));
    }

    [Test]
    public async Task InvokeAsync_WithNonMcpPath_PassesThroughUnchanged()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Path = "/health";
        var tenantContext = new TenantContext();
        var nextCalled = false;
        var middleware = CreateMiddleware(ctx =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });

        // Act
        await middleware.InvokeAsync(context, tenantContext);

        // Assert
        Assert.That(context.Request.Path.Value, Is.EqualTo("/health"));
        Assert.That(tenantContext.TenantId, Is.EqualTo(string.Empty));
        Assert.That(nextCalled, Is.True);
    }

    [Test]
    public async Task InvokeAsync_WithFaviconPath_PassesThroughUnchanged()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Path = "/favicon.ico";
        var tenantContext = new TenantContext();
        var nextCalled = false;
        var middleware = CreateMiddleware(ctx =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });

        // Act
        await middleware.InvokeAsync(context, tenantContext);

        // Assert
        Assert.That(context.Request.Path.Value, Is.EqualTo("/favicon.ico"));
        Assert.That(tenantContext.TenantId, Is.EqualTo(string.Empty));
        Assert.That(nextCalled, Is.True);
    }

    [Test]
    public async Task InvokeAsync_WithDashedTenantId_PreservesTenantId()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Path = "/acme-corp/mcp";
        var tenantContext = new TenantContext();
        var middleware = CreateMiddleware();

        // Act
        await middleware.InvokeAsync(context, tenantContext);

        // Assert
        Assert.That(tenantContext.TenantId, Is.EqualTo("acme-corp"));
        Assert.That(context.Request.Path.Value, Is.EqualTo("/mcp"));
    }

    [Test]
    public async Task InvokeAsync_WithLongSubPath_RewritesCorrectly()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Path = "/tenant1/mcp/v1/api/messages";
        var tenantContext = new TenantContext();
        var middleware = CreateMiddleware();

        // Act
        await middleware.InvokeAsync(context, tenantContext);

        // Assert
        Assert.That(tenantContext.TenantId, Is.EqualTo("tenant1"));
        Assert.That(context.Request.Path.Value, Is.EqualTo("/mcp/v1/api/messages"));
    }

    [Test]
    public async Task InvokeAsync_WithCaseVariation_HandlesCorrectly()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Path = "/MyTenant/MCP/sse";
        var tenantContext = new TenantContext();
        var middleware = CreateMiddleware();

        // Act
        await middleware.InvokeAsync(context, tenantContext);

        // Assert
        Assert.That(tenantContext.TenantId, Is.EqualTo("MyTenant"));
        Assert.That(context.Request.Path.Value, Is.EqualTo("/MCP/sse"));
    }

    [Test]
    public async Task InvokeAsync_CallsNextMiddleware_WhenPathIsValid()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Path = "/tenant/mcp";
        var tenantContext = new TenantContext();
        var nextCalled = false;
        var middleware = CreateMiddleware(ctx =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });

        // Act
        await middleware.InvokeAsync(context, tenantContext);

        // Assert
        Assert.That(nextCalled, Is.True);
    }

    [Test]
    public async Task InvokeAsync_DoesNotCallNext_WhenReturning400()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Path = "/mcp";
        context.Response.Body = new MemoryStream();
        var tenantContext = new TenantContext();
        var nextCalled = false;
        var middleware = CreateMiddleware(ctx =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });

        // Act
        await middleware.InvokeAsync(context, tenantContext);

        // Assert
        Assert.That(nextCalled, Is.False);
        Assert.That(context.Response.StatusCode, Is.EqualTo(400));
    }
}
