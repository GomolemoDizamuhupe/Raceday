namespace Part2_Raceday.Models
{
    public class EventResultDto
    {
        public int ResultId { get; set; }
        public int EntryId { get; set; }
        public string ParticipantName { get; set; } = "";
        public string CategoryName { get; set; } = "";
        public TimeSpan FinishTime { get; set; }
        public int? Position { get; set; }
    }
}