namespace Part2_Raceday.Models
{
    public class RouteDto
    {
        public int RouteId { get; set; }
        public int CategoryId { get; set; }
        public string StartPoint { get; set; } = "";
        public string EndPoint { get; set; } = "";
        public int? ElevationGainM { get; set; }
        public string? MapUrl { get; set; }
    }
}