namespace EatsDash.Helpers;

public static class AvatarHelper
{
    private static readonly string[] ToneClasses =
    [
        "avatar-tone-purple-1",
        "avatar-tone-purple-2",
        "avatar-tone-purple-3",
        "avatar-tone-gold-1",
        "avatar-tone-gold-2",
        "avatar-tone-violet-1"
    ];

    public static string GetToneClass(string key)
    {
        if (string.IsNullOrWhiteSpace(key)) return ToneClasses[0];
        var hash = key.Trim().GetHashCode(StringComparison.Ordinal);
        var index = Math.Abs(hash) % ToneClasses.Length;
        return ToneClasses[index];
    }
}
