using Asp.Versioning;
using blastcms.web.Attributes;
using blastcms.web.Data;
using blastcms.web.Handlers;
using blastcms.web.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;
using System.Threading.Tasks;


namespace blastcms.web.Api
{
    [Route("api")]
    [ApiController]
    [ApiVersion("1.0")]
    [ApiKey]
    public class SitemapItemController : ControllerBase
    {
        private readonly IDispatcher _mediator;

        public SitemapItemController(IDispatcher mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("sitemapitem/all")]
        [Produces("application/json")]
        [SwaggerOperation(
            Summary = "Get Sitemap Items",
            Description = "Returns a paged list of Sitemap Items",
            OperationId = "GetSitemapItems",
            Tags = new[] { "Sitemap Item" }
        )]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(IPagedData<SitemapItem>))]
        [SwaggerResponse((int)HttpStatusCode.Unauthorized, "Api Key is not valid")]
        public async Task<ActionResult<GetSitemapItems.PagedData>> GetAll([FromQuery] int skip = 0, [FromQuery] int take = 10, [FromQuery] int currentPage = 0)
        {
            return await _mediator.Send(new GetSitemapItems.Query(skip, take, currentPage));

        }

        [ApiKeyFull]
        [HttpPost("sitemapitem/")]
        [Produces("application/json")]
        [SwaggerOperation(
           Summary = "Alter Sitemap Item",
           Description = "Inserts or Updates a Sitemap Item",
           OperationId = "SitemapItemAlter",
           Tags = new[] { "Sitemap Item" }
       )]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(AlterSitemapItem.Model))]
        [SwaggerResponse((int)HttpStatusCode.Unauthorized, "Api Key is not valid")]
        public async Task<ActionResult<AlterSitemapItem.Model>> PostPodcastEpisode(AlterSitemapItem.Command episode)
        {
            var result = await _mediator.Send(episode);
            return result;
        }

    }
}
