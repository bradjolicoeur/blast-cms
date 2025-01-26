using MediatR;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using System.Threading;
using blastcms.UserManagement;

namespace blastcms.web.Handlers
{
    public class DeleteUser
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
            private readonly IUserManagementProvider _userManagement;

            public Handler(IUserManagementProvider userManagement)
            {
                _userManagement = userManagement;
            }

            public async Task<Model> Handle(Command request, CancellationToken cancellationToken)
            {

                await _userManagement.DeleteUser(request.Id);

                return new Model(true);

            }

        }
    }
}
