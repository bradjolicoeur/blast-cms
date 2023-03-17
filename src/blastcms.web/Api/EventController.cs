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
            Summary = "Get Event Venues",
            Description = "Returns a paged list of Event Venues",
            OperationId = "GetEvents",
            Tags = new[] { "Event" }
        )]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(IPagedData<EventItem>))]
        [SwaggerResponse((int)HttpStatusCode.Unauthorized, "Api Key is not valid")]
        public async Task<ActionResult<GetEventItems.PagedData>> GetAll([FromQuery] int skip = 0, [FromQuery] int take = 10, [FromQuery] int currentPage = 0, [FromQuery] string search = null)
        {
            return await _mediator.Send(new GetEventItems.Query(skip, take, currentPage, search));
        }

        [HttpGet("event/{id}")]
        [Produces("application/json")]
        [SwaggerOperation(
            Summary = "Get Event Venue",
            Description = "Returns an Event Venue",
            OperationId = "GetEvent",
            Tags = new[] { "Event" }
        )]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(EventItem))]
        [SwaggerResponse((int)HttpStatusCode.Unauthorized, "Api Key is not valid")]
        public async Task<ActionResult<EventItem>> Get(Guid id)
        {
            var results = await _mediator.Send(new GetEventItem.Query(id));

            return results.Data;
        }

        [HttpGet("event/slug/{slug}")]
        [Produces("application/json")]
        [SwaggerOperation(
           Summary = "Get Event Venue By Slug",
           Description = "Returns an Event Venue",
           OperationId = "GetEventBySlug",
           Tags = new[] { "Event" }
       )]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(EventItem))]
        [SwaggerResponse((int)HttpStatusCode.Unauthorized, "Api Key is not valid")]
        public async Task<ActionResult<EventItem>> GetBySlug(string slug)
        {
            var results = await _mediator.Send(new GetEventItemBySlug.Query(slug));

            return results.Data;
        }

        [ApiKeyFull]
        [HttpPost("event/")]
        [Produces("application/json")]
        [SwaggerOperation(
           Summary = "Alter Event",
           Description = "Inserts or Updates an Event",
           OperationId = "EventAlter",
           Tags = new[] { "Event" }
       )]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(AlterEventItem.Model))]
        [SwaggerResponse((int)HttpStatusCode.Unauthorized, "Api Key is not valid")]
        public async Task<ActionResult<AlterEventItem.Model>> PostEventItem(AlterEventItem.Command episode)
        {
            var result = await _mediator.Send(episode);
            return result;
        }
    }
}
