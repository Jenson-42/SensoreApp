namespace SensoreApp.Models
{
    public class AlertSystem
    {
        public int AlertId { get; set; } // needed for the database 
        public int PatientId { get; set; } // links to the patient table 
        public int FrameId { get; set; } // links to the sensor frame that cause the alert
        public string Status { get; set; } = "Active";
        //"Active" is used as a default value to indicate a new alert that is created 
        public string Reason { get; set; } = string.Empty; // e.g., "High Pressure Detected"
        //string.Empty to leave it blank initially
        public int SeverityLevel { get; set; } // e.g., 1 (Low), 2 (Medium), 3 (High)
        public DateTime CreatedAt { get; set; } = DateTime.Now; // timestamp when the alert was created
        public int? ReviewedBy { get; set; } // links to the clinician user who reviewed the alert
        //int? for a value to remain null until review by clinician

    }
}
