using EatsDash.Data;
using EatsDash.Helpers;
using EatsDash.Models;
using EatsDash.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EatsDash.Controllers;

[Authorize]
public class ModerationController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public ModerationController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    private async Task<ApplicationUser?> GetModeratorAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null || !user.IsModerator)
        {
            return null;
        }

        return user;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var moderator = await GetModeratorAsync();
        if (moderator == null)
        {
            TempData["HomeAlert"] = "Раздел доступен только модераторам.";
            return RedirectToAction("Index", "Home");
        }

        var pending = await _db.Reviews
            .AsNoTracking()
            .Where(r => r.Status == ReviewStatus.PendingModeration)
            .OrderBy(r => r.CreatedAt)
            .ToListAsync();

        var ids = pending.Select(r => r.Id).ToList();
        var reports = ids.Count == 0
            ? new List<ReviewReport>()
            : await _db.ReviewReports
                .AsNoTracking()
                .Where(r => ids.Contains(r.ReviewId))
                .ToListAsync();

        var reporterIds = reports.Select(r => r.ReporterId).Distinct().ToList();
        var reporters = reporterIds.Count == 0
            ? new Dictionary<string, string>()
            : await _db.Users
                .AsNoTracking()
                .Where(u => reporterIds.Contains(u.Id))
                .ToDictionaryAsync(
                    u => u.Id,
                    u => string.IsNullOrWhiteSpace(u.DisplayName) ? u.UserName ?? u.Id : u.DisplayName);

        var items = pending.Select(r =>
        {
            var lines = reports
                .Where(rep => rep.ReviewId == r.Id)
                .Select(rep =>
                {
                    var who = reporters.TryGetValue(rep.ReporterId, out var n) ? n : "пользователь";
                    var extra = string.IsNullOrWhiteSpace(rep.Comment) ? "" : $" — «{rep.Comment}»";
                    return $"{rep.Reason} (от {who}){extra}";
                })
                .ToList();

            return new ModerationQueueItemViewModel
            {
                ReviewId = r.Id,
                AuthorName = r.AuthorName,
                Text = r.Text,
                Rating = r.Rating,
                CourierNickname = r.CourierNickname,
                CreatedAt = r.CreatedAt.ToLocalTime(),
                ReportLines = lines
            };
        }).ToList();

        var displayName = string.IsNullOrWhiteSpace(moderator.DisplayName)
            ? moderator.UserName ?? ""
            : moderator.DisplayName;

        var model = new ModerationIndexViewModel
        {
            PendingCount = items.Count,
            UserName = displayName,
            UserInitial = GetInitial(displayName),
            UserAvatarToneClass = AvatarHelper.GetToneClass(moderator.Id),
            PendingReviews = items
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(int id)
    {
        if (await GetModeratorAsync() == null)
        {
            TempData["HomeAlert"] = "Действие доступно только модераторам.";
            return RedirectToAction("Index", "Home");
        }

        var review = await _db.Reviews.FindAsync(id);
        if (review == null || review.Status != ReviewStatus.PendingModeration)
        {
            TempData["ModMessage"] = "Отзыв не найден или уже обработан.";
            return RedirectToAction(nameof(Index));
        }

        review.Status = ReviewStatus.Approved;
        await _db.SaveChangesAsync();

        TempData["ModSuccess"] = "Отзыв одобрен и снова виден на главной.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(int id)
    {
        if (await GetModeratorAsync() == null)
        {
            TempData["HomeAlert"] = "Действие доступно только модераторам.";
            return RedirectToAction("Index", "Home");
        }

        var review = await _db.Reviews.FindAsync(id);
        if (review == null || review.Status != ReviewStatus.PendingModeration)
        {
            TempData["ModMessage"] = "Отзыв не найден или уже обработан.";
            return RedirectToAction(nameof(Index));
        }

        _db.Reviews.Remove(review);
        await _db.SaveChangesAsync();

        TempData["ModSuccess"] = "Отзыв отклонён и удалён.";
        return RedirectToAction(nameof(Index));
    }

    private static string GetInitial(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return "?";
        return char.ToUpper(name.Trim()[0]).ToString();
    }
}
