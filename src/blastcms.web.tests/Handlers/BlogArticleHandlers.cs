using blastcms.web.Data;
using blastcms.web.Handlers;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FizzWare.NBuilder;
using Marten.Linq;

namespace blastcms.web.tests.Handlers
{
    public class BlogArticleHandlers
    {
        [Test]
        public void GetBlogArticles()
        {
            using (var session = Tests.Store.QuerySession())
            {
                var data = session.Query<BlogArticle>().Count();
                Assert.IsTrue(data >= 100);
            }
        }

        [Test]
        public void GetBlogArticle()
        {

            var testArticle = DatabaseInitialization.GeneratedBlogArticles.Skip(2).First();

            using (var session = Tests.SessionFactory.QuerySession())
            {
                var article = session.Query<BlogArticle>().First(q => q.Id == testArticle.Id);
                Assert.IsNotNull(article);
                Assert.AreEqual(testArticle.Title, article.Title);
            }
        }

        [Test]
        public async Task AlterBlogArticle_update()
        {
            var testArticle = DatabaseInitialization.GeneratedBlogArticles.Skip(10).First();

            BlogArticle article = null;
            using (var session = Tests.SessionFactory.QuerySession())
            {
                article = session.Query<BlogArticle>().First(q => q.Id == testArticle.Id);
                Assert.IsNotNull(article);
            }

            var command = Tests.Mapper.Map<AlterBlogArticle.Command>(article);
            command.Author = "New Author";

            var sut = new AlterBlogArticle.Handler(Tests.SessionFactory, Tests.Mapper);

            var result = await sut.Handle(command, new CancellationToken());

            Assert.IsNotNull(result);

            using (var session = Tests.Store.QuerySession())
            {
                var modArticle = session.Query<BlogArticle>().First(q => q.Id == testArticle.Id);
                Assert.IsNotNull(modArticle);
                Assert.AreEqual(command.Author, modArticle.Author);
            }

        }

        [Test]
        public async Task AlterBlogArticle_insert()
        {
            var command = Builder<AlterBlogArticle.Command>.CreateNew()
                .With(p => p.Id = null)
                .With(p => p.Author = "Mic Man")
                .With(p => p.Title = "Unique Title")
                .Build();


            var sut = new AlterBlogArticle.Handler(Tests.SessionFactory, Tests.Mapper);

            var result = await sut.Handle(command, new CancellationToken());

            Assert.IsNotNull(result);

            using (var session = Tests.Store.QuerySession())
            {
                var modArticle = session.Query<BlogArticle>().First(q => q.Author == "Mic Man");
                Assert.IsNotNull(modArticle);
                Assert.AreEqual(command.Author, modArticle.Author);
            }

        }
    }
}
