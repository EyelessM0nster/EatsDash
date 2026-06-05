namespace EatsDash.ViewModels;

public class ModerationIndexViewModel
{
    public int PendingCount { get; set; }
    public string UserInitial { get; set; } = "?";
    public string UserAvatarToneClass { get; set; } = "avatar-tone-purple-1";
    public string? UserName { get; set; }
    public List<ModerationQueueItemViewModel> PendingReviews { get; set; } = new();
}

public class ModerationQueueItemViewModel
{
    public int ReviewId { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string CourierNickname { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public List<string> ReportLines { get; set; } = new();
}
