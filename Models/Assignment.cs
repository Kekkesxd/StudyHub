using System.ComponentModel.DataAnnotations;

namespace StudyHub.Models
{
    public class Assignment
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        public DateTime DueDate { get; set; }

        // Foreign Keys
        public int CourseId { get; set; }

        public int UserId { get; set; }

        // Navigation Properties
        public Course? Course { get; set; }

        public User? User { get; set; }
    }
}