namespace Part3_Raceday.Models
{
    public class ApiCategory
    {
        public int CategoryId { get; set; }
        public int EventId { get; set; }
        public string Name { get; set; } = "";
        public decimal DistanceKm { get; set; }
        public int MaxParticipants { get; set; }
    }
}