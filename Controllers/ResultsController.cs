using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Part2_Raceday.Backend;
using Part2_Raceday.Models;

namespace Part2_Raceday.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ResultsController : ControllerBase
    {

        RaceResults results = new RaceResults();
        /*****************************/

        [HttpGet]
        public IActionResult displaying_participant_results(int participantId)
        {
            var list = results.get_results_for_participant(participantId);

            if (list == null)
                return StatusCode(403, new { message = "Only participants have results." });

            return Ok(list);
        }

        /*****************************/
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

        /*[HttpGet]
        public List<string> displaying_participant_results(int ParticipantId)
        {
            return results.get_participant_event_results(ParticipantId);
        }*/

        [HttpGet("{EventId}")]
        public List<string> displaying_all_event_results(int EventId, int OrganiserId)
        {
            return results.get_event_results(EventId, OrganiserId);
        }

    }
}
