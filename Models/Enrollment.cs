namespace StudyHub.Models
{
    public class Enrollment
    {
        public int Id { get; set; }

        // Foreign Keys
        public int UserId { get; set; }

        public int CourseId { get; set; }

        public DateTime EnrollmentDate { get; set; } = DateTime.Now;

        // Navigation Properties
        public User? User { get; set; }

        public Course? Course { get; set; }
    }
}