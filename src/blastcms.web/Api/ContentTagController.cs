using blastcms.web.Attributes;
using blastcms.web.Data;
using blastcms.web.Handlers;
using blastcms.web.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;
using System.Threading.Tasks;
using Asp.Versioning;

namespace blastcms.web.Api
{
    [Route("api")]
    [ApiController]
    [ApiVersion("1.0")]
    [ApiKey]
    public class ContentTagController : ControllerBase
    {
        private readonly IDispatcher _mediator;

        public ContentTagController(IDispatcher mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("contenttag/all")]
        [Produces("application/json")]
        [SwaggerOperation(
            Summary = "Get Content Tags",
            Description = "Returns a paged list of Content Tags",
            OperationId = "GetContentTags",
            Tags = new[] { "Content Tag" }
        )]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(IPagedData<ContentTag>))]
        [SwaggerResponse((int)HttpStatusCode.Unauthorized, "Api Key is not valid")]
        public async Task<ActionResult<GetContentTags.PagedData>> GetAll([FromQuery] int skip = 0, [FromQuery] int take = 10, [FromQuery] int currentPage=0)
        {
            return await _mediator.Send(new GetContentTags.Query(skip, take, currentPage));

        }

        [ApiKeyFull]
        [HttpPost("contenttag/")]
        [Produces("application/json")]
        [SwaggerOperation(
            Summary = "Alter Content Tag",
            Description = "Inserts or Updates a Content Tag",
            OperationId = "ContentTagAlter",
            Tags = new[] { "Content Tag" }
        )]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(AlterContentTag.Model))]
        [SwaggerResponse((int)HttpStatusCode.Unauthorized, "Api Key is not valid")]
        public async Task<ActionResult<AlterContentTag.Model>> PostContentTag(AlterContentTag.Command episode)
        {
            var result = await _mediator.Send(episode);
            return result;
        }

    }
}
