using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Drawing.Printing;
using ThreeAmigos.Models.Entities;
using ThreeAmigos.Services;

namespace ThreeAmigos.Controllers
{
    public class ForSignedInUsersProductController : Controller
    {
        private readonly ApplicationDbContext context;

        public ForSignedInUsersProductController(ApplicationDbContext context)
        {
            this.context = context;
        }

        // GET: ForSignedInUsersProductController
        [Authorize(Roles = "customer,staff")]
        public async Task<IActionResult> Index(string searchQuery = "", string priceRange = "", int page = 1, int size = 9)
        {
            var query = context.Products.Where(x => x.ProductQty > 0);

            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                query = query.Where(p =>
                    p.Name.ToLower().Contains(searchQuery.ToLower()) ||
                    p.Category.ToLower().Contains(searchQuery.ToLower()) ||
                    p.Description.ToLower().Contains(searchQuery.ToLower()));
            }

            // Apply price range filter
            if (priceRange == "lowToHigh")
            {
                query = query.OrderBy(p => p.Price);
            }
            else if (priceRange == "highToLow")
            {
                query = query.OrderByDescending(p => p.Price);
            }

            var products = await query.Skip((page - 1) * size).Take(size).ToListAsync();

            if (products.Count == 0)
            {
                ViewBag.NoProductsFound = "No products are available.";
            }

            ViewData["SearchQuery"] = searchQuery;
            ViewData["PriceRange"] = priceRange;
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = size;
            ViewBag.TotalCount = await query.CountAsync();
            return View(products);
        }

    }
}
