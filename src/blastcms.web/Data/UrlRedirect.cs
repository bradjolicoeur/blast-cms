using System;

namespace blastcms.web.Data
{
    public class UrlRedirect
    {
        public Guid Id { get; set; }
        public string RedirectFrom { get; set; }
        public string RedirectTo { get; set; }
        public bool Permanent { get; set; }

    }

   
}
