namespace CinePrime.DAL.Entities
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = "snacks";
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public int MinStockAlert { get; set; } = 5;
        public string Status { get; set; } = "active";
    }
}
