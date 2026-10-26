using System.ComponentModel.DataAnnotations;

namespace Part3_Raceday.Models
{
    public class CategoryFormViewModel
    {
        public int CategoryId { get; set; }
        public int EventId { get; set; }

        [Required] public string Name { get; set; } = "";

        [Required, Range(0.1, 1000), Display(Name = "Distance (km)")]
        public decimal DistanceKm { get; set; }

        [Required, Range(1, 100000), Display(Name = "Max participants")]
        public int MaxParticipants { get; set; } = 500;
    }
}