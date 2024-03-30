using System;
using System.Collections.Generic;

namespace blastcms.web.Data
{
    public class ContentBlock
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public HashSet<String> Groups { get; set; }
        public ImageFile Image { get; set; }
        public string Body { get; set; }
        public string Slug { get; set; }
    }
}
