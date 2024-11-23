using blastcms.web.Data;
using blastcms.web.Handlers;
using FizzWare.NBuilder;
using NUnit.Framework.Legacy;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using blastcms.web.Handlers.Tenant;

namespace blastcms.web.tests.Handlers
{
    public class BlastTenantHandlers
    {
        [Test]
        public void GetTenants()
        {
            using var session = Tests.SessionFactory.QuerySession();
            var data = session.Query<BlastTenant>().Count();
            ClassicAssert.IsTrue(data >= 100);
        }

        [Test]
        public async Task TenantExists()
        {
            var testTenant = DatabaseInitialization.GeneratedBlastTenants.Skip(2).First();

            var query = new GetTenantExists.Query(testTenant.Identifier);

            var sut = new GetTenantExists.Handler(Tests.Store);

            var result = await sut.Handle(query, new CancellationToken());

            ClassicAssert.IsNotNull(result);
            ClassicAssert.IsTrue(result.Exists);
        }

        [Test]
        public async Task TenantDoesNotExist()
        {
            var tenantIdentifier = "ThisDoesNotExist";

            var query = new GetTenantExists.Query(tenantIdentifier);

            var sut = new GetTenantExists.Handler(Tests.Store);

            var result = await sut.Handle(query, new CancellationToken());

            ClassicAssert.IsNotNull(result);
            ClassicAssert.IsFalse(result.Exists);
        }


        [Test]
        [TestCase(1, null, 10, 100, Description = "First Page no search")]
        [TestCase(2, null, 10, 100, Description = "Second Page no search")]
        [TestCase(1, "test-tenant-1", 1, 1, Description = "First Page with search match")]
        [TestCase(1, "empty", 0, 0, Description = "First Page with search no match")]
        public async Task GetBlastTenants_handler(int page, string search, int expectedCount, int expectedTotal)
        {
            var command = new GetTenants.Query(0, 10, page, search);

            var sut = new GetTenants.Handler(Tests.Store);

            var result = await sut.Handle(command, new CancellationToken());

            ClassicAssert.IsNotNull(result);
            ClassicAssert.AreEqual(expectedCount, result.Data.Count());
            ClassicAssert.GreaterOrEqual(result.Count, expectedTotal);
            ClassicAssert.AreEqual(page, result.Page);
        }

        [Test]
        public void GetBlastTenant()
        {

            var testTenant = DatabaseInitialization.GeneratedBlastTenants.Skip(2).First();

            using var session = Tests.SessionFactory.QuerySession();
            var tenant = session.Query<BlastTenant>().First(q => q.Id == testTenant.Id);
            ClassicAssert.IsNotNull(tenant);
            ClassicAssert.AreEqual(testTenant.Identifier, tenant.Identifier);
        }

        [Test]
        public async Task AlterBlastTenant_update()
        {
            var testArticle = DatabaseInitialization.GeneratedBlastTenants.Skip(10).First();

            BlastTenant tenant = null;
            using (var session = Tests.SessionFactory.QuerySession())
            {
                tenant = session.Query<BlastTenant>().First(q => q.Id == testArticle.Id);
                ClassicAssert.IsNotNull(tenant);
            }

            var command = Tests.Mapper.Map<AlterTenant.Command>(tenant);
            command.ReferenceId = "NewTag";

            var sut = new AlterTenant.Handler(Tests.Store, Tests.Mapper);

            var result = await sut.Handle(command, new CancellationToken());

            ClassicAssert.IsNotNull(result);

            using (var session = Tests.SessionFactory.QuerySession())
            {
                var modArticle = session.Query<BlastTenant>().First(q => q.Id == testArticle.Id);
                ClassicAssert.IsNotNull(modArticle);
                ClassicAssert.AreEqual(command.ReferenceId, modArticle.ReferenceId);
            }

        }

        [Test]
        public async Task AlterBlastTenant_insert()
        {
            var command = Builder<AlterTenant.Command>.CreateNew()
                .With(p => p.Id = Guid.NewGuid().ToString())
                .With(p => p.CustomerId = "Mic Man")
                .Build();


            var sut = new AlterTenant.Handler(Tests.Store, Tests.Mapper);

            var result = await sut.Handle(command, new CancellationToken());

            ClassicAssert.IsNotNull(result);

            using (var session = Tests.SessionFactory.QuerySession())
            {
                var modArticle = session.Query<BlastTenant>().First(q => q.CustomerId == "Mic Man");
                ClassicAssert.IsNotNull(modArticle);
                ClassicAssert.AreEqual(command.CustomerId, modArticle.CustomerId);
            }

        }
    }
}
