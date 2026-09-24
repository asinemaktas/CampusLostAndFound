using CampusLostAndFound.Data;
using CampusLostAndFound.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CampusLostAndFound.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        [Authorize(Roles = "Student")]
        public async Task<IActionResult> Student()
        {
            var studentId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            ViewBag.MyClaimCount =
                await _context.OwnershipClaims
                    .CountAsync(x => x.StudentId == studentId);

            ViewBag.MyPendingClaimCount =
                await _context.OwnershipClaims
                    .CountAsync(x =>
                        x.StudentId == studentId &&
                        x.Status == ClaimStatus.Pending);

            return View();
        }

        [Authorize(Roles = "Student")]
        public IActionResult StudentGuide()
        {
            return View();
        }

        [Authorize(Roles = "Security,Admin")]
        public async Task<IActionResult> Security()
        {
            ViewBag.ActiveItemCount = await _context.FoundItems
                .CountAsync(x => !x.IsDelivered);

            ViewBag.DeliveredItemCount = await _context.FoundItems
                .CountAsync(x => x.IsDelivered);

            ViewBag.PendingClaimCount = await _context.OwnershipClaims
                .CountAsync(x =>
                    x.Status == ClaimStatus.Pending &&
                    x.FoundItem != null &&
                    !x.FoundItem.IsDelivered);

            ViewBag.PendingReportCount = await _context.FoundItemReports
                .CountAsync(x =>
                    x.Status == FoundItemReportStatus.Pending);

            var recentReports = await _context.FoundItemReports
                .Where(x =>
                    x.Status == FoundItemReportStatus.Pending ||
                    x.Status == FoundItemReportStatus.Received)
                .OrderByDescending(x => x.ReportedAt)
                .Take(3)
                .ToListAsync();

            return View(recentReports);
        }

        [Authorize(Roles = "Security,Admin")]
        public IActionResult SecurityGuide()
        {
            return View();
        }
    }
}
