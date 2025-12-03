using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SensoreApp.Data;
using SensoreApp.Models;
using Rotativa.AspNetCore;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

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
        public async Task<IActionResult> Index(string userType = "patient", int? userId = null, DateTime? dateFrom = null, DateTime? dateTo = null)
        {
            // TODO: Replace with actual authentication
            // For now, using URL parameter: ?userType=patient or ?userType=clinician

            ViewBag.UserType = userType;
            ViewBag.CurrentUserId = userId ?? 1;

            // If clinician, get list of assigned patients
            if (userType.ToLower() == "clinician")
            {
                // Get REAL list of patients from database
                ViewBag.PatientList = await _context.Users
                    .Where(u => u.IsActive) // Only active users
                    .Select(u => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                    {
                        Value = u.UserId.ToString(),
                        Text = $"{u.FirstName} {u.LastName}"
                    })
                    .ToListAsync();
            }

            // Load available snapshots based on date range
            if (!dateFrom.HasValue)
            {
                // Default: last 30 days to ensure we capture uploaded data
                dateFrom = DateTime.Now.AddDays(-30);
            }
            if (!dateTo.HasValue)
            {
                // Default: tomorrow (to include today's data)
                dateTo = DateTime.Now.AddDays(1);
            }

            // Get frames within date range
            var allFrames = await _context.FrameMetrics
                .Where(m => m.ComputedAt >= dateFrom && m.ComputedAt <= dateTo)
                .OrderBy(m => m.ComputedAt)
                .ToListAsync();

            // Smart sampling: Limit to maximum 20 frames, evenly distributed
            int maxFrames = 20;
            var sampledFrames = new List<dynamic>();

            if (allFrames.Count <= maxFrames)
            {
                // Show all frames if 20 or fewer
                sampledFrames = allFrames.Select(m => new
                {
                    m.FrameID,
                    m.FrameMetricID,
                    m.ComputedAt
                }).Cast<dynamic>().ToList();
            }
            else
            {
                // Sample evenly across the time period
                int interval = allFrames.Count / maxFrames;

                for (int i = 0; i < allFrames.Count; i += interval)
                {
                    if (sampledFrames.Count >= maxFrames) break;

                    var frame = allFrames[i];
                    sampledFrames.Add(new
                    {
                        frame.FrameID,
                        frame.FrameMetricID,
                        frame.ComputedAt
                    });
                }
            }

            ViewBag.AvailableFrames = sampledFrames;
            ViewBag.TotalFramesInRange = allFrames.Count;
            ViewBag.DateFrom = dateFrom;
            ViewBag.DateTo = dateTo;

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

            // Save selected metrics with comparison data
            if (request.IncludePeakPressure)
            {
                await AddReportMetric(report.ReportID, "Peak Pressure", request.DateFrom, request.DateTo, request.ComparisonDateFrom, request.ComparisonDateTo);
            }
            if (request.IncludeContactArea)
            {
                await AddReportMetric(report.ReportID, "Contact Area %", request.DateFrom, request.DateTo, request.ComparisonDateFrom, request.ComparisonDateTo);
            }
            if (request.IncludeCOV)
            {
                await AddReportMetric(report.ReportID, "Coefficient of Variation", request.DateFrom, request.DateTo, request.ComparisonDateFrom, request.ComparisonDateTo);
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
            // Get real patient name first
            var user = await _context.Users.FindAsync(report.UserID);
            // Build preview view model
            var viewModel = new ReportPreviewViewModel
            {
                ReportID = report.ReportID,
                PatientName = user != null ? $"{user.FirstName} {user.LastName}" : "Unknown User",
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
            // Get chart data from selected frames
            if (report.ReportFrame != null && report.ReportFrame.Any())
            {
                var frameIds = report.ReportFrame.Select(rf => rf.FrameID).ToList();

                var frameMetrics = await _context.FrameMetrics
                    .Where(fm => frameIds.Contains(fm.FrameID))
                    .OrderBy(fm => fm.ComputedAt)
                    .ToListAsync();

                foreach (var frame in frameMetrics)
                {
                    viewModel.ChartLabels.Add(frame.ComputedAt.ToString("HH:mm"));
                    viewModel.PeakPressureData.Add((decimal)frame.PeakPressureIndex);
                    viewModel.ContactAreaData.Add((decimal)frame.ContactAreaPercent);
                    viewModel.COVData.Add((decimal)frame.COV);
                }
            }
            
            viewModel.PeakPressureData = viewModel.PeakPressureData
                .Select(x => decimal.Parse(x.ToString(CultureInfo.InvariantCulture)))
                .ToList();

            viewModel.ContactAreaData = viewModel.ContactAreaData
                .Select(x => decimal.Parse(x.ToString(CultureInfo.InvariantCulture)))
                .ToList();

            viewModel.COVData = viewModel.COVData
                .Select(x => decimal.Parse(x.ToString(CultureInfo.InvariantCulture)))
                .ToList();
            

            // Generate chart URL
            if (viewModel.ChartLabels.Any()) 
            {
                viewModel.ChartImageUrl = GenerateChartImageUrl(
                    viewModel.ChartLabels,
                    viewModel.PeakPressureData,
                    viewModel.ContactAreaData,
                    viewModel.COVData
                );
            }


            return View(viewModel);
        }

        // GET: Reports/DownloadPDF/5
        // Downloads the report as PDF using Rotativa
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
            // Get real patient name
            var user = await _context.Users.FindAsync(report.UserID);
            // Build preview view model (same as Preview action)
            var viewModel = new ReportPreviewViewModel
            {
                ReportID = report.ReportID,
                PatientName = user != null ? $"{user.FirstName} {user.LastName}" : "Unknown User",
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

            viewModel.FrameCount = report.ReportFrame?.Count ?? 0;
            if (report.ReportFrame != null && report.ReportFrame.Any())
            {
                var frameIds = report.ReportFrame.Select(rf => rf.FrameID).ToList();

                var frameMetrics = await _context.FrameMetrics
                    .Where(fm => frameIds.Contains(fm.FrameID))
                    .OrderBy(fm => fm.ComputedAt)
                    .ToListAsync();

                foreach (var frame in frameMetrics)
                {
                    viewModel.ChartLabels.Add(frame.ComputedAt.ToString("HH:mm"));
                    viewModel.PeakPressureData.Add((decimal)frame.PeakPressureIndex);
                    viewModel.ContactAreaData.Add((decimal)frame.ContactAreaPercent);
                    viewModel.COVData.Add((decimal)frame.COV);
                }
            }
            
            viewModel.PeakPressureData = viewModel.PeakPressureData
                .Select(x => decimal.Parse(x.ToString(CultureInfo.InvariantCulture)))
                .ToList();

            viewModel.ContactAreaData = viewModel.ContactAreaData
                .Select(x => decimal.Parse(x.ToString(CultureInfo.InvariantCulture)))
                .ToList();

            viewModel.COVData = viewModel.COVData
                .Select(x => decimal.Parse(x.ToString(CultureInfo.InvariantCulture)))
                .ToList();
          
            // Generate chart URL
            if (viewModel.ChartLabels.Any())
            {
                viewModel.ChartImageUrl = GenerateChartImageUrl(
                    viewModel.ChartLabels,
                    viewModel.PeakPressureData,
                    viewModel.ContactAreaData,
                    viewModel.COVData
                );
            }


            // Generate PDF from PreviewPDF view
            return new ViewAsPdf("PreviewPDF", viewModel)
            {
                FileName = $"SensoreReport_{report.ReportID}_{DateTime.Now:yyyyMMdd}.pdf"
            };
        }

        // Helper method to add metric calculations
        private async Task AddReportMetric(int reportId, string metricName, DateTime dateFrom, DateTime dateTo, DateTime? comparisonDateFrom = null, DateTime? comparisonDateTo = null)
        {
            // Calculate REAL metric value from FrameMetrics table for main period
            var mainPeriodFrames = await _context.FrameMetrics
                .Where(fm => fm.ComputedAt >= dateFrom && fm.ComputedAt <= dateTo)
                .ToListAsync();

            decimal metricValue = 0m;

            if (mainPeriodFrames.Any())
            {
                metricValue = metricName switch
                {
                    "Peak Pressure" => (decimal)mainPeriodFrames.Average(fm => fm.PeakPressureIndex),
                    "Contact Area %" => (decimal)mainPeriodFrames.Average(fm => fm.ContactAreaPercent),
                    "Coefficient of Variation" => (decimal)mainPeriodFrames.Average(fm => fm.COV),
                    _ => 0m
                };
            }

            // Calculate comparison value if comparison period provided
            decimal? comparisonValue = null;

            if (comparisonDateFrom.HasValue && comparisonDateTo.HasValue)
            {
                var comparisonFrames = await _context.FrameMetrics
                    .Where(fm => fm.ComputedAt >= comparisonDateFrom && fm.ComputedAt <= comparisonDateTo)
                    .ToListAsync();

                if (comparisonFrames.Any())
                {
                    comparisonValue = metricName switch
                    {
                        "Peak Pressure" => (decimal)comparisonFrames.Average(fm => fm.PeakPressureIndex),
                        "Contact Area %" => (decimal)comparisonFrames.Average(fm => fm.ContactAreaPercent),
                        "Coefficient of Variation" => (decimal)comparisonFrames.Average(fm => fm.COV),
                        _ => 0m
                    };
                }
            }

            var reportMetric = new ReportMetric
            {
                ReportID = reportId,
                MetricName = metricName,
                MetricValue = metricValue,
                ComparisonValue = comparisonValue
            };

            _context.ReportMetrics.Add(reportMetric);
            await _context.SaveChangesAsync();
        }



        // Generate chart image URL using QuickChart.io
        private string GenerateChartImageUrl(
            List<string> labels,
            List<decimal> peakData,
            List<decimal> contactData,
            List<decimal> covData)
        {
            var chartConfig = new
            {
                type = "line",
                data = new
                {
                    labels = labels,
                    datasets = new object[]
                    {
                new
                {
                    label = "Peak Pressure",
                    data = peakData,
                    borderColor = "rgb(33, 118, 210)",
                    backgroundColor = "rgba(33, 118, 210, 0.1)",
                    fill = false
                },
                new
                {
                    label = "Contact Area %",
                    data = contactData,
                    borderColor = "rgb(56, 142, 60)",
                    backgroundColor = "rgba(56, 142, 60, 0.1)",
                    fill = false
                },
                new
                {
                    label = "COV",
                    data = covData,
                    borderColor = "rgb(0, 151, 167)",
                    backgroundColor = "rgba(0, 151, 167, 0.1)",
                    fill = false,
                    yAxisID = "y-axis-1"
                }
                    }
                },
                options = new
                {
                    // Chart.js v2 style scales for QuickChart (yAxes array)
                    scales = new
                    {
                        yAxes = new[]
                        {
                    new {
                        id = "y-axis-0",
                        type = "linear",
                        position = "left",
                        gridLines = new { drawOnChartArea = true } // included so anonymous types match
                    },
                    new {
                        id = "y-axis-1",
                        type = "linear",
                        position = "right",
                        gridLines = new { drawOnChartArea = false }
                    }
                }
                    },
                  
                    legend = new { position = "top" }
                }
            };

            // Serialize with relaxed escaping to avoid strange escaping issues
            var json = System.Text.Json.JsonSerializer.Serialize(chartConfig,
                new System.Text.Json.JsonSerializerOptions
                {
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                });


            var encodedChart = Uri.EscapeDataString(json);
            return $"https://quickchart.io/chart?c={encodedChart}&width=800&height=400";
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

        // Chart data properties
        public List<string> ChartLabels { get; set; } = new();
        public List<decimal> PeakPressureData { get; set; } = new();
        public List<decimal> ContactAreaData { get; set; } = new();
        public List<decimal> COVData { get; set; } = new();
        public string ChartImageUrl { get; set; } = string.Empty;

    }

    public class ReportMetricDisplay
    {
        public string MetricName { get; set; } = string.Empty;
        public decimal CurrentValue { get; set; }
        public decimal? ComparisonValue { get; set; }
    }

}