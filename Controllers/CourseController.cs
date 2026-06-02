using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using StudyHub.Data;
using StudyHub.Models;

namespace StudyHub.Controllers
{
    public class CoursesController : BaseController
    {
        private readonly ApplicationDbContext _context;
        private readonly IMemoryCache _memoryCache;

        public CoursesController(ApplicationDbContext context, IMemoryCache memoryCache)
        {
            _context = context;
            _memoryCache = memoryCache;
        }

        // =========================
        // COURSES LIST
        // =========================

        public IActionResult Index(string? searchTerm, string? sortOrder)
        {
            if (!TryGetUserId(out int userId))
            {
                return RedirectToLogin();
            }

            string cacheKey = $"courses_cache_{userId}";

            // Load courses from cache if available
            if (!_memoryCache.TryGetValue(cacheKey, out List<Course>? courses) || courses == null)
            {
                courses = _context.Courses
                    .Where(c => c.UserId == userId)
                    .ToList();

                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(5));

                _memoryCache.Set(cacheKey, courses, cacheOptions);
            }

            // Search courses by title or code
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                courses = courses
                    .Where(c =>
                        c.Title.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)
                        || c.Code.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)
                    )
                    .ToList();
            }

            // Sort courses
            courses = sortOrder switch
            {
                "title_desc" => courses.OrderByDescending(c => c.Title).ToList(),
                "code_asc" => courses.OrderBy(c => c.Code).ToList(),
                "code_desc" => courses.OrderByDescending(c => c.Code).ToList(),
                _ => courses.OrderBy(c => c.Title).ToList()
            };

            ViewBag.SearchTerm = searchTerm;
            ViewBag.SortOrder = sortOrder;

            // Get current semester course IDs
            var currentCourseIds = _context.Enrollments
                .Where(e => e.UserId == userId)
                .Select(e => e.CourseId)
                .ToList();

            ViewBag.CurrentCourseIds = currentCourseIds;
            ViewBag.EnrolledCourseIds = currentCourseIds;

            return View(courses);
        }

        // =========================
        // COURSE DETAILS
        // =========================

        public IActionResult Details(int id, string? returnUrl = null)
        {
            if (!TryGetUserId(out int userId))
            {
                return RedirectToLogin();
            }

            // Load course with its assignments
            Course? course = _context.Courses
                .Include(c => c.Assignments)
                .FirstOrDefault(c => c.Id == id && c.UserId == userId);

            if (course == null)
            {
                return NotFound();
            }

            // Save where the user came from so Back returns correctly
            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                ViewBag.ReturnUrl = returnUrl;
            }
            else
            {
                ViewBag.ReturnUrl = Url.Action("Index", "Courses");
            }

            return View(course);
        }

        // =========================
        // CREATE COURSE PAGE
        // =========================

        [HttpGet]
        public IActionResult Create()
        {
            if (!TryGetUserId(out int userId))
            {
                return RedirectToLogin();
            }

            return View();
        }

        // =========================
        // CREATE COURSE
        // =========================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Course course)
        {
            if (!TryGetUserId(out int userId))
            {
                return RedirectToLogin();
            }

            if (!ModelState.IsValid)
            {
                return View(course);
            }

            // Prevent duplicate course code for the same user
            bool codeExists = _context.Courses.Any(c =>
                c.Code == course.Code && c.UserId == userId
            );

            if (codeExists)
            {
                ModelState.AddModelError("Code", "Course code already exists.");
                return View(course);
            }

            course.UserId = userId;

            _context.Courses.Add(course);
            _context.SaveChanges();

            ClearCourseCache(userId);

            TempData["Success"] = "Course created successfully.";

            return RedirectToAction("Index");
        }

        // =========================
        // EDIT COURSE PAGE
        // =========================

        [HttpGet]
        public IActionResult Edit(int id)
        {
            if (!TryGetUserId(out int userId))
            {
                return RedirectToLogin();
            }

            Course? course = _context.Courses
                .FirstOrDefault(c => c.Id == id && c.UserId == userId);

            if (course == null)
            {
                return NotFound();
            }

            return View(course);
        }

        // =========================
        // EDIT COURSE
        // =========================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Course course)
        {
            if (!TryGetUserId(out int userId))
            {
                return RedirectToLogin();
            }

            Course? existingCourse = _context.Courses
                .FirstOrDefault(c => c.Id == course.Id && c.UserId == userId);

            if (existingCourse == null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(course);
            }

            // Prevent duplicate course code while editing
            bool duplicateCode = _context.Courses.Any(c =>
                c.Code == course.Code
                && c.Id != course.Id
                && c.UserId == userId
            );

            if (duplicateCode)
            {
                ModelState.AddModelError("Code", "Course code already exists.");
                return View(course);
            }

            // Update only allowed fields
            existingCourse.Title = course.Title;
            existingCourse.Code = course.Code;
            existingCourse.Description = course.Description;
            existingCourse.Instructor = course.Instructor;

            _context.SaveChanges();

            ClearCourseCache(userId);

            TempData["Success"] = "Course updated successfully.";

            return RedirectToAction("Index");
        }

        // =========================
        // DELETE COURSE PAGE
        // =========================

        [HttpGet]
        public IActionResult Delete(int id)
        {
            if (!TryGetUserId(out int userId))
            {
                return RedirectToLogin();
            }

            Course? course = _context.Courses
                .FirstOrDefault(c => c.Id == id && c.UserId == userId);

            if (course == null)
            {
                return NotFound();
            }

            return View(course);
        }

        // =========================
        // DELETE COURSE
        // =========================

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            if (!TryGetUserId(out int userId))
            {
                return RedirectToLogin();
            }

            Course? course = _context.Courses
                .FirstOrDefault(c => c.Id == id && c.UserId == userId);

            if (course == null)
            {
                return NotFound();
            }

            _context.Courses.Remove(course);
            _context.SaveChanges();

            ClearCourseCache(userId);

            TempData["Success"] = "Course deleted successfully.";

            return RedirectToAction("Index");
        }

        // =========================
        // ADD TO CURRENT COURSES
        // =========================

        public IActionResult Enroll(int id)
        {
            if (!TryGetUserId(out int userId))
            {
                return RedirectToLogin();
            }

            Course? course = _context.Courses
                .FirstOrDefault(c => c.Id == id && c.UserId == userId);

            if (course == null)
            {
                return NotFound();
            }

            // Check if course is already current
            bool alreadyCurrent = _context.Enrollments.Any(e =>
                e.UserId == userId && e.CourseId == id
            );

            if (alreadyCurrent)
            {
                TempData["Error"] = "This course is already in your Current Courses.";
                return RedirectToAction("Index");
            }

            Enrollment enrollment = new Enrollment
            {
                UserId = userId,
                CourseId = id
            };

            _context.Enrollments.Add(enrollment);
            _context.SaveChanges();

            TempData["Success"] = "Course added to Current Courses.";

            return RedirectToAction("Index");
        }

        // =========================
        // REMOVE FROM CURRENT COURSES
        // =========================

        public IActionResult Unenroll(int id)
        {
            if (!TryGetUserId(out int userId))
            {
                return RedirectToLogin();
            }

            Enrollment? enrollment = _context.Enrollments
                .FirstOrDefault(e => e.UserId == userId && e.CourseId == id);

            if (enrollment == null)
            {
                TempData["Error"] = "Course was not found in your Current Courses.";
                return RedirectToAction("Index");
            }

            // Remove assignments connected to this current course
            var assignmentsToRemove = _context.Assignments
                .Where(a => a.CourseId == id && a.UserId == userId);

            _context.Assignments.RemoveRange(assignmentsToRemove);
            _context.Enrollments.Remove(enrollment);

            _context.SaveChanges();

            TempData["Success"] = "Course removed from Current Courses.";

            return RedirectToAction("Index");
        }

        // =========================
        // CURRENT COURSES PAGE
        // =========================

        public IActionResult MyCourses()
        {
            if (!TryGetUserId(out int userId))
            {
                return RedirectToLogin();
            }

            var currentCourseIds = _context.Enrollments
                .Where(e => e.UserId == userId)
                .Select(e => e.CourseId);

            var courses = _context.Courses
                .Where(c => currentCourseIds.Contains(c.Id))
                .ToList();

            return View(courses);
        }

        // =========================
        // CLEAR COURSE CACHE
        // =========================

        private void ClearCourseCache(int userId)
        {
            string cacheKey = $"courses_cache_{userId}";

            _memoryCache.Remove(cacheKey);
        }
    }
}