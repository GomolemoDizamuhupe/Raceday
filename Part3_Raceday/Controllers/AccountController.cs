using Microsoft.AspNetCore.Mvc;
using Part3_Raceday.Models;
using Part3_Raceday.Services;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Part3_Raceday.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApiService _api;

        public AccountController(ApiService api)
        {
            _api = api;
        }

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var result = await _api.PostAsync<ApiUser>("api/Auth/Login",
                new { email = model.Email, password = model.Password });

            if (!result.Success || result.Data == null)
            {
                ModelState.AddModelError("", result.Message);
                return View(model);
            }

            HttpContext.Session.SetInt32("UserId", result.Data.UserId);
            HttpContext.Session.SetString("Role", result.Data.Role);
            HttpContext.Session.SetString("Name", result.Data.Name);

            return result.Data.Role == "organiser"
                ? RedirectToAction("Dashboard", "Organiser")
                : RedirectToAction("Events", "Participant");
        }

        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var result = await _api.PostAsync<object>("api/Auth/Register", new
            {
                name = model.Name,
                surname = model.Surname,
                email = model.Email,
                password = model.Password,
                role = model.Role
            });

            if (!result.Success)
            {
                ModelState.AddModelError("", result.Message);
                return View(model);
            }

            TempData["Success"] = "Account created. Please log in.";
            return RedirectToAction("Login");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
        public IActionResult AccessDenied() => View();
    }
}