using AutoMapper;
using blastcms.web.CloudStorage;
using blastcms.web.Data;
using Marten;
using MediatR;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace blastcms.web.Handlers
{
    public class UploadImageFile
    {
        public class Command : IRequest<Model>
        {
            public Guid? Id { get; set; }
            public string Title { get; set; }
            public HashSet<String> Tags { get; set; }
            public string Description { get; set; }         
            public virtual IBrowserFile ImageFile { get; set; }
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
                CreateMap<Command, ImageFile>().ReverseMap();
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
                var article = _mapper.Map<ImageFile>(request);
                string fileNameForStorage = FormFileName(request.Title, request.ImageFile.Name);

                article.ImageStorageName = fileNameForStorage;
                article.ImageUrl = await _cloudStorage.UploadFileAsync(request.ImageFile, fileNameForStorage); ;
                
                using var session = _sessionFactory.OpenSession();
                {
                    session.Store(article);

                    await session.SaveChangesAsync();

                    return new Model(article);
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
