using blastcms.web.Data;
using Marten;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace blastcms.web.Handlers
{
    public class GetContentBlockByGroup
    {
        public class Query : IRequest<Model>
        {
            public Query(string group)
            {
                Group = group;
            }

            public string Group { get; }
        }

        public class Model
        {
            public Model(IEnumerable<ContentBlock> data)
            {
                Data = data;
            }
            public IEnumerable<ContentBlock> Data { get; }
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
                    var data = session.Query<ContentBlock>().Where(q => q.Groups.Contains(request.Group));

                    var content = await data.ToListAsync(cancellationToken);

                    return new Model(content);
                }
            }

        }
    }
}