using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Part2_Raceday.Backend;

namespace Part2_Raceday.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        //Registering the user and auth the user if they are registered checking the email
        [HttpPost("Register")]
        public string register(string name, string surname, string email, string password_hash, string role)
        {
            Auth auth = new Auth();

            return auth.user_auth_reg(name, surname, email, password_hash, role);
        }

        //user logining in and auth the user if they are registered checking the email
        [HttpPost("Login")]
        public string login(string email, string password_hash)
        {
            Auth auth = new Auth();

            return auth.user_auth_login(email, password_hash);
        }

    }
}
