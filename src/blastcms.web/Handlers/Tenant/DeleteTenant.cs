using blastcms.web.Data;
using Marten;
using MediatR;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using System.Threading;

namespace blastcms.web.Handlers
{
    public class DeleteTenant
    {
        public class Command : IRequest<Model>
        {
            [Required]
            public string Id { get; set; }

        }

        public readonly record struct Model(bool Success)
        {
        }


        public class Handler : IRequestHandler<Command, Model>
        {
            private readonly IDocumentStore _documentStore;

            public Handler(IDocumentStore documentStore)
            {
                _documentStore = documentStore;
            }

            public async Task<Model> Handle(Command request, CancellationToken cancellationToken)
            {

                using var session = _documentStore.OpenSession();

                session.Delete<BlastTenant>(request.Id);
                await session.SaveChangesAsync(cancellationToken);

                return new Model(true);

            }

        }
    }
}
