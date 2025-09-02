using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InsuranceApp.Models
{
    public class Insurance
    {
        [Key]
        public int InsuranceId { get; set; }

        [Display(Name = "Insurance Name")]
        [Required(ErrorMessage = "The name of the insurance is required.")]
        [StringLength(100, ErrorMessage = "The name can have a maximum of 100 characters.")]
        public string InsuranceName { get; set; } = string.Empty;

        [Display(Name = "Insurance Object")]
        [Required(ErrorMessage = "The object of the insurance is required.")]
        [StringLength(100, ErrorMessage = "The object can have a maximum of 100 characters.")]
        public string SubjectName { get; set; } = string.Empty;

        [Display(Name = "Amount (€)")]
        [Range(1, 1000000, ErrorMessage = "The amount must be between 1 and 1,000,000.")]
        public int Amount { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Valid From")]
        [Required(ErrorMessage = "The date from is required.")]
        public DateTime From { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Valid To")]
        [Required(ErrorMessage = "The date to is required.")]
        public DateTime To { get; set; }

       
        [Display(Name = "Insured Person")]
        [Required(ErrorMessage = "The insured person is required.")]
        public int InsuredPersonId { get; set; }

        [ForeignKey("InsuredPersonId")]
        public virtual InsuredPerson? InsuredPerson { get; set; }

        
        public virtual ICollection<InsuranceClaim> Claims { get; set; } = new List<InsuranceClaim>();

     
        public virtual ICollection<InsuranceParticipant> Participants { get; set; } = new List<InsuranceParticipant>();

        [NotMapped]
        public InsuredPerson? PolicyHolder =>
    Participants?.FirstOrDefault(p => p.Role == InsuranceRole.PolicyHolder)?.InsuredPerson;

        [NotMapped]
        public IEnumerable<InsuredPerson> InsuredPersons =>
            Participants?.Where(p => p.Role == InsuranceRole.Insured).Select(p => p.InsuredPerson) ?? Enumerable.Empty<InsuredPerson>();
        public Insurance()
        {
            From = DateTime.Today;
            To = DateTime.Today.AddYears(1);
        }
    }
}