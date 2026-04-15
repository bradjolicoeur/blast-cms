using blastcms.web.CloudStorage;
using blastcms.web.Data;
using Marten;
using blastcms.web.Infrastructure;
using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using Riok.Mapperly.Abstractions;
using System.Threading;
using System.Threading.Tasks;

namespace blastcms.web.Handlers
{
    public partial class TransferImageWithUrl
    {
        public class Command : IRequest<Model>
        {
            public Guid? Id { get; set; }

            [Required]
            public string ImageUrl { get; set; }

            [Required]
            public string ImageStorageName { get; set; }

            [MaxLength(500)]
            public string Description { get; set; }

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

                var imageFile = Mapper.ToImageFile(request);
                string fileNameForStorage = FormFileName(request.Id.ToString(), request.ImageStorageName);

                imageFile.ImageStorageName = fileNameForStorage;
                imageFile.ImageUrl = await _cloudStorage.UploadFileAsync(request.ImageUrl, fileNameForStorage);
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
