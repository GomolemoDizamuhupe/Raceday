using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Part2_Raceday.Backend;

namespace Part2_Raceday.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        Categories categories = new Categories();

        [HttpGet("{EventId}/Categories")]
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
        }

        [HttpGet("{categoryId}/Category")]
        public string category_with_route(int categoryId)
        {
            return categories.get_category_route(categoryId);
        }

    }
}
