using System.ComponentModel.DataAnnotations;

namespace StudyHub.Models
{
    public class Course
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string Code { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        public string Instructor { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public int UserId { get; set; }

        // Navigation Properties
        public User? User { get; set; }

        public ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();

        public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    }
}