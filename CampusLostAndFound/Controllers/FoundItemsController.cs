using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CampusLostAndFound.Data;
using CampusLostAndFound.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace CampusLostAndFound.Controllers
{
    [Authorize(Roles = "Security,Admin")]
  
    
        public class FoundItemsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public FoundItemsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: FoundItems
        [AllowAnonymous]
      
        public async Task<IActionResult> Index(
            string? searchTerm,
            string? category,
            string? building)

        {
            var activeItems = _context.FoundItems
                .Where(x => !x.IsDelivered);

            ViewBag.Categories = await activeItems
                .Select(x => x.Category)
                .Distinct()
                .OrderBy(x => x)
                .ToListAsync();

            ViewBag.Buildings = await activeItems
                .Select(x => x.Building)
                .Distinct()
                .OrderBy(x => x)
                .ToListAsync();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                activeItems = activeItems.Where(x =>
                    x.Name.Contains(searchTerm) ||
                    x.Color.Contains(searchTerm) ||
                    x.PublicDescription.Contains(searchTerm));
            }

            if (!string.IsNullOrWhiteSpace(category))
            {
                activeItems = activeItems.Where(x =>
                    x.Category == category);
            }

            if (!string.IsNullOrWhiteSpace(building))
            {
                activeItems = activeItems.Where(x =>
                    x.Building == building);
            }


            ViewBag.SearchTerm = searchTerm;
            ViewBag.SelectedCategory = category;
            ViewBag.SelectedBuilding = building; 

            var items = await activeItems
                .OrderByDescending(x => x.FoundDate)
                .ToListAsync();

            return View(items);
        }

        // GET: FoundItems/Details/5
        [AllowAnonymous]
        
     
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var foundItem = await _context.FoundItems
                .FirstOrDefaultAsync(m => m.Id == id);
            if (foundItem == null)
            {
                return NotFound();
            }

            string? studentId =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            bool hasExistingClaim = false;

            if (!string.IsNullOrWhiteSpace(studentId))
            {
                hasExistingClaim =
                    await _context.OwnershipClaims.AnyAsync(x =>
                        x.FoundItemId == foundItem.Id &&
                        x.StudentId == studentId);
            }

            bool hasApprovedClaim =
                await _context.OwnershipClaims.AnyAsync(x =>
                    x.FoundItemId == foundItem.Id &&
                    x.Status == ClaimStatus.Approved);

            ViewBag.CanClaim =
                User.IsInRole("Student") &&
                !foundItem.IsDelivered &&
                !hasExistingClaim &&
                !hasApprovedClaim;

            return View(foundItem);
        }

        // GET: FoundItems/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: FoundItems/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Category,Color,Building,FoundLocation,FoundDate,PublicDescription,SecretDetail")] FoundItem foundItem)
        {
            if (ModelState.IsValid)
            {
                _context.Add(foundItem);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(foundItem);
        }

        // GET: FoundItems/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var foundItem = await _context.FoundItems.FindAsync(id);
            if (foundItem == null)
            {
                return NotFound();
            }
            return View(foundItem);
        }

        // POST: FoundItems/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Category,Color,Building,FoundLocation,FoundDate,PublicDescription,SecretDetail,IsDelivered")] FoundItem foundItem)
        {
            if (id != foundItem.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(foundItem);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FoundItemExists(foundItem.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(foundItem);
        }

        // GET: FoundItems/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var foundItem = await _context.FoundItems
                .FirstOrDefaultAsync(m => m.Id == id);

            if (foundItem == null)
            {
                return NotFound();
            }

            ViewBag.PendingClaimCount =
                await _context.OwnershipClaims.CountAsync(x =>
                    x.FoundItemId == foundItem.Id &&
                    x.Status == ClaimStatus.Pending);

            return View(foundItem);
        }


        // POST: FoundItems/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var foundItem = await _context.FoundItems.FindAsync(id);
            if (foundItem != null)
            {
                _context.FoundItems.Remove(foundItem);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool FoundItemExists(int id)
        {
            return _context.FoundItems.Any(e => e.Id == id);
        }

        [Authorize(Roles = "Security,Admin")]
        public async Task<IActionResult> DeliveredItems()
        {
            var deliveredItems = await _context.FoundItems
                .Where(x => x.IsDelivered)
                .OrderByDescending(x => x.FoundDate)
                .ToListAsync();

            return View(deliveredItems);
        }
    }
}
