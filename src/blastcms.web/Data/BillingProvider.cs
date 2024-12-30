using System;
using System.Collections.Generic;
using System.Linq;

namespace blastcms.web.Data
{
    public class BillingProvider : Enumeration
    {
        public static BillingProvider None { get; } = new BillingProvider(0, "None");
        public static BillingProvider Paddle { get; } = new BillingProvider(1, "Paddle");

        public string Name { get; private set; }
        public new int Value { get; private set; }

        public BillingProvider(int value, string name)
        {
            Value = value;
            Name = name;
        }

        public static IEnumerable<BillingProvider> List()
        {
            return new[] { None, Paddle };
        }

        public static BillingProvider FromName(string name)
        {
            return List().Single(r => String.Equals(r.Name, name, StringComparison.OrdinalIgnoreCase));
        }

        public static BillingProvider FromValue(int value)
        {
            return List().Single(r => r.Value == value);
        }
    }
}
