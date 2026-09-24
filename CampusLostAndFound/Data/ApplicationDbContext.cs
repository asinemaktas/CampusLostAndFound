using CampusLostAndFound.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CampusLostAndFound.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<FoundItem> FoundItems { get; set; } = null!;

        public DbSet<OwnershipClaim> OwnershipClaims { get; set; } = null!;

        public DbSet<FoundItemReport> FoundItemReports { get; set; } = null!;

        public DbSet<SecurityPoint> SecurityPoints { get; set; } = null!;
    }
}