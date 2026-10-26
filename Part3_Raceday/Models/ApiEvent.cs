namespace Part3_Raceday.Models
{
    public class ApiEvent
    {
        public int EventId { get; set; }
        public int OrganiserId { get; set; }
        public string Name { get; set; } = "";
        public string? Description { get; set; }
        public DateTime EventDate { get; set; }
        public string Location { get; set; } = "";
        public string? EventType { get; set; }
        public decimal? DistanceKm { get; set; }
        public string? BannerUrl { get; set; }
    }
}