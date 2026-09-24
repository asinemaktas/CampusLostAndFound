using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace CampusLostAndFound.ViewModels
{
    public class FoundItemReportCreateViewModel
    {
        [Required(ErrorMessage = "Eşya adı zorunludur.")]
        [StringLength(
            100,
            ErrorMessage = "Eşya adı en fazla 100 karakter olabilir.")]
        [Display(Name = "Eşya Adı")]
        public string ItemName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Açıklama zorunludur.")]
        [StringLength(
            500,
            ErrorMessage = "Açıklama en fazla 500 karakter olabilir.")]
        [Display(Name = "Kısa Açıklama")]
        public string Description { get; set; } = string.Empty;

        [StringLength(
            250,
            ErrorMessage = "Konum açıklaması en fazla 250 karakter olabilir.")]
        [Display(Name = "Bulunduğu Yer")]
        public string? LocationDescription { get; set; }

        [Display(Name = "Bulunduğunuz Kampüs Bölgesi")]
        public string? CampusArea { get; set; }

        public double? Latitude { get; set; }

        public double? Longitude { get; set; }

        [Required(ErrorMessage = "İletişim bilgisi zorunludur.")]
        [StringLength(
            150,
            ErrorMessage = "İletişim bilgisi en fazla 150 karakter olabilir.")]
        [Display(Name = "Telefon veya E-posta")]
        public string ContactInformation { get; set; } = string.Empty;

        [Required(ErrorMessage = "Eşyanın fotoğrafını yüklemelisiniz.")]
        [Display(Name = "Eşya Fotoğrafı")]
        public IFormFile? Photo { get; set; }
    }
}