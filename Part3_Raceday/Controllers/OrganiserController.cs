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

        public IActionResult Dashboard() => View();

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

    }
}