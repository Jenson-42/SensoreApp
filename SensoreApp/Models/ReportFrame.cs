namespace SensoreApp.Models
{
    public class ReportFrame
    {
        public int ReportFrameID { get; set; }
        public int ReportID { get; set; }
        public int FrameID { get; set; }

        // Navigation properties
        public Report? Report { get; set; }

        // This will be enabled when teammate creates SensorFrames model
        // public SensorFrames? Frame { get; set; }
    }
}
