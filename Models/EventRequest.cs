namespace Part2_Raceday.Models
{
    public class EventRequest
    {
        public int OrganiserId { get; set; }
        public string Name { get; set; } = "";
        public string? Description { get; set; }
        public DateTime EventDate { get; set; }
        public string Location { get; set; } = "";
        public string? EventType { get; set; }
        public decimal? DistanceKm { get; set; }
    }
}