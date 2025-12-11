using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SensoreApp.Data; // 
using SensoreApp.Models;
using System.Linq;
using System.Threading.Tasks;

namespace SensoreApp.Controllers
{
    /// Note: This would be protected by authentication and authorisation in an ideal app but due to issues faced noted in logbook (teamate exentension) ,
    public class AdminDashboardController : Controller
    {
        private readonly AppDBContext _context;
        private readonly ILogger<AdminDashboardController> _logger;

        // Dependency-injected database context and logger
        public AdminDashboardController(AppDBContext context, ILogger<AdminDashboardController> logger)
        {
            _context = context;
            _logger = logger;
        }

        
        /// Builds the AdminDashboardViewModel by collecting information from Users, Patients, Clinicians, SensorDevices and PatientClinicians.
        public async Task<IActionResult> Index()
        {
            _logger.LogInformation("Admin Dashboard Requested.");

            // get total users, patients and clinicians
            var totalUsers = await _context.Users.CountAsync();
            var totalPatients = await _context.Patients.CountAsync();
            var totalClinicians = await _context.Clinicians.CountAsync();
            // get active and inactive sensor devices
            var activeDevices = await _context.SensorDevices.CountAsync(d => d.IsActive);
            var inactiveDevices = await _context.SensorDevices.CountAsync(d => !d.IsActive);

            // to create clinician table
            var clinicianPatientCounts = await _context.PatientClinicians
                //to put all patient-clinician relationships into groups by clinician ID
                .GroupBy(pc => pc.ClinicianID)
                // to build a view model that makes objects into new shapes 
                .Select(g => new { ClinicianId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.ClinicianId, x => x.Count);
            // get all clinicians from database
            var clinicianEntities = await _context.Clinicians.ToListAsync();
            // build clinician summaries with patient counts from database 
            var clinicianSummaries = clinicianEntities
                .Select(c => new ClinicianSummary
                {
                    ClinicianID = c.UserId,
                    // concatenate first and last name using string interpolation
                    Name = $"{c.FirstName} {c.LastName}",
                    WorkEmail = c.WorkEmail,
                    PersonalEmail = c.PersonalEmail,
                    IsActive = c.IsActive,
                    NumberOfPatients = clinicianPatientCounts.ContainsKey(c.UserId)
                        ? clinicianPatientCounts[c.UserId]
                        : 0
                })
                .OrderByDescending(c => c.NumberOfPatients)
                .ThenBy(c => c.Name)
                .ToList();

            // build patient table from database
            var patientClinicianCounts = await _context.PatientClinicians
                .GroupBy(pc => pc.PatientID)
                .Select(g => new { PatientId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.PatientId, x => x.Count);

            var patientEntities = await _context.Patients.ToListAsync();
            // build patient summaries with clinician counts from database
            // get patient list 
            var patientSummaries = patientEntities
                .Select(p => new PatientSummary
                {
                    PatientID = p.UserId,
                    Name = $"{p.FirstName} {p.LastName}",
                    // to avoid getting runtime error if DateOfBirth is null
                    DateOfBirth = p.DateOfBirth,
                    IsActive = p.IsActive,
                    AssignedClinicians = patientClinicianCounts.ContainsKey(p.UserId)
                        ? patientClinicianCounts[p.UserId]
                        : 0
                })
                .OrderBy(p => p.Name)
                .ToList();

            // To give view model values from the database
            var viewModel = new AdminDashboardViewModel
            {
                TotalUsers = totalUsers,
                TotalPatients = totalPatients,
                TotalClinicians = totalClinicians,
                ActiveDevices = activeDevices,
                InactiveDevices = inactiveDevices,
                Clinicians = clinicianSummaries,
                Patients = patientSummaries
            };

            _logger.LogInformation(
                "The Admin dashboard loaded: Users - {Users} ,Patients - {Patients} ,Clinicians -  {Clinicians} ,Active Devices -  {ActiveDevices}.",totalUsers, totalPatients, totalClinicians, activeDevices);

            // Return the view with the populated view model
            return View(viewModel);
        }
    }
}

