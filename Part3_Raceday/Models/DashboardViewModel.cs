namespace Part3_Raceday.Models
{
    public class DashboardRow
    {
        public ApiEvent Event { get; set; } = new();
        public int Enrolments { get; set; }
    }

    public class DashboardViewModel
    {
        public int TotalEvents { get; set; }
        public int UpcomingEvents { get; set; }
        public int TotalEnrolments { get; set; }
        public List<DashboardRow> Rows { get; set; } = new();
        public string? Error { get; set; }
    }
}