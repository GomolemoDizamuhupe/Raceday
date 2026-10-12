using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Part2_Raceday.Backend;

namespace Part2_Raceday.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EntriesController : ControllerBase
    {

        Entries entries = new Entries();

        [HttpPost("/categories/{CategoryId}/entries")]
        public string adding_participant_entry(int ParticipantId, string CategoryId, string EntryDate, string Status) 
        {
            return entries.uploading_entry(ParticipantId, CategoryId, EntryDate, Status);
        }

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
