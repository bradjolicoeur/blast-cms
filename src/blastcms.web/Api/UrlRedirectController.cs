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
    public class UrlRedirectController : ControllerBase
    {
        private readonly IMediator _mediator;

        public UrlRedirectController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("urlredirect/all")]
        [Produces("application/json")]
        [SwaggerOperation(
            Summary = "Get URL Redirects",
            Description = "Returns a paged list of URL Redirects",
            OperationId = "GetUrlRedirects",
            Tags = new[] { "URL Redirect" }
        )]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(IPagedData<UrlRedirect>))]
        [SwaggerResponse((int)HttpStatusCode.Unauthorized, "Api Key is not valid")]
        public async Task<ActionResult<GetUrlRedirects.PagedData>> GetAll([FromQuery] int skip = 0, [FromQuery] int take = 10, [FromQuery] int currentPage = 0)
        {
            return await _mediator.Send(new GetUrlRedirects.Query(skip, take, currentPage));

        }

        [HttpGet("urlredirect/from/{fromurl}")]
        [Produces("application/json")]
        [SwaggerOperation(
          Summary = "Get URL Redirect By From",
          Description = "Returns a URL Redirect By the FROM Relative URL",
          OperationId = "GetUrlRedirectByFrom",
          Tags = new[] { "URL Redirect" }
      )]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(UrlRedirect))]
        [SwaggerResponse((int)HttpStatusCode.Unauthorized, "Api Key is not valid")]
        public async Task<ActionResult<UrlRedirect>> GetBySlug(string fromurl)
        {
            var results = await _mediator.Send(new GetUrlRedirectByFrom.Query(fromurl));

            return results.Data;
        }

        [ApiKeyFull]
        [HttpPost("urlredirect/")]
        [Produces("application/json")]
        [SwaggerOperation(
           Summary = "Alter URL Redirect",
           Description = "Inserts or Updates a Url Redirect",
           OperationId = "UrlRedirectAlter",
           Tags = new[] { "URL Redirect" }
       )]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(AlterUrlRedirect.Model))]
        [SwaggerResponse((int)HttpStatusCode.Unauthorized, "Api Key is not valid")]
        public async Task<ActionResult<AlterUrlRedirect.Model>> PostPodcastEpisode(AlterUrlRedirect.Command episode)
        {
            var result = await _mediator.Send(episode);
            return result;
        }

    }
}
