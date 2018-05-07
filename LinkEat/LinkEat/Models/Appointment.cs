using System;

namespace LinkEat.Models
{
    public class Appointment
    {
        public int Id { get; set; }
        public int PlaceId { get; set; }
        public int UserId { get; set; }
        public DateTime Date { get; set; }
        public bool Reminded { get; set; }
        public bool RemindedDepart { get; set; }
    }
}
