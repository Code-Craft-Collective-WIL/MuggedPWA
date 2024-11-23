using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MuggedShop.Models;
using MuggedShop.ModelViews;

namespace Mugged.Controllers
{
    public class LoginController : Controller
    {
        // Context to interact with the database(Troeslen & Japikse, 2021)
        private readonly MuggedContext _context = new MuggedContext();

        // GET: Login(Troeslen & Japikse, 2021)
        public IActionResult Login()
        {
            return View(new LoginViewModel());
        }

        // POST: Login(Troeslen & Japikse, 2021)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // Check in AdminUsers table(Troeslen & Japikse, 2021)
                var admin = await _context.AdminUsers.FirstOrDefaultAsync(a => a.Email == model.EmailAddress);
                if (admin != null && VerifyPassword(model.Password, admin.PasswordHash))
                {
                    HttpContext.Session.SetString("AdminEmail", admin.Email);
                    return RedirectToAction("AdminHome", "Admin");
                }

                // Check in Users table(Troeslen & Japikse, 2021)
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.EmailAddress);
                if (user != null && VerifyPassword(model.Password, user.PasswordHash))
                {
                    HttpContext.Session.SetString("UserEmail", user.Email);
                    return RedirectToAction("UserHome", "User");
                }

                // If no match found(Troeslen & Japikse, 2021)
                ModelState.AddModelError(string.Empty, "Invalid email or password.");
                return View(model);
            }
            catch
            {
                ModelState.AddModelError(string.Empty, "An error occurred during login. Please try again.");
                return View(model);
            }
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
    }
}
