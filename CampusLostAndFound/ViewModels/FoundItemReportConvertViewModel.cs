using System.ComponentModel.DataAnnotations;

namespace CampusLostAndFound.ViewModels
{
    public class FoundItemReportConvertViewModel
    {
        public int ReportId { get; set; }

        [Required(ErrorMessage = "Eşya adı zorunludur.")]
        [StringLength(
            100,
            ErrorMessage = "Eşya adı en fazla 100 karakter olabilir.")]
        [Display(Name = "Eşya Adı")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Kategori zorunludur.")]
        [StringLength(
            50,
            ErrorMessage = "Kategori en fazla 50 karakter olabilir.")]
        [Display(Name = "Kategori")]
        public string Category { get; set; } = string.Empty;

        [Required(ErrorMessage = "Renk zorunludur.")]
        [StringLength(
            50,
            ErrorMessage = "Renk en fazla 50 karakter olabilir.")]
        [Display(Name = "Renk")]
        public string Color { get; set; } = string.Empty;

        [Required(ErrorMessage = "Bulunduğu bina zorunludur.")]
        [StringLength(
            100,
            ErrorMessage = "Bina adı en fazla 100 karakter olabilir.")]
        [Display(Name = "Bulunduğu Bina")]
        public string Building { get; set; } = string.Empty;

        [StringLength(
            150,
            ErrorMessage = "Bulunduğu yer en fazla 150 karakter olabilir.")]
        [Display(Name = "Bulunduğu Yer")]
        public string? FoundLocation { get; set; }

        [Required(ErrorMessage = "Bulunma tarihi zorunludur.")]
        [Display(Name = "Bulunma Tarihi ve Saati")]
        [DataType(DataType.DateTime)]
        public DateTime FoundDate { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Herkese açık açıklama zorunludur.")]
        [StringLength(
            500,
            ErrorMessage = "Açıklama en fazla 500 karakter olabilir.")]
        [Display(Name = "Herkese Açık Açıklama")]
        public string PublicDescription { get; set; } = string.Empty;

        [Required(ErrorMessage = "Gizli ayırt edici bilgi zorunludur.")]
        [StringLength(
            500,
            ErrorMessage = "Gizli bilgi en fazla 500 karakter olabilir.")]
        [Display(Name = "Gizli Ayırt Edici Bilgi")]
        public string SecretDetail { get; set; } = string.Empty;
    }
}