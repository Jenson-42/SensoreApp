namespace SensoreApp.Models
{
    public class ReportMetric
    {
        public int ReportMetricID { get; set; }
        public int ReportID { get; set; }
        public string MetricName { get; set; } = string.Empty; // gives it a default empty value which prevents null issues
        public decimal MetricValue { get; set; }
        public decimal? ComparisonValue { get; set; }

        // Navigation property
        public Report? Report { get; set; }
    }
}
