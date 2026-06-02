using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StudyHub.Data;
using StudyHub.Models;

namespace StudyHub.Controllers
{
    public class AssignmentsController : BaseController
    {
        private readonly ApplicationDbContext _context;

        public AssignmentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =========================
        // ASSIGNMENTS LIST
        // =========================

        public IActionResult Index(int? courseId)
        {
            if (!TryGetUserId(out int userId))
            {
                return RedirectToLogin();
            }

            // Load assignments for the logged-in user
            var assignments = _context.Assignments
                .Include(a => a.Course)
                .Where(a => a.UserId == userId);

            // Optional filter by course
            if (courseId.HasValue)
            {
                assignments = assignments.Where(a => a.CourseId == courseId.Value);
            }

            LoadCurrentCourses(userId);

            ViewBag.SelectedCourseId = courseId;

            return View(assignments.ToList());
        }

        // =========================
        // ASSIGNMENT DETAILS
        // =========================

        public IActionResult Details(int id)
        {
            if (!TryGetUserId(out int userId))
            {
                return RedirectToLogin();
            }

            Assignment? assignment = _context.Assignments
                .Include(a => a.Course)
                .FirstOrDefault(a => a.Id == id && a.UserId == userId);

            if (assignment == null)
            {
                return NotFound();
            }

            return View(assignment);
        }

        // =========================
        // CREATE ASSIGNMENT PAGE
        // =========================

        [HttpGet]
        public IActionResult Create()
        {
            if (!TryGetUserId(out int userId))
            {
                return RedirectToLogin();
            }

            LoadCurrentCourses(userId);

            return View();
        }

        // =========================
        // CREATE ASSIGNMENT
        // =========================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Assignment assignment)
        {
            if (!TryGetUserId(out int userId))
            {
                return RedirectToLogin();
            }

            if (!ModelState.IsValid)
            {
                LoadCurrentCourses(userId);
                return View(assignment);
            }

            // Assignment must belong to one of the user's current courses
            if (!IsCurrentCourse(userId, assignment.CourseId))
            {
                ModelState.AddModelError("CourseId", "Please select one of your Current Courses.");
                LoadCurrentCourses(userId);
                return View(assignment);
            }

            assignment.UserId = userId;

            _context.Assignments.Add(assignment);
            _context.SaveChanges();

            TempData["Success"] = "Assignment created successfully.";

            return RedirectToAction("Index");
        }

        // =========================
        // EDIT ASSIGNMENT PAGE
        // =========================

        [HttpGet]
        public IActionResult Edit(int id)
        {
            if (!TryGetUserId(out int userId))
            {
                return RedirectToLogin();
            }

            Assignment? assignment = _context.Assignments
                .FirstOrDefault(a => a.Id == id && a.UserId == userId);

            if (assignment == null)
            {
                return NotFound();
            }

            LoadCurrentCourses(userId);

            return View(assignment);
        }

        // =========================
        // EDIT ASSIGNMENT
        // =========================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Assignment assignment)
        {
            if (!TryGetUserId(out int userId))
            {
                return RedirectToLogin();
            }

            Assignment? existingAssignment = _context.Assignments
                .FirstOrDefault(a => a.Id == assignment.Id && a.UserId == userId);

            if (existingAssignment == null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                LoadCurrentCourses(userId);
                return View(assignment);
            }

            // Assignment can only be moved to another current course
            if (!IsCurrentCourse(userId, assignment.CourseId))
            {
                ModelState.AddModelError("CourseId", "Please select one of your Current Courses.");
                LoadCurrentCourses(userId);
                return View(assignment);
            }

            existingAssignment.Title = assignment.Title;
            existingAssignment.Description = assignment.Description;
            existingAssignment.DueDate = assignment.DueDate;
            existingAssignment.CourseId = assignment.CourseId;

            _context.SaveChanges();

            TempData["Success"] = "Assignment updated successfully.";

            return RedirectToAction("Index");
        }

        // =========================
        // DELETE ASSIGNMENT PAGE
        // =========================

        [HttpGet]
        public IActionResult Delete(int id)
        {
            if (!TryGetUserId(out int userId))
            {
                return RedirectToLogin();
            }

            Assignment? assignment = _context.Assignments
                .Include(a => a.Course)
                .FirstOrDefault(a => a.Id == id && a.UserId == userId);

            if (assignment == null)
            {
                return NotFound();
            }

            return View(assignment);
        }

        // =========================
        // DELETE ASSIGNMENT
        // =========================

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            if (!TryGetUserId(out int userId))
            {
                return RedirectToLogin();
            }

            Assignment? assignment = _context.Assignments
                .FirstOrDefault(a => a.Id == id && a.UserId == userId);

            if (assignment == null)
            {
                return NotFound();
            }

            _context.Assignments.Remove(assignment);
            _context.SaveChanges();

            TempData["Success"] = "Assignment deleted successfully.";

            return RedirectToAction("Index");
        }

        // =========================
        // LOAD CURRENT COURSES
        // =========================

        private void LoadCurrentCourses(int userId)
        {
            var currentCourseIds = _context.Enrollments
                .Where(e => e.UserId == userId)
                .Select(e => e.CourseId);

            var currentCourses = _context.Courses
                .Where(c => currentCourseIds.Contains(c.Id))
                .ToList();

            ViewBag.Courses = new SelectList(currentCourses, "Id", "Title");
        }

        // =========================
        // CHECK CURRENT COURSE
        // =========================

        private bool IsCurrentCourse(int userId, int courseId)
        {
            return _context.Enrollments.Any(e =>
                e.UserId == userId && e.CourseId == courseId
            );
        }
    }
}