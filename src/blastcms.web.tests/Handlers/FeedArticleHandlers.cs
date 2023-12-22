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
    class FeedArticleHandlers
    {
        [Test]
        public void GetFeedArticles()
        {
            using (var session = Tests.SessionFactory.QuerySession())
            {
                var data = session.Query<FeedArticle>().Count();
                ClassicAssert.IsTrue(data >= 100);
            }
        }

        [Test]
        [TestCase(1, null, 10, 100, Description = "First Page no search")]
        [TestCase(2, null, 10, 100, Description = "Second Page no search")]
        [TestCase(1, "Title12", 1, 1, Description = "First Page with search match")]
        [TestCase(1, "empty", 0, 0, Description = "First Page with search no match")]
        public async Task GetFeedArticles_handler(int page, string search, int expectedCount, int expectedTotal)
        {
            var command = new GetFeedArticles.Query(0, 10, page, search);

            var sut = new GetFeedArticles.Handler(Tests.SessionFactory, Tests.Mapper);

            var result = await sut.Handle(command, new CancellationToken());

            ClassicAssert.IsNotNull(result);
            ClassicAssert.AreEqual(expectedCount, result.Data.Count());
            ClassicAssert.GreaterOrEqual(result.Count, expectedTotal);
            ClassicAssert.AreEqual(page, result.Page);
        }

        [Test]
        public void GetFeedArticle()
        {

            var testArticle = DatabaseInitialization.GeneratedFeedArticles.Skip(2).First();

            using (var session = Tests.SessionFactory.QuerySession())
            {
                var article = session.Query<FeedArticle>().First(q => q.Id == testArticle.Id);
                ClassicAssert.IsNotNull(article);
                ClassicAssert.AreEqual(testArticle.Title, article.Title);
            }
        }

        [Test]
        public async Task AlterFeedArticle_update()
        {
            var testArticle = DatabaseInitialization.GeneratedFeedArticles.Skip(10).First();

            FeedArticle article = null;
            using (var session = Tests.SessionFactory.QuerySession())
            {
                article = session.Query<FeedArticle>().First(q => q.Id == testArticle.Id);
                ClassicAssert.IsNotNull(article);
            }

            var command = Tests.Mapper.Map<AlterFeedArticle.Command>(article);
            command.Title = "NewTag";

            var sut = new AlterFeedArticle.Handler(Tests.SessionFactory, Tests.Mapper);

            var result = await sut.Handle(command, new CancellationToken());

            ClassicAssert.IsNotNull(result);

            using (var session = Tests.SessionFactory.QuerySession())
            {
                var modArticle = session.Query<FeedArticle>().First(q => q.Id == testArticle.Id);
                ClassicAssert.IsNotNull(modArticle);
                ClassicAssert.AreEqual(command.Title, modArticle.Title);
            }

        }

        [Test]
        public async Task AlterFeedArticle_insert()
        {
            var command = Builder<AlterFeedArticle.Command>.CreateNew()
                .With(p => p.Id = null)
                .With(p => p.Title = "Mic Man")
                .Build();


            var sut = new AlterFeedArticle.Handler(Tests.SessionFactory, Tests.Mapper);

            var result = await sut.Handle(command, new CancellationToken());

            ClassicAssert.IsNotNull(result);

            using (var session = Tests.SessionFactory.QuerySession())
            {
                var modArticle = session.Query<FeedArticle>().First(q => q.Title == "Mic Man");
                ClassicAssert.IsNotNull(modArticle);
                ClassicAssert.AreEqual(command.Title, modArticle.Title);
            }

        }
    }
}
