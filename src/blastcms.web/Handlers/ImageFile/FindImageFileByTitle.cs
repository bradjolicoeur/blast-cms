using AutoMapper;
using blastcms.web.Data;
using Marten;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace blastcms.web.Handlers
{
    public class FindImageFileByTitle
    {
        public class Query : IRequest<Model>
        {
            public Query(string title)
            {
                Title = title;
            }

            public string Title { get; }
        }

        public class Model
        {
            public Model(ImageFile data)
            {
                Data = data;
            }
            public ImageFile Data { get; }
        }




        public class Handler : IRequestHandler<Query, Model>
        {
            private readonly ISessionFactory _sessionFactory;
            private readonly IMapper _mapper;

            public Handler(ISessionFactory sessionFactory, IMapper mapper)
            {
                _sessionFactory = sessionFactory;
                _mapper = mapper;
            }

            public async Task<Model> Handle(Query request, CancellationToken cancellationToken)
            {
                using var session = _sessionFactory.QuerySession();
                {
                    var data = await session.Query<ImageFile>().FirstOrDefaultAsync(q => q.Title.Equals(request.Title, System.StringComparison.OrdinalIgnoreCase));

                    return new Model(data);
                }
            }

        }
    }
}
