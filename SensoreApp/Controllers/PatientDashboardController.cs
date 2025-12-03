using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SensoreApp.Data;
using SensoreApp.Models;
using System;

namespace SensoreApp.Controllers
{
    public class PatientDashboardController : Controller
    {
        //Dependency injection of the database context
        private readonly AppDBContext _context;

        public PatientDashboardController(AppDBContext context)
        {
            _context = context;
        }

        // GET: PatientDashboard
        // Main dashboard page
        public async Task<IActionResult> Index(int? userId)
        {
            // TODO: Replace with authenticated user ID once login system is implemented
            int currentUserId = userId ?? 1;
            // Get real patient name from Users table
            var user = await _context.Users.FindAsync(currentUserId);
            string patientName = user != null ? $"{user.FirstName} {user.LastName}" : "Unknown User";
            // Get user's saved threshold or use default
            var thresholdSetting = await _context.ThresholdSettings
            .Where(t => t.UserID == currentUserId)
            .FirstOrDefaultAsync();
            var viewModel = new PatientDashboardViewModel
            {
                PatientName = patientName,
                LastUploadTime = DateTime.Now.AddMinutes(-15),
                AlertThresholdPercent = thresholdSetting != null
                ? (int)thresholdSetting.ThresholdValue
                : 80 // Default threshold if not set
            };

            // Get the most recent frame metrics from database
            var latestMetrics = await _context.FrameMetrics
                .OrderByDescending(m => m.ComputedAt)
                .FirstOrDefaultAsync();

            if (latestMetrics != null)
            {
                // Use REAL data from database
                viewModel.CurrentFrameID = latestMetrics.FrameID;
                viewModel.PeakPressureIndex = latestMetrics.PeakPressureIndex;
                viewModel.ContactAreaPercent = latestMetrics.ContactAreaPercent;
                viewModel.COV = latestMetrics.COV;
                viewModel.LastUploadTime = latestMetrics.ComputedAt;
            }
            else
            {
                // No data yet - use placeholder values
                viewModel.CurrentFrameID = 0;
                viewModel.PeakPressureIndex = 0;
                viewModel.ContactAreaPercent = 0;
                viewModel.COV = 0;
            }

            // Get REAL recent alerts from database
            viewModel.RecentAlerts = await _context.Alerts
                .Where(a => a.UserId == currentUserId)
                .OrderByDescending(a => a.CreatedAt)
                .Take(5) // Last 5 alerts
                .Select(a => new AlertInfo
                {
                    AlertID = a.AlertId,
                    Status = a.Status == "New" ? "High Pressure Detected" : "Normal pressure restored",
                    Timestamp = a.CreatedAt,
                    Reason = a.Reason,
                    TriggerValue = a.TriggerValue
                })
                .ToListAsync();

            // Sample comments (TODO: replace with real Comments table data when teammate creates it)
            viewModel.Comments = new List<CommentInfo>
            {
                new CommentInfo
                {
                    CommentID = 1,
                    AuthorName = "Patient",
                    AuthorType = "Patient",
                    Text = "Feeling some pressure on the left side",
                    CreatedAt = DateTime.Now.AddHours(-2)
                },
                new CommentInfo
                {
                    CommentID = 2,
                    AuthorName = "Dr. Smith",
                    AuthorType = "Clinician",
                    Text = "Monitor this area closely. Try shifting position every 30 minutes.",
                    CreatedAt = DateTime.Now.AddHours(-1)
                }
            };

            return View(viewModel);
        }

        // POST: PatientDashboard/AddComment
        // Adds a new comment from patient or clinician
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddComment(int frameId, string commentText, int userId)
        {
            if (string.IsNullOrWhiteSpace(commentText))
            {
                return BadRequest("Comment cannot be empty");
            }

            // TODO: Save to Comments table when teammate creates it
            /*
            var comment = new Comment
            {
                FrameID = frameId,
                AuthorID = userId,
                Text = commentText,
                CreatedAt = DateTime.Now
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();
            */

            // For now, just redirect back to dashboard
            TempData["Message"] = "Comment added successfully!";
            return RedirectToAction(nameof(Index));
        }

        // POST: PatientDashboard/UpdateThreshold
        // Updates alert threshold setting
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateThreshold(int userId, int thresholdValue)
        {
            try
            {
                // Check if user already has threshold settings
                var settings = await _context.ThresholdSettings
                    .FirstOrDefaultAsync(s => s.UserID == userId);

                if (settings == null)
                {
                    // Create new threshold setting
                    settings = new ThresholdSettings
                    {
                        UserID = userId,
                        ThresholdValue = thresholdValue,
                        CreatedAt = DateTime.Now
                    };
                    _context.ThresholdSettings.Add(settings);
                }
                else
                {
                    // Update existing threshold setting
                    settings.ThresholdValue = thresholdValue;
                }

                await _context.SaveChangesAsync();
                TempData["Message"] = $"Alert threshold updated to {thresholdValue}%";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Failed to update threshold: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
