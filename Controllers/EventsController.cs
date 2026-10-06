using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Part2_Raceday.Backend;

namespace Part2_Raceday.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventsController : ControllerBase
    {

        //Event table
        [HttpPost]
        public string adding_events(int organiser_id, string name, string description, string event_date, string location)
        {
            Events events = new Events();

            return events.uploading_event(organiser_id, name, description, event_date, location);
        }

    }
}
