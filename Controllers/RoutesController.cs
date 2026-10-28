using Microsoft.AspNetCore.Mvc;
using Part2_Raceday.Backend;
using Part2_Raceday.Models;

namespace Part2_Raceday.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoutesController : ControllerBase
    {
        Routes routes = new Routes();

        [HttpGet("category/{categoryId}")]
        public IActionResult get_route_for_category(int categoryId)
        {
            var route = routes.get_route_by_category(categoryId);

            if (route == null)
                return NotFound(new { message = "No route found for this category." });

            return Ok(route);
        }

        [HttpPost]
        public IActionResult creating_route([FromBody] RouteRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.StartPoint) || string.IsNullOrWhiteSpace(req.EndPoint))
                return BadRequest(new { message = "Start point and end point are required." });

            int result = routes.create_route(req);

            if (result == -1)
                return StatusCode(403, new { message = "Category not found or you don't own its event." });

            if (result == -2)
                return Conflict(new { message = "This category already has a route." });

            return Created($"/api/Routes/category/{req.CategoryId}", new { routeId = result });
        }

        [HttpPut("{routeId}")]
        public IActionResult updating_route(int routeId, [FromBody] RouteRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.StartPoint) || string.IsNullOrWhiteSpace(req.EndPoint))
                return BadRequest(new { message = "Start point and end point are required." });

            if (!routes.update_route(routeId, req))
                return NotFound(new { message = "Route not found for this organiser." });

            return Ok(new { message = "Route updated." });
        }
    }
}