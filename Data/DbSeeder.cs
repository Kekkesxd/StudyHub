using StudyHub.Models;

namespace StudyHub.Data
{
    public static class DbSeeder
    {
        public static void Seed(ApplicationDbContext context)
        {
            // Prevent duplicate seeding
            if (context.Courses.Any())
            {
                return;
            }

            // =========================
            // DEMO USER
            // =========================

            var user = new User
            {
                FullName = "Demo Student",
                Username = "student",
                Email = "student@studyhub.com",
                PasswordHash = Services.PasswordHelper.HashPassword("123456"),
            };

            context.Users.Add(user);

            context.SaveChanges();

            // =========================
            // COURSES
            // =========================

            var courses = new List<Course>
            {
                new Course
                {
                    Title = "Web Programming",
                    Code = "WEB201",
                    Description = "Learn ASP.NET Core MVC and modern web development.",
                    Instructor = "Dr. Sarah Johnson",
                    UserId = user.Id,
                },
                new Course
                {
                    Title = "Database Systems",
                    Code = "DB202",
                    Description = "Introduction to relational databases and SQL.",
                    Instructor = "Prof. Michael Smith",
                    UserId = user.Id,
                },
                new Course
                {
                    Title = "Software Engineering",
                    Code = "SE301",
                    Description = "Software development methodologies and architecture.",
                    Instructor = "Dr. Emily Davis",
                    UserId = user.Id,
                },
            };

            context.Courses.AddRange(courses);

            context.SaveChanges();

            // =========================
            // ENROLLMENTS
            // =========================

            var enrollments = new List<Enrollment>
            {
                new Enrollment { UserId = user.Id, CourseId = courses[0].Id },
                new Enrollment { UserId = user.Id, CourseId = courses[1].Id },
            };

            context.Enrollments.AddRange(enrollments);

            context.SaveChanges();

            // =========================
            // ASSIGNMENTS
            // =========================

            var assignments = new List<Assignment>
            {
                new Assignment
                {
                    Title = "MVC Project",
                    Description = "Build a complete ASP.NET Core MVC application.",
                    DueDate = DateTime.Now.AddDays(7),
                    CourseId = courses[0].Id,
                    UserId = user.Id,
                },
                new Assignment
                {
                    Title = "Database Design",
                    Description = "Design and normalize a relational database schema.",
                    DueDate = DateTime.Now.AddDays(10),
                    CourseId = courses[1].Id,
                    UserId = user.Id,
                },
            };

            context.Assignments.AddRange(assignments);

            context.SaveChanges();
        }
    }
}
