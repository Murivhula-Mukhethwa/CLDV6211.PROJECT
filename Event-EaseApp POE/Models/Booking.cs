namespace Event_EaseApp.Models
{

    public class Booking

    {

        public int BookingID { get; set; }

        public int EventID { get; set; }

        public int VenueID { get; set; }

        public DateTime BookingDate { get; set; }

        public Event? Event { get; set; } // Navigation property 

        public Venue? Venue { get; set; } // Navigation property 



    }
}