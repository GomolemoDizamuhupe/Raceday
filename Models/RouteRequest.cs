namespace Part2_Raceday.Models
{
    public class RouteRequest
    {
        public int OrganiserId { get; set; }
        public int CategoryId { get; set; }
        public string StartPoint { get; set; } = "";
        public string EndPoint { get; set; } = "";
        public int? ElevationGainM { get; set; }
        public string? MapUrl { get; set; }
    }
}