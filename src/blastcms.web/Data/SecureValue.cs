﻿using System;

namespace blastcms.web.Data
{
    public class SecureValue
    {
        public string Id { get; set; }
        public bool Expired { get; set; }
        public string Display { get; set; }
        public bool Readonly { get; set; } = true;
        public DateTime Created { get; set; } = DateTime.UtcNow;
    }
}
