using System.ComponentModel.DataAnnotations;

namespace CampusLostAndFound.Models
{
    public enum ClaimStatus
    {
        Pending,
        Approved,
        Rejected,
        Closed
    }

    public class OwnershipClaim
    {
        public int Id { get; set; }

        [Required]
        public int FoundItemId { get; set; }

        public FoundItem? FoundItem { get; set; }

        [Required]
        public string StudentId { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        [Display(Name = "Sahiplik Doğrulama Açıklaması")]
        public string ProofDescription { get; set; } = string.Empty;

        [Display(Name = "Talep Tarihi")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Display(Name = "Talep Durumu")]
        public ClaimStatus Status { get; set; } =
            ClaimStatus.Pending;

        [StringLength(500)]
        [Display(Name = "Görevli Notu")]
        public string? ReviewNote { get; set; }

        [Display(Name = "İncelenme Tarihi")]
        public DateTime? ReviewedAt { get; set; }
    }
}