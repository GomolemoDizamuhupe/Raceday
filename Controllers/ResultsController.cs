using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Part2_Raceday.Backend;

namespace Part2_Raceday.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ResultsController : ControllerBase
    {

        RaceResults results = new RaceResults();

        [HttpPost("{EntryId}")]
        public string entering_participant_results(int EntryId, string FinishTime, int Position, int OrganiserId) 
        {
            return results.uploading_results(EntryId, FinishTime, Position, OrganiserId);
        }

        [HttpPut("{ResultId}")]
        public string updating_participant_results(int ResultId, string FinishTime, int Position, int OrganiserId)
        {
            return results.updating_results(ResultId, FinishTime, Position, OrganiserId);
        }

        [HttpGet]
        public List<string> displaying_participant_results(int ParticipantId)
        {
            return results.get_participant_event_results(ParticipantId);
        }

        [HttpGet("{EventId}")]
        public List<string> displaying_all_event_results(int EventId, int OrganiserId)
        {
            return results.get_event_results(EventId, OrganiserId);
        }

    }
}
