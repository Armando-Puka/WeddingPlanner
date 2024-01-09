#pragma warning disable CS8618
using System.ComponentModel.DataAnnotations;
namespace WeddingPlanner.Models;

public class Wedding {
    [Key]
    public int WeddingId { get; set; }

    [Required]
    [MinLength(3)]
    public string WedderOne { get; set; }

    [Required]
    [MinLength(3)]
    public string WedderTwo { get; set; }

    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "Date Of Wedding")]
    [FutureDate]
    public DateTime WeddingDate { get; set; }

    [Required]
    public string WeddingAddress { get; set; }

    public int? UserId { get; set; }
    public User? Creator { get; set; }
    public List<Invitation>? Invitations { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}

public class FutureDateAttribute : ValidationAttribute
{
    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        if (value is DateTime dateValue)
        {
            if (dateValue.Date < DateTime.Now.Date)
            {
                return new ValidationResult("The date must be in the future.");
            }
        }

        return ValidationResult.Success;
    }
}