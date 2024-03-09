using blastcms.web.Attributes;
using blastcms.web.Handlers;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Net;
using System.Threading.Tasks;
using Asp.Versioning;

namespace blastcms.web.Api
{
    [Route("api")]
    [ApiController]
    [ApiVersion("1.0")]
    [ApiKey]
    public class EventController : ControllerBase
    {
        private readonly IMediator _mediator;

        public EventController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("event/all")]
        [Produces("application/json")]
        [SwaggerOperation(
            Summary = "Get Events",
            Description = "Returns a paged list of Events",
            OperationId = "GetEvents",
            Tags = new[] { "Event" }
        )]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(IPagedData<GetRecentEvents.Model>))]
        [SwaggerResponse((int)HttpStatusCode.Unauthorized, "Api Key is not valid")]
        public async Task<ActionResult<GetEventItems.PagedData>> GetAll([FromQuery] int skip = 0, [FromQuery] int take = 10, [FromQuery] int currentPage = 0, [FromQuery] string search = null, [FromQuery] string tag = null)
        {
            return await _mediator.Send(new GetEventItems.Query(skip, take, currentPage, search,tag));
        }

        [HttpGet("event/recent")]
        [Produces("application/json")]
        [SwaggerOperation(
            Summary = "Get Recent Events",
            Description = "Returns a paged list of Events that have happened recently or will happen in the future",
            OperationId = "GetEventsRecent",
            Tags = new[] { "Event" }
        )]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(IPagedData<GetRecentEvents.Model>))]
        [SwaggerResponse((int)HttpStatusCode.Unauthorized, "Api Key is not valid")]
        public async Task<ActionResult<GetRecentEvents.PagedData>> GetRecent([FromQuery] int skip = 0, [FromQuery] int take = 10, [FromQuery] int currentPage = 0, [FromQuery] string search = null, [FromQuery] string tag = null, [FromQuery] int days = 30)
        {
            return await _mediator.Send(new GetRecentEvents.Query(skip, take, currentPage, search, tag, days));
        }

        [HttpGet("event/{id}")]
        [Produces("application/json")]
        [SwaggerOperation(
            Summary = "Get Event by Id",
            Description = "Returns an Event",
            OperationId = "GetEvent",
            Tags = new[] { "Event" }
        )]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(GetEventItem.Model))]
        [SwaggerResponse((int)HttpStatusCode.Unauthorized, "Api Key is not valid")]
        public async Task<ActionResult<GetEventItem.Model>> Get(Guid id)
        {
            var results = await _mediator.Send(new GetEventItem.Query(id));

            return results;
        }

        [HttpGet("event/slug/{slug}")]
        [Produces("application/json")]
        [SwaggerOperation(
           Summary = "Get Event By Slug",
           Description = "Returns an Event",
           OperationId = "GetEventBySlug",
           Tags = new[] { "Event" }
       )]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(GetEventItemBySlug.Model))]
        [SwaggerResponse((int)HttpStatusCode.Unauthorized, "Api Key is not valid")]
        public async Task<ActionResult<GetEventItemBySlug.Model>> GetBySlug(string slug)
        {
            var results = await _mediator.Send(new GetEventItemBySlug.Query(slug));

            return results;
        }

        [ApiKeyFull]
        [HttpPut("event/")]
        [Produces("application/json")]
        [SwaggerOperation(
           Summary = "Alter Event",
           Description = "Inserts or Updates an Event",
           OperationId = "EventAlter",
           Tags = new[] { "Event" }
       )]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(PutEventItem.Model))]
        [SwaggerResponse((int)HttpStatusCode.Unauthorized, "Api Key is not valid")]
        public async Task<ActionResult<PutEventItem.Model>> PostEventItem(PutEventItem.Command episode)
        {
            var result = await _mediator.Send(episode);
            return result;
        }
    }
}
