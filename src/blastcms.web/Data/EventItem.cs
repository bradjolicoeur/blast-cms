using System;
using System.Collections.Generic;
using System.Linq;

namespace blastcms.web.Data
{
    public class EventItem
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public string Summary { get; set; }
        public string Special { get; set; }
        public Guid VenueId { get; set; }
        public string TicketPrice { get; set; }
        public ImageFile Flyer { get; set; }
        public OpenMicOption OpenMicSignup { get; set; }
        public DateTime? EventDate { get; set; }
        public TimeSpan? EventTime { get; set; }
        public string Sponsor { get; set; }
        public TicketSaleProvider TicketSaleProvider { get; set; }
        public string TicketSaleValue { get; set; }
        public string VenueTicketsUrl { get; set; }
        public string Slug { get; set; }

    }

    public class TicketSaleProvider : Enumeration
    {
        public static TicketSaleProvider None { get; } = new TicketSaleProvider(0, "None", "N/A");
        public static TicketSaleProvider ShowSlinger { get; } = new TicketSaleProvider(1, "Showslinger", "ShowId");
        public static TicketSaleProvider VenueTicket { get; } = new TicketSaleProvider(2, "VenueTicket", "N/A");
        public static TicketSaleProvider Squadup { get; } = new TicketSaleProvider(3, "Squadup", "SquadupId");

        public string Name { get; private set; }
        public int Value { get; private set; }

        /// <summary>
        /// the name of the identifier the ticket sale provider uses
        /// </summary>
        public string ValueLabel { get; private set; }

        public TicketSaleProvider(int value, string name, string valuelabel)
        {
            Value = value;
            Name = name;
            ValueLabel = valuelabel;
        }

        public static IEnumerable<TicketSaleProvider> List()
        {
            return new[] {None, ShowSlinger };
        }

        public static TicketSaleProvider FromName(string name)
        {
            return List().Single(r => String.Equals(r.Name, name, StringComparison.OrdinalIgnoreCase));
        }

        public static TicketSaleProvider FromValue(int value)
        {
            return List().Single(r => r.Value == value);
        }
    }

    public class OpenMicOption : Enumeration
    {

        public static OpenMicOption HideForm { get; } = new OpenMicOption(0, "Hide Form");
        public static OpenMicOption ShowForm { get; } = new OpenMicOption(1, "Show Form");
        public static OpenMicOption ShowFull { get; } = new OpenMicOption(2, "Open Mic Full");

        public string Name { get; private set; }
        public int Value { get; private set; }

        public OpenMicOption(int value, string name)
        {
            Value = value;
            Name = name;
        }

        public static IEnumerable<OpenMicOption> List()
        {
            return new[] { HideForm, ShowForm, ShowFull };
        }

        public static OpenMicOption FromName(string name)
        {
            return List().Single(r => String.Equals(r.Name, name, StringComparison.OrdinalIgnoreCase));
        }

        public static OpenMicOption FromValue(int value)
        {
            return List().Single(r => r.Value == value);
        }
    }
}
