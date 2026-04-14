using System;
using System.Reflection;
using AutoMapper;
using blastcms.web.Data;
using blastcms.web.Factories;
using blastcms.web.Tenant;
using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.Abstractions;
using Marten;
using Moq;
using NUnit.Framework;
using ThrowawayDb.Postgres;
using Weasel.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace blastcms.web.tests
{
    [SetUpFixture]
    public  class Tests
    {
        public static DocumentStore Store;
        public static ThrowawayDatabase Database;
        public static ISessionFactory SessionFactory;
        public static IMapper  Mapper;

        [OneTimeSetUp]
        public  void Setup()
        {
            // Use DI to create the mapper (AutoMapper 15 approach)
            var services = new ServiceCollection();
            services.AddLogging(); // Required by AutoMapper 15
            services.AddAutoMapper(cfg =>
            {
                cfg.AddMaps(typeof(Program).Assembly);
            });
            var provider = services.BuildServiceProvider();
            Mapper = provider.GetRequiredService<IMapper>();

            var testHost = Environment.GetEnvironmentVariable("DB_HOST")?? "localhost";

            Database = ThrowawayDatabase.Create(username: "blastcms_user", password: "not_magical_scary", host: testHost);

            Store = DocumentStore.For( _ =>
            {
                // Turn this off in production
                _.AutoCreateSchemaObjects = AutoCreate.All;
                _.Schema.For<PodcastEpisode>().ForeignKey<Podcast>(x => x.PodcastId);
                _.Schema.For<EventItem>().ForeignKey<EventVenue>(x => x.VenueId);
                // This is still mandatory
                _.Connection(Database.ConnectionString);

            });

            var multitenantContext = new Mock<IMultiTenantContext<CustomTenantInfo>>();
            multitenantContext.Setup(p => p.TenantInfo).Returns(new CustomTenantInfo { Id = "test-tenant-1", Name = "Test Tenant 1", Identifier = "test_tenant_1" });

            var multitenantContextAccessor = new Mock<IMultiTenantContextAccessor<CustomTenantInfo>>();
            multitenantContextAccessor.Setup(p => p.MultiTenantContext).Returns(multitenantContext.Object);

            SessionFactory = new CustomSessionFactory(Store, multitenantContextAccessor.Object);

            Store.InitializeDatabase();

        }

        [OneTimeTearDown]
        public  void TearDown()
        {
            Store.Dispose();
            Database.Dispose();
        }
    }
}