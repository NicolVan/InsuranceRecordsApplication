using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace InsuranceApp.ViewModel
{
    public class InsuranceCreateEditViewModel
    {
        public int InsuranceId { get; set; }

        [Required]
        public string InsuranceName { get; set; } = string.Empty;

        [Required]
        public string SubjectName { get; set; } = string.Empty;

        [Range(1, 1000000)]
        public int Amount { get; set; }

        [DataType(DataType.Date)]
        public DateTime From { get; set; } = DateTime.Today;

        [DataType(DataType.Date)]
        public DateTime To { get; set; } = DateTime.Today.AddYears(1);

        [Required]
        public int PolicyHolderId { get; set; } 

        [Required]
        public List<int> InsuredPersonIds { get; set; } = new();  

       
        public IEnumerable<SelectListItem> PossiblePersons { get; set; } = new List<SelectListItem>();
        public int InsuredPersonId { get; internal set; }
    }

}
