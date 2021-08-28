using blastcms.web.Attributes;
using blastcms.web.Data;
using blastcms.web.Handlers;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace blastcms.web.Api
{
    [Route("api")]
    [ApiController]
    [ApiVersion("1.0")]
    [ApiKey]
    public class ContentTagController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ContentTagController(IMediator mediator)
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

       

    }
}
