using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models.DTO;

public class ClientDTO
{
    
    [Required]
    [MaxLength(120)]
    public string FirstName { get; set; } = null!;
    [Required]
    [MaxLength(120)]
    public string LastName { get; set; } = null!;
    [Required]
    [MaxLength(120)]
    [EmailAddress]
    public string Email { get; set; } = null!;
    [Required]
    [MaxLength(12)]
    [Phone]
    public string Telephone { get; set; } = null!;
    [Required]
    [StringLength(11, MinimumLength = 11)]
    public string Pesel { get; set; } = null!;
    [DataType(DataType.Currency)]
    public DateTime? PaymentDate { get; set; }
}