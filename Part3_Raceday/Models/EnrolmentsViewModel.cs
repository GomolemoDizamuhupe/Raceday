namespace Part3_Raceday.Models
{
    public class EnrolmentsViewModel
    {
        public ApiEvent Event { get; set; } = new();
        public List<ApiEventEntry> Entries { get; set; } = new();
        public string? Category { get; set; }
        public List<string> Categories { get; set; } = new();
    }
}