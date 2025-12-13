using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SensoreApp.Data; // 
using SensoreApp.Models;
using System.Linq;
using System.Threading.Tasks;

// GET ([HttpGet] - To show
// POST ([HttpPost]) - To submit 

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
            // updates to get total patients based on type of user created on Create New Account Page 
            var totalPatients = await _context.Users.CountAsync(u => u.Role == UserRole.Patient);
            // updates to get total clinicians based on type of user created on Create New Account Page
            var totalClinicians = await _context.Users.CountAsync(u => u.Role == UserRole.Clinician);
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
                "The Admin dashboard loaded: Users - {Users} ,Patients - {Patients} ,Clinicians -  {Clinicians} ,Active Devices -  {ActiveDevices}.", totalUsers, totalPatients, totalClinicians, activeDevices);

            // Return the view with the populated view model
            return View(viewModel);
        }
        // using HttpGet to show the manage users page
        public async Task<IActionResult> ManageUsers(UserRole? role)
        {
            // to get all users from database for admin dashboard
            // initial query to get all users
            var usersQuery = _context.Users.AsQueryable();

            if (role.HasValue)
            {
                // filter users by role if specified
                usersQuery = usersQuery.Where(u => u.Role == role.Value);
            }
            // execute the query and get the list of users
            var users = await usersQuery.ToListAsync();
            // to pass users to the view 
            return View(users);
        }

        // To create user page 
        // shows the user form 
        [HttpGet]
        public IActionResult CreateUser()
        {
            return View(new User());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        //submits the user form 
        public async Task<IActionResult> CreateUser(User user)
        {
            if (!ModelState.IsValid)
            {
                return View(user);
            }
            // set new user to active by default
            user.IsActive = true;

            // will save new user to the database
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            // to redirect to admin dashboard index after creating user
            return RedirectToAction("Index");

        }
        // to save the users 
        public async Task<IActionResult> SaveUsers()
        {
            var users = await _context.Users.ToListAsync();
            return View(users);
        }
        [HttpPost]
        public async Task<IActionResult> DeactivateUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            // Deactivate the user by setting IsActive to false
            user.IsActive = false;
            await _context.SaveChangesAsync();
            // Redirect back to the Manage Users view
            return RedirectToAction("ManageUsers");
        }

        // to show edit user form
        [HttpGet]
        public async Task<IActionResult> EditUsers(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }

        // to submit edited user form
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUsers(User user)
        {
            if (!ModelState.IsValid)
            {
                return View(user);
            }
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            return RedirectToAction("ManageUsers");
        }
        public IActionResult AuditLogs()
        {
            return View();
        }
        [HttpGet]
        public async Task<IActionResult> AssignClinician()
        {
            // Load all patients
            var patients = await _context.Users
                // filter to get only active patients for assignment
                .Where(u => u.Role == UserRole.Patient && u.IsActive)
                // to get patient names
                // create anonymous object with UserId and concatenated Name
                .Select(u => new { u.UserId, Name = u.FirstName + " " + u.LastName }).ToListAsync();

            // Load all clinicians
            var clinicians = await _context.Users
                // filter to get only active clinicians for assignment
                .Where(u => u.Role == UserRole.Clinician && u.IsActive)
                // to get clinician names
                // create anonymous object with UserId and concatenated Name
                .Select(u => new { u.UserId, Name = u.FirstName + " " + u.LastName })
                .ToListAsync();
            // pass patients and clinicians to the view using ViewBag
            ViewBag.Patients = patients;
            ViewBag.Clinicians = clinicians;

            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignClinician(int patientId, int clinicianId)
        {
            // Basic validation
            // avoid assigning if either id is 0 (bad inserts)
            if (patientId == 0 || clinicianId == 0)
            {
                TempData["Error"] = "Please select both a patient and a clinician.";
                return RedirectToAction("AssignClinician");
            }

            // Check if the assignment already exists
            var assignment = new PatientClinician
            {
                PatientID = patientId,
                ClinicianID = clinicianId

            };

            // to check if assignment already exists in the database
            _context.PatientClinicians.Add(assignment);
            // save changes to the database
            await _context.SaveChangesAsync();

            // Provide feedback 
            TempData["Success"] = "Clinician assigned successfully!";
            // redirect to admin dashboard index after assignment
            return RedirectToAction("Index");
            


        }
    }    }    

