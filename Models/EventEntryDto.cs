namespace Part2_Raceday.Models
{
    public class EventEntryDto
    {
        public int EntryId { get; set; }
        public int ParticipantId { get; set; }
        public string ParticipantName { get; set; } = "";
        public string Email { get; set; } = "";
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = "";
        public DateTime EntryDate { get; set; }
        public string Status { get; set; } = "";
    }
}