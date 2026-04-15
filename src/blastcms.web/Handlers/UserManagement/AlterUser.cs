using blastcms.UserManagement;
using blastcms.UserManagement.Models;
using blastcms.web.Infrastructure;
using Riok.Mapperly.Abstractions;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;

namespace blastcms.web.Handlers
{
    public partial class AlterUser
    {
        public class Command : IRequest<Model>
        {
            public string Id { get; set; }

            [Required]
            public string FirstName { get; set; }

            [Required]
            public string LastName { get; set; }

            [Required]
            public string Email { get; set; }
            public bool? Active { get; set; }  = true;
        }

        public record Model(BlastUser User) { }

        [Mapper]
        public partial class SliceMapper
        {
            public partial BlastUser ToBlastUser(Command source);

            public partial Command ToCommand(BlastUser source);
        }

        public class Handler : IRequestHandler<Command, Model>
        {
            private static readonly SliceMapper Mapper = new();
            private readonly IUserManagementProvider _userManagement;

            public Handler(IUserManagementProvider userManagement)
            {
                _userManagement = userManagement;
            }
            public async Task<Model> Handle(Command request, CancellationToken cancellationToken)
            {

                var user = Mapper.ToBlastUser(request); 

                var result = await _userManagement.AlterUser(user);

                return new Model(result);
               
            }
        }
    }
}
