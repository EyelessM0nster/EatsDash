namespace EatsDash.ViewModels;

public class HomeIndexViewModel
{
    public int TotalReviews { get; set; }
    public double? AverageRating { get; set; }
    public bool IsAuthenticated { get; set; }
    public bool CompactReviews { get; set; }
    public bool IsModerator { get; set; }
    public string? UserName { get; set; }
    public string UserInitial { get; set; } = "?";
    public string UserAvatarToneClass { get; set; } = "avatar-tone-purple-1";
    public List<CourierOptionViewModel> Couriers { get; set; } = new();
    public List<ReviewItemViewModel> RecentReviews { get; set; } = new();
}

public class CourierOptionViewModel
{
    public int Id { get; set; }
    public string Nickname { get; set; } = string.Empty;
}

public class ReviewItemViewModel
{
    public int Id { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public string AuthorInitial { get; set; } = "?";
    public string AvatarToneClass { get; set; } = "avatar-tone-purple-1";
    public string CourierNickname { get; set; } = string.Empty;
    public int CourierId { get; set; }
    public DateTime CreatedAt { get; set; }
    public int Rating { get; set; }
    public string Text { get; set; } = string.Empty;
    public int LikesCount { get; set; }
    public int DislikesCount { get; set; }
    public string AuthorId { get; set; } = string.Empty;
    public bool CanEdit { get; set; }
    public bool CanReport { get; set; }
    public bool CanModerate { get; set; }
    public bool CanBlockAuthor { get; set; }
    public string? UserReaction { get; set; }
}
