namespace SensoreApp.Models
{
    
    public class AdminDashbAuditViewModel
    {
        // Lists to hold recent uploads and alerts
        public List<AuditUploadViewModel> RecentUploads { get; set; } = new();
        public List<AuditAlertViewModel> RecentAlerts { get; set; } = new();
    }

    public class AuditUploadViewModel
    {
        // Details of the time of upload and frameID
       public DateTime UploadedAt { get; set; }
       public int FrameID { get; set; }
    }

    public class AuditAlertViewModel
    {
       public int AlertID { get; set; }
       public DateTime CreatedAt { get; set; }
       public int UserID { get; set; }
       public string Status { get; set; } = string.Empty;
       public float? TriggerValue { get; set; }
    }
}
