using System.Threading;
using System.Threading.Tasks;

namespace blastcms.web.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string from, string to, string subject, string body, CancellationToken cancellationToken = default);
    }
}
