using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace InsuranceApp.Models
{
    public class InsuranceParticipant
    {
        public int Id { get; set; }

        public InsuranceRole Role { get; set; }

        public int InsuranceId { get; set; }
        [ValidateNever]
        public virtual Insurance Insurance { get; set; }

        public int InsuredPersonId { get; set; }
        [ValidateNever]
        public virtual InsuredPerson InsuredPerson { get; set; }
    }
}
