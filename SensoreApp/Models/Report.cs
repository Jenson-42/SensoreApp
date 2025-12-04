namespace SensoreApp.Models
{
    public class Report
    {
        public int ReportID { get; set; }
        public int RequestedBy { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
        public DateTime? ComparisonDateFrom { get; set; }
        public DateTime? ComparisonDateTo { get; set; }
        public int UserID { get; set; }
        public string? FilePath { get; set; } // can be null
        public DateTime GeneratedAt { get; set; }
        public string ReportType { get; set; } = string.Empty; // gives it a default empty value

         public User? RequestedByUser { get; set; }
         public User? User { get; set; }
       


        public ICollection<ReportMetric>? ReportMetric { get; set; }
        public ICollection<ReportFrame>? ReportFrame { get; set; }
    }
}
