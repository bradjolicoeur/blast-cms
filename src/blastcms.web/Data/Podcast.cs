using System;

namespace blastcms.web.Data
{
    public class Podcast
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime PublishedDate { get; set; }
        public string PodcastUrl { get; set; }
        public string RssCategory { get; set; }
        public string RssSubcategory { get; set; }
        public ImageFile CoverImage { get; set; }
        public string OwnerName { get; set; }
        public string OwnerEmail { get; set; }
        public bool ExplicitContent { get; set; }
        public string Slug { get; set; }
    }
}
