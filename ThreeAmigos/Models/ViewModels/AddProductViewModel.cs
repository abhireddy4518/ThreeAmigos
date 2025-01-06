using System.ComponentModel.DataAnnotations;

namespace ThreeAmigos.Models.ViewModels;

public class AddProductViewModel
{
    public int AddedProductId { get; set; }

    public string Name { get; set; }

    public string? Description { get; set; }

    public string? Category { get; set; }

    public decimal Price { get; set; }

    public int ProductQty { get; set; }

    public string? ImageUrl { get; set; }
    public string? ThumbnailImageUrl { get; set; }
}
