using System.ComponentModel.DataAnnotations;

namespace Part3_Raceday.Models
{
    public class RouteFormViewModel
    {
        public int RouteId { get; set; }
        public int CategoryId { get; set; }
        public int EventId { get; set; }

        [Required, StringLength(150), Display(Name = "Start point")]
        public string StartPoint { get; set; } = "";

        [Required, StringLength(150), Display(Name = "End point")]
        public string EndPoint { get; set; } = "";

        [Range(0, 10000), Display(Name = "Elevation gain (m)")]
        public int? ElevationGainM { get; set; }

        [Url, StringLength(2000), Display(Name = "Map URL")]
        public string? MapUrl { get; set; }
    }
}