using System.ComponentModel.DataAnnotations;

namespace ThreeAmigos.Models.ViewModels;

public class CustomerViewModel
{
    public int UserId { get; set; }

    public string Name { get; set; }

    public string Email { get; set; }

    public string PasswordHash { get; set; }

    public string Role { get; set; }
    public string PermentAddress { get; set; }
    public string? DeliveryAddress { get; set; }

    public string? PhoneNumber { get; set; }

    public decimal? FundsAvailable { get; set; }

    public double usedFund { get; set; }

    public bool IsDeleted { get; set; } = false;

    public bool deleteAccountRequest { get; set; } = false;

    public DateTime createdAt { get; set; } = DateTime.Now;

    public DateTime? updatedAt { get; set; }
}
