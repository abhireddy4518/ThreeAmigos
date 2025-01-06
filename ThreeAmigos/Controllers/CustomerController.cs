using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ThreeAmigos.Models.Entities;
using ThreeAmigos.Models.ViewModels;

namespace ThreeAmigos.Controllers
{
    public class CustomerController : Controller
    {
        private readonly ApplicationDbContext context;

        public CustomerController(ApplicationDbContext context)
        {
            this.context = context;
        }

        [Authorize(Roles = "customer,staff")]
        public async Task<IActionResult> Edit(int? userId)
        {
            if (userId == null)
            {
                return NotFound();
            }

            var room = await context.Customers.FindAsync(userId);
            if (room == null)
            {
                return NotFound();
            }
            var roomVM = new CustomerViewModel()
            {
                UserId = room.UserId,
                Email = room.Email,
                Name = room.Name,
                PasswordHash = room.PasswordHash,
                Role = room.Role,
                PermentAddress = room.PermentAddress,
                DeliveryAddress = room.DeliveryAddress,
                PhoneNumber = room.PhoneNumber,
                FundsAvailable = room.FundsAvailable,
                usedFund = room.usedFund,
                createdAt = room.createdAt,
            };

            return View(roomVM);
        }

        [Authorize(Roles = "customer,staff")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CustomerViewModel roomVM)
        {
            var checkRoom = await context.Customers.FindAsync(roomVM.UserId);
            if (checkRoom == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    checkRoom.Name = roomVM.Name;
                    checkRoom.PasswordHash = roomVM.PasswordHash;
                    checkRoom.PhoneNumber = roomVM.PhoneNumber;
                    checkRoom.PermentAddress = roomVM.PermentAddress;
                    checkRoom.DeliveryAddress = roomVM.DeliveryAddress;
                    checkRoom.updatedAt = DateTime.Now;


                    context.Update(checkRoom);
                    await context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {

                    throw new Exception("Something went wrong");
                }
                TempData["SuccessMessage"] = "Profile is updated!!";
                return RedirectToAction("Index", "ForSignedInUsersProduct");

            }

            return View(roomVM);
        }

        [Authorize(Roles = "customer,staff")]
        public async Task<IActionResult> Delete(int userId)
        {
            if (userId <= 0)
            {
                return NotFound();
            }

            var user = await context.Customers.FirstOrDefaultAsync(p => p.UserId == userId);
            if (user == null)
            {
                return NotFound();
            }

            user.deleteAccountRequest = true;
            context.Customers.Update(user);

            await context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Your account will removed within 15 working days!";
            return RedirectToAction("Edit", new { userId = user.UserId });

        }

        [Authorize(Roles = "staff")]
        public async Task<IActionResult> Index()
        {
            var customerDetails = await context.Customers.Where(x => x.Role=="customer").ToListAsync();
            if(customerDetails.Count > 0)
            {
                var customerViewModels = customerDetails.Select(c => new CustomerViewModel
                {
                    UserId = c.UserId,
                    Name = c.Name,
                    Email = c.Email,
                    PasswordHash = c.PasswordHash,
                    Role = c.Role,
                    PermentAddress = c.PermentAddress,
                    DeliveryAddress = c.DeliveryAddress,
                    PhoneNumber = c.PhoneNumber,
                    FundsAvailable = c.FundsAvailable,
                    usedFund = c.usedFund
                }).ToList();
                return View(customerViewModels);
            }

            //TempData["ErrorMessage"] = "Your account will removed within 15 working days!";
            TempData["ErrorMessage"] = "There is no customers in our web-site";
            return View(new List<CustomerViewModel>());
        }

        [Authorize(Roles = "staff")]
        public async Task<IActionResult> DeleteCustomerAccountRequests()
        {
            var list = await context.Customers.Where(x => x.deleteAccountRequest == true).ToListAsync();
            
            if(list.Count > 0)
            {
                return View(list);
            }

            TempData["ErrorMessage"] = "No customer has request for delete the account!!";
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "staff")]
        public async Task<IActionResult> DeleteCustomerAccount(int customerId)
        {
            var customer = await context.Customers.FindAsync(customerId);
            if(customer == null)
            {
                TempData["ErrorMessage"] = "Customer is not found";
                return RedirectToAction("DeleteCustomerAccountRequests");
            }

            
            context.Customers.Remove(customer);
            await context.SaveChangesAsync();

            TempData["SuccessMessage"] = "User Deleted Successfully!";
            return RedirectToAction("Index");
        }
    }

}