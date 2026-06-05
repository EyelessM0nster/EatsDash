namespace EatsDash.Models;

public class ReviewReaction
{
    public int Id { get; set; }
    public int ReviewId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public bool IsLike { get; set; }

    public Review? Review { get; set; }
    public ApplicationUser? User { get; set; }
}
