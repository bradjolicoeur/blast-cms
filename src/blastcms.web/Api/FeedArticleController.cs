using Asp.Versioning;
using blastcms.web.Attributes;
using blastcms.web.Data;
using blastcms.web.Handlers;
using blastcms.web.Infrastructure;
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
    public class FeedArticleController : ControllerBase
    {
        private readonly IDispatcher _mediator;

        public FeedArticleController(IDispatcher mediator)
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
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(IPagedData<FeedArticle>))]
        [SwaggerResponse((int)HttpStatusCode.Unauthorized, "Api Key is not valid")]
        public async Task<ActionResult<GetFeedArticles.PagedData>> GetAll([FromQuery] int skip = 0, [FromQuery] int take = 10, [FromQuery] int currentPage=0, [FromQuery] string search=null)
        {
            return await _mediator.Send(new GetFeedArticles.Query(skip, take, currentPage, search));
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
