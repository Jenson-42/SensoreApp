using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SensoreApp.Data;
using SensoreApp.Models;
using System;

namespace SensoreApp.Controllers
{
    public class PatientDashboardController : Controller
    {
        private readonly AppDBContext _context;

        public PatientDashboardController(AppDBContext context)
        {
            _context = context;
        }

        // GET: PatientDashboard
        // Main dashboard page
        public async Task<IActionResult> Index(int? userId)
        {
            // TODO: Get userId from logged-in user session
            // For now, using parameter or default to user 1
            int currentUserId = userId ?? 1;

            var viewModel = new PatientDashboardViewModel
            {
                PatientName = "John Doe", // TODO: Get from Users table when teammate creates it
                LastUploadTime = DateTime.Now.AddMinutes(-15), // TODO: Get from latest SensorFrame
                AlertThresholdPercent = 80 // Default threshold
            };

            // Get the most recent frame for this user
            // TODO: Uncomment when teammate creates SensorFrames table
            /*
            var latestFrame = await _context.SensorFrames
                .Where(f => f.UserID == currentUserId && !f.IsArchived)
                .OrderByDescending(f => f.Timestamp)
                .FirstOrDefaultAsync();

            if (latestFrame != null)
            {
                viewModel.CurrentFrameID = latestFrame.FrameID;
                viewModel.HeatmapData = latestFrame.FrameData;
                viewModel.LastUploadTime = latestFrame.Timestamp;

                // Get metrics for this frame
                var metrics = await _context.FrameMetrics
                    .Where(m => m.FrameID == latestFrame.FrameID)
                    .FirstOrDefaultAsync();

                if (metrics != null)
                {
                    viewModel.PeakPressureIndex = metrics.PeakPressureIndex;
                    viewModel.ContactAreaPercent = metrics.ContactAreaPercent;
                    viewModel.COV = metrics.COV;
                }
            }
            */

            // For now, use sample data for display
            viewModel.CurrentFrameID = 1;
            viewModel.PeakPressureIndex = 187.43;
            viewModel.ContactAreaPercent = 65.50;
            viewModel.COV = 0.2347;

            // Get recent alerts
            // TODO: Uncomment when teammate creates Alerts table
            /*
            viewModel.RecentAlerts = await _context.Alerts
                .Where(a => a.UserID == currentUserId)
                .OrderByDescending(a => a.StartTime)
                .Take(3)
                .Select(a => new AlertInfo
                {
                    AlertID = a.AlertID,
                    Status = a.Status,
                    Timestamp = a.StartTime
                })
                .ToListAsync();
            */

            // Sample alerts for now
            viewModel.RecentAlerts = new List<AlertInfo>
            {
                new AlertInfo
                {
                    AlertID = 1,
                    Status = "Normal pressure restored",
                    Timestamp = DateTime.Now.AddMinutes(-15)
                },
                new AlertInfo
                {
                    AlertID = 2,
                    Status = "High pressure detected",
                    Timestamp = DateTime.Now.AddMinutes(-47)
                }
            };

            // Get recent comments
            // TODO: Uncomment when teammate creates Comments table
            /*
            viewModel.Comments = await _context.Comments
                .Where(c => c.FrameID == viewModel.CurrentFrameID)
                .OrderByDescending(c => c.CreatedAt)
                .Select(c => new CommentInfo
                {
                    CommentID = c.CommentID,
                    AuthorName = c.Author.FirstName + " " + c.Author.LastName,
                    AuthorType = c.Author.UserType,
                    Text = c.Text,
                    CreatedAt = c.CreatedAt
                })
                .ToListAsync();
            */

            // Sample comments for now
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
            // TODO: Save threshold to user settings or ThresholdSettings table
            /*
            var settings = await _context.ThresholdSettings
                .FirstOrDefaultAsync(s => s.UserID == userId);

            if (settings == null)
            {
                settings = new ThresholdSettings
                {
                    UserID = userId,
                    ThresholdValue = thresholdValue
                };
                _context.ThresholdSettings.Add(settings);
            }
            else
            {
                settings.ThresholdValue = thresholdValue;
            }

            await _context.SaveChangesAsync();
            */

            TempData["Message"] = $"Alert threshold updated to {thresholdValue}%";
            return RedirectToAction(nameof(Index));
        }
    }
}
