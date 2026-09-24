using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CampusLostAndFound.Models
{
    public class FoundItem : IValidatableObject
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Eşya adı zorunludur.")]
        [StringLength(
            100,
            ErrorMessage = "Eşya adı en fazla 100 karakter olabilir.")]
        [Display(Name = "Eşya Adı")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Kategori seçimi zorunludur.")]
        [StringLength(
            50,
            ErrorMessage = "Kategori en fazla 50 karakter olabilir.")]
        [Display(Name = "Kategori")]
        public string Category { get; set; } = string.Empty;

        [Required(ErrorMessage = "Renk bilgisi zorunludur.")]
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

        [Required(ErrorMessage = "Bulunduğu yer zorunludur.")]
        [StringLength(
            150,
            ErrorMessage = "Bulunduğu yer en fazla 150 karakter olabilir.")]
        [Display(Name = "Bulunduğu Yer")]
        public string FoundLocation { get; set; } = string.Empty;

        [Display(Name = "Bulunma Tarihi ve Saati")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(
            DataFormatString = "{0:dd.MM.yyyy HH:mm}",
            ApplyFormatInEditMode = false)]
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

        [Display(Name = "Teslim Edildi")]
        public bool IsDelivered { get; set; } = false;

        public IEnumerable<ValidationResult> Validate(
            ValidationContext validationContext)
        {
            if (FoundDate > DateTime.Now)
            {
                yield return new ValidationResult(
                    "Bulunma tarihi gelecekte olamaz.",
                    new[] { nameof(FoundDate) });
            }
        }
    }
}