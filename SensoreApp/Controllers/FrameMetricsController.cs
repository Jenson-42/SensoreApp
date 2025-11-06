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
            // If no frameId provided, get the most recent frame
            if (frameId == null)
            {
                // TODO: Once SensorFrames is created by teammate, get latest frame
                // For now, just show empty view
                return View();
            }

            // Get metrics for the specific frame
            var metrics = await _context.FrameMetrics
                .Where(m => m.FrameID == frameId)
                .FirstOrDefaultAsync();

            if (metrics == null)
            {
                // No metrics calculated for this frame yet
                ViewBag.Message = "No metrics available for this frame.";
                return View();
            }

            return View(metrics);
        }

        // GET: FrameMetrics/Calculate/5
        // Calculates metrics for a specific frame
        public async Task<IActionResult> Calculate(int frameId)
        {
            // TODO: Get the actual frame data from SensorFrames table
            // TODO: Calculate Peak Pressure Index, Contact Area %, COV
            // For now, this is a placeholder

            // Check if metrics already exist for this frame
            var existingMetrics = await _context.FrameMetrics
                .FirstOrDefaultAsync(m => m.FrameID == frameId);

            if (existingMetrics != null)
            {
                // Metrics already calculated
                return RedirectToAction(nameof(Index), new { frameId = frameId });
            }

            // Create new metrics (placeholder values for now)
            var newMetrics = new FrameMetric
            {
                FrameID = frameId,
                PeakPressureIndex = 0, // TODO: Calculate from frame data
                ContactAreaPercent = 0, // TODO: Calculate from frame data
                COV = 0, // TODO: Calculate from frame data
                ComputedAt = DateTime.Now
            };

            _context.FrameMetrics.Add(newMetrics);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index), new { frameId = frameId });
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
