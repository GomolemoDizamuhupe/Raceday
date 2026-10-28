using Microsoft.AspNetCore.Mvc;
using Part3_Raceday.Filters;
using Part3_Raceday.Models;
using Part3_Raceday.Services;

namespace Part3_Raceday.Controllers
{
    [RequireLogin("organiser")]
    public class OrganiserController : Controller
    {
        private readonly ApiService _api;

        public OrganiserController(ApiService api)
        {
            _api = api;
        }

        private int UserId => HttpContext.Session.GetInt32("UserId")!.Value;

        public async Task<IActionResult> Dashboard()
        {
            var vm = new DashboardViewModel();

            var result = await _api.GetAsync<List<ApiEvent>>("api/Events");

            if (!result.Success || result.Data == null)
            {
                vm.Error = result.Message;
                return View(vm);
            }

            var mine = result.Data.Where(e => e.OrganiserId == UserId).OrderBy(e => e.EventDate).ToList();

            foreach (var ev in mine)
            {
                var entries = await _api.GetAsync<List<ApiEventEntry>>($"api/Entries/{ev.EventId}?organiserId={UserId}");
                int count = entries.Data?.Count ?? 0;

                vm.Rows.Add(new DashboardRow { Event = ev, Enrolments = count });
                vm.TotalEnrolments += count;
            }

            vm.TotalEvents = mine.Count;
            vm.UpcomingEvents = mine.Count(e => e.EventDate.Date >= DateTime.Today);

            return View(vm);
        }

        public async Task<IActionResult> Events()
        {
            var result = await _api.GetAsync<List<ApiEvent>>("api/Events");

            if (!result.Success)
                ViewBag.Error = result.Message;

            var mine = (result.Data ?? new List<ApiEvent>())
                .Where(e => e.OrganiserId == UserId)
                .OrderBy(e => e.EventDate)
                .ToList();

            return View(mine);
        }

        [HttpGet]
        public IActionResult CreateEvent() => View("EventForm", new EventFormViewModel());

        [HttpGet]
        public async Task<IActionResult> EditEvent(int id)
        {
            var result = await _api.GetAsync<ApiEvent>($"api/Events/{id}");

            if (!result.Success || result.Data == null || result.Data.OrganiserId != UserId)
            {
                TempData["Error"] = "Event not found.";
                return RedirectToAction("Events");
            }

            var e = result.Data;

            return View("EventForm", new EventFormViewModel
            {
                EventId = e.EventId,
                Name = e.Name,
                Description = e.Description,
                EventDate = e.EventDate,
                Location = e.Location,
                EventType = e.EventType,
                DistanceKm = e.DistanceKm
            });
        }

        [HttpPost]
        public async Task<IActionResult> SaveEvent(EventFormViewModel model)
        {
            if (!ModelState.IsValid) return View("EventForm", model);

            var body = new
            {
                organiserId = UserId,
                name = model.Name,
                description = model.Description,
                eventDate = model.EventDate,
                location = model.Location,
                eventType = model.EventType,
                distanceKm = model.DistanceKm
            };

            var result = model.EventId == 0
                ? await _api.PostAsync<object>("api/Events", body)
                : await _api.PutAsync<object>($"api/Events/{model.EventId}", body);

            if (!result.Success)
            {
                ModelState.AddModelError("", result.Message);
                return View("EventForm", model);
            }

            TempData["Success"] = model.EventId == 0 ? "Event created." : "Event updated.";
            return RedirectToAction("Events");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var result = await _api.DeleteAsync<object>($"api/Events/{id}?organiserId={UserId}");

            if (result.Success)
                TempData["Success"] = "Event deleted.";
            else
                TempData["Error"] = result.Message;

            return RedirectToAction("Events");
        }

        public async Task<IActionResult> Categories(int id)
        {
            var ev = await _api.GetAsync<ApiEvent>($"api/Events/{id}");

            if (!ev.Success || ev.Data == null || ev.Data.OrganiserId != UserId)
            {
                TempData["Error"] = "Event not found.";
                return RedirectToAction("Events");
            }

            var cats = await _api.GetAsync<List<ApiCategory>>($"api/Categories/{id}/Categories");

            return View(new CategoriesPageViewModel
            {
                Event = ev.Data,
                Categories = cats.Data ?? new List<ApiCategory>()
            });
        }

        [HttpGet]
        public IActionResult CreateCategory(int eventId) =>
            View("CategoryForm", new CategoryFormViewModel { EventId = eventId });

        [HttpGet]
        public async Task<IActionResult> EditCategory(int id, int eventId)
        {
            var cats = await _api.GetAsync<List<ApiCategory>>($"api/Categories/{eventId}/Categories");
            var c = cats.Data?.FirstOrDefault(x => x.CategoryId == id);

            if (c == null)
            {
                TempData["Error"] = "Category not found.";
                return RedirectToAction("Categories", new { id = eventId });
            }

            return View("CategoryForm", new CategoryFormViewModel
            {
                CategoryId = c.CategoryId,
                EventId = c.EventId,
                Name = c.Name,
                DistanceKm = c.DistanceKm,
                MaxParticipants = c.MaxParticipants
            });
        }

        [HttpPost]
        public async Task<IActionResult> SaveCategory(CategoryFormViewModel model)
        {
            if (!ModelState.IsValid) return View("CategoryForm", model);

            var body = new
            {
                organiserId = UserId,
                eventId = model.EventId,
                name = model.Name,
                distanceKm = model.DistanceKm,
                maxParticipants = model.MaxParticipants
            };

            var result = model.CategoryId == 0
                ? await _api.PostAsync<object>("api/Categories", body)
                : await _api.PutAsync<object>($"api/Categories/{model.CategoryId}", body);

            if (!result.Success)
            {
                ModelState.AddModelError("", result.Message);
                return View("CategoryForm", model);
            }

            TempData["Success"] = model.CategoryId == 0 ? "Category added." : "Category updated.";
            return RedirectToAction("Categories", new { id = model.EventId });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteCategory(int id, int eventId)
        {
            var result = await _api.DeleteAsync<object>($"api/Categories/{id}?organiserId={UserId}");

            if (result.Success)
                TempData["Success"] = "Category deleted.";
            else
                TempData["Error"] = result.Message;

            return RedirectToAction("Categories", new { id = eventId });
        }

        public async Task<IActionResult> Enrolments(int id, string? category)
        {
            var ev = await _api.GetAsync<ApiEvent>($"api/Events/{id}");

            if (!ev.Success || ev.Data == null || ev.Data.OrganiserId != UserId)
            {
                TempData["Error"] = "Event not found.";
                return RedirectToAction("Events");
            }

            var entries = await _api.GetAsync<List<ApiEventEntry>>($"api/Entries/{id}?organiserId={UserId}");

            var all = entries.Data ?? new List<ApiEventEntry>();

            var vm = new EnrolmentsViewModel
            {
                Event = ev.Data,
                Category = category,
                Categories = all.Select(e => e.CategoryName).Distinct().OrderBy(n => n).ToList(),
                Entries = string.IsNullOrWhiteSpace(category)
                    ? all
                    : all.Where(e => e.CategoryName == category).ToList()
            };

            return View(vm);
        }

        public async Task<IActionResult> Results(int id)
        {
            var ev = await _api.GetAsync<ApiEvent>($"api/Events/{id}");

            if (!ev.Success || ev.Data == null || ev.Data.OrganiserId != UserId)
            {
                TempData["Error"] = "Event not found.";
                return RedirectToAction("Events");
            }

            var entries = await _api.GetAsync<List<ApiEventEntry>>($"api/Entries/{id}?organiserId={UserId}");
            var results = await _api.GetAsync<List<ApiEventResult>>($"api/Results/{id}?organiserId={UserId}");

            var existing = (results.Data ?? new List<ApiEventResult>()).ToDictionary(r => r.EntryId);

            var rows = (entries.Data ?? new List<ApiEventEntry>())
                .Select(e =>
                {
                    existing.TryGetValue(e.EntryId, out var r);

                    return new ResultRow
                    {
                        EntryId = e.EntryId,
                        ParticipantName = e.ParticipantName,
                        CategoryName = e.CategoryName,
                        ResultId = r?.ResultId,
                        FinishTime = r != null ? r.FinishTime.ToString(@"hh\:mm\:ss") : "",
                        Position = r?.Position
                    };
                })
                .OrderBy(r => r.CategoryName)
                .ThenBy(r => r.ParticipantName)
                .ToList();

            return View(new ResultsEntryViewModel
            {
                Event = ev.Data,
                EventHasHappened = ev.Data.EventDate.Date <= DateTime.Today,
                Rows = rows
            });
        }

        [HttpPost]
        public async Task<IActionResult> SaveResult(int eventId, int entryId, int? resultId, string finishTime, int? position)
        {
            if (!TimeSpan.TryParse(finishTime, out var time) || time <= TimeSpan.Zero)
            {
                TempData["Error"] = "Enter the finish time as hh:mm:ss, for example 01:45:30.";
                return RedirectToAction("Results", new { id = eventId });
            }

            if (position.HasValue && position <= 0)
            {
                TempData["Error"] = "Position must be 1 or higher.";
                return RedirectToAction("Results", new { id = eventId });
            }

            var body = new { organiserId = UserId, finishTime = time.ToString(@"hh\:mm\:ss"), position };

            var result = resultId == null
                ? await _api.PostAsync<object>($"api/Results/{entryId}", body)
                : await _api.PutAsync<object>($"api/Results/{resultId}", body);

            if (result.Success)
                TempData["Success"] = "Result saved.";
            else
                TempData["Error"] = result.Message;

            return RedirectToAction("Results", new { id = eventId });
        }

        [HttpGet]
        public async Task<IActionResult> EditRoute(int categoryId, int eventId)
        {
            var ev = await _api.GetAsync<ApiEvent>($"api/Events/{eventId}");

            if (!ev.Success || ev.Data == null || ev.Data.OrganiserId != UserId)
            {
                TempData["Error"] = "Event not found.";
                return RedirectToAction("Events");
            }

            var vm = new RouteFormViewModel { CategoryId = categoryId, EventId = eventId };

            var existing = await _api.GetAsync<ApiRoute>($"api/Routes/category/{categoryId}");

            if (existing.Success && existing.Data != null)
            {
                vm.RouteId = existing.Data.RouteId;
                vm.StartPoint = existing.Data.StartPoint;
                vm.EndPoint = existing.Data.EndPoint;
                vm.ElevationGainM = existing.Data.ElevationGainM;
                vm.MapUrl = existing.Data.MapUrl;
            }

            return View("RouteForm", vm);
        }

        [HttpPost]
        public async Task<IActionResult> SaveRoute(RouteFormViewModel model)
        {
            if (!ModelState.IsValid) return View("RouteForm", model);

            var body = new
            {
                organiserId = UserId,
                categoryId = model.CategoryId,
                startPoint = model.StartPoint,
                endPoint = model.EndPoint,
                elevationGainM = model.ElevationGainM,
                mapUrl = model.MapUrl
            };

            var result = model.RouteId == 0
                ? await _api.PostAsync<object>("api/Routes", body)
                : await _api.PutAsync<object>($"api/Routes/{model.RouteId}", body);

            if (!result.Success)
            {
                ModelState.AddModelError("", result.Message);
                return View("RouteForm", model);
            }

            TempData["Success"] = model.RouteId == 0 ? "Route added." : "Route updated.";
            return RedirectToAction("Categories", new { id = model.EventId });
        }

    }
}