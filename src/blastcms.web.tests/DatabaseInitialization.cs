using Marten;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FizzWare.NBuilder;
using Faker;
using blastcms.web.Data;

namespace blastcms.web.tests
{
    internal static class DatabaseInitialization
    {
        public static IEnumerable<BlogArticle> GeneratedBlogArticles;
        public static void InitializeDatabase(this DocumentStore documentStore)
        {
            LoadBlogArticles(documentStore);
        }

        private static void LoadBlogArticles(DocumentStore documentStore)
        {
            GeneratedBlogArticles = Builder<BlogArticle>.CreateListOfSize(100)
                .Build();

            documentStore.BulkInsert(GeneratedBlogArticles.ToArray(), BulkInsertMode.InsertsOnly, 100);

        }

    }
}
