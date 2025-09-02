using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;


namespace InsuranceApp.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required(ErrorMessage = "Full name is required.")]
        [StringLength(100, ErrorMessage = "The maximum name length is 100 characters.")]
        public string FullName { get; set; }

        public virtual InsuredPerson InsuredPerson { get; set; }
    }
}
