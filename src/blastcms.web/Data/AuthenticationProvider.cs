using System;
using System.Collections.Generic;
using System.Linq;

namespace blastcms.web.Data
{
    public class AuthenticationProvider
    {
        public static AuthenticationProvider None { get; } = new AuthenticationProvider(0, "None");
        public static AuthenticationProvider FusionAuth { get; } = new AuthenticationProvider(1, "FusionAuth");

        public string Name { get; private set; }
        public new int Value { get; private set; }

        public AuthenticationProvider(int value, string name)
        {
            Value = value;
            Name = name;
        }

        public static IEnumerable<AuthenticationProvider> List()
        {
            return new[] { None, FusionAuth };
        }

        public static AuthenticationProvider FromName(string name)
        {
            return List().Single(r => String.Equals(r.Name, name, StringComparison.OrdinalIgnoreCase));
        }

        public static AuthenticationProvider FromValue(int value)
        {
            return List().Single(r => r.Value == value);
        }
    }
}
