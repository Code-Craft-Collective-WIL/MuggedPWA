using Microsoft.AspNetCore.Mvc;
using MuggedShop.Models;

namespace Mugged.Controllers
{
    public class CategoryController : Controller
    {
        // Context to interact with the database (Troeslen & Japikse, 2021)
        private readonly MuggedContext _context = new MuggedContext();

        // GET: Displays the view for adding a new category (Troeslen & Japikse, 2021)
        public IActionResult AddCategory()
        {
            // Prevent the page from being cached(Hewlett, 2015)
            PreventPageCaching();

            // Check if admin email is in session; redirect to login if not(Shahzad, 2019)
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("AdminEmail")))
            {
                return RedirectToAction("Login", "Login");
            }

            return View();
        }

        // POST: Handles the form submission for adding a new category (Troeslen & Japikse, 2021)
        [HttpPost]
        public IActionResult AddCategory(Category category)
        {
            try
            {
                // Retrieve the admin email from the session (Shahzad, 2019)
                var adminEmail = HttpContext.Session.GetString("AdminEmail");

                // Validate that the category name is provided (Troeslen & Japikse, 2021)
                if (string.IsNullOrWhiteSpace(category.CategoryName))
                {
                    ViewBag.Error = "Please enter all fields.";
                    return View();
                }

                // Check if the admin email is available (Troeslen & Japikse, 2021)
                if (string.IsNullOrEmpty(adminEmail))
                {
                    ViewBag.Error = "Admin is not logged in.";
                    return View();
                }

                // Normalize the category name to upper case for case-insensitive comparison (Troeslen & Japikse, 2021)
                var normalizedCategoryName = category.CategoryName.Trim().ToUpper();

                // Check if the category already exists in a case-insensitive manner (Troeslen & Japikse, 2021)
                bool categoryExists = _context.Categories
                    .Any(c => c.CategoryName.Trim().ToUpper() == normalizedCategoryName);

                if (categoryExists)
                {
                    ViewBag.Error = "Category already exists in the database.";
                    return View();
                }

                // Create a new category (Troeslen & Japikse, 2021)
                Category newCategory = new Category()
                {
                    CategoryName = category.CategoryName,
                    Description = category.Description,
                };

                // Add the new category to the database and save changes (Troeslen & Japikse, 2021)
                _context.Categories.Add(newCategory);
                _context.SaveChanges();

                return RedirectToAction("ViewCategory");
            }
            catch (Exception)
            {
                // Handle any exceptions that occur during the add operation (Troeslen & Japikse, 2021)
                ViewBag.Error = "An error occurred while adding the category.";
                return View();
            }
        }

        // GET: Displays a list of all categories (Troeslen & Japikse, 2021)
        public IActionResult ViewCategory(string searchTerm)
        {
            // Prevent the page from being cached(Hewlett, 2015)
            PreventPageCaching();

            // Check if admin email is in session; redirect to login if not(Shahzad, 2019)
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("AdminEmail")))
            {
                return RedirectToAction("Login", "Login");
            }

            var categories = _context.Categories.AsQueryable();

            // Check if a search term has been provided
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                // Filter categories by name
                categories = categories.Where(c => c.CategoryName.Contains(searchTerm));
            }

            return View(categories.ToList());
        }

        // GET: Displays the details of a specific category (Troeslen & Japikse, 2021)
        public IActionResult DetailsCategory(int id)
        {
            // Prevent the page from being cached(Hewlett, 2015)
            PreventPageCaching();

            // Check if admin email is in session; redirect to login if not(Shahzad, 2019)
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("AdminEmail")))
            {
                return RedirectToAction("Login", "Login");
            }

            // Validate the category ID (Troeslen & Japikse, 2021)
            if (id <= 0)
            {
                return NotFound();
            }

            // Retrieve the category with the given ID (Troeslen & Japikse, 2021)
            Category category = _context.Categories.SingleOrDefault(e => e.CategoryId == id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // GET: Displays the edit view for a specific category (Troeslen & Japikse, 2021)
        public IActionResult EditCategory(int id)
        {
            // Prevent the page from being cached(Hewlett, 2015)
            PreventPageCaching();

            // Check if admin email is in session; redirect to login if not(Shahzad, 2019)
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("AdminEmail")))
            {
                return RedirectToAction("Login", "Login");
            }

            // Validate the category ID (Troeslen & Japikse, 2021)
            if (id <= 0)
            {
                return BadRequest("Invalid category ID.");
            }

            // Retrieve the category with the given ID (Troeslen & Japikse, 2021)
            var category = _context.Categories.FirstOrDefault(c => c.CategoryId == id);

            // Return an error if the category is not found (Troeslen & Japikse, 2021)
            if (category == null)
            {
                return NotFound("Category not found.");
            }

            return View(category);
        }

        // POST: Handles the form submission for editing a category (Troeslen & Japikse, 2021)
        [HttpPost]
        public IActionResult EditCategory(Category category)
        {
            // Validate the model state (Troeslen & Japikse, 2021)
            if (!ModelState.IsValid)
            {
                return View(category);
            }

            // Normalize the updated category name for case-insensitive comparison (Troeslen & Japikse, 2021)
            var normalizedCategoryName = category.CategoryName.Trim().ToUpper();

            // Check if another category with the same name (case-insensitive) exists (Troeslen & Japikse, 2021)
            bool categoryExists = _context.Categories
                .Any(c => c.CategoryId != category.CategoryId &&
                          c.CategoryName.Trim().ToUpper() == normalizedCategoryName);

            if (categoryExists)
            {
                ViewBag.Error = "A category with this name already exists.";
                return View(category);
            }

            // Update the category in the database (Troeslen & Japikse, 2021)
            _context.Categories.Update(category);
            _context.SaveChanges();

            return RedirectToAction("ViewCategory", "Category");
        }

        // GET: Displays the form to confirm deletion of a category (Troeslen & Japikse, 2021)
        public IActionResult DeleteCategory(int id)
        {
            // Prevent the page from being cached(Hewlett, 2015)
            PreventPageCaching();

            // Check if admin email is in session; redirect to login if not(Shahzad, 2019)
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("AdminEmail")))
            {
                return RedirectToAction("Login", "Login");
            }

            // Validate the category ID (Troeslen & Japikse, 2021)
            if (id <= 0)
            {
                return NotFound();
            }

            // Retrieve the category with the specified ID (Troeslen & Japikse, 2021)
            Category cat = _context.Categories.SingleOrDefault(e => e.CategoryId == id);
            if (cat == null)
            {
                return NotFound();
            }

            return View(cat);
        }

        // POST: Handles the deletion of a category (Troeslen & Japikse, 2021)
        [HttpPost]
        public IActionResult DeleteCategory(Category cat)
        {
            // First, retrieve all products associated with the category (Troeslen & Japikse, 2021)
            var products = _context.Products.Where(p => p.CategoryId == cat.CategoryId).ToList();

            // Remove all associated products from the database (Troeslen & Japikse, 2021)
            _context.Products.RemoveRange(products);

            // Then remove the category from the database (Troeslen & Japikse, 2021)
            _context.Categories.Remove(cat);

            // Save changes to the database (Troeslen & Japikse, 2021)
            _context.SaveChanges();

            return RedirectToAction("ViewCategory", "Category");
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
