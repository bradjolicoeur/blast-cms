using FakeItEasy;
using io.fusionauth.domain.api.user;
using io.fusionauth.domain;
using io.fusionauth;
using io.fusionauth.domain.api;

namespace blastcms.FusionAuthService.Tests
{
    public class UserManagementProviderTests
    {
        [Fact]
        public async Task GetAllUsers()
        {
            

            var searchResponse = new SearchResponse
            {
                users = new List<User>
                    { new User {id = Guid.Parse("bfcfa4a6-541a-4108-bd08-85b78ca18a5b"), active = true, fullName="test user", email="testuser@blastcms.net"} ,
                      new User {id = Guid.Parse("d883fd6e-99bf-4827-ba16-2ae3474fb319"), active = false, fullName="inactive user", email="inactiveuser@blastcms.net"},
                      new User {id = Guid.Parse("4836fcc0-388f-4c96-9941-cba42ee49c79"), active = null, fullName="nullactive user", email="nullactive@blastcms.net"}
                    }
            };
            var clientResponse = new ClientResponse<SearchResponse> { statusCode = 200, successResponse = searchResponse };

            var fusionAuthTenanted = A.Fake<IFusionAuthAsyncClient>();
            A.CallTo(() => fusionAuthTenanted.SearchUsersByQueryAsync(A<SearchRequest>._)).Returns(clientResponse);

            var fusionAuthFactory = A.Fake<IFusionAuthFactory>();
            A.CallTo(() => fusionAuthFactory.GetClientWithTenant(A<string>._)).Returns(fusionAuthTenanted);

            var tenant = A.Fake<IFusionAuthTenantProvider>();
            A.CallTo(() => tenant.GetTenantId()).Returns("faketenantid");

            var sut = new FusionAuthUserManagementProvider(fusionAuthFactory, tenant);

            var users = await sut.GetAllUsers(0, 10);

            await Verify(users);

        }
    }
}