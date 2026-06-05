using EatsDash.Models;
using EatsDash.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EatsDash.ViewComponents;

public class HelpViewComponent : ViewComponent
{
    private readonly UserManager<ApplicationUser> _userManager;

    public HelpViewComponent(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        HelpAudience audience = HelpAudience.Guest;

        if (User.Identity?.IsAuthenticated == true)
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            audience = user?.IsModerator == true ? HelpAudience.Moderator : HelpAudience.User;
        }

        return View(new HelpViewModel { Audience = audience });
    }
}
