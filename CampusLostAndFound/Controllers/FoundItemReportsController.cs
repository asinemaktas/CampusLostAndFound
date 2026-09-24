using CampusLostAndFound.Data;
using CampusLostAndFound.Models;
using CampusLostAndFound.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CampusLostAndFound.Controllers
{
    public class FoundItemReportsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public FoundItemReportsController(
            ApplicationDbContext context,
            IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // Ziyaretçiye bildirim formunu gösterir.
        [AllowAnonymous]
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // Ziyaretçinin gönderdiği bildirimi kaydeder.
        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            FoundItemReportCreateViewModel model)
        {
            bool hasWrittenLocation =
                !string.IsNullOrWhiteSpace(
                    model.LocationDescription);

            bool hasCoordinates =
                model.Latitude.HasValue &&
                model.Longitude.HasValue;

            // Kullanıcı yazılı konum veya koordinattan
            // en az birini vermelidir.
            if (!hasWrittenLocation && !hasCoordinates)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Bulunduğu yeri yazmalı veya anlık konumunuzu eklemelisiniz.");
            }

            // Koordinatların geçerli aralıkta
            // olup olmadığını kontrol eder.
            if (hasCoordinates &&
                (model.Latitude < -90 ||
                 model.Latitude > 90 ||
                 model.Longitude < -180 ||
                 model.Longitude > 180))
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Konum bilgisi geçerli değil.");
            }

            // Fotoğraf kontrolleri
            if (model.Photo == null)
            {
                ModelState.AddModelError(
                    nameof(model.Photo),
                    "Eşyanın fotoğrafını yüklemelisiniz.");
            }
            else
            {
                string extension =
                    Path.GetExtension(model.Photo.FileName)
                        .ToLowerInvariant();

                string[] allowedExtensions =
                {
                    ".jpg",
                    ".jpeg",
                    ".png",
                    ".webp"
                };

                if (!allowedExtensions.Contains(extension))
                {
                    ModelState.AddModelError(
                        nameof(model.Photo),
                        "Yalnızca JPG, JPEG, PNG veya WEBP fotoğraf yükleyebilirsiniz.");
                }

                const long maximumFileSize =
                    5 * 1024 * 1024;

                if (model.Photo.Length > maximumFileSize)
                {
                    ModelState.AddModelError(
                        nameof(model.Photo),
                        "Fotoğraf en fazla 5 MB olabilir.");
                }

                if (model.Photo.Length == 0)
                {
                    ModelState.AddModelError(
                        nameof(model.Photo),
                        "Seçilen fotoğraf boş olamaz.");
                }
            }

            // Formda hata varsa formu tekrar gösterir.
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            string extensionForSaving =
                Path.GetExtension(model.Photo!.FileName)
                    .ToLowerInvariant();

            string uniqueFileName =
                $"{Guid.NewGuid()}{extensionForSaving}";

            string uploadFolder = Path.Combine(
                _environment.WebRootPath,
                "uploads",
                "found-reports");

            Directory.CreateDirectory(uploadFolder);

            string fullFilePath =
                Path.Combine(uploadFolder, uniqueFileName);

            await using (var fileStream =
                new FileStream(
                    fullFilePath,
                    FileMode.Create))
            {
                await model.Photo.CopyToAsync(fileStream);
            }

            var report = new FoundItemReport
            {
                ItemName = model.ItemName.Trim(),

                Description =
                    model.Description.Trim(),

                LocationDescription =
                    model.LocationDescription?.Trim(),
                CampusArea = model.CampusArea,

                Latitude = model.Latitude,

                Longitude = model.Longitude,

                ContactInformation =
                    model.ContactInformation.Trim(),

                PhotoPath =
                    $"/uploads/found-reports/{uniqueFileName}",

                ReportedAt = DateTime.Now,

                Status =
                    FoundItemReportStatus.Pending
            };

            _context.FoundItemReports.Add(report);

            await _context.SaveChangesAsync();

            return RedirectToAction(
                nameof(Success),
                new { id = report.Id });
        }

        // Bildirim kaydedildikten sonra
        // ziyaretçiye başarı sayfasını gösterir.
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Success(int id)
        {
            var report = await _context.FoundItemReports
                .FirstOrDefaultAsync(x => x.Id == id);

            if (report == null)
            {
                return NotFound();
            }

            ViewBag.ReportId = report.Id;

            // Kullanıcı konumunu eklediyse en yakın
            // güvenlik noktasını hesaplar.
            if (report.Latitude.HasValue &&
                report.Longitude.HasValue)
            {
                var securityPoints = await _context.SecurityPoints
                    .Where(x => x.IsActive)
                    .ToListAsync();

                var nearestPoint = securityPoints
                    .Select(point => new
                    {
                        Point = point,

                        DistanceKm = CalculateDistanceInKilometers(
                            report.Latitude.Value,
                            report.Longitude.Value,
                            point.Latitude,
                            point.Longitude)
                    })
                    .OrderBy(x => x.DistanceKm)
                    .FirstOrDefault();

                if (nearestPoint != null)
                {
                    ViewBag.NearestSecurityPointName =
                        nearestPoint.Point.Name;

                    ViewBag.NearestSecurityPointDescription =
                        nearestPoint.Point.Description;

                    ViewBag.DistanceMeters =
                        Math.Round(nearestPoint.DistanceKm * 1000);

                    ViewBag.DirectionsUrl =
                        FormattableString.Invariant(
                            $"https://www.google.com/maps/dir/?api=1&destination={nearestPoint.Point.Latitude},{nearestPoint.Point.Longitude}");
                }
            }

            else if (!string.IsNullOrWhiteSpace(report.CampusArea))
            {
                string? securityPointName =
                    report.CampusArea switch
                    {
                        "Rektorluk" =>
                            "Rektörlük Güvenlik Noktası",

                        "Kutuphane" =>
                            "Kütüphane Güvenlik Noktası",

                        "Hastane" =>
                            "Hastane Girişi Güvenlik Noktası",

                        "AltGiris" =>
                            "Alt Giriş Güvenlik Noktası",

                        "SporTesisleri" =>
                            "Kapalı Yüzme Havuzu Girişi Güvenlik Noktası",

                        _ => null
                    };

                if (securityPointName != null)
                {
                    var suggestedPoint =
                        await _context.SecurityPoints
                            .FirstOrDefaultAsync(x =>
                                x.IsActive &&
                                x.Name == securityPointName);

                    if (suggestedPoint != null)
                    {

                        ViewBag.IsAreaSuggestion = true;

                        ViewBag.NearestSecurityPointName =
                            suggestedPoint.Name;

                        ViewBag.NearestSecurityPointDescription =
                            suggestedPoint.Description;

                        ViewBag.DirectionsUrl =
                            FormattableString.Invariant(
                                $"https://www.google.com/maps/dir/?api=1&destination={suggestedPoint.Latitude},{suggestedPoint.Longitude}");


                    }
                }
            }


            return View(report);
        }

        // Güvenliğe bekleyen bildirimleri gösterir.
        [Authorize(Roles = "Security,Admin")]
        [HttpGet]
        public async Task<IActionResult> Pending(string sort = "newest")
        {
            var query = _context.FoundItemReports
                .Where(x =>
                    x.Status == FoundItemReportStatus.Pending);

            ViewBag.CurrentSort =
                sort == "oldest" ? "oldest" : "newest";

            var reports = await (sort == "oldest"
                    ? query.OrderBy(x => x.ReportedAt)
                    : query.OrderByDescending(x => x.ReportedAt))
                .ToListAsync();

            return View(reports);
        }

        // Teslim alınmış fakat henüz yayımlanmamış bildirimleri gösterir.
        [Authorize(Roles = "Security,Admin")]
        [HttpGet]
        public async Task<IActionResult> Received()
        {
            var reports = await _context.FoundItemReports
                .Where(x =>
                    x.Status == FoundItemReportStatus.Received)
                .OrderByDescending(x => x.ReportedAt)
                .ToListAsync();

            return View(reports);
        }

        [Authorize(Roles = "Security,Admin")]
        [HttpGet]
        public async Task<IActionResult> Review(int id)
        {
            var report =
                await _context.FoundItemReports
                    .FirstOrDefaultAsync(x =>
                        x.Id == id &&
                        (x.Status == FoundItemReportStatus.Pending ||
                         x.Status == FoundItemReportStatus.Received));

            if (report == null)
            {
                return NotFound();
            }

            return View(report);
        }

        [Authorize(Roles = "Security,Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkReceived(int id)
        {
            var report =
                await _context.FoundItemReports
                    .FirstOrDefaultAsync(x =>
                        x.Id == id &&
                        x.Status == FoundItemReportStatus.Pending);

            if (report == null)
            {
                return NotFound();
            }

            report.Status = FoundItemReportStatus.Received;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Eşya güvenlik tarafından teslim alındı.";

            return RedirectToAction(
                nameof(Review),
                new { id = report.Id });
        }

        [Authorize(Roles = "Security,Admin")]
        [HttpGet]
        public async Task<IActionResult> ConvertToFoundItem(int id)
        {
            var report =
                await _context.FoundItemReports
                    .FirstOrDefaultAsync(x =>
                        x.Id == id &&
                        x.Status == FoundItemReportStatus.Received);

            if (report == null)
            {
                return NotFound();
            }

            var model = new FoundItemReportConvertViewModel
            {
                ReportId = report.Id,
                Name = report.ItemName,
                FoundLocation = report.LocationDescription,
                FoundDate = report.ReportedAt,
                PublicDescription = report.Description
            };

            return View(model);
        }

        [Authorize(Roles = "Security,Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConvertToFoundItem(
            FoundItemReportConvertViewModel model)
        {
            var report =
                await _context.FoundItemReports
                    .FirstOrDefaultAsync(x =>
                        x.Id == model.ReportId &&
                        x.Status == FoundItemReportStatus.Received);

            if (report == null)
            {
                return NotFound();
            }

            if (model.FoundDate > DateTime.Now)
            {
                ModelState.AddModelError(
                    nameof(model.FoundDate),
                    "Bulunma tarihi gelecekte olamaz.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                var foundItem = new FoundItem
                {
                    Name = model.Name.Trim(),
                    Category = model.Category.Trim(),
                    Color = model.Color.Trim(),
                    Building = model.Building.Trim(),
                    FoundLocation =
                        model.FoundLocation?.Trim() ?? string.Empty,
                    FoundDate = model.FoundDate,
                    PublicDescription =
                        model.PublicDescription.Trim(),
                    SecretDetail =
                        model.SecretDetail.Trim(),
                    IsDelivered = false
                };

                _context.FoundItems.Add(foundItem);
                await _context.SaveChangesAsync();

                report.Status =
                    FoundItemReportStatus.Published;

                report.CreatedFoundItemId =
                    foundItem.Id;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                TempData["SuccessMessage"] =
                    "Bildirim bulunan eşya ilanına dönüştürüldü.";

                return RedirectToAction(
                    "Details",
                    "FoundItems",
                    new { id = foundItem.Id });
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        private static double CalculateDistanceInKilometers(
    double latitude1,
    double longitude1,
    double latitude2,
    double longitude2)
        {
            const double earthRadiusKm = 6371;

            double latitudeDifference =
                DegreesToRadians(latitude2 - latitude1);

            double longitudeDifference =
                DegreesToRadians(longitude2 - longitude1);

            double calculation =
                Math.Sin(latitudeDifference / 2) *
                Math.Sin(latitudeDifference / 2) +
                Math.Cos(DegreesToRadians(latitude1)) *
                Math.Cos(DegreesToRadians(latitude2)) *
                Math.Sin(longitudeDifference / 2) *
                Math.Sin(longitudeDifference / 2);

            double angle =
                2 * Math.Atan2(
                    Math.Sqrt(calculation),
                    Math.Sqrt(1 - calculation));

            return earthRadiusKm * angle;
        }

        private static double DegreesToRadians(double degree)
        {
            return degree * Math.PI / 180;
        }
    }
}
