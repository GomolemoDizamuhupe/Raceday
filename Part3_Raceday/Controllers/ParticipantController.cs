using Microsoft.AspNetCore.Mvc;
using Part3_Raceday.Filters;
using Part3_Raceday.Models;
using Part3_Raceday.Services;

namespace Part3_Raceday.Controllers
{
    [RequireLogin("participant")]
    public class ParticipantController : Controller
    {
        private readonly ApiService _api;

        public ParticipantController(ApiService api)
        {
            _api = api;
        }

        public async Task<IActionResult> Events(string? search, string? type, string? location)
        {
            var vm = new EventsListViewModel { Search = search, Type = type, Location = location };

            var result = await _api.GetAsync<List<ApiEvent>>("api/Events");

            if (!result.Success || result.Data == null)
            {
                vm.Error = result.Message;
                return View(vm);
            }

            var upcoming = result.Data.Where(e => e.EventDate.Date >= DateTime.Today).ToList();

            vm.Types = upcoming
                .Where(e => !string.IsNullOrWhiteSpace(e.EventType))
                .Select(e => e.EventType!)
                .Distinct()
                .OrderBy(t => t)
                .ToList();

            IEnumerable<ApiEvent> query = upcoming;

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(e =>
                    e.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    (e.Description ?? "").Contains(search, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrWhiteSpace(type))
                query = query.Where(e => string.Equals(e.EventType, type, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrWhiteSpace(location))
                query = query.Where(e => e.Location.Contains(location, StringComparison.OrdinalIgnoreCase));

            vm.Events = query.OrderBy(e => e.EventDate).ToList();

            return View(vm);
        }


        public async Task<IActionResult> EventDetail(int id)
        {
            var ev = await _api.GetAsync<ApiEvent>($"api/Events/{id}");

            if (!ev.Success || ev.Data == null)
            {
                TempData["Error"] = ev.Message;
                return RedirectToAction("Events");
            }

            var cats = await _api.GetAsync<List<ApiCategory>>($"api/Categories/{id}/Categories");

            return View(new EventDetailViewModel
            {
                Event = ev.Data,
                Categories = cats.Data ?? new List<ApiCategory>()
            });
        }

        [HttpPost]
        public async Task<IActionResult> Enter(int eventId, int categoryId)
        {
            int userId = HttpContext.Session.GetInt32("UserId")!.Value;

            var result = await _api.PostAsync<object>($"api/categories/{categoryId}/entries",
                new { participantId = userId });

            if (result.Success)
                TempData["Success"] = "You're entered! See it under My Entries.";
            else
                TempData["Error"] = result.Message;

            return RedirectToAction("EventDetail", new { id = eventId });
        }

        public async Task<IActionResult> MyEvents()
        {
            int userId = HttpContext.Session.GetInt32("UserId")!.Value;

            var result = await _api.GetAsync<List<ApiEntry>>($"api/Entries?participantId={userId}");

            if (!result.Success)
                ViewBag.Error = result.Message;

            return View(result.Data ?? new List<ApiEntry>());
        }

        [HttpPost]
        public async Task<IActionResult> Withdraw(int entryId)
        {
            int userId = HttpContext.Session.GetInt32("UserId")!.Value;

            var result = await _api.DeleteAsync<object>($"api/Entries/{entryId}?participantId={userId}");

            if (result.Success)
                TempData["Success"] = "You have withdrawn from the event.";
            else
                TempData["Error"] = result.Message;

            return RedirectToAction("MyEvents");
        }


    }
}