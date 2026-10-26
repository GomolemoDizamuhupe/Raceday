using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Part2_Raceday.Backend;
using Part2_Raceday.Models;

namespace Part2_Raceday.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventsController : ControllerBase
    {

        Events events = new Events();

        //Event table


        [HttpPost]
        public IActionResult adding_events([FromBody] EventRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.Name) || string.IsNullOrWhiteSpace(req.Location))
                return BadRequest(new { message = "Name and location are required." });

            int newId = events.create_event(req);

            if (newId == -1)
                return StatusCode(403, new { message = "Only organisers can create events." });

            return CreatedAtAction(nameof(displaying_an_event), new { EventId = newId }, new { eventId = newId });
        }

        [HttpPut("{eventId}")]
        public IActionResult updating_events(int eventId, [FromBody] EventRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.Name) || string.IsNullOrWhiteSpace(req.Location))
                return BadRequest(new { message = "Name and location are required." });

            if (!events.update_event(eventId, req))
                return NotFound(new { message = "Event not found for this organiser." });

            return Ok(new { message = "Event updated." });
        }

        [HttpDelete("{eventId}")]
        public IActionResult removing_events(int eventId, int organiserId)
        {
            int result = events.delete_event_by_id(eventId, organiserId);

            if (result == 0)
                return NotFound(new { message = "Event not found for this organiser." });

            if (result == -1)
                return Conflict(new { message = "Delete this event's categories first." });

            return NoContent();
        }

        /*[HttpPost]
        public string adding_events(int organiser_id, string name, string description, string event_date, string location)
        {
            return events.uploading_event(organiser_id, name, description, event_date, location);
        }


        [HttpPut("{eventId}")]
        public string updating_events(int eventId, int organiser_id, string name, string description, string event_date, string location)
        {
            return events.editing_event(eventId, organiser_id, name, description, event_date, location);
        }

        [HttpDelete("{eventId}")]
        public string removing_events(int eventId, int organiser_id)
        {
            return events.deleting_event(eventId, organiser_id);
        }

        [HttpGet]
        public List <string> displaying_all_events()
        {
            return events.viewing_event();
        }

        [HttpGet("{EventId}")]
        public string displaying_an_event(int EventId)
        {
            return events.viewing_event_using_eventid(EventId);
        }*/

        [HttpGet]
        public IActionResult displaying_all_events()
        {
            return Ok(events.get_all_events());
        }

        [HttpGet("{EventId}")]
        public IActionResult displaying_an_event(int EventId)
        {
            var ev = events.get_event_by_id(EventId);

            if (ev == null)
                return NotFound(new { message = "Event not found." });

            return Ok(ev);
        }

    }
}
