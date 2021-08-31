using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace blastcms.web.Data
{
    public class SitemapItem
    {
        public Guid Id { get; set; }
        public string RelativePath { get; set; }
        public DateTime LastModified { get; set; }
    }
}
