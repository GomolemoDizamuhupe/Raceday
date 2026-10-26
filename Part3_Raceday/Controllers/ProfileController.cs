using Microsoft.AspNetCore.Mvc;
using Part3_Raceday.Filters;
using Part3_Raceday.Models;
using Part3_Raceday.Services;

namespace Part3_Raceday.Controllers
{
    [RequireLogin]
    public class ProfileController : Controller
    {
        private readonly ApiService _api;

        public ProfileController(ApiService api)
        {
            _api = api;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            int userId = HttpContext.Session.GetInt32("UserId")!.Value;

            var result = await _api.GetAsync<ApiUser>($"api/Users/me?userId={userId}");

            if (!result.Success || result.Data == null)
            {
                TempData["Error"] = result.Message;
                return RedirectToAction("Index", "Home");
            }

            return View(new ProfileViewModel
            {
                Name = result.Data.Name,
                Surname = result.Data.Surname,
                Email = result.Data.Email,
                Role = result.Data.Role,
                ProfilePictureUrl = result.Data.ProfilePictureUrl
            });
        }

        [HttpPost]
        public async Task<IActionResult> Index(ProfileViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            int userId = HttpContext.Session.GetInt32("UserId")!.Value;

            var result = await _api.PutAsync<object>($"api/Users/me?userId={userId}", new
            {
                name = model.Name,
                surname = model.Surname,
                email = model.Email
            });

            if (!result.Success)
            {
                ModelState.AddModelError("", result.Message);
                return View(model);
            }

            HttpContext.Session.SetString("Name", model.Name);
            TempData["Success"] = "Profile updated.";
            return RedirectToAction("Index");
        }
    }
}