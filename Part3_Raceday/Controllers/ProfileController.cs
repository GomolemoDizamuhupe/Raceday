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
        private readonly IWebHostEnvironment _env;

        public ProfileController(ApiService api, IWebHostEnvironment env)
        {
            _api = api;
            _env = env;
        }

        [HttpPost]
        public async Task<IActionResult> UploadPicture(IFormFile? picture)
        {
            int userId = HttpContext.Session.GetInt32("UserId")!.Value;

            if (picture == null || picture.Length == 0)
            {
                TempData["Error"] = "Choose an image first.";
                return RedirectToAction("Index");
            }

            string ext = Path.GetExtension(picture.FileName).ToLowerInvariant();
            string[] allowed = { ".jpg", ".jpeg", ".png", ".webp" };

            if (!allowed.Contains(ext) || !picture.ContentType.StartsWith("image/") || picture.Length > 5 * 1024 * 1024)
            {
                TempData["Error"] = "Upload a .jpg, .png or .webp image under 5 MB.";
                return RedirectToAction("Index");
            }

            string folder = Path.Combine(_env.WebRootPath, "uploads", "profiles");
            Directory.CreateDirectory(folder);

            string fileName = $"{Guid.NewGuid()}{ext}";
            string fullPath = Path.Combine(folder, fileName);

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await picture.CopyToAsync(stream);
            }

            string url = $"/uploads/profiles/{fileName}";

            var current = await _api.GetAsync<ApiUser>($"api/Users/me?userId={userId}");
            string? oldUrl = current.Data?.ProfilePictureUrl;

            var result = await _api.PutAsync<object>($"api/Users/me/picture?userId={userId}", new { pictureUrl = url });

            if (!result.Success)
            {
                System.IO.File.Delete(fullPath);
                TempData["Error"] = result.Message;
                return RedirectToAction("Index");
            }

            // delete the previous picture so old files don't pile up
            if (!string.IsNullOrEmpty(oldUrl) && oldUrl.StartsWith("/uploads/profiles/"))
            {
                string oldPath = Path.Combine(folder, Path.GetFileName(oldUrl));
                if (System.IO.File.Exists(oldPath)) System.IO.File.Delete(oldPath);
            }

            TempData["Success"] = "Profile picture updated.";
            return RedirectToAction("Index");
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