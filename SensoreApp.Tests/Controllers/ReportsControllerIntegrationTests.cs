using Xunit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.InMemory;
using SensoreApp.Controllers;
using SensoreApp.Data;
using SensoreApp.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SensoreApp.Tests.Controllers
{
    public class ReportsControllerIntegrationTests
    {
        private AppDBContext CreateInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<AppDBContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new AppDBContext(options);
        }

        [Fact]
        public async Task Test71_VerifyReportFrames_SavedCorrectly()
        {
            // Arrange
            using var context = CreateInMemoryContext();

            // Add test patient
            var patient = new Patient
            {
                UserId = 1,
                FirstName = "John",
                LastName = "Doe",
                Email = "john@test.com",
                Phone = "1234567890",
                IsActive = true
            };
            context.Users.Add(patient);

            // Add test frames with metrics
            var frame1 = new FrameMetric { FrameID = 1, PeakPressureIndex = 450, ContactAreaPercent = 60, COV = 0.25, ComputedAt = DateTime.Now.AddHours(-2) };
            var frame2 = new FrameMetric { FrameID = 2, PeakPressureIndex = 480, ContactAreaPercent = 65, COV = 0.30, ComputedAt = DateTime.Now.AddHours(-1) };
            var frame3 = new FrameMetric { FrameID = 3, PeakPressureIndex = 420, ContactAreaPercent = 55, COV = 0.20, ComputedAt = DateTime.Now };

            context.FrameMetrics.AddRange(frame1, frame2, frame3);
            await context.SaveChangesAsync();

            // Create report
            var report = new Report
            {
                RequestedBy = 1,
                UserID = 1,
                DateFrom = DateTime.Now.AddDays(-1),
                DateTo = DateTime.Now,
                GeneratedAt = DateTime.Now,
                ReportType = "Standard"
            };
            context.Reports.Add(report);
            await context.SaveChangesAsync();

            // Act: Add ReportFrames linking frames to report
            var reportFrame1 = new ReportFrame { ReportID = report.ReportID, FrameID = 1 };
            var reportFrame2 = new ReportFrame { ReportID = report.ReportID, FrameID = 2 };
            var reportFrame3 = new ReportFrame { ReportID = report.ReportID, FrameID = 3 };

            context.ReportFrames.AddRange(reportFrame1, reportFrame2, reportFrame3);
            await context.SaveChangesAsync();

            // Assert: Verify ReportFrames were saved correctly
            var savedReportFrames = await context.ReportFrames
                .Where(rf => rf.ReportID == report.ReportID)
                .ToListAsync();

            Assert.Equal(3, savedReportFrames.Count);
            Assert.Contains(savedReportFrames, rf => rf.FrameID == 1);
            Assert.Contains(savedReportFrames, rf => rf.FrameID == 2);
            Assert.Contains(savedReportFrames, rf => rf.FrameID == 3);

            // Verify correct frames are linked
            var linkedFrameIds = savedReportFrames.Select(rf => rf.FrameID).ToList();
            Assert.Contains(1, linkedFrameIds);
            Assert.Contains(2, linkedFrameIds);
            Assert.Contains(3, linkedFrameIds);
        }

        [Fact]
        public async Task Test72_CalculateAverageMetrics_ForPeriod()
        {
            // Arrange
            using var context = CreateInMemoryContext();

            // Add test patient
            var patient = new Patient
            {
                UserId = 1,
                FirstName = "Jane",
                LastName = "Smith",
                Email = "jane@test.com",
                Phone = "9876543210",
                IsActive = true
            };
            context.Users.Add(patient);

            // Add frames with known metrics for averaging
            var testDate = new DateTime(2025, 1, 15);
            var frame1 = new FrameMetric { FrameID = 1, PeakPressureIndex = 400, ContactAreaPercent = 50, COV = 0.20, ComputedAt = testDate.AddHours(1) };
            var frame2 = new FrameMetric { FrameID = 2, PeakPressureIndex = 500, ContactAreaPercent = 60, COV = 0.30, ComputedAt = testDate.AddHours(2) };
            var frame3 = new FrameMetric { FrameID = 3, PeakPressureIndex = 600, ContactAreaPercent = 70, COV = 0.40, ComputedAt = testDate.AddHours(3) };

            context.FrameMetrics.AddRange(frame1, frame2, frame3);
            await context.SaveChangesAsync();

            // Act: Calculate averages manually (simulating what controller does)
            var dateFrom = testDate;
            var dateTo = testDate.AddDays(1);

            var framesInPeriod = await context.FrameMetrics
                .Where(fm => fm.ComputedAt >= dateFrom && fm.ComputedAt <= dateTo)
                .ToListAsync();

            decimal avgPeakPressure = (decimal)framesInPeriod.Average(fm => fm.PeakPressureIndex);
            decimal avgContactArea = (decimal)framesInPeriod.Average(fm => fm.ContactAreaPercent);
            decimal avgCOV = (decimal)framesInPeriod.Average(fm => fm.COV);

            // Assert: Verify averages are calculated correctly
            Assert.Equal(3, framesInPeriod.Count);

            // Expected: (400 + 500 + 600) / 3 = 500
            Assert.Equal(500m, avgPeakPressure);

            // Expected: (50 + 60 + 70) / 3 = 60
            Assert.Equal(60m, avgContactArea);

            // Expected: (0.20 + 0.30 + 0.40) / 3 = 0.30
            Assert.Equal(0.30m, avgCOV, 2); // 2 decimal places precision

            // Create report with calculated metrics
            var report = new Report
            {
                RequestedBy = 1,
                UserID = 1,
                DateFrom = dateFrom,
                DateTo = dateTo,
                GeneratedAt = DateTime.Now,
                ReportType = "Standard"
            };
            context.Reports.Add(report);
            await context.SaveChangesAsync();

            // Save calculated metrics
            var peakMetric = new ReportMetric
            {
                ReportID = report.ReportID,
                MetricName = "Peak Pressure",
                MetricValue = avgPeakPressure
            };

            var contactMetric = new ReportMetric
            {
                ReportID = report.ReportID,
                MetricName = "Contact Area %",
                MetricValue = avgContactArea
            };

            var covMetric = new ReportMetric
            {
                ReportID = report.ReportID,
                MetricName = "Coefficient of Variation",
                MetricValue = avgCOV
            };

            context.ReportMetrics.AddRange(peakMetric, contactMetric, covMetric);
            await context.SaveChangesAsync();

            // Verify metrics saved correctly
            var savedMetrics = await context.ReportMetrics
                .Where(rm => rm.ReportID == report.ReportID)
                .ToListAsync();

            Assert.Equal(3, savedMetrics.Count);

            var savedPeakMetric = savedMetrics.First(m => m.MetricName == "Peak Pressure");
            Assert.Equal(500m, savedPeakMetric.MetricValue);

            var savedContactMetric = savedMetrics.First(m => m.MetricName == "Contact Area %");
            Assert.Equal(60m, savedContactMetric.MetricValue);

            var savedCOVMetric = savedMetrics.First(m => m.MetricName == "Coefficient of Variation");
            Assert.Equal(0.30m, savedCOVMetric.MetricValue, 2);
        }
    }
}