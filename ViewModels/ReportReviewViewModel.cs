using System.ComponentModel.DataAnnotations;

namespace EatsDash.ViewModels;

public class ReportReviewViewModel
{
    [Required]
    public int ReviewId { get; set; }

    [Required(ErrorMessage = "Выберите причину жалобы")]
    public string ReasonKey { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Comment { get; set; }

    [StringLength(200)]
    public string? CustomReason { get; set; }
}

public class EditReviewViewModel
{
    [Required]
    public int Id { get; set; }

    [Required(ErrorMessage = "Введите имя")]
    [StringLength(100)]
    public string AuthorName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Выберите курьера")]
    [Range(1, int.MaxValue)]
    public int CourierId { get; set; }

    [Required]
    [Range(1, 5)]
    public int Rating { get; set; }

    [Required(ErrorMessage = "Напишите отзыв")]
    [StringLength(2000)]
    public string Text { get; set; } = string.Empty;
}
