using blastcms.web.Data;
using blastcms.web.Infrastructure;
using blastcms.web.Services;
using Marten;
using System;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace blastcms.web.Handlers
{
    public class SubmitCtaSignup
    {
        public class Command : IRequest<Model>
        {
            [Required]
            public string Slug { get; set; }

            [Required]
            [EmailAddress]
            public string Email { get; set; }

            public string Name { get; set; }
        }

        public readonly record struct Model(bool Success);

        public class Handler : IRequestHandler<Command, Model>
        {
            private readonly ISessionFactory _sessionFactory;
            private readonly IEmailService _emailService;

            public Handler(ISessionFactory sessionFactory, IEmailService emailService)
            {
                _sessionFactory = sessionFactory;
                _emailService = emailService;
            }

            public async Task<Model> Handle(Command request, CancellationToken cancellationToken)
            {
                using var session = _sessionFactory.QuerySession();

                var config = await session.Query<CtaConfiguration>()
                    .FirstOrDefaultAsync(q => q.Slug == request.Slug, token: cancellationToken);

                if (config == null)
                    throw new InvalidOperationException($"CTA configuration '{request.Slug}' not found.");

                var tokens = new
                {
                    email = request.Email,
                    name = request.Name ?? request.Email,
                    date = DateTime.UtcNow.ToString("MMMM d, yyyy")
                };

                if (config.AdminEmailTemplateId.HasValue)
                {
                    var adminTemplate = await session.LoadAsync<EmailTemplate>(
                        config.AdminEmailTemplateId.Value, cancellationToken);

                    if (adminTemplate != null)
                    {
                        await _emailService.SendEmailAsync(
                            config.FromEmail,
                            config.AdminEmail,
                            SubstituteTokens(adminTemplate.Subject, tokens),
                            SubstituteTokens(adminTemplate.Body, tokens),
                            cancellationToken);
                    }
                }

                if (config.SubmitterEmailTemplateId.HasValue)
                {
                    var submitterTemplate = await session.LoadAsync<EmailTemplate>(
                        config.SubmitterEmailTemplateId.Value, cancellationToken);

                    if (submitterTemplate != null)
                    {
                        await _emailService.SendEmailAsync(
                            config.FromEmail,
                            request.Email,
                            SubstituteTokens(submitterTemplate.Subject, tokens),
                            SubstituteTokens(submitterTemplate.Body, tokens),
                            cancellationToken);
                    }
                }

                return new Model(true);
            }

            private static string SubstituteTokens(string template, dynamic tokens)
            {
                if (string.IsNullOrEmpty(template))
                    return template;

                return template
                    .Replace("{{email}}", tokens.email)
                    .Replace("{{name}}", tokens.name)
                    .Replace("{{date}}", tokens.date);
            }
        }
    }
}
