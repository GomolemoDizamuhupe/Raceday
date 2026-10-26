using Microsoft.AspNetCore.Mvc;
using Part2_Raceday.Backend;
using Part2_Raceday.Models;

namespace Part2_Raceday.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        Users users = new Users();

        [HttpGet("me")]
        public IActionResult get_own_profile(int userId)
        {
            var user = users.get_user_dto(userId);

            if (user == null)
                return NotFound(new { message = "User not found." });

            return Ok(user);
        }

        [HttpPut("me")]
        public IActionResult update_own_profile(int userId, [FromBody] UpdateProfileRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.Name) || string.IsNullOrWhiteSpace(req.Surname) || !req.Email.Contains('@'))
                return BadRequest(new { message = "Name, surname and a valid email are required." });

            int result = users.update_profile(userId, req);

            if (result == 0)
                return NotFound(new { message = "User not found." });

            if (result == -1)
                return Conflict(new { message = "That email is already in use." });

            return Ok(new { message = "Profile updated." });
        }

        [HttpGet("{userId}")]
        public IActionResult get_user_by_id(int userId)
        {
            var user = users.get_user_dto(userId);

            if (user == null)
                return NotFound(new { message = "User not found." });

            return Ok(new { user.UserId, user.Name, user.Surname, user.Role });
        }
    }
}