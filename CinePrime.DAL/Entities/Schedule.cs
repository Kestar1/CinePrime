using System;

namespace CinePrime.DAL.Entities
{
    public class Schedule
    {
        public int Id { get; set; }
        public int MovieId { get; set; }
        public int HallId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public decimal TicketPrice { get; set; }
        public decimal VipPrice { get; set; }
        public string Status { get; set; } = "scheduled";
    }
}
