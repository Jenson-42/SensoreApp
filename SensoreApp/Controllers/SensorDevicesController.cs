using Microsoft.AspNetCore.Mvc; // Using directive for ASP.NET Core MVC as instructed to for app structure
using Microsoft.EntityFrameworkCore; // Using directive for Entity Framework Core for database operations
using SensoreApp.Data; // Using directive for the application's data context
using SensoreApp.Models;
using System.Threading.Tasks; // Using directive for the application's models

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

        // Indexing - Admin dashboard will be able to list all sensor devices 
        public async Task<IActionResult> Index()
        {
            // Fetch all sensor devices from the database that have been registered
            var devices = await _context.SensorDevices.ToListAsync();   
            // This action will return a view that lists all sensor devices
            return View();
        }

        // Details - All information for a device will be shown 
        public async Task<IActionResult> Details(int id)
        {
            // Fetch a specific sensor device by its ID and show its details 
            var device = await _context.SensorDevices.FindAsync(id);
            if (device == null)
            {
                _logger.LogWarning("Sensor device with ID {DeviceId} not found.", id); // Log a warning if the device is not found
                return NotFound(); // Return 404 if the device does not exist
            }
            // Return a view displaying the details of the sensor device
            return View(device);
        }

        // Create - Admin can register new sensor device 
        public IActionResult Create()
        {
            // Return a viewfor admin to create new device
            return View();
        }
        [HttpPost] 
        [ValidateAntiForgeryToken]
        // saves new device into the database as it is being created
        public async Task<IActionResult> Create(SensorDevices device)
        {
            if (ModelState.IsValid)
            {
                _context.Add(device); // Add the new device to the database context
                await _context.SaveChangesAsync(); // Save changes to the database
                _logger.LogInformation("New sensor device with Serial Number {SerialNumber} created.", device.SerialNumber); // Logs the creation of a new device
                return RedirectToAction(nameof(Index)); // Redirect to the index action after successful creation
            }
            return View(device); // If model state is invalid, return to the create view with validation errors
        }

        // Edit - updated device infromation can be shown here 
        public async Task<IActionResult> Edit(int id)
        {
            // Fetch the sensor device to be edited
            var device = await _context.SensorDevices.FindAsync(id);
            if (device == null)
            {
                _logger.LogWarning("Sensor device with ID {DeviceId} not found for editing.", id); // Log a warning if the device is not found
                return NotFound(); // Return 404 if the device does not exist
            }
            return View(device); // Return the edit view with the device data
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        // to save edited device information into the database
        public async Task<IActionResult> Edit(int id, SensorDevices device)
        {
            if (id != device.SensorDeviceID) 
                return NotFound();

            if (ModelState.IsValid)
            {
                try 
                {
                    _context.Update(device);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.SensorDevices.Any(d => d.SensorDeviceID == id))
                        return NotFound();

                    throw;

                }
                return RedirectToAction(nameof(Index));
            }
            return View(device);
        }

        // device is removed if faulty
        // to show deleted confirmation page 
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();
            var device = await _context.SensorDevices
                .FirstOrDefaultAsync(m => m.SensorDeviceID == id);
            if (device == null)
                return NotFound();
            return View(device);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var device = await _context.SensorDevices.FindAsync(id);

            if (device != null)
            {
                _context.SensorDevices.Remove(device);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
            


    }
}
