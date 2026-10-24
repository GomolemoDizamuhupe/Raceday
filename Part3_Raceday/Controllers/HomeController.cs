using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Part3_Raceday.Models;
using Part3_Raceday.Filters;
using System.ComponentModel.DataAnnotations;

namespace Part3_Raceday.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        [RequireLogin]
        public IActionResult Index()
        {
            return View();
        }

        [RequireLogin]
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
