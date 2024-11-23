using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MuggedShop.Models;
using MuggedShop.ModelViews;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Globalization;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;
using System.Text;
using System.Security.Cryptography;

namespace MuggedShop.Controllers
{
    public class HomeController : Controller
    {
        // Initialize the database context for accessing the data (Troeslen & Japikse, 2021)
        MuggedContext _context = new MuggedContext();

        // GET: Home page (Troeslen & Japikse, 2021)
        public IActionResult Index()
        {
            // Retrieve the cart items from the session or initialize an empty list (Troeslen & Japikse, 2021)
            var cartItems = HttpContext.Session.GetObject<List<CartItem>>("CartItems") ?? new List<CartItem>();
            ViewBag.CartCount = cartItems.Count;

            // Retrieve products with StockCount > 0 and map to ProductViewModel (Troeslen & Japikse, 2021)
            var availableProducts = _context.Products
                .Where(p => p.StockCount > 0)
                .Select(p => new ProductViewModel
                {
                    ProductName = p.ProductName,
                    ImageURL = p.ImageUrl,
                    Description = p.Description
                })
                .ToList();

            ViewBag.AvailableProducts = availableProducts;

            return View();
        }

        // GET: About Us page (Troeslen & Japikse, 2021)
        public IActionResult AboutUs()
        {
            // Retrieve the cart items from the session (Troeslen & Japikse, 2021)
            var cartItems = HttpContext.Session.GetObject<List<CartItem>>("CartItems") ?? new List<CartItem>();

            // Set the cart count in the ViewBag (Troeslen & Japikse, 2021)
            ViewBag.CartCount = cartItems.Count;
            return View();
        }

        // GET: Shop page with optional category filter (Troeslen & Japikse, 2021)
        public IActionResult Shop(string selectedCategory = null)
        {
            // Fetch categories along with their related products from the database (Troeslen & Japikse, 2021)
            var categories = _context.Categories
                                     .Include(c => c.Products)
                                     .ToList();

            // Calculate the total number of products across all categories (Troeslen & Japikse, 2021)
            int totalProductCount = categories.Sum(c => c.Products.Count());

            // Create view models for categories and their products (Troeslen & Japikse, 2021)
            var categoryViewModels = categories.Select(category => new CategoryViewModel
            {
                CategoryName = category.CategoryName,
                ProductCount = category.Products.Count(),
                Subcategories = category.Products
                                        .GroupBy(p => p.Category.CategoryName)
                                        .Select(subcategoryGroup => new SubcategoryViewModel
                                        {
                                            SubcategoryName = subcategoryGroup.Key,
                                            SubcategoryProductCount = subcategoryGroup.Count(),
                                            Products = subcategoryGroup.ToList()
                                        })
                                        .ToList()
            }).ToList();

            // Filter products by the selected category if specified (Troeslen & Japikse, 2021)
            if (!string.IsNullOrEmpty(selectedCategory))
            {
                foreach (var category in categoryViewModels)
                {
                    foreach (var subcategory in category.Subcategories)
                    {
                        subcategory.Products = subcategory.Products
                            .Where(p => p.Category.CategoryName == selectedCategory)
                            .ToList();
                    }
                }
            }

            // Set ViewBag properties for display in the view (Troeslen & Japikse, 2021)
            ViewBag.TotalProductCount = totalProductCount;
            ViewBag.SelectedCategory = selectedCategory;
            ViewBag.CartCount = GetCartCount();

            // Retrieve the cart items from the session or initialize an empty list (Troeslen & Japikse, 2021)
            var cartItems = HttpContext.Session.GetObject<List<CartItem>>("CartItems") ?? new List<CartItem>();

            // Set the cart count in the ViewBag for display in the view (Troeslen & Japikse, 2021)
            ViewBag.CartCount = cartItems.Count;

            return View(categoryViewModels);
        }

        // Helper method to get the current cart count from the session (Troeslen & Japikse, 2021)
        public int GetCartCount()
        {
            return HttpContext.Session.GetInt32("CartCount") ?? 0;
        }

        // POST: Add a product to the cart (Troeslen & Japikse, 2021)
        [HttpPost]
        public IActionResult AddToCart(int productId, int quantity)
        {
            Debug.WriteLine($"Product ID: {productId}, Quantity: {quantity}");

            // Find the product in the database (Troeslen & Japikse, 2021)
            var product = _context.Products.Find(productId);
            if (product == null)
            {
                return NotFound(); // Return 404 if the product does not exist (Troeslen & Japikse, 2021)
            }

            // Update the cart count in the session (Troeslen & Japikse, 2021)
            int cartCount = GetCartCount();
            HttpContext.Session.SetInt32("CartCount", cartCount + quantity);

            // Retrieve the current cart items from the session (Troeslen & Japikse, 2021)
            var cartItems = HttpContext.Session.GetObject<List<CartItem>>("CartItems") ?? new List<CartItem>();
            var existingCartItem = cartItems.FirstOrDefault(item => item.ProductId == productId);

            // Update the quantity if the product already exists in the cart (Troeslen & Japikse, 2021)
            if (existingCartItem != null)
            {
                existingCartItem.Quantity += quantity;
            }
            else
            {
                // Add new item to the cart (Troeslen & Japikse, 2021)
                cartItems.Add(new CartItem
                {
                    ProductId = product.ProductId,
                    ProductName = product.ProductName,
                    ImageUrl = product.ImageUrl,
                    Price = product.Price,
                    Quantity = quantity
                });
            }

            // Save updated cart items to the session (Troeslen & Japikse, 2021)
            HttpContext.Session.SetObject("CartItems", cartItems);

            // Store feedback in TempData
            TempData["ProductName"] = product.ProductName;
            TempData["CartCount"] = cartCount + quantity;

            return RedirectToAction("Shop", new { selectedCategory = ViewBag.SelectedCategory });
        }

        // GET: Cart page (Troeslen & Japikse, 2021)
        public IActionResult Cart()
        {
            var cartItems = HttpContext.Session.GetObject<List<CartItem>>("CartItems") ?? new List<CartItem>();
            var customImageData = HttpContext.Session.GetObject<byte[]>("CustomImageData");

            // Pass custom image data to the view if it exists (Troeslen & Japikse, 2021)
            ViewBag.CustomImageData = customImageData;

            return View(cartItems);
        }


        // POST: Update quantity of an item in the cart (Troeslen & Japikse, 2021)
        [HttpPost]
        public IActionResult UpdateCart(int productId, int quantity)
        {
            // Retrieve the current cart items from the session (Troeslen & Japikse, 2021)
            var cartItems = HttpContext.Session.GetObject<List<CartItem>>("CartItems") ?? new List<CartItem>();

            // Find the item in the cart (Troeslen & Japikse, 2021)
            var cartItem = cartItems.FirstOrDefault(item => item.ProductId == productId);
            if (cartItem != null)
            {
                // Update the quantity of the item in the cart (Troeslen & Japikse, 2021)
                cartItem.Quantity = quantity;

                // Remove the item if the quantity is zero (Troeslen & Japikse, 2021)
                if (cartItem.Quantity <= 0)
                {
                    cartItems.Remove(cartItem);
                }
            }

            // Update the cart items in the session (Troeslen & Japikse, 2021)
            HttpContext.Session.SetObject("CartItems", cartItems);

            // Update the cart count in the session (Troeslen & Japikse, 2021)
            HttpContext.Session.SetInt32("CartCount", cartItems.Sum(item => item.Quantity));

            // Redirect back to the Cart view (Troeslen & Japikse, 2021)
            return RedirectToAction("Cart");
        }

        // POST: Remove an item from the cart (Troeslen & Japikse, 2021)
        [HttpPost]
        public IActionResult RemoveFromCart(int productId)
        {
            // Retrieve the current cart items from the session (Troeslen & Japikse, 2021)
            var cartItems = HttpContext.Session.GetObject<List<CartItem>>("CartItems") ?? new List<CartItem>();

            // Find the item in the cart
            var cartItem = cartItems.FirstOrDefault(item => item.ProductId == productId);
            if (cartItem != null)
            {
                // Remove the item from the cart (Troeslen & Japikse, 2021)
                cartItems.Remove(cartItem);
            }

            // Update the cart items in the session (Troeslen & Japikse, 2021)
            HttpContext.Session.SetObject("CartItems", cartItems);

            // Update the cart count in the session (Troeslen & Japikse, 2021)
            HttpContext.Session.SetInt32("CartCount", cartItems.Sum(item => item.Quantity));

            // Redirect back to the Cart view (Troeslen & Japikse, 2021)
            return RedirectToAction("Cart");
        }

        // GET: Contact Us page (Troeslen & Japikse, 2021)
        public IActionResult ContactUs()
        {
            // Retrieve the cart items from the session (Troeslen & Japikse, 2021)
            var cartItems = HttpContext.Session.GetObject<List<CartItem>>("CartItems") ?? new List<CartItem>();

            // Set the cart count in the ViewBag (Troeslen & Japikse, 2021)
            ViewBag.CartCount = cartItems.Count;

            return View();
        }

        // GET: Display the Checkout Order page with the cart items (Troeslen & Japikse, 2021)
        public IActionResult CheckoutOrder(string email)
        {
            // Initialize the order model (Troeslen & Japikse, 2021)
            var orderModel = new Order();
            var cartItems = HttpContext.Session.GetObject<List<CartItem>>("CartItems") ?? new List<CartItem>();

            // Log session keys and count of cart items for debugging (Troeslen & Japikse, 2021)
            Console.WriteLine($"Session Keys: {string.Join(", ", HttpContext.Session.Keys)}");
            Console.WriteLine($"Cart Items Count: {cartItems.Count}");

            // Create the ViewModel instance
            var viewModel = new CheckoutViewModel
            {
                Order = orderModel,
                CartItems = cartItems // Ensure cart items are populated (Troeslen & Japikse, 2021)
            };

            // Populate email if provided (Troeslen & Japikse, 2021)
            if (!string.IsNullOrEmpty(email))
            {
                viewModel.Order.Email = email; // Pre-fill email field (Troeslen & Japikse, 2021)
            }

            // Log the ViewModel for debugging (Troeslen & Japikse, 2021)
            Console.WriteLine($"CheckoutViewModel: {JsonConvert.SerializeObject(viewModel)}");

            return View(viewModel); // Pass the ViewModel to the view (Troeslen & Japikse, 2021)
        }

        // POST: Checkout Action (Troeslen & Japikse, 2021)
        [HttpPost]
        public IActionResult CheckoutOrder(Order order)
        {
            var cartItems = HttpContext.Session.GetObject<List<CartItem>>("CartItems") ?? new List<CartItem>();

            if (!cartItems.Any())
            {
                ViewBag.Error = "Your cart is empty. Please add items to the cart.";
                return View(new CheckoutViewModel { Order = order, CartItems = cartItems });
            }

            if (!ModelState.IsValid)
            {
                return View(new CheckoutViewModel { Order = order, CartItems = cartItems });
            }

            try
            {
                var checkout = new Order
                {
                    Email = order.Email,
                    FullName = order.FullName,
                    Address = order.Address,
                    City = order.City,
                    Zip = order.Zip,
                    CardName = HashCardInfo(order.CardName),
                    CardNumber = HashCardInfo(order.CardNumber),
                    ExpMonth = HashCardInfo(order.ExpMonth),
                    ExpYear = HashCardInfo(order.ExpYear),
                    Cvv = HashCardInfo(order.Cvv),
                    CreatedAt = DateTime.Now,
                    ProcessedDate = null,
                    Status ="Pending"
                };

                _context.Orders.Add(checkout);
                _context.SaveChanges();

                foreach (var item in cartItems)
                {
                    if (item.ProductId == 0) // Custom mug (Troeslen & Japikse, 2021)
                    {
                        var customOrder = new CustomOrder
                        {
                            UserEmail = order.Email,
                            CustomText = item.ProductName,
                            Price = item.Price,
                            Quantity = item.Quantity,
                            OrderId = checkout.OrderId,
                            CreatedAt = DateTime.Now,
                            ProcessedDate=null,
                            Status = "Pending"
                        };

                        // Check for the image data in the session (Troeslen & Japikse, 2021)
                        var imageData = HttpContext.Session.GetObject<byte[]>("CustomImageData");
                        if (imageData != null && imageData.Length > 0)
                        {
                            var uniqueFileName = $"{Guid.NewGuid()}_{item.ImageUrl}"; // Generate unique file name (Troeslen & Japikse, 2021)
                            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Images/custom_mugs");
                            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                            // Save the image to the permanent folder (Troeslen & Japikse, 2021)
                            System.IO.File.WriteAllBytes(filePath, imageData);
                            customOrder.CustomImageUrl = $"/Images/custom_mugs/{uniqueFileName}";
                        }
                        else
                        {
                            Console.WriteLine("No image data found for custom order."); // Log if something is wrong (Troeslen & Japikse, 2021)
                        }

                        // Debugging information (Troeslen & Japikse, 2021)
                        Console.WriteLine($"Saving Custom Order: Email: {customOrder.UserEmail}, ImageUrl: {customOrder.CustomImageUrl}, CustomText: {customOrder.CustomText}");

                        _context.CustomOrders.Add(customOrder);
                    }
                    else // Regular product order (Troeslen & Japikse, 2021)
                    {
                        var product = _context.Products.FirstOrDefault(p => p.ProductId == item.ProductId);
                        if (product == null || product.StockCount < item.Quantity)
                        {
                            TempData["CheckoutError"] = "One or more items in your cart are out of stock.";
                            return RedirectToAction("CheckoutOrder");
                        }

                        product.StockCount -= item.Quantity;
                        _context.Products.Update(product);

                        var orderItem = new OrderItem
                        {
                            OrderId = checkout.OrderId,
                            ProductId = item.ProductId,
                            Price = item.Price,
                            Quantity = item.Quantity
                        };
                        _context.OrderItems.Add(orderItem);
                    }
                }

                _context.SaveChanges(); // Save all changes at once (Troeslen & Japikse, 2021)
                HttpContext.Session.Remove("CartItems");
                HttpContext.Session.Remove("CustomImageData"); // Remove the image data after saving (Troeslen & Japikse, 2021)

                TempData["CheckoutSuccess"] = true;
                TempData["RedirectPath"] = Url.Action("UserHome", "User", new { email = order.Email });

                return RedirectToAction("CheckoutOrder");
            }
            catch (Exception ex)
            {
                // Log exception details for debugging
                Console.WriteLine($"Checkout failed: {ex.Message}");
                TempData["CheckoutError"] = "Checkout failed. Please try again.";
                return RedirectToAction("CheckoutOrder");
            }
        }

        // POST: User registration action (Troeslen & Japikse, 2021)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterUser([Bind("FullName,Email,PhoneNumber,Password")] User user, string Password)
        {
            if (ModelState.IsValid)
            {
                // Check if the email already exists in the database (Troeslen & Japikse, 2021)
                var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == user.Email);
                if (existingUser != null)
                {
                    TempData["ErrorMessage"] = "A user with this email already exists.";
                    TempData["ShowModal"] = true; // Show modal on error (Troeslen & Japikse, 2021)
                    return RedirectToAction("Cart", "Home"); // Redirect back to the cart page or home (Troeslen & Japikse, 2021)
                }

                // Hash the password (Troeslen & Japikse, 2021)
                user.PasswordHash = HashPassword(Password);

                // Add the new user to the database (Troeslen & Japikse, 2021)
                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // Set success message and prompt user to login
                TempData["SuccessMessage"] = "Registration successful! Please log in.";
                TempData["ShowLoginModal"] = true; // Show login modal after registration (Troeslen & Japikse, 2021)
                return RedirectToAction("Cart", "Home");
            }

            // Handle model validation errors (Troeslen & Japikse, 2021)
            TempData["ErrorMessage"] = "Invalid registration details.";
            TempData["ShowModal"] = true; // Show modal on error (Troeslen & Japikse, 2021)
            return RedirectToAction("Cart", "Home");
        }


        // POST: User login action (Troeslen & Japikse, 2021)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LoginUser([Bind("Email,PasswordHash")] User user)
        {
            if (ModelState.IsValid)
            {
                // Check if the user exists in the database (Troeslen & Japikse, 2021)
                var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == user.Email);

                if (existingUser != null)
                {
                    // Verify the password (Troeslen & Japikse, 2021)
                    if (VerifyPassword(user.PasswordHash, existingUser.PasswordHash))
                    {
                        HttpContext.Session.SetString("UserEmail", existingUser.Email); // Store user email in session (Troeslen & Japikse, 2021)
                        return RedirectToAction("CheckoutOrder", "Home", new { email = user.Email });
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Invalid password."; // Set error message for invalid password (Troeslen & Japikse, 2021)
                        TempData["ShowModal"] = true; // Indicate to show modal for error (Troeslen & Japikse, 2021)
                        TempData["Email"] = user.Email; // Optional: Preserve entered email (Troeslen & Japikse, 2021)
                        return RedirectToAction("Cart", "Home"); // Redirect back to the cart page (Troeslen & Japikse, 2021)
                    }
                }
                else
                {
                    TempData["ErrorMessage"] = "User not found."; // Set error message for user not found (Troeslen & Japikse, 2021)
                    TempData["ShowModal"] = true;
                    return RedirectToAction("Cart", "Home");
                }
            }

            TempData["ErrorMessage"] = "Invalid data."; // Set error message for invalid data (Troeslen & Japikse, 2021)
            TempData["ShowModal"] = true;
            return RedirectToAction("Cart", "Home");
        }

        // Method to hash a password (note: needs unique salt implementation)(Microsoft, 2022)
        private string HashPassword(string password)
        {
            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password!,
                salt: new byte[0],
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 100000,
                numBytesRequested: 256 / 8));

            return hashed;
        }

        // Method to verify a plain text password against a hashed password (Troeslen & Japikse, 2021)
        private bool VerifyPassword(string plainPassword, string hashedPassword)
        {
            var hashedInputPassword = HashPassword(plainPassword);
            return hashedInputPassword == hashedPassword; // Compare the hashed passwords (Troeslen & Japikse, 2021)
        }


        // Method to hash a card information (note: needs unique salt implementation)(Troeslen & Japikse, 2021)
        public static string HashCardInfo(string input)
        {
            // Create a new instance of the SHA256 class to perform the hashing (Troeslen & Japikse, 2021)
            using (SHA256 sha256 = SHA256.Create())
            {
                // Convert the input string to a byte array using UTF-8 encoding (Troeslen & Japikse, 2021)
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));

                // Initialize a StringBuilder to build the hash string in hexadecimal format (Troeslen & Japikse, 2021)
                StringBuilder builder = new StringBuilder();

                // Loop through each byte in the byte array.
                for (int i = 0; i < bytes.Length; i++)
                {
                    // Convert each byte to a hexadecimal string and append it to the StringBuilder (Troeslen & Japikse, 2021)
                    // The "x2" format specifier ensures that each byte is represented as a two-digit hexadecimal number (Troeslen & Japikse, 2021)
                    builder.Append(bytes[i].ToString("x2"));
                }

                // Return the final hashed string, which is the concatenation of all the hexadecimal representations (Troeslen & Japikse, 2021)
                return builder.ToString();
            }
        }

        // GET: Display the customization form for the mug (Troeslen & Japikse, 2021)
        public IActionResult CustomizeMug()
        {
            // Retrieve cart items from session (if needed for the view) (Troeslen & Japikse, 2021)
            var cartItems = HttpContext.Session.GetObject<List<CartItem>>("CartItems") ?? new List<CartItem>();
            ViewBag.CartCount = cartItems.Count;

            return View(new CustomOrder());
        }

        // POST: Adding custom order to cart (Troeslen & Japikse, 2021)
        [HttpPost]
        public IActionResult AddCustomOrder(CustomOrder customOrder, IFormFile customImageUrl)
        {
            // Validate model state (Troeslen & Japikse, 2021)
            if (!ModelState.IsValid)
            {
                return View("CustomizeMug", customOrder);
            }

            // Ensure custom order details are valid (Troeslen & Japikse, 2021)
            if (string.IsNullOrEmpty(customOrder.CustomText))
            {
                ModelState.AddModelError("CustomText", "Please provide custom text."); // Check for empty text (Troeslen & Japikse, 2021)
                return View("CustomizeMug", customOrder); // Return the view with the error (Troeslen & Japikse, 2021)
            }

            // Handle image upload
            if (customImageUrl != null && customImageUrl.Length > 0)
            {
                // Store the image temporarily in session or in-memory (Troeslen & Japikse, 2021)
                var imageData = new MemoryStream();
                customImageUrl.CopyTo(imageData);
                customOrder.CustomImageUrl = customImageUrl.FileName; // Store the original name for reference (Troeslen & Japikse, 2021)
                HttpContext.Session.SetObject("CustomImageData", imageData.ToArray()); // Save the image data in session (Troeslen & Japikse, 2021)
            }
            else
            {
                ModelState.AddModelError("CustomImageUrl", "Please upload an image."); // Ensure image is uploaded (Troeslen & Japikse, 2021)
                return View("CustomizeMug", customOrder);
            }

            // Retrieve the current cart items from the session (Troeslen & Japikse, 2021)
            var cartItems = HttpContext.Session.GetObject<List<CartItem>>("CartItems") ?? new List<CartItem>();

            // Create a new cart item for the custom order (Troeslen & Japikse, 2021)
            var newCartItem = new CartItem
            {
                ProductId = 0, // Default ID for custom mugs (Troeslen & Japikse, 2021)
                ProductName = customOrder.CustomText, // Use custom text as the name (Troeslen & Japikse, 2021)
                Price = 20.00m, // Set a price for custom mugs (Troeslen & Japikse, 2021)
                Quantity = customOrder.Quantity ?? 0, // Input quantity (Troeslen & Japikse, 2021)
                StockCount = 10, // Set a default stock count (you may want to adjust this) (Troeslen & Japikse, 2021)
                ImageUrl = customOrder.CustomImageUrl // Store the image name for later reference (Troeslen & Japikse, 2021)
            };

            // Add the new cart item to the cart (Troeslen & Japikse, 2021)
            cartItems.Add(newCartItem);

            // Save updated cart items to the session (Troeslen & Japikse, 2021)
            HttpContext.Session.SetObject("CartItems", cartItems);
            HttpContext.Session.SetInt32("CartCount", cartItems.Sum(item => item.Quantity));

            // Set a success message (Troeslen & Japikse, 2021)
            TempData["SuccessMessage"] = "Your custom mug has been added to the cart!";

            return RedirectToAction("CustomizeMug");
        }
    }
}
