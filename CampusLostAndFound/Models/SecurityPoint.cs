using System.ComponentModel.DataAnnotations;

namespace CampusLostAndFound.Models
{
    public class SecurityPoint
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Güvenlik Noktası")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Konum Açıklaması")]
        public string Description { get; set; } = string.Empty;

        [Required]
        public double Latitude { get; set; }

        [Required]
        public double Longitude { get; set; }

        [Display(Name = "Aktif")]
        public bool IsActive { get; set; } = true;
    }
}