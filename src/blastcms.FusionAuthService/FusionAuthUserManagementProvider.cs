using blastcms.FusionAuthService.Exceptions;
using blastcms.UserManagement;
using blastcms.UserManagement.Models;
using io.fusionauth;
using io.fusionauth.domain.api.user;
using io.fusionauth.domain.search;
using blastcms.FusionAuthService.Helpers;
using io.fusionauth.domain.api;
using io.fusionauth.domain.message;


namespace blastcms.FusionAuthService
{
    public class FusionAuthUserManagementProvider : IUserManagementProvider
    {
        private readonly IFusionAuthFactory _fusionAuthFactory;
        private readonly IFusionAuthTenantProvider _tenant;

        public FusionAuthUserManagementProvider(IFusionAuthFactory fusionAuthFactory, IFusionAuthTenantProvider tenant)
        {
            _fusionAuthFactory = fusionAuthFactory;
            _tenant = tenant;
        }

        public Task<BlastUser> AlterUser(BlastUser user)
        {
            if (user.Id == null)
            {
                return CreateUser(user);
            } else
            {
                return PutUser(user);
            }
        }
        private async Task<BlastUser> PutUser(BlastUser user)
        {
            var client = IniatializeClient();

            Dictionary<string, object> request = new Dictionary<string, object>();
            Dictionary<string, object> userRequest = new Dictionary<string, object>();
            userRequest.Add("fullName", user.FullName);
            userRequest.Add("email", user.Email);
            userRequest.Add("active", user.Active ?? false);
            request.Add("user", userRequest);

            var result = await client.PatchUserAsync(Guid.Parse(user.Id), request);

            if (result == null || !result.WasSuccessful())
                throw new FusionAuthException($"User not Altered {user.Id}. {result.errorResponse?.FusionAuthErrorMessage()}");

            var resultUser = result.successResponse.user;

            var newUser = new BlastUser
            {
                Id = resultUser.id.ToString(),
                FullName = resultUser.fullName,
                Email = resultUser.email,
                Active = resultUser.active,
            };

            return newUser;
        }
        private async Task<BlastUser> CreateUser(BlastUser user)
        {
            var client = IniatializeClient();
            var request = new UserRequest
            {
                applicationId = Guid.Parse(_tenant.GetApplicationId()),
                user = new io.fusionauth.domain.User { email = user.Email, fullName = user.FullName, password = Guid.NewGuid().ToString() },
                sendSetPasswordEmail = true,

            };

            var result = await client.CreateUserAsync(null, request);

            if (result == null || !result.WasSuccessful())
                throw new FusionAuthException($"User not Created {user.Email}. {result.errorResponse?.FusionAuthErrorMessage()}");

            var resultUser = result.successResponse.user;

            var newUser = new BlastUser
            {
                Id = resultUser.id.ToString(),
                FullName = resultUser.fullName,
                Email = resultUser.email,
                Active = resultUser.active,
            };

            return newUser;
        }

        public async Task DeactivateUser(string id)
        {
            var client = IniatializeClient();
            var result = await client.DeactivateUserAsync(Guid.Parse(id));

            if (result == null || !result.WasSuccessful())
                throw new FusionAuthException($"User not Deactivated {id}. {result.errorResponse?.FusionAuthErrorMessage()}");
        }

        public async Task DeleteUser(string id)
        {
            var client = IniatializeClient();
            var result = await client.DeleteUserAsync(Guid.Parse(id));

            if (result == null || !result.WasSuccessful())
                throw new FusionAuthException($"User not Deleted {id}. {result.errorResponse?.FusionAuthErrorMessage()}");
        }

        public async Task<UsersResponse> GetAllUsers(int skip, int take, string search)
        {
            var users = new List<BlastUser>();

            var request = new SearchRequest { 
                    search = new UserSearchCriteria 
                    { 
                        queryString = search??"*", 
                        numberOfResults = 25, 
                        startRow = 0,
                        sortFields = new List<SortField> { new SortField { name = "fullName", order = Sort.asc } } 
                    } 
            };
            var client = IniatializeClient();
            var results = await client.SearchUsersByQueryAsync(request);

            if (results == null || !results.WasSuccessful())
                throw new FusionAuthException($"Error Retrieving All Users. {results.errorResponse?.FusionAuthErrorMessage()}");

            foreach (var item in results.successResponse.users)
            {
                users.Add(new BlastUser
                {
                    Id = item.id.Value.ToString(),
                    FullName = item.fullName?? item.firstName + " " + item.lastName ,
                    Email = item.email,
                    Active = item.active??false,
                });
            }

            return new UsersResponse(users, results.successResponse.total);
        }

        private IFusionAuthAsyncClient IniatializeClient()
        {
            return _fusionAuthFactory.GetClientWithTenant(_tenant.GetTenantId());
        }

        public async Task<BlastUser> GetUser(string id)
        {
            var client = IniatializeClient();
            var result = await client.RetrieveUserAsync(Guid.Parse(id));

            if (result == null || !result.WasSuccessful())
                throw new FusionAuthException($"User not retrieved {id}. {result.errorResponse?.FusionAuthErrorMessage()}");

            return new BlastUser
            {
                Id = result.successResponse.user.id.Value.ToString(),
                FullName = result.successResponse.user.fullName,
                Email = result.successResponse.user.email,
                Active = result.successResponse.user.active,
            };
        }
    }
}
