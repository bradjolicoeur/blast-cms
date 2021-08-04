using System;
using System.Collections.Generic;

namespace blastcms.web.Data
{
    public class LandingPage
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public HashSet<String> Tags { get; set; }
        public string Description { get; set; }
        public string HeroTitle { get; set; }
        public string HeroPhrase { get; set; }
        public string HeroImageUrl { get; set; }
        public string Body { get; set; }
        public string Slug { get; set; }
    }
}
