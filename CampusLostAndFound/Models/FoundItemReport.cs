using System.ComponentModel.DataAnnotations;

namespace CampusLostAndFound.Models
{
    public enum FoundItemReportStatus
    {
        Pending,
        Received,
        Published,
        Rejected
    }

    public class FoundItemReport
    {
        public int Id { get; set; }

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

        [MaxLength(
             100,
             ErrorMessage = "Kampüs bölgesi en fazla 100 karakter olabilir.")]
        [Display(Name = "Kampüs Bölgesi")]
        public string? CampusArea { get; set; }

        [Display(Name = "Enlem")]
        public double? Latitude { get; set; }

        [Display(Name = "Boylam")]
        public double? Longitude { get; set; }

        [Required(ErrorMessage = "İletişim bilgisi zorunludur.")]
        [StringLength(
            150,
            ErrorMessage = "İletişim bilgisi en fazla 150 karakter olabilir.")]
        [Display(Name = "Telefon veya E-posta")]
        public string ContactInformation { get; set; } = string.Empty;

        [Display(Name = "Fotoğraf")]
        public string? PhotoPath { get; set; }

        [Display(Name = "Bildirim Tarihi")]
        public DateTime ReportedAt { get; set; } = DateTime.Now;

        [Display(Name = "Bildirim Durumu")]
        public FoundItemReportStatus Status { get; set; }
            = FoundItemReportStatus.Pending;

        [Display(Name = "İlana Dönüştürülen Eşya")]
        public int? CreatedFoundItemId { get; set; }
    }
}