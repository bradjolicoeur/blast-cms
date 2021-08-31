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
        public static IEnumerable<UrlRedirect> GeneratedUrlRedirects;
        public static IEnumerable<SitemapItem> GeneratedSitemapItems;

        public static void InitializeDatabase(this DocumentStore documentStore)
        {
            LoadBlogArticles(documentStore);
            LoadContentTags(documentStore);
            LoadLandingPages(documentStore);
            LoadFeedArticless(documentStore);
            LoadUrlRedirects(documentStore);
            LoadSitemapItems(documentStore);
        }

        private static void LoadBlogArticles(DocumentStore documentStore)
        {
            GeneratedBlogArticles = Builder<BlogArticle>.CreateListOfSize(100)
                .Build();

            GeneratedBlogArticles.OrderBy(o => o.Title).Last().Tags = new HashSet<string> { "POP", "XX" };

            documentStore.BulkInsert("test-tenant-1", GeneratedBlogArticles.ToArray(), BulkInsertMode.InsertsOnly, 100);

        }

        private static void LoadContentTags(DocumentStore documentStore)
        {

            GeneratedContentTags = Builder<ContentTag>.CreateListOfSize(100)
                .Build();

            documentStore.BulkInsert("test-tenant-1", GeneratedContentTags.ToArray(), BulkInsertMode.InsertsOnly, 100);
        }

        private static void LoadLandingPages(DocumentStore documentStore)
        {

            GeneratedLandingPages = Builder<LandingPage>.CreateListOfSize(100)
                .Build();

            documentStore.BulkInsert("test-tenant-1", GeneratedLandingPages.ToArray(), BulkInsertMode.InsertsOnly, 100);
        }

        private static void LoadFeedArticless(DocumentStore documentStore)
        {

            GeneratedFeedArticles = Builder<FeedArticle>.CreateListOfSize(100)
                .Build();

            documentStore.BulkInsert("test-tenant-1", GeneratedFeedArticles.ToArray(), BulkInsertMode.InsertsOnly, 100);
        }

        private static void LoadUrlRedirects(DocumentStore documentStore)
        {

            GeneratedUrlRedirects = Builder<UrlRedirect>.CreateListOfSize(100)
                .Build();

            documentStore.BulkInsert("test-tenant-1", GeneratedUrlRedirects.ToArray(), BulkInsertMode.InsertsOnly, 100);
        }
        private static void LoadSitemapItems(DocumentStore documentStore)
        {

            GeneratedSitemapItems = Builder<SitemapItem>.CreateListOfSize(100)
                .Build();

            documentStore.BulkInsert("test-tenant-1", GeneratedSitemapItems.ToArray(), BulkInsertMode.InsertsOnly, 100);
        }

    }
}
