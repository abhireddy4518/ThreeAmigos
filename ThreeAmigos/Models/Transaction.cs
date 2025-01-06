using System.ComponentModel.DataAnnotations;

namespace ThreeAmigos.Models;

public class Transaction
{
    [Key]
    public int TransactionId { get; set; }

    public int CustomerId { get; set; }
    public Customer Customer { get; set; }

    [Required]
    public decimal Amount { get; set; }

    [Required]
    public string TransactionType { get; set; } // "Credit" or "Debit"

    public DateTime TransactionDate { get; set; } = DateTime.Now;

}
