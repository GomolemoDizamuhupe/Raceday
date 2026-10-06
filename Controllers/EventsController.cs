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


        [HttpDelete("{eventId}")]
        public string removing_events(int eventId, int organiser_id)
        {
            Events events = new Events();

            return events.deleting_event(eventId, organiser_id);
        }


        [HttpGet("{EventId}")]
        public string displaying_an_event(int EventId)
        {
            Events events = new Events();

            return events.viewing_event_using_eventid(EventId);
        }

    }
}
