using Microsoft.AspNetCore.Mvc;
using SensoreApp.Data;
using SensoreApp.Models;
using SensoreApp.Services;
using System;

namespace SensoreApp.Controllers
{
    public class UploadController : Controller
    {
        private readonly AppDBContext _context;
        private readonly CSVParserService _csvParser;
        private readonly FrameMetricsService _metricsService;

        public UploadController(
            AppDBContext context,
            CSVParserService csvParser,
            FrameMetricsService metricsService)
        {
            _context = context;
            _csvParser = csvParser;
            _metricsService = metricsService;
        }

        // GET: Upload
        // Shows the upload page
        public IActionResult Index()
        {
            return View();
        }

        // POST: Upload/ProcessCSV
        // Handles CSV file upload and processing
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessCSV(IFormFile csvFile, int userId = 1)
        {
            // Validate file was uploaded
            if (csvFile == null || csvFile.Length == 0)
            {
                TempData["Error"] = "Please select a CSV file to upload.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                // Step 1: Validate CSV file
                using (var validationStream = csvFile.OpenReadStream())
                {
                    var validation = await _csvParser.ValidateCSVAsync(validationStream, csvFile.FileName);

                    if (!validation.IsValid)
                    {
                        TempData["Error"] = $"Invalid CSV file: {string.Join(", ", validation.Errors)}";
                        return RedirectToAction(nameof(Index));
                    }

                    // Show warnings if any
                    if (validation.Warnings.Any())
                    {
                        TempData["Warning"] = string.Join(", ", validation.Warnings);
                    }

                    // Store validation info for results page
                    TempData["TotalRows"] = validation.TotalRows;
                    TempData["EstimatedFrames"] = validation.EstimatedFrames;
                    TempData["FileSizeMB"] = validation.FileSizeMB.ToString("F2");
                }

                // Step 2: Parse CSV file
                CSVParseResult parseResult;
                using (var parseStream = csvFile.OpenReadStream())
                {
                    parseResult = await _csvParser.ParseCSVFileAsync(parseStream);

                    if (!parseResult.Success)
                    {
                        TempData["Error"] = $"Error parsing CSV: {parseResult.ErrorMessage}";
                        return RedirectToAction(nameof(Index));
                    }
                }

                // Step 3: Process each frame and calculate metrics
                int processedCount = 0;
                int errorCount = 0;
                var errors = new List<string>();

                foreach (var frame in parseResult.Frames)
                {
                    try
                    {
                        // Calculate metrics for this frame
                        var metrics = _metricsService.CalculateMetrics(frame.Data, frame.FrameIndex + 1);

                        // Save to database
                        _context.FrameMetrics.Add(metrics);
                        processedCount++;

                        // Batch save every 100 frames for performance
                        if (processedCount % 100 == 0)
                        {
                            await _context.SaveChangesAsync();
                        }
                    }
                    catch (Exception ex)
                    {
                        errorCount++;
                        errors.Add($"Frame {frame.FrameIndex + 1}: {ex.Message}");

                        // Stop if too many errors
                        if (errorCount > 10)
                        {
                            errors.Add("Too many errors. Stopping processing.");
                            break;
                        }
                    }
                }

                // Save any remaining frames
                await _context.SaveChangesAsync();

                // Prepare results
                TempData["Success"] = $"Successfully processed {processedCount} frames from {csvFile.FileName}";
                TempData["ProcessedFrames"] = processedCount;
                TempData["FileName"] = csvFile.FileName;

                if (errorCount > 0)
                {
                    TempData["ErrorCount"] = errorCount;
                    TempData["Errors"] = string.Join("; ", errors.Take(5)); // Show first 5 errors
                }

                return RedirectToAction(nameof(Results));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Unexpected error: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Upload/Results
        // Shows processing results
        public IActionResult Results()
        {
            // Check if we have results to display
            if (TempData["ProcessedFrames"] == null)
            {
                return RedirectToAction(nameof(Index));
            }

            ViewBag.FileName = TempData["FileName"];
            ViewBag.ProcessedFrames = TempData["ProcessedFrames"];
            ViewBag.TotalRows = TempData["TotalRows"];
            ViewBag.EstimatedFrames = TempData["EstimatedFrames"];
            ViewBag.FileSizeMB = TempData["FileSizeMB"];
            ViewBag.ErrorCount = TempData["ErrorCount"];
            ViewBag.Errors = TempData["Errors"];

            return View();
        }
    }
}
