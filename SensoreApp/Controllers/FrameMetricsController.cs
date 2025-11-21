using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SensoreApp.Data;
using SensoreApp.Models;

namespace SensoreApp.Controllers
{
    public class FrameMetricsController : Controller
    {
        private readonly AppDBContext _context;

        // Constructor - receives database context
        public FrameMetricsController(AppDBContext context)
        {
            _context = context;
        }

        // GET: FrameMetrics
        // Shows the main Frame Metrics page
        public async Task<IActionResult> Index(int? frameId)
        {
            FrameMetric? metrics = null;

            // If frameId provided, get that specific frame
            if (frameId.HasValue)
            {
                metrics = await _context.FrameMetrics
                    .Where(m => m.FrameID == frameId.Value)
                    .FirstOrDefaultAsync();
            }
            else
            {
                // No frameId - get the most recent frame
                metrics = await _context.FrameMetrics
                    .OrderByDescending(m => m.ComputedAt)
                    .FirstOrDefaultAsync();
            }

            // If still no metrics found, show empty view with message
            if (metrics == null)
            {
                ViewBag.Message = "No frame data available. Please upload a CSV file first.";
                return View();
            }

            return View(metrics);
        }

        // GET: FrameMetrics/Calculate/5
        // Calculates metrics for a specific frame (not used with CSV upload, but kept for compatibility)
        public async Task<IActionResult> Calculate(int frameId)
        {
            // Check if metrics already exist for this frame
            var existingMetrics = await _context.FrameMetrics
                .FirstOrDefaultAsync(m => m.FrameID == frameId);

            if (existingMetrics != null)
            {
                // Metrics already calculated
                return RedirectToAction(nameof(Index), new { frameId = frameId });
            }

            // Note: In production, this would get frame data from SensorFrames table
            // and calculate metrics. For now, metrics are calculated during CSV upload.

            TempData["Message"] = "Metrics are calculated automatically during CSV upload.";
            return RedirectToAction(nameof(Index));
        }

        // GET: FrameMetrics/Details/5
        // Shows detailed metrics for a specific metric record
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var frameMetric = await _context.FrameMetrics
                .FirstOrDefaultAsync(m => m.FrameMetricID == id);

            if (frameMetric == null)
            {
                return NotFound();
            }

            return View(frameMetric);
        }

        // POST: FrameMetrics/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var frameMetric = await _context.FrameMetrics.FindAsync(id);
            if (frameMetric != null)
            {
                _context.FrameMetrics.Remove(frameMetric);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}