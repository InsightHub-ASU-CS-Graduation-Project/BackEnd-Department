using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace InsightHub.Models
{
    public enum Gender
    {
        Male = 1,
        Female = 2
    }

    public class ApplicationUser : IdentityUser
    {
        [Required]
        [StringLength(50)]
        public string FirstName { get; set; }

        [Required]
        [StringLength(50)]
        public string LastName { get; set; }

        [Required]
        public Gender Gender { get; set; }

        public DateTime? BirthDate { get; set; }

        [StringLength(50)]
        public string? Collage { get; set; }

        public bool IsGraduated { get; set; } = false;

        // علاقة many-to-many مع الوظائف
        public ICollection<UserJob> UserJobs { get; set; } = new List<UserJob>();
    }
}