using Marten;
using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using blastcms.web.Data;

namespace blastcms.web.tests
{
    internal static class DatabaseInitialization
    {
        public static IEnumerable<BlogArticle> GeneratedBlogArticles;
        public static IEnumerable<ContentTag> GeneratedContentTags;
        public static IEnumerable<LandingPage> GeneratedLandingPages;
        public static IEnumerable<FeedArticle> GeneratedFeedArticles;

        public static void InitializeDatabase(this DocumentStore documentStore)
        {
            LoadBlogArticles(documentStore);
            LoadContentTags(documentStore);
            LoadLandingPages(documentStore);
            LoadFeedArticless(documentStore);
        }

        private static void LoadBlogArticles(DocumentStore documentStore)
        {
            GeneratedBlogArticles = Builder<BlogArticle>.CreateListOfSize(100)
                .Build();

            documentStore.BulkInsert(GeneratedBlogArticles.ToArray(), BulkInsertMode.InsertsOnly, 100);

        }

        private static void LoadContentTags(DocumentStore documentStore)
        {

            GeneratedContentTags = Builder<ContentTag>.CreateListOfSize(100)
                .Build();

            documentStore.BulkInsert(GeneratedContentTags.ToArray(), BulkInsertMode.InsertsOnly, 100);
        }

        private static void LoadLandingPages(DocumentStore documentStore)
        {

            GeneratedLandingPages = Builder<LandingPage>.CreateListOfSize(100)
                .Build();

            documentStore.BulkInsert(GeneratedLandingPages.ToArray(), BulkInsertMode.InsertsOnly, 100);
        }

        private static void LoadFeedArticless(DocumentStore documentStore)
        {

            GeneratedFeedArticles = Builder<FeedArticle>.CreateListOfSize(100)
                .Build();

            documentStore.BulkInsert(GeneratedFeedArticles.ToArray(), BulkInsertMode.InsertsOnly, 100);
        }

    }
}
