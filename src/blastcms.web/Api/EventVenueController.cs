﻿using blastcms.web.Attributes;
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
    public class EventVenueController : ControllerBase
    {
        private readonly IMediator _mediator;

        public EventVenueController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("eventvenue/all")]
        [Produces("application/json")]
        [SwaggerOperation(
            Summary = "Get Event Venues",
            Description = "Returns a paged list of Event Venues",
            OperationId = "GetEventVenues",
            Tags = new[] { "Event Venue" }
        )]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(IPagedData<EventVenue>))]
        [SwaggerResponse((int)HttpStatusCode.Unauthorized, "Api Key is not valid")]
        public async Task<ActionResult<GetEventVenues.PagedData>> GetAll([FromQuery] int skip = 0, [FromQuery] int take = 10, [FromQuery] int currentPage = 0, [FromQuery] string search = null)
        {
            return await _mediator.Send(new GetEventVenues.Query(skip, take, currentPage, search));
        }

        [HttpGet("eventvenue/{id}")]
        [Produces("application/json")]
        [SwaggerOperation(
            Summary = "Get Event Venue",
            Description = "Returns an Event Venue",
            OperationId = "GetEventVenue",
            Tags = new[] { "Event Venue" }
        )]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(EventVenue))]
        [SwaggerResponse((int)HttpStatusCode.Unauthorized, "Api Key is not valid")]
        public async Task<ActionResult<EventVenue>> Get(Guid id)
        {
            var results = await _mediator.Send(new GetEventVenue.Query(id));

            return results.Data;
        }

        [HttpGet("eventvenue/slug/{slug}")]
        [Produces("application/json")]
        [SwaggerOperation(
           Summary = "Get Event Venue By Slug",
           Description = "Returns an Event Venue",
           OperationId = "GetEventVenueBySlug",
           Tags = new[] { "Event Venue" }
       )]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(EventVenue))]
        [SwaggerResponse((int)HttpStatusCode.Unauthorized, "Api Key is not valid")]
        public async Task<ActionResult<EventVenue>> GetBySlug(string slug)
        {
            var results = await _mediator.Send(new GetEventVenueBySlug.Query(slug));

            return results.Data;
        }
    }
}