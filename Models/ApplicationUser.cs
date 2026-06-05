using Microsoft.AspNetCore.Identity;

namespace EatsDash.Models;

public class ApplicationUser : IdentityUser
{
    public string DisplayName { get; set; } = string.Empty;
    public bool CompactReviews { get; set; }
    public bool IsModerator { get; set; }
    public bool IsBlocked { get; set; }
}
