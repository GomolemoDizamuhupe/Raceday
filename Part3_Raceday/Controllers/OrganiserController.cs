using Microsoft.AspNetCore.Mvc;
using Part3_Raceday.Filters;

namespace Part3_Raceday.Controllers
{
    [RequireLogin("organiser")]
    public class OrganiserController : Controller
    {
        public IActionResult Dashboard() => View();
    }
}