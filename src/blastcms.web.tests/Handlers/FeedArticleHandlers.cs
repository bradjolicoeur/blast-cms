using blastcms.web.Data;
using blastcms.web.Handlers;
using FizzWare.NBuilder;
using NUnit.Framework;
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
                Assert.IsTrue(data >= 100);
            }
        }

        [Test]
        public void GetFeedArticle()
        {

            var testArticle = DatabaseInitialization.GeneratedFeedArticles.Skip(2).First();

            using (var session = Tests.SessionFactory.QuerySession())
            {
                var article = session.Query<FeedArticle>().First(q => q.Id == testArticle.Id);
                Assert.IsNotNull(article);
                Assert.AreEqual(testArticle.Title, article.Title);
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
                Assert.IsNotNull(article);
            }

            var command = Tests.Mapper.Map<AlterFeedArticle.Command>(article);
            command.Title = "NewTag";

            var sut = new AlterFeedArticle.Handler(Tests.SessionFactory, Tests.Mapper);

            var result = await sut.Handle(command, new CancellationToken());

            Assert.IsNotNull(result);

            using (var session = Tests.SessionFactory.QuerySession())
            {
                var modArticle = session.Query<FeedArticle>().First(q => q.Id == testArticle.Id);
                Assert.IsNotNull(modArticle);
                Assert.AreEqual(command.Title, modArticle.Title);
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

            Assert.IsNotNull(result);

            using (var session = Tests.SessionFactory.QuerySession())
            {
                var modArticle = session.Query<FeedArticle>().First(q => q.Title == "Mic Man");
                Assert.IsNotNull(modArticle);
                Assert.AreEqual(command.Title, modArticle.Title);
            }

        }
    }
}
