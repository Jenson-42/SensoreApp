namespace SensoreApp.Models
{
    public class AuditLog
    {
        // additional model created to log important actions for
        // auditing purposes (AssigningClinician, CreatingUser, DeactivateUser, EditUser etc)
        public int AuditLogID { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty; // e.g., "CREATE", "UPDATE", "DELETE"    
        public int PerformedByUserID { get; set; } 
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
