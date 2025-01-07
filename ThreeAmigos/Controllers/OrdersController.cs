using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using ThreeAmigos.Models.Entities;
using ThreeAmigos.Models.ViewModels;
using ThreeAmigos.Services;

namespace ThreeAmigos.Controllers
{
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext context;
        private readonly EmailService emailService;

        public OrdersController(ApplicationDbContext context, EmailService emailService)
        {
            this.context = context;
            this.emailService = emailService;
        }
        public IActionResult Index()
        {
            return View();
        }

        [Authorize(Roles = "staff")]
        public async Task<IActionResult> DispatchList()
        {
            var ordersToDispatch = await context.Orders
                .Where(o => o.OrderStatus == "Pending")
                .Include(o => o.orderItems)
                .Include(o => o.Customer)
                .ToListAsync();

            if(ordersToDispatch.Count <= 0)
            {
                TempData["SuccessMessage"] = "No orders are panding";

            }

            return View(ordersToDispatch);
        }

        [HttpPost]
        public async Task<IActionResult> MarkAsDispatched(int orderId)
        {
            var order = await context.Orders.FindAsync(orderId);

            if (order == null)
            {
                TempData["ErrorMessage"] = "Order not found.";
                return RedirectToAction("DispatchList");
            }

            order.OrderStatus = "Dispatched";
            context.Orders.Update(order);
            await context.SaveChangesAsync();

            order.OrderStatus = "Dispatched";
            order.DispatchDate = DateTime.Now;
            context.Orders.Update(order);

            var products = await context.OrderItems
                .Where(o => o.OrderId == orderId)
                .Include(o => o.Product)
                .ToListAsync();

            var customer = await context.Customers.FindAsync(order.CustomerId);

            string emailBody = $"<h3>Order Status is Updated!</h3>" +
                    $"<p>Your order status with Order ID: {order.OrderId} has been updated successfully.</p>" +
                    $"<p>Total Amount: ${order.TotalPrice}</p>" +
                    $"<p>Order Date: {order.OrderDate}</p>" +
                    $"<p>Order Status: {order.OrderStatus}</p>" +
                    "<h4>Ordered Products:</h4>" +
                    "<table style='border-collapse: collapse; width: 100%; border: 1px solid #ddd;'> " +
                    "<thead>" +
                    "   <tr>" +
                    "       <th style='border: 1px solid #ddd; padding: 8px;'>Product Name</th>" +
                    "       <th style='border: 1px solid #ddd; padding: 8px;'>Quantity</th>" +
                    "       <th style='border: 1px solid #ddd; padding: 8px;'>Unit Price</th>" +
                    "       <th style='border: 1px solid #ddd; padding: 8px;'>Subtotal</th>" +
                    "   </tr>" +
                    "</thead>" +
                    "<tbody>";

            foreach (var item in products)
            {
                var product = await context.Products.FirstOrDefaultAsync(p => p.ProductId == item.ProductId);
                var subtotal = item.Quantity * item.UnitPrice;
                emailBody += $"<tr>" +
                             $"<td style='border: 1px solid #ddd; padding: 8px;'>{product?.Name}</td>" +
                             $"<td style='border: 1px solid #ddd; padding: 8px;'>{item.Quantity}</td>" +
                             $"<td style='border: 1px solid #ddd; padding: 8px;'>${item.UnitPrice:F2}</td>" +
                             $"<td style='border: 1px solid #ddd; padding: 8px;'>${subtotal:F2}</td>" +
                             $"</tr>";
            }

            emailBody += "</tbody></table>";

            emailBody += "<p>Best regards,</p>" +
                         "<p>ThreeAmigos Team</p>";

            if (customer != null)
                await emailService.SendEmailAsync(customer.Email, "Order Status Updated - ThreeAmigos", emailBody);


            TempData["SuccessMessage"] = "Order marked as dispatched!";
            return RedirectToAction("DispatchList");
        }

        public async Task<IActionResult> OrderDetails(int orderId)
        {
            var order = await context.Orders
                .Include(o => o.orderItems)
                .ThenInclude(oi => oi.Product) // Include product details
                .FirstOrDefaultAsync(o => o.OrderId == orderId);

            if (order == null)
            {
                TempData["ErrorMessage"] = "Order not found.";
                return RedirectToAction("OrderHistory");
            }

            var orderDetails = new OrderHistoryViewModel
            {
                OrderId = order.OrderId,
                OrderDate = order.OrderDate,
                TotalPrice = order.TotalPrice,
                OrderStatus = order.OrderStatus,
                OrderItems = order.orderItems.Select(oi => new OrderItemViewModel
                {
                    ProductId = oi.Product.ProductId,
                    ImageUrl = oi.Product.ImageUrl,
                    ProductName = oi.Product.Name,
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice
                }).ToList()
            };

            return View(orderDetails);
        }


        public async Task<IActionResult> OrderList()
        {
            var ordersToDispatch = await context.Orders
                .Include(o => o.orderItems)
                .Include(o => o.Customer)
                .ToListAsync();

            return View(ordersToDispatch);
        }

    }
}
