using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Part2_Raceday.Backend;

namespace Part2_Raceday.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EntriesController : ControllerBase
    {

        [HttpPost("/categories/{CategoryId}/entries")]
        public string adding_participant_entry(int ParticipantId, string CategoryId, string EntryDate, string Status) 
        {
            Entries entries = new Entries();

            return entries.uploading_entry(ParticipantId, CategoryId, EntryDate, Status);
        }

        [HttpGet]
        public List<string> getting_participant_entries(int ParticipantId)
        {
            Entries entries = new Entries();

            return entries.get_participant_event_entries(ParticipantId);
        }

        [HttpGet("{EventId}")]
        public List<string> getting_organiser_event_entries(int OrganiserId, int EventId)
        {
            Entries entries = new Entries();

            return entries.displaying_organiser_event_entries(OrganiserId, EventId);
        }

    }
}
