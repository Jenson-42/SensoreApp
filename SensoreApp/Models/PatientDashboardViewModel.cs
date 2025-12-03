namespace SensoreApp.Models
{
    public class PatientDashboardViewModel
    {
        // Patient Info
        public string PatientName { get; set; } = string.Empty;
        public DateTime LastUploadTime { get; set; }

        // Current Frame Data (32x32 grid)
        public int CurrentFrameID { get; set; }
        public string? HeatmapData { get; set; } // JSON string of 32x32 array

        // Current Metrics
        public double? PeakPressureIndex { get; set; }
        public double? ContactAreaPercent { get; set; }
        public double? COV { get; set; }

        // Recent Alerts (last 3-5 alerts)
        public List<AlertInfo>? RecentAlerts { get; set; }

        // Alert Threshold Setting
        public int AlertThresholdPercent { get; set; } = 80;

        // Comment Section Data
        public List<CommentInfo>? Comments { get; set; }
    }

    // Helper class for displaying alerts
    public class AlertInfo
    {
        public int AlertID { get; set; }
        public string Status { get; set; } = string.Empty; // "Normal" or "High Pressure"
        public DateTime Timestamp { get; set; }
        public string? Reason { get; set; } 
        public float? TriggerValue { get; set; } 
    }

    // Helper class for displaying comments
    public class CommentInfo
    {
        public int CommentID { get; set; }
        public string AuthorName { get; set; } = string.Empty;
        public string AuthorType { get; set; } = string.Empty; // "Patient" or "Clinician"
        public string Text { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
