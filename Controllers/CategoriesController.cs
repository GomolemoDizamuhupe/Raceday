using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Part2_Raceday.Backend;
using Part2_Raceday.Models;

namespace Part2_Raceday.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        Categories categories = new Categories();

        [HttpGet("{EventId}/Categories")]
        public IActionResult event_categories(int EventId)
        {
            return Ok(categories.get_categories_for_event(EventId));
        }

        [HttpPost]
        public IActionResult adding_category([FromBody] CategoryRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.Name) || req.DistanceKm <= 0 || req.MaxParticipants <= 0)
                return BadRequest(new { message = "Name, a positive distance and max participants are required." });

            int newId = categories.create_category(req);

            if (newId == -1)
                return StatusCode(403, new { message = "Event not found or you don't own it." });

            return Created($"/api/Categories/{newId}", new { categoryId = newId });
        }

        [HttpPut("{categoryId}")]
        public IActionResult updating_category(int categoryId, [FromBody] CategoryRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.Name) || req.DistanceKm <= 0 || req.MaxParticipants <= 0)
                return BadRequest(new { message = "Name, a positive distance and max participants are required." });

            if (!categories.update_category(categoryId, req))
                return NotFound(new { message = "Category not found for this organiser." });

            return Ok(new { message = "Category updated." });
        }

        [HttpDelete("{categoryId}")]
        public IActionResult removing_category(int categoryId, int organiserId)
        {
            int result = categories.delete_category_by_id(categoryId, organiserId);

            if (result == 0)
                return NotFound(new { message = "Category not found for this organiser." });

            if (result == -1)
                return Conflict(new { message = "This category has a route or entries and can't be deleted." });

            return NoContent();
        }

        /*[HttpGet("{EventId}/Categories")]
        public List<string> event_categories(int EventId)
        {
            return categories.get_event_category(EventId);
        }

        [HttpPost]
        public string adding_category(int eventId, string name, string distanceKm, int maxParticipants)
        {
            return categories.uploading_category(eventId, name, distanceKm, maxParticipants);
        }

        [HttpDelete("{categoryId}")]
        public string removing_category(int categoryId, int organiserId)
        {
            return categories.deleting_category(categoryId, organiserId);
        }


        [HttpPut("{categoryId}")]
        public string updating_category(int categoryId, int organiserId, string name, string distancekm, int maxparticipant)
        {
            return categories.editing_category(categoryId, organiserId, name, distancekm, maxparticipant);
        }*/

        [HttpGet("{categoryId}/Category")]
        public string category_with_route(int categoryId)
        {
            return categories.get_category_route(categoryId);
        }

    }
}
