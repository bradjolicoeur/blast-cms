using Amazon;
using Amazon.SimpleEmailV2;
using Amazon.SimpleEmailV2.Model;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace blastcms.web.Services
{
    public class SesEmailService : IEmailService
    {
        private readonly AmazonSimpleEmailServiceV2Client _client;

        public SesEmailService(IConfiguration configuration)
        {
            var accessKey = configuration["AWS:AccessKeyId"];
            var secretKey = configuration["AWS:SecretAccessKey"];
            var region = configuration["AWS:Region"] ?? "us-east-1";

            if (!string.IsNullOrEmpty(accessKey) && !string.IsNullOrEmpty(secretKey))
            {
                _client = new AmazonSimpleEmailServiceV2Client(
                    accessKey, secretKey, RegionEndpoint.GetBySystemName(region));
            }
            else
            {
                _client = new AmazonSimpleEmailServiceV2Client(
                    RegionEndpoint.GetBySystemName(region));
            }
        }

        public async Task SendEmailAsync(string from, string to, string subject, string body, CancellationToken cancellationToken = default)
        {
            var request = new SendEmailRequest
            {
                FromEmailAddress = from,
                Destination = new Destination
                {
                    ToAddresses = new List<string> { to }
                },
                Content = new EmailContent
                {
                    Simple = new Message
                    {
                        Subject = new Amazon.SimpleEmailV2.Model.Content { Data = subject },
                        Body = new Body
                        {
                            Html = new Amazon.SimpleEmailV2.Model.Content { Data = body },
                            Text = new Amazon.SimpleEmailV2.Model.Content { Data = body }
                        }
                    }
                }
            };

            await _client.SendEmailAsync(request, cancellationToken);
        }
    }
}
