using blastcms.UserManagement.Models;

namespace blastcms.UserManagement
{
    public interface IUserManagementProvider
    {
        Task<IEnumerable<BlastUser>> GetAllUsers(int skip, int take);
        Task<BlastUser> GetUser(string id);
        Task<BlastUser> AlterUser(BlastUser user);
        Task DeactivateUser(string id);
        Task DeleteUser(string id);
    }
}
