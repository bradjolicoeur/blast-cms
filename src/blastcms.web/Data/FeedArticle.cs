using System;
using System.Collections.Generic;

namespace blastcms.web.Data
{
    public class FeedArticle
    {
        public Guid Id { get; set; }
        public string Slug { get; set; }
        public HashSet<String> Tags { get; set; }
        public string KeyWords { get; set; }
        public string Title { get; set; }
        public string ArticleUrl { get; set; }
        public string ImageUrl { get; set; }
        public string Author { get; set; }
        public string Description { get; set; }
        public string Notes { get; set; }
        public string SiteName { get; set; }
        public DateTime DatePosted { get; set; }
    }
}
