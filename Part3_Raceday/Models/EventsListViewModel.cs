namespace Part3_Raceday.Models
{
    public class EventsListViewModel
    {
        public List<ApiEvent> Events { get; set; } = new();
        public List<string> Types { get; set; } = new();
        public string? Search { get; set; }
        public string? Type { get; set; }
        public string? Location { get; set; }
        public string? Error { get; set; }
    }
}