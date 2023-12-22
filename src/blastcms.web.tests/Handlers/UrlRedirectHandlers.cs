using blastcms.web.Data;
using blastcms.web.Handlers;
using FizzWare.NBuilder;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace blastcms.web.tests.Handlers
{
    class UrlRedirectHandlers
    {
        [Test]
        public void GetUrlRedirects()
        {
            using (var session = Tests.SessionFactory.QuerySession())
            {
                var data = session.Query<UrlRedirect>().Count();
                ClassicAssert.IsTrue(data >= 100);
            }
        }

        [Test]
        [TestCase(1, null, 10, 100, Description = "First Page no search")]
        [TestCase(2, null, 10, 100, Description = "Second Page no search")]
        [TestCase(1, "RedirectFrom12", 1, 1, Description = "First Page with search match")]
        [TestCase(1, "empty", 0, 0, Description = "First Page with search no match")]
        public async Task GetUrlRedirects_handler(int page, string search, int expectedCount, int expectedTotal)
        {
            var command = new GetUrlRedirects.Query(0, 10, page, search);

            var sut = new GetUrlRedirects.Handler(Tests.SessionFactory);

            var result = await sut.Handle(command, new CancellationToken());

            ClassicAssert.IsNotNull(result);
            ClassicAssert.AreEqual(expectedCount, result.Data.Count());
            ClassicAssert.GreaterOrEqual(result.Count, expectedTotal);
            ClassicAssert.AreEqual(page, result.Page);
        }

        [Test]
        public async Task GetUrlRedirectByFrom()
        {

            var testArticle = DatabaseInitialization.GeneratedUrlRedirects.Skip(2).First();

            var command = new GetUrlRedirectByFrom.Query(testArticle.RedirectFrom);

            var sut = new GetUrlRedirectByFrom.Handler(Tests.SessionFactory);

            var result = await sut.Handle(command, new CancellationToken());

            ClassicAssert.IsNotNull(result);
            ClassicAssert.AreEqual(result.Data.RedirectFrom, result.Data.RedirectFrom);
        }

        [Test]
        public async Task GetUrlRedirect()
        {

            var testArticle = DatabaseInitialization.GeneratedUrlRedirects.Skip(2).First();

            var command = new GetUrlRedirect.Query(testArticle.Id);

            var sut = new GetUrlRedirect.Handler(Tests.SessionFactory);

            var result = await sut.Handle(command, new CancellationToken());

            ClassicAssert.IsNotNull(result);
            ClassicAssert.AreEqual(result.Data.RedirectFrom, result.Data.RedirectFrom);
        }

        [Test]
        public async Task AlterUrlRedirect_update()
        {
            var testArticle = DatabaseInitialization.GeneratedUrlRedirects.Skip(10).First();

            UrlRedirect article = null;
            using (var session = Tests.SessionFactory.QuerySession())
            {
                article = session.Query<UrlRedirect>().First(q => q.Id == testArticle.Id);
                ClassicAssert.IsNotNull(article);
            }

            var command = Tests.Mapper.Map<AlterUrlRedirect.Command>(article);
            command.RedirectFrom = "NewTag";

            var sut = new AlterUrlRedirect.Handler(Tests.SessionFactory, Tests.Mapper);

            var result = await sut.Handle(command, new CancellationToken());

            ClassicAssert.IsNotNull(result);

            using (var session = Tests.SessionFactory.QuerySession())
            {
                var modArticle = session.Query<UrlRedirect>().First(q => q.Id == testArticle.Id);
                ClassicAssert.IsNotNull(modArticle);
                ClassicAssert.AreEqual(command.RedirectFrom, modArticle.RedirectFrom);
                ClassicAssert.AreEqual(command.Id, modArticle.Id);
            }

        }

        [Test]
        public async Task AlterUrlRedirect_insert()
        {
            var command = Builder<AlterUrlRedirect.Command>.CreateNew()
                .With(p => p.Id = null)
                .With(p => p.RedirectFrom = "Mic Man")
                .Build();


            var sut = new AlterUrlRedirect.Handler(Tests.SessionFactory, Tests.Mapper);

            var result = await sut.Handle(command, new CancellationToken());

            ClassicAssert.IsNotNull(result);

            using (var session = Tests.SessionFactory.QuerySession())
            {
                var modArticle = session.Query<UrlRedirect>().First(q => q.RedirectFrom == "Mic Man");
                ClassicAssert.IsNotNull(modArticle);
                ClassicAssert.AreEqual(command.RedirectFrom, modArticle.RedirectFrom);
            }

        }
    }
}
