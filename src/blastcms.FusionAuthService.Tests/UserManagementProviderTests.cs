using FakeItEasy;
using io.fusionauth.domain.api.user;
using io.fusionauth.domain;
using io.fusionauth;
using io.fusionauth.domain.api;
using blastcms.FusionAuthService.Exceptions;

namespace blastcms.FusionAuthService.Tests
{
    public class UserManagementProviderTests
    {
        [Fact]
        public async Task GetAllUsers_Success()
        {

            var searchResponse = new SearchResponse
            {
                users = new List<User>
                    { new User {id = Guid.Parse("bfcfa4a6-541a-4108-bd08-85b78ca18a5b"), active = true, firstName="test", lastName="user", fullName="test user", email="testuser@blastcms.net"} ,
                      new User {id = Guid.Parse("d883fd6e-99bf-4827-ba16-2ae3474fb319"), active = false, firstName = "inactive", lastName="user", fullName="inactive user", email="inactiveuser@blastcms.net"},
                      new User {id = Guid.Parse("4836fcc0-388f-4c96-9941-cba42ee49c79"), active = null, firstName="nullactive", lastName="user", fullName="nullactive user", email="nullactive@blastcms.net"}
                    }, 
                total = 3,
            };
            var clientResponse = new ClientResponse<SearchResponse> { statusCode = 200, successResponse = searchResponse};

            var fusionAuthTenanted = A.Fake<IFusionAuthAsyncClient>();
            A.CallTo(() => fusionAuthTenanted.SearchUsersByQueryAsync(A<SearchRequest>._)).Returns(clientResponse);

            var fusionAuthFactory = A.Fake<IFusionAuthFactory>();
            A.CallTo(() => fusionAuthFactory.GetClientWithTenant(A<string>._)).Returns(fusionAuthTenanted);

            var tenant = A.Fake<IFusionAuthTenantProvider>();
            A.CallTo(() => tenant.GetTenantId()).Returns("faketenantid");

            string search = null;

            var sut = new FusionAuthUserManagementProvider(fusionAuthFactory, tenant);

            var users = await sut.GetAllUsers(0, 10, search);

            await Verify(users);

        }

        [Fact]
        public async Task GetAllUsers_Failed()
        {
            var clientResponse = new ClientResponse<SearchResponse> { statusCode = 400, successResponse = null, exception = new Exception("test message") };

            var fusionAuthTenanted = A.Fake<IFusionAuthAsyncClient>();
            A.CallTo(() => fusionAuthTenanted.SearchUsersByQueryAsync(A<SearchRequest>._)).Returns(clientResponse);

            var fusionAuthFactory = A.Fake<IFusionAuthFactory>();
            A.CallTo(() => fusionAuthFactory.GetClientWithTenant(A<string>._)).Returns(fusionAuthTenanted);

            var tenant = A.Fake<IFusionAuthTenantProvider>();
            A.CallTo(() => tenant.GetTenantId()).Returns("faketenantid");

            string search = null;

            var sut = new FusionAuthUserManagementProvider(fusionAuthFactory, tenant);

            var caughtException = await Assert.ThrowsAsync<FusionAuthException>(() => sut.GetAllUsers(0,10, search));
            await Verify(caughtException.Message);
        }

        [Fact]
        public async Task AlterUser_Success()
        {
            var user = new UserManagement.Models.BlastUser
            {
                Id = "f301c22d-3484-46c8-b392-868944474f88",
                Active = true,
                FirstName = "Tester",
                LastName = "Mouse",
                Email = "testermouse@blastcms.net",
            };

            var userResponse = new UserResponse { user = new User { id = Guid.Parse(user.Id), active = true, firstName = user.FirstName, lastName = user.LastName, email = user.Email } };
            var clientResponse = new ClientResponse<UserResponse> { statusCode = 200, successResponse = userResponse };

            var fusionAuthTenanted = A.Fake<IFusionAuthAsyncClient>();
            A.CallTo(() => fusionAuthTenanted.PatchUserAsync(A<Guid>._, A<Dictionary<string, object>>._)).Returns(clientResponse);

            var fusionAuthFactory = A.Fake<IFusionAuthFactory>();
            A.CallTo(() => fusionAuthFactory.GetClientWithTenant(A<string>._)).Returns(fusionAuthTenanted);

            var tenant = A.Fake<IFusionAuthTenantProvider>();
            A.CallTo(() => tenant.GetTenantId()).Returns("faketenantid");
            A.CallTo(() => tenant.GetApplicationId()).Returns("12c3b12a-42d0-494a-be20-afd336331852");

            var sut = new FusionAuthUserManagementProvider(fusionAuthFactory, tenant);

            var users = await sut.AlterUser(user);

            await Verify(users);
        }

        [Fact]
        public async Task AlterUser_Failed()
        {
            var user = new UserManagement.Models.BlastUser
            {
                Id = "f301c22d-3484-46c8-b392-868944474f88",
                Active = true,
                FirstName = "Tester",
                LastName = "Mouse",
                Email = "testermouse@blastcms.net",
            };

            var clientResponse = new ClientResponse<UserResponse> { statusCode = 400, successResponse = null };

            var fusionAuthTenanted = A.Fake<IFusionAuthAsyncClient>();
            A.CallTo(() => fusionAuthTenanted.PatchUserAsync(A<Guid>._, A<Dictionary<string, object>>._)).Returns(clientResponse);

            var fusionAuthFactory = A.Fake<IFusionAuthFactory>();
            A.CallTo(() => fusionAuthFactory.GetClientWithTenant(A<string>._)).Returns(fusionAuthTenanted);

            var tenant = A.Fake<IFusionAuthTenantProvider>();
            A.CallTo(() => tenant.GetTenantId()).Returns("faketenantid");
            A.CallTo(() => tenant.GetApplicationId()).Returns("12c3b12a-42d0-494a-be20-afd336331852");

            var sut = new FusionAuthUserManagementProvider(fusionAuthFactory, tenant);

            var caughtException = await Assert.ThrowsAsync<FusionAuthException>(() => sut.AlterUser(user));
            await Verify(caughtException.Message);
        }

        [Fact]
        public async Task CreateUser_Success()
        {
            var user = new UserManagement.Models.BlastUser
            {
                Active = true,
                FirstName = "Tester",
                LastName = "Mouse",
                Email = "testermouse@blastcms.net",
            };

            var resultId = Guid.Parse("f301c22d-3484-46c8-b392-868944474f88");

            var userResponse = new UserResponse { user = new User { id = resultId, active = true, firstName = user.FirstName, lastName = user.LastName, email = user.Email } };
            var clientResponse = new ClientResponse<UserResponse> { statusCode = 200, successResponse = userResponse };

            var fusionAuthTenanted = A.Fake<IFusionAuthAsyncClient>();
            A.CallTo(() => fusionAuthTenanted.CreateUserAsync(A<Guid?>._, A<UserRequest>._)).Returns(clientResponse);

            var fusionAuthFactory = A.Fake<IFusionAuthFactory>();
            A.CallTo(() => fusionAuthFactory.GetClientWithTenant(A<string>._)).Returns(fusionAuthTenanted);

            var tenant = A.Fake<IFusionAuthTenantProvider>();
            A.CallTo(() => tenant.GetTenantId()).Returns("faketenantid");
            A.CallTo(() => tenant.GetApplicationId()).Returns("12c3b12a-42d0-494a-be20-afd336331852");

            var sut = new FusionAuthUserManagementProvider(fusionAuthFactory, tenant);

            var users = await sut.AlterUser(user);

            await Verify(users);
        }

        [Fact]
        public async Task CreateUser_Failed()
        {
            var user = new UserManagement.Models.BlastUser
            {
                Active = true,
                FirstName = "Tester",
                LastName = "Mouse",
                Email = "testermouse@blastcms.net",
            };

            var clientResponse = new ClientResponse<UserResponse> { statusCode = 400, successResponse = null };

            var fusionAuthTenanted = A.Fake<IFusionAuthAsyncClient>();
            A.CallTo(() => fusionAuthTenanted.CreateUserAsync(A<Guid?>._, A<UserRequest>._)).Returns(clientResponse);

            var fusionAuthFactory = A.Fake<IFusionAuthFactory>();
            A.CallTo(() => fusionAuthFactory.GetClientWithTenant(A<string>._)).Returns(fusionAuthTenanted);

            var tenant = A.Fake<IFusionAuthTenantProvider>();
            A.CallTo(() => tenant.GetTenantId()).Returns("faketenantid");
            A.CallTo(() => tenant.GetApplicationId()).Returns("12c3b12a-42d0-494a-be20-afd336331852");

            var sut = new FusionAuthUserManagementProvider(fusionAuthFactory, tenant);

            var caughtException = await Assert.ThrowsAsync<FusionAuthException>(() => sut.AlterUser(user));
            await Verify(caughtException.Message);
        }

        [Fact]
        public async Task DeactivateUser_Success()
        {
            var userid = "f301c22d-3484-46c8-b392-868944474f88";

            var restVoid = new RESTVoid();
            var clientResponse = new ClientResponse<RESTVoid> { statusCode = 200, successResponse = restVoid };

            var fusionAuthTenanted = A.Fake<IFusionAuthAsyncClient>();
            A.CallTo(() => fusionAuthTenanted.DeactivateUserAsync(A<Guid>._)).Returns(clientResponse);

            var fusionAuthFactory = A.Fake<IFusionAuthFactory>();
            A.CallTo(() => fusionAuthFactory.GetClientWithTenant(A<string>._)).Returns(fusionAuthTenanted);

            var tenant = A.Fake<IFusionAuthTenantProvider>();
            A.CallTo(() => tenant.GetTenantId()).Returns("faketenantid");

            var sut = new FusionAuthUserManagementProvider(fusionAuthFactory, tenant);

            await sut.DeactivateUser(userid);

        }

        [Fact]
        public async Task DeactivateUser_Failed()
        {
            var userid = "f301c22d-3484-46c8-b392-868944474f88";

            var restVoid = new RESTVoid();
            var clientResponse = new ClientResponse<RESTVoid> { statusCode = 400, successResponse = restVoid };

            var fusionAuthTenanted = A.Fake<IFusionAuthAsyncClient>();
            A.CallTo(() => fusionAuthTenanted.DeactivateUserAsync(A<Guid>._)).Returns(clientResponse);

            var fusionAuthFactory = A.Fake<IFusionAuthFactory>();
            A.CallTo(() => fusionAuthFactory.GetClientWithTenant(A<string>._)).Returns(fusionAuthTenanted);

            var tenant = A.Fake<IFusionAuthTenantProvider>();
            A.CallTo(() => tenant.GetTenantId()).Returns("faketenantid");

            var sut = new FusionAuthUserManagementProvider(fusionAuthFactory, tenant);

            var caughtException = await Assert.ThrowsAsync<FusionAuthException>(() => sut.DeactivateUser(userid));
            await Verify(caughtException.Message);
        }

        [Fact]
        public async Task ReactivateUser_Success()
        {
            var userid = "f301c22d-3484-46c8-b392-868944474f88";

            var user = new UserManagement.Models.BlastUser
            {
                Active = true,
                FirstName = "Tester",
                LastName = "Mouse",
                Email = "testermouse@blastcms.net",
            };

            var resultId = Guid.Parse("f301c22d-3484-46c8-b392-868944474f88");

            var userResponse = new UserResponse { user = new User { id = resultId, active = true, firstName = user.FirstName, lastName = user.LastName, email = user.Email } };

            var clientResponse = new ClientResponse<UserResponse> { statusCode = 200, successResponse = userResponse };

            var fusionAuthTenanted = A.Fake<IFusionAuthAsyncClient>();
            A.CallTo(() => fusionAuthTenanted.ReactivateUserAsync(A<Guid>._)).Returns(clientResponse);

            var fusionAuthFactory = A.Fake<IFusionAuthFactory>();
            A.CallTo(() => fusionAuthFactory.GetClientWithTenant(A<string>._)).Returns(fusionAuthTenanted);

            var tenant = A.Fake<IFusionAuthTenantProvider>();
            A.CallTo(() => tenant.GetTenantId()).Returns("faketenantid");

            var sut = new FusionAuthUserManagementProvider(fusionAuthFactory, tenant);

            await sut.ReactivateUser(userid);
        }

        [Fact]
        public async Task ReactivateUser_Failed()
        {
            var userid = "f301c22d-3484-46c8-b392-868944474f88";

            var restVoid = new RESTVoid();
            var clientResponse = new ClientResponse<RESTVoid> { statusCode = 400, successResponse = restVoid };

            var fusionAuthTenanted = A.Fake<IFusionAuthAsyncClient>();
            A.CallTo(() => fusionAuthTenanted.DeactivateUserAsync(A<Guid>._)).Returns(clientResponse);

            var fusionAuthFactory = A.Fake<IFusionAuthFactory>();
            A.CallTo(() => fusionAuthFactory.GetClientWithTenant(A<string>._)).Returns(fusionAuthTenanted);

            var tenant = A.Fake<IFusionAuthTenantProvider>();
            A.CallTo(() => tenant.GetTenantId()).Returns("faketenantid");

            var sut = new FusionAuthUserManagementProvider(fusionAuthFactory, tenant);

            var caughtException = await Assert.ThrowsAsync<FusionAuthException>(() => sut.ReactivateUser(userid));
            await Verify(caughtException.Message);
        }

        [Fact]
        public async Task DeleteUser_Success()
        {
            var userid = "f301c22d-3484-46c8-b392-868944474f88";

            var restVoid = new RESTVoid();
            var clientResponse = new ClientResponse<RESTVoid> { statusCode = 200, successResponse = restVoid };

            var fusionAuthTenanted = A.Fake<IFusionAuthAsyncClient>();
            A.CallTo(() => fusionAuthTenanted.DeleteUserAsync(A<Guid>._)).Returns(clientResponse);

            var fusionAuthFactory = A.Fake<IFusionAuthFactory>();
            A.CallTo(() => fusionAuthFactory.GetClientWithTenant(A<string>._)).Returns(fusionAuthTenanted);

            var tenant = A.Fake<IFusionAuthTenantProvider>();
            A.CallTo(() => tenant.GetTenantId()).Returns("faketenantid");

            var sut = new FusionAuthUserManagementProvider(fusionAuthFactory, tenant);

            await sut.DeleteUser(userid);
        }

        [Fact]
        public async Task DeleteUser_Failed()
        {
            var userid = "f301c22d-3484-46c8-b392-868944474f88";

            var restVoid = new RESTVoid();
            var clientResponse = new ClientResponse<RESTVoid> { statusCode = 400, successResponse = restVoid };

            var fusionAuthTenanted = A.Fake<IFusionAuthAsyncClient>();
            A.CallTo(() => fusionAuthTenanted.DeleteUserAsync(A<Guid>._)).Returns(clientResponse);

            var fusionAuthFactory = A.Fake<IFusionAuthFactory>();
            A.CallTo(() => fusionAuthFactory.GetClientWithTenant(A<string>._)).Returns(fusionAuthTenanted);

            var tenant = A.Fake<IFusionAuthTenantProvider>();
            A.CallTo(() => tenant.GetTenantId()).Returns("faketenantid");

            var sut = new FusionAuthUserManagementProvider(fusionAuthFactory, tenant);

            var caughtException = await Assert.ThrowsAsync<FusionAuthException>(() => sut.DeleteUser(userid));
            await Verify(caughtException.Message);
        }
    }
}