using Microsoft.AspNetCore.Mvc;
using StudyHub.Data;
using StudyHub.Models;
using StudyHub.Models.ViewModels;
using StudyHub.Services;

namespace StudyHub.Controllers
{
    public class AuthController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AuthController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =========================
        // REGISTER PAGE
        // =========================

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // =========================
        // REGISTER USER
        // =========================

        [HttpPost]
        public IActionResult Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            string fullName = model.FullName.Trim();
            string username = model.Username.Trim();
            string email = model.Email.Trim();

            // Check duplicate username
            bool usernameExists = _context.Users.Any(u => u.Username == username);

            if (usernameExists)
            {
                ModelState.AddModelError("Username", "Username already exists.");
                return View(model);
            }

            // Check duplicate email
            bool emailExists = _context.Users.Any(u => u.Email == email);

            if (emailExists)
            {
                ModelState.AddModelError("Email", "Email already exists.");
                return View(model);
            }

            // Create new user with hashed password
            User user = new User
            {
                FullName = fullName,
                Username = username,
                Email = email,
                PasswordHash = PasswordHelper.HashPassword(model.Password)
            };

            _context.Users.Add(user);
            _context.SaveChanges();

            TempData["Success"] = "Account created successfully!";

            return RedirectToAction("Login");
        }

        // =========================
        // LOGIN PAGE
        // =========================

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // =========================
        // LOGIN USER
        // =========================

        [HttpPost]
        public IActionResult Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            string username = model.Username.Trim();
            string hashedPassword = PasswordHelper.HashPassword(model.Password);

            // Find user by username first
            User? user = _context.Users.FirstOrDefault(u => u.Username == username);

            if (user == null)
            {
                ModelState.AddModelError("", "Invalid username or password.");
                return View(model);
            }

            // Check password hash
            if (user.PasswordHash != hashedPassword)
            {
                ModelState.AddModelError("", "Invalid username or password.");
                return View(model);
            }

            // Create session
            HttpContext.Session.SetInt32("UserId", user.Id);
            HttpContext.Session.SetString("Username", user.Username);
            HttpContext.Session.SetString("FullName", user.FullName);

            return RedirectToAction("Dashboard", "Home");
        }

        // =========================
        // LOGOUT
        // =========================

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();

            return RedirectToAction("Login");
        }
    }
}