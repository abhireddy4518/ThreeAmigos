using System.ComponentModel.DataAnnotations;

namespace ThreeAmigos.Models;

public class User
{
    [Key]
    public int UserId { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; }

    [Required, EmailAddress, MaxLength(100)]
    public string Email { get; set; }

    [Required]
    public string PasswordHash { get; set; }

    [Required]
    public string Role { get; set; }
}
