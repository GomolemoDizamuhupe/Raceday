using Microsoft.AspNetCore.Mvc;
using Part2_Raceday.Backend;

namespace Part2_Raceday
{
    [Route("api/[controller]")]
    [ApiController]
    public class users_controller : ControllerBase
    {

        Users users = new Users();

        [HttpGet("me")]
        public string get_own_profile(int userId)
        {
            return users.get_own_profile(userId);
        }

        [HttpPut("me")]
        public string update_own_profile(int userId, string name, string surname, string email)
        {
            return users.update_own_profile(userId, name, surname, email);
        }

        [HttpGet("{userId}")]
        public string get_user_by_id(int userId)
        {
            return users.get_user_by_id(userId);
        }

        /*        public void Test1()
        {

            users textUsers = new users();

            string name = "Gomolemo"; string surname = "Tom"; string email = "123abc098@gmail.com"; string password = "12312scfef@"; string role = "organiser";

           string returned_results= textUsers.store_user_info(name, surname, email, password, role);
            string expected = "user registered successfully...";


            Assert.Equal(returned_results , expected);


        }*/

    }
}