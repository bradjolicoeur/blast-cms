using AutoMapper;
using blastcms.web.Factories;
using blastcms.web.Registry;
using Marten;
using NUnit.Framework;
using ThrowawayDb.Postgres;

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
            var configuration = new MapperConfiguration(cfg => cfg.AddMaps(typeof(Startup)));
            Mapper = new Mapper(configuration);


            Database = ThrowawayDatabase.Create(username: "blastcms_user", password: "not_magical_scary", host: "postgres");

            Store = DocumentStore.For( _ =>
            {
                // Turn this off in production
                _.AutoCreateSchemaObjects = AutoCreate.All;

                // This is still mandatory
                _.Connection(Database.ConnectionString);

               _.Schema.Include<BlastcmsMartenRegistry>();
            });

            SessionFactory = new CustomSessionFactory(Store);

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