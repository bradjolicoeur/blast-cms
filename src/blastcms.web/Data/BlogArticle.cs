using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace blastcms.web.Data
{
    public class BlogArticle
    {
        public Guid Id { get; set; }
        public string Title { get; set; } 
        public string Author { get; set; }
        public HashSet<String> Tags { get; set; }
        public DateTime PublishedDate { get; set; }
        public string ImageUrl { get; set; }
        public string Description { get; set; }
        public string Body { get; set; }
        public string Slug { get; set; }

    }
}
