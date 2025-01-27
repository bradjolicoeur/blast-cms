using blastcms.UserManagement.Models;

namespace blastcms.UserManagement
{
    public interface IUserManagementProvider
    {
        Task<UsersResponse> GetAllUsers(int skip, int take, string search);
        Task<BlastUser> GetUser(string id);
        Task<BlastUser> AlterUser(BlastUser user);
        Task DeactivateUser(string id);
        Task ReactivateUser(string id);
        Task DeleteUser(string id);
    }

    public record UsersResponse(IEnumerable<BlastUser> Users, long? Total) { }
}
