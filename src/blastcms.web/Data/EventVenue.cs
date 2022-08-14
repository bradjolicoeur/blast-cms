using System;

namespace blastcms.web.Data
{
    public class EventVenue
    {
        public Guid Id { get; set; }
        public string VenueName { get; set; }
        public string Address { get; set; } 
        public string City { get; set; }    
        public string State { get; set; }
        public string Zip { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public string WebsiteUrl { get; set; }
        public ImageFile Image { get; set; }
        public string Slug { get; set; }

    }
}
