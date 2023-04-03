using blastcms.web.Attributes;
using blastcms.web.Data;
using blastcms.web.Handlers;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;
using System.Threading.Tasks;
using System;

namespace blastcms.web.Api
{
    [Route("api")]
    [ApiController]
    [ApiVersion("1.0")]
    [ApiKey]
    public class ImageFileController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ImageFileController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("imagefile/all")]
        [Produces("application/json")]
        [SwaggerOperation(
            Summary = "Get Image Files",
            Description = "Returns a paged list of ImageFiles",
            OperationId = "GetImageFiles",
            Tags = new[] { "Image File" }
        )]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(IPagedData<ImageFile>))]
        [SwaggerResponse((int)HttpStatusCode.Unauthorized, "Api Key is not valid")]
        public async Task<ActionResult<GetImageFiles.PagedData>> GetAll([FromQuery] int skip = 0, [FromQuery] int take = 10, [FromQuery] int currentPage = 0, [FromQuery] string search = null, [FromQuery] string tag = null)
        {
            return await _mediator.Send(new GetImageFiles.Query(skip, take, currentPage, search, tag));
        }

        [HttpGet("imagefile/id/{id}")]
        [Produces("application/json")]
        [SwaggerOperation(
           Summary = "Get Image File",
           Description = "Returns a Image File",
           OperationId = "GetImageFile",
           Tags = new[] { "Image File" }
       )]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(ImageFile))]
        [SwaggerResponse((int)HttpStatusCode.Unauthorized, "Api Key is not valid")]
        public async Task<ActionResult<ImageFile>> GetById(Guid id)
        {
            var results = await _mediator.Send(new GetImageFile.Query(id));

            return results.Data;
        }

        [ApiKeyFull]
        [HttpPost("imageFile/")]
        [Produces("application/json")]
        [SwaggerOperation(
           Summary = "Alter Image File",
           Description = "Inserts or Updates an image file",
           OperationId = "ImageFileAlter",
           Tags = new[] { "Image File" }
       )]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(AlterImageFile.Model))]
        [SwaggerResponse((int)HttpStatusCode.Unauthorized, "Api Key is not valid")]
        public async Task<ActionResult<AlterImageFile.Model>> PostImageFile(AlterImageFile.Command data)
        {
            var result = await _mediator.Send(data);
            return result;
        }

        [ApiKeyFull]
        [RequestSizeLimit(5 * 1024 * 1024)]
        [HttpPost("imageFile/upload")]
        [Consumes("multipart/form-data")]
        [Produces("application/json")]
        [SwaggerOperation(
           Summary = "Upload Image File",
           Description = "Uploads an image file",
           OperationId = "ImageFileUpload",
           Tags = new[] { "Image File" }
       )]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(UploadImageWithForm.Model))]
        [SwaggerResponse((int)HttpStatusCode.Unauthorized, "Api Key is not valid")]
        public async Task<ActionResult<UploadImageWithForm.Model>> PostUploadImageFile(UploadImageWithForm.Command data)
        {
            var result = await _mediator.Send(data);
            return result;
        }

    }
}
