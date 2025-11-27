using SensoreApp.Models;

namespace SensoreApp.Services
{
    public class FrameMetricsService
    {
        // Thresholds for 12-bit sensor (0-4095 range)
        private const int CONTACT_THRESHOLD = 30;        // Values > 30 indicate actual contact
        private const int HIGH_PRESSURE_THRESHOLD = 500; // Values > 500 are high pressure
        private const int MIN_REGION_SIZE = 10;          // Minimum pixels for valid pressure region

        /// <summary>
        /// Calculates all metrics from a 32x32 pressure frame
        /// </summary>
        /// <param name="frameData">1024-element array representing 32x32 grid (row-major order)</param>
        /// <param name="frameId">Frame identifier</param>
        /// <returns>FrameMetric object with calculated values</returns>
        public FrameMetric CalculateMetrics(int[] frameData, int frameId)
        {
            if (frameData == null || frameData.Length != 1024)
            {
                throw new ArgumentException("Frame data must be exactly 1024 values (32x32 grid)");
            }

            var metrics = new FrameMetric
            {
                FrameID = frameId,
                PeakPressureIndex = CalculatePeakPressureIndex(frameData),
                ContactAreaPercent = CalculateContactAreaPercent(frameData),
                COV = CalculateCOV(frameData),
                ComputedAt = DateTime.Now
            };

            return metrics;
        }

        /// <summary>
        /// Calculates Peak Pressure Index
        /// Returns highest pressure value, excluding regions smaller than 10 pixels
        /// </summary>
        private double CalculatePeakPressureIndex(int[] frameData)
        {
            // Find all high-pressure pixels (above threshold)
            var highPressurePixels = new List<int>();
            for (int i = 0; i < frameData.Length; i++)
            {
                if (frameData[i] > HIGH_PRESSURE_THRESHOLD)
                {
                    highPressurePixels.Add(i);
                }
            }

            // If no high pressure detected, return maximum value in frame
            if (highPressurePixels.Count == 0)
            {
                int maxValue = frameData.Max();
                return maxValue > CONTACT_THRESHOLD ? maxValue : 0;
            }

            // If high pressure region is too small, check if clustered
            if (highPressurePixels.Count < MIN_REGION_SIZE)
            {
                if (!ArePixelsClustered(highPressurePixels))
                {
                    return 0; // Scattered pixels = likely noise
                }
            }

            // Return the maximum pressure value
            return frameData.Max();
        }

        /// <summary>
        /// Calculates Contact Area Percentage
        /// Returns percentage of sensor mat in contact with surface
        /// </summary>
        private double CalculateContactAreaPercent(int[] frameData)
        {
            // Count pixels above contact threshold
            int contactPixels = frameData.Count(value => value > CONTACT_THRESHOLD);

            // Total pixels = 32 x 32 = 1024
            double totalPixels = 1024.0;

            // Calculate percentage
            double percentage = (contactPixels / totalPixels) * 100.0;

            return Math.Round(percentage, 2);
        }

        /// <summary>
        /// Calculates Coefficient of Variation (COV)
        /// Measures pressure distribution variability
        /// </summary>
        private double CalculateCOV(int[] frameData)
        {
            // Get only contact pixels (exclude non-contact areas)
            var contactPixels = frameData.Where(p => p > CONTACT_THRESHOLD).ToArray();

            // Need at least 2 pixels for variance calculation
            if (contactPixels.Length < 2)
            {
                return 0;
            }

            // Calculate mean (average pressure)
            double mean = contactPixels.Average();

            // Calculate standard deviation
            double sumSquaredDifferences = contactPixels.Sum(value => Math.Pow(value - mean, 2));
            double variance = sumSquaredDifferences / contactPixels.Length;
            double standardDeviation = Math.Sqrt(variance);

            // COV = Standard Deviation / Mean
            // Avoid division by zero
            if (mean == 0)
            {
                return 0;
            }

            double cov = standardDeviation / mean;

            return Math.Round(cov, 4);
        }

        /// <summary>
        /// Helper method: Checks if high-pressure pixels are clustered together
        /// Used to distinguish real pressure regions from scattered noise
        /// </summary>
        private bool ArePixelsClustered(List<int> pixelIndices)
        {
            if (pixelIndices.Count < 2)
            {
                return false;
            }

            // Convert flat indices to 2D coordinates
            var coordinates = pixelIndices.Select(idx => new
            {
                Row = idx / 32,
                Col = idx % 32
            }).ToList();

            // Check if at least some pixels are adjacent
            int adjacentPairs = 0;
            for (int i = 0; i < coordinates.Count; i++)
            {
                for (int j = i + 1; j < coordinates.Count; j++)
                {
                    int rowDiff = Math.Abs(coordinates[i].Row - coordinates[j].Row);
                    int colDiff = Math.Abs(coordinates[i].Col - coordinates[j].Col);

                    // Adjacent if within 1 row and 1 column
                    if (rowDiff <= 1 && colDiff <= 1)
                    {
                        adjacentPairs++;
                    }
                }
            }

            // If more than half the pixels have neighbors, they're clustered
            return adjacentPairs >= pixelIndices.Count / 2;
        }

        /// <summary>
        /// Parses 32 rows from CSV into a single 1024-element frame array
        /// </summary>
        /// <param name="csvRows">32 rows, each containing 32 comma-separated values</param>
        /// <returns>1024-element array in row-major order</returns>
        public int[] ParseFrameFromCSV(List<string[]> csvRows)
        {
            if (csvRows == null || csvRows.Count != 32)
            {
                throw new ArgumentException("Must provide exactly 32 rows for one frame");
            }

            int[] frameData = new int[1024];

            for (int row = 0; row < 32; row++)
            {
                if (csvRows[row].Length < 32)
                {
                    throw new ArgumentException($"Row {row} has fewer than 32 columns");
                }

                for (int col = 0; col < 32; col++)
                {
                    if (!int.TryParse(csvRows[row][col], out int value))
                    {
                        throw new ArgumentException($"Invalid value at row {row}, col {col}");
                    }

                    frameData[row * 32 + col] = value;
                }
            }

            return frameData;
        }
    }
}
