using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Part2_Raceday.Backend;
using Part2_Raceday.Models;

namespace Part2_Raceday.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EntriesController : ControllerBase
    {

        Entries entries = new Entries();


        [HttpPost("/api/categories/{categoryId}/entries")]
        public IActionResult adding_participant_entry(int categoryId, [FromBody] EntryRequest req)
        {
            int result = entries.create_entry(req.ParticipantId, categoryId);

            return result switch
            {
                -1 => StatusCode(403, new { message = "Only participants can enter events." }),
                -2 => NotFound(new { message = "Category not found." }),
                -3 => Conflict(new { message = "You have already entered this category." }),
                -4 => Conflict(new { message = "This category is full." }),
                _ => Created($"/api/Entries?participantId={req.ParticipantId}", new { entryId = result })
            };
        }

        /*[HttpPost("/categories/{CategoryId}/entries")]
        public string adding_participant_entry(int ParticipantId, string CategoryId, string EntryDate, string Status) 
        {
            return entries.uploading_entry(ParticipantId, CategoryId, EntryDate, Status);
        }*/

        [HttpGet]
        public List<string> getting_participant_entries(int ParticipantId)
        {
            return entries.get_participant_event_entries(ParticipantId);
        }

        [HttpGet("{EventId}")]
        public List<string> getting_organiser_event_entries(int OrganiserId, int EventId)
        {
            return entries.displaying_organiser_event_entries(OrganiserId, EventId);
        }

        [HttpDelete("{EntryId}")]
        public string deleteing_participant_entry(int EntryId, int ParticipantId)
        {
            return entries.deleting_entry(EntryId, ParticipantId);
        }

    }
}
