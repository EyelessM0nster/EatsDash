namespace EatsDash.Models;

public class Review
{
    public int Id { get; set; }
    public string AuthorId { get; set; } = string.Empty;
    public string AuthorName { get; set; } = string.Empty;
    public int CourierId { get; set; }
    public string CourierNickname { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string Text { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int LikesCount { get; set; }
    public int DislikesCount { get; set; }
    public ReviewStatus Status { get; set; } = ReviewStatus.Approved;

    public ApplicationUser? Author { get; set; }
    public Courier? Courier { get; set; }
    public ICollection<ReviewReaction> Reactions { get; set; } = new List<ReviewReaction>();
}
