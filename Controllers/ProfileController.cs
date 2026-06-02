using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudyHub.Data;
using StudyHub.Models;

namespace StudyHub.Controllers
{
    public class ProfileController : BaseController
    {
        private readonly ApplicationDbContext _context;

        public ProfileController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =========================
        // PROFILE PAGE
        // =========================

        public IActionResult Index()
        {
            if (!TryGetUserId(out int userId))
            {
                return RedirectToLogin();
            }

            // Load the logged-in user with related data
            User? user = _context.Users
                .Include(u => u.Enrollments)
                .Include(u => u.Assignments)
                .FirstOrDefault(u => u.Id == userId);

            if (user == null)
            {
                return NotFound();
            }

            ViewBag.EnrollmentCount = user.Enrollments.Count;
            ViewBag.AssignmentCount = user.Assignments.Count;

            return View(user);
        }
    }
}