namespace Part3_Raceday.Models
{
    public class ApiEventResult
    {
        public int ResultId { get; set; }
        public int EntryId { get; set; }
        public string ParticipantName { get; set; } = "";
        public string CategoryName { get; set; } = "";
        public TimeSpan FinishTime { get; set; }
        public int? Position { get; set; }
    }

    public class ResultRow
    {
        public int EntryId { get; set; }
        public string ParticipantName { get; set; } = "";
        public string CategoryName { get; set; } = "";
        public int? ResultId { get; set; }
        public string FinishTime { get; set; } = "";
        public int? Position { get; set; }
    }

    public class ResultsEntryViewModel
    {
        public ApiEvent Event { get; set; } = new();
        public bool EventHasHappened { get; set; }
        public List<ResultRow> Rows { get; set; } = new();
    }
}