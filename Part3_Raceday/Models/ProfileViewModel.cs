using System.ComponentModel.DataAnnotations;

namespace Part3_Raceday.Models
{
    public class ProfileViewModel
    {
        [Required] public string Name { get; set; } = "";
        [Required] public string Surname { get; set; } = "";

        [Required, EmailAddress]
        public string Email { get; set; } = "";

        public string Role { get; set; } = "";
        public string? ProfilePictureUrl { get; set; }
    }
}