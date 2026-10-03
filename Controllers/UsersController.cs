using Microsoft.AspNetCore.Mvc;
using Part2_Raceday.Backend;

namespace Part2_Raceday
{
    [Route("api/[controller]")]
    [ApiController]
    public class users_controller : ControllerBase
    {
        [HttpGet("me")]
        public string get_own_profile(int userId)
        {
            Users users = new Users();

            return users.get_own_profile(userId);
        }

        [HttpPut("me")]
        public string update_own_profile(int userId, string name, string surname, string email)
        {
            Users users = new Users();

            return users.update_own_profile(userId, name, surname, email);
        }

        [HttpGet("{userId}")]
        public string get_user_by_id(int userId)
        {
            Users users = new Users();

            return users.get_user_by_id(userId);
        }
    }
}