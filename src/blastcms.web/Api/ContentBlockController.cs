using Asp.Versioning;
using blastcms.web.Attributes;
using blastcms.web.Data;
using blastcms.web.Handlers;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Net;
using System.Threading.Tasks;

namespace blastcms.web.Api
{
    [Route("api")]
    [ApiController]
    [ApiVersion("1.0")]
    [ApiKey]
    public class ContentBlockController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ContentBlockController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("contentblock/all")]
        [Produces("application/json")]
        [SwaggerOperation(
            Summary = "Get Content Block",
            Description = "Returns a paged list of Content Blocks",
            OperationId = "GetContentBlocks",
            Tags = new[] { "Content Block" }
        )]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(IPagedData<ContentBlock>))]
        [SwaggerResponse((int)HttpStatusCode.Unauthorized, "Api Key is not valid")]
        public async Task<ActionResult<GetContentBlocks.PagedData>> GetAll([FromQuery] int skip = 0, [FromQuery] int take = 10, [FromQuery] int currentPage = 0, [FromQuery] string search = null)
        {
            return await _mediator.Send(new GetContentBlocks.Query(skip, take, currentPage, search));
        }

        [HttpGet("contentblock/id/{id}")]
        [Produces("application/json")]
        [SwaggerOperation(
            Summary = "Get Content Block",
            Description = "Returns a Content Block",
            OperationId = "GetContentBlock",
            Tags = new[] { "Content Block" }
        )]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(ContentBlock))]
        [SwaggerResponse((int)HttpStatusCode.Unauthorized, "Api Key is not valid")]
        public async Task<ActionResult<ContentBlock>> GetById(Guid id)
        {
            var results = await _mediator.Send(new GetContentBlock.Query(id));

            return results.Data;
        }

        [HttpGet("contentblock/slug/{slug}")]
        [Produces("application/json")]
        [SwaggerOperation(
           Summary = "Get Content Block",
           Description = "Returns a Content Block",
           OperationId = "GetContentBlockBySlug",
           Tags = new[] { "Content Block" }
       )]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(ContentBlock))]
        [SwaggerResponse((int)HttpStatusCode.Unauthorized, "Api Key is not valid")]
        public async Task<ActionResult<ContentBlock>> GetBySlug(string slug)
        {
            var results = await _mediator.Send(new GetContentBlockBySlug.Query(slug));

            return results.Data;
        }

        [ApiKeyFull]
        [HttpPost("contentblock/")]
        [Produces("application/json")]
        [SwaggerOperation(
           Summary = "Alter Content Block",
           Description = "Inserts or Updates a Content Block",
           OperationId = "ContentBlockAlter",
           Tags = new[] { "Content Block" }
       )]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(AlterContentBlock.Model))]
        [SwaggerResponse((int)HttpStatusCode.Unauthorized, "Api Key is not valid")]
        public async Task<ActionResult<AlterContentBlock.Model>> PostContentBlock(AlterContentBlock.Command episode)
        {
            var result = await _mediator.Send(episode);
            return result;
        }

    }
}