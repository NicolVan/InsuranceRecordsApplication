using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace InsuranceApp.Models
{
    public class InsuranceClaim
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Description is required.")]
        [StringLength(500, ErrorMessage = "Description can't be longer than 500 characters.")]
        [Display(Name = "Description")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Date of incident is required.")]
        [Display(Name = "Occurred On")]
        [DataType(DataType.Date)]
        [CustomValidation(typeof(InsuranceClaim), nameof(ValidateOccurredOn))]
        public DateTime OccurredOn { get; set; }

        [Required(ErrorMessage = "Estimated damage is required.")]
        [Range(0, double.MaxValue, ErrorMessage = "Estimated damage must be positive.")]
        [Display(Name = "Estimated Damage (€)")]
        public decimal EstimatedDamage { get; set; }

        public IncidentStatus Status { get; set; }

        public int InsuranceId { get; set; }
        [ValidateNever]
        public virtual Insurance Insurance { get; set; } = null!;

        public int InsuredPersonId { get; set; }
        [ValidateNever]
        public InsuredPerson InsuredPerson { get; set; } = null!;

        public InsuranceClaim()
        {
            OccurredOn = DateTime.Today;
        }
        public enum IncidentStatus
        {
            New,
            InProgress,
            Complete
        }
        public static ValidationResult? ValidateOccurredOn(DateTime occurredOn, ValidationContext context)
        {
            if (occurredOn > DateTime.Today)
            {
                return new ValidationResult("Occurred date cannot be in the future.");
            }
            return ValidationResult.Success;
        }
    }
}
