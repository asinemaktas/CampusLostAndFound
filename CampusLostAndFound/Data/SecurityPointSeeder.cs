using CampusLostAndFound.Models;
using Microsoft.EntityFrameworkCore;

namespace CampusLostAndFound.Data
{
    public static class SecurityPointSeeder
    {
        public static async Task SeedAsync(
            IServiceProvider serviceProvider)
        {
            var context =
                serviceProvider.GetRequiredService<ApplicationDbContext>();

            // Daha önce güvenlik noktası eklenmişse
            // tekrar eklenmesini engeller.
            if (await context.SecurityPoints.AnyAsync())
            {
                return;
            }

            var securityPoints = new List<SecurityPoint>
            {
                new SecurityPoint
                {
                    Name = "Rektörlük Güvenlik Noktası",
                    Description = "Düzce Üniversitesi Rektörlük binası girişi.",
                    Latitude = 40.907259,
                    Longitude = 31.183071
                },

                new SecurityPoint
                {
                    Name = "Kütüphane Güvenlik Noktası",
                    Description = "Düzce Üniversitesi Kütüphane binası girişi.",
                    Latitude = 40.906466,
                    Longitude = 31.179218
                },

                new SecurityPoint
                {
                    Name = "Hastane Girişi Güvenlik Noktası",
                    Description = "Üniversite hastanesi tarafındaki kampüs girişi.",
                    Latitude = 40.906446,
                    Longitude = 31.174508
                },

                new SecurityPoint
                {
                    Name = "Alt Giriş Güvenlik Noktası",
                    Description = "Konuralp Yerleşkesi alt giriş bölümü.",
                    Latitude = 40.902898,
                    Longitude = 31.183122
                },

                new SecurityPoint
                {
                    Name = "Kapalı Yüzme Havuzu Girişi Güvenlik Noktası",
                    Description = "Kapalı yüzme havuzu tarafındaki kampüs girişi.",
                    Latitude = 40.901110,
                    Longitude = 31.178052
                }
            };

            context.SecurityPoints.AddRange(securityPoints);
            await context.SaveChangesAsync();
        }
    }
}