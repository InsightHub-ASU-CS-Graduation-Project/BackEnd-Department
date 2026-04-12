using System.ComponentModel.DataAnnotations;

namespace InsightHub.Models
{
    public class UserJob
    {
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        public int JobId { get; set; }
        public Job Job { get; set; }

        [Required]
        [Range(0, 50)]
        public int YearsExperience { get; set; }
    }
}
