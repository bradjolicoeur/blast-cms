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
    public class LandingPageController : ControllerBase
    {
        private readonly IMediator _mediator;

        public LandingPageController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("landingpage/all")]
        [Produces("application/json")]
        [SwaggerOperation(
            Summary = "Get Landing Page",
            Description = "Returns a paged list of Landing Pages",
            OperationId = "GetLandingPages",
            Tags = new[] { "Landing Page" }
        )]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(IPagedData<LandingPage>))]
        [SwaggerResponse((int)HttpStatusCode.Unauthorized, "Api Key is not valid")]
        public async Task<ActionResult<GetLandingPages.PagedData>> GetAll([FromQuery] int skip = 0, [FromQuery] int take = 10, [FromQuery] int currentPage=0, [FromQuery] string search=null)
        {
            return await _mediator.Send(new GetLandingPages.Query(skip, take, currentPage, search));
        }

        [HttpGet("landingpage/id/{id}")]
        [Produces("application/json")]
        [SwaggerOperation(
            Summary = "Get Landing Page",
            Description = "Returns a Landing Page",
            OperationId = "GetLandingPage",
            Tags = new[] { "Landing Page" }
        )]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(LandingPage))]
        [SwaggerResponse((int)HttpStatusCode.Unauthorized, "Api Key is not valid")]
        public async Task<ActionResult<LandingPage>> GetById(Guid id)
        {
            var results = await _mediator.Send(new GetLandingPage.Query(id));

            return results.Data;
        }

        [HttpGet("landingpage/slug/{slug}")]
        [Produces("application/json")]
        [SwaggerOperation(
           Summary = "Get Landing Page",
           Description = "Returns a Landing Page",
           OperationId = "GetLandingPage",
           Tags = new[] { "Landing Page" }
       )]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(LandingPage))]
        [SwaggerResponse((int)HttpStatusCode.Unauthorized, "Api Key is not valid")]
        public async Task<ActionResult<LandingPage>> GetBySlug(string slug)
        {
            var results = await _mediator.Send(new GetLandingPageBySlug.Query(slug));

            return results.Data;
        }

    }
}
