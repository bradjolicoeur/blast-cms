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
    public class EmailTemplateController : ControllerBase
    {

        private readonly IMediator _mediator;

        public EmailTemplateController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("emailtemplate/id/{id}")]
        [Produces("application/json")]
        [SwaggerOperation(
            Summary = "Get Email Template",
            Description = "Returns an Email Template",
            OperationId = "GetEmailTemplate",
            Tags = new[] { "Email Template" }
        )]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(EmailTemplate))]
        [SwaggerResponse((int)HttpStatusCode.Unauthorized, "Api Key is not valid")]
        public async Task<ActionResult<EmailTemplate>> GetById(Guid id)
        {
            var results = await _mediator.Send(new GetEmailTemplate.Query(id));

            return results.Data;
        }
    }
}
