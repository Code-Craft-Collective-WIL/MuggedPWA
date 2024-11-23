using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MuggedShop.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MuggedShop.Controllers
{
    public class ProductController : Controller
    {
        // Context to interact with the database (Troeslen & Japikse, 2021)
        private readonly MuggedContext _context = new MuggedContext();

        // GET: Displays the view for adding a new product (Troeslen & Japikse, 2021)
        public IActionResult AddProduct()
        {
            // Prevent the page from being cached(Hewlett, 2015)
            PreventPageCaching();

            // Check if admin email is in session; redirect to login if not(Shahzad, 2019)
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("AdminEmail")))
            {
                return RedirectToAction("Login", "Login");
            }

            // Populate ViewBag with a list of categories for a dropdown in the view (Troeslen & Japikse, 2021)
            ViewBag.CategoryId = GetCategorySelectList();
            return View();
        }

        // POST: Action to handle the form submission for adding a new product (Troeslen & Japikse, 2021)
        [HttpPost]
        public async Task<IActionResult> AddProduct(Product product, IFormFile file)
        {
            // Check if an image file was provided (Troeslen & Japikse, 2021)
            if (file == null || file.Length == 0)
            {
                ViewBag.Error = "An image file is required.";
                ViewBag.CategoryId = GetCategorySelectList();
                return View(product);
            }

            // Validate that the file has an allowed extension (Troeslen & Japikse, 2021)
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var extension = Path.GetExtension(file.FileName).ToLower();

            if (!allowedExtensions.Contains(extension))
            {
                ViewBag.Error = "Only image files (.jpg, .jpeg, .png, .gif) are allowed.";
                ViewBag.CategoryId = GetCategorySelectList();
                return View(product);
            }

            // Generate a unique filename for the image (Troeslen & Japikse, 2021)
            var fileName = Path.GetRandomFileName() + extension;
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Images", fileName);

            // Save the file to the specified path (Troeslen & Japikse, 2021)
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            // Set the ImageUrl property on the product to the saved file's path (Troeslen & Japikse, 2021)
            product.ImageUrl = $"/Images/{fileName}";

            try
            {
                // Add the product to the database and save changes (Troeslen & Japikse, 2021)
                _context.Products.Add(product);
                await _context.SaveChangesAsync();
                return RedirectToAction("ViewProduct");
            }
            catch
            {
                // If saving fails, show an error message (Troeslen & Japikse, 2021)
                ViewBag.Error = "Error adding product. It may already be in the database.";
                ViewBag.CategoryId = GetCategorySelectList();
                return View(product);
            }
        }

        // GET: Action to display a list of products, optionally filtered by a search term (Troeslen & Japikse, 2021)
        public IActionResult ViewProduct(string searchTerm)
        {
            // Prevent the page from being cached(Hewlett, 2015)
            PreventPageCaching();

            // Check if admin email is in session; redirect to login if not(Shahzad, 2019)
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("AdminEmail")))
            {
                return RedirectToAction("Login", "Login");
            }

            var products = _context.Products.AsQueryable();

            // Filter products if a search term is provided (Troeslen & Japikse, 2021)
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                products = products.Where(p => p.ProductName.Contains(searchTerm));
            }

            return View(products.ToList());
        }

        // GET: Action to display details for a specific product (Troeslen & Japikse, 2021)
        public IActionResult DetailsProduct(int id)
        {
            // Prevent the page from being cached(Hewlett, 2015)
            PreventPageCaching();

            // Check if admin email is in session; redirect to login if not(Shahzad, 2019)
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("AdminEmail")))
            {
                return RedirectToAction("Login", "Login");
            }

            var product = _context.Products.Find(id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        // GET: Action to display the EditProduct view with the product's current data (Troeslen & Japikse, 2021)
        public IActionResult EditProduct(int id)
        {
            // Prevent the page from being cached(Hewlett, 2015)
            PreventPageCaching();

            // Check if admin email is in session; redirect to login if not(Shahzad, 2019)
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("AdminEmail")))
            {
                return RedirectToAction("Login", "Login");
            }

            var product = _context.Products.Find(id);
            if (product == null)
            {
                return NotFound();
            }

            // Populate ViewBag with categories for a dropdown in the view (Troeslen & Japikse, 2021)
            ViewBag.CategoryId = GetCategorySelectList();
            return View(product);
        }

        // POST: Action to handle the form submission for editing a product (Troeslen & Japikse, 2021)
        [HttpPost]
        public async Task<IActionResult> EditProduct(Product product)
        {
            // Find the existing product in the database (Troeslen & Japikse, 2021)
            var existingProduct = await _context.Products.FindAsync(product.ProductId);
            if (existingProduct == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                // Update properties of the existing product (Troeslen & Japikse, 2021)
                existingProduct.CategoryId = product.CategoryId;
                existingProduct.ProductName = product.ProductName;
                existingProduct.Price = product.Price;
                existingProduct.Description = product.Description;
                existingProduct.StockCount = product.StockCount;

                // Check if a new image is provided in the form submission (Troeslen & Japikse, 2021)
                var file = Request.Form.Files.FirstOrDefault();
                if (file != null && file.Length > 0)
                {
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                    var extension = Path.GetExtension(file.FileName).ToLower();

                    if (allowedExtensions.Contains(extension))
                    {
                        // Delete the old image if it exists (Troeslen & Japikse, 2021)
                        if (!string.IsNullOrEmpty(existingProduct.ImageUrl))
                        {
                            var oldImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", existingProduct.ImageUrl.TrimStart('~', '/'));
                            if (System.IO.File.Exists(oldImagePath))
                            {
                                System.IO.File.Delete(oldImagePath);
                            }
                        }

                        // Save the new image and update ImageUrl (Troeslen & Japikse, 2021)
                        var fileName = Path.GetRandomFileName() + extension;
                        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Images", fileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(fileStream);
                        }

                        existingProduct.ImageUrl = $"/Images/{fileName}";
                    }
                }

                // Update the product in the database and save changes (Troeslen & Japikse, 2021)
                _context.Products.Update(existingProduct);
                await _context.SaveChangesAsync();

                return RedirectToAction("ViewProduct");
            }

            ViewBag.CategoryId = GetCategorySelectList();
            return View(existingProduct);
        }

        // Action to display the DeleteProduct confirmation view (Troeslen & Japikse, 2021)
        public IActionResult DeleteProduct(int id)
        {
            // Prevent the page from being cached(Hewlett, 2015)
            PreventPageCaching();

            // Check if admin email is in session; redirect to login if not(Shahzad, 2019)
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("AdminEmail")))
            {
                return RedirectToAction("Login", "Login");
            }

            var product = _context.Products.Find(id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        // Action to handle the deletion of a product (Troeslen & Japikse, 2021)
        [HttpPost, ActionName("DeleteProduct")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                // Delete the image file if it exists (Troeslen & Japikse, 2021)
                if (!string.IsNullOrEmpty(product.ImageUrl))
                {
                    var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", product.ImageUrl.TrimStart('~', '/'));

                    // Log for debugging (Troeslen & Japikse, 2021)
                    Console.WriteLine($"Attempting to delete image at: {imagePath}");

                    if (System.IO.File.Exists(imagePath))
                    {
                        System.IO.File.Delete(imagePath);
                        Console.WriteLine("Image deleted successfully.");
                    }
                    else
                    {
                        Console.WriteLine($"Image file not found: {imagePath}");
                    }
                }

                // Remove the product from the database and save changes (Troeslen & Japikse, 2021)
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("ViewProduct");
        }

        // Helper method to create a list of categories for a dropdown (Troeslen & Japikse, 2021)
        private List<SelectListItem> GetCategorySelectList()
        {
            return _context.Categories.Select(c => new SelectListItem
            {
                Value = c.CategoryId.ToString(),
                Text = c.CategoryName
            }).ToList();
        }

        // Helper method to prevent page caching(Hewlett, 2015)
        private void PreventPageCaching()
        {
            Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
            Response.Headers["Pragma"] = "no-cache";
            Response.Headers["Expires"] = "0";
        }
    }
}
