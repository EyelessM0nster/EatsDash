using System.ComponentModel.DataAnnotations;

namespace EatsDash.ViewModels;

public class LoginViewModel
{
    [Required(ErrorMessage = "Введите имя пользователя")]
    [Display(Name = "Имя пользователя")]
    public string UserName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Введите пароль")]
    [DataType(DataType.Password)]
    [Display(Name = "Пароль")]
    public string Password { get; set; } = string.Empty;
}

public class RegisterViewModel
{
    [Required(ErrorMessage = "Введите имя пользователя")]
    [Display(Name = "Имя пользователя")]
    [StringLength(50, MinimumLength = 2)]
    public string UserName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Введите пароль")]
    [StringLength(100, MinimumLength = 4)]
    [DataType(DataType.Password)]
    [Display(Name = "Пароль")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Подтвердите пароль")]
    [DataType(DataType.Password)]
    [Compare(nameof(Password), ErrorMessage = "Пароли не совпадают")]
    [Display(Name = "Подтвердите пароль")]
    public string ConfirmPassword { get; set; } = string.Empty;
}

public class CreateReviewViewModel
{
    [Required(ErrorMessage = "Введите имя")]
    [StringLength(100)]
    [Display(Name = "Ваше имя")]
    public string AuthorName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Выберите курьера")]
    [Range(1, int.MaxValue, ErrorMessage = "Выберите курьера")]
    [Display(Name = "Курьер")]
    public int CourierId { get; set; }

    [Required]
    [Range(1, 5, ErrorMessage = "Выберите оценку от 1 до 5")]
    [Display(Name = "Оценка")]
    public int Rating { get; set; } = 5;

    [Required(ErrorMessage = "Напишите отзыв")]
    [StringLength(2000, MinimumLength = 1)]
    [Display(Name = "Ваш отзыв")]
    public string Text { get; set; } = string.Empty;
}
