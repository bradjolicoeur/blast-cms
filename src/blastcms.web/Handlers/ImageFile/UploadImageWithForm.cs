using AutoMapper;
using blastcms.web.CloudStorage;
using blastcms.web.Data;
using Marten;
using MediatR;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Http;
using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace blastcms.web.Handlers
{
    public class UploadImageWithForm
    {
        public class Command : IRequest<Model>
        {
            public Guid? Id { get; set; }

            [Required]
            public IFormFile Image { get; set; }

            [Required]
            public string ImageStorageName { get; set; }

        }

        public class Model
        {
            public Model(ImageFile data)
            {
                Data = data;
            }

            public ImageFile Data { get; }
        }


        public class AutoMapperProfile : Profile
        {
            public AutoMapperProfile()
            {
                CreateMap<Command, ImageFile>()
                    .ReverseMap();
            }
        }

        public class Handler : IRequestHandler<Command, Model>
        {
            private readonly ISessionFactory _sessionFactory;
            private readonly IMapper _mapper;
            private readonly ICloudStorage _cloudStorage;

            public Handler(ISessionFactory sessionFactory, IMapper mapper, ICloudStorage cloudStorage)
            {
                _sessionFactory = sessionFactory;
                _mapper = mapper;
                _cloudStorage = cloudStorage;
            }

            public async Task<Model> Handle(Command request, CancellationToken cancellationToken)
            {
                request.Id = Guid.NewGuid();

                var imageFile = _mapper.Map<ImageFile>(request);
                string fileNameForStorage = FormFileName(request.Id.ToString(), request.Image.FileName);

                imageFile.ImageStorageName = fileNameForStorage;
                imageFile.ImageUrl = await _cloudStorage.UploadFileAsync(request.Image, fileNameForStorage);
                imageFile.Title = request.ImageStorageName;

                using var session = _sessionFactory.OpenSession();
                {
                    session.Store(imageFile);

                    await session.SaveChangesAsync();

                    return new Model(imageFile);
                }
            }

            private static string FormFileName(string title, string fileName)
            {
                var fileExtension = Path.GetExtension(fileName);
                var fileNameForStorage = $"{title}-{DateTime.Now:yyyyMMddHHmmss}{fileExtension}";
                return fileNameForStorage;
            }

        }
    }
}
