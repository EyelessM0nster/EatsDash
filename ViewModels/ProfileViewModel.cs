using System.ComponentModel.DataAnnotations;

namespace EatsDash.ViewModels;

public class ProfileViewModel
{
    public string DisplayName { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string AuthorInitial { get; set; } = "?";
    public string AvatarToneClass { get; set; } = "avatar-tone-purple-1";
    public int MyReviewsCount { get; set; }
    public bool CompactReviews { get; set; }
    public string? SuccessMessage { get; set; }
    public string? ErrorMessage { get; set; }
    public bool IsModerator { get; set; }
}

public class UpdateProfileViewModel
{
    [Required(ErrorMessage = "Введите никнейм")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "Никнейм от 2 до 50 символов")]
    [Display(Name = "Никнейм")]
    public string DisplayName { get; set; } = string.Empty;
}

public class UpdateSettingsViewModel
{
    [Display(Name = "Компактный список отзывов")]
    public bool CompactReviews { get; set; }
}

public class ChangePasswordViewModel
{
    [Required(ErrorMessage = "Введите текущий пароль")]
    [DataType(DataType.Password)]
    [Display(Name = "Текущий пароль")]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Введите новый пароль")]
    [StringLength(100, MinimumLength = 4)]
    [DataType(DataType.Password)]
    [Display(Name = "Новый пароль")]
    public string NewPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Подтвердите пароль")]
    [DataType(DataType.Password)]
    [Compare(nameof(NewPassword), ErrorMessage = "Пароли не совпадают")]
    [Display(Name = "Подтвердите пароль")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
