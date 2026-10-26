namespace Part2_Raceday.Models
{
    public class ParticipantResultDto
    {
        public int ResultId { get; set; }
        public int EntryId { get; set; }
        public string EventName { get; set; } = "";
        public DateTime EventDate { get; set; }
        public string CategoryName { get; set; } = "";
        public decimal DistanceKm { get; set; }
        public TimeSpan FinishTime { get; set; }
        public int? Position { get; set; }
        public int TotalFinishers { get; set; }
    }
}