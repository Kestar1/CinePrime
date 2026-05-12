namespace CinePrime.DAL.Entities
{
    public class Hall
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Capacity { get; set; }
        public int RowsCount { get; set; }
        public int SeatsPerRow { get; set; }
        public string HallType { get; set; } = "standard";
        public string Status { get; set; } = "active";
    }
}
