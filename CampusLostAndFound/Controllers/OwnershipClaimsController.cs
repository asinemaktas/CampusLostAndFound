using CampusLostAndFound.Data;
using CampusLostAndFound.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CampusLostAndFound.Controllers
{
    [Authorize]
    public class OwnershipClaimsController : Controller
    {
        private readonly ApplicationDbContext _context;

        private readonly UserManager<IdentityUser>
            _userManager;

        public OwnershipClaimsController(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [Authorize(Roles = "Student")]
        public async Task<IActionResult> Create(
            int foundItemId)
        {
            var foundItem = await _context.FoundItems
                .FirstOrDefaultAsync(x =>
                    x.Id == foundItemId &&
                    !x.IsDelivered);

            if (foundItem == null)
            {
                return NotFound();
            }

            ViewBag.ItemName = foundItem.Name;

            return View(new OwnershipClaim
            {
                FoundItemId = foundItemId
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> Create(
            [Bind("FoundItemId,ProofDescription")]
            OwnershipClaim claim)
        {
            string? studentId =
                _userManager.GetUserId(User);

            if (studentId == null)
            {
                return Challenge();
            }

            claim.StudentId = studentId;
            claim.CreatedAt = DateTime.Now;
            claim.Status = ClaimStatus.Pending;

            ModelState.Remove(nameof(claim.StudentId));
            ModelState.Remove(nameof(claim.FoundItem));

            var foundItem = await _context.FoundItems
                .FirstOrDefaultAsync(x =>
                    x.Id == claim.FoundItemId &&
                    !x.IsDelivered);

            if (foundItem == null)
            {
                return NotFound();
            }

            bool existingClaim =
                await _context.OwnershipClaims.AnyAsync(x =>
                    x.FoundItemId == claim.FoundItemId &&
                    x.StudentId == studentId &&
                    (
                        x.Status == ClaimStatus.Pending ||
                        x.Status == ClaimStatus.Approved
                    ));

            if (existingClaim)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Bu eşya için zaten devam eden bir talebiniz var.");
            }

            if (ModelState.IsValid)
            {
                _context.OwnershipClaims.Add(claim);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(MyClaims));
            }

            ViewBag.ItemName = foundItem.Name;

            return View(claim);
        }

        [Authorize(Roles = "Student")]
        public async Task<IActionResult> MyClaims()
        {
            string? studentId =
                _userManager.GetUserId(User);

            var claims = await _context.OwnershipClaims
                .Include(x => x.FoundItem)
                .Where(x => x.StudentId == studentId)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();

            return View(claims);
        }

        [Authorize(Roles = "Security,Admin")]
        public async Task<IActionResult> Manage(
            int? openClaimId = null)
        {
            var claims = await _context.OwnershipClaims
                .Include(x => x.FoundItem)
                .Where(x =>
                    x.FoundItem != null &&
                    !x.FoundItem.IsDelivered)
                .OrderBy(x => x.Status)
                .ThenByDescending(x => x.CreatedAt)
                .ToListAsync();

            var studentIds = claims
                .Select(x => x.StudentId)
                .Distinct()
                .ToList();

            var studentEmails = await _userManager.Users
                .Where(x => studentIds.Contains(x.Id))
                .ToDictionaryAsync(
                    x => x.Id,
                    x => x.Email ?? x.UserName ?? "Bilinmiyor");

            ViewBag.StudentEmails = studentEmails;
            ViewBag.OpenClaimId = openClaimId;

            return View(claims);
        }

        [Authorize(Roles = "Security,Admin")]
        public async Task<IActionResult> Review(int id)
        {
            var claim = await _context.OwnershipClaims
                .Include(x => x.FoundItem)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (claim == null)
            {
                return NotFound();
            }

            var student =
                await _userManager.FindByIdAsync(
                    claim.StudentId);

            ViewBag.StudentEmail = student?.Email;

            return View(claim);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Security,Admin")]
        public async Task<IActionResult> Approve(
            int id,
            string? reviewNote)
        {
            var claim = await _context.OwnershipClaims
                .Include(x => x.FoundItem)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (claim == null ||
                claim.FoundItem == null)
            {
                return NotFound();
            }

            if (claim.Status != ClaimStatus.Pending ||
                claim.FoundItem.IsDelivered)
            {
                return BadRequest();
            }

            bool anotherApprovedClaim =
                await _context.OwnershipClaims.AnyAsync(x =>
                    x.FoundItemId == claim.FoundItemId &&
                    x.Id != claim.Id &&
                    x.Status == ClaimStatus.Approved);

            if (anotherApprovedClaim)
            {
                TempData["ErrorMessage"] =
                    "Bu eşya için başka bir talep zaten onaylanmış. " +
                    "Önce mevcut onayı geri almalısınız.";

                return RedirectToAction(
                    nameof(Manage),
                    new { openClaimId = id });
            }

            reviewNote = reviewNote?.Trim();

            if (reviewNote?.Length > 500)
            {
                TempData["ErrorMessage"] =
                    "Görevli notu en fazla 500 karakter olabilir.";

                return RedirectToAction(
                    nameof(Manage),
                    new { openClaimId = id });
            }

            claim.Status = ClaimStatus.Approved;
            claim.ReviewNote = reviewNote;
            claim.ReviewedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Sahiplik talebi onaylandı.";

            return RedirectToAction(
                nameof(Manage),
                new { openClaimId = id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Security,Admin")]
        public async Task<IActionResult> Reject(
            int id,
            string? reviewNote)
        {
            var claim =
                await _context.OwnershipClaims.FindAsync(id);

            if (claim == null)
            {
                return NotFound();
            }

            if (claim.Status != ClaimStatus.Pending)
            {
                return BadRequest();
            }

            reviewNote = reviewNote?.Trim();

            if (string.IsNullOrWhiteSpace(reviewNote))
            {
                TempData["ErrorMessage"] =
                    "Talebi reddederken bir gerekçe yazmalısınız.";

                return RedirectToAction(
                    nameof(Manage),
                    new { openClaimId = id });
            }

            if (reviewNote.Length > 500)
            {
                TempData["ErrorMessage"] =
                    "Görevli notu en fazla 500 karakter olabilir.";

                return RedirectToAction(
                    nameof(Manage),
                    new { openClaimId = id });
            }

            claim.Status = ClaimStatus.Rejected;
            claim.ReviewNote = reviewNote;
            claim.ReviewedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Sahiplik talebi reddedildi.";

            return RedirectToAction(
                nameof(Manage),
                new { openClaimId = id });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Security,Admin")]
        public async Task<IActionResult> RevokeRejection(
    int id)
        {
            var claim = await _context.OwnershipClaims
                .Include(x => x.FoundItem)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (claim == null ||
                claim.FoundItem == null)
            {
                return NotFound();
            }

            if (claim.Status != ClaimStatus.Rejected ||
                claim.FoundItem.IsDelivered)
            {
                return BadRequest();
            }

            claim.Status = ClaimStatus.Pending;
            claim.ReviewNote = null;
            claim.ReviewedAt = null;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Talebin reddi geri alındı. " +
                "Talep yeniden beklemeye geçirildi.";

            return RedirectToAction(
                nameof(Manage),
                new { openClaimId = id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Security,Admin")]
        public async Task<IActionResult> RevokeApproval(
            int id)
        {
            var claim = await _context.OwnershipClaims
                .Include(x => x.FoundItem)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (claim == null ||
                claim.FoundItem == null)
            {
                return NotFound();
            }

            if (claim.Status != ClaimStatus.Approved ||
                claim.FoundItem.IsDelivered)
            {
                return BadRequest();
            }

            claim.Status = ClaimStatus.Pending;
            claim.ReviewNote = null;
            claim.ReviewedAt = null;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Talep onayı geri alındı. " +
                "Talep yeniden beklemeye geçirildi.";

            return RedirectToAction(
                nameof(Manage),
                new { openClaimId = id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Security,Admin")]
        public async Task<IActionResult> MarkDelivered(
            int id)
        {
            var claim = await _context.OwnershipClaims
                .Include(x => x.FoundItem)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (claim == null ||
                claim.FoundItem == null)
            {
                return NotFound();
            }

            if (claim.Status != ClaimStatus.Approved ||
                claim.FoundItem.IsDelivered)
            {
                return BadRequest();
            }

            claim.FoundItem.IsDelivered = true;

            var otherOpenClaims =
                await _context.OwnershipClaims
                    .Where(x =>
                        x.FoundItemId == claim.FoundItemId &&
                        x.Id != claim.Id &&
                        (
                            x.Status == ClaimStatus.Pending ||
                            x.Status == ClaimStatus.Approved
                        ))
                    .ToListAsync();

            foreach (var otherClaim in otherOpenClaims)
            {
                otherClaim.Status = ClaimStatus.Closed;
                otherClaim.ReviewedAt = DateTime.Now;
                otherClaim.ReviewNote =
                    "Eşya doğrulanan sahibine teslim edildiği " +
                    "için bu talep kapatılmıştır.";
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Eşya sahibine teslim edildi ve kayıt kapatıldı.";

            return RedirectToAction(nameof(Manage));
        }
    }
}
