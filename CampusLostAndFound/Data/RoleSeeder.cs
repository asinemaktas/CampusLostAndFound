using Microsoft.AspNetCore.Identity;

namespace CampusLostAndFound.Data
{
    public static class RoleSeeder
    {
        public static async Task SeedRolesAsync(
            IServiceProvider serviceProvider)
        {
            var roleManager =
                serviceProvider.GetRequiredService<
                    RoleManager<IdentityRole>>();

            var userManager =
                serviceProvider.GetRequiredService<
                    UserManager<IdentityUser>>();

            string[] roleNames =
            {
                "Student",
                "Security",
                "Admin"
            };

            // Roller yoksa oluşturulur.
            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(
                        new IdentityRole(roleName));
                }
            }

            // Sistemin ilk admin hesabı
            const string adminEmail = "admin@test.com";
            const string adminPassword = "DemoAdmin123!";

            var adminUser =
                await userManager.FindByEmailAsync(adminEmail);

            // Admin hesabı yoksa oluşturulur.
            if (adminUser == null)
            {
                adminUser = new IdentityUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };

                var result =
                    await userManager.CreateAsync(
                        adminUser,
                        adminPassword);

                if (!result.Succeeded)
                {
                    var errors = string.Join(
                        ", ",
                        result.Errors.Select(
                            x => x.Description));

                    throw new Exception(
                        $"Admin hesabı oluşturulamadı: {errors}");
                }
            }

            // Hesaba Admin rolü verilir.
            if (!await userManager.IsInRoleAsync(
                adminUser,
                "Admin"))
            {
                await userManager.AddToRoleAsync(
                    adminUser,
                    "Admin");
            }

            // Bu işlem yalnızca eski hesapları temizlemek
            // için bir defa çalıştırılacaktır.
            const bool cleanupOldAccounts = false;

            if (cleanupOldAccounts)
            {
                string[] accountsToDelete =
                {
                    "admin@kampus.com",
                    "ogrenci@test.com",
                    "ogrenci1@test.com"
                };

                foreach (var email in accountsToDelete)
                {
                    var userToDelete =
                        await userManager.FindByEmailAsync(email);

                    if (userToDelete != null)
                    {
                        var deleteResult =
                            await userManager.DeleteAsync(
                                userToDelete);

                        if (!deleteResult.Succeeded)
                        {
                            var errors = string.Join(
                                ", ",
                                deleteResult.Errors.Select(
                                    x => x.Description));

                            throw new Exception(
                                $"{email} hesabı silinemedi: {errors}");
                        }
                    }
                }
            }
        }
    }
}