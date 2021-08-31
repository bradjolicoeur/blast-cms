using blastcms.web.Attributes;
using blastcms.web.Data;
using blastcms.web.Handlers;
using MediatR;
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
        private readonly IMediator _mediator;

        public SitemapItemController(IMediator mediator)
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

    }
}
