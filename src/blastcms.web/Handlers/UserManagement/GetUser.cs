using blastcms.web.Infrastructure;
using System.Threading.Tasks;
using System.Threading;
using blastcms.UserManagement.Models;
using blastcms.UserManagement;

namespace blastcms.web.Handlers
{
    public class GetUser
    {
        public class Query : IRequest<Model>
        {
            public Query(string id)
            {
                Id = id;
            }

            public string Id { get; }
        }

        public record Model(BlastUser User)
        {
           
        }

        public class Handler : IRequestHandler<Query, Model>
        {
            private readonly IUserManagementProvider _userManagement;

            public Handler(IUserManagementProvider userManagement)
            {
                _userManagement = userManagement;
            }

            public async Task<Model> Handle(Query request, CancellationToken cancellationToken)
            {
                var result = await _userManagement.GetUser(request.Id);

                return new Model(result);
            }

        }
    }
}
