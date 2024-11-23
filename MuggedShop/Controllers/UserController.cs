using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MuggedShop.Models;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using PdfSharp;
using MuggedShop.ModelViews;

namespace Mugged.Controllers
{
    public class UserController : Controller
    {
        // Context to interact with the database (Troeslen & Japikse, 2021)
        private readonly MuggedContext _context = new MuggedContext();

        // GET: User/RegisterUser - Displays the user registration form (Troeslen & Japikse, 2021)
        public IActionResult RegisterUser()
        {
            return View();
        }

        // POST: User/RegisterUser - Displays the user registration form (Troeslen & Japikse, 2021)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterUser([Bind("Email,FullName,PhoneNumber,PasswordHash")] User user)
        {
            if (ModelState.IsValid)
            {
                // Check if the email already exists in Users table (Troeslen & Japikse, 2021)
                var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == user.Email);
                if (existingUser != null)
                {
                    ViewBag.Error = "Email is already taken. Please choose a different one.";
                    return View(user);
                }

                // Check if the email already exists in AdminUsers table (Troeslen & Japikse, 2021)
                var adminEmail = await _context.AdminUsers.FirstOrDefaultAsync(a => a.Email == user.Email);
                if (adminEmail != null)
                {
                    ViewBag.Error = "Email belongs to an admin. Please use a different email.";
                    return View(user);
                }

                // Validate that the password is at least 6 characters long (Troeslen & Japikse, 2021)
                if (user.PasswordHash.Length < 6)
                {
                    ViewBag.Error = "Password must be at least 6 characters long.";
                    return View(user);
                }

                // Hash the password before storing it in the database (Troeslen & Japikse, 2021)
                user.PasswordHash = HashPassword(user.PasswordHash);

                // Add the user to the database
                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // Redirect to the login page after successful registration (Troeslen & Japikse, 2021)
                return RedirectToAction("Login", "Login");
            }

            // Return the same view if validation fails (Troeslen & Japikse, 2021)
            return View(user);
        }

        // GET: User/LoginUser - Displays the user login form (Troeslen & Japikse, 2021)
        public IActionResult LoginUser()
        {
            return View();
        }

        // POST: Login the user - Processes the login form (Troeslen & Japikse, 2021)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LoginUser([Bind("Email,PasswordHash")] User user)
        {
            // Check if the model state is valid (Troeslen & Japikse, 2021)
            if (ModelState.IsValid)
            {
                // Find the user by email (Troeslen & Japikse, 2021)
                var existingUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == user.Email);

                if (existingUser == null)
                {
                    ViewBag.Error = "User not found.";
                    return View(user);
                }

                if (!VerifyPassword(user.PasswordHash, existingUser.PasswordHash))
                {
                    ViewBag.Error = "Invalid password.";
                    return View(user);
                }

                if (existingUser != null)
                {
                    // Verify the password (Troeslen & Japikse, 2021)
                    if (VerifyPassword(user.PasswordHash, existingUser.PasswordHash))
                    {
                        // Log the user in (set session, etc.) (Troeslen & Japikse, 2021)
                        HttpContext.Session.SetString("UserEmail", existingUser.Email);

                        // Redirect to a secure area of the site (Troeslen & Japikse, 2021)
                        return RedirectToAction("UserHome", "User");
                    }
                    else
                    {
                        // Password is incorrect (Troeslen & Japikse, 2021)
                        ViewBag.Error = "Invalid password.";
                        return View(user);
                    }
                }
                else
                {
                    // User not found (Troeslen & Japikse, 2021)
                    ViewBag.Error = "User not found.";
                    return View(user);
                }
            }

            // Return the same view if validation fails (Troeslen & Japikse, 2021)
            return View(user);
        }

        // Method to hash the password using PBKDF2 (Troeslen & Japikse, 2021)
        private string HashPassword(string password)
        {
            // Derive a 256-bit subkey (using HMACSHA256 with 100,000 iterations) (Microsoft, 2022)
            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password!, // The original password (Microsoft, 2022)
                salt: new byte[0],   // No salt used for now (Microsoft, 2022)
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 100000, // Number of iterations for key derivation (Microsoft, 2022)
                numBytesRequested: 256 / 8)); // Generate a 256-bit key (32 bytes) (Microsoft, 2022)

            return hashed;
        }

        // Method to verify if the entered password matches the stored hash (Troeslen & Japikse, 2021)
        private bool VerifyPassword(string password, string storedHash)
        {
            // Hash the entered password and compare it with the stored hash (Troeslen & Japikse, 2021)
            string hashedPassword = HashPassword(password);
            return hashedPassword == storedHash;
        }

        // GET: User/UserHome - Displays the user's home page (Troeslen & Japikse, 2021)
        public IActionResult UserHome()
        {
            // Prevent the page from being cached(Hewlett, 2015)
            PreventPageCaching();

            // Check if user email is in session; redirect to login if not (Troeslen & Japikse, 2021)
            var userEmail = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(userEmail))
            {
                return RedirectToAction("Login","Login");
            }

            // Get counts for regular orders (Troeslen & Japikse, 2021)
            var pendingOrdersCount = _context.Orders
                .CountAsync(o => o.Email == userEmail && o.Status == "Pending")
                .GetAwaiter().GetResult();
            var processedOrdersCount = _context.Orders
                .CountAsync(o => o.Email == userEmail && o.Status == "Processed")
                .GetAwaiter().GetResult();

            // Get counts for custom orders (Troeslen & Japikse, 2021)
            var pendingCustomOrdersCount = _context.CustomOrders
                .CountAsync(c => c.UserEmail == userEmail && c.Status == "Pending")
                .GetAwaiter().GetResult();
            var processedCustomOrdersCount = _context.CustomOrders
                .CountAsync(c => c.UserEmail == userEmail && c.Status == "Processed")
                .GetAwaiter().GetResult();

            // Pass data to the view (Troeslen & Japikse, 2021)
            ViewBag.PendingOrdersCount = pendingOrdersCount;
            ViewBag.ProcessedOrdersCount = processedOrdersCount;
            ViewBag.PendingCustomOrdersCount = pendingCustomOrdersCount;
            ViewBag.ProcessedCustomOrdersCount = processedCustomOrdersCount;

            return View();
        }

        // GET: User/OrderHistory - Displays the user's order history (Troeslen & Japikse, 2021)
        public async Task<IActionResult> OrderHistory(DateTime? startDate, DateTime? endDate, string status = "Pending")
        {
            // Prevent the page from being cached(Hewlett, 2015)
            PreventPageCaching();

            // Get the user's email from the session to identify the orders associated with them (Troeslen & Japikse, 2021)
            var userEmail = HttpContext.Session.GetString("UserEmail");

            // If no user email is found in session, redirect to the login page (Troeslen & Japikse, 2021)
            if (userEmail == null)
            {
                return RedirectToAction("Login", "Login");
            }

            // Query the database for orders by joining OrderItems and Products tables (Troeslen & Japikse, 2021)
            var ordersQuery = from o in _context.Orders
                              join oi in _context.OrderItems on o.OrderId equals oi.OrderId
                              join p in _context.Products on oi.ProductId equals p.ProductId
                              where o.Email == userEmail
                              select new
                              {
                                  o.OrderId,
                                  o.CreatedAt, 
                                  o.Status,
                                  oi.ProductId, 
                                  ProductName = p.ProductName, 
                                  oi.Quantity, 
                                  oi.Price 
                              };

            // Filter by order status if specified (e.g., Pending, Processed) (Troeslen & Japikse, 2021)
            if (!string.IsNullOrEmpty(status))
            {
                ordersQuery = ordersQuery.Where(o => o.Status == status);
            }

            // Filter by the start date if provided (Troeslen & Japikse, 2021)
            if (startDate.HasValue)
            {
                ordersQuery = ordersQuery.Where(o => o.CreatedAt >= startDate.Value);
            }

            // Filter by the end date if provided (Troeslen & Japikse, 2021)
            if (endDate.HasValue)
            {
                ordersQuery = ordersQuery.Where(o => o.CreatedAt <= endDate.Value);
            }

            // Execute the query asynchronously and order the results by the order creation date in descending order (Troeslen & Japikse, 2021)
            var orders = await ordersQuery
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            // Group the orders by OrderId, CreatedAt, and Status to calculate total amounts and item details for each order (Troeslen & Japikse, 2021)
            var orderHistoryViewModels = orders
                .GroupBy(o => new
                {
                    o.OrderId,
                    o.CreatedAt,
                    o.Status
                })
                .Select(g => new OrderHistoryViewModel
                {
                    OrderId = g.Key.OrderId,
                    CreatedAt = g.Key.CreatedAt,
                    Status = g.Key.Status, 
                    TotalAmount = g.Sum(item => item.Price * item.Quantity), 
                    OrderItems = g.Select(item => new OrderItemViewModel
                    {
                        ProductName = item.ProductName, 
                        Quantity = item.Quantity
                    }).ToList() 
                }).ToList();

            // Pass the filter parameters (status, start date, end date) to the view to retain the user selections on the page (Troeslen & Japikse, 2021)
            ViewBag.Status = status;
            ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd");

            // Return the order history view with the filtered and grouped data (Troeslen & Japikse, 2021)
            return View(orderHistoryViewModels);
        }

        // Export Pdf  is to export orders for logged in users to a pdf (Troeslen & Japikse, 2021)
        public async Task<IActionResult> ExportToPdf3(DateTime? startDate, DateTime? endDate, string status = "Pending")
        {
            // Set default dates if not provided (Troeslen & Japikse, 2021)
            startDate ??= DateTime.Now.AddMonths(-1);
            endDate ??= DateTime.Now;

            // Adjust the end date to include the entire day (Troeslen & Japikse, 2021)
            endDate = endDate.Value.Date.AddDays(1).AddTicks(-1);

            // Get the user's email from session
            var userEmail = HttpContext.Session.GetString("UserEmail");
            if (userEmail == null)
            {
                return RedirectToAction("Login", "Login");
            }

            var ordersQuery = from o in _context.Orders
                              join oi in _context.OrderItems on o.OrderId equals oi.OrderId
                              join p in _context.Products on oi.ProductId equals p.ProductId
                              where o.Email == userEmail // Ensure that we filter by logged-in user (Troeslen & Japikse, 2021)
                              select new
                              {
                                  o.OrderId,
                                  o.CreatedAt,
                                  o.ProcessedDate,
                                  o.Status,
                                  oi.ProductId,
                                  ProductName = p.ProductName,
                                  oi.Quantity,
                                  oi.Price
                              };

            // Apply additional filters (Troeslen & Japikse, 2021)
            if (!string.IsNullOrEmpty(status))
            {
                ordersQuery = ordersQuery.Where(o => o.Status == status); // Filter by status if specified (Troeslen & Japikse, 2021)
            }

            if (startDate.HasValue)
            {
                ordersQuery = ordersQuery.Where(o => o.CreatedAt >= startDate.Value); // Filter by start date if specified (Troeslen & Japikse, 2021)
            }

            if (endDate.HasValue)
            {
                ordersQuery = ordersQuery.Where(o => o.CreatedAt <= endDate.Value); // Filter by end date if specified (Troeslen & Japikse, 2021)
            }

            // Execute query and retrieve the orders
            var orders = await ordersQuery
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();


            // Group the orders by OrderId, CreatedAt, and Status, calculating the total amount for each order (Troeslen & Japikse, 2021)
            var orderHistoryViewModels = orders
                .GroupBy(o => new
                {
                    o.OrderId,
                    o.CreatedAt,
                    o.Status
                })
                .Select(g => new OrderHistoryViewModel
                {
                    OrderId = g.Key.OrderId,
                    CreatedAt = g.Key.CreatedAt,
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
            pdfDocument.Info.Title = "My Order History Report";

            int currentPage = 1;
            PdfPage page = pdfDocument.AddPage();
            page.Orientation = PageOrientation.Landscape; // Landscape orientation for more space (Hövel, 2015)
            page.Size = PageSize.A3; // Page size set to A3 (Hövel, 2015)
            XGraphics gfx = XGraphics.FromPdfPage(page);
            XFont font = new XFont("Arial", 11);
            XFont headerFont = new XFont("Arial", 12);
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
            string orderHistoryText = "My Order History:";
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

            // Space before table (Hövel, 2015)
            gfx.DrawString("", font, XBrushes.Black, new XPoint(40, 120));

            // Define column widths for the table
            float[] columnWidths = { 222, 222, 222, 222, 222 };
            string[] headers = { "Order ID", "Order Date", "Status", "Product Name", "Quantity" };

            // Draw table headers (Hövel, 2015)
            DrawTableRow(gfx, headers, headerFont, (int)yPosition, columnWidths, true);

            // Draw table rows for each order (Hövel, 2015)
            int yPoint = (int)yPosition + 20; // Starting position for table rows (Hövel, 2015)
            double rowHeight = 20; // Row height (Hövel, 2015)
            foreach (var order in orderHistoryViewModels)
            {
                foreach (var item in order.OrderItems)
                {
                    string[] rowData = {
                order.OrderId.ToString(),
order.CreatedAt.HasValue ? order.CreatedAt.Value.ToString("MM/dd/yyyy") : "N/A",

                order.Status,
                item.ProductName,
                item.Quantity.ToString()
            };

                    // Check if there's enough space for the next row (Hövel, 2015)
                    if (yPoint + rowHeight > page.Height - 40)
                    {
                        DrawPageNumber(gfx, pageNumberFont, currentPage, page);
                        currentPage++;
                        page = pdfDocument.AddPage();
                        gfx = XGraphics.FromPdfPage(page);
                        yPoint = 40; // Reset Y position for new page (Hövel, 2015)
                    }

                    // Draw the current row (Hövel, 2015)
                    DrawTableRow(gfx, rowData, font, yPoint, columnWidths, false);
                    yPoint += (int)rowHeight;
                }
            }

            // Draw page number on the last page (Hövel, 2015)
            DrawPageNumber(gfx, pageNumberFont, currentPage, page);

            // Return the PDF as a file
            var stream = new MemoryStream();
            pdfDocument.Save(stream);
            stream.Position = 0;

            return File(stream, "application/pdf", "OrderHistoryReport.pdf");
        }

        // Export Pdf  is to export custom orders for logged in users to a pdf (Troeslen & Japikse, 2021)
        public async Task<IActionResult> ExportPdf4(DateTime? startDate, DateTime? endDate, string status = "Pending")
        {
            // Set default dates if not provided (Troeslen & Japikse, 2021)
            startDate ??= DateTime.Now.AddMonths(-1);
            endDate ??= DateTime.Now;

            // Adjust the end date to include the entire day (Troeslen & Japikse, 2021)
            endDate = endDate.Value.Date.AddDays(1).AddTicks(-1);

            // Get the user's email from session (Troeslen & Japikse, 2021)
            var userEmail = HttpContext.Session.GetString("UserEmail");
            if (userEmail == null)
            {
                return RedirectToAction("Login", "Login");
            }

            // Retrieve the custom orders for the logged-in user (Troeslen & Japikse, 2021)
            var customOrdersQuery = _context.CustomOrders
                                            .Where(order => order.UserEmail == userEmail);

            // Apply additional filters based on status and date range (Troeslen & Japikse, 2021)
            if (!string.IsNullOrEmpty(status))
            {
                customOrdersQuery = customOrdersQuery.Where(order => order.Status == status);
            }

            if (startDate.HasValue)
            {
                customOrdersQuery = customOrdersQuery.Where(order => order.CreatedAt >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                customOrdersQuery = customOrdersQuery.Where(order => order.CreatedAt <= endDate.Value);
            }

            // Execute the query and retrieve the custom orders
            var customOrders = await customOrdersQuery.ToListAsync();

            // Group the orders by CustomOrderId, CreatedAt, and Status, calculating the total amount for each order (Troeslen & Japikse, 2021)
            var customOrderHistoryViewModels = customOrders
                .GroupBy(o => new
                {
                    o.CustomOrderId,
                    o.CreatedAt,
                    o.Status
                })
                .Select(g => new CustomOrderHistoryViewModel
                {
                    CustomOrderId = g.Key.CustomOrderId,
                    CreatedAt = g.Key.CreatedAt,
                    Status = g.Key.Status,
                    TotalAmount = g.Sum(item => item.Price.GetValueOrDefault() * item.Quantity.GetValueOrDefault()),

                    CustomOrderItems = g.Select(item => new CustomOrderItemViewModel
                    {
                        Quantity = item.Quantity.GetValueOrDefault()
                    }).ToList()
                }).ToList();


            // Prepare the PDF document (Hövel, 2015)
            var pdfDocument = new PdfDocument();
            pdfDocument.Info.Title = "My Custom Orders Report";

            int currentPage = 1;
            PdfPage page = pdfDocument.AddPage();
            page.Orientation = PageOrientation.Landscape; // Landscape orientation for more space (Hövel, 2015)
            page.Size = PageSize.A3; // Page size set to A3 (Hövel, 2015)
            XGraphics gfx = XGraphics.FromPdfPage(page);
            XFont font = new XFont("Arial", 11);
            XFont headerFont = new XFont("Arial", 12);
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
            string orderHistoryText = "My Custom Order History:";
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

            // Space before table (Hövel, 2015)
            gfx.DrawString("", font, XBrushes.Black, new XPoint(40, 120));

            // Define column widths for the table (removed the column for Product Name) (Hövel, 2015)
            float[] columnWidths = { 277, 277, 277, 277 };
            string[] headers = { "Order ID", "Order Date", "Status", "Quantity" };

            // Draw table headers (Hövel, 2015)
            DrawTableRow(gfx, headers, headerFont, (int)yPosition, columnWidths, true);

            // Draw table rows for each order (Hövel, 2015)
            int yPoint = (int)yPosition + 20; // Starting position for table rows (Hövel, 2015)
            double rowHeight = 20; // Row height (Hövel, 2015)
            foreach (var customOrder in customOrderHistoryViewModels)
            {
                foreach (var item in customOrder.CustomOrderItems)
                {
                    string[] rowData = {
                customOrder.CustomOrderId.ToString(),
                customOrder.CreatedAt.ToString("MM/dd/yyyy"),
                customOrder.Status,
                item.Quantity.ToString()
            };

                    // Check if there's enough space for the next row (Hövel, 2015)
                    if (yPoint + rowHeight > page.Height - 40)
                    {
                        DrawPageNumber(gfx, pageNumberFont, currentPage, page);
                        currentPage++;
                        page = pdfDocument.AddPage();
                        gfx = XGraphics.FromPdfPage(page);
                        yPoint = 40; // Reset Y position for new page (Hövel, 2015)
                    }

                    // Draw the current row (Hövel, 2015)
                    DrawTableRow(gfx, rowData, font, yPoint, columnWidths, false);
                    yPoint += (int)rowHeight;
                }
            }

            // Draw page number on the last page (Hövel, 2015)
            DrawPageNumber(gfx, pageNumberFont, currentPage, page);

            // Return the PDF as a file (Hövel, 2015)
            var stream = new MemoryStream();
            pdfDocument.Save(stream);
            stream.Position = 0;

            return File(stream, "application/pdf", "CustomOrdersReport.pdf");
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

        // GET: User/EditProfile - Displays the profile edit form (Troeslen & Japikse, 2021)
        public async Task<IActionResult> EditProfile()
        {
            // Prevent the page from being cached(Hewlett, 2015)
            PreventPageCaching();

            var userEmail = HttpContext.Session.GetString("UserEmail");
            // Redirect to login if the user is not logged in (Troeslen & Japikse, 2021)
            if (userEmail == null)
            {
                return RedirectToAction("Login", "Login");
            }

            // Retrieve the user's current profile information (Troeslen & Japikse, 2021)
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);
            return View(user);
        }

        // POST: User/EditProfile - Processes the profile edit form (Troeslen & Japikse, 2021)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile([Bind("Email,FullName,PhoneNumber")] User updatedUser)
        {
            if (ModelState.IsValid)
            {
                // Retrieve the existing user record from the database (Troeslen & Japikse, 2021)
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == updatedUser.Email);
                if (user != null)
                {
                    // Update user's information (Troeslen & Japikse, 2021)
                    user.FullName = updatedUser.FullName;
                    user.PhoneNumber = updatedUser.PhoneNumber;
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Profile updated successfully!";
                    return RedirectToAction("EditProfile");
                }
            }
            // Return the same view if validation fails (Troeslen & Japikse, 2021)
            return View(updatedUser);
        }

        // GET: User/CustomOrders - Displays the custom orders for the logged-in user (Troeslen & Japikse, 2021)
        [HttpGet]
        public async Task<IActionResult> CustomOrders(DateTime? startDate, DateTime? endDate, string status = "Pending")
        {
            // Prevent the page from being cached(Hewlett, 2015)
            PreventPageCaching();

            var userEmail = HttpContext.Session.GetString("UserEmail");

            // Redirect to login if the user is not logged in (Troeslen & Japikse, 2021)
            if (string.IsNullOrEmpty(userEmail))
            {
                return RedirectToAction("Login", "Login");
            }

            // Retrieve the custom orders for the logged-in user (Troeslen & Japikse, 2021)
            var customOrdersQuery = _context.CustomOrders
                                             .Where(order => order.UserEmail == userEmail);

            // Apply the status filter (Troeslen & Japikse, 2021)
            if (!string.IsNullOrEmpty(status))
            {
                customOrdersQuery = customOrdersQuery.Where(order => order.Status == status);
            }

            // Apply date filters if specified (Troeslen & Japikse, 2021)
            if (startDate.HasValue)
            {
                customOrdersQuery = customOrdersQuery.Where(order => order.CreatedAt >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                customOrdersQuery = customOrdersQuery.Where(order => order.CreatedAt <= endDate.Value);
            }

            // Execute the query and return the results (Troeslen & Japikse, 2021)
            var customOrders = await customOrdersQuery.ToListAsync();

            // Pass the filters to the view for persistence (Troeslen & Japikse, 2021)
            ViewBag.Status = status;
            ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd");

            return View(customOrders);
        }

        // Helper method to prevent page caching(Hewlett, 2015)
        private void PreventPageCaching()
        {
            Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
            Response.Headers["Pragma"] = "no-cache";
            Response.Headers["Expires"] = "0";
        }


        public IActionResult Logout()
        {
            // Clear session data for the logged-in user(Shahzad, 2019)
            HttpContext.Session.Remove("UserEmail");

            // Redirect to the home page (or login page)(Shahzad, 2019)
            return RedirectToAction("Index", "Home");
        }
    }
}
