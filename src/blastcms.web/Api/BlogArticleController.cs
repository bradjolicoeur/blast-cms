﻿using blastcms.web.Attributes;
using blastcms.web.Data;
using blastcms.web.Handlers;
using blastcms.web.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Net;
using System.Threading.Tasks;
using Asp.Versioning;
using blastcms.web.Infrastructure;

namespace blastcms.web.Api
{
    [Route("api")]
    [ApiController]
    [ApiVersion("1.0")]
    [ApiKey]
    public class BlogArticleController : ControllerBase
    {

        private readonly IDispatcher _mediator;

        public BlogArticleController(IDispatcher dispatcher)
        {
            _mediator = dispatcher;
        }

        [HttpGet("blogarticle/all")]
        [Produces("application/json")]
        [SwaggerOperation(
            Summary = "Get Blog Articles",
            Description = "Returns a paged list of Blog Articles",
            OperationId = "GetBlogArticles",
            Tags = new[] { "Blog Article" }
        )]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(IPagedData<BlogArticle>))]
        [SwaggerResponse((int)HttpStatusCode.Unauthorized, "Api Key is not valid")]
        public async Task<ActionResult<GetBlogArticles.PagedData>> GetAll([FromQuery] int skip = 0, [FromQuery] int take = 10, [FromQuery] int currentPage=0, [FromQuery] string search=null, [FromQuery] string tag = null)
        {
            return await _mediator.Send(new GetBlogArticles.Query(skip, take, currentPage, search, tag));
        }

        [HttpGet("blogarticle/id/{id}")]
        [Produces("application/json")]
        [SwaggerOperation(
            Summary = "Get Blog Article",
            Description = "Returns a Blog Article",
            OperationId = "GetBlogArticle",
            Tags = new[] { "Blog Article" }
        )]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(BlogArticle))]
        [SwaggerResponse((int)HttpStatusCode.Unauthorized, "Api Key is not valid")]
        public async Task<ActionResult<BlogArticle>> GetById(Guid id)
        {
            var results = await _mediator.Send(new GetBlogArticle.Query(id));

            return results.Article;
        }


        [HttpGet("blogarticle/slug/{slug}")]
        [Produces("application/json")]
        [SwaggerOperation(
           Summary = "Get Blog Article",
           Description = "Returns a Blog Article",
           OperationId = "GetBlogArticleBySlug",
           Tags = new[] { "Blog Article" }
       )]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(BlogArticle))]
        [SwaggerResponse((int)HttpStatusCode.Unauthorized, "Api Key is not valid")]
        public async Task<ActionResult<BlogArticle>> GetBySlug(string slug)
        {
            var results = await _mediator.Send(new GetBlogArticleBySlug.Query(slug));

            return results.Article;
        }

        [ApiKeyFull]
        [HttpPost("blogarticle/")]
        [Produces("application/json")]
        [SwaggerOperation(
           Summary = "Alter Blog Article",
           Description = "Inserts or Updates a blog article",
           OperationId = "BlogArticleAlter",
           Tags = new[] { "Blog Article" }
       )]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(AlterBlogArticle.Model))]
        [SwaggerResponse((int)HttpStatusCode.Unauthorized, "Api Key is not valid")]
        public async Task<ActionResult<AlterBlogArticle.Model>> PostBlogArticle(AlterBlogArticle.Command article)
        {
            var result = await _mediator.Send(article);
            return result;
        }

    }
}
