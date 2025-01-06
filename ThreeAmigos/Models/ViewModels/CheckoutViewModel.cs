namespace ThreeAmigos.Models.ViewModels;

public class CheckoutViewModel
{
    public List<CartItemViewModel> CartItems { get; set; }
    public decimal TotalAmount { get; set; }
    public string DeliveryAddress { get; set; }
    public string PhoneNumber { get; set; }
}
