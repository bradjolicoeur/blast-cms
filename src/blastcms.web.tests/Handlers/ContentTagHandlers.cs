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
    public class ContentTagHandlers
    {
        [Test]
        public void GetContentTags()
        {
            using (var session = Tests.SessionFactory.QuerySession())
            {
                var data = session.Query<ContentTag>().Count();
                ClassicAssert.IsTrue(data >= 100);
            }
        }

        [Test]
        public void GetContentTag()
        {

            var testArticle = DatabaseInitialization.GeneratedContentTags.Skip(2).First();

            using (var session = Tests.SessionFactory.QuerySession())
            {
                var article = session.Query<ContentTag>().First(q => q.Id == testArticle.Id);
                ClassicAssert.IsNotNull(article);
                ClassicAssert.AreEqual(testArticle.Value, article.Value);
            }
        }

        [Test]
        public async Task AlterContentTag_update()
        {
            var testArticle = DatabaseInitialization.GeneratedContentTags.Skip(10).First();

            ContentTag article = null;
            using (var session = Tests.SessionFactory.QuerySession())
            {
                article = session.Query<ContentTag>().First(q => q.Id == testArticle.Id);
                ClassicAssert.IsNotNull(article);
            }

            var command = Tests.Mapper.Map<AlterContentTag.Command>(article);
            command.Value = "NewTag";

            var sut = new AlterContentTag.Handler(Tests.SessionFactory, Tests.Mapper);

            var result = await sut.Handle(command, new CancellationToken());

            ClassicAssert.IsNotNull(result);

            using (var session = Tests.SessionFactory.QuerySession())
            {
                var modArticle = session.Query<ContentTag>().First(q => q.Id == testArticle.Id);
                ClassicAssert.IsNotNull(modArticle);
                ClassicAssert.AreEqual(command.Value, modArticle.Value);
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

            ClassicAssert.IsNotNull(result);

            using (var session = Tests.SessionFactory.QuerySession())
            {
                var modArticle = session.Query<ContentTag>().First(q => q.Value == "Mic Man");
                ClassicAssert.IsNotNull(modArticle);
                ClassicAssert.AreEqual(command.Value, modArticle.Value);
            }

        }
    }
}
