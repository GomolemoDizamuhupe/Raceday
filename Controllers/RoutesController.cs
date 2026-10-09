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


        [HttpPost("{categoryId}route")]
        public string creating_route(int categoryId, string StartPoint, string EndPoint, int ElevationGainM, string MapUrl)
        {
            Routes routes = new Routes();

            return routes.uploading_route(categoryId, StartPoint, EndPoint, ElevationGainM, MapUrl);
        }

        [HttpPut("{RouteId}")]
        public string updating_route(int OrganiserId, int RouteId, int categoryId, string StartPoint, string EndPoint, int ElevationGainM, string MapUrl)
        {
            Routes routes = new Routes();

            return routes.editing_route(OrganiserId, RouteId, categoryId, StartPoint, EndPoint, ElevationGainM, MapUrl);
        }


    }
}
