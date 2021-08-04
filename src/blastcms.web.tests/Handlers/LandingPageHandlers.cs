using blastcms.web.Data;
using blastcms.web.Handlers;
using FizzWare.NBuilder;
using NUnit.Framework;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace blastcms.web.tests.Handlers
{
    public class LandingPageHandlers
    {
        [Test]
        public void GetLandingPages()
        {
            using (var session = Tests.SessionFactory.QuerySession())
            {
                var data = session.Query<LandingPage>().Count();
                Assert.IsTrue(data >= 100);
            }
        }

        [Test]
        [TestCase(1, null, 10, 100, Description = "First Page no search")]
        [TestCase(2, null, 10, 100, Description = "Second Page no search")]
        [TestCase(1, "Title12", 1, 1, Description = "First Page with search match")]
        [TestCase(1, "empty", 0, 0, Description = "First Page with search no match")]
        public async Task GetLandingPages_handler(int page, string search, int expectedCount, int expectedTotal)
        {
            var command = new GetLandingPages.Query(0, 10, page, search);

            var sut = new GetLandingPages.Handler(Tests.SessionFactory, Tests.Mapper);

            var result = await sut.Handle(command, new CancellationToken());

            Assert.IsNotNull(result);
            Assert.AreEqual(expectedCount, result.Data.Count());
            Assert.GreaterOrEqual(result.Count, expectedTotal);
            Assert.AreEqual(page, result.Page);
        }

        [Test]
        public void GetLandingPage()
        {

            var testArticle = DatabaseInitialization.GeneratedLandingPages.Skip(2).First();

            using (var session = Tests.SessionFactory.QuerySession())
            {
                var article = session.Query<LandingPage>().First(q => q.Id == testArticle.Id);
                Assert.IsNotNull(article);
                Assert.AreEqual(testArticle.Title, article.Title);
            }
        }

        [Test]
        public async Task AlterLandingPage_update()
        {
            var testArticle = DatabaseInitialization.GeneratedLandingPages.Skip(10).First();

            LandingPage article = null;
            using (var session = Tests.SessionFactory.QuerySession())
            {
                article = session.Query<LandingPage>().First(q => q.Id == testArticle.Id);
                Assert.IsNotNull(article);
            }

            var command = Tests.Mapper.Map<AlterLandingPage.Command>(article);
            command.Title = "NewTag";

            var sut = new AlterLandingPage.Handler(Tests.SessionFactory, Tests.Mapper);

            var result = await sut.Handle(command, new CancellationToken());

            Assert.IsNotNull(result);

            using (var session = Tests.SessionFactory.QuerySession())
            {
                var modArticle = session.Query<LandingPage>().First(q => q.Id == testArticle.Id);
                Assert.IsNotNull(modArticle);
                Assert.AreEqual(command.Title, modArticle.Title);
                Assert.AreEqual(command.Id, modArticle.Id);
            }

        }

        [Test]
        public async Task AlterLandingPage_insert()
        {
            var command = Builder<AlterLandingPage.Command>.CreateNew()
                .With(p => p.Id = null)
                .With(p => p.Title = "Mic Man")
                .Build();


            var sut = new AlterLandingPage.Handler(Tests.SessionFactory, Tests.Mapper);

            var result = await sut.Handle(command, new CancellationToken());

            Assert.IsNotNull(result);

            using (var session = Tests.SessionFactory.QuerySession())
            {
                var modArticle = session.Query<LandingPage>().First(q => q.Title == "Mic Man");
                Assert.IsNotNull(modArticle);
                Assert.AreEqual(command.Title, modArticle.Title);
            }

        }
    }
}
