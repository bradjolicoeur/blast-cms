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
    public class FeedArticleController : ControllerBase
    {
        private readonly IMediator _mediator;

        public FeedArticleController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("feedarticle/all")]
        [Produces("application/json")]
        [SwaggerOperation(
            Summary = "Get Feed Articles",
            Description = "Returns a paged list of Feed Articles",
            OperationId = "GetFeedArticles",
            Tags = new[] { "Feed Article" }
        )]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(IEnumerable<FeedArticle>))]
        [SwaggerResponse((int)HttpStatusCode.Unauthorized, "Api Key is not valid")]
        public async Task<ActionResult<IEnumerable<FeedArticle>>> GetAll([FromQuery] int skip = 0, [FromQuery] int take = 10, [FromQuery] int currentPage=0, [FromQuery] string search=null)
        {
            var results = await _mediator.Send(new GetFeedArticles.Query(skip, take, currentPage, search));

            return results.Data.ToArray();
        }

        [HttpGet("feedarticle/{id}")]
        [Produces("application/json")]
        [SwaggerOperation(
            Summary = "Get Feed Article",
            Description = "Returns a Feed Article",
            OperationId = "GetFeedArticle",
            Tags = new[] { "Feed Article" }
        )]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(FeedArticle))]
        [SwaggerResponse((int)HttpStatusCode.Unauthorized, "Api Key is not valid")]
        public async Task<ActionResult<FeedArticle>> Get(Guid id)
        {
            var results = await _mediator.Send(new GetFeedArticle.Query(id));

            return results.Data;
        }

    }
}
