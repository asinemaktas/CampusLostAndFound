using CampusLostAndFound.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace CampusLostAndFound.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;

        public AdminController(
            UserManager<IdentityUser> userManager)
        {
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users
                .OrderBy(x => x.Email)
                .ToListAsync();

            var model = new List<AdminUserViewModel>();

            foreach (var user in users)
            {
                var roles =
                    await _userManager.GetRolesAsync(user);

                model.Add(new AdminUserViewModel
                {
                    Id = user.Id,

                    Email =
                        user.Email ??
                        user.UserName ??
                        "E-posta bulunamadı",

                    IsStudent =
                        roles.Contains("Student"),

                    IsSecurity =
                        roles.Contains("Security"),

                    IsAdmin =
                        roles.Contains("Admin")
                });
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GrantSecurity(
            string id)
        {
            var user =
                await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            if (await _userManager.IsInRoleAsync(
                user,
                "Admin"))
            {
                TempData["ErrorMessage"] =
                    "Admin hesabının rolü değiştirilemez.";

                return RedirectToAction(nameof(Index));
            }

            if (await _userManager.IsInRoleAsync(
                user,
                "Student"))
            {
                await _userManager.RemoveFromRoleAsync(
                    user,
                    "Student");
            }

            if (!await _userManager.IsInRoleAsync(
                user,
                "Security"))
            {
                await _userManager.AddToRoleAsync(
                    user,
                    "Security");
            }

            TempData["SuccessMessage"] =
                $"{user.Email} kullanıcısına güvenlik yetkisi verildi.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RevokeSecurity(
            string id)
        {
            var user =
                await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            if (await _userManager.IsInRoleAsync(
                user,
                "Admin"))
            {
                TempData["ErrorMessage"] =
                    "Admin hesabının rolü değiştirilemez.";

                return RedirectToAction(nameof(Index));
            }

            if (await _userManager.IsInRoleAsync(
                user,
                "Security"))
            {
                await _userManager.RemoveFromRoleAsync(
                    user,
                    "Security");
            }

            if (!await _userManager.IsInRoleAsync(
                user,
                "Student"))
            {
                await _userManager.AddToRoleAsync(
                    user,
                    "Student");
            }

            TempData["SuccessMessage"] =
                $"{user.Email} kullanıcısının güvenlik yetkisi kaldırıldı.";

            return RedirectToAction(nameof(Index));
        }
    }
}