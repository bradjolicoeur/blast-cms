﻿using AutoMapper;
using blastcms.UserManagement;
using blastcms.UserManagement.Models;
using blastcms.web.Infrastructure;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;

namespace blastcms.web.Handlers
{
    public class AlterUser
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

        public class AutoMapperProfile : Profile
        {
            public AutoMapperProfile()
            {
                CreateMap<Command, BlastUser>().ReverseMap();
            }
        }

        public class Handler : IRequestHandler<Command, Model>
        {
            private readonly IUserManagementProvider _userManagement;
            private readonly IMapper _mapper;

            public Handler(IUserManagementProvider userManagement, IMapper mapper)
            {
                _userManagement = userManagement;
                _mapper = mapper;
            }
            public async Task<Model> Handle(Command request, CancellationToken cancellationToken)
            {

                var user = _mapper.Map<BlastUser>(request); 

                var result = await _userManagement.AlterUser(user);

                return new Model(result);
               
            }
        }
    }
}
