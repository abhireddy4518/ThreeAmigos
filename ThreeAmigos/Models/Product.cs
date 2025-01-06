using System.ComponentModel.DataAnnotations;

namespace ThreeAmigos.Models;

public class Product
{
    [Key]
    public int ProductId { get; set; }

    public int AddedProductId { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    [Required]
    public string? Category { get; set; }

    [Required]
    public decimal Price { get; set; }

    public int ProductQty { get; set; }

    public string? ImageUrl { get; set; }
    public string? ThumbnailImageUrl { get; set; }

    public ICollection<ProductForDisplayCustomers> ProductForDisplayCustomers { get; set; }
    public ICollection<OrderItem> orderItems { get; set; }
}
