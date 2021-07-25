using blastcms.web.Data;
using blastcms.web.Handlers;
using FizzWare.NBuilder;
using NUnit.Framework;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace blastcms.web.tests.Handlers
{
    public class ContentTagHandlers
    {
        [Test]
        public void GetContentTags()
        {
            using (var session = Tests.Store.QuerySession())
            {
                var data = session.Query<ContentTag>().Count();
                Assert.IsTrue(data >= 100);
            }
        }

        [Test]
        public void GetContentTag()
        {

            var testArticle = DatabaseInitialization.GeneratedContentTags.Skip(2).First();

            using (var session = Tests.Store.QuerySession())
            {
                var article = session.Query<ContentTag>().First(q => q.Id == testArticle.Id);
                Assert.IsNotNull(article);
                Assert.AreEqual(testArticle.Value, article.Value);
            }
        }

        [Test]
        public async Task AlterContentTag_update()
        {
            var testArticle = DatabaseInitialization.GeneratedContentTags.Skip(10).First();

            ContentTag article = null;
            using (var session = Tests.Store.QuerySession())
            {
                article = session.Query<ContentTag>().First(q => q.Id == testArticle.Id);
                Assert.IsNotNull(article);
            }

            var command = Tests.Mapper.Map<AlterContentTag.Command>(article);
            command.Value = "NewTag";

            var sut = new AlterContentTag.Handler(Tests.SessionFactory, Tests.Mapper);

            var result = await sut.Handle(command, new CancellationToken());

            Assert.IsNotNull(result);

            using (var session = Tests.Store.QuerySession())
            {
                var modArticle = session.Query<ContentTag>().First(q => q.Id == testArticle.Id);
                Assert.IsNotNull(modArticle);
                Assert.AreEqual(command.Value, modArticle.Value);
            }

        }

        [Test]
        public async Task AlterContentTag_insert()
        {
            var command = Builder<AlterContentTag.Command>.CreateNew()
                .With(p => p.Id = null)
                .With(p => p.Value = "Mic Man")
                .Build();


            var sut = new AlterContentTag.Handler(Tests.SessionFactory, Tests.Mapper);

            var result = await sut.Handle(command, new CancellationToken());

            Assert.IsNotNull(result);

            using (var session = Tests.Store.QuerySession())
            {
                var modArticle = session.Query<ContentTag>().First(q => q.Value == "Mic Man");
                Assert.IsNotNull(modArticle);
                Assert.AreEqual(command.Value, modArticle.Value);
            }

        }
    }
}
