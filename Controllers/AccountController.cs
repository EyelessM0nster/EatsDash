using EatsDash.Data;
using EatsDash.Helpers;
using EatsDash.Models;
using EatsDash.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EatsDash.Controllers;

public class AccountController : Controller
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _db;

    public AccountController(
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext db)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _db = db;
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Profile()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Index", "Home");

        var displayName = string.IsNullOrWhiteSpace(user.DisplayName)
            ? user.UserName ?? ""
            : user.DisplayName;

        var model = new ProfileViewModel
        {
            DisplayName = displayName,
            UserName = user.UserName ?? "",
            AuthorInitial = GetInitial(displayName),
            AvatarToneClass = AvatarHelper.GetToneClass(user.Id),
            MyReviewsCount = await _db.Reviews.CountAsync(r =>
                r.AuthorId == user.Id && r.Status == ReviewStatus.Approved),
            CompactReviews = user.CompactReviews,
            SuccessMessage = TempData["ProfileSuccess"] as string,
            ErrorMessage = TempData["ProfileError"] as string,
            IsModerator = user.IsModerator
        };

        return View(model);
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateProfile(UpdateProfileViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["ProfileError"] = string.Join(" ", ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage));
            return RedirectToAction(nameof(Profile));
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Index", "Home");

        var newName = model.DisplayName.Trim();

        if (user.UserName != newName)
        {
            var setNameResult = await _userManager.SetUserNameAsync(user, newName);
            if (!setNameResult.Succeeded)
            {
                TempData["ProfileError"] = string.Join(" ", setNameResult.Errors.Select(e => e.Description));
                return RedirectToAction(nameof(Profile));
            }
        }

        user.DisplayName = newName;
        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            TempData["ProfileError"] = string.Join(" ", updateResult.Errors.Select(e => e.Description));
            return RedirectToAction(nameof(Profile));
        }

        await _db.Reviews
            .Where(r => r.AuthorId == user.Id)
            .ExecuteUpdateAsync(s => s.SetProperty(r => r.AuthorName, newName));

        await _signInManager.RefreshSignInAsync(user);

        TempData["ProfileSuccess"] = "Никнейм обновлён.";
        return RedirectToAction(nameof(Profile));
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateSettings(UpdateSettingsViewModel model)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Index", "Home");

        user.CompactReviews = model.CompactReviews;

        await _userManager.UpdateAsync(user);

        TempData["ProfileSuccess"] = "Настройки сохранены.";
        return RedirectToAction(nameof(Profile));
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["ProfileError"] = string.Join(" ", ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage));
            return RedirectToAction(nameof(Profile));
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Index", "Home");

        var result = await _userManager.ChangePasswordAsync(
            user, model.CurrentPassword, model.NewPassword);

        if (!result.Succeeded)
        {
            TempData["ProfileError"] = string.Join(" ", result.Errors.Select(e => e.Description));
            return RedirectToAction(nameof(Profile));
        }

        await _signInManager.RefreshSignInAsync(user);
        TempData["ProfileSuccess"] = "Пароль изменён.";
        return RedirectToAction(nameof(Profile));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        returnUrl ??= Url.Action("Index", "Home");

        if (!ModelState.IsValid)
        {
            TempData["AuthTab"] = "login";
            TempData["AuthError"] = string.Join(" ", ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage));
            return Redirect(returnUrl!);
        }

        var userName = model.UserName.Trim();

        var existingUser = await _userManager.FindByNameAsync(userName);
        if (existingUser is { IsBlocked: true })
        {
            TempData["AuthTab"] = "login";
            TempData["AuthError"] = "Ваш аккаунт заблокирован модератором.";
            return Redirect(returnUrl!);
        }

        var result = await _signInManager.PasswordSignInAsync(
            userName,
            model.Password,
            isPersistent: true,
            lockoutOnFailure: false);

        if (result.Succeeded)
        {
            return Redirect(returnUrl!);
        }

        TempData["AuthTab"] = "login";
        TempData["AuthError"] = "Неверное имя пользователя или пароль.";
        return Redirect(returnUrl!);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model, string? returnUrl = null)
    {
        returnUrl ??= Url.Action("Index", "Home");

        if (!ModelState.IsValid)
        {
            TempData["AuthTab"] = "register";
            TempData["AuthError"] = string.Join(" ", ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage));
            return Redirect(returnUrl!);
        }

        var userName = model.UserName.Trim();

        var user = new ApplicationUser
        {
            UserName = userName,
            DisplayName = userName,
            IsBlocked = false
        };

        var result = await _userManager.CreateAsync(user, model.Password);

        if (result.Succeeded)
        {
            await _signInManager.SignInAsync(user, isPersistent: true);
            return Redirect(returnUrl!);
        }

        TempData["AuthTab"] = "register";
        TempData["AuthError"] = string.Join(" ", result.Errors.Select(e => e.Description));
        return Redirect(returnUrl!);
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }

    private static string GetInitial(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return "?";
        return char.ToUpper(name.Trim()[0]).ToString();
    }
}
