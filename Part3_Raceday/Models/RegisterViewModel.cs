using System.ComponentModel.DataAnnotations;

namespace Part3_Raceday.Models
{
    public class RegisterViewModel
    {
        [Required] public string Name { get; set; } = "";
        [Required] public string Surname { get; set; } = "";

        [Required, EmailAddress]
        public string Email { get; set; } = "";

        [Required, MinLength(6), DataType(DataType.Password)]
        public string Password { get; set; } = "";

        [Required]
        public string Role { get; set; } = "participant";
    }
}