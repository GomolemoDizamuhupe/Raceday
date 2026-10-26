using System.ComponentModel.DataAnnotations;

namespace Part3_Raceday.Models
{
    public class EventFormViewModel
    {
        public int EventId { get; set; }

        [Required] public string Name { get; set; } = "";
        public string? Description { get; set; }

        [Required, DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Event date")]
        public DateTime EventDate { get; set; } = DateTime.Today.AddDays(30);

        [Required] public string Location { get; set; } = "";

        [Display(Name = "Event type")]
        public string? EventType { get; set; }

        [Display(Name = "Distance (km)"), Range(0.1, 1000)]
        public decimal? DistanceKm { get; set; }
    }
}