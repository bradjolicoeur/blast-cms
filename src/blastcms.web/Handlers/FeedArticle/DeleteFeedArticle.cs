﻿namespace blastcms.web.Handlers
{

    using blastcms.web.Data;
    using Marten;
    using blastcms.web.Infrastructure;
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.Threading;
    using System.Threading.Tasks;
    public class DeleteFeedArticle
    {
        public class Command : IRequest<Model>
        {
            [Required]
            public Guid Id { get; set; }

        }

        public readonly record struct Model(bool Success)
        {
        }


        public class Handler : IRequestHandler<Command, Model>
        {
            private readonly ISessionFactory _sessionFactory;

            public Handler(ISessionFactory sessionFactory)
            {
                _sessionFactory = sessionFactory;
            }

            public async Task<Model> Handle(Command request, CancellationToken cancellationToken)
            {

                using var session = _sessionFactory.OpenSession();

                session.Delete<FeedArticle>(request.Id);
                await session.SaveChangesAsync(cancellationToken);

                return new Model(true);

            }

        }
    }
}
