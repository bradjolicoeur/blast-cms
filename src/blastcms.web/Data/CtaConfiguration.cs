using System;

namespace blastcms.web.Data
{
    public class CtaConfiguration
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Slug { get; set; }
        public string FromEmail { get; set; }
        public string AdminEmail { get; set; }
        public Guid? AdminEmailTemplateId { get; set; }
        public Guid? SubmitterEmailTemplateId { get; set; }
    }
}
