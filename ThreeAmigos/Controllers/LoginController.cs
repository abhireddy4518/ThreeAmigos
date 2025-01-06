using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using ThreeAmigos.Models.Entities;
using ThreeAmigos.Models.ViewModels;
using ThreeAmigos.Models;

namespace ThreeAmigos.Controllers
{
    public class LoginController : Controller
    {
        private readonly ApplicationDbContext context;

        public LoginController(ApplicationDbContext context)
        {
            this.context = context;
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (string.IsNullOrEmpty(model.Email))
            {
                ViewBag.ErrorMessage = "Please login through username/password.";
                return View(model);
            }

            ClaimsIdentity identity = null;

            var userDetails = await context.Users.FirstOrDefaultAsync(x => x.Email == model.Email);
            if (userDetails != null)
            {
                if (!string.IsNullOrEmpty(model.Password) && userDetails.PasswordHash == model.Password)
                {
                    identity = CreateClaimsIdentity(model.Email, userDetails.Role.ToLower());
                    HttpContext.Session.SetInt32("UserId", userDetails.UserId);
                    return await SignInAndRedirect(identity);
                }
            }
            ViewBag.ErrorMessage = "Invalid username or password.";
            return View(model);
        }


        private ClaimsIdentity CreateClaimsIdentity(string userName, string role)
        {
            return new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, userName),
                new Claim(ClaimTypes.Role, role)
            }, CookieAuthenticationDefaults.AuthenticationScheme);
        }

        private async Task<IActionResult> SignInAndRedirect(ClaimsIdentity identity, string studentLogInId = null)
        {
            var principal = new ClaimsPrincipal(identity);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
            if (identity.HasClaim(c => c.Type == ClaimTypes.Role && c.Value == "staff"))
            {
                return RedirectToAction("Index", "Product");
            }
            else
            {
                return RedirectToAction("Index", "ForSignedInUsersProduct", new { studentLogInId });
            }
        }


        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Product");
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Create(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var existingResult = await context.Users.FirstOrDefaultAsync(r => r.Email == model.Email);

                if (existingResult != null)
                {
                    TempData["ErrorMessage"] = "User " + existingResult?.Email + " is already created";
                    return View();
                }

                var users = new Customer
                {
                    Name = model.Name,
                    Email = model.Email,
                    PasswordHash = model.Password,
                    Role = model.Role,
                    DeliveryAddress = model.DeliveryAddress,
                    PermentAddress = model.PermenentAddress,
                    PhoneNumber = model.PhoneNumber,
                    FundsAvailable = 100000, 
                    usedFund = 0,
                    createdAt = DateTime.Now
                };

                context.Add(users);
                await context.SaveChangesAsync();

                return RedirectToAction("Login");
            }

            return View(model);
        }
    }
}
