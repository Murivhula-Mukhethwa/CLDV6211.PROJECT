﻿namespace Event_EaseApp.Models
{
    public class Event

    {

        public int EventID { get; set; }

        public string EventName { get; set; }

        public DateTime EventDate { get; set; }

        public string Description { get; set; }

        public int VenueID { get; set; }

        public Venue? Venue { get; set; } // Navigation property 

    }
}