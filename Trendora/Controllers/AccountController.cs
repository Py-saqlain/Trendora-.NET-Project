using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Trendora.Models;
using Trendora.Models.Interfaces;
using Trendora.Models.Repositories;

namespace Trendora.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly IOrderRepository _orderRepository;

        public AccountController(ICustomerRepository customerRepository , IOrderRepository orderRepository)
        {
            _customerRepository = customerRepository;
            _orderRepository = orderRepository;
        }

        public IActionResult Index()
        {
            
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userEmail))
            {
                return RedirectToAction("Login", "Account");
            }


            var customer = _customerRepository.GetCustomer(userEmail);
            var customerOrders = _orderRepository.GetCustomerOrders(userId);
            if (customer == null)
            {
                return NotFound();
            }
            ViewBag.CustomerOrders = customerOrders;
            return View(customer);
        }

       

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateProfile(Customer customer)
        {
            if (ModelState.IsValid)
            {
                try
                {
                  
                    var existingCustomer = _customerRepository.GetCustomerById(customer.Id);
                    if (existingCustomer != null)
                    {
                        
                        if (string.IsNullOrEmpty(customer.ProfileImgPath))
                        {
                            customer.ProfileImgPath = existingCustomer.ProfileImgPath;
                        }

                   
                        customer.UserName = existingCustomer.UserName;
                        customer.NormalizedUserName = existingCustomer.NormalizedUserName;
                        customer.NormalizedEmail = existingCustomer.NormalizedEmail;
                        customer.PasswordHash = existingCustomer.PasswordHash;
                    }

                    _customerRepository.UpdateCustomer(customer);
                    TempData["SuccessMessage"] = "Profile updated successfully!";
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Error updating profile: " + ex.Message;
                }
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfileImage(IFormFile profileImage)
        {
            if (profileImage != null && profileImage.Length > 0)
            {
                try
                {
                    var userEmail = User.FindFirstValue(ClaimTypes.Email);

                    if (string.IsNullOrEmpty(userEmail))
                    {
                        TempData["ErrorMessage"] = "User not found!";
                        return RedirectToAction("Index");
                    }

                    var customer = _customerRepository.GetCustomer(userEmail);

                    if (customer == null)
                    {
                        TempData["ErrorMessage"] = "Customer not found!";
                        return RedirectToAction("Index");
                    }

               
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                    var fileExtension = Path.GetExtension(profileImage.FileName).ToLower();

                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        TempData["ErrorMessage"] = "Only image files (JPG, JPEG, PNG, GIF) are allowed!";
                        return RedirectToAction("Index");
                    }

                 
                    if (profileImage.Length > 5 * 1024 * 1024)
                    {
                        TempData["ErrorMessage"] = "File size must be less than 5MB!";
                        return RedirectToAction("Index");
                    }

               
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "profiles");

                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    var uniqueFileName = $"{customer.Id}_{Guid.NewGuid()}{fileExtension}";
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await profileImage.CopyToAsync(fileStream);
                    }

               
                    customer.ProfileImgPath = $"/images/profiles/{uniqueFileName}";
                    _customerRepository.UpdateCustomer(customer);

                    TempData["SuccessMessage"] = "Profile image updated successfully!";
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"Error updating profile image: {ex.Message}";
                }
            }
            else
            {
                TempData["ErrorMessage"] = "Please select an image file!";
            }

            return RedirectToAction("Index");
        }
    }
}