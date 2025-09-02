using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InsuranceApp.Models
{
    public class InsuredPerson
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Name and LastName")]
        public string FullName { get; set; }

        [Display(Name = "Date Of Birth")]
        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }

        [Phone]
        [Display(Name = "PhoneNumber")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Display(Name = "Street")]
        public string Street { get; set; } = string.Empty;

        [Display(Name = "City")]
        public string City { get; set; } = string.Empty;

        [Display(Name = "Country")]
        public string Country { get; set; } = string.Empty;

        [Display(Name = "PostCode")]
        [StringLength(10)]
        public string PostCode { get; set; } = string.Empty;

        public ICollection<Insurance> Insurances { get; set; } = new List<Insurance>();
        public ICollection<InsuranceParticipant> InsuranceParticipants { get; set; } = new List<InsuranceParticipant>();
        public ICollection<InsuranceClaim> InsuranceClaims { get; set; } = new List<InsuranceClaim>();

        public string? ApplicationUserId { get; set; }

        [ForeignKey("ApplicationUserId")]
        public virtual ApplicationUser ApplicationUser { get; set; }
    }
}
