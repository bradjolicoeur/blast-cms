using Asp.Versioning;
using blastcms.web.Attributes;
using blastcms.web.Handlers;
using blastcms.web.Handlers.Tenant;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;
using System.Threading.Tasks;

namespace blastcms.web.Api
{
    [Route("api")]
    [ApiController]
    [ApiVersion("1.0")]
    [ApiKeyTenant]
    public class TenantController : ControllerBase
    {
        private readonly IMediator _mediator;

        public TenantController(IMediator mediator)
        {
            _mediator = mediator;
        }

        
        [HttpPut("tenant/")]
        [Produces("application/json")]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(PutEventItem.Model))]
        [SwaggerResponse((int)HttpStatusCode.Unauthorized, "Api Key is not valid")]
        public async Task<ActionResult<PutTenant.Model>> PostEventItem(PutTenant.Command tenant)
        {
            var result = await _mediator.Send(tenant);
            return result;
        }
    }
}
