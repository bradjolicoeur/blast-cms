using System;

namespace blastcms.web.Data
{
    public class EmailTemplate
    {
        public Guid Id { get; set; }
        public string FromAddress { get; set; }
        public string Name { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
    }
}
