using Microsoft.AspNetCore.Mvc; // Using directive for ASP.NET Core MVC as instructed to for app structure
using Microsoft.EntityFrameworkCore; // Using directive for Entity Framework Core for database operations
using SensoreApp.Data; // Using directive for the application's data context
using SensoreApp.Models; // Using directive for the application's models

namespace SensoreApp.Controllers
{
    public class SensorDevicesController : Controller
    {
        private readonly AppDBContext _context; 
        private readonly ILogger<SensorDevicesController> _logger; // to add logging to system for easier debugging and tracking as well as auditing for admin dashboard

        // Dependency injection of the database context and logger
        public SensorDevicesController(AppDBContext context, ILogger<SensorDevicesController> logger)
        {
            _context = context;
            _logger = logger; // to initialise the logger implementation
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}
