using CampusLostAndFound.Data;
using CampusLostAndFound.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace CampusLostAndFound.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(
            ILogger<HomeController> logger,
            ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                if (User.IsInRole("Admin"))
                {
                    return RedirectToAction(
                        "Index",
                        "Admin");
                }

                if (User.IsInRole("Security"))
                {
                    return RedirectToAction(
                        "Security",
                        "Dashboard");
                }

                if (User.IsInRole("Student"))
                {
                    return RedirectToAction(
                        "Student",
                        "Dashboard");
                }
            }

            var recentItems = await _context.FoundItems
                .Where(x =>
                    !x.IsDelivered &&
                    !_context.OwnershipClaims.Any(c =>
                        c.FoundItemId == x.Id &&
                        c.Status == ClaimStatus.Approved))
                .OrderByDescending(x => x.FoundDate)
                .Take(6)
                .ToListAsync();

            return View(recentItems);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(
            Duration = 0,
            Location = ResponseCacheLocation.None,
            NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId =
                    Activity.Current?.Id ??
                    HttpContext.TraceIdentifier
            });
        }
    }
}