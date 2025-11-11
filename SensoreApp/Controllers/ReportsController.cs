using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SensoreApp.Data;
using SensoreApp.Models;
using System;
using System.Text;

namespace SensoreApp.Controllers
{
    public class ReportsController : Controller
    {
        private readonly AppDBContext _context;

        public ReportsController(AppDBContext context)
        {
            _context = context;
        }

        // GET: Reports
        // Main report builder page
        public async Task<IActionResult> Index(string userType = "patient", int? userId = null)
        {
            // TODO: Replace with actual authentication
            // For now, using URL parameter: ?userType=patient or ?userType=clinician
            // When authentication exists, get from: User.Claims or Session

            ViewBag.UserType = userType;
            ViewBag.CurrentUserId = userId ?? 1; // Default to user 1 for testing

            // If clinician, get list of assigned patients
            if (userType.ToLower() == "clinician")
            {
                // TODO: Get only assigned patients when ClinicianPatientAssignment table exists
                /*
                var clinicianId = userId ?? 1;
                var assignedPatients = await _context.Users
                    .Join(_context.ClinicianPatientAssignments,
                        user => user.UserID,
                        assignment => assignment.PatientUserID,
                        (user, assignment) => new { user, assignment })
                    .Where(x => x.assignment.ClinicianUserID == clinicianId)
                    .Select(x => new SelectListItem
                    {
                        Value = x.user.UserID.ToString(),
                        Text = x.user.FirstName + " " + x.user.LastName
                    })
                    .ToListAsync();
                ViewBag.PatientList = assignedPatients;
                */

                // Sample patient list for testing
                ViewBag.PatientList = new List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>
                {
                    new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Value = "1", Text = "John Doe" },
                    new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Value = "2", Text = "Jane Smith" },
                    new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Value = "3", Text = "Robert Johnson" }
                };
            }

            return View();
        }

        // POST: Reports/GeneratePreview
        // Generates report preview based on selected criteria
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GeneratePreview(ReportGenerationRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid report parameters");
            }

            // Create report record in database
            var report = new Report
            {
                RequestedBy = request.CurrentUserId,
                DateFrom = request.DateFrom,
                DateTo = request.DateTo,
                ComparisonDateFrom = request.ComparisonDateFrom,
                ComparisonDateTo = request.ComparisonDateTo,
                UserID = request.SelectedPatientId ?? request.CurrentUserId,
                ReportType = request.ComparisonDateFrom.HasValue ? "Comparison" : "Standard",
                GeneratedAt = DateTime.Now
            };

            _context.Reports.Add(report);
            await _context.SaveChangesAsync();

            // Save selected metrics
            if (request.IncludePeakPressure)
            {
                await AddReportMetric(report.ReportID, "Peak Pressure", request.DateFrom, request.DateTo);
            }
            if (request.IncludeContactArea)
            {
                await AddReportMetric(report.ReportID, "Contact Area %", request.DateFrom, request.DateTo);
            }
            if (request.IncludeCOV)
            {
                await AddReportMetric(report.ReportID, "Coefficient of Variation", request.DateFrom, request.DateTo);
            }

            // Save selected frames
            if (request.SelectedFrameIds != null && request.SelectedFrameIds.Any())
            {
                foreach (var frameId in request.SelectedFrameIds)
                {
                    var reportFrame = new ReportFrame
                    {
                        ReportID = report.ReportID,
                        FrameID = frameId
                    };
                    _context.ReportFrames.Add(reportFrame);
                }
                await _context.SaveChangesAsync();
            }

            // Redirect to preview page with report ID
            return RedirectToAction(nameof(Preview), new { id = report.ReportID });
        }

        // GET: Reports/Preview/5
        // Shows report preview before download
        public async Task<IActionResult> Preview(int id)
        {
            var report = await _context.Reports
                .Include(r => r.ReportMetric)
                .Include(r => r.ReportFrame)
                .FirstOrDefaultAsync(r => r.ReportID == id);

            if (report == null)
            {
                return NotFound();
            }

            // Build preview view model
            var viewModel = new ReportPreviewViewModel
            {
                ReportID = report.ReportID,
                PatientName = "John Doe", // TODO: Get from Users table
                DateFrom = report.DateFrom,
                DateTo = report.DateTo,
                ComparisonDateFrom = report.ComparisonDateFrom,
                ComparisonDateTo = report.ComparisonDateTo,
                GeneratedAt = report.GeneratedAt,
                ReportType = report.ReportType,
                Metrics = new List<ReportMetricDisplay>()
            };

            // Get metric data
            foreach (var metric in report.ReportMetric)
            {
                viewModel.Metrics.Add(new ReportMetricDisplay
                {
                    MetricName = metric.MetricName,
                    CurrentValue = metric.MetricValue,
                    ComparisonValue = metric.ComparisonValue
                });
            }

            // Get frame data
            // TODO: Load actual frame data and heatmaps when SensorFrames exists
            viewModel.FrameCount = report.ReportFrame?.Count ?? 0;

            return View(viewModel);
        }

        // GET: Reports/DownloadPDF/5
        // Downloads the report as PDF
        public async Task<IActionResult> DownloadPDF(int id)
        {
            var report = await _context.Reports
                .Include(r => r.ReportMetric)
                .Include(r => r.ReportFrame)
                .FirstOrDefaultAsync(r => r.ReportID == id);

            if (report == null)
            {
                return NotFound();
            }

            // TODO: Implement actual PDF generation library (e.g., iTextSharp, SelectPdf, or Rotativa)
            // For now, create a simple text-based "PDF" as placeholder

            var pdfContent = GeneratePdfContent(report);
            var bytes = Encoding.UTF8.GetBytes(pdfContent);

            return File(bytes, "application/pdf", $"Report_{report.ReportID}_{DateTime.Now:yyyyMMdd}.pdf");
        }

        // Helper method to add metric calculations
        private async Task AddReportMetric(int reportId, string metricName, DateTime dateFrom, DateTime dateTo)
        {
            // TODO: Calculate actual metrics from FrameMetrics table
            /*
            var avgValue = await _context.FrameMetrics
                .Where(fm => fm.ComputedAt >= dateFrom && fm.ComputedAt <= dateTo)
                .AverageAsync(fm => metricName == "Peak Pressure" ? fm.PeakPressureIndex :
                                    metricName == "Contact Area %" ? fm.ContactAreaPercent :
                                    fm.COV);
            */

            // Sample data for now
            var sampleValue = metricName switch
            {
                "Peak Pressure" => 185.50m,
                "Contact Area %" => 67.30m,
                "Coefficient of Variation" => 0.2456m,
                _ => 0m
            };

            var reportMetric = new ReportMetric
            {
                ReportID = reportId,
                MetricName = metricName,
                MetricValue = sampleValue,
                ComparisonValue = sampleValue * 0.95m // 5% lower for comparison (sample)
            };

            _context.ReportMetrics.Add(reportMetric);
            await _context.SaveChangesAsync();
        }

        // Helper method to generate PDF content (placeholder)
        private string GeneratePdfContent(Report report)
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== SENSORE PRESSURE ANALYSIS REPORT ===");
            sb.AppendLine();
            sb.AppendLine($"Report ID: {report.ReportID}");
            sb.AppendLine($"Generated: {report.GeneratedAt:dd/MM/yyyy HH:mm:ss}");
            sb.AppendLine($"Period: {report.DateFrom:dd/MM/yyyy} to {report.DateTo:dd/MM/yyyy}");
            sb.AppendLine();
            sb.AppendLine("=== METRICS ===");

            foreach (var metric in report.ReportMetric)
            {
                sb.AppendLine($"{metric.MetricName}: {metric.MetricValue:F2}");
                if (metric.ComparisonValue.HasValue)
                {
                    sb.AppendLine($"  Comparison: {metric.ComparisonValue.Value:F2}");
                }
            }

            sb.AppendLine();
            sb.AppendLine($"Total Frames Analyzed: {report.ReportFrame?.Count ?? 0}");
            sb.AppendLine();
            sb.AppendLine("Note: This is a placeholder PDF. Actual PDF generation will be implemented.");

            return sb.ToString();
        }
    }

    // Helper classes for report generation
    public class ReportGenerationRequest
    {
        public int CurrentUserId { get; set; }
        public int? SelectedPatientId { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
        public DateTime? ComparisonDateFrom { get; set; }
        public DateTime? ComparisonDateTo { get; set; }
        public bool IncludePeakPressure { get; set; }
        public bool IncludeContactArea { get; set; }
        public bool IncludeCOV { get; set; }
        public bool IncludeClinicianNotes { get; set; }
        public bool IncludeAlertSummary { get; set; }
        public List<int>? SelectedFrameIds { get; set; }
    }

    public class ReportPreviewViewModel
    {
        public int ReportID { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
        public DateTime? ComparisonDateFrom { get; set; }
        public DateTime? ComparisonDateTo { get; set; }
        public DateTime GeneratedAt { get; set; }
        public string ReportType { get; set; } = string.Empty;
        public List<ReportMetricDisplay> Metrics { get; set; } = new();
        public int FrameCount { get; set; }
    }

    public class ReportMetricDisplay
    {
        public string MetricName { get; set; } = string.Empty;
        public decimal CurrentValue { get; set; }
        public decimal? ComparisonValue { get; set; }
    }
}
