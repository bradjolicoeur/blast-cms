using System;
using System.Collections.Generic;

namespace blastcms.web.Data
{
    public class PodcastEpisode
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public HashSet<String> Tags { get; set; }
        public DateTime PublishedDate { get; set; }
        public ImageFile Image { get; set; }
        public string Summary { get; set; }
        public string Content { get; set; }
        public int Episode { get; set; }
        public string Duration { get; set; }
        public string Mp3Url { get; set; }
        public string YouTubeUrl { get; set; }
        public string Slug { get; set; }
    }
}
