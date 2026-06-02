using Microsoft.AspNetCore.Mvc;
using StudyHub.Data;

namespace StudyHub.Controllers
{
    public class HomeController : BaseController
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =========================
        // DASHBOARD
        // =========================

        public IActionResult Dashboard()
        {
            if (!TryGetUserId(out int userId))
            {
                return RedirectToLogin();
            }

            // Count all user-created courses
            int totalCourses = _context.Courses.Count(c => c.UserId == userId);

            // Count current semester courses
            int currentCourses = _context.Enrollments.Count(e => e.UserId == userId);

            // Count all user assignments
            int totalAssignments = _context.Assignments.Count(a => a.UserId == userId);

            // Get next upcoming assignments
            var upcomingAssignments = _context.Assignments
                .Where(a => a.UserId == userId)
                .OrderBy(a => a.DueDate)
                .Take(3)
                .ToList();

            ViewBag.TotalCourses = totalCourses;
            ViewBag.CurrentCourses = currentCourses;
            ViewBag.MyEnrollments = currentCourses;
            ViewBag.TotalAssignments = totalAssignments;
            ViewBag.UpcomingAssignments = upcomingAssignments;

            return View();
        }
    }
}