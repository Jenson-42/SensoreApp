namespace SensoreApp.Models
{
    public class PatientClinician
    {
        
            public int PatientID { get; set; }
            public Patient Patient { get; set; }

            public int ClinicianID { get; set; }
            public Clinician Clinician { get; set; }
        

    }
}
