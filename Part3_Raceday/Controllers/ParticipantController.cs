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
    }
}