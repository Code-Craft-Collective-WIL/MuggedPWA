using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MuggedShop.Models;
using MuggedShop.ModelViews;
using PdfSharp;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System.Drawing;

namespace MuggedShop.Controllers
{
    public class AdminController : Controller
    {
        // Context to interact with the database(Troeslen & Japikse, 2021)
        private readonly MuggedContext _context = new MuggedContext();

        // GET: Registration Page(Troeslen & Japikse, 2021)
        public IActionResult AddAdmin()
        {
            return View();
        }

        // POST: Registration Page(Troeslen & Japikse, 2021)
        [HttpPost]
        public async Task<IActionResult> AddAdmin(AdminUser admin)
        {
            try
            {
                PreventPageCaching();

                // Check if admin email is in session; redirect to login if not(Troeslen & Japikse, 2021)
                if (string.IsNullOrEmpty(HttpContext.Session.GetString("AdminEmail")))
                {
                    return RedirectToAction("Login", "Login");
                }

                // Validate that all fields are filled(Troeslen & Japikse, 2021)
                if (string.IsNullOrEmpty(admin.Email) || string.IsNullOrEmpty(admin.PasswordHash))
                {
                    ViewBag.Error = "Please enter both Email and Password.";
                    return View();
                }

                // Validate if the email is in a valid format(Troeslen & Japikse, 2021)
                if (!IsValidEmail(admin.Email))
                {
                    ViewBag.Error = "Please enter a valid email address.";
                    return View();
                }

                // Check if the email already exists in AdminUsers table(Troeslen & Japikse, 2021)
                var existingAdmin = await _context.AdminUsers.FirstOrDefaultAsync(a => a.Email == admin.Email);
                if (existingAdmin != null)
                {
                    ViewBag.Error = "Admin with this email already exists.";
                    return View(admin);
                }

                // Check if the email already exists in Users table(Troeslen & Japikse, 2021)
                var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == admin.Email);
                if (existingUser != null)
                {
                    ViewBag.Error = "Email belongs to a registered user. Please use a different email.";
                    return View(admin);
                }

                // Validate if the password is at least 6 characters long(Troeslen & Japikse, 2021)
                if (admin.PasswordHash.Length < 6)
                {
                    ViewBag.Error = "Password must be at least 6 characters long.";
                    return View(admin);
                }

                // Hash the password before saving(Troeslen & Japikse, 2021)
                admin.PasswordHash = HashPassword(admin.PasswordHash);

                // Add the admin to the database
                _context.AdminUsers.Add(admin);
                await _context.SaveChangesAsync();

                // Redirect to admin view page after successful registration(Troeslen & Japikse, 2021)
                return RedirectToAction("ViewAdmin");
            }
            catch
            {
                ViewBag.Error = "An error occurred during admin registration. Please try again.";
                return View();
            }
        }

        // GET: Login Page(Troeslen & Japikse, 2021)
        public IActionResult AdminLogin()
        {
            return View();
        }

        // POST: Authenticate Admin (Login)(Troeslen & Japikse, 2021)
        [HttpPost]
        public IActionResult AdminLogin(string email, string password)
        {
            try
            {
                // Find admin by email(Troeslen & Japikse, 2021)
                var admin = _context.AdminUsers.FirstOrDefault(a => a.Email == email);

                // If admin is not found(Troeslen & Japikse, 2021)
                if (admin == null)
                {
                    ViewBag.Error = "Admin not found.";
                    return View();
                }

                // Verify the password(Troeslen & Japikse, 2021)
                bool isValidPassword = VerifyPassword(password, admin.PasswordHash);
                if (!isValidPassword)
                {
                    ViewBag.Error = "Invalid password.";
                    return View();
                }

                // Set session and redirect to Admin home(Troeslen & Japikse, 2021)
                HttpContext.Session.SetString("AdminEmail", admin.Email);
                return RedirectToAction("AdminHome");
            }
            catch
            {
                ViewBag.Error = "An error occurred during login. Please try again.";
                return View();
            }
        }

        // Method to hash the password using PBKDF2(Microsoft, 2022)
        private string HashPassword(string password)
        {
            // Derive a 256-bit subkey (use HMACSHA256 with 100,000 iterations)(Microsoft, 2022)
            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password!,
                salt: new byte[0], // No salt for now, could be improved with salt(Microsoft, 2022)
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 100000,
                numBytesRequested: 256 / 8));

            return hashed;
        }

        // Verify hashed password(Troeslen & Japikse, 2021)
        private bool VerifyPassword(string password, string storedHash)
        {
            string hashedPassword = HashPassword(password);
            return hashedPassword == storedHash;
        }

        // Helper method to validate email format(Troeslen & Japikse, 2021)
        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        // GET: Displays a list of all admin
        public IActionResult ViewAdmin()
        {
            // Prevent the page from being cached(Hewlett, 2015)
            PreventPageCaching();

            // Check if admin email is in session; redirect to login if not(Shahzad, 2019)
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("AdminEmail")))
            {
                return RedirectToAction("Login", "Login");
            }

            var admin = (from Admin in _context.AdminUsers
                         select Admin).ToList();

            return View(admin);
        }

        // Admin Home Page after successful login(Troeslen & Japikse, 2021)
        public IActionResult AdminHome()
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

        // Total Users (Troeslen & Japikse, 2021)
        // This action calculates and returns the total number of users in the system (Troeslen & Japikse, 2021)
        public IActionResult TotalUsers()
        {
            var userCount = _context.Users.Count(); // Get the total count of users from the Users table (Troeslen & Japikse, 2021)
            return Json(userCount); // Return the result as JSON (Troeslen & Japikse, 2021)
        }

        // Total Admin (Troeslen & Japikse, 2021)
        // This action calculates and returns the total number of admin users in the system (Troeslen & Japikse, 2021)
        public IActionResult TotalAdmin()
        {
            var adminCount = _context.AdminUsers.Count(); // Get the total count of admin users from the AdminUsers table (Troeslen & Japikse, 2021)
            return Json(adminCount); // Return the result as JSON (Troeslen & Japikse, 2021)
        }

        // Action method to get the count of pending custom orders (Troeslen & Japikse, 2021)
        // This method queries the database for all custom orders with a "Pending" status (Troeslen & Japikse, 2021)
        // and returns the count as a JSON response (Troeslen & Japikse, 2021)
        public IActionResult PendingCustomOrders()
        {
            // Fetch the count of custom orders with status "Pending" (Troeslen & Japikse, 2021)
            var pendingCustomOrdersCount = _context.CustomOrders.Count(co => co.Status == "Pending");

            // Return the count as a JSON response
            return Json(pendingCustomOrdersCount);
        }

        // Action method to get the count of pending regular orders (Troeslen & Japikse, 2021)
        // This method queries the database for all regular orders with a "Pending" status (Troeslen & Japikse, 2021)
        // and returns the count as a JSON response (Troeslen & Japikse, 2021)
        public IActionResult PendingRegularOrders()
        {
            // Fetch the count of regular orders with status "Pending" (Troeslen & Japikse, 2021)
            var pendingRegularOrdersCount = _context.Orders.Count(o => o.Status == "Pending");

            // Return the count as a JSON response (Troeslen & Japikse, 2021)
            return Json(pendingRegularOrdersCount);
        }


        // Total Sales (Including Custom Orders) (Troeslen & Japikse, 2021)
        // This action calculates and returns the total number of orders and custom orders grouped by each month (Troeslen & Japikse, 2021)
        public IActionResult TotalSales()
        {
            // Group Orders by Month (Troeslen & Japikse, 2021)
            var ordersData = _context.Orders
                .GroupBy(order => order.CreatedAt.Month)
                .Select(group => new
                {
                    Month = group.Key,
                    OrderCount = group.Count()
                });

            // Group Custom Orders by Month (Troeslen & Japikse, 2021)
            var customOrdersData = _context.CustomOrders
                .GroupBy(customOrder => customOrder.CreatedAt.Month)
                .Select(group => new
                {
                    Month = group.Key,
                    CustomOrderCount = group.Count()
                });

            // Combine both datasets (Troeslen & Japikse, 2021)
            var combinedData = ordersData
                .ToList() // Materialize the query to List (Troeslen & Japikse, 2021)
                .GroupJoin(customOrdersData.ToList(),
                    order => order.Month,
                    customOrder => customOrder.Month,
                    (order, customOrders) => new
                    {
                        Month = order.Month,
                        OrderCount = order.OrderCount,
                        CustomOrderCount = customOrders.FirstOrDefault()?.CustomOrderCount ?? 0
                    })
                .OrderBy(data => data.Month)
                .ToList();

            return Json(combinedData);
        }

        // Product Performance (Troeslen & Japikse, 2021)
        // This action calculates the total sales count for each product, ordered by highest sales (Troeslen & Japikse, 2021)
        public IActionResult ProductPerformance()
        {
            var productData = _context.OrderItems
                .Join(
                    _context.Products, // Join the OrderItems with Products table (Troeslen & Japikse, 2021)
                    orderItem => orderItem.ProductId, // Use ProductId to match OrderItems and Products (Troeslen & Japikse, 2021)
                    product => product.ProductId, // Use ProductId to match OrderItems and Products (Troeslen & Japikse, 2021)
                    (orderItem, product) => new { product.ProductName, orderItem.Quantity } // Select the product name and quantity sold (Troeslen & Japikse, 2021)
                )
                .GroupBy(p => p.ProductName) // Group by product name (Troeslen & Japikse, 2021)
                .Select(g => new { Product = g.Key, SalesCount = g.Sum(p => p.Quantity) }) // Sum the quantity to get total sales per product (Troeslen & Japikse, 2021)
                .OrderByDescending(p => p.SalesCount) // Order by sales count (descending) (Troeslen & Japikse, 2021)
                .ToList();

            return Json(productData); // Return the result as JSON (Troeslen & Japikse, 2021)
        }

        // Sales by Category (Troeslen & Japikse, 2021)
        // This action calculates the total quantity of products sold by category, ordered by highest sales (Troeslen & Japikse, 2021)
        public IActionResult SalesByCategory()
        {
            var categoryProductCount = _context.Products
                .Join(
                    _context.OrderItems, // Join Products with OrderItems (Troeslen & Japikse, 2021)
                    product => product.ProductId, // Match on ProductId (Troeslen & Japikse, 2021)
                    orderItem => orderItem.ProductId, // Match on ProductId (Troeslen & Japikse, 2021)
                    (product, orderItem) => new { product.CategoryId, orderItem.Quantity } // Select CategoryId and Quantity (Troeslen & Japikse, 2021)
                )
                .Join(
                    _context.Categories, // Join with Categories table to get category names (Troeslen & Japikse, 2021)
                    productOrder => productOrder.CategoryId, // Match CategoryId (Troeslen & Japikse, 2021)
                    category => category.CategoryId, // Match CategoryId (Troeslen & Japikse, 2021)
                    (productOrder, category) => new { category.CategoryName, productOrder.Quantity } // Select CategoryName and Quantity sold (Troeslen & Japikse, 2021)
                )
                .GroupBy(c => c.CategoryName) // Group by category name (Troeslen & Japikse, 2021)
                .Select(g => new
                {
                    Category = g.Key, // The category name (Troeslen & Japikse, 2021)
                    ProductsSold = g.Sum(x => x.Quantity) // Total products sold in this category (Troeslen & Japikse, 2021)
                })
                .OrderByDescending(c => c.ProductsSold) // Order by number of products sold (descending) (Troeslen & Japikse, 2021)
                .ToList();

            return Json(categoryProductCount); // Return the result as JSON (Troeslen & Japikse, 2021)
        }

        // This action calculates the total quantity of pending and processed orders (Troeslen & Japikse, 2021)
        public IActionResult OrderStatusDistribution()
        {
            // Count pending regular orders (Troeslen & Japikse, 2021)
            var pendingRegularOrdersCount = _context.Orders.Count(o => o.Status == "Pending");

            // Count pending custom orders (Troeslen & Japikse, 2021)
            var pendingCustomOrdersCount = _context.CustomOrders.Count(co => co.Status == "Pending");

            // Count processed regular orders (Troeslen & Japikse, 2021)
            var processedRegularOrdersCount = _context.Orders.Count(o => o.Status == "Processed");

            // Count processed custom orders (Troeslen & Japikse, 2021)
            var processedCustomOrdersCount = _context.CustomOrders.Count(co => co.Status == "Processed");

            // Combine and return data (Troeslen & Japikse, 2021)
            var statusDistribution = new[]
            {
                 new { status = "Pending Regular Orders", count = pendingRegularOrdersCount },
                 new { status = "Pending Custom Orders", count = pendingCustomOrdersCount },
                 new { status = "Processed Regular Orders", count = processedRegularOrdersCount },
                 new { status = "Processed Custom Orders", count = processedCustomOrdersCount }
            };

            return Json(statusDistribution); // Return the data as JSON (Troeslen & Japikse, 2021)
        }

        // Stock Availability (Troeslen & Japikse, 2021)
        // This action retrieves and returns the list of products and their current stock availability (Troeslen & Japikse, 2021)
        public IActionResult StockAvailability()
        {
            var stockData = _context.Products
                .Select(p => new { Product = p.ProductName, Stock = p.StockCount }) // Select product name and current stock count (Troeslen & Japikse, 2021)
                .ToList();

            return Json(stockData); // Return the result as JSON
        }

        // Most Active Customers (Troeslen & Japikse, 2021)
        // This action retrieves the most active customers, ordered by the total amount they spent (Troeslen & Japikse, 2021)
        public IActionResult MostActiveCustomers()
        {
            var currentMonth = DateTime.Now.Month;
            var currentYear = DateTime.Now.Year;

            var activeCustomers = _context.Orders
                .Where(o => o.CreatedAt.Month == currentMonth && o.CreatedAt.Year == currentYear) // Filter by current month and year (Troeslen & Japikse, 2021)
                .GroupBy(o => o.FullName) // Group orders by customer  (Troeslen & Japikse, 2021)
                .Select(g => new
                {
                    Customer = g.Key, // Customer name (Troeslen & Japikse, 2021)
                    Orders = g.Count(), // Number of orders placed by this customer (Troeslen & Japikse, 2021)
                    TotalSpent = _context.OrderItems
                        .Where(oi => g.Select(o => o.OrderId).Contains(oi.OrderId)) // Find the order items for the current customer's orders (Troeslen & Japikse, 2021)
                        .Sum(oi => oi.Quantity * oi.Price) // Calculate total amount spent by the customer (Troeslen & Japikse, 2021)
                })
                .OrderByDescending(c => c.TotalSpent) // Order customers by total amount spent (descending) (Troeslen & Japikse, 2021)
                .Take(3) // Take the top 3 spenders (Troeslen & Japikse, 2021)
                .ToList();

            return Json(activeCustomers); // Return the result as JSON (Troeslen & Japikse, 2021)
        }

        // Helper method to prevent page caching(Hewlett, 2015)
        private void PreventPageCaching()
        {
            Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
            Response.Headers["Pragma"] = "no-cache";
            Response.Headers["Expires"] = "0";
        }

        // GET: Admin/CustomOrders(Troeslen & Japikse, 2021)
        [HttpGet]
        public IActionResult CustomOrders(DateTime? startDate, DateTime? endDate, string status = "Pending")
        {
            // Prevent the page from being cached(Hewlett, 2015)
            PreventPageCaching();

            // Check if admin email is in session; redirect to login if not(Shahzad, 2019)
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("AdminEmail")))
            {
                return RedirectToAction("Login", "Login");
            }


            // Fetch custom orders based on status(Troeslen & Japikse, 2021)
            var customOrdersQuery = _context.CustomOrders
                                            .Where(o => o.Status == status);

            // Apply date filters if specified(Troeslen & Japikse, 2021)
            if (startDate.HasValue)
            {
                customOrdersQuery = customOrdersQuery.Where(o => o.CreatedAt >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                customOrdersQuery = customOrdersQuery.Where(o => o.CreatedAt <= endDate.Value);
            }

            // Execute query and return results(Troeslen & Japikse, 2021)
            var customOrders = customOrdersQuery.ToList();

            // Pass the filters to the view
            ViewBag.Status = status;
            ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd");

            return View(customOrders);
        }

        // GET: Admin/ViewCustomOrder/{id}(Troeslen & Japikse, 2021)
        [HttpGet]
        public IActionResult ViewCustomOrder(int id)
        {
            // Prevent the page from being cached(Hewlett, 2015)
            PreventPageCaching();

            // Check if admin email is in session; redirect to login if not(Shahzad, 2019)
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("AdminEmail")))
            {
                return RedirectToAction("Login", "Login");
            }


            var customOrder = _context.CustomOrders.FirstOrDefault(o => o.CustomOrderId == id);

            if (customOrder == null)
            {
                TempData["ErrorMessage"] = "Order not found.";
                return RedirectToAction("CustomOrders");
            }

            return View(customOrder);
        }

        // GET: Admin/OrderHistory/{id}(Troeslen & Japikse, 2021)
        public async Task<IActionResult> OrderHistory(DateTime? startDate, DateTime? endDate, string status = "Pending")
        {
            // Prevent the page from being cached(Hewlett, 2015)
            PreventPageCaching();

            // Check if admin email is in session; redirect to login if not(Shahzad, 2019)
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("AdminEmail")))
            {
                return RedirectToAction("Login", "Login");
            }


            // Fetch orders based on status and include processed date
            var ordersQuery = from o in _context.Orders
                              join oi in _context.OrderItems on o.OrderId equals oi.OrderId
                              join p in _context.Products on oi.ProductId equals p.ProductId
                              where (status == "Processed" && o.Status == "Processed" || status == "Pending" && o.Status == "Pending")
                              select new
                              {
                                  o.OrderId,
                                  o.CreatedAt,
                                  o.Email,
                                  o.ProcessedDate,
                                  oi.ProductId,
                                  ProductName = p.ProductName,
                                  oi.Quantity,
                                  oi.Price,
                                  o.Status
                              };

            // Apply date filters if specified(Troeslen & Japikse, 2021)
            if (startDate.HasValue)
            {
                ordersQuery = ordersQuery.Where(o => o.CreatedAt >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                ordersQuery = ordersQuery.Where(o => o.CreatedAt <= endDate.Value);
            }

            var orders = await ordersQuery
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            var orderHistoryViewModels = orders
                .GroupBy(o => new
                {
                    o.OrderId,
                    o.CreatedAt,
                    o.Email,
                    o.ProcessedDate,
                    o.Status
                })
                .Select(g => new OrderHistoryViewModel
                {
                    OrderId = g.Key.OrderId,
                    CreatedAt = g.Key.CreatedAt,
                    Email = g.Key.Email,
                    ProcessedDate = g.Key.ProcessedDate,
                    Status = g.Key.Status,
                    TotalAmount = g.Sum(item => item.Price * item.Quantity),
                    OrderItems = g.Select(item => new OrderItemViewModel
                    {
                        ProductName = item.ProductName,
                        Quantity = item.Quantity
                    }).ToList()
                }).ToList();

            // Pass the filters to the view
            ViewBag.Status = status;
            ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd");

            return View(orderHistoryViewModels);
        }

        // Export Pdf  is to export orders to a pdf (Hövel, 2015)
        public async Task<IActionResult> ExportToPdf(DateTime? startDate, DateTime? endDate, string status = "Pending")
        {
            // Set default dates if not provided (Troeslen & Japikse, 2021)
            startDate ??= DateTime.Now.AddMonths(-1);
            endDate ??= DateTime.Now;

            // Adjust the end date to include the entire day (Troeslen & Japikse, 2021)
            endDate = endDate.Value.Date.AddDays(1).AddTicks(-1);

            // Fetch orders based on status and include processed date (Troeslen & Japikse, 2021)
            var ordersQuery = from o in _context.Orders
                              join oi in _context.OrderItems on o.OrderId equals oi.OrderId
                              join p in _context.Products on oi.ProductId equals p.ProductId
                              where (status == "Processed" && o.Status == "Processed" || status == "Pending" && o.Status == "Pending")
                              select new
                              {
                                  o.OrderId,
                                  o.CreatedAt,
                                  o.Email,
                                  o.ProcessedDate,
                                  oi.ProductId,
                                  ProductName = p.ProductName,
                                  oi.Quantity,
                                  oi.Price,
                                  o.Status
                              };

            // Apply date filters (Troeslen & Japikse, 2021)
            if (startDate.HasValue)
            {
                ordersQuery = ordersQuery.Where(o => o.CreatedAt >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                ordersQuery = ordersQuery.Where(o => o.CreatedAt <= endDate.Value);
            }

            var orders = await ordersQuery
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            var orderHistoryViewModels = orders
                .GroupBy(o => new
                {
                    o.OrderId,
                    o.CreatedAt,
                    o.Email,
                    o.ProcessedDate,
                    o.Status
                })
                .Select(g => new OrderHistoryViewModel
                {
                    OrderId = g.Key.OrderId,
                    CreatedAt = g.Key.CreatedAt,
                    Email = g.Key.Email,
                    ProcessedDate = g.Key.ProcessedDate,
                    Status = g.Key.Status,
                    TotalAmount = g.Sum(item => item.Price * item.Quantity),
                    OrderItems = g.Select(item => new OrderItemViewModel
                    {
                        ProductName = item.ProductName,
                        Quantity = item.Quantity
                    }).ToList()
                }).ToList();

            // Prepare the PDF document (Hövel, 2015)
            var pdfDocument = new PdfDocument();
            pdfDocument.Info.Title = "Order History Report";

            int currentPage = 1;
            PdfPage page = pdfDocument.AddPage();
            page.Orientation = PageOrientation.Landscape; // Set landscape orientation (Hövel, 2015)
            page.Size = PageSize.A3; // Set page size to A3 for more space (Hövel, 2015)
            XGraphics gfx = XGraphics.FromPdfPage(page); // Create graphics object to draw on the page (Hövel, 2015)
            XFont font = new XFont("Arial", 11);
            XFont headerFont = new XFont("Arial Bold", 12);
            XFont pageNumberFont = new XFont("Arial", 11);

            DateTime reportGeneratedDate = DateTime.Now;
            string formattedReportDate = reportGeneratedDate.ToString("d MMMM yyyy");

            // Load and draw the logo on the left (Hövel, 2015)
            string logoPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Images", "Mugged_Logo.jpg");
            XImage logo = XImage.FromFile(logoPath);

            // Calculate aspect ratio to preserve the image's original proportions (Hövel, 2015)
            double aspectRatio = (double)logo.PixelWidth / logo.PixelHeight;
            double logoWidth = 100;  // Fixed width for the logo (Hövel, 2015)
            double logoHeight = logoWidth / aspectRatio;  // Adjust height based on aspect ratio (Hövel, 2015)

            double xLogoPosition = 50; // Position the logo on the left with some margin (Hövel, 2015)
            double yLogoPosition = 50; // Position the logo at the top (Hövel, 2015)
            gfx.DrawImage(logo, xLogoPosition, yLogoPosition, logoWidth, logoHeight);

            // Draw the "Mugged" text next to the logo
            string muggedText = "Mugged";
            XFont muggedFont = new XFont("Arial Bold", 24); // Bold font for the text (Hövel, 2015)
            XSize muggedTextSize = gfx.MeasureString(muggedText, muggedFont);
            double xTextPosition = xLogoPosition + logoWidth + 20; // Position the text next to the logo with some spacing (Hövel, 2015)
            double yTextPosition = yLogoPosition + (logoHeight / 2) + (muggedTextSize.Height / 4); // Vertically align the text with the logo (Hövel, 2015)
            gfx.DrawString(muggedText, muggedFont, XBrushes.Black, new XPoint(xTextPosition, yTextPosition));

            // Define the dimensions of the border box (Hövel, 2015)
            double borderPadding = 19; // Add padding around the content inside the border (Hövel, 2015)
                                       // Define the dimensions of the border box to touch the left, right, and top margins (Hövel, 2015)
            double borderX = 0; // Start from the very left edge of the page (Hövel, 2015)
            double borderY = 0; // Start from the top edge of the page (Hövel, 2015)
            double borderWidth = page.Width; // Full page width (Hövel, 2015)
            double borderHeight = 160; // Adjust the height to cover the logo, text, and contact info (Hövel, 2015)

            // Draw the border rectangle with a pink outline (no fill) (Hövel, 2015)
            XColor pinkColor = XColor.FromArgb(255, 105, 180); // Define a pink color (example: hot pink) (Hövel, 2015)
            gfx.DrawRectangle(new XPen(pinkColor, 5), borderX, borderY, borderWidth, borderHeight);

            // Draw the contact information aligned to the right and vertically with the logo (Hövel, 2015)
            XFont contactFont = new XFont("Arial", 12);
            double contactXPosition = page.Width - 300; // Align to the right with some margin (Hövel, 2015)
            double contactYPosition = yLogoPosition + 30; // Adjusted position to align better with the logo (Hövel, 2015)

            // Combine current directory with relative path to the images (Hövel, 2015)
            string emailIconPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Images", "email_icon.png");
            string locationIconPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Images", "location_icon.png");
            string phoneIconPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Images", "phone_icon.png");

            // Load the icons as images (Hövel, 2015)
            XImage emailIcon = XImage.FromFile(emailIconPath);
            XImage locationIcon = XImage.FromFile(locationIconPath);
            XImage phoneIcon = XImage.FromFile(phoneIconPath);


            // Adjust the vertical position for the icons to align better with the text (Hövel, 2015)
            double iconOffset = 15; // Adjust this value to move the icons up or down (Hövel, 2015)

            // Draw the icons with text (Hövel, 2015)
            gfx.DrawImage(emailIcon, contactXPosition - 20, contactYPosition - iconOffset, 20, 20); // Move icon up (Hövel, 2015)
            gfx.DrawString(" muggedbc@gmail.com", contactFont, XBrushes.Black, new XPoint(contactXPosition, contactYPosition));

            contactYPosition += 20;
            gfx.DrawImage(locationIcon, contactXPosition - 20, contactYPosition - iconOffset, 20, 20); // Move icon up (Hövel, 2015)
            gfx.DrawString(" Gqeberha", contactFont, XBrushes.Black, new XPoint(contactXPosition, contactYPosition));

            contactYPosition += 20;
            gfx.DrawImage(phoneIcon, contactXPosition - 20, contactYPosition - iconOffset, 20, 20); // Move icon up (Hövel, 2015)
            gfx.DrawString(" 072-611-7505", contactFont, XBrushes.Black, new XPoint(contactXPosition, contactYPosition));


            // Add spacing after the contact info (Hövel, 2015)
            double spacingAfterContactInfo = 85; // Adjust this value as needed (Hövel, 2015)
            double orderHistoryYPosition = contactYPosition + spacingAfterContactInfo; // Calculate the Y position for the Order History heading (Hövel, 2015)

            // Draw the "Order History" heading centered
            XFont orderHistoryFont = new XFont("Arial Bold", 24); // Bold font for the heading (Hövel, 2015)
            string orderHistoryText = "Order History:";
            double orderHistoryXPosition = (page.Width - gfx.MeasureString(orderHistoryText, orderHistoryFont).Width) / 2; // Center the heading (Hövel, 2015)
            gfx.DrawString(orderHistoryText, orderHistoryFont, XBrushes.Black, new XPoint(orderHistoryXPosition, orderHistoryYPosition));

            // Draw the underline
            double underlineYPosition = orderHistoryYPosition + 5; // Position the underline slightly below the text (Hövel, 2015)
            double underlineStartX = orderHistoryXPosition; // Start position of the underline (Hövel, 2015)
            double underlineEndX = orderHistoryXPosition + gfx.MeasureString(orderHistoryText, orderHistoryFont).Width; // End position of the underline (Hövel, 2015)
            gfx.DrawLine(new XPen(XColors.Black, 1), new XPoint(underlineStartX, underlineYPosition), new XPoint(underlineEndX, underlineYPosition));

            // Position Date Range and Report Generated text below the header (Hövel, 2015)
            double yPosition = yLogoPosition + logoHeight + 80; // Space between header and next section (Hövel, 2015)
            gfx.DrawString($"Date Range: {startDate.Value.ToString("d MMMM yyyy")} - {endDate.Value.ToString("d MMMM yyyy")}", font, XBrushes.Black, new XPoint(40, yPosition));
            string reportGeneratedText = $"Report Generated: {formattedReportDate}";
            XSize reportGeneratedTextSize = gfx.MeasureString(reportGeneratedText, font);
            double xPositionForReportGenerated = page.Width - reportGeneratedTextSize.Width - 40;
            gfx.DrawString(reportGeneratedText, font, XBrushes.Black, new XPoint(xPositionForReportGenerated, yPosition));

            // Space before table (Hövel, 2015)
            yPosition += 40;

            // Define fixed column widths for the table (Hövel, 2015)
            float[] columnWidths = { 225, 225, 225, 225, 225 };
            string[] headers = { "Order ID", "Order Date", "User Email", "Total Amount", "Status" };

            // Draw table headers (Hövel, 2015)
            DrawTableRow(gfx, headers, headerFont, (int)yPosition, columnWidths, true);

            // Draw table rows for each order (Hövel, 2015)
            int yPoint = (int)yPosition + 20; // Starting position for table rows (Hövel, 2015)
            double rowHeight = 20; // Row height (Hövel, 2015)
            foreach (var order in orderHistoryViewModels)
            {
                string[] rowData = {
            order.OrderId.ToString(),
            order.CreatedAt?.ToString("MM/dd/yyyy") ?? "",
            order.Email,
            order.TotalAmount.ToString("C"),
            order.Status
        };

                // Check if there's enough space for the next row (Hövel, 2015)
                if (yPoint + rowHeight > page.Height - 40) // Leave some space at the bottom (Hövel, 2015)
                {
                    DrawPageNumber(gfx, pageNumberFont, currentPage, page);
                    currentPage++;
                    page = pdfDocument.AddPage();
                    gfx = XGraphics.FromPdfPage(page);
                    yPoint = 40; // Reset Y position for new page (Hövel, 2015)
                }

                // Draw the current row with actual data (Hövel, 2015)
                DrawTableRow(gfx, rowData, font, yPoint, columnWidths, false);
                yPoint += (int)rowHeight;
            }

            // Draw page number on the last page (Hövel, 2015)
            DrawPageNumber(gfx, pageNumberFont, currentPage, page);

            // Return the PDF as a file (Hövel, 2015)
            var stream = new MemoryStream();
            pdfDocument.Save(stream);
            stream.Position = 0;

            return File(stream, "application/pdf", "OrderHistoryReport.pdf");
        }

        // Export Pdf 2 is to export custom orders to a pdf (Hövel, 2015)
        public async Task<IActionResult> ExportToPdf2(DateTime? startDate, DateTime? endDate, string status = "Pending")
        {
            // Set default dates if not provided (Troeslen & Japikse, 2021)
            startDate ??= DateTime.Now.AddMonths(-1);
            endDate ??= DateTime.Now;

            // Adjust the end date to include the entire day (Troeslen & Japikse, 2021)
            endDate = endDate.Value.Date.AddDays(1).AddTicks(-1);

            // Fetch custom orders based on status (Troeslen & Japikse, 2021)
            var customOrdersQuery = _context.CustomOrders
                                            .Where(o => o.Status == status);

            // Apply date filters (Troeslen & Japikse, 2021)
            if (startDate.HasValue)
            {
                customOrdersQuery = customOrdersQuery.Where(o => o.CreatedAt >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                customOrdersQuery = customOrdersQuery.Where(o => o.CreatedAt <= endDate.Value);
            }

            var customOrders = await customOrdersQuery
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            // Prepare the PDF document (Hövel, 2015)
            var pdfDocument = new PdfDocument();
            pdfDocument.Info.Title = "Custom Orders Report";

            int currentPage = 1;
            PdfPage page = pdfDocument.AddPage();
            page.Orientation = PageOrientation.Landscape; // Set landscape orientation (Hövel, 2015)
            page.Size = PageSize.A3; // Set page size to A3 for more space (Hövel, 2015)
            XGraphics gfx = XGraphics.FromPdfPage(page); // Create graphics object to draw on the page (Hövel, 2015)
            XFont font = new XFont("Arial", 11);
            XFont headerFont = new XFont("Arial Bold", 12);
            XFont pageNumberFont = new XFont("Arial", 11);

            DateTime reportGeneratedDate = DateTime.Now;
            string formattedReportDate = reportGeneratedDate.ToString("d MMMM yyyy");

            // Load and draw the logo on the left (Hövel, 2015)
            string logoPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Images", "Mugged_Logo.jpg");
            XImage logo = XImage.FromFile(logoPath);

            // Calculate aspect ratio to preserve the image's original proportions (Hövel, 2015)
            double aspectRatio = (double)logo.PixelWidth / logo.PixelHeight;
            double logoWidth = 100;  // Fixed width for the logo (Hövel, 2015)
            double logoHeight = logoWidth / aspectRatio;  // Adjust height based on aspect ratio (Hövel, 2015)

            double xLogoPosition = 50; // Position the logo on the left with some margin (Hövel, 2015)
            double yLogoPosition = 50; // Position the logo at the top (Hövel, 2015)
            gfx.DrawImage(logo, xLogoPosition, yLogoPosition, logoWidth, logoHeight);

            // Draw the "Mugged" text next to the logo (Hövel, 2015)
            string muggedText = "Mugged";
            XFont muggedFont = new XFont("Arial Bold", 24); // Bold font for the text (Hövel, 2015)
            XSize muggedTextSize = gfx.MeasureString(muggedText, muggedFont);
            double xTextPosition = xLogoPosition + logoWidth + 20; // Position the text next to the logo with some spacing (Hövel, 2015)
            double yTextPosition = yLogoPosition + (logoHeight / 2) + (muggedTextSize.Height / 4); // Vertically align the text with the logo (Hövel, 2015)
            gfx.DrawString(muggedText, muggedFont, XBrushes.Black, new XPoint(xTextPosition, yTextPosition));

            // Define the dimensions of the border box (Hövel, 2015)
            double borderPadding = 19; // Add padding around the content inside the border (Hövel, 2015)
            double borderX = 0; // Start from the very left edge of the page (Hövel, 2015)
            double borderY = 0; // Start from the top edge of the page (Hövel, 2015)
            double borderWidth = page.Width; // Full page width (Hövel, 2015)
            double borderHeight = 160; // Adjust the height to cover the logo, text, and contact info (Hövel, 2015)

            // Draw the border rectangle with a pink outline (no fill) (Hövel, 2015)
            XColor pinkColor = XColor.FromArgb(255, 105, 180); // Define a pink color (example: hot pink) (Hövel, 2015)
            gfx.DrawRectangle(new XPen(pinkColor, 5), borderX, borderY, borderWidth, borderHeight);

            // Draw the contact information aligned to the right and vertically with the logo (Hövel, 2015)
            XFont contactFont = new XFont("Arial", 12);
            double contactXPosition = page.Width - 300; // Align to the right with some margin (Hövel, 2015)
            double contactYPosition = yLogoPosition + 30; // Adjusted position to align better with the logo (Hövel, 2015)

            // Combine current directory with relative path to the images (Hövel, 2015)
            string emailIconPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Images", "email_icon.png");
            string locationIconPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Images", "location_icon.png");
            string phoneIconPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Images", "phone_icon.png");

            // Load the icons as images (Hövel, 2015)
            XImage emailIcon = XImage.FromFile(emailIconPath);
            XImage locationIcon = XImage.FromFile(locationIconPath);
            XImage phoneIcon = XImage.FromFile(phoneIconPath);

            // Adjust the vertical position for the icons to align better with the text (Hövel, 2015)
            double iconOffset = 15; // Adjust this value to move the icons up or down (Hövel, 2015)

            // Draw the icons with text (Hövel, 2015)
            gfx.DrawImage(emailIcon, contactXPosition - 20, contactYPosition - iconOffset, 20, 20); // Move icon up (Hövel, 2015)
            gfx.DrawString(" muggedbc@gmail.com", contactFont, XBrushes.Black, new XPoint(contactXPosition, contactYPosition));

            contactYPosition += 20;
            gfx.DrawImage(locationIcon, contactXPosition - 20, contactYPosition - iconOffset, 20, 20); // Move icon up (Hövel, 2015)
            gfx.DrawString(" Gqeberha", contactFont, XBrushes.Black, new XPoint(contactXPosition, contactYPosition));

            contactYPosition += 20;
            gfx.DrawImage(phoneIcon, contactXPosition - 20, contactYPosition - iconOffset, 20, 20); // Move icon up (Hövel, 2015)
            gfx.DrawString(" 072-611-7505", contactFont, XBrushes.Black, new XPoint(contactXPosition, contactYPosition));

            // Add spacing after the contact info (Hövel, 2015)
            double spacingAfterContactInfo = 85; // Adjust this value as needed (Hövel, 2015)
            double orderHistoryYPosition = contactYPosition + spacingAfterContactInfo; // Calculate the Y position for the Custom Orders heading (Hövel, 2015)

            // Draw the "Custom Orders" heading centered
            XFont orderHistoryFont = new XFont("Arial Bold", 24); // Bold font for the heading (Hövel, 2015)
            string orderHistoryText = "Custom Orders:";
            double orderHistoryXPosition = (page.Width - gfx.MeasureString(orderHistoryText, orderHistoryFont).Width) / 2; // Center the heading (Hövel, 2015)
            gfx.DrawString(orderHistoryText, orderHistoryFont, XBrushes.Black, new XPoint(orderHistoryXPosition, orderHistoryYPosition));

            // Draw the underline (Hövel, 2015)
            double underlineYPosition = orderHistoryYPosition + 5; // Position the underline slightly below the text (Hövel, 2015)
            double underlineStartX = orderHistoryXPosition; // Start position of the underline (Hövel, 2015)
            double underlineEndX = orderHistoryXPosition + gfx.MeasureString(orderHistoryText, orderHistoryFont).Width; // End position of the underline (Hövel, 2015)
            gfx.DrawLine(new XPen(XColors.Black, 1), new XPoint(underlineStartX, underlineYPosition), new XPoint(underlineEndX, underlineYPosition));

            // Position Date Range and Report Generated text below the header (Hövel, 2015)
            double yPosition = yLogoPosition + logoHeight + 80; // Space between header and next section (Hövel, 2015)
            gfx.DrawString($"Date Range: {startDate.Value.ToString("d MMMM yyyy")} - {endDate.Value.ToString("d MMMM yyyy")}", font, XBrushes.Black, new XPoint(40, yPosition));
            string reportGeneratedText = $"Report Generated: {formattedReportDate}";
            XSize reportGeneratedTextSize = gfx.MeasureString(reportGeneratedText, font);
            double xPositionForReportGenerated = page.Width - reportGeneratedTextSize.Width - 40;
            gfx.DrawString(reportGeneratedText, font, XBrushes.Black, new XPoint(xPositionForReportGenerated, yPosition));

            // Space before table (Hövel, 2015)
            yPosition += 40;

            // Define fixed column widths for the table (Hövel, 2015)
            float[] columnWidths = { 277, 277, 277, 277 }; // Adjust based on the data you need (Hövel, 2015)
            string[] headers = { "Order ID", "Order Date", "User Email", "Status" };

            // Draw table headers (Hövel, 2015)
            DrawTableRow(gfx, headers, headerFont, (int)yPosition, columnWidths, true);

            // Draw table rows for each custom order (Hövel, 2015)
            int yPoint = (int)yPosition + 20; // Starting position for table rows (Hövel, 2015)
            double rowHeight = 20; // Adjust row height as necessary (Hövel, 2015)
            foreach (var order in customOrders)
            {
                string[] rowData = { order.OrderId.ToString(), order.CreatedAt.ToString("d MMMM yyyy"), order.UserEmail, order.Status };
                DrawTableRow(gfx, rowData, font, yPoint, columnWidths, false);
                yPoint += (int)rowHeight;
            }

            // Add page number at the bottom (Hövel, 2015)
            string pageNumberText = $"Page {currentPage}";
            XSize pageNumberTextSize = gfx.MeasureString(pageNumberText, pageNumberFont);
            gfx.DrawString(pageNumberText, pageNumberFont, XBrushes.Black, new XPoint(page.Width - pageNumberTextSize.Width - 40, page.Height - 40));

            // Save the PDF to memory stream (Hövel, 2015)
            using (MemoryStream memoryStream = new MemoryStream())
            {
                pdfDocument.Save(memoryStream);
                memoryStream.Position = 0;
                return File(memoryStream.ToArray(), "application/pdf", "CustomOrdersReport.pdf");
            }
        }

        // Method to draw a table row (Hövel, 2015)
        void DrawTableRow(XGraphics gfx, string[] values, XFont font, int y, float[] columnWidths, bool isHeader)
        {
            double x = 40; // Starting X position (Hövel, 2015)
            XBrush brush = isHeader ? XBrushes.White : XBrushes.Black;
            XColor backgroundColor = isHeader ? XColor.FromArgb(248, 143, 95) : XColor.FromArgb(255, 255, 255);

            for (int i = 0; i < values.Length; i++)
            {
                // Draw cell background (Hövel, 2015)
                gfx.DrawRectangle(new XSolidBrush(backgroundColor), x, y, columnWidths[i], 20);

                // Draw text (Hövel, 2015)
                gfx.DrawString(values[i], font, brush, new XRect(x, y, columnWidths[i], 20), XStringFormats.Center);

                // Draw cell border (Hövel, 2015)
                gfx.DrawRectangle(new XPen(XColors.Gray), x, y, columnWidths[i], 20);

                x += columnWidths[i]; // Move to next column (Hövel, 2015)
            }
        }

        // Method to draw the page number (Hövel, 2015)
        private void DrawPageNumber(XGraphics gfx, XFont font, int pageNumber, PdfPage page)
        {
            gfx.DrawString($"Page {pageNumber}", font, XBrushes.Black, new XPoint(page.Width - 50, page.Height - 30));
        }


        public async Task<IActionResult> ViewOrder(int id)
        {
            // Prevent the page from being cached(Hewlett, 2015)
            PreventPageCaching();

            // Check if admin email is in session; redirect to login if not(Shahzad, 2019)
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("AdminEmail")))
            {
                return RedirectToAction("Login", "Login");
            }


            // Fetch the order details by order ID(Troeslen & Japikse, 2021)
            var order = await (from o in _context.Orders
                               join oi in _context.OrderItems on o.OrderId equals oi.OrderId
                               join p in _context.Products on oi.ProductId equals p.ProductId
                               where o.OrderId == id
                               select new
                               {
                                   o.OrderId,
                                   o.CreatedAt,
                                   o.ProcessedDate,
                                   o.Email,
                                   oi.ProductId,
                                   ProductName = p.ProductName,
                                   oi.Quantity,
                                   oi.Price
                               }).ToListAsync();

            if (order == null || !order.Any())
            {
                return NotFound(); // Return 404 if the order is not found(Troeslen & Japikse, 2021)
            }

            // Create a view model to pass to the view(Troeslen & Japikse, 2021)
            var orderDetailsViewModel = new OrderDetailsViewModel
            {
                OrderId = order.First().OrderId,
                CreatedAt = order.First().CreatedAt,
                ProcessedDate = order.First().ProcessedDate,
                UserEmail = order.First().Email,
                OrderItems = order.Select(item => new OrderItemViewModel
                {
                    ProductName = item.ProductName,
                    Quantity = item.Quantity,
                    Price = item.Price
                }).ToList()
            };

            return View(orderDetailsViewModel);
        }

        // GET: Displays a list of all users(Troeslen & Japikse, 2021)
        public IActionResult ViewUsers()
        {
            // Prevent the page from being cached(Hewlett, 2015)
            PreventPageCaching();

            // Check if admin email is in session; redirect to login if not(Shahzad, 2019)
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("AdminEmail")))
            {
                return RedirectToAction("Login", "Login");
            }

            // Fetch the list of users from the database(Troeslen & Japikse, 2021)
            var users = _context.Users.ToList();

            return View(users);
        }

        // Processes a standard order by updating its status to "Processed" and saving the change to the database(Troeslen & Japikse, 2021)
        [HttpPost]
        public async Task<IActionResult> ProcessOrder(int id)
        {
            // Find the order by its ID in the Orders table(Troeslen & Japikse, 2021)
            var order = await _context.Orders.FindAsync(id);

            // If the order is not found, return a NotFound result(Troeslen & Japikse, 2021)
            if (order == null)
            {
                return NotFound();
            }

            // Update the date field(Troeslen & Japikse, 2021)
            order.ProcessedDate = DateTime.Now;
            // Update the status of the order to "Processed"(Troeslen & Japikse, 2021)
            order.Status = "Processed";

            // Save changes to the database
            await _context.SaveChangesAsync();

            // Set a success message to display to the user after redirection(Troeslen & Japikse, 2021)
            TempData["SuccessMessage"] = "Order processed successfully.";

            // Redirect to the OrderHistory action(Troeslen & Japikse, 2021)
            return RedirectToAction("OrderHistory");
        }

        // Processes a custom order by updating its status to "Processed" and saving the change to the database(Troeslen & Japikse, 2021)
        [HttpPost]
        public async Task<IActionResult> ProcessCustomOrder(int id)
        {
            // Find the custom order by its ID in the CustomOrders table(Troeslen & Japikse, 2021)
            var customOrder = await _context.CustomOrders.FindAsync(id);

            // If the custom order is not found, return a NotFound result(Troeslen & Japikse, 2021)
            if (customOrder == null)
            {
                return NotFound();
            }

            // Update the date field(Troeslen & Japikse, 2021)
            customOrder.ProcessedDate = DateTime.Now;
            // Update the status of the custom order to "Processed"(Troeslen & Japikse, 2021)
            customOrder.Status = "Processed";

            // Save changes to the database(Troeslen & Japikse, 2021)
            await _context.SaveChangesAsync();

            // Set a success message to display to the user after redirection(Troeslen & Japikse, 2021)
            TempData["SuccessMessage"] = "Custom order processed successfully.";

            // Redirect to the CustomOrders action(Troeslen & Japikse, 2021)
            return RedirectToAction("CustomOrders");
        }

        public IActionResult Logout()
        {
            // Clear session data for the logged-in user(Shahzad, 2019)
            HttpContext.Session.Remove("AdminEmail");

            // Redirect to the home page (or login page)(Shahzad, 2019)
            return RedirectToAction("Index", "Home");
        }
    }
}
