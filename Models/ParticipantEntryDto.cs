namespace Part2_Raceday.Models
{
    public class ParticipantEntryDto
    {
        public int EntryId { get; set; }
        public int CategoryId { get; set; }
        public DateTime EntryDate { get; set; }
        public string Status { get; set; } = "";
        public int EventId { get; set; }
        public string EventName { get; set; } = "";
        public DateTime EventDate { get; set; }
        public string CategoryName { get; set; } = "";
        public decimal DistanceKm { get; set; }
    }
}