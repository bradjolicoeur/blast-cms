using blastcms.web.CloudStorage;
using blastcms.web.Data;
using Marten;
using blastcms.web.Infrastructure;
using Microsoft.AspNetCore.Components.Forms;
using Riok.Mapperly.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace blastcms.web.Handlers
{
    public partial class UploadImageFile
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


        [Mapper]
        public partial class SliceMapper
        {
            [MapperIgnoreTarget(nameof(ImageFile.ImageUrl))]
            public partial ImageFile ToImageFile(Command source);
        }

        public class Handler : IRequestHandler<Command, Model>
        {
            private static readonly SliceMapper Mapper = new();
            private readonly ISessionFactory _sessionFactory;
            private readonly ICloudStorage _cloudStorage;

            public Handler(ISessionFactory sessionFactory, ICloudStorage cloudStorage)
            {
                _sessionFactory = sessionFactory;
                _cloudStorage = cloudStorage;
            }

            public async Task<Model> Handle(Command request, CancellationToken cancellationToken)
            {
                var article = Mapper.ToImageFile(request);
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
