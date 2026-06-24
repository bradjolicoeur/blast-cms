using Asp.Versioning;
using blastcms.web.Attributes;
using blastcms.web.Handlers;
using blastcms.web.Infrastructure;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;
using System.Threading.Tasks;

namespace blastcms.web.Api
{
    [Route("api")]
    [ApiController]
    [ApiVersion("1.0")]
    public class CtaController : ControllerBase
    {
        private readonly IDispatcher _mediator;

        public CtaController(IDispatcher mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("cta/{slug}")]
        [ApiKey]
        [EnableCors("CtaPolicy")]
        [Produces("application/json")]
        [SwaggerOperation(
            Summary = "Submit CTA Signup",
            Description = "Submits an email signup for a call-to-action configuration. Sends confirmation emails to the administrator and submitter.",
            OperationId = "SubmitCtaSignup",
            Tags = new[] { "CTA" }
        )]
        [SwaggerResponse((int)HttpStatusCode.OK, "Signup submitted successfully")]
        [SwaggerResponse((int)HttpStatusCode.BadRequest, "Invalid request")]
        [SwaggerResponse((int)HttpStatusCode.Unauthorized, "Api Key is not valid")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "CTA configuration not found")]
        public async Task<ActionResult> Post(string slug, [FromBody] CtaSignupRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _mediator.Send(new SubmitCtaSignup.Command
            {
                Slug = slug,
                Email = request.Email,
                Name = request.Name
            });

            return Ok(new { success = result.Success });
        }
    }

    public class CtaSignupRequest
    {
        [System.ComponentModel.DataAnnotations.Required]
        [System.ComponentModel.DataAnnotations.EmailAddress]
        public string Email { get; set; }

        public string Name { get; set; }
    }
}
