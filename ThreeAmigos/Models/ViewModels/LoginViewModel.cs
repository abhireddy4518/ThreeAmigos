using System.ComponentModel.DataAnnotations;

namespace ThreeAmigos.Models.ViewModels;

public class LoginViewModel
{
    public string Name { get; set; }

    public string Email { get; set; }

    public string Password { get; set; }

    public string Role { get; set; }
    public string? DeliveryAddress { get; set; }
    public string PermenentAddress { get; set; }

    public string? PhoneNumber { get; set; }

    public bool IsDeleted { get; set; } = false;

    public bool deleteAccountRequest { get; set; } = false;

    public DateTime createdAt { get; set; } = DateTime.Now;

    public DateTime? updatedAt { get; set; }
}
