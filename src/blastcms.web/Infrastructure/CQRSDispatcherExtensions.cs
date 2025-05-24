using blastcms.web.Handlers;
using Microsoft.Extensions.DependencyInjection;
using Scrutor;
using System.Reflection;

namespace blastcms.web.Infrastructure
{
    public static class CQRSDispatcherExtensions
    {
        public static IServiceCollection RegisterCQRSDispatcherAndHandlers(this IServiceCollection services, Assembly assembly)
        {
            // Register the dispatcher
            services.AddScoped<IDispatcher, Dispatcher>();

            // Register your command/query handlers here manually, Example:
            // services.AddTransient<ICommandHandler<CreateUser>, CreateUserHandler>();
            services.AddTransient<AlterBlogArticle.Handler>();

            services.Scan(scan => scan.FromAssemblies(assembly)
            .AddClasses(classes => classes.AssignableTo(typeof(IRequestHandler<,>)))
                .AsImplementedInterfaces()
                .WithScopedLifetime()
            .AddClasses(classes => classes.AssignableTo(typeof(IRequestHandler<>)))
                .AsImplementedInterfaces()
                .WithScopedLifetime());

            return services;
        }
    }
}
