using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Mail;
using System.Security.Claims;
using ThreeAmigos.Models;
using ThreeAmigos.Models.Entities;
using ThreeAmigos.Models.ViewModels;
using ThreeAmigos.Services;

namespace ThreeAmigos.Controllers
{
    public class CartController : Controller
    {

        private readonly ApplicationDbContext context;
        private readonly EmailService emailService;

        public CartController(ApplicationDbContext _context, EmailService emailService)
        {
            context = _context;
            this.emailService = emailService;
        }

        [Authorize(Roles = "customer,staff")]
        public ActionResult Index()
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItemViewModel>>("Cart") ?? new List<CartItemViewModel>();
            ViewBag.Total = cart.Sum(item => item.TotalPrice);
            return View(cart);
        }

        [Authorize(Roles = "customer")]
        [HttpPost]
        public IActionResult AddToCart(int productId, int quantity = 1)
        {
            var product = context.Products.FirstOrDefault(p => p.ProductId == productId);

            if (product == null || product.ProductQty < quantity)
            {
                TempData["ErrorMessage"] = "Product is not available.";
                return RedirectToAction("Index", "ForSignedInUsersProduct");
            }

            var cart = HttpContext.Session.GetObjectFromJson<List<CartItemViewModel>>("Cart") ?? new List<CartItemViewModel>();

            var cartItem = cart.FirstOrDefault(c => c.ProductId == productId);

            if (cartItem != null)
            {
                cartItem.Quantity += quantity;
            }
            else
            {
                cart.Add(new CartItemViewModel
                {
                    ProductId = product.ProductId,
                    Name = product.Name,
                    UnitPrice = product.Price * 1.1m,
                    Quantity = quantity
                });
            }

            HttpContext.Session.SetObjectAsJson("Cart", cart);
            TempData["SuccessMessage"] = "Product added to cart successfully!";
            return RedirectToAction("Index", "ForSignedInUsersProduct");
        }

        [Authorize(Roles = "customer")]
        [HttpPost]
        public IActionResult RemoveFromCart(int productId)
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItemViewModel>>("Cart") ?? new List<CartItemViewModel>();

            var itemToRemove = cart.FirstOrDefault(c => c.ProductId == productId);

            if (itemToRemove != null)
            {
                cart.Remove(itemToRemove);
            }

            HttpContext.Session.SetObjectAsJson("Cart", cart);
            return RedirectToAction("Index");
        }
        // GET: CartController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        [Authorize(Roles = "customer")]
        public IActionResult Checkout()
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItemViewModel>>("Cart") ?? new List<CartItemViewModel>();

            var emailAddress = User?.FindFirst(ClaimTypes.Name)?.Value;
            
            if(emailAddress == null)
            {
                TempData["ErrorMessage"] = "Something went wrong...";
                return RedirectToAction("Index", "ForSignedInUsersProduct");
            }
            var getCustomerDetails = context.Customers.FirstOrDefault(r => r.Email == emailAddress);

            if(getCustomerDetails == null)
            {
                TempData["ErrorMessage"] = "UnAuthorized customer.";
                return RedirectToAction("Index", "ForSignedInUsersProduct");
            }

            if (cart.Count == 0)
            {
                TempData["ErrorMessage"] = "Your cart is empty.";
                return RedirectToAction("Index", "ForSignedInUsersProduct");
            }

            var totalAmount = cart.Sum(item => item.Quantity * item.UnitPrice);

            HttpContext.Session.SetObjectAsJson("CheckoutCart", cart);
            HttpContext.Session.SetString("TotalAmount", totalAmount.ToString());

            var model = new CheckoutViewModel
            {
                CartItems = cart,
                TotalAmount = totalAmount,
                DeliveryAddress = User.Identity.IsAuthenticated ? getCustomerDetails.DeliveryAddress : string.Empty,
                PhoneNumber = User.Identity.IsAuthenticated ? getCustomerDetails.PhoneNumber : string.Empty
            };

            return View(model);
        }

        [Authorize(Roles = "customer")]
        [HttpPost]
        public async Task<IActionResult> Checkout(CheckoutViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var emailAddress = User?.FindFirst(ClaimTypes.Name)?.Value;
            var getCustomerDetails = context.Customers.FirstOrDefault(r => r.Email == emailAddress);

            var customerId = getCustomerDetails?.UserId;

            if (customerId == null)
            {
                TempData["ErrorMessage"] = "You must be logged in to checkout.";
                return RedirectToAction("Login", "Account");
            }

            var cart = HttpContext.Session.GetObjectFromJson<List<CartItemViewModel>>("Cart") ?? new List<CartItemViewModel>();

            if (cart.Count == 0)
            {
                TempData["ErrorMessage"] = "Your cart is empty.";
                return RedirectToAction("Index", "ForSignedInUsersProduct");
            }

            var customer = await context.Customers.FirstOrDefaultAsync(c => c.UserId == customerId);

            if (customer == null)
            {
                TempData["ErrorMessage"] = "Customer not found.";
                return RedirectToAction("Login", "Login");
            }

            else
            {
                customer.FundsAvailable = customer.FundsAvailable - model.TotalAmount;
                customer.usedFund += (double)model.TotalAmount;
                context.Customers.Update(customer);
            }

            // Create the order
            var order = new Order
            {
                CustomerId = customer.UserId,
                OrderStatus = "Pending",
                OrderDate = DateTime.Now,
                TotalPrice = model.TotalAmount,
                DeliveryAddress = model.DeliveryAddress,
                PhoneNumber = model.PhoneNumber,
                orderItems = new List<OrderItem>()
            };

            // Add order items
            foreach (var cartItem in cart)
            {
                var orderItem = new OrderItem
                {
                    ProductId = cartItem.ProductId,
                    Quantity = cartItem.Quantity,
                    UnitPrice = cartItem.UnitPrice
                };

                order.orderItems.Add(orderItem);

                // Reduce the product quantity in stock
                var product = await context.Products.FirstOrDefaultAsync(p => p.ProductId == cartItem.ProductId);
                if (product != null)
                {
                    product.ProductQty -= cartItem.Quantity;
                    context.Products.Update(product);
                }
            }

            var transaction = new Transaction()
            {
                CustomerId = customer.UserId,
                Amount = model.TotalAmount,
                TransactionType = "FUNDS",
                TransactionDate = DateTime.Now
            };

            // Save the order
            await context.Orders.AddAsync(order);
            await context.Transactions.AddAsync(transaction);
            await context.SaveChangesAsync();

            string emailBody = $"<h3>Thank you for your order!</h3>" +
                    $"<p>Your order with Order ID: {order.OrderId} has been placed successfully.</p>" +
                    $"<p>Total Amount: ${model.TotalAmount}</p>" +
                    $"<p>Delivery Address: {model.DeliveryAddress}</p>" +
                    $"<p>Order Date: {DateTime.Now}</p>" +
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

            foreach (var item in cart)
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

            await emailService.SendEmailAsync(customer.Email, "Order Confirmation - ThreeAmigos", emailBody);


            // Clear the cart
            HttpContext.Session.SetObjectAsJson("Cart", new List<CartItemViewModel>());

            TempData["SuccessMessage"] = "Order placed successfully!";
            return RedirectToAction("Index", "ForSignedInUsersProduct");
        }

        [Authorize(Roles = "customer,staff")]
        [HttpGet]
        public async Task<IActionResult> OrderHistory(int? customerId)
        {
            var emailAddress = User?.FindFirst(ClaimTypes.Name)?.Value;

            if (emailAddress == null)
            {
                TempData["ErrorMessage"] = "You must be logged in to view your order history.";
                return RedirectToAction("Login", "Account");
            }

            Customer fetchCustomer;

            if (customerId > 0)
            {
                fetchCustomer = await context.Customers.FindAsync(customerId);
            }
            else
            {
                fetchCustomer = await context.Customers
                .FirstOrDefaultAsync(c => c.Email == emailAddress);

            }
            

            if (fetchCustomer == null)
            {
                TempData["ErrorMessage"] = "Customer not found.";
                return RedirectToAction("Login", "Account");
            }

            // Fetch the orders of the logged-in customer
            var orders = await context.Orders
                .Where(o => o.CustomerId == fetchCustomer.UserId)
                .Include(o => o.orderItems)
                .ThenInclude(oi => oi.Product) // Include product details in order items
                .ToListAsync();

            var orderHistory = orders.Select(o => new OrderHistoryViewModel
            {
                OrderId = o.OrderId,
                OrderDate = o.OrderDate,
                TotalPrice = o.TotalPrice,
                OrderStatus = o.OrderStatus,
                OrderItems = o.orderItems.Select(oi => new OrderItemViewModel
                {
                    ProductName = oi.Product.Name,
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice
                }).ToList()
            }).ToList();

            return View(orderHistory);
        }

        [Authorize(Roles = "customer,staff")]
        public async Task<IActionResult> OrderDetails(int orderId)
        {
            var emailAddress = User?.FindFirst(ClaimTypes.Name)?.Value;

            if (emailAddress == null)
            {
                TempData["ErrorMessage"] = "You must be logged in to view order details.";
                return RedirectToAction("Login", "Account");
            }

            var customer = await context.Customers
                .FirstOrDefaultAsync(c => c.Email == emailAddress);

            if (customer == null)
            {
                TempData["ErrorMessage"] = "Customer not found.";
                return RedirectToAction("Login", "Account");
            }

            var order = await context.Orders
                .Include(o => o.orderItems)
                .ThenInclude(oi => oi.Product) // Include product details
                .FirstOrDefaultAsync(o => o.OrderId == orderId || o.CustomerId == customer.UserId);

            if (order == null)
            {
                TempData["ErrorMessage"] = "Order not found.";
                return RedirectToAction("OrderHistory");
            }

            var orderDetails = new OrderHistoryViewModel
            {
                OrderId = order.OrderId,
                CustomerId = order.CustomerId,
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


    }
}
