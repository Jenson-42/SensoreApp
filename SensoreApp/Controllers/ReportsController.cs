using Microsoft.AspNetCore.Mvc;

namespace SensoreApp.Controllers
{
    public class ReportsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
