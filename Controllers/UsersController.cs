using Microsoft.AspNetCore.Mvc;
using Race_Day.database;

namespace Race_Day.Controllers
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

    }
}