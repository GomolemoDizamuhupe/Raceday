using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Part2_Raceday.Backend;

namespace Part2_Raceday.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {


        [HttpGet("{EventId}/Categories")]
        public List<string> event_categories(int EventId)
        {
            Categories categories = new Categories();

            return categories.get_event_category(EventId);
        }

        [HttpPost]
        public string adding_category(int eventId, string name, string distanceKm, int maxParticipants)
        {
            Categories categories = new Categories();

            return categories.uploading_category(eventId, name, distanceKm, maxParticipants);
        }

        [HttpDelete("{categoryId}")]
        public string removing_category(int categoryId, int organiserId)
        {
            Categories categories = new Categories();

            return categories.deleting_category(categoryId, organiserId);
        }


        [HttpPut("{categoryId}")]
        public string updating_category(int categoryId, int organiserId, string name, string distancekm, int maxparticipant)
        {
            Categories categories = new Categories();

            return categories.editing_category(categoryId, organiserId, name, distancekm, maxparticipant);
        }

        [HttpGet("{categoryId}/Category")]
        public string category_with_route(int categoryId)
        {
            Categories categories = new Categories();

            return categories.get_category_route(categoryId);
        }

    }
}
