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

        [HttpPost("{entryId}")]
        public IActionResult entering_participant_results(int entryId, [FromBody] ResultRequest req)
        {
            if (req.FinishTime <= TimeSpan.Zero || (req.Position.HasValue && req.Position <= 0))
                return BadRequest(new { message = "Finish time and position must be positive." });

            int result = results.create_result(entryId, req);

            return result switch
            {
                -1 => StatusCode(403, new { message = "Entry not found or you don't own its event." }),
                -2 => Conflict(new { message = "A result already exists for this entry." }),
                _ => Created($"/api/Results/{result}", new { resultId = result })
            };
        }

        [HttpPut("{resultId}")]
        public IActionResult updating_participant_results(int resultId, [FromBody] ResultRequest req)
        {
            if (req.FinishTime <= TimeSpan.Zero || (req.Position.HasValue && req.Position <= 0))
                return BadRequest(new { message = "Finish time and position must be positive." });

            if (!results.update_result(resultId, req))
                return NotFound(new { message = "Result not found for this organiser." });

            return Ok(new { message = "Result updated." });
        }

        [HttpGet("{eventId}")]
        public IActionResult displaying_all_event_results(int eventId, int organiserId)
        {
            var list = results.get_results_for_event(eventId, organiserId);

            if (list == null)
                return StatusCode(403, new { message = "Event not found or you don't own it." });

            return Ok(list);
        }

        /*****************************/

        /*[HttpPost("{EntryId}")]
        public string entering_participant_results(int EntryId, string FinishTime, int Position, int OrganiserId) 
        {
            return results.uploading_results(EntryId, FinishTime, Position, OrganiserId);
        }*/

        /*[HttpPut("{ResultId}")]
        public string updating_participant_results(int ResultId, string FinishTime, int Position, int OrganiserId)
        {
            return results.updating_results(ResultId, FinishTime, Position, OrganiserId);
        }*/

        /*[HttpGet]
        public List<string> displaying_participant_results(int ParticipantId)
        {
            return results.get_participant_event_results(ParticipantId);
        }*/

        /*[HttpGet("{EventId}")]
        public List<string> displaying_all_event_results(int EventId, int OrganiserId)
        {
            return results.get_event_results(EventId, OrganiserId);
        }*/

    }
}
