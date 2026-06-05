using EatsDash.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;

namespace EatsDash.Data;

public static class ModeratorSeeder
{
    /// <summary>
    /// Создаёт учётную запись модератора и помечает её флагом IsModerator.
    /// Логин и пароль задаются в appsettings.json → секция Moderator.
    /// </summary>
    public static async Task SeedAsync(
        UserManager<ApplicationUser> userManager,
        IConfiguration configuration)
    {
        var section = configuration.GetSection("Moderator");
        var userName = (section["UserName"] ?? "moderator").Trim();
        var password = section["Password"] ?? "Mod_2026!";

        if (string.IsNullOrEmpty(userName))
        {
            return;
        }

        var user = await userManager.FindByNameAsync(userName);
        if (user == null)
        {
            user = new ApplicationUser
            {
                UserName = userName,
                DisplayName = section["DisplayName"] ?? "Модератор",
                Email = $"{userName.Replace(" ", "_")}@eatsdash.local",
                EmailConfirmed = true,
                IsBlocked = false
            };

            var create = await userManager.CreateAsync(user, password);
            if (!create.Succeeded)
            {
                return;
            }
        }

        if (!user.IsModerator)
        {
            user.IsModerator = true;
            user.IsBlocked = false;
            if (string.IsNullOrWhiteSpace(user.DisplayName))
            {
                user.DisplayName = section["DisplayName"] ?? "Модератор";
            }

            await userManager.UpdateAsync(user);
        }
    }
}
