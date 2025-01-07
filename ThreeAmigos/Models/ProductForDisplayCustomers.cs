using System.ComponentModel.DataAnnotations;

namespace ThreeAmigos.Models;

public class ProductForDisplayCustomers
{
    [Key]
    public int Id { get; set; }

    public int ProductId { get; set; }
    public Product Product { get; set; }

    [Required]
    public decimal ProductPrice { get; set; }

    [Required]
    public int StockQuantity { get; set; }
}
