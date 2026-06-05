using EatsDash.Data;
using EatsDash.Helpers;
using EatsDash.Models;
using EatsDash.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EatsDash.Controllers;

public class HomeController : Controller
{
    private static readonly Dictionary<string, string> ReportReasons = new()
    {
        ["spam"] = "Спам",
        ["insult"] = "Оскорбления",
        ["fake"] = "Недостоверная информация",
        ["other"] = "Другое"
    };

    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public HomeController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var approved = _db.Reviews.Where(r => r.Status == ReviewStatus.Approved);

        var reviews = await approved
            .OrderByDescending(r => r.CreatedAt)
            .Take(20)
            .ToListAsync();

        var stats = await approved
            .GroupBy(_ => 1)
            .Select(g => new
            {
                Count = g.Count(),
                Avg = g.Average(r => (double?)r.Rating)
            })
            .FirstOrDefaultAsync();

        var couriers = await _db.Couriers
            .Where(c => c.IsActive)
            .OrderBy(c => c.Nickname)
            .Select(c => new CourierOptionViewModel { Id = c.Id, Nickname = c.Nickname })
            .ToListAsync();

        var user = User.Identity?.IsAuthenticated == true
            ? await _userManager.GetUserAsync(User)
            : null;

        var userId = user?.Id;
        var reviewIds = reviews.Select(r => r.Id).ToList();

        var reactions = userId == null || reviewIds.Count == 0
            ? new Dictionary<int, bool>()
            : await _db.ReviewReactions
                .AsNoTracking()
                .Where(r => r.UserId == userId && reviewIds.Contains(r.ReviewId))
                .ToDictionaryAsync(r => r.ReviewId, r => r.IsLike);

        var isModerator = user?.IsModerator ?? false;
        var authorIds = reviews.Select(r => r.AuthorId).Distinct().ToList();
        var authors = authorIds.Count == 0
            ? new Dictionary<string, ApplicationUser>()
            : await _db.Users
                .AsNoTracking()
                .Where(u => authorIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id);

        var model = new HomeIndexViewModel
        {
            TotalReviews = stats?.Count ?? 0,
            AverageRating = stats?.Avg,
            IsAuthenticated = user != null,
            CompactReviews = user?.CompactReviews ?? false,
            IsModerator = isModerator,
            UserName = user?.DisplayName ?? user?.UserName,
            UserInitial = GetInitial(user?.DisplayName ?? user?.UserName ?? ""),
            UserAvatarToneClass = user != null ? AvatarHelper.GetToneClass(user.Id) : "avatar-tone-purple-1",
            Couriers = couriers,
            RecentReviews = reviews.Select(r =>
            {
                authors.TryGetValue(r.AuthorId, out var author);
                var isOwn = userId != null && r.AuthorId == userId;
                var canModerate = isModerator && !isOwn;

                return new ReviewItemViewModel
                {
                    Id = r.Id,
                    AuthorId = r.AuthorId,
                    AuthorName = r.AuthorName,
                    AuthorInitial = GetInitial(r.AuthorName),
                    AvatarToneClass = AvatarHelper.GetToneClass(r.AuthorId),
                    CourierNickname = r.CourierNickname,
                    CourierId = r.CourierId,
                    CreatedAt = r.CreatedAt.ToLocalTime(),
                    Rating = r.Rating,
                    Text = r.Text,
                    LikesCount = r.LikesCount,
                    DislikesCount = r.DislikesCount,
                    CanEdit = isOwn,
                    CanReport = userId != null && !isOwn && !isModerator,
                    CanModerate = canModerate,
                    CanBlockAuthor = canModerate
                        && author is { IsModerator: false, IsBlocked: false },
                    UserReaction = reactions.TryGetValue(r.Id, out var isLike)
                        ? (isLike ? "like" : "dislike")
                        : null
                };
            }).ToList()
        };

        return View(model);
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateReview(CreateReviewViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["ReviewError"] = "Проверьте поля отзыва.";
            return RedirectToAction(nameof(Index));
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction(nameof(Index));

        var courier = await _db.Couriers.FindAsync(model.CourierId);
        if (courier == null || !courier.IsActive)
        {
            TempData["ReviewError"] = "Выберите курьера из списка.";
            return RedirectToAction(nameof(Index));
        }

        _db.Reviews.Add(new Review
        {
            AuthorId = user.Id,
            AuthorName = model.AuthorName.Trim(),
            CourierId = courier.Id,
            CourierNickname = courier.Nickname,
            Rating = model.Rating,
            Text = model.Text.Trim(),
            CreatedAt = DateTime.UtcNow,
            Status = ReviewStatus.Approved
        });
        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditReview(EditReviewViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["ReviewError"] = "Проверьте поля отзыва.";
            return RedirectToAction(nameof(Index));
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction(nameof(Index));

        var review = await _db.Reviews.FindAsync(model.Id);
        if (review == null || review.AuthorId != user.Id)
        {
            return RedirectToAction(nameof(Index));
        }

        var courier = await _db.Couriers.FindAsync(model.CourierId);
        if (courier == null || !courier.IsActive)
        {
            TempData["ReviewError"] = "Выберите курьера из списка.";
            return RedirectToAction(nameof(Index));
        }

        review.AuthorName = model.AuthorName.Trim();
        review.CourierId = courier.Id;
        review.CourierNickname = courier.Nickname;
        review.Rating = model.Rating;
        review.Text = model.Text.Trim();
        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteReview(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction(nameof(Index));

        var review = await _db.Reviews.FindAsync(id);
        if (review == null)
        {
            return RedirectToAction(nameof(Index));
        }

        if (review.AuthorId != user.Id && !user.IsModerator)
        {
            TempData["HomeAlert"] = "Недостаточно прав для удаления отзыва.";
            return RedirectToAction(nameof(Index));
        }

        _db.Reviews.Remove(review);
        await _db.SaveChangesAsync();

        TempData["HomeAlert"] = user.IsModerator && review.AuthorId != user.Id
            ? "Отзыв удалён модератором."
            : null;

        return RedirectToAction(nameof(Index));
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> BlockAuthor(string authorId)
    {
        var moderator = await _userManager.GetUserAsync(User);
        if (moderator == null || !moderator.IsModerator)
        {
            TempData["HomeAlert"] = "Действие доступно только модераторам.";
            return RedirectToAction(nameof(Index));
        }

        if (string.IsNullOrWhiteSpace(authorId) || authorId == moderator.Id)
        {
            TempData["HomeAlert"] = "Нельзя заблокировать себя.";
            return RedirectToAction(nameof(Index));
        }

        var author = await _userManager.FindByIdAsync(authorId);
        if (author == null)
        {
            TempData["HomeAlert"] = "Пользователь не найден.";
            return RedirectToAction(nameof(Index));
        }

        if (author.IsModerator)
        {
            TempData["HomeAlert"] = "Нельзя заблокировать модератора.";
            return RedirectToAction(nameof(Index));
        }

        if (author.IsBlocked)
        {
            TempData["HomeAlert"] = "Пользователь уже заблокирован.";
            return RedirectToAction(nameof(Index));
        }

        author.IsBlocked = true;
        await _userManager.UpdateAsync(author);

        TempData["HomeAlert"] = $"Пользователь «{author.DisplayName ?? author.UserName}» заблокирован.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> React(int reviewId, string reaction)
    {
        if (reaction != "like" && reaction != "dislike")
        {
            return BadRequest(new { error = "Некорректная реакция." });
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var isLike = reaction == "like";
        var review = await _db.Reviews.FirstOrDefaultAsync(r =>
            r.Id == reviewId && r.Status == ReviewStatus.Approved);

        if (review == null) return NotFound(new { error = "Отзыв не найден." });

        var existing = await _db.ReviewReactions
            .FirstOrDefaultAsync(r => r.ReviewId == reviewId && r.UserId == user.Id);

        if (existing != null)
        {
            if (existing.IsLike == isLike)
            {
                _db.ReviewReactions.Remove(existing);
            }
            else
            {
                existing.IsLike = isLike;
            }
        }
        else
        {
            _db.ReviewReactions.Add(new ReviewReaction
            {
                ReviewId = reviewId,
                UserId = user.Id,
                IsLike = isLike
            });
        }

        await _db.SaveChangesAsync();

        review.LikesCount = await _db.ReviewReactions.CountAsync(r => r.ReviewId == reviewId && r.IsLike);
        review.DislikesCount = await _db.ReviewReactions.CountAsync(r => r.ReviewId == reviewId && !r.IsLike);
        await _db.SaveChangesAsync();

        var current = await _db.ReviewReactions
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.ReviewId == reviewId && r.UserId == user.Id);

        return Json(new
        {
            likesCount = review.LikesCount,
            dislikesCount = review.DislikesCount,
            userReaction = current == null ? null : (current.IsLike ? "like" : "dislike")
        });
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ReportReview(ReportReviewViewModel model)
    {
        if (string.IsNullOrWhiteSpace(model.ReasonKey) || !ReportReasons.ContainsKey(model.ReasonKey))
        {
            TempData["ReportError"] = "Выберите причину жалобы.";
            return RedirectToAction(nameof(Index));
        }

        if (model.ReasonKey == "other" && string.IsNullOrWhiteSpace(model.CustomReason))
        {
            TempData["ReportError"] = "Укажите свою причину жалобы.";
            return RedirectToAction(nameof(Index));
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction(nameof(Index));

        var review = await _db.Reviews.FindAsync(model.ReviewId);
        if (review == null)
        {
            TempData["ReportError"] = "Отзыв не найден.";
            return RedirectToAction(nameof(Index));
        }

        if (review.AuthorId == user.Id)
        {
            TempData["ReportError"] = "Нельзя пожаловаться на свой отзыв.";
            return RedirectToAction(nameof(Index));
        }

        var reasonText = model.ReasonKey == "other"
            ? model.CustomReason!.Trim()
            : ReportReasons[model.ReasonKey];

        _db.ReviewReports.Add(new ReviewReport
        {
            ReviewId = model.ReviewId,
            ReporterId = user.Id,
            Reason = reasonText,
            Comment = string.IsNullOrWhiteSpace(model.Comment) ? null : model.Comment.Trim(),
            CreatedAt = DateTime.UtcNow
        });

        if (review.Status != ReviewStatus.PendingModeration)
        {
            review.Status = ReviewStatus.PendingModeration;
        }

        await _db.SaveChangesAsync();

        TempData["ReportSuccess"] = "Жалоба отправлена. Отзыв передан модератору.";
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Error() => View();

    private static string GetInitial(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return "?";
        return char.ToUpper(name.Trim()[0]).ToString();
    }
}
