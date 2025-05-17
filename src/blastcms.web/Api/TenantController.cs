using Asp.Versioning;
using blastcms.web.Attributes;
using blastcms.web.Data;
using blastcms.web.Handlers;
using blastcms.web.Handlers.Tenant;
using blastcms.web.Infrastructure;
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
        private readonly IDispatcher _mediator;

        public TenantController(IDispatcher mediator)
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

        [HttpGet("tenant/{id}")]
        [Produces("application/json")]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(GetTenantExists.Model))]
        [SwaggerResponse((int)HttpStatusCode.Unauthorized, "Api Key is not valid")]
        public async Task<ActionResult<GetTenantExists.Model>> Get(string id)
        {
            var results = await _mediator.Send(new GetTenantExists.Query(id));

            return results;
        }
    }
}
