using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel;

namespace HRMS.Models
{   
    public class Employee : IdentityUser
    {
        [Required]
        public string? Name { get; set; }
        
        [Required]
        public string? Address { get; set; }
        [Required]
        [Display(Name = "NRC or Passport")]
        public string? NRC { get; set; }        
        public string? Photo1Name { get; set; }
        public string? Photo2Name { get; set; }
        public string? Photo3Name { get; set; }
        [NotMapped]
        public IFormFile? Photo1 { get; set; }
        [NotMapped]
        public IFormFile? Photo2 { get; set; }
        [NotMapped]
        public IFormFile? Photo3 { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
