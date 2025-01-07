using System.ComponentModel.DataAnnotations;

namespace ThreeAmigos.Models;

public class Customer : User
{
    public string PermentAddress { get; set; }
    public string? DeliveryAddress { get; set; }

    [Phone]
    public string? PhoneNumber { get; set; }

    public decimal? FundsAvailable { get; set; }

    public double usedFund { get; set; }

    public bool IsDeleted { get; set; } = false;

    public bool deleteAccountRequest { get; set; } = false;

    public DateTime createdAt { get; set; } = DateTime.Now;

    public DateTime? updatedAt { get; set; }

    public ICollection<Order> orders { get; set; }

    public ICollection<Transaction> transactions { get; set; }
}
