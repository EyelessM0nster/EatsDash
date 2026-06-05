namespace EatsDash.ViewModels;

public enum HelpAudience
{
    Guest,
    User,
    Moderator
}

public class HelpViewModel
{
    public HelpAudience Audience { get; set; }
}
