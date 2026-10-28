namespace Part3_Raceday.Models
{
    public class ApiRoute
    {
        public int RouteId { get; set; }
        public int CategoryId { get; set; }
        public string StartPoint { get; set; } = "";
        public string EndPoint { get; set; } = "";
        public int? ElevationGainM { get; set; }
        public string? MapUrl { get; set; }
    }
}