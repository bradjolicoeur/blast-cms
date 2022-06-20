using blastcms.web.Factories;
using blastcms.web.Helpers;
using blastcms.web.Registry;
using Marten;
using MediatR;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading.Tasks;
using MudBlazor.Services;
using Finbuckle.MultiTenant;
using blastcms.ArticleScanService;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Mvc;
using blastcms.web.Swagger;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.HttpOverrides;
using blastcms.web.CloudStorage;
using FluentValidation;
using blastcms.ImageResizeService;
using Weasel.Core;
using blastcms.web.Security;

namespace blastcms.web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public static IConfiguration Configuration { get; private set; }

        private void CheckSameSite(HttpContext httpContext, CookieOptions options)
        {
            if (options.SameSite == SameSiteMode.None)
            {
                var userAgent = httpContext.Request.Headers["User-Agent"].ToString();
                // TODO: Use your User Agent library of choice here.
                if (SameSitePolicyHelper.DisallowsSameSiteNone(userAgent))
                {
                    options.SameSite = SameSiteMode.Unspecified;
                }
            }
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedFor |
                    ForwardedHeaders.XForwardedProto;
                // Only loopback proxies are allowed by default.
                // Clear that restriction because forwarders are enabled by explicit 
                // configuration.
                options.KnownNetworks.Clear();
                options.KnownProxies.Clear();
            });

            services.AddRazorPages();
            services.AddServerSideBlazor()
                .AddHubOptions(x => x.MaximumReceiveMessageSize = 102400000);

            services.AddMvc();
            services.AddApiVersioning(config =>
            {
                config.DefaultApiVersion = new ApiVersion(1, 0);
                config.AssumeDefaultVersionWhenUnspecified = true;
                config.ReportApiVersions = true;
            });

            // Register the Swagger generator, defining 1 or more Swagger documents
            services.AddSwaggerGen(c =>
            {
                c.OperationFilter<AddRequiredHeaderParameter>();
                //c.DocumentFilter<InjectSamples>();
                c.EnableAnnotations();

                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "Blast CMS API",
                    Description = "Blast CMS Content API",
                    Contact = new OpenApiContact
                    {
                        Name = "Brad Jolicoeur",
                        Email = string.Empty,
                        Url = new Uri("https://bradjoli"),
                    },
                    License = new OpenApiLicense
                    {
                        Name = "Use under MIT",
                        Url = new Uri("https://example.com/license"),
                    }
                });


            });

            AddAuthenticationServices(services);

            services.AddMultiTenant<TenantInfo>()
                        .WithHostStrategy()
                        .WithConfigurationStore();

            services.AddHttpContextAccessor();

            services.AddMarten(opts =>
            {
                opts.Connection(Configuration["BLASTCMS_DB"]);

                opts.AutoCreateSchemaObjects = AutoCreate.All;

                opts.Schema.Include<BlastcmsMartenRegistry>();

                opts.Policies.AllDocumentsAreMultiTenanted();

            })
                .BuildSessionsWith<CustomSessionFactory>();

            services.AddTransient<IMetaScraper, MetaScraper>();

            services.AddMediatR(typeof(Startup).Assembly);
            services.AddAutoMapper(typeof(Startup));

            services.AddValidatorsFromAssemblyContaining<Startup>();

            services.AddMudServices();

            services.AddHealthChecks();

            services.AddSingleton<ITinifyService, TinifyService>();
            services.AddSingleton<ICloudStorage, GoogleCloudStorage>();
            services.AddSingleton<IHashingService, HashingService>();
        }



        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseForwardedHeaders();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
            });

            app.UseReDoc(c =>
            {
                c.DocumentTitle = "BLAST CMS API Documentation";
                //c.HeadContent = "This is some content";
                c.SpecUrl = "/swagger/v1/swagger.json";
            });

            //app.UseHttpsRedirection();
            app.UseHealthChecks("/health");
            app.UseStaticFiles();
            
            app.UseRouting();
            app.UseMultiTenant();

            app.UseCookiePolicy();
            app.UseAuthentication();
            app.UseAuthorization();


            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapBlazorHub();

                //2019-10-05 For Finbuckle Multitenant
                endpoints.MapControllerRoute("default", "{__tenant__=}/{controller=Home}/{action=Index}");

                endpoints.MapFallbackToPage("/_Host");
            });
        }

        private void AddAuthenticationServices(IServiceCollection services)
        {
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            })
            .AddCookie()
            .AddOpenIdConnect("Auth0", options =>
            {
                // Set the authority to your Auth0 domain
                options.Authority = $"https://{Configuration["Auth0:Domain"]}";

                // Configure the Auth0 Client ID and Client Secret
                options.ClientId = Configuration["Auth0:ClientId"];
                options.ClientSecret = Configuration["Auth0:ClientSecret"];

                // Set response type to code
                options.ResponseType = "code";

                // Configure the scope
                options.Scope.Clear();
                options.Scope.Add("openid");
                options.Scope.Add("profile");

                // Set the callback path, so Auth0 will call back to http://localhost:3000/callback
                // Also ensure that you have added the URL as an Allowed Callback URL in your Auth0 dashboard
                options.CallbackPath = new PathString("/callback");

                // Configure the Claims Issuer to be Auth0
                options.ClaimsIssuer = "Auth0";

                options.Events = new OpenIdConnectEvents
                {
                    OnRedirectToIdentityProvider = context =>
                    {
                        var builder = new UriBuilder(context.ProtocolMessage.RedirectUri);

                        builder.Scheme = "https";

                        context.ProtocolMessage.RedirectUri = builder.ToString().Replace(":80", "");

                        return Task.FromResult(0);
                    },
                    // handle the logout redirection
                    OnRedirectToIdentityProviderForSignOut = (context) =>
                    {
                        var logoutUri = $"https://{Configuration["Auth0:Domain"]}/v2/logout?client_id={Configuration["Auth0:ClientId"]}";

                        var postLogoutUri = context.Properties.RedirectUri;
                        if (!string.IsNullOrEmpty(postLogoutUri))
                        {
                            if (postLogoutUri.StartsWith("/"))
                            {
                                // transform to absolute
                                var request = context.Request;
                                postLogoutUri = request.Scheme + "://" + request.Host + request.PathBase + postLogoutUri;
                            }
                            logoutUri += $"&returnTo={ Uri.EscapeDataString(postLogoutUri)}";
                        }

                        context.Response.Redirect(logoutUri);
                        context.HandleResponse();

                        return Task.CompletedTask;
                    }
                };
            });
        }
    }
}
