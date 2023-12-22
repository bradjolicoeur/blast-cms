using System;
using System.Collections.Generic;
using blastcms.web.Helpers;

namespace blastcms.web.Data
{
    public class ImageFile
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public HashSet<String> Tags { get; set; }
        public string Description { get; set; }
        public string ImageStorageName { get; set; } 
        public string ImageUrl { get; set; }
        public string ResolvedUrl { get => ImageUrl.GetFullImageUrl(); }
    }
}
