using InsightHub.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace InsightHub.ViewModels
{
    public class SelectedJobInput
    {
        [Range(1, int.MaxValue, ErrorMessage = "JobId must be greater than 0.")]
        public int JobId { get; set; }

        [Required]
        [Range(0, 50)]
        public int YearsExperience { get; set; }
    }

    public class RegisterViewModel
    {
        [Required]
        [StringLength(50)]
        public string FirstName { get; set; }

        [Required]
        [StringLength(50)]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public Gender Gender { get; set; }

        public DateTime? BirthDate { get; set; }

        [StringLength(50)]
        public string? Collage { get; set; }

        public bool IsGraduated { get; set; } = false;

        [Required]
        [DataType(DataType.Password)]
        [MinLength(6)]
        public string Password { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; }

        [Required]
        [MinLength(1, ErrorMessage = "At least one job is required.")]
        public List<SelectedJobInput> SelectedJobs { get; set; } = new List<SelectedJobInput>();

    }
}
