using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using ThreeAmigos.Models.Entities;
using ThreeAmigos.Models.ViewModels;

namespace ThreeAmigos.Controllers
{
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext context;

        public OrdersController(ApplicationDbContext context)
        {
            this.context = context;
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
