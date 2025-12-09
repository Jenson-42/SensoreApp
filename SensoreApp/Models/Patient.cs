namespace SensoreApp.Models
{
    public class Patient : User
    {
        public DateTime? DateOfBirth { get; set; }
        public ICollection<PatientClinician> PatientClinicians { get; set; } = new List<PatientClinician>();
    }
}