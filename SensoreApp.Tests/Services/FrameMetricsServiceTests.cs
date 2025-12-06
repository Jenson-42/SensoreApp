using Xunit;
using SensoreApp.Services;
using System;
using System.Collections.Generic;

namespace SensoreApp.Tests.Services
{
    public class FrameMetricsServiceTests
    {
        private readonly FrameMetricsService _service;

        public FrameMetricsServiceTests()
        {
            _service = new FrameMetricsService();
        }

        // =====================================
        // PEAK PRESSURE INDEX TESTS (3 tests)
        // =====================================

        [Fact]
        public void Test16_CalculatePeakPressureIndex_WithNormalData_ReturnsCorrectPeak()
        {
            // Arrange
            int[] testFrame = new int[1024];
            int expectedPeak = 450;

            // Create frame with known peak pressure in center region
            for (int i = 0; i < 1024; i++)
            {
                int row = i / 32;
                int col = i % 32;

                // High pressure region (center) - 9x9 = 81 pixels (> 10 minimum)
                if (row >= 12 && row <= 20 && col >= 12 && col <= 20)
                {
                    testFrame[i] = 450;  // Expected peak
                }
                // Medium pressure around it
                else if (row >= 8 && row <= 24 && col >= 8 && col <= 24)
                {
                    testFrame[i] = 200;
                }
                // Low pressure elsewhere
                else
                {
                    testFrame[i] = 15;
                }
            }

            // Act
            var result = _service.CalculateMetrics(testFrame, 1);

            // Assert
            Assert.Equal(expectedPeak, result.PeakPressureIndex);
            Assert.Equal(1, result.FrameID);
        }

        [Fact]
        public void Test17_CalculatePeakPressureIndex_WithAllZeros_ReturnsZero()
        {
            // Arrange
            int[] testFrame = new int[1024]; // All zeros by default

            // Act
            var result = _service.CalculateMetrics(testFrame, 2);

            // Assert
            Assert.Equal(0, result.PeakPressureIndex);
        }

        [Fact]
        public void Test18_CalculatePeakPressureIndex_WithScatteredNoise_IgnoresSmallRegions()
        {
            // Arrange
            int[] testFrame = new int[1024];

            // Fill with low values (below contact threshold of 30)
            for (int i = 0; i < 1024; i++)
            {
                testFrame[i] = 15;
            }

            // Add scattered high values (< 10 pixels, not clustered)
            testFrame[100] = 600;  // Single isolated high pixel
            testFrame[500] = 650;  // Another isolated pixel
            testFrame[900] = 700;  // Another isolated pixel

            // Act
            var result = _service.CalculateMetrics(testFrame, 3);

            // Assert
            // Should return low value (15) or 0, not the scattered high values
            Assert.True(result.PeakPressureIndex < 100, 
                "Peak pressure should ignore scattered noise pixels under 10 in size");
        }

        // =====================================
        // CONTACT AREA PERCENT TESTS (3 tests)
        // =====================================

        [Fact]
        public void Test19_CalculateContactAreaPercent_WithFullContact_Returns100Percent()
        {
            // Arrange
            int[] testFrame = new int[1024];

            // Fill all pixels with values above contact threshold (30)
            for (int i = 0; i < 1024; i++)
            {
                testFrame[i] = 100;  // Above threshold
            }

            // Act
            var result = _service.CalculateMetrics(testFrame, 4);

            // Assert
            Assert.Equal(100.0, result.ContactAreaPercent);
        }

        [Fact]
        public void Test20_CalculateContactAreaPercent_WithNoContact_ReturnsZero()
        {
            // Arrange
            int[] testFrame = new int[1024];

            // Fill all pixels with values below contact threshold (30)
            for (int i = 0; i < 1024; i++)
            {
                testFrame[i] = 20;  // Below threshold of 30
            }

            // Act
            var result = _service.CalculateMetrics(testFrame, 5);

            // Assert
            Assert.Equal(0.0, result.ContactAreaPercent);
        }

        [Fact]
        public void Test21_CalculateContactAreaPercent_With50PercentContact_Returns50Percent()
        {
            // Arrange
            int[] testFrame = new int[1024];

            // Fill exactly half (512 pixels) with contact, half without
            for (int i = 0; i < 1024; i++)
            {
                if (i < 512)
                {
                    testFrame[i] = 100;  // Above threshold
                }
                else
                {
                    testFrame[i] = 20;   // Below threshold
                }
            }

            // Act
            var result = _service.CalculateMetrics(testFrame, 6);

            // Assert
            Assert.Equal(50.0, result.ContactAreaPercent);
        }

        // =====================================
        // COV (COEFFICIENT OF VARIATION) TESTS (3 tests)
        // =====================================

        [Fact]
        public void Test22_CalculateCOV_WithUniformPressure_ReturnsLowCOV()
        {
            // Arrange
            int[] testFrame = new int[1024];

            // Fill with uniform pressure (all same value above threshold)
            for (int i = 0; i < 1024; i++)
            {
                testFrame[i] = 150;  // All same value
            }

            // Act
            var result = _service.CalculateMetrics(testFrame, 7);

            // Assert
            // COV should be 0 for perfectly uniform distribution
            Assert.Equal(0.0, result.COV);
        }

        [Fact]
        public void Test23_CalculateCOV_WithConcentratedPressure_ReturnsHighCOV()
        {
            // Arrange
            int[] testFrame = new int[1024];

            // Create highly variable pressure (concentrated in one spot)
            for (int i = 0; i < 1024; i++)
            {
                int row = i / 32;
                int col = i % 32;

                // Very high pressure in small center region
                if (row >= 14 && row <= 18 && col >= 14 && col <= 18)
                {
                    testFrame[i] = 800;  // Very high
                }
                // Low pressure elsewhere
                else if (row >= 8 && row <= 24 && col >= 8 && col <= 24)
                {
                    testFrame[i] = 50;   // Low
                }
                else
                {
                    testFrame[i] = 20;   // Below threshold
                }
            }

            // Act
            var result = _service.CalculateMetrics(testFrame, 8);

            // Assert
            // COV should be high (> 0.5) due to high variability
            Assert.True(result.COV > 0.5, 
                $"COV should be high for concentrated pressure. Got: {result.COV}");
        }

        [Fact]
        public void Test24_CalculateCOV_WithNoContactPixels_ReturnsZero()
        {
            // Arrange
            int[] testFrame = new int[1024];

            // All pixels below contact threshold (30)
            for (int i = 0; i < 1024; i++)
            {
                testFrame[i] = 15;  // Below threshold
            }

            // Act
            var result = _service.CalculateMetrics(testFrame, 9);

            // Assert
            // COV should be 0 when no contact pixels exist
            Assert.Equal(0.0, result.COV);
        }

        // =====================================
        // FRAME SIZE VALIDATION TESTS (3 tests)
        // =====================================

        [Fact]
        public void Test25_CalculateMetrics_WithExactly1024Values_ProcessesSuccessfully()
        {
            // Arrange
            int[] testFrame = new int[1024];
            for (int i = 0; i < 1024; i++)
            {
                testFrame[i] = 100;
            }

            // Act
            var result = _service.CalculateMetrics(testFrame, 10);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(10, result.FrameID);
            Assert.True(result.PeakPressureIndex > 0);
        }

        [Fact]
        public void Test26_CalculateMetrics_With1023Values_ThrowsArgumentException()
        {
            // Arrange
            int[] wrongSizeFrame = new int[1023]; // Wrong size

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => 
                _service.CalculateMetrics(wrongSizeFrame, 11));
            
            Assert.Contains("1024", exception.Message);
        }

        [Fact]
        public void Test27_CalculateMetrics_WithNullFrame_ThrowsArgumentException()
        {
            // Arrange
            int[] nullFrame = null;

            // Act & Assert
            Assert.Throws<ArgumentException>(() => 
                _service.CalculateMetrics(nullFrame, 12));
        }

        // =====================================
        // PIXEL CLUSTERING TESTS (2 tests)
        // =====================================

        [Fact]
        public void Test28_ArePixelsClustered_WithAdjacentPixels_ReturnsTrue()
        {
            // Arrange
            // Create a 3x3 cluster in the grid (indices for adjacent pixels)
            // Row 10, Cols 10-12 = indices 330, 331, 332
            // Row 11, Cols 10-12 = indices 362, 363, 364
            var clusteredIndices = new List<int> { 330, 331, 332, 362, 363, 364 };

            // Act
            // Use reflection to access private method
            var method = typeof(FrameMetricsService).GetMethod(
                "ArePixelsClustered", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            var result = (bool)method.Invoke(_service, new object[] { clusteredIndices });

            // Assert
            Assert.True(result, "Adjacent pixels should be detected as clustered");
        }

        [Fact]
        public void Test29_ArePixelsClustered_WithScatteredPixels_ReturnsFalse()
        {
            // Arrange
            // Create scattered pixels (far apart, not adjacent)
            var scatteredIndices = new List<int> { 10, 200, 500, 800, 1000 };

            // Act
            var method = typeof(FrameMetricsService).GetMethod(
                "ArePixelsClustered", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            var result = (bool)method.Invoke(_service, new object[] { scatteredIndices });

            // Assert
            Assert.False(result, "Scattered pixels should not be detected as clustered");
        }

        // =====================================
        // CSV PARSING TEST (1 test)
        // =====================================

        [Fact]
        public void Test30_ParseFrameFromCSV_With32ValidRows_Returns1024ElementArray()
        {
            // Arrange
            var csvRows = new List<string[]>();

            // Create 32 rows, each with 32 columns
            for (int row = 0; row < 32; row++)
            {
                var rowData = new string[32];
                for (int col = 0; col < 32; col++)
                {
                    rowData[col] = (row * 32 + col).ToString(); // Sequential values
                }
                csvRows.Add(rowData);
            }

            // Act
            var result = _service.ParseFrameFromCSV(csvRows);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1024, result.Length);
            
            // Verify first and last values are correct
            Assert.Equal(0, result[0]);      // First value (0,0)
            Assert.Equal(1023, result[1023]); // Last value (31,31)
            
            // Verify a middle value
            Assert.Equal(512, result[512]);  // Middle value (16,0)
        }
    }
}