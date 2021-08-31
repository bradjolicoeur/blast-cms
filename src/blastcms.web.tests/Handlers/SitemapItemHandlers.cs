using blastcms.web.Data;
using blastcms.web.Handlers;
using FizzWare.NBuilder;
using NUnit.Framework;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace blastcms.web.tests.Handlers
{
    public class SitemapItemHandlers
    {
        [Test]
        public void GetSitemapItems()
        {
            using (var session = Tests.SessionFactory.QuerySession())
            {
                var data = session.Query<SitemapItem>().Count();
                Assert.IsTrue(data >= 100);
            }
        }

        [Test]
        [TestCase(1, null, 10, 100, Description = "First Page no search")]
        [TestCase(2, null, 10, 100, Description = "Second Page no search")]
        [TestCase(1, "RelativePath12", 1, 1, Description = "First Page with search match")]
        [TestCase(1, "empty", 0, 0, Description = "First Page with search no match")]
        public async Task GetSitemapItems_handler(int page, string search, int expectedCount, int expectedTotal)
        {
            var command = new GetSitemapItems.Query(0, 10, page, search);

            var sut = new GetSitemapItems.Handler(Tests.SessionFactory);

            var result = await sut.Handle(command, new CancellationToken());

            Assert.IsNotNull(result);
            Assert.AreEqual(expectedCount, result.Data.Count());
            Assert.GreaterOrEqual(result.Count, expectedTotal);
            Assert.AreEqual(page, result.Page);
        }


        [Test]
        public async Task GetSitemapItem()
        {

            var testArticle = DatabaseInitialization.GeneratedSitemapItems.Skip(2).First();

            var command = new GetSitemapItem.Query(testArticle.Id);

            var sut = new GetSitemapItem.Handler(Tests.SessionFactory);

            var result = await sut.Handle(command, new CancellationToken());

            Assert.IsNotNull(result);
            Assert.AreEqual(result.Data.RelativePath, result.Data.RelativePath);
        }

        [Test]
        public async Task AlterSitemapItem_update()
        {
            var testArticle = DatabaseInitialization.GeneratedSitemapItems.Skip(10).First();

            SitemapItem article = null;
            using (var session = Tests.SessionFactory.QuerySession())
            {
                article = session.Query<SitemapItem>().First(q => q.Id == testArticle.Id);
                Assert.IsNotNull(article);
            }

            var command = Tests.Mapper.Map<AlterSitemapItem.Command>(article);
            command.RelativePath = "NewTag";

            var sut = new AlterSitemapItem.Handler(Tests.SessionFactory, Tests.Mapper);

            var result = await sut.Handle(command, new CancellationToken());

            Assert.IsNotNull(result);

            using (var session = Tests.SessionFactory.QuerySession())
            {
                var modArticle = session.Query<SitemapItem>().First(q => q.Id == testArticle.Id);
                Assert.IsNotNull(modArticle);
                Assert.AreEqual(command.RelativePath, modArticle.RelativePath);
                Assert.AreEqual(command.Id, modArticle.Id);
            }

        }

        [Test]
        public async Task AlterSitemapItem_insert()
        {
            var command = Builder<AlterSitemapItem.Command>.CreateNew()
                .With(p => p.Id = null)
                .With(p => p.RelativePath = "Mic Man")
                .Build();


            var sut = new AlterSitemapItem.Handler(Tests.SessionFactory, Tests.Mapper);

            var result = await sut.Handle(command, new CancellationToken());

            Assert.IsNotNull(result);

            using (var session = Tests.SessionFactory.QuerySession())
            {
                var modArticle = session.Query<SitemapItem>().First(q => q.RelativePath == "Mic Man");
                Assert.IsNotNull(modArticle);
                Assert.AreEqual(command.RelativePath, modArticle.RelativePath);
            }

        }
    }
}
