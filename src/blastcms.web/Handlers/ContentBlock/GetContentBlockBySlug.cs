﻿using blastcms.web.Data;
using Marten;
using blastcms.web.Infrastructure;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace blastcms.web.Handlers
{
    public class GetContentBlockBySlug
    {
        public class Query : IRequest<Model>
        {
            public Query(string slug)
            {
                Slug = slug;
            }

            public string Slug { get; }
        }

        public class Model
        {
            public Model(ContentBlock data)
            {
                Data = data;
            }
            public ContentBlock Data { get; }
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
                    var data = await session.Query<ContentBlock>().FirstOrDefaultAsync(q => q.Slug.Equals(request.Slug,StringComparison.OrdinalIgnoreCase));

                    return new Model(data);
                }
            }

        }
    }
}
