using blastcms.web.Data;
using blastcms.web.Handlers.Tenant;
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

namespace blastcms.web.tests.Handlers
{
    public class EmailTemplateHandlers
    {
        [Test]
        public void GetEmailTemplates()
        {
            using var session = Tests.SessionFactory.QuerySession();
            var data = session.Query<EmailTemplate>().Count();
            ClassicAssert.IsTrue(data >= 100);
        }

        [Test]
        [TestCase(1, null, 10, 100, Description = "First Page no search")]
        [TestCase(2, null, 10, 100, Description = "Second Page no search")]
        [TestCase(1, "email", 1, 1, Description = "First Page with search match")]
        [TestCase(1, "empty", 0, 0, Description = "First Page with search no match")]
        public async Task GetEmailTemplates_handler(int page, string search, int expectedCount, int expectedTotal)
        {
            var command = new GetEmailTemplates.Query(0, 10, page, search);

            var sut = new GetEmailTemplates.Handler(Tests.SessionFactory);

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
            var testTemplate = DatabaseInitialization.GeneratedEmailTemplates.Skip(10).First();

            EmailTemplate template = null;
            using (var session = Tests.SessionFactory.QuerySession())
            {
                template = session.Query<EmailTemplate>().FirstOrDefault(q => q.Id == testTemplate.Id);
                ClassicAssert.IsNotNull(template);
            }

            var command = Tests.Mapper.Map<AlterEmailTemplate.Command>(template);
            command.Name = "NewTag";

            var sut = new AlterEmailTemplate.Handler(Tests.SessionFactory, Tests.Mapper);

            var result = await sut.Handle(command, new CancellationToken());

            ClassicAssert.IsNotNull(result);

            using (var session = Tests.SessionFactory.QuerySession())
            {
                var modArticle = session.Query<EmailTemplate>().First(q => q.Id == testTemplate.Id);
                ClassicAssert.IsNotNull(modArticle);
                ClassicAssert.AreEqual(command.Name, modArticle.Name);
            }

        }

        [Test]
        public async Task AlterBlastTenant_insert()
        {
            var command = Builder<AlterEmailTemplate.Command>.CreateNew()
                .With(p => p.Id = Guid.NewGuid())
                .With(p => p.Name = "Mic Man")
                .Build();


            var sut = new AlterEmailTemplate.Handler(Tests.SessionFactory, Tests.Mapper);

            var result = await sut.Handle(command, new CancellationToken());

            ClassicAssert.IsNotNull(result);

            using (var session = Tests.SessionFactory.QuerySession())
            {
                var modArticle = session.Query<EmailTemplate>().First(q => q.Name == "Mic Man");
                ClassicAssert.IsNotNull(modArticle);
                ClassicAssert.AreEqual(command.Name, modArticle.Name);
            }

        }
    }
}
