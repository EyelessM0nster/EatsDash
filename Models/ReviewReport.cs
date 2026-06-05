namespace EatsDash.Models;

public class ReviewReport
{
    public int Id { get; set; }
    public int ReviewId { get; set; }
    public string ReporterId { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Review? Review { get; set; }
    public ApplicationUser? Reporter { get; set; }
}
