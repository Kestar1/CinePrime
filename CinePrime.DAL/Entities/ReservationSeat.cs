namespace CinePrime.DAL.Entities
{
    public class ReservationSeat
    {
        public int Id { get; set; }
        public int ReservationId { get; set; }
        public int SeatRow { get; set; }
        public int SeatNumber { get; set; }
        public string SeatType { get; set; } = "standard";
    }
}
