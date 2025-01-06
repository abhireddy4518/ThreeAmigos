namespace ThreeAmigos.Models.ViewModels;

public class CartItemViewModel
{

    public int ProductId { get; set; }
    public string Name { get; set; }
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal TotalPrice => UnitPrice * Quantity;

}
