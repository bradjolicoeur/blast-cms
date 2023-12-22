using blastcms.web.Data;
using Marten;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace blastcms.web.Handlers
{
    public class GetEventItem
    {
        public class Query : IRequest<Model>
        {
            public Query(Guid id)
            {
                Id = id;
            }

            public Guid Id { get; }
        }

        public record Model(EventItem Data, EventVenue Venue)
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
                {
                    EventVenue venue = null;
                    var item = await session.Query<EventItem>()
                        .Include<EventVenue>(x => x.VenueId, x => venue = x )
                        .FirstAsync(q => q.Id == request.Id, cancellationToken);

                    return new Model(item, venue);
                }
            }

        }
    }
}
