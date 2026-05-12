using System;

namespace CinePrime.DAL.Entities
{
    public class Sale
    {
        public int Id { get; set; }
        public string SaleType { get; set; } = string.Empty;
        public int? ReservationId { get; set; }
        public decimal TotalAmount { get; set; }
        public string PaymentMethod { get; set; } = "cash";
        public int CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
