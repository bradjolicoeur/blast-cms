using System;

namespace blastcms.web.Data
{
    public class LandingPage
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string HeroTitle { get; set; }
        public string HeroPhrase { get; set; }
        public string HeroImageUrl { get; set; }
        public string Body { get; set; }
        public string Slug { get; set; }
    }
}
