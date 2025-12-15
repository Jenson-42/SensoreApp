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
using Microsoft.AspNetCore.Mvc.Rendering; // For SelectListItem

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
        public async Task<IActionResult> Index(int? userId = null, DateTime? dateFrom = null, DateTime? dateTo = null)
        {
            // Get current user ID (in production, this would come from authentication)
            int currentUserId = userId ?? 1;

            // Load the user from database to check their type
            var currentUser = await _context.Users.FindAsync(currentUserId);

            if (currentUser == null)
            {
                return NotFound("User not found");
            }

            // Auto-detect user type based on database discriminator
            string userType = currentUser is Clinician ? "clinician" : "patient";

            ViewBag.UserType = userType;
            ViewBag.CurrentUserId = currentUserId;
            ViewBag.CurrentUserName = $"{currentUser.FirstName} {currentUser.LastName}";

            if (currentUser is Clinician clinicianUser)
            {
                ViewBag.PatientList = await _context.Patients
                    .Where(p => p.IsActive &&
                                _context.PatientClinicians.Any(pc =>
                                    pc.ClinicianID == clinicianUser.UserId &&
                                    pc.PatientID == p.UserId))
                    .OrderBy(p => p.LastName)
                    .Select(p => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                    {
                        Value = p.UserId.ToString(),
                        Text = $"{p.FirstName} {p.LastName}"
                    })
                    .ToListAsync();
            }
            else
            {
                // If PATIENT, no dropdown needed - they only see their own data
                ViewBag.PatientList = null;
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

            // Smart sampling: Limit to maximum 6 frames, evenly distributed
            int maxFrames = 6;
            var sampledFrames = new List<dynamic>();

            if (allFrames.Count <= maxFrames)
            {
                // Show all frames if 6 or fewer
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
            bool isAdmin = currentUser.Role == UserRole.Admin;

            if (isAdmin)
            {
                ViewBag.PatientList = await _context.Patients
                    .Where(p => p.IsActive)
                    .OrderBy(p => p.LastName)
                    .Select(p => new SelectListItem
                    {
                        Value = p.UserId.ToString(),
                        Text = $"{p.FirstName} {p.LastName}"
                    })
                    .ToListAsync();
            }
            ViewBag.IsAdmin = isAdmin;

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
                .Include(r => r.RequestedByUser)  // Load the clinician/user who requested
                .Include(r => r.User)              // Load the patient
                .FirstOrDefaultAsync(r => r.ReportID == id);

            if (report == null)
            {
                return NotFound();
            }
            // Now access directly without additional queries
            var patientName = report.User != null
                ? $"{report.User.FirstName} {report.User.LastName}"
                : "Unknown User";

            var clinicianName = report.RequestedByUser is Clinician clinician
                ? $"Dr. {clinician.FirstName} {clinician.LastName}"
                : report.RequestedByUser != null
                    ? $"{report.RequestedByUser.FirstName} {report.RequestedByUser.LastName}"
                    : "N/A";
            var clinicianEmail = report.RequestedByUser is Clinician c
                ? c.WorkEmail
                : report.RequestedByUser?.Email ?? "N/A";
            
            // Find the assigned clinician for this patient
            var assignedClinician = await _context.PatientClinicians
                .Where(pc => pc.PatientID == report.UserID)
                .Select(pc => pc.Clinician)
                .FirstOrDefaultAsync();

            string assignedClinicianName = assignedClinician != null
                ? $"{assignedClinician.FirstName} {assignedClinician.LastName}"
                : "No clinician assigned";

            string assignedClinicianEmail = assignedClinician?.Email ?? "N/A";



            // Build preview view model
            var viewModel = new ReportPreviewViewModel
            {
                ReportID = report.ReportID,
                PatientName = patientName,
                DateFrom = report.DateFrom,
                DateTo = report.DateTo,
                ComparisonDateFrom = report.ComparisonDateFrom,
                ComparisonDateTo = report.ComparisonDateTo,
                GeneratedAt = report.GeneratedAt,
                ReportType = report.ReportType,
                ClinicianName = clinicianName,   // who requested
                ClinicianEmail = clinicianEmail,  // who requested
                AssignedClinicianName = assignedClinicianName,   // who is assigned
                AssignedClinicianEmail = assignedClinicianEmail, // who is assigned
                ClinicianNote = "No clinical notes available yet.", // Placeholder
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
            if (report.ReportFrame != null && report.ReportFrame.Any())
            {
                var frameIds = report.ReportFrame.Select(rf => rf.FrameID).ToList();

                var frameMetrics = await _context.FrameMetrics
                    .Where(fm => frameIds.Contains(fm.FrameID))
                    .OrderBy(fm => fm.ComputedAt)
                    .ToListAsync();

                viewModel.SnapshotHeatmaps = frameMetrics
                    .Select(fm => (fm.FrameID, GenerateHeatmapImageUrlForFrame(fm.FrameID)))
                    .ToList();
            }



            // Calculate REAL alert summary from Alerts table
            var alertsInPeriod = await _context.Alerts
                .Where(a => a.UserId == report.UserID &&
                            a.CreatedAt >= report.DateFrom &&
                            a.CreatedAt <= report.DateTo)
                .ToListAsync();

            // Count alerts by status/severity
            // Assuming: Status "New" or "Active" = Warning/Critical based on TriggerValue
            // Status "Resolved" or contains "Normal" = Normal
            viewModel.NormalReadingsCount = alertsInPeriod.Count(a =>
                a.Status.Contains("Normal", StringComparison.OrdinalIgnoreCase) ||
                a.Status == "Resolved");

            viewModel.WarningAlertsCount = alertsInPeriod.Count(a =>
                (a.Status == "New" || a.Status == "Active") &&
                a.TriggerValue < 600); // Warning threshold

            viewModel.CriticalAlertsCount = alertsInPeriod.Count(a =>
                (a.Status == "New" || a.Status == "Active") &&
                a.TriggerValue >= 600); // Critical threshold

            // Get most recent alert
            var mostRecentAlert = alertsInPeriod
                .OrderByDescending(a => a.CreatedAt)
                .FirstOrDefault();

            if (mostRecentAlert != null)
            {
                viewModel.MostRecentAlertTime = mostRecentAlert.CreatedAt;
                viewModel.MostRecentAlertReason = mostRecentAlert.Reason ?? "High pressure detected";
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
                .Include(r => r.RequestedByUser)  // Load the clinician/user who requested
                .Include(r => r.User)              // Load the patient
                .FirstOrDefaultAsync(r => r.ReportID == id);

            if (report == null)
            {
                return NotFound();
            }
            var patientName = report.User != null
                ? $"{report.User.FirstName} {report.User.LastName}"
                : "Unknown User";

            var clinicianName = report.RequestedByUser is Clinician clinician
                ? $"Dr. {clinician.FirstName} {clinician.LastName}"
                : report.RequestedByUser != null
                    ? $"{report.RequestedByUser.FirstName} {report.RequestedByUser.LastName}"
                    : "N/A";

            var clinicianEmail = report.RequestedByUser is Clinician c
                ? c.WorkEmail
                : report.RequestedByUser?.Email ?? "N/A";

            // Find the assigned clinician for this patient
            var assignedClinician = await _context.PatientClinicians
                .Where(pc => pc.PatientID == report.UserID)
                .Select(pc => pc.Clinician)
                .FirstOrDefaultAsync();

            string assignedClinicianName = assignedClinician != null
                ? $"{assignedClinician.FirstName} {assignedClinician.LastName}"
                : "No clinician assigned";

            string assignedClinicianEmail = assignedClinician?.Email ?? "N/A";


            // Build preview view model (same as Preview action)
            var viewModel = new ReportPreviewViewModel
            {
                ReportID = report.ReportID,
                PatientName = patientName,
                DateFrom = report.DateFrom,
                DateTo = report.DateTo,
                ComparisonDateFrom = report.ComparisonDateFrom,
                ComparisonDateTo = report.ComparisonDateTo,
                GeneratedAt = report.GeneratedAt,
                ReportType = report.ReportType,
                ClinicianName = clinicianName,
                ClinicianEmail = clinicianEmail,
                ClinicianNote = "No clinical notes available yet.", // Placeholder
                AssignedClinicianName = assignedClinicianName,   // who is assigned
                AssignedClinicianEmail = assignedClinicianEmail, // who is assigned
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
            if (report.ReportFrame != null && report.ReportFrame.Any())
            {
                var frameIds = report.ReportFrame.Select(rf => rf.FrameID).ToList();

                var frameMetrics = await _context.FrameMetrics
                    .Where(fm => frameIds.Contains(fm.FrameID))
                    .OrderBy(fm => fm.ComputedAt)
                    .ToListAsync();

                viewModel.SnapshotHeatmaps = frameMetrics
                    .Select(fm => (fm.FrameID, GenerateHeatmapImageUrlForFrame(fm.FrameID)))
                    .ToList();
            }



            // Calculate REAL alert summary from Alerts table
            var alertsInPeriod = await _context.Alerts
                .Where(a => a.UserId == report.UserID &&
                            a.CreatedAt >= report.DateFrom &&
                            a.CreatedAt <= report.DateTo)
                .ToListAsync();

            viewModel.NormalReadingsCount = alertsInPeriod.Count(a =>
                a.Status.Contains("Normal", StringComparison.OrdinalIgnoreCase) ||
                a.Status == "Resolved");

            viewModel.WarningAlertsCount = alertsInPeriod.Count(a =>
                (a.Status == "New" || a.Status == "Active") &&
                a.TriggerValue < 600);

            viewModel.CriticalAlertsCount = alertsInPeriod.Count(a =>
                (a.Status == "New" || a.Status == "Active") &&
                a.TriggerValue >= 600);

            var mostRecentAlert = alertsInPeriod
                .OrderByDescending(a => a.CreatedAt)
                .FirstOrDefault();

            if (mostRecentAlert != null)
            {
                viewModel.MostRecentAlertTime = mostRecentAlert.CreatedAt;
                viewModel.MostRecentAlertReason = mostRecentAlert.Reason ?? "High pressure detected";
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
        private string GenerateHeatmapImageUrlForFrame(int frameId)
        {
            var random = new Random(frameId); // seed for consistency per frame
            var dataPoints = new List<object>();

            for (int row = 0; row < 32; row++)
            {
                for (int col = 0; col < 32; col++)
                {
                    int pressure = CalculateSamplePressureInt(row, col, random);
                    if (pressure > 30)
                    {
                        dataPoints.Add(new { x = col, y = 31 - row, pressure });
                    }
                }
            }

            var chartConfig = new
            {
                type = "bubble",
                data = new
                {
                    datasets = new[]
                    {
                new {
                    label = "High",
                    data = dataPoints.Where(p => ((dynamic)p).pressure >= 400)
                                     .Select(p => new { x = ((dynamic)p).x, y = ((dynamic)p).y, r = 4 }).ToList(),
                    backgroundColor = "rgba(211, 47, 47, 0.9)"
                },
                new {
                    label = "Medium",
                    data = dataPoints.Where(p => ((dynamic)p).pressure >= 200 && ((dynamic)p).pressure < 400)
                                     .Select(p => new { x = ((dynamic)p).x, y = ((dynamic)p).y, r = 3 }).ToList(),
                    backgroundColor = "rgba(255, 167, 38, 0.9)"
                },
                new {
                    label = "Low",
                    data = dataPoints.Where(p => ((dynamic)p).pressure >= 30 && ((dynamic)p).pressure < 200)
                                     .Select(p => new { x = ((dynamic)p).x, y = ((dynamic)p).y, r = 2 }).ToList(),
                    backgroundColor = "rgba(46, 125, 50, 0.9)"
                }
            }
                },
                options = new
                {
                    responsive = false,
                    layout = new { padding = 0 },
                    plugins = new
                    {
                        legend = new { display = false },
                        title = new { display = false },
                        tooltip = new { enabled = false }
                    },
                    // Chart.js v3+ scales format for QuickChart
                    scales = new
                    {
                        x = new
                        {
                            display = false,
                            min = -1,
                            max = 32,
                            grid = new { display = false },
                            ticks = new { display = false }
                        },
                        y = new
                        {
                            display = false,
                            min = -1,
                            max = 32,
                            grid = new { display = false },
                            ticks = new { display = false }
                        }
                    },
                    elements = new
                    {
                        point = new { borderWidth = 0 }
                    }
                }
            };

            var json = System.Text.Json.JsonSerializer.Serialize(
                chartConfig,
                new System.Text.Json.JsonSerializerOptions
                {
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                });

            // Small, clean thumbnail, no background clutter
            var url = $"https://quickchart.io/chart?c={Uri.EscapeDataString(json)}&width=300&height=300&backgroundColor=white&format=png";


            // Cache-bust per frame to avoid stale images
            return $"{url}&v={frameId}";
        }



        private int CalculateSamplePressureInt(int row, int col, Random random)
        {
            // High pressure region (buttocks)
            if (row >= 16 && row <= 24 && col >= 12 && col <= 20)
            {
                double dist = Math.Sqrt(Math.Pow(row - 20, 2) + Math.Pow(col - 16, 2));
                return (int)Math.Max(0, 500 - dist * 40 + random.Next(0, 50));
            }
            // Medium pressure (thighs)
            else if (row >= 10 && row <= 28 && col >= 8 && col <= 24)
            {
                return 100 + random.Next(0, 150);
            }
            // Low/no pressure
            else
            {
                return random.Next(0, 30);
            }
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
        // NEW: Clinician Information
        public string ClinicianName { get; set; } = "N/A";
        public string ClinicianEmail { get; set; } = "N/A";
        // NEW: Placeholder for clinical notes
        public string ClinicianNote { get; set; } = "No clinical notes available yet.";

        public string AssignedClinicianName { get; set; } = "N/A";
        public string AssignedClinicianEmail { get; set; } = "N/A";


        // Chart data properties
        public List<string> ChartLabels { get; set; } = new();
        public List<decimal> PeakPressureData { get; set; } = new();
        public List<decimal> ContactAreaData { get; set; } = new();
        public List<decimal> COVData { get; set; } = new();
        public string ChartImageUrl { get; set; } = string.Empty;
        // NEW: Add heatmap URL
        public List<(int FrameID, string HeatmapUrl)> SnapshotHeatmaps { get; set; } = new();


        // NEW: Alert Summary Properties
        public int NormalReadingsCount { get; set; }
        public int WarningAlertsCount { get; set; }
        public int CriticalAlertsCount { get; set; }
        public DateTime? MostRecentAlertTime { get; set; }
        public string? MostRecentAlertReason { get; set; }
       

    }

    public class ReportMetricDisplay
    {
        public string MetricName { get; set; } = string.Empty;
        public decimal CurrentValue { get; set; }
        public decimal? ComparisonValue { get; set; }
    }

}