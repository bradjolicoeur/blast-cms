using blastcms.web.Factories;
using Marten;
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
using FluentValidation.AspNetCore;
using blastcms.ArticleScanService;
using Microsoft.OpenApi.Models;
using blastcms.web.Swagger;
using System.Linq;
using Microsoft.AspNetCore.HttpOverrides;
using blastcms.web.CloudStorage;
using FluentValidation;
using blastcms.ImageResizeService;
using Weasel.Core;
using blastcms.web.Security;
using blastcms.web.Data;
using System.Reflection;
using Asp.Versioning;
using blastcms.web.Converters;
using Microsoft.OpenApi.Any;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http.Json;
using blastcms.web.Tenant;
using Microsoft.Extensions.Primitives;
using blastcms.web.Middleware;

namespace blastcms.web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public static IConfiguration Configuration { get; private set; }

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
            
            
            //services.AddFluentValidationAutoValidation().AddFluentValidationClientsideAdapters(fv =>
            //{

            //});

            services.AddControllers().AddFluentValidation(fv =>
                    {
                        fv.ImplicitlyValidateChildProperties = true;
                        fv.ImplicitlyValidateRootCollectionElements = true;
                        fv.RegisterValidatorsFromAssemblyContaining<Startup>();
                    })
                .AddJsonOptions(opts =>
                {
                    opts.JsonSerializerOptions.Converters.Add(new JsonTimeSpanConverter());
                    opts.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                });

            services.Configure<JsonOptions>(opts =>
            {
                opts.SerializerOptions.Converters.Add(new JsonTimeSpanConverter());
                opts.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });

            services.AddMvc();
            services.AddApiVersioning(config =>
            {
                config.DefaultApiVersion = new ApiVersion(1, 0);
                config.AssumeDefaultVersionWhenUnspecified = true;
                config.ReportApiVersions = true;
            });

            services.AddMarten(opts =>
            {
                opts.Connection(Configuration["BLASTCMS_DB"]);

                opts.AutoCreateSchemaObjects = AutoCreate.All;

                opts.Schema.For<PodcastEpisode>().ForeignKey<Podcast>(x => x.PodcastId);
                opts.Schema.For<EventItem>().ForeignKey<EventVenue>(x => x.VenueId);

                opts.Policies.AllDocumentsAreMultiTenanted();

            })
                .InitializeWith(new InitialData(InitialDatasets.Tenants(Configuration)))
                .BuildSessionsWith<CustomSessionFactory>();


            // Register the Swagger generator, defining 1 or more Swagger documents
            services.AddSwaggerGen(c =>
            {
                c.OperationFilter<AddRequiredHeaderParameter>();
                c.DocumentFilter<HideInDocsFilter>();
                //c.DocumentFilter<InjectSamples>();
                c.EnableAnnotations();

                c.MapType<TimeSpan>(() => new OpenApiSchema
                {
                    Type = "string",
                    Format = "duration",
                    Example = new OpenApiString("00:00:00")
                });

                c.CustomSchemaIds(type =>
                {

                    var fullname = type.FullName;
                    if (fullname.Contains("IPagedData"))
                    {
                        return GetName(type);
                    }

                    var lastIndex = fullname.LastIndexOf('.');
                    var name = fullname[(lastIndex + 1)..]
                        .Replace("+", "");
                    return name;
                });

                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "Blast CMS API",
                    Description = "Blast CMS Content API",
                    Contact = new OpenApiContact
                    {
                        Name = "Brad Jolicoeur",
                        Email = string.Empty,
                        Url = new Uri("https://bradjolicoeur.com"),
                    },
                    License = new OpenApiLicense
                    {
                        Name = "Use under MIT",
                        Url = new Uri("https://example.com/license"),
                    }
                });


            });

            AddAuthenticationServices(services);

            services.AddMultiTenant<CustomTenantInfo>()
                        //.WithHostStrategy()
                        .WithBasePathStrategy()
                        //.WithStaticStrategy("customer2")
                        //.WithDelegateStrategy(async context =>
                        //{
                        //    var httpContext = context as HttpContext;
                        //    if (httpContext == null)
                        //        return null;

                        //    //httpContext.Request.Query.TryGetValue("tenant", out StringValues tenantIdParam);
                        //    //return tenantIdParam.ToString();
                        //    return "customer2";
                        //})
                        .WithStore<MartenTenantStore>(ServiceLifetime.Transient)
                        //.WithConfigurationStore()
                        .WithPerTenantAuthentication();
            
            

            services.AddHttpContextAccessor();

            

            services.AddTransient<IMetaScraper, MetaScraperOpenAI>();

            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
            services.AddAutoMapper(typeof(Startup));

            services.AddValidatorsFromAssemblyContaining<Startup>();

            services.AddMudServices();

            services.AddHealthChecks();

            services.AddSingleton<ITinifyService, TinifyService>();
            services.AddSingleton<ICloudStorage, GoogleCloudStorage>();
            services.AddSingleton<IHashingService, HashingService>();
        }

        private string GetName(Type type)
        {
            if (!type.IsConstructedGenericType) return type.Name;

            var prefix = type.GetGenericArguments()
                .Select(genericArg => GetName(genericArg))
                .Aggregate((previous, current) => previous + current);

            return prefix + type.Name.Split('`').First();
        }


        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {

            app.UseMultiTenant();
            app.UseTenantBasePathMiddleware();

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
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "BLAST CMS API Documentation V1");
            
            });

            app.UseReDoc(c =>
            {
                c.DocumentTitle = "BLAST CMS API Documentation";
                //c.HeadContent = "This is some content";
                c.SpecUrl = "/swagger/v1/swagger.json";
            });

            app.UseHealthChecks("/health");
            app.UseStaticFiles();

            app.UseRouting();
            
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


            // add authentication services
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                   .AddCookie()
                   .AddOpenIdConnect();

            services.ConfigurePerTenant<OpenIdConnectOptions, CustomTenantInfo>(OpenIdConnectDefaults.AuthenticationScheme, (options, tenantInfo) =>
            {
                options.ResponseType = "code";
                options.RequireHttpsMetadata = false;
                options.Scope.Add("email");
            });

            //services.ConfigurePerTenant<CookieAuthenticationOptions, CustomTenantInfo>((options, tenantInfo) =>
            //{
            //    options.Cookie.Name = "SignInCookie-" + tenantInfo.Id;
            //    options.Cookie.SameSite = SameSiteMode.None;
            //});

        }

        private void CheckSameSite(HttpContext httpContext, CookieOptions options)
        {
            var userAgent = httpContext.Request.Headers["User-Agent"].ToString();
            if (DisallowsSameSiteNone(userAgent))
            {
                options.SameSite = SameSiteMode.Unspecified;
                options.Secure = false;
            }
        }

        //  Read comments in https://docs.microsoft.com/en-us/aspnet/core/security/samesite?view=aspnetcore-3.1
        public bool DisallowsSameSiteNone(string userAgent)
        {
            // Check if a null or empty string has been passed in, since this
            // will cause further interrogation of the useragent to fail.
            if (String.IsNullOrWhiteSpace(userAgent))
                return false;

            // Cover all iOS based browsers here. This includes:
            // - Safari on iOS 12 for iPhone, iPod Touch, iPad
            // - WkWebview on iOS 12 for iPhone, iPod Touch, iPad
            // - Chrome on iOS 12 for iPhone, iPod Touch, iPad
            // All of which are broken by SameSite=None, because they use the iOS networking
            // stack.
            if (userAgent.Contains("CPU iPhone OS 12") ||
                userAgent.Contains("iPad; CPU OS 12"))
            {
                return true;
            }

            // Cover Mac OS X based browsers that use the Mac OS networking stack. 
            // This includes:
            // - Safari on Mac OS X.
            // This does not include:
            // - Chrome on Mac OS X
            // Because they do not use the Mac OS networking stack.
            if (userAgent.Contains("Macintosh; Intel Mac OS X") &&
                userAgent.Contains("Version/") && userAgent.Contains("Safari"))
            {
                return true;
            }

            // Cover Chrome 50-69, because some versions are broken by SameSite=None, 
            // and none in this range require it.
            // Note: this covers some pre-Chromium Edge versions, 
            // but pre-Chromium Edge does not require SameSite=None.
            if (userAgent.Contains("Chrome/5") || userAgent.Contains("Chrome/6"))
            {
                return true;
            }

            return false;
        }
    }
}
