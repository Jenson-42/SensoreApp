namespace SensoreApp.Models
{
    // model 
    public class Clinician : User
    {
        // personal details
        public int ClinicianId { get; set; }
        public required String WorkEmail { get; set; }
        public required String PersonalEmail { get; set; }


    }
}
