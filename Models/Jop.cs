using System.ComponentModel.DataAnnotations;

namespace InsightHub.Models
{
    public class Job
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string JobName { get; set; }

        // علاقة many-to-many مع المستخدمين
        public ICollection<UserJob> UserJobs { get; set; } = new List<UserJob>();
    }
}
