using blastcms.web.CloudStorage;
using blastcms.web.Data;
using Marten;
using blastcms.web.Infrastructure;
using Microsoft.AspNetCore.Components.Forms;
using Riok.Mapperly.Abstractions;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace blastcms.web.Handlers
{
    public partial class UploadImageFileSimple
    {
        public class Command : IRequest<Model>
        {
            public Guid? Id { get; set; }     
            public virtual IBrowserFile BrowserFile { get; set; }
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
            [MapperIgnoreTarget(nameof(ImageFile.Title))]
            [MapperIgnoreTarget(nameof(ImageFile.Tags))]
            [MapperIgnoreTarget(nameof(ImageFile.Description))]
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
                request.Id = Guid.NewGuid();
 
                var article = Mapper.ToImageFile(request);
                string fileNameForStorage = FormFileName(request.Id.ToString(), request.BrowserFile.Name);
                
                article.ImageStorageName = fileNameForStorage;
                article.ImageUrl = await _cloudStorage.UploadFileAsync(request.BrowserFile, fileNameForStorage);
                article.Title = request.BrowserFile.Name;

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
