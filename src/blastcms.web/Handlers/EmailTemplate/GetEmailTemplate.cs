using AutoMapper;
using blastcms.web.Data;
using Marten;
using blastcms.web.Infrastructure;
using System.Threading.Tasks;
using System.Threading;
using System;

namespace blastcms.web.Handlers
{
    public class GetEmailTemplate
    {
        public class Query : IRequest<Model>
        {
            public Query(Guid id)
            {
                Id = id;
            }

            public Guid Id { get; }
        }

        public record Model(EmailTemplate Data)
        {
         
        }


        public class Handler : IRequestHandler<Query, Model>
        {
            private readonly ISessionFactory _sessionFactory;

            public Handler(ISessionFactory sessionFactory)
            {
                _sessionFactory = sessionFactory;
            }

            public async Task<Model> Handle(Query request, CancellationToken cancellationToken)
            {
                using var session = _sessionFactory.QuerySession();

                var data = await session.Query<EmailTemplate>().FirstAsync(q => q.Id == request.Id, token: cancellationToken);

                return new Model(data);

            }

        }
    }
}
