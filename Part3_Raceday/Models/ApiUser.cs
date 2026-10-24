namespace Part3_Raceday.Models
{
    public class ApiUser
    {
        public int UserId { get; set; }
        public string Name { get; set; } = "";
        public string Surname { get; set; } = "";
        public string Email { get; set; } = "";
        public string Role { get; set; } = "";
        public string? ProfilePictureUrl { get; set; }
    }
}