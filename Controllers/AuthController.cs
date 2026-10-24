/*using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Part2_Raceday.Backend;

namespace Part2_Raceday.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {

        Auth auth = new Auth();
        
        //Registering the user and auth the user if they are registered checking the email
        [HttpPost("Register")]
        public string register(string name, string surname, string email, string password_hash, string role)
        {
            return auth.user_auth_reg(name, surname, email, password_hash, role);
        }

        //user logining in and auth the user if they are registered checking the email
        [HttpPost("Login")]
        public string login(string email, string password_hash)
        {
            return auth.user_auth_login(email, password_hash);
        }

    }
}
*/

using Microsoft.AspNetCore.Mvc;
using Part2_Raceday.Backend;
using Part2_Raceday.Models;

namespace Part2_Raceday.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        Auth auth = new Auth();

        [HttpPost("Register")]
        public IActionResult register([FromBody] RegisterRequest req)
        {
            string role = req.Role.Trim().ToLower();

            if (string.IsNullOrWhiteSpace(req.Name) || string.IsNullOrWhiteSpace(req.Surname) ||
                string.IsNullOrWhiteSpace(req.Email) || string.IsNullOrWhiteSpace(req.Password))
                return BadRequest(new { message = "All fields are required." });

            if (role != "organiser" && role != "participant")
                return BadRequest(new { message = "Role must be 'organiser' or 'participant'." });

            if (!req.Email.Contains('@'))
                return BadRequest(new { message = "Email is invalid." });

            if (req.Password.Length < 6)
                return BadRequest(new { message = "Password must be at least 6 characters." });

            int newId = auth.register_user(req);

            if (newId == -1)
                return Conflict(new { message = "Email already registered." });

            return Created($"/api/Users/{newId}", new { userId = newId });
        }

        [HttpPost("Login")]
        public IActionResult login([FromBody] LoginRequest req)
        {
            var user = auth.login_user(req);

            if (user == null)
                return Unauthorized(new { message = "Invalid email or password." });

            return Ok(user);
        }
    }
}