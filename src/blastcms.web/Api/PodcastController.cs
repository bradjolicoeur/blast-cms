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
    public class PodcastController : ControllerBase
    {

        private readonly IMediator _mediator;

        public PodcastController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("podcastepisode/{podcastid}/all")]
        [Produces("application/json")]
        [SwaggerOperation(
            Summary = "Get Podcast Episodes",
            Description = "Returns a paged list of Podcast Episodes for a podcast",
            OperationId = "GetPodcastEpisodes",
            Tags = new[] { "Podcast Episode" }
        )]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(IPagedData<PodcastEpisode>))]
        [SwaggerResponse((int)HttpStatusCode.Unauthorized, "Api Key is not valid")]
        public async Task<ActionResult<GetPodcastEpisodes.PagedData>> GetPodcastEpisodeAll(Guid podcastid ,[FromQuery] int skip = 0, [FromQuery] int take = 10, [FromQuery] int currentPage = 0, [FromQuery] string search = null, [FromQuery] string tag = null)
        {
            return await _mediator.Send(new GetPodcastEpisodes.Query(skip, take, currentPage, search, tag, podcastid.ToString()));
        }

        [HttpGet("podcastepisode/id/{episodeid}")]
        [Produces("application/json")]
        [SwaggerOperation(
            Summary = "Get Podcast Episode",
            Description = "Returns a podcast episode",
            OperationId = "GetPodcastEpisode",
            Tags = new[] { "Podcast Episode" }
        )]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(PodcastEpisode))]
        [SwaggerResponse((int)HttpStatusCode.Unauthorized, "Api Key is not valid")]
        public async Task<ActionResult<PodcastEpisode>> GetPodcastEpisode(Guid episodeId)
        {
            var result = await _mediator.Send(new GetPodcastEpisode.Query(episodeId));
            return result.Episode;
        }

        [ApiKeyFull]
        [HttpPost("podcastepisode/")]
        [Produces("application/json")]
        [SwaggerOperation(
            Summary = "Alter Podcast Episode",
            Description = "Inserts or Updates a Podcast Episode",
            OperationId = "PostPodcastEpisode",
            Tags = new[] { "Podcast Episode" }
        )]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(AlterPodcastEpisode.Model))]
        [SwaggerResponse((int)HttpStatusCode.Unauthorized, "Api Key is not valid")]
        public async Task<ActionResult<AlterPodcastEpisode.Model>> PostPodcastEpisode(AlterPodcastEpisode.Command episode)
        {
            var result = await _mediator.Send(episode);
            return result;
        }

        [HttpGet("podcastepisode/slug/{slug}")]
        [Produces("application/json")]
        [SwaggerOperation(
            Summary = "Get Podcast Episode By Slug",
            Description = "Returns a podcast episode",
            OperationId = "GetPodcastEpisodeBySlug",
            Tags = new[] { "Podcast Episode" }
        )]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(PodcastEpisode))]
        [SwaggerResponse((int)HttpStatusCode.Unauthorized, "Api Key is not valid")]
        public async Task<ActionResult<PodcastEpisode>> GetPodcastEpisodeBySlug(string slug)
        {
            var result = await _mediator.Send(new GetPodcastEpisodeBySlug.Query(slug));
            return result.Episode;
        }

        [HttpGet("podcast/id/{podcastid}")]
        [Produces("application/json")]
        [SwaggerOperation(
            Summary = "Get Podcast",
            Description = "Returns a podcast",
            OperationId = "GetPodcast",
            Tags = new[] { "Podcast" }
        )]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(Podcast))]
        [SwaggerResponse((int)HttpStatusCode.Unauthorized, "Api Key is not valid")]
        public async Task<ActionResult<Podcast>> GetPodcast(Guid podcastid)
        {
            var result = await _mediator.Send(new GetPodcast.Query(podcastid));
            return result.Podcast;
        }

        [HttpGet("podcast/slug/{slug}")]
        [Produces("application/json")]
        [SwaggerOperation(
            Summary = "Get Podcast By Slug",
            Description = "Returns a podcast",
            OperationId = "GetPodcastBySlug",
            Tags = new[] { "Podcast" }
        )]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(Podcast))]
        [SwaggerResponse((int)HttpStatusCode.Unauthorized, "Api Key is not valid")]
        public async Task<ActionResult<Podcast>> GetPodcastBySlug(string slug)
        {
            var result = await _mediator.Send(new GetPodcastBySlug.Query(slug));
            return result.Podcast;
        }

        [ApiKeyFull]
        [HttpPost("podcast/")]
        [Produces("application/json")]
        [SwaggerOperation(
           Summary = "Alter Podcast",
           Description = "Inserts or Updates a Podcast Episode",
           OperationId = "PostPodcastEpisode",
           Tags = new[] { "Podcast" }
       )]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(AlterPodcast.Model))]
        [SwaggerResponse((int)HttpStatusCode.Unauthorized, "Api Key is not valid")]
        public async Task<ActionResult<AlterPodcast.Model>> PostPodcastEpisode(AlterPodcast.Command episode)
        {
            var result = await _mediator.Send(episode);
            return result;
        }

        [HttpGet("podcast/all")]
        [Produces("application/json")]
        [SwaggerOperation(
            Summary = "Get Podcasts",
            Description = "Returns a paged list of Podcasts",
            OperationId = "GetPodcasts",
            Tags = new[] { "Podcast" }
        )]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(IPagedData<Podcast>))]
        [SwaggerResponse((int)HttpStatusCode.Unauthorized, "Api Key is not valid")]
        public async Task<ActionResult<GetPodcasts.PagedData>> GetPodcastAll([FromQuery] int skip = 0, [FromQuery] int take = 10, [FromQuery] int currentPage = 0, [FromQuery] string search = null, [FromQuery] string tag = null)
        {
            return await _mediator.Send(new GetPodcasts.Query(skip, take, currentPage, search, tag));
        }
    }
}
