using System.ComponentModel.DataAnnotations;

namespace ThreeAmigos.Models;

public class Order
{
    [Key]
    public int OrderId { get; set; }

    public int CustomerId { get; set; }
    public Customer Customer { get; set; }

    public DateTime OrderDate { get; set; } = DateTime.Now;

    [Required]
    public string OrderStatus { get; set; } // "Pending", "Dispatched", "Delivered"

    public decimal TotalPrice { get; set; }

    public DateTime? DispatchDate { get; set; }

    public string DeliveryAddress { get; set; }

    public String PhoneNumber { get; set; }

    public ICollection<OrderItem> orderItems { get; set; }
}
