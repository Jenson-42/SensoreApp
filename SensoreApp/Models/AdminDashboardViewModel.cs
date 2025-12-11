using System; 
using System.Collections.Generic; // to allow for collections if needed in future

namespace SensoreApp.Models
{
    // combine information from Users, Patients, Clinicians and SensorDevices for admin dashboard overview
    public class AdminDashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalPatients { get; set; }
        public int TotalClinicians { get; set; }
        public int ActiveDevices { get; set; }
        public int InactiveDevices { get; set; }
        public List<ClinicianSummary> Clinicians { get; set; } = new();
        public List<PatientSummary> Patients { get; set; } = new();
    }

    // summary of Clinician information for admin dashboard
    public class ClinicianSummary
    {
        public int ClinicianID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string WorkEmail { get; set; } = string.Empty;
        public string PersonalEmail { get; set; } = string.Empty;
        public int NumberOfPatients { get; set; }
        public bool IsActive { get; set; }
    }

    // summary of Patient information for admin dashboard
    public class PatientSummary
    {
        public int PatientID { get; set; }
        public string Name { get; set; } = string.Empty;
        // To make patient DateOfBirth nullable as sometimes DateOfBirth might not be provided
        public DateTime? DateOfBirth { get; set; }
        // The amount of clinicians linked to patient 
        public int AssignedClinicians { get; set; }
        public bool IsActive { get; set; }
    }
}
