namespace SensoreApp.Models
{
    public class FrameMetric
    {
        public int FrameMetricID { get; set; }
        public int FrameID { get; set; }
        public double PeakPressureIndex { get; set; }
        public double ContactAreaPercent { get; set; }
        public double COV { get; set; }
        public DateTime ComputedAt { get; set; }

        // Navigation property - connects to SensorFrames table commented out until teammate creates it
        //public SensorFrames? Frame { get; set; }
    }
}
