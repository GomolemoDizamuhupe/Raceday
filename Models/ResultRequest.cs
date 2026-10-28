namespace Part2_Raceday.Models
{
    public class ResultRequest
    {
        public int OrganiserId { get; set; }
        public TimeSpan FinishTime { get; set; }
        public int? Position { get; set; }
    }
}