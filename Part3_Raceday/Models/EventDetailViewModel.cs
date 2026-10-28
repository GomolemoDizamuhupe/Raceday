namespace Part3_Raceday.Models
{
    public class EventDetailViewModel
    {
        public ApiEvent Event { get; set; } = new();
        public List<ApiCategory> Categories { get; set; } = new();
        public Dictionary<int, ApiRoute> Routes { get; set; } = new();
    }
}