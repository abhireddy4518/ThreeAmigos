using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ThreeAmigos.Models;
using ThreeAmigos.Models.Entities;
using ThreeAmigos.Models.ViewModels;
using ThreeAmigos.Services;

namespace ThreeAmigos.Controllers
{
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext context;
        private readonly ProductService productService;

        public ProductController(ApplicationDbContext context, ProductService productService)
        {
            this.context = context;
            this.productService = productService;
        }


        // GET: ProductController
        public async Task<IActionResult> Index(string searchTerm, int page = 1, int size = 9)
        {
            // Fetch total products count for pagination
            int totalCount = 194; ;

            // Calculate skip count for pagination
            int skip = (page - 1) * size;

            // Get products with search and pagination
            var products = await productService.GetProductsAsync(skip, size, searchTerm);

            // Set ViewBag variables for pagination and search term
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = size;
            ViewBag.TotalCount = totalCount;
            ViewBag.SearchTerm = searchTerm;

            return View(products);
        }


        public async Task<IActionResult> Details(int productId)
        {
            if (productId <= 0)
            {
                return NotFound();
            }

            var productDetais = await productService.GetProductByIdAsync(productId);
            if (productDetais == null)
            {
                return NotFound();
            }

            return View(productDetais);
        }

        [Authorize(Roles = "staff")]
        public async Task<IActionResult> Create(int productId)
        {
            if(productId <= 0)
            {
                return NotFound();
            }

            var productDetais = await productService.GetProductByIdAsync(productId);
            if (productDetais == null)
            {
                return NotFound();
            }

            return View(productDetais);
        }
     

        // POST: ProductController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AddProductViewModel addProductViewModel)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var addProduct = new Product()
                    {
                        AddedProductId = addProductViewModel.AddedProductId,
                        Name = addProductViewModel.Name,
                        ProductQty = addProductViewModel.ProductQty,
                        Description = addProductViewModel.Description,
                        Category = addProductViewModel.Category,
                        Price = addProductViewModel.Price,
                        ImageUrl = addProductViewModel.ImageUrl,
                        ThumbnailImageUrl = addProductViewModel.ThumbnailImageUrl
                    };

                    var existingProduct = await context.Products.FirstOrDefaultAsync(x => x.AddedProductId == addProduct.AddedProductId);
                    if(existingProduct == null)
                    {
                       await context.AddAsync(addProduct);
                       TempData["SuccessMessage"] = "Product added successfully!";
                    }
                    else
                    {
                        existingProduct.ProductQty = existingProduct.ProductQty + addProductViewModel.ProductQty;
                        context.Update(existingProduct);
                        TempData["SuccessMessage"] = "Product updated successfully!";
                    }
                    await context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }
            catch
            {
                TempData["ErrorMessage"] = "An error occurred while saving the product.";
                return View(addProductViewModel);
            }
            return View(addProductViewModel);
        }
    }
}
