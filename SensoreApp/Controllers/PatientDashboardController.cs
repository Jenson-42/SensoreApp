using Microsoft.AspNetCore.Mvc;

namespace SensoreApp.Controllers
{
    public class PatientDashboardController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
