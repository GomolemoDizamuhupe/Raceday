namespace Part2_Raceday.Models
{
    public class CategoryRequest
    {
        public int OrganiserId { get; set; }
        public int EventId { get; set; }
        public string Name { get; set; } = "";
        public decimal DistanceKm { get; set; }
        public int MaxParticipants { get; set; } = 500;
    }
}