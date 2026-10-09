using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Part2_Raceday.Backend;

namespace Part2_Raceday.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoutesController : ControllerBase
    {

        [HttpGet("{categoryId}route")]
        public string get_route_for_category(int categoryId) 
        {
            Routes routes = new Routes();

            return routes.get_route_for_category(categoryId);
        }

    }
}
