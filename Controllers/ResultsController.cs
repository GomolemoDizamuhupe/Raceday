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
            return results.uploading_entry(EntryId, FinishTime, Position, OrganiserId);
        }

        [HttpPut("{ResultId}")]
        public string updating_participant_results(int ResultId, string FinishTime, int Position, int OrganiserId)
        {
            return results.updating_entry(ResultId, FinishTime, Position, OrganiserId);
        }


    }
}
