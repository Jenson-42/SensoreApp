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
            // TODO: Get userId from logged-in user session
            // For now, using parameter or default to user 1
            int currentUserId = userId ?? 1;

            var viewModel = new PatientDashboardViewModel
            {
                PatientName = "John Doe", // TODO: Get from Users table when teammate creates it
                LastUploadTime = DateTime.Now.AddMinutes(-15),
                AlertThresholdPercent = 80 // Default threshold
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

            // Sample alerts (TODO: replace with real Alerts table data when teammate creates it)
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
